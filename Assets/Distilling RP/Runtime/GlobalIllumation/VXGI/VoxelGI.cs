using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Distilling;
using UnityEngine.Experimental.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class VoxelGI : ScriptableRendererFeature
{
    public VoxelGIData vxgiData = null;

    VoxelGIPass m_VoxelGIPass;
    public VoxelGI(VoxelGIData m_vxgiData)
    {
        vxgiData = m_vxgiData;
        Create();
    }
    
    public override void Create()
    {
        if (vxgiData != null)
        {
            m_VoxelGIPass = new VoxelGIPass(vxgiData);
            m_VoxelGIPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;   
        }
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var src = renderer.cameraColorTarget;
        renderer.EnqueuePass(m_VoxelGIPass);
    }
    
}
