using System;
using UnityEngine;
using UnityEditor.Experimental.Rendering.Distilling.Path2D.GUIFramework;

namespace UnityEditor.Experimental.Rendering.Distilling.Path2D
{
    internal interface IEditablePathView
    {
        IEditablePathController controller { get; set; }
        void Install(GUISystem guiSystem);
        void Uninstall(GUISystem guiSystem);
    }
}
