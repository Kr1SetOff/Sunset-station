// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Wizard;

[RegisterComponent, NetworkedComponent]
public sealed partial class FartComponent : Component
{
    [DataField]
    public ProtoId<EmotePrototype>? Emote;

    [DataField]
    public bool FartTimeout;

    [DataField]
    public bool FartInhale;

    [DataField]
    public bool SuperFarted;

    [DataField]
    public float MolesAmmoniaPerFart = 5f;

    [DataField]
    public Gas GasToFart = Gas.Ammonia;

    [DataField]
    [Access(Other = AccessPermissions.ReadWriteExecute)]
    public SoundSpecifier BibleSmiteSnd = new SoundPathSpecifier("/Audio/_Goobstation/Effects/thunder_clap.ogg");
}

[Serializable, NetSerializable]
public sealed partial class FartComponentState : ComponentState
{
    public ProtoId<EmotePrototype>? Emote;
    public bool FartTimeout;
    public bool FartInhale;
    public bool SuperFarted;

    public FartComponentState(ProtoId<EmotePrototype>? emote, bool fartTimeout, bool fartInhale, bool superFarted)
    {
        Emote = emote;
        FartTimeout = fartTimeout;
        FartInhale = fartInhale;
        SuperFarted = superFarted;
    }
}

public sealed class PostFartEvent : EntityEventArgs
{
    public readonly EntityUid Uid;
    public readonly bool SuperFart;
    public PostFartEvent(EntityUid uid, bool IsSuperFart = false)
    {
        Uid = uid;
        SuperFart = IsSuperFart;
    }
}

[Serializable, NetSerializable]
public sealed class BibleFartSmiteEvent(NetEntity uid) : EntityEventArgs
{
    public NetEntity Bible = uid;
}
