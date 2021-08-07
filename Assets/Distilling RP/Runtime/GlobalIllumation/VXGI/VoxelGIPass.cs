using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Distilling;
using UnityEngine.Experimental.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
public class VoxelGIPass : ScriptableRenderPass
{
    const string m_ProfilerTag = "Voxel GI";
    ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);
    public VoxelGIData passData;
    private Camera MainCamera;

    public VoxelGIPass(VoxelGIData data)
    {
        passData = data;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        Init();
    }

    public void Init()
    {
        passData._resolution = (int)passData.resolution;
        passData._lights = new List<LightSource>(64);
        passData._lightSources = new ComputeBuffer(64, LightSource.size);
        passData._mipmapper = new Mipmapper(this);
        passData._parameterizer = new Parameterizer();
        passData._voxelizer = new Voxelizer(this);
        passData._voxelShader = new VoxelShader(this);
        passData._lastVoxelSpaceCenter = passData.voxelSpaceCenter;

        CreateBuffers();
        CreateTextureDescriptor();
        CreateTextures();
    }
    
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.camera.tag == "MainCamera")
        {
            MainCamera = renderingData.cameraData.camera;
        }
        CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
        using (new ProfilingScope(cmd, m_ProfilingSampler))
        {
            Render(cmd,context,renderingData);
        }
    }

    public void Render(CommandBuffer cmd, ScriptableRenderContext context, RenderingData renderingData)
    {
        UpdateResolution();
        
        var time = Time.realtimeSinceStartup;

        if (!passData.limitRefreshRate || (passData._previousRefresh + 1f / passData.refreshRate < time))
        {
            passData._previousRefresh = time;
            PrePass(context, renderingData);
        }
        
        cmd.ClearRenderTarget(
            (MainCamera.clearFlags & CameraClearFlags.Depth) != 0,
            MainCamera.clearFlags == CameraClearFlags.Color,
            MainCamera.backgroundColor
        );
        
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        
        SetupShader(cmd,context);
        
        cmd.EndSample(cmd.name);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
    
    public void SetupShader(CommandBuffer cmd,ScriptableRenderContext renderContext)
    {
        passData._lightSources.SetData(passData._lights);
        cmd.SetGlobalBuffer(ShaderIDs.LightSources, passData._lightSources);
        cmd.SetGlobalFloat(ShaderIDs.IndirectDiffuseModifier, passData.indirectDiffuseModifier);
        cmd.SetGlobalFloat(ShaderIDs.IndirectSpecularModifier, passData.indirectSpecularModifier);
        cmd.SetGlobalInt(ShaderIDs.LightCount, passData._lights.Count);
        cmd.SetGlobalInt(ShaderIDs.Resolution, passData._resolution);
        cmd.SetGlobalMatrix(ShaderIDs.WorldToVoxel, passData.worldToVoxel);
        renderContext.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
    
    public void PrePass(ScriptableRenderContext renderContext, RenderingData renderingData)
    {
        if (MainCamera != null)
        {
            if (passData.followCamera) 
                passData.center = MainCamera.transform.position;   
        }

        var displacement = (passData.voxelSpaceCenter - passData._lastVoxelSpaceCenter) / passData.voxelSize;

        if (displacement.sqrMagnitude > 0f)
        {
            passData._mipmapper.Shift(renderContext, Vector3Int.RoundToInt(displacement));
        }

        passData._voxelizer.Voxelize(renderContext);
        passData._voxelShader.Render(renderContext);
        passData._mipmapper.Filter(renderContext);

        passData._lastVoxelSpaceCenter = passData.voxelSpaceCenter;
    }
    
    
    #region RenderTextures
    public void CreateTextureDescriptor()
    {
        passData._radianceDescriptor = new RenderTextureDescriptor()
        {
            colorFormat = RenderTextureFormat.ARGBHalf,
            dimension = TextureDimension.Tex3D,
            enableRandomWrite = true,
            msaaSamples = 1,
            sRGB = false
        };
    }

    public void CreateTextures()
    {
        int resolutionModifier = passData._resolution % 2;

        passData._radiances = new RenderTexture[(int)Mathf.Log(passData._resolution, 2f)];

        for (
            int i = 0, currentResolution = passData._resolution;
            i < passData._radiances.Length;
            i++, currentResolution = (currentResolution - resolutionModifier) / 2 + resolutionModifier
        )
        {
            passData._radianceDescriptor.height = passData._radianceDescriptor.width = passData._radianceDescriptor.volumeDepth = currentResolution;
            passData._radiances[i] = new RenderTexture(passData._radianceDescriptor);
            passData._radiances[i].Create();
        }

        for (int i = 0; i < 9; i++)
        {
            Shader.SetGlobalTexture(ShaderIDs.Radiance[i], passData.radiances[Mathf.Min(i, passData._radiances.Length - 1)]);
        }
    }

    public void DisposeTextures()
    {
        foreach (var radiance in passData._radiances)
        {
            radiance.DiscardContents();
            radiance.Release();
        }
    }

    public void ResizeTextures()
    {
        DisposeTextures();
        CreateTextures();
    }
    #endregion
    
    #region Buffers
    public void CreateBuffers()
    {
        passData._voxelBuffer = new ComputeBuffer((int)(passData.bufferScale * passData.volume), VoxelData.size, ComputeBufferType.Append);
    }

    public void DisposeBuffers()
    {
        passData._voxelBuffer.Dispose();
    }

    public void ResizeBuffers()
    {
        DisposeBuffers();
        CreateBuffers();
    }
    #endregion
    
    public void UpdateResolution()
    {
        int newResolution = (int)passData.resolution;

        if (passData.resolutionPlusOne) newResolution++;

        if (passData._resolution != newResolution)
        {
            passData._resolution = newResolution;
            ResizeBuffers();
            ResizeTextures();
        }
    }
    
    public override void FrameCleanup(CommandBuffer cmd)
    {
    }
}
