using UnityEngine;
using UnityEditor;

namespace UnityEditor.Experimental.Rendering.Distilling.Path2D
{
    internal class EditablePathUtility
    {
        public static int Mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }
    }
}
