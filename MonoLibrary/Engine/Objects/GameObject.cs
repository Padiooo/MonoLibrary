using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonoLibrary.Engine.Objects;

[DebuggerDisplay("Id={Id}")]
public class GameObject : DrawableGameComponent, IEquatable<GameObject>
{
    private readonly SpriteBatch spriteBatch;
    private readonly List<IDisposable> _disposables = [];

    private bool _destroyed = false;

    private static int id = 0;

    public int Id { get; } = ++id;

    public new GameEngine Game { get; }

    public ComponentCollection Components { get; }

    public Vector2 Position { get; set; }

    public GameObject(GameEngine game) : base(game)
    {
        Game = game;
        game.Components.Add(this);
        Components = new(this);
        spriteBatch = game.Services.GetService<SpriteBatch>();
    }

    public override void Update(GameTime gameTime)
    {
        if (_destroyed)
        {
            Game.Components.Remove(this);
            Components.DelayModifications = false;
            return;
        }

        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Components.DelayModifications = true;

        foreach (var comp in Components.updateComponents)
            comp.Update(elapsed);

        Components.DelayModifications = false;
    }

    public override void Draw(GameTime gameTime)
    {
        if (_destroyed)
        {
            Game.Components.Remove(this);
            Components.DelayModifications = false;
            return;
        }

        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Components.DelayModifications = true;

        foreach (var comp in Components.drawComponents)
            comp.Draw(elapsed, spriteBatch);

        Components.DelayModifications = false;
    }

    /// <summary>
    /// Also calls <see cref="GameComponent.Dispose()"/>.
    /// </summary>
    public void Destroy()
    {
        if (_destroyed)
            return;

        _destroyed = true;
        Components.Destroy();
        Dispose();
    }

    public void Add(IDisposable disposable) => _disposables.Add(disposable);
    public void Remove(IDisposable disposable) => _disposables.Remove(disposable);

    protected override void Dispose(bool disposing)
    {
        foreach (var disposable in _disposables)
            disposable.Dispose();

        base.Dispose(disposing);
    }

    #region Overrides

    public bool Equals(GameObject other)
    {
        return other is not null && other.Id == Id;
    }

    public override bool Equals(object obj)
    {
        return obj is GameObject go && go.Id == Id;
    }

    public override int GetHashCode()
    {
        return Id;
    }

    public static bool operator ==(GameObject a, GameObject b)
    {
        if (ReferenceEquals(a, b))
            return true;

        if (a is null && b is not null ||
            a is not null && b is null)
            return false;

        return a.Id == b.Id;
    }

    public static bool operator !=(GameObject a, GameObject b) => !(a == b);

    #endregion
}
