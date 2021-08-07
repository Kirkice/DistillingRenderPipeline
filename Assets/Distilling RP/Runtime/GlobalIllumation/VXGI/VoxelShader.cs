using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Distilling;

public class VoxelShader : System.IDisposable 
{
    public ComputeShader compute
    {
        get
        {
            if (_compute == null) _compute = AssetDatabase.LoadAssetAtPath<ComputeShader>(ShaderIDs.VoxelShaderCSPath);

            return _compute;
        }
    }
    
    const string sampleCleanup = "Cleanup";
    const string sampleComputeAggregate = "Compute.Aggregate";
    const string sampleComputeClear = "Compute.Clear";
    const string sampleComputeRender = "Compute.Render";
    const string sampleSetup = "Setup";
    
    private int _kernelAggregate;
    private int _kernelClear;
    private int _kernelRender;
    private CommandBuffer _command;
    private ComputeBuffer _arguments;
    private ComputeBuffer _lightSources;
    private ComputeShader _compute;
    private NumThreads _threadsAggregate;
    private NumThreads _threadsClear;
    private NumThreads _threadsTrace;
    private RenderTextureDescriptor _descriptor;
    private VoxelGIPass voxelGI;
    
    public VoxelShader(VoxelGIPass gi) 
    {
        voxelGI = gi;

        _command = new CommandBuffer { name = "VoxelGI.VoxelShader" };

        _arguments = new ComputeBuffer(3, sizeof(int), ComputeBufferType.IndirectArguments);
        _arguments.SetData(new int[] { 1, 1, 1 });
        _lightSources = new ComputeBuffer(64, LightSource.size);

        _kernelAggregate = 0;
        _kernelClear = compute.FindKernel("CSClear");
        _kernelRender = compute.FindKernel("CSRender");

        _threadsAggregate = new NumThreads(compute, _kernelAggregate);
        _threadsClear = new NumThreads(compute, _kernelClear);
        _threadsTrace = new NumThreads(compute, _kernelRender);

        _descriptor = new RenderTextureDescriptor() {
            colorFormat = RenderTextureFormat.RInt,
            dimension = TextureDimension.Tex3D,
            enableRandomWrite = true,
            msaaSamples = 1,
            sRGB = false
        };
    }
    
    public void Dispose()
    {
        _arguments.Dispose();
        _command.Dispose();
        _lightSources.Dispose();
    }
    
    public void Render(ScriptableRenderContext renderContext)
    {
        Setup();
        ComputeClear();
        ComputeRender();
        ComputeAggregate();
        Cleanup();

        renderContext.ExecuteCommandBuffer(_command);
        _command.Clear();
    }
    
    void Cleanup()
    {
        _command.BeginSample(sampleCleanup);

        _command.ReleaseTemporaryRT(ShaderIDs.RadianceBA);
        _command.ReleaseTemporaryRT(ShaderIDs.RadianceRG);
        _command.ReleaseTemporaryRT(ShaderIDs.RadianceCount);

        _command.EndSample(sampleCleanup);
    }
    
        void ComputeAggregate()
    {
        _command.BeginSample(sampleComputeAggregate);

        _command.SetComputeTextureParam(compute, _kernelAggregate, ShaderIDs.RadianceBA, ShaderIDs.RadianceBA);
        _command.SetComputeTextureParam(compute, _kernelAggregate, ShaderIDs.RadianceRG, ShaderIDs.RadianceRG);
        _command.SetComputeTextureParam(compute, _kernelAggregate, ShaderIDs.RadianceCount, ShaderIDs.RadianceCount);
        _command.SetComputeTextureParam(compute, _kernelAggregate, ShaderIDs.Target, voxelGI.passData.radiances[0]);
        _command.DispatchCompute(compute, _kernelAggregate,
          Mathf.CeilToInt((float)voxelGI.passData.resolution / _threadsAggregate.x),
          Mathf.CeilToInt((float)voxelGI.passData.resolution / _threadsAggregate.y),
          Mathf.CeilToInt((float)voxelGI.passData.resolution / _threadsAggregate.z)
        );

        _command.EndSample(sampleComputeAggregate);
    }

