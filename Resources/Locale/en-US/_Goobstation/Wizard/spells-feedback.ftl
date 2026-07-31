# Runtime popup/examine/verb text for the ported Goob-Station wizard spells and items. None of
# these existed anywhere in locale before - every one of the calls below was showing the player
# the raw, untranslated key instead of a message.

# Core spell failures (SharedSpellsSystem/SpellsSystem)
spell-fail-no-targets = There's nothing in range to target!
spell-fail-target-borg = That won't work on a synthetic body.
spell-fail-not-dead = Your target needs to be dead first!
spell-fail-soul-not-bound = Your soul isn't bound to anything.
spell-fail-item-destroyed = Your phylactery has been destroyed!
spell-fail-item-on-another-plane = Your phylactery is somewhere you cannot reach!
spell-fail-no-soul = Your soul isn't yours to give anymore!
spell-fail-bind-soul-silicon = Silicons don't have souls to bind.
spell-fail-no-held-entity = You need to be holding something!
spell-fail-unremoveable = { CAPITALIZE(THE($item)) } can't hold a soul.
spell-fail-soul-item-not-suitable = { CAPITALIZE(THE($item)) } isn't suitable for holding a soul!
spell-fail-mutate-silicon = Silicons can't mutate into a hulking abomination.
spell-fail-lightning-bolt = You don't have a clear shot!
spell-fail-target-cant-wear-mask = Your target can't wear a mask!
spell-fail-target-cursed = Your target's mask resists the curse!
spell-fail-target-silicon = That won't work on a synthetic!
spell-fail-cant-wear-eyepatch = You have nowhere to wear an eyepatch!
spell-fail-already-wear-eyepatch = You're already wearing the eyepatch of power!
spell-fail-sanguine-strike-no-item = You need to be holding a weapon!
spell-fail-sanguine-strike-already-empowered = Your weapon is already thirsting for blood!
spell-fail-sanguine-strike-not-weapon = { CAPITALIZE(THE($item)) } isn't a weapon!
spell-fail-hands-occupied = Your hands are full!
spell-fail-tesla-blast = The lightning dissipates into nothing!
spell-fail-no-spells = You have no spells to recharge!

spell-soul-tap-message = You feel your life force drain away to power your next spell.
spell-soul-tap-almost-dead-message = You feel like you're standing on death's door!
spell-soul-tap-dead-message-user = Everything goes black as the last of your life force is spent.
spell-soul-tap-dead-message-others = { CAPITALIZE(THE($uid)) } collapses, lifeless!

spell-charge-spells-charged-entity = { CAPITALIZE(THE($entity)) }'s spells have been recharged!
spell-charge-spells-charged-pulled = Your spells have been recharged!
spell-charge-no-spells-to-charge-pulled = You have no spells to recharge!

spell-rathen-fart-popup = { CAPITALIZE(THE($target)) }'s appendix bursts out in a spray of gore!
spell-rathen-gut-popup = Your gut wrenches in agony!

spell-summon-simians-maxed-out-message = A wizard's monkey army has reached its peak - a true simian ascension!

instant-summons-item-marked = You mark { THE($item) } for summoning.

lich-greeting = You have bound your soul to your phylactery. You are now a lich - undeath itself will sustain you, as long as your phylactery remains intact.

# Blink item (Content.Shared._Goobstation.Wizard.Blink)
blink-activated-message = Your body begins to flicker unpredictably.
blink-deactivated-message = You feel stable again.

# Chuuni Eyepatch (Content.Shared._Goobstation.Wizard.Chuuni)
chuuni-eyepatch-backstory-1 = This eyepatch was forged in the heart of a dying star, its power sealed by an ancient pact between light and shadow. Or maybe you just really like how it looks.
chuuni-eyepatch-backstory-2 = Legends speak of a warrior who gouged out their own eye to peer beyond the veil of reality. You'd like to think that warrior was you, in a past life.
chuuni-eyepatch-backstory-3 = Sealed within this eyepatch is the fragment of a demon lord who begged for release. You haven't heard from it in a while. That's probably fine.
chuuni-eyepatch-backstory-4 = Some say the eyepatch chooses its wearer. Others say you found it in a cereal box. Both are true, in their own way.

# Lesser Summon Guns (Content.Shared._Goobstation.Wizard.LesserSummonGuns)
enchanted-rifle-guns-left = This rifle has { $guns } enchanted { $guns ->
    [one] shot
   *[other] shots
    } left.

# Bind Soul phylactery (Content.Shared._Goobstation.Wizard.BindSoul)
ensouled-item-desc = A faint, cold presence lingers within, tethered to something that should no longer exist.
ensouled-item-name = Soul-bound { $item }

# Mutate/Hulk (Content.Shared._Goobstation.Wizard.Mutate)
hulk-roar-1 = RAAAGH!
hulk-roar-2 = SMASH!
hulk-roar-3 = I AM UNSTOPPABLE!
hulk-roar-4 = FEEL MY WRATH!
hulk-roar-5 = NOTHING CAN STOP ME NOW!

# Ice Cube trap (Content.Shared._Goobstation.Wizard.Traps)
ice-cube-break-free-start = You start struggling to break free of the ice!

# Exsanguinating Strike (Content.Shared._Goobstation.Wizard.SanguineStrike)
sanguine-strike-examine = This weapon thirsts for blood.

# Scrying Orb (Content.Server._Goobstation.Wizard.Systems.ScryingOrbSystem)
scrying-orb-verb-message = Leave your body behind and scry through the orb.
scrying-orb-verb-text = Scry

# Spellblade (Content.Shared._Goobstation.Wizard.Spellblade)
spellblade-examine-enchantment = This blade is currently enchanted with { $name }.

# Teleport scroll (Content.Server/Content.Shared._Goobstation.Wizard.Teleport)
teleport-scroll-no-charges = This scroll has no charges left!
teleport-scroll-uses-left = This scroll has { $uses } { $uses ->
    [one] teleport
   *[other] teleports
    } left.

# Wizard traps (Content.Shared._Goobstation.Wizard.Traps)
trap-triggered-message = You triggered { THE($trap) }!
trap-revealed-message = You spot { THE($trap) } hidden nearby!
trap-flare-message = { CAPITALIZE(THE($trap)) } flares up, revealing itself!

# Wizard Mirror (Content.Server._Goobstation.Wizard.Systems.WizardMirrorSystem)
wizard-mirror-guardian-change-species-fail = The mirror's magic cannot reach past your guardian's bond!

# Rathen's Curse forced emote and Tile Toggle's speed-buff alert (Resources/Prototypes/_Goobstation/Wizard/misc_prototypes.yml)
chat-emote-name-fart-super = Fart
chat-emote-msg-fart-super = lets out a thunderous fart!
alerts-hierophant-beat-name = Hierophant's Beat
alerts-hierophant-beat-desc = Your steps echo with an ancient rhythm - you move faster.
