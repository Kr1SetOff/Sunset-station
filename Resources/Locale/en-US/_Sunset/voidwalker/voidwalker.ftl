guide-entry-voidwalker = Voidwalker

admin-verb-text-make-voidwalker = Make Voidwalker
admin-verb-make-voidwalker = Permanently turns the player into a Voidwalker, replacing their body.
voidwalker-polymorph-popup = The void swallows {$parent} whole, and {$child} steps out of the dark in their place.

ghost-role-information-voidwalker-name = Voidwalker
ghost-role-information-voidwalker-description = A stealthy predator drifting through the void, invisible while in open space. Kidnap the incapacitated and drag them into the void, or just unsettle the crew from the shadows.
ghost-role-information-voidwalker-rules = You are a Voidwalker - an ambush predator native to the void between stars. You're near-invisible while floating in open space (no grid beneath you), and turn fully visible the instant you drift back over a grid. You can dash short distances (Cosmic Dash), stare someone down to stun and reveal yourself to them (Unsettle), send a one-line telepathic message to anyone (Cosmic Transmit), turn every wall, window, and grille in a 3x3 area to passable glass for a short while so you - and anyone you're pulling - can simply walk through the opening (Glassify - won't work on a live/shocked grille or window), and channel a Kidnap on anyone incapacitated while in space. Kidnap victims are automatically sent back to the station with their suit sensors switched to maximum, and a void tumor starts growing inside them that gradually darkens and hurts them until it's surgically removed - left alone, it finishes the job in a few minutes and leaves them permanently void-touched. You have no set objective beyond existing as the void does - show them the void, or don't. Antagonist, but not obligated to kill.

ent-MobVoidwalker = voidwalker
    .desc = A glass-like entity from the void between stars. You probably shouldn't stare.

ent-VoidwalkerCosmicSkull = cosmic skull
    .desc = You can see and feel the surrounding space pulsing through it...

roles-antag-voidwalker-name = Voidwalker
roles-antag-voidwalker-objective = Show the crew the truth of the void.
objective-issuer-voidwalker = [color=#a64dff]The Void[/color]
voidwalker-round-end-agent-name = voidwalker

ent-VoidwalkerObjective = Show them the truth
    .desc = Show them the beauty of the void. Drag them into the cosmic abyss, then impart the truth of the void unto them.

ent-ActionVoidwalkerDash = Cosmic Dash
    .desc = Dash a short distance through the void.
ent-ActionVoidwalkerUnsettle = Unsettle
    .desc = Stare directly at someone until they notice you, stunning and revealing you to them.
ent-ActionVoidwalkerTelepathy = Cosmic Transmit
    .desc = Send an unsettling telepathic message to a target.
ent-ActionVoidwalkerKidnap = Kidnap
    .desc = Channel on an incapacitated target while in space, cursing them with the void.
ent-ActionVoidwalkerGlassify = Glassify
    .desc = Temporarily turn every wall, window, and grille in a 3x3 area to passable glass - you and anyone you're pulling can walk right through the opening.

voidwalker-unsettle-no-los = They can't see you from there!
voidwalker-unsettle-success-self = A cold presence stares into your soul... then it's gone. Something in the dark just revealed itself to you.
voidwalker-unsettle-success-others = {$target} flinches, as if struck by something unseen!
voidwalker-voided-fades = The cosmic chill fades from you.

voidwalker-telepathy-sent = You cast your thoughts outward...
voidwalker-telepathy-received = A cold, alien voice echoes in your mind: "{$phrase}"
voidwalker-telepathy-phrase-watching = We are watching.
voidwalker-telepathy-phrase-cold = It's so cold out here. Come see.
voidwalker-telepathy-phrase-glass = Do you like glass? We do.
voidwalker-telepathy-phrase-come = Come to the dark. It's warmer than you think.
voidwalker-telepathy-phrase-truth = You are not ready for the truth. Yet.

voidwalker-kidnap-dead = They're already dead!
voidwalker-kidnap-conscious = They're still conscious!
voidwalker-kidnap-already-voided = They've already seen the void!
voidwalker-kidnap-not-in-space = They're not in space!
voidwalker-kidnap-too-far = Get closer to them first!
voidwalker-kidnap-success-self = The void swallows you whole, then spits you back out... changed.
voidwalker-kidnap-success-voidwalker = They have seen the truth.

voidwalker-glassify-invalid-target = That's not something you can turn to glass!
voidwalker-glassify-electrified = It's live - you'd get shocked!
voidwalker-glassify-already-glass = It's already glass!
voidwalker-glassify-no-los = You need a clear line of sight to it!
voidwalker-glassify-success = The wall shudders and turns to glass...

voidwalker-tumor-removed = The cold knot in your chest is gone. Whatever was growing in you has stopped.
voidwalker-tumor-consumed = Something in you finishes uncoiling. Your skin feels wrong now, permanently.

voidwalker-spawn-direction = You're adrift in open space. The station is somewhere to the { $direction }.
voidwalker-spawn-direction-north = north
voidwalker-spawn-direction-south = south
voidwalker-spawn-direction-east = east
voidwalker-spawn-direction-west = west
