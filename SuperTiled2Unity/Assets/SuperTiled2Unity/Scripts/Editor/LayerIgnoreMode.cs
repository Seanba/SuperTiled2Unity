namespace SuperTiled2Unity.Editor
{
    public enum LayerIgnoreMode
    {
        // Ingores nothing (layer is fully enabled)
        False,

        // Ignore everything on the layer
        True,

        // Ignores colliders on the layer (visuals are imported)
        Collision,

        // Ignores visuals on the layers (colliders still imported)
        Visual,
    }
}
