# 🌇Sunset🌇 - Ratvar cult (clockwork cult)

ratvar-cult-title = Ratvar Cult
ratvar-cult-description = Cultists of Ratvar, the clockwork god, hide among the crew. Brass, cogs and sacrifices in the name of the Engine.

roles-antag-ratvar-cultist-name = Ratvar Cultist
roles-antag-ratvar-cultist-objective = Serve Ratvar: offer a sacrifice to the clockwork god and survive the shift.
role-subtype-ratvar-cultist = Ratvar Cultist

ratvar-cultist-role-greeting =
    You are a cultist of Ratvar, the clockwork god. This station's crew wallows in Nar'Sian heresy and corporate filth.
    Your backpack holds the cult's brass equipment - don it when the time comes to act.
    Offer a sacrifice in the Engine's name and survive the shift. The Forge does not stop. Tick-tock.

ratvar-cult-round-end-agent-name = Ratvar cultist

objective-issuer-ratvar = [color=#BE8700]Ratvar[/color]

objective-condition-ratvar-sacrifice-title = Sacrifice { $targetName }, { $job }
objective-condition-ratvar-sacrifice-description = Ratvar demands a sacrifice. This soul must be fed to the cogs - kill the target and make sure they don't leave the station.
objective-condition-ratvar-survive-title = Survive the shift
objective-condition-ratvar-survive-description = A dead cultist is useless to the Forge. Stay alive at any cost.

ent-RatvarCultSurviveObjective = { objective-condition-ratvar-survive-title }
    .desc = { objective-condition-ratvar-survive-description }
ent-MindRoleRatvarCultist = Ratvar cultist role
