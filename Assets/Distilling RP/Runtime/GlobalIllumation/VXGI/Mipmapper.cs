using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Distilling;

public class Mipmapper
{
    public enum Mode { Box = 0, Gaussian3x3x3 = 1, Gaussian4x4x4 = 2 }
    public ComputeShader compute
    {
        get
        {
            if (_compute == null) _compute = (ComputeShader)Resources.Load(ShaderIDs.MipmapperCSPath);
            return _compute;
        }
    }
    
    const string _sampleFilter = "Filter.";
    const string _sampleShift = "Shift";

    int _kernelFilter;
    int _kernelShift;
    CommandBuffer _command;
    ComputeShader _compute;
    NumThreads _threadsFilter;
    NumThreads _threadsShift;
    VoxelGI vxgi;
    
}
