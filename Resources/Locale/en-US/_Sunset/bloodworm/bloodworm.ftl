# 🌇Sunset🌇 - Blood Worm

guide-entry-blood-worm = Blood Worm

## Role

roles-antag-blood-worm-name = Blood Worm
roles-antag-blood-worm-objective = Kill. Consume. Multiply. Conquer.
role-subtype-blood-worm = Blood Worm
blood-worm-round-end-agent-name = blood worm

ghost-role-information-blood-worm-name = Blood Worm
ghost-role-information-blood-worm-description = A failed Syndicate bioweapon: a space-faring leech that grows on consumed blood. Drink blood, grow, puppet corpses and multiply.
ghost-role-information-blood-worm-rules = You are an antagonist. The crew is food. Follow the general server rules.

## Entities

ent-MobBloodWormHatchling = hatchling blood worm
    .desc = A freshly hatched blood worm. It looks hungry and weak, requiring blood to grow further.
ent-MobBloodWormJuvenile = juvenile blood worm
    .desc = A mid-sized blood worm. It looks bloodthirsty and has numerous long and extremely sharp teeth.
ent-MobBloodWormAdult = adult blood worm
    .desc = A monstrosity of a blood worm. It'd probably be better to put your head in an industrial shredder rather than its maw.
ent-BloodWormCocoonMedium = blood worm cocoon
    .desc = A pulsating cocoon of hardened blood. Something is growing inside.
ent-BloodWormCocoonLarge = blood worm cocoon
    .desc = { ent-BloodWormCocoonMedium.desc }
ent-BloodWormEggCocoon = blood worm egg cocoon
    .desc = A small cocoon of hardened blood. Something wriggles hungrily inside.
ent-ProjectileBloodWormSpit = corrosive blood
    .desc = A glob of corrosive blood.

## Actions

ent-ActionBloodWormLeech = Leech Blood
    .desc = Latch onto a creature with your teeth and drain its blood. Blood fuels your growth and knits your wounds.
ent-ActionBloodWormMatureHatchling = Mature
    .desc = Cocoon up and grow into a juvenile blood worm. Requires 500 units of consumed blood.
ent-ActionBloodWormMatureJuvenile = Mature
    .desc = Cocoon up and grow into an adult blood worm. Requires 1500 units of consumed blood in total.
ent-ActionBloodWormSpit = Spit Blood
    .desc = Spit corrosive blood at your target in exchange for your own health.
ent-ActionBloodWormInvade = Invade Corpse
    .desc = Crawl into a humanoid corpse with blood still in its veins and puppet it as your host.
ent-ActionBloodWormLeaveHost = Leave Host
    .desc = Abandon your host body. It will not survive your departure.
ent-ActionBloodWormInject = Inject Blood
    .desc = Inject your blood into the damaged tissues of your host, healing them in exchange for your own health.
ent-ActionBloodWormReviveHost = Revive Host
    .desc = Restart the blood circulation of your host, bringing them back to life.
ent-ActionBloodWormReproduce = Reproduce
    .desc = Lay an egg cocoon that hatches into a new blood worm. Costs 500 units of consumed blood.

## Blood counter alert (status bar icon)

alerts-blood-worm-blood-name = Consumed Blood
alerts-blood-worm-blood-desc = Shows how much blood you've consumed over your whole life. Determines when you can mature.

## Objectives

objective-issuer-blood-worm = [color=crimson]Hunger[/color]

objective-condition-blood-worm-kill-title = Kill { $targetName }
objective-condition-blood-worm-kill-description = Eliminate this target - however you manage it.
objective-condition-blood-worm-consume-title = Consume {$amount} units of blood
objective-condition-blood-worm-consume-description = Drink at least {$amount} units of blood over your lifetime - blood consumed at any growth stage counts.
objective-condition-blood-worm-survive-title = Survive
objective-condition-blood-worm-survive-description = Live until the end of the round.

