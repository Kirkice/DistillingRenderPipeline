using UnityEngine;

namespace UnityEditor.Experimental.Rendering.Distilling.Path2D
{
    internal interface IUndoObject
    {
        void RegisterUndo(string name);
    }
}
