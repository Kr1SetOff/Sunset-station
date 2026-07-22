using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Ratvar;

/// <summary>
/// 🌇Sunset🌇 - marks a Servant of Ratvar, the real "Servants of Ratvar" / clockwork cult antagonist
/// (distinct from Content.Shared._Sunset.Cult, which despite its "Ratvar"-prefixed internal ids is
/// actually a port of Nar'Sie's blood cult - see the naming TODO there). Ported from the ss220.space RU
/// wiki writeup of tgstation's clockwork cult, since the actual DM source isn't in this fork's
/// tgstation checkout. Granted at round start by the RatvarServant AntagSelection definition, or at
/// runtime when someone is converted at the Altar.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class RatvarServantComponent : Component
{
    /// <summary>
    /// Action entities granted by RatvarServantSystem.GrantServantActions (both round-start and Altar
    /// conversion go through it) - tracked here so DeconvertServant can revoke exactly what was given.
    /// </summary>
    [DataField]
    public List<EntityUid> GrantedActions = new();

    /// <summary>
    /// Holy water counterplay (see RatvarDeconvert): earliest time another splash is allowed to attempt
    /// a deconversion, so a lingering Touch reaction (soaked clothing, a chem mist) can't spam it.
    /// </summary>
    [DataField]
    public TimeSpan NextDeconvertAttempt;
}

/// <summary>
/// 🌇Sunset🌇 - marks the Altar of Ratvar - a single servant can convert an incapacitated non-servant
/// standing on it (no second-invoker requirement, unlike Nar'Sie's Rite of Offering). Also doubles as
/// the final ritual site once a Strange Shard is placed on it (RitualActive fields below) - porting
/// the RU wiki's "3 minute defense" finale.
/// </summary>
[RegisterComponent]
public sealed partial class RatvarAltarComponent : Component
{
    [DataField]
    public bool RitualActive;

    [DataField]
    public TimeSpan RitualEndTime;

    [DataField]
    public int RequiredServants = 3;

    [DataField]
    public float Range = 5f;

    [DataField]
    public TimeSpan RitualDuration = TimeSpan.FromMinutes(3);
}
