using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TempleSetFactory : MonoBehaviour
{
    public Texture2D lut;
    public Cubemap cubemap;
    void Start()
    {
        Shader.SetGlobalTexture("_LUT",lut);
        Shader.SetGlobalTexture("_CubeMap",cubemap);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
