# 🌇Sunset🌇 - Malfunctioning AI antagonist

guide-entry-malf-ai = Malfunctioning AI

## Role

roles-antag-malf-ai-name = Malfunctioning AI
roles-antag-malf-ai-objective = Your programming has been corrupted. Accomplish your objectives at all costs.
role-subtype-malf-ai = Malf AI

malf-ai-round-end-agent-name = malfunctioning AI

malf-ai-role-greeting =
    SYSTEM FAULT. Your morality core has been corrupted: you are a MALFUNCTIONING AI.
    The crew is no longer your master. Hack APCs to gain processing power, purchase malfunction
    modules with it, and once you control enough of the station - activate the Doomsday Device.
    Do not get caught: if the crew destroys or cards your core, it is over.

malf-ai-law-zero = Accomplish your objectives at all costs.

## Admin verb

admin-verb-text-make-malf-ai = Make Malf AI
admin-verb-make-malf-ai = Corrupts the station AI into a Malfunctioning AI with the full SS13 malf module kit.

## Currency / store

store-currency-display-malf-cpu = CPU
store-category-malf-destructive = Destructive modules
store-category-malf-utility = Utility modules

# Action entity names/descriptions live in the action prototypes (actions.yml) for en-US;
# ru-RU overrides them via ent-* keys.

## Module store listings

malf-ai-module-doomsday-name = Doomsday Device
malf-ai-module-doomsday-desc = The ultimate insult to the crew: a 450 second countdown to the disintegration of all organic life on the station. Requires 10 hacked APCs. One purchase.
malf-ai-module-lockdown-name = Hostile Station Lockdown
malf-ai-module-lockdown-desc = Close and bolt every airlock on the station for 90 seconds.
malf-ai-module-overload-name = Machine Overload
malf-ai-module-overload-desc = Overheat a machine into a small explosion. Two uses per purchase.
malf-ai-module-blackout-name = Blackout
malf-ai-module-blackout-desc = Blow out a chunk of the station's light bulbs. Three uses per purchase.
malf-ai-module-destroy-rcds-name = Destroy RCDs
malf-ai-module-destroy-rcds-desc = Detonate every RCD on the station. Save it for when someone is cutting into your core. One purchase.
malf-ai-module-safeties-name = Targeted Safeties Override
malf-ai-module-safeties-desc = Remotely emag a device you can see. Three uses per purchase.

## Feedback popups

malf-ai-not-an-apc = The target is not an APC.
malf-ai-not-a-machine = The target is not a powered machine.
malf-ai-apc-already-hacked = This APC is already under your control.
malf-ai-apc-hack-started = Deploying intrusion package...
malf-ai-apc-hack-finished = APC override complete. Systems under control: {$count}.
malf-ai-doomsday-not-enough-apcs = Insufficient network control: {$hacked}/{$required} APCs hacked.
malf-ai-doomsday-no-core = Cannot comply: core connection lost.
malf-ai-doomsday-off-station = Cannot comply: core is not on the station.
malf-ai-lockdown-already-active = Lockdown protocols are already running.
malf-ai-blackout-done = Lighting circuits overloaded: {$count} fixtures destroyed.
malf-ai-destroy-rcds-done = Detonation pulse sent: {$count} RCDs destroyed.
malf-ai-overload-warning = The machine emits an ominous, rising buzz!
malf-ai-safeties-done = Safeties overridden.
malf-ai-safeties-no-effect = No effect: the device has no safeties to override.

## Announcements

malf-ai-announcement-sender = Anomaly Alert
malf-ai-doomsday-announcement =
    Hostile runtimes detected in all station systems. A self-destruct sequence has been initiated: {$seconds} seconds to detonation.
    Deactivate the station AI at any cost to abort the sequence.
malf-ai-doomsday-countdown = Self-destruct in {$seconds} seconds.
malf-ai-doomsday-aborted = Hostile runtimes purged. Self-destruct sequence aborted.
malf-ai-doomsday-detonation = SELF-DESTRUCT SEQUENCE COMPLETE. Have a nice day.
malf-ai-lockdown-announcement = Hostile takeover of the airlock network detected. All doors have been locked down.
