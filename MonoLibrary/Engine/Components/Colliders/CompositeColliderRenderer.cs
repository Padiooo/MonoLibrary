using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoLibrary.Engine.Objects;
using MonoLibrary.Helpers;

namespace MonoLibrary.Engine.Components.Colliders;

public class CompositeColliderRenderer : ColliderRenderer
{
    public CompositeColliderComponent Collider { get; }
    private readonly ColliderRenderer[] _renderers;

    private Color compositeColor = Color.Blue;
    public Color CompositeColor
    {
        get => compositeColor;
        set
        {
            foreach (var renderer in _renderers)
                renderer.Color = value;
            compositeColor = value;
        }
    }

    public CompositeColliderRenderer(GameObject owner, CompositeColliderComponent collider)
        : base(owner)
    {
        Collider = collider;
        _renderers = new ColliderRenderer[collider.Colliders.Count];
        int i = 0;
        foreach (var current in collider.Colliders)
        {
            var renderer = current.GetRenderer();
            renderer.Color = compositeColor;
            _renderers[i++] = renderer;
        }
    }

    public override void Draw(float time, SpriteBatch spriteBatch)
    {
        foreach (var renderer in _renderers)
            renderer.Draw(time, spriteBatch);

        spriteBatch.DrawRectStroke(Collider.Bounds, color: base.Color);
    }
}
