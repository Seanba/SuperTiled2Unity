namespace SuperTiled2Unity.Ase.Editor
{
    public enum LayerChunkFlags : ushort
    {
        Visible = 1,
        Editable = 2,
        LockMovement = 4,
        Background = 8,
        PreferLinkedCels = 16,
        DisplayCollapsed = 32,
        ReferenceLayer = 64,
    }
}