    void ComputeClear()
    {
        _command.BeginSample(sampleComputeClear);

        _command.SetComputeTextureParam(compute, _kernelClear, ShaderIDs.RadianceBA, ShaderIDs.RadianceBA);
        _command.SetComputeTextureParam(compute, _kernelClear, ShaderIDs.RadianceRG, ShaderIDs.RadianceRG);
        _command.SetComputeTextureParam(compute, _kernelClear, ShaderIDs.RadianceCount, ShaderIDs.RadianceCount);
        _command.DispatchCompute(compute, _kernelClear,
          Mathf.CeilToInt((float)voxelGI.passData.resolution / _threadsClear.x),
          Mathf.CeilToInt((float)voxelGI.passData.resolution / _threadsClear.y),
          Mathf.CeilToInt((float)voxelGI.passData.resolution / _threadsClear.z)
        );

        _command.EndSample(sampleComputeClear);
    }

    void ComputeRender()
    {
        _command.BeginSample(sampleComputeRender);

        _lightSources.SetData(voxelGI.passData.lights);

        _command.SetComputeIntParam(compute, ShaderIDs.Resolution, (int)voxelGI.passData.resolution);
        _command.SetComputeIntParam(compute, ShaderIDs.LightCount, voxelGI.passData.lights.Count);
        _command.SetComputeBufferParam(compute, _kernelRender, ShaderIDs.LightSources, _lightSources);
        _command.SetComputeBufferParam(compute, _kernelRender, ShaderIDs.VoxelBuffer, voxelGI.passData.voxelBuffer);
        _command.SetComputeMatrixParam(compute, ShaderIDs.VoxelToWorld, voxelGI.passData.voxelToWorld);
        _command.SetComputeMatrixParam(compute, ShaderIDs.WorldToVoxel, voxelGI.passData.worldToVoxel);
        _command.SetComputeTextureParam(compute, _kernelRender, ShaderIDs.RadianceBA, ShaderIDs.RadianceBA);
        _command.SetComputeTextureParam(compute, _kernelRender, ShaderIDs.RadianceRG, ShaderIDs.RadianceRG);
        _command.SetComputeTextureParam(compute, _kernelRender, ShaderIDs.RadianceCount, ShaderIDs.RadianceCount);

        for (var i = 0; i < 9; i++)
        {
            _command.SetComputeTextureParam(compute, _kernelRender, ShaderIDs.Radiance[i], voxelGI.passData.radiances[Mathf.Min(i, voxelGI.passData.radiances.Length - 1)]);
        }

        _command.CopyCounterValue(voxelGI.passData.voxelBuffer, _arguments, 0);
        voxelGI.passData.parameterizer.Parameterize(_command, _arguments, _threadsTrace);
        _command.DispatchCompute(compute, _kernelRender, _arguments, 0);

        _command.EndSample(sampleComputeRender);
    }
    
    /// <summary>
    /// SetUp
    /// </summary>
    #region SetUp
    void Setup()
    {
        _command.BeginSample(sampleSetup);

        UpdateNumThreads();
        _descriptor.height = _descriptor.width = _descriptor.volumeDepth = (int)voxelGI.passData.resolution;
        _command.GetTemporaryRT(ShaderIDs.RadianceCount, _descriptor);
        _command.GetTemporaryRT(ShaderIDs.RadianceBA, _descriptor);
        _command.GetTemporaryRT(ShaderIDs.RadianceRG, _descriptor);

        _command.EndSample(sampleSetup);
    }
    #endregion

    #region UpdateNumThreads
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    void UpdateNumThreads()
    {
        _threadsAggregate = new NumThreads(compute, _kernelAggregate);
        _threadsClear = new NumThreads(compute, _kernelClear);
        _threadsTrace = new NumThreads(compute, _kernelRender);
    }
    #endregion
    
}
