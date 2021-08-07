using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Distilling;

public class Mipmapper
{
    public enum Mode { Box = 0, Gaussian3x3x3 = 1, Gaussian4x4x4 = 2 }
    // public ComputeShader compute
    // {
    //     get
    //     {
    //         if (_compute == null) 
    //             _compute = AssetDatabase.LoadAssetAtPath<ComputeShader>(ShaderIDs.MipmapperCSPath);
    //         return _compute;
    //     }
    // }
    
    const string _sampleFilter = "Filter.";
    const string _sampleShift = "Shift";

    int _kernelFilter;
    int _kernelShift;
    CommandBuffer _command;
    ComputeShader _compute;
    NumThreads _threadsFilter;
    NumThreads _threadsShift;
    VoxelGIPass vxgi;
    
    #region Mipmapper
    public Mipmapper(VoxelGIPass gi) 
    {
        vxgi = gi;
        _command = new CommandBuffer { name = "VoxelGI.Mipmapper" };
        InitializeKernel();
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        _command.Dispose();
    }
    #endregion

    private void LoadComputeShader()
    {
        if (_compute == null) 
            _compute = AssetDatabase.LoadAssetAtPath<ComputeShader>(ShaderIDs.MipmapperCSPath);
    }
    /// <summary>
    /// 采样Filter
    /// </summary>
    /// <param name="renderContext"></param>
    #region Filter
    public void Filter(ScriptableRenderContext renderContext)
    {
        UpdateKernel();
        LoadComputeShader();
        
        var radiances = vxgi.passData.radiances;

        for (var i = 1; i < radiances.Length; i++)
        {
            int resolution = radiances[i].volumeDepth;

            _command.BeginSample(_sampleFilter + vxgi.passData.mipmapFilterMode.ToString() + '.' + resolution.ToString("D3"));
            _command.SetComputeIntParam(_compute, ShaderIDs.Resolution, resolution);
            _command.SetComputeTextureParam(_compute, _kernelFilter, ShaderIDs.Source, radiances[i - 1]);
            _command.SetComputeTextureParam(_compute, _kernelFilter, ShaderIDs.Target, radiances[i]);
            _command.DispatchCompute(_compute, _kernelFilter,
               Mathf.CeilToInt((float)resolution / _threadsFilter.x),
               Mathf.CeilToInt((float)resolution / _threadsFilter.y),
               Mathf.CeilToInt((float)resolution / _threadsFilter.z)
            );
            _command.EndSample(_sampleFilter + vxgi.passData.mipmapFilterMode.ToString() + '.' + resolution.ToString("D3"));
        }

        renderContext.ExecuteCommandBuffer(_command);
        _command.Clear();
    }
    #endregion

    /// <summary>
    /// Shift
    /// </summary>
    /// <param name="renderContext"></param>
    /// <param name="displacement"></param>
    #region Shift
    public void Shift(ScriptableRenderContext renderContext, Vector3Int displacement)
    {
        UpdateKernel();
        LoadComputeShader();
        _command.BeginSample(_sampleShift);
        _command.SetComputeIntParam(_compute, ShaderIDs.Resolution, (int)vxgi.passData.resolution);
        _command.SetComputeIntParams(_compute, ShaderIDs.Displacement, new[] { displacement.x, displacement.y, displacement.z });
        _command.SetComputeTextureParam(_compute, _kernelShift, ShaderIDs.Target, vxgi.passData.radiances[0]);
        _command.DispatchCompute(_compute, _kernelShift,
          Mathf.CeilToInt((float)vxgi.passData.resolution / _threadsShift.x),
          Mathf.CeilToInt((float)vxgi.passData.resolution / _threadsShift.y),
          Mathf.CeilToInt((float)vxgi.passData.resolution / _threadsShift.z)
        );
        _command.EndSample(_sampleShift);
        renderContext.ExecuteCommandBuffer(_command);
        _command.Clear();

        Filter(renderContext);
    }
    #endregion

    /// <summary>
    /// 初始化 InitializeKernel
    /// </summary>
    #region InitializeKernel
    void InitializeKernel()
    {
        LoadComputeShader();
        _kernelFilter = 2 * (int)vxgi.passData.mipmapFilterMode;
        _kernelShift = _compute.FindKernel("CSShift");
        _threadsFilter = new NumThreads(_compute, _kernelFilter);
        _threadsShift = new NumThreads(_compute, _kernelShift);
    }
    #endregion

    #region UpdateKernel
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    void UpdateKernel()
    {
        InitializeKernel();
    }
    #endregion
    
}
