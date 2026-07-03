// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Shared._Sunset.Genetics;

/// <summary>Telekinesis gene action: hurl the targeted entity away from the user.</summary>
public sealed partial class GeneTelekinesisActionEvent : EntityTargetActionEvent;

/// <summary>Pyrokinesis gene action: set the targeted entity ablaze.</summary>
public sealed partial class GenePyrokinesisActionEvent : EntityTargetActionEvent;
