namespace Deucarian.ViewerShell
{
    /// <summary>Single canonical shell profile shared by every viewer.</summary>
    public static class ViewerShellReferencePreset
    {
        private static readonly ViewerShellReferenceProfile SharedProfile =
            new ViewerShellReferenceProfile();

        public static ViewerShellReferenceProfile Profile => SharedProfile;
    }
}
