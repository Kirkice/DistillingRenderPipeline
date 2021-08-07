using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Distilling;

public class Parameterizer : System.IDisposable {
    int _kernelParameterize;
    ComputeBuffer _arguments = new ComputeBuffer(3, sizeof(int), ComputeBufferType.IndirectArguments);
    private ComputeShader _compute = AssetDatabase.LoadAssetAtPath<ComputeShader>(ShaderIDs.ParameterizerCSPath);

    public Parameterizer() {
        _kernelParameterize = _compute.FindKernel("CSParameterize");
        _arguments.SetData(new int[] { 1, 1, 1 });
    }

    public void Dispose() {
        _arguments.Dispose();
    }

    public void Parameterize(CommandBuffer command, ComputeBuffer arguments, NumThreads numThreads) {
        command.SetComputeIntParams(_compute, ShaderIDs.NumThreads, numThreads);
        command.SetComputeBufferParam(_compute, _kernelParameterize, ShaderIDs.Arguments, arguments);
        command.DispatchCompute(_compute, _kernelParameterize, _arguments, 0);
    }
}