// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Shared._Sunset.Genetics.Components;

namespace Content.Server._Sunset.Genetics;

/// <summary>
///     Wires the coughing/sneezing/hiccuping disease genes to the shared <see cref="AutoEmoteSystem"/>. Each
///     gene grants its own marker component rather than a raw AutoEmoteComponent, so several vocal-tic
///     diseases can be active on the same carrier at once without one gene's activation clobbering another's.
/// </summary>
public sealed class GeneVocalTicSystem : EntitySystem
{
    [Dependency] private readonly AutoEmoteSystem _autoEmote = default!;

    private const string CoughEmote = "GeneCoughEmote";
    private const string SneezeEmote = "GeneSneezeEmote";
    private const string HiccupEmote = "GeneHiccupEmote";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GeneCoughingComponent, ComponentStartup>(OnCoughingStartup);
        SubscribeLocalEvent<GeneCoughingComponent, ComponentShutdown>(OnCoughingShutdown);

        SubscribeLocalEvent<GeneSneezingComponent, ComponentStartup>(OnSneezingStartup);
        SubscribeLocalEvent<GeneSneezingComponent, ComponentShutdown>(OnSneezingShutdown);

        SubscribeLocalEvent<GeneHiccupingComponent, ComponentStartup>(OnHiccupingStartup);
        SubscribeLocalEvent<GeneHiccupingComponent, ComponentShutdown>(OnHiccupingShutdown);
    }

    private void OnCoughingStartup(Entity<GeneCoughingComponent> ent, ref ComponentStartup args) => _autoEmote.AddEmote(ent, CoughEmote);
    private void OnCoughingShutdown(Entity<GeneCoughingComponent> ent, ref ComponentShutdown args) => _autoEmote.RemoveEmote(ent, CoughEmote);

    private void OnSneezingStartup(Entity<GeneSneezingComponent> ent, ref ComponentStartup args) => _autoEmote.AddEmote(ent, SneezeEmote);
    private void OnSneezingShutdown(Entity<GeneSneezingComponent> ent, ref ComponentShutdown args) => _autoEmote.RemoveEmote(ent, SneezeEmote);

    private void OnHiccupingStartup(Entity<GeneHiccupingComponent> ent, ref ComponentStartup args) => _autoEmote.AddEmote(ent, HiccupEmote);
    private void OnHiccupingShutdown(Entity<GeneHiccupingComponent> ent, ref ComponentShutdown args) => _autoEmote.RemoveEmote(ent, HiccupEmote);
}
