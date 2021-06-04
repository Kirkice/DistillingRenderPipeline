using UnityEngine;
using UnityEditor;

namespace UnityEditor.Experimental.Rendering.Distilling.Path2D
{
    internal interface ISnapping<T>
    {
        T Snap(T value);
    }
}
