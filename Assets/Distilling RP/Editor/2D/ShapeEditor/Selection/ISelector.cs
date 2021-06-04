namespace UnityEditor.Experimental.Rendering.Distilling.Path2D
{
    internal interface ISelector<T>
    {
        bool Select(T element);
    }
}
