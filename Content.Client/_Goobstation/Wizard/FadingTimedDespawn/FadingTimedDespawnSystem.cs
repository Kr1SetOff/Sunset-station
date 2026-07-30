// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;

namespace Content.Client._Goobstation.Wizard.FadingTimedDespawn;

public sealed class FadingTimedDespawnSystem : SharedFadingTimedDespawnSystem
{
    protected override bool CanDelete(EntityUid uid)
    {
        return IsClientSide(uid);
    }
}