objective-condition-blood-worm-rp-silent-title = Silent Horror
objective-condition-blood-worm-rp-silent-description = Never speak for the entire round - communicate only through bites and body language.
objective-condition-blood-worm-rp-nest-title = Nest
objective-condition-blood-worm-rp-nest-description = Build a cozy nest somewhere on the station - out of rags, bodies, anything - and return to it every so often.
objective-condition-blood-worm-rp-taunt-title = Taunt
objective-condition-blood-worm-rp-taunt-description = Leave something in plain sight that reveals your presence to the crew - without revealing who you are.
objective-condition-blood-worm-rp-mimic-title = Mimicry
objective-condition-blood-worm-rp-mimic-description = While puppeting a host, pass yourself off as them at least once to someone who knows them well.
objective-condition-blood-worm-rp-spare-title = Mercy
objective-condition-blood-worm-rp-spare-description = Let at least one helpless victim go free without draining them dry.
objective-condition-blood-worm-rp-bloodline-title = Bloodline
objective-condition-blood-worm-rp-bloodline-description = Give one of your children (from a laid egg) a name.

## Objective entity names/descriptions.
## Robust doesn't resolve name:/description: from objectives.yml as a Loc ID on its own - only via
## ent-<id> (see LocalizationManager.CalcEntityLoc) or an explicit Loc.GetString call from code (as
## for the Kill objective via TargetObjectiveSystem and Consume via BloodWormConsumeConditionSystem).
## Without this, players would see the raw Loc ID instead of the translated text.

ent-BloodWormKillObjective =
    .desc = { objective-condition-blood-worm-kill-description }
ent-BloodWormSurviveObjective = { objective-condition-blood-worm-survive-title }
    .desc = { objective-condition-blood-worm-survive-description }
ent-BloodWormRpObjectiveSilent = { objective-condition-blood-worm-rp-silent-title }
    .desc = { objective-condition-blood-worm-rp-silent-description }
ent-BloodWormRpObjectiveNest = { objective-condition-blood-worm-rp-nest-title }
    .desc = { objective-condition-blood-worm-rp-nest-description }
ent-BloodWormRpObjectiveTaunt = { objective-condition-blood-worm-rp-taunt-title }
    .desc = { objective-condition-blood-worm-rp-taunt-description }
ent-BloodWormRpObjectiveMimic = { objective-condition-blood-worm-rp-mimic-title }
    .desc = { objective-condition-blood-worm-rp-mimic-description }
ent-BloodWormRpObjectiveSpare = { objective-condition-blood-worm-rp-spare-title }
    .desc = { objective-condition-blood-worm-rp-spare-description }
ent-BloodWormRpObjectiveBloodline = { objective-condition-blood-worm-rp-bloodline-title }
    .desc = { objective-condition-blood-worm-rp-bloodline-description }

## Feedback

blood-worm-no-blood = There is no blood in this body.
blood-worm-leech-start = { CAPITALIZE($worm) } latches on and starts draining blood!
blood-worm-leech-finish = { CAPITALIZE($worm) } greedily gulps down blood!
blood-worm-ready-to-mature = Your body strains at the seams - you are ready to mature!
blood-worm-not-enough-blood = Not enough consumed blood: growth is only at {$percent}%.
blood-worm-maturing = { CAPITALIZE($worm) } curls up and vanishes into a cocoon!
blood-worm-cocoon-hatch = The cocoon shudders and bursts - a grown blood worm emerges!
blood-worm-reproduce = { CAPITALIZE($worm) } lays a pulsating cocoon!
blood-worm-invade-not-corpse = You can only invade a humanoid corpse.
blood-worm-invade-occupied = Something already lives in this body.
blood-worm-invade-start = { CAPITALIZE($worm) } crawls into the corpse!
blood-worm-invade-finish = The body shudders and rises to its feet!
blood-worm-leave-host = A bloodied worm bursts out of the body, and it collapses lifelessly!
blood-worm-host-out-of-blood = The body has run out of blood!
blood-worm-host-died = The host body has stopped moving. Revive it or leave before it's too late.
blood-worm-inject-success = You inject blood into the host's damaged tissues, healing them.
blood-worm-inject-not-enough-health = You don't have enough health to inject blood.
blood-worm-revive-not-dead = The host is still alive - there's no one to revive.
blood-worm-revive-not-enough-health = You don't have enough health to restart the host's blood circulation.
blood-worm-revive-success = Blood flows through the host's veins again - they return to life!
