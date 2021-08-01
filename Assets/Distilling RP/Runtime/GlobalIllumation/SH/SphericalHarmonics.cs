using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering.Distilling.Internal
{
    public enum SphericalHarmonicsDegree
    {
        Zero = 0,
        One = 1,
        Two = 2,
        Three = 3,
    };
    
    public class SphericalHarmonics : ScriptableRenderPass
    {
        const string m_ProfilerTag = "Spherical Harmonics";
        private int degree = 0;
        private Cubemap cubemap;
        private Dictionary<int, int> faceCalculate = new Dictionary<int, int>();
    
        public SphericalHarmonics(RenderPassEvent evt, int degree)
        {
            this.degree = degree;
            cubemap = RenderSettings.customReflection;
            SetSH();
            renderPassEvent = evt;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
        }

        /// <summary>
        /// 获取SHPos
        /// </summary>
        /// <returns></returns>
        Vector3 RandomSHPos() 
        {
            Vector3 pos = Random.onUnitSphere;
            return pos;
        }
        
        /// <summary>
        /// GetSHColor
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        Vector4 GetSHColor(Vector3 pos) 
        {
            Color col = new Color();

            float xabs = pos.x;
            float yabs = pos.y;
            float zabs = pos.z;
            int faceIndex = -1;
            Vector2 uv = new Vector2();
            if (xabs >= yabs && xabs >= zabs) {
                //x
                faceIndex = pos.x > 0 ? 0 : 1;
                uv.x = pos.y / xabs;
                uv.y = pos.z / xabs;
            } else if (yabs >= xabs && yabs >= zabs) {
                //y 
                faceIndex = pos.y > 0 ? 2 : 3;
                uv.x = pos.x / yabs;
                uv.y = pos.z / yabs;
            } else {
                //z
                faceIndex = pos.z > 0 ? 4 : 5;
                uv.x = pos.x / zabs;
                uv.y = pos.y / zabs;
            }
            //[0,1.0]
            uv.x = (uv.x + 1.0f) / 2.0f;
            uv.y = (uv.y + 1.0f) / 2.0f;
            int w = cubemap.width - 1;
            int x = (int)(w * uv.x);
            int y = (int)(w * uv.y);
            //Debug.Log("random face:" + faceIndex.ToString());
            if (faceCalculate.ContainsKey(faceIndex)) {
                faceCalculate[faceIndex]++;
            }
            col = cubemap.GetPixel((CubemapFace)faceIndex, x, y);
            Vector4 colVec4 = new Vector4(col.r, col.g, col.b, col.a);
            return colVec4;
        }
        
        /// <summary>
        /// SH
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        List<float> HarmonicsBasis(Vector3 pos) 
        {
            int basisCount = (degree + 1) * (degree + 1);
            float[] sh = new float[basisCount];
            for (int i = 0; i < basisCount; i++) {

            }
            Vector3 normal = pos.normalized;
            float x = normal.x;
            float y = normal.y;
            float z = normal.z;

            if (degree >= 0) {
                sh[0] = 1.0f / 2.0f * Mathf.Sqrt(1.0f / Mathf.PI);
            }
            if (degree >= 1) {
                sh[1] = Mathf.Sqrt(3.0f / (4.0f * Mathf.PI)) * z;
                sh[2] = Mathf.Sqrt(3.0f / (4.0f * Mathf.PI)) * y;
                sh[3] = Mathf.Sqrt(3.0f / (4.0f * Mathf.PI)) * x;
            }
            if (degree >= 2) {
                sh[4] = 1.0f / 2.0f * Mathf.Sqrt(15.0f / Mathf.PI) * x * z;
                sh[5] = 1.0f / 2.0f * Mathf.Sqrt(15.0f / Mathf.PI) * z * y;
                sh[6] = 1.0f / 4.0f * Mathf.Sqrt(5.0f / Mathf.PI) * (-x * x - z * z + 2 * y * y);
                sh[7] = 1.0f / 2.0f * Mathf.Sqrt(15.0f / Mathf.PI) * y * x;
                sh[8] = 1.0f / 4.0f * Mathf.Sqrt(15.0f / Mathf.PI) * (x * x - z * z);
            }
            if (degree >= 3) {
                sh[9] = 1.0f / 4.0f * Mathf.Sqrt(35.0f / (2.0f * Mathf.PI)) * (3 * x * x - z * z) * z;
                sh[10] = 1.0f / 2.0f * Mathf.Sqrt(105.0f / Mathf.PI) * x * z * y;
                sh[11] = 1.0f / 4.0f * Mathf.Sqrt(21.0f / (2.0f * Mathf.PI)) * z * (4 * y * y - x * x - z * z);
                sh[12] = 1.0f / 4.0f * Mathf.Sqrt(7.0f / Mathf.PI) * y * (2 * y * y - 3 * x * x - 3 * z * z);
                sh[13] = 1.0f / 4.0f * Mathf.Sqrt(21.0f / (2.0f * Mathf.PI)) * x * (4 * y * y - x * x - z * z);
                sh[14] = 1.0f / 4.0f * Mathf.Sqrt(105.0f / Mathf.PI) * (x * x - z * z) * y;
                sh[15] = 1.0f / 4.0f * Mathf.Sqrt(35.0f / (2 * Mathf.PI)) * (x * x - 3 * z * z) * x;
            }
            List<float> shList = new List<float>(sh);
            return shList;

        }
        
        private void SetSH()
        {
            if(cubemap==null)
                return;
            
            int n = (degree + 1) * (degree + 1);
            Vector4[] coefs = new Vector4[n];
            
            int sampleNum = 10000;
            faceCalculate.Add(0, 0);
            faceCalculate.Add(1, 0);
            faceCalculate.Add(2, 0);
            faceCalculate.Add(3, 0);
            faceCalculate.Add(4, 0);
            faceCalculate.Add(5, 0);
            for (int i = 0; i < sampleNum; i++) 
            {
                var p = RandomSHPos();
                var h = HarmonicsBasis(p);
                var c = GetSHColor(p);
                for (int t = 0; t < n; t++) 
                {
                    coefs[t] = coefs[t] + h[t] * c;
                }
            }
            for (int t = 0; t < n; t++) 
            {
                coefs[t] = 4.0f * Mathf.PI * coefs[t] / (sampleNum * 1.0f);
            }
            
            for (int i = 0; i < n; ++i) {
                Shader.SetGlobalVector("c" + i.ToString(), coefs[i]);
            }
        }
        public override void FrameCleanup(CommandBuffer cmd)
        {
        }
    }
}
