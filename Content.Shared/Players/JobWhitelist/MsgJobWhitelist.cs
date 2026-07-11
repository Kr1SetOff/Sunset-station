using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.Players.JobWhitelist;

public sealed class MsgJobWhitelist : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    public HashSet<string> Whitelist = new();

    // 🌇Sunset🌇 - tier 4+ (SunSetter/Ghost) sponsors bypass every role requirement/whitelist check.
    // Sent alongside the whitelist itself so the client-side requirement UI (job priorities, antag
    // preferences) doesn't lock/reset roles the server would actually allow for these sponsors.
    public bool SponsorRoleBypass;

    // 🌇Sunset🌇 - the actual resolved tier (0-5), so client-side gating (e.g. loadout items requiring
    // a minimum tier) can unlock correctly instead of always seeing tier 0 - see
    // ClientSunsetSponsorTierReader.
    public int SponsorTier;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        var count = buffer.ReadVariableInt32();
        Whitelist.EnsureCapacity(count);

        for (var i = 0; i < count; i++)
        {
            Whitelist.Add(buffer.ReadString());
        }

        SponsorRoleBypass = buffer.ReadBoolean();
        SponsorTier = buffer.ReadVariableInt32();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.WriteVariableInt32(Whitelist.Count);

        foreach (var ban in Whitelist)
        {
            buffer.Write(ban);
        }

        buffer.Write(SponsorRoleBypass);
        buffer.WriteVariableInt32(SponsorTier);
    }
}
