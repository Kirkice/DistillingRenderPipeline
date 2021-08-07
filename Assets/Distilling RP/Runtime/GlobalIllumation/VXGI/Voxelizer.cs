using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Distilling;

public class Voxelizer : System.IDisposable 
{
    private int _antiAliasing;
    private int _resolution;
    private Camera _camera;
    private CommandBuffer _command;
    private DrawingSettings _drawingSettings;
    private FilteringSettings _filteringSettings;
    private RenderTextureDescriptor _cameraDescriptor;
    private ScriptableCullingParameters _cullingParameters;
    private VoxelGIPass voxelgi;
    
    public Voxelizer(VoxelGIPass vxgi) 
    {
        voxelgi = vxgi;
        _command = new CommandBuffer { name = "vxgi.Voxelizer" };
        CreateCamera();
        CreateCameraDescriptor();
        CreateCameraSettings();
    }
    
    public void Dispose() {
#if UNITY_EDITOR
        GameObject.DestroyImmediate(_camera.gameObject);
#else
    GameObject.Destroy(_camera.gameObject);
#endif
        _command.Dispose();
    }
    
    void CreateCamera()
    {
        var gameObject = new GameObject("__" + voxelgi.passData.name + "_VOXELIZER__") { hideFlags = HideFlags.HideAndDontSave };
        gameObject.SetActive(false);

        _camera = gameObject.AddComponent<Camera>();
        _camera.allowMSAA = true;
        _camera.aspect = 1f;
        _camera.orthographic = true;
    }
    
    void CreateCameraDescriptor()
    {
        _cameraDescriptor = new RenderTextureDescriptor()
        {
            colorFormat = RenderTextureFormat.R8,
            dimension = TextureDimension.Tex2D,
            memoryless = RenderTextureMemoryless.Color | RenderTextureMemoryless.Depth | RenderTextureMemoryless.MSAA,
            volumeDepth = 1,
            sRGB = false
        };
    }

    public void Voxelize(ScriptableRenderContext renderContext) 
    {
        if (!_camera.TryGetCullingParameters(out _cullingParameters)) return;
        var cullingResults = renderContext.Cull(ref _cullingParameters);

        voxelgi.passData.lights.Clear();

        foreach (var light in cullingResults.visibleLights) {
            if (VoxelGIData.supportedLightTypes.Contains(light.lightType) && light.finalColor.maxColorComponent > 0f) {
                voxelgi.passData.lights.Add(new LightSource(light, voxelgi.passData.worldToVoxel));
            }
        }

        UpdateCamera();

        _command.BeginSample(_command.name);
        _command.GetTemporaryRT(ShaderIDs.Dummy, _cameraDescriptor);
        _command.SetRenderTarget(ShaderIDs.Dummy, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
        _command.SetGlobalInt(ShaderIDs.Resolution, _resolution);
        _command.SetRandomWriteTarget(1, voxelgi.passData.voxelBuffer, false);
        _command.SetViewProjectionMatrices(_camera.worldToCameraMatrix, _camera.projectionMatrix);
        renderContext.ExecuteCommandBuffer(_command);
        renderContext.DrawRenderers(cullingResults, ref _drawingSettings, ref _filteringSettings);
        _command.Clear();
        _command.ClearRandomWriteTargets();
        _command.ReleaseTemporaryRT(ShaderIDs.Dummy);
        _command.EndSample(_command.name);
        renderContext.ExecuteCommandBuffer(_command);
        _command.Clear();
    }
    
    void CreateCameraSettings()
    {
        var sortingSettings = new SortingSettings(_camera) { criteria = SortingCriteria.OptimizeStateChanges };
        _drawingSettings = new DrawingSettings(new ShaderTagId("UniversalForward"), sortingSettings);
        _filteringSettings = new FilteringSettings(RenderQueueRange.all);
    }

    void UpdateCamera()
    {
        if (_antiAliasing != (int)voxelgi.passData.antiAliasing)
        {
            _antiAliasing = (int)voxelgi.passData.antiAliasing;
            _cameraDescriptor.msaaSamples = _antiAliasing;
        }

        if (_resolution != (int)voxelgi.passData.resolution)
        {
            _resolution = (int)voxelgi.passData.resolution;
            _cameraDescriptor.height = _cameraDescriptor.width = _resolution;
        }

        _camera.farClipPlane = .5f * voxelgi.passData.bound;
        _camera.nearClipPlane = -.5f * voxelgi.passData.bound;
        _camera.orthographicSize = .5f * voxelgi.passData.bound;
        _camera.transform.position = voxelgi.passData.voxelSpaceCenter;
    }
    
}
