namespace Deucarian.ViewerShell
{
    /// <summary>
    /// Consumer-neutral lifecycle state rendered by the shared viewer shell.
    /// </summary>
    public enum ViewerShellStatusState
    {
        Uninitialized = 0,
        Loading = 1,
        Ready = 2,
        Error = 3
    }
}
