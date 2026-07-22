using Content.Shared._Sunset.Ratvar;
using Robust.Client.GameObjects;

namespace Content.Client._Sunset.Ratvar;

/// <summary>
/// 🌇Sunset🌇 - see RatvarConcealedComponent.
/// </summary>
public sealed class RatvarConcealedVisualsSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RatvarConcealedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<RatvarConcealedComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<RatvarConcealedComponent> ent, ref ComponentStartup args)
    {
        if (TryComp<SpriteComponent>(ent.Owner, out var sprite))
            _sprite.SetVisible((ent.Owner, sprite), false);
    }

    private void OnShutdown(Entity<RatvarConcealedComponent> ent, ref ComponentShutdown args)
    {
        if (TryComp<SpriteComponent>(ent.Owner, out var sprite))
            _sprite.SetVisible((ent.Owner, sprite), true);
    }
}
