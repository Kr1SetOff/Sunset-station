### Cult of Nar'Sie antag/gamemode
### (internal identifiers still carry the Ratvar/RatvarCult prefix for historical reasons - see the
### TODO in Content.Server._Sunset.Cult; this is pure tech debt, no gameplay impact)

ratvar-cultist-role-name = Cultist of Nar'Sie
ratvar-cultist-role-objective = Convert crew through the Rite of Offering (needs a second cultist), and help scribe the rune that calls Nar'Sie back into the world.
ratvar-cult-leader-role-name = High Cultist of Nar'Sie
ratvar-cult-leader-role-objective = Lead the cult of Nar'Sie - convert the crew, keep your flock alive, and complete the summoning rite yourself.

role-subtype-ratvar-cult-leader = High Cultist

ratvar-cult-title = Cult of Nar'Sie
ratvar-cult-description = A handful of crew already serve Nar'Sie, the Geometer of Blood, in secret. Convert, coordinate, and bring Her back to this world.

chat-radio-ratvar-cult = Cult

ratvar-cult-scribe-convert-success = The rune of offering settles into the floor, humming faintly.
ratvar-cult-convert-no-victim = There's no incapacitated victim on this rune right now.
ratvar-cult-sacrifice-accepted = "Yes! This is the one I desire. You have done well."
ratvar-cult-convert-success-self = Cold blood fills your veins - you serve Nar'Sie now.
ratvar-cult-convert-success-others = { $target } shudders and their eyes flash red for a moment.
ratvar-cult-commune-message = { $speaker } (Cult): a voice echoes in your mind.
ratvar-cult-scribe-summon-success = The summoning rune settles into the floor, humming faintly.
ratvar-cult-rite-started = The rite has begun - hold this ground with at least { $count } cultists until it completes!
ratvar-cult-rite-interrupted = Not enough of the faithful remain here - the rite falters.

ratvar-cult-scribe-wall-success = The barrier rune settles into the floor, humming faintly.
ratvar-cult-wall-raised = A blood-red wall rumbles up out of the floor!
ratvar-cult-wall-lowered = The wall rumbles back down into the floor.

ratvar-cult-scribe-boil-success = The boiling blood rune settles into the floor, humming faintly.
ratvar-cult-blood-boils = Your blood boils in your veins!

ratvar-cult-scribe-teleport-success = The teleport rune settles into the floor, humming faintly.
ratvar-cult-teleport-no-target = There are no other teleport runes to warp to.
ratvar-cult-teleport-success = There's a sharp crack of inrushing air - something materializes above the rune!

ratvar-cult-scribe-raise-dead-success = The raise dead rune settles into the floor, humming faintly.
ratvar-cult-raise-dead-no-target = There's no dead cultist on this rune.
ratvar-cult-raise-dead-not-enough-souls = Nar'Sie demands more sacrifices - { $required } needed, only { $available } banked.
ratvar-cult-raise-dead-success = You draw in a huge breath, red light shining from your eyes. You're alive!

ratvar-cult-scribe-apocalypse-success = The apocalypse rune settles into the floor, radiating dark heat.

ratvar-cult-blood-magic-already-prepared = You've already awakened blood magic - it can't be done twice.
ratvar-cult-blood-magic-carving = You begin carving unnatural symbols into your flesh!
ratvar-cult-blood-magic-prepared = Your wounds glow with power - you've awakened blood magic!
ratvar-cult-blood-stun-success = Your hand flashes, stunning { $target }.
ratvar-cult-blood-emp-success = Your hand flashes blue, emitting an EMP blast.
ratvar-cult-blood-dagger-success = Your hand glows red for a moment - a ritual dagger appears in it!

admin-verb-text-make-ratvar-cult-leader = Make Nar'Sie Cult Leader
admin-verb-make-ratvar-cult-leader = Turns the player into the head of the Nar'Sie cult.
admin-verb-text-make-ratvar-cultist = Make Nar'Sie Cultist
admin-verb-make-ratvar-cultist = Turns the player into a rank-and-file Nar'Sie cultist.

ratvar-cult-dagger-open-verb = Open ritual wheel

objective-issuer-ratvar-cult = Nar'Sie

objective-condition-ratvar-cult-convert-title = Convert the crew
objective-condition-ratvar-cult-convert-desc = Convert at least { $count } crew members to the cult of Nar'Sie.
objective-condition-ratvar-cult-survive-title = Survive
objective-condition-ratvar-cult-survive-desc = Live to see the round's end.
objective-condition-ratvar-cult-summon-title = Bring back Nar'Sie
objective-condition-ratvar-cult-summon-desc = Scribe the summoning rune and hold it until the rite calling Nar'Sie back is complete.
