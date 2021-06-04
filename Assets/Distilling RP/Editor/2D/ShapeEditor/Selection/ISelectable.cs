namespace UnityEditor.Experimental.Rendering.Distilling.Path2D
{
    internal interface ISelectable<T>
    {
        bool Select(ISelector<T> selector);
    }
}
