using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.Distilling
{
    //抗锯齿
    [System.Serializable]
    public enum Antiliasing
    {
        X1 = 1,
        X2 = 2,
        X4 = 4,
        X8 = 8
    };
    
    //大小
    [System.Serializable]
    public enum Resolution
    {
        [InspectorName("Low (32^3)")] Low = 32,
        [InspectorName("Medium (64^3)")] Medium = 64,
        [InspectorName("High (128^3)")] High = 128,
        [InspectorName("VeryHigh (256^3)")] VeryHigh = 256
    };

    public class VoxelGIData : ScriptableObject
    {
        //Make the voxel volume center follow the camera position.
        [SerializeField] [Tooltip("Make the voxel volume center follow the camera position.")] public bool followCamera = false;
        //The center of the voxel volume in World Space
        [SerializeField] [Tooltip("The center of the voxel volume in World Space")] public Vector3 center;
        //The size of the voxel volume in World Space.
        [SerializeField] [Min(0.001f), Tooltip("The size of the voxel volume in World Space.")] public float bound = 10f;
        //The resolution of the voxel volume.
        [SerializeField] [Tooltip("The resolution of the voxel volume.")] public Resolution resolution = Resolution.Medium;
        //The anti-aliasing level of the voxelization process.
        [SerializeField] [Tooltip("The anti-aliasing level of the voxelization process.")] public Antiliasing antiAliasing = Antiliasing.X1;
        [SerializeField] [Tooltip(@"Specify the method to generate the voxel mipmap volume:Box: fast, 2^n voxel resolution.Gaussian 3x3x3: fast, 2^n+1 voxel resolution (recommended).Gaussian 4x4x4: slow, 2^n voxel resolution.")]public Mipmapper.Mode mipmapFilterMode = Mipmapper.Mode.Box;
        [SerializeField] [Tooltip("Limit the voxel volume refresh rate.")]public bool limitRefreshRate = false;
        [SerializeField] [Min(0f), Tooltip("The target refresh rate of the voxel volume.")] public float refreshRate = 30f;
        [SerializeField] [Min(0f), Tooltip("How strong the diffuse cone tracing can affect the scene.")] public float indirectDiffuseModifier = 1f;
        [SerializeField] [Min(0f), Tooltip("How strong the specular cone tracing can affect the scene.")] public float indirectSpecularModifier = 1f;
        [SerializeField] [Range(.1f, 1f), Tooltip("Downscale the diffuse cone tracing pass.")] public float diffuseResolutionScale = 1f;
        
        public bool resolutionPlusOne
        {
            get { return mipmapFilterMode == Mipmapper.Mode.Gaussian3x3x3; }
        }
        public float bufferScale
        {
            get { return 64f / (_resolution - _resolution % 2); }
        }
        public float voxelSize
        {
            get { return bound / (_resolution - _resolution % 2); }
        }
        public int volume
        {
            get { return _resolution * _resolution * _resolution; }
        }
        public ComputeBuffer voxelBuffer
        {
            get { return _voxelBuffer; }
        }
        public List<LightSource> lights
        {
            get { return _lights; }
        }
        public Matrix4x4 voxelToWorld
        {
            get { return Matrix4x4.TRS(origin, Quaternion.identity, Vector3.one * voxelSize); }
        }
        public Matrix4x4 worldToVoxel
        {
            get { return voxelToWorld.inverse; }
        }
        public Parameterizer parameterizer
        {
            get { return _parameterizer; }
        }
        public RenderTexture[] radiances
        {
            get { return _radiances; }
        }
        public Vector3 origin
        {
            get { return voxelSpaceCenter - Vector3.one * .5f * bound; }
        }
        public Vector3 voxelSpaceCenter
        {
            get
            {
                var position = center;

                position /= voxelSize;
                position.x = Mathf.Floor(position.x);
                position.y = Mathf.Floor(position.y);
                position.z = Mathf.Floor(position.z);

                return position * voxelSize;
            }
        }
        public Voxelizer voxelizer
        {
            get { return _voxelizer; }
        }
        
        private int _resolution = 0;
        private float _previousRefresh = 0f;
        private CommandBuffer _command;
        private ComputeBuffer _lightSources;
        private ComputeBuffer _voxelBuffer;
        private List<LightSource> _lights;
        private Mipmapper _mipmapper;
        private Parameterizer _parameterizer;
        private RenderTexture[] _radiances;
        private RenderTextureDescriptor _radianceDescriptor;
        private Vector3 _lastVoxelSpaceCenter;
        private Voxelizer _voxelizer;
        private VoxelShader _voxelShader;
    }
}
