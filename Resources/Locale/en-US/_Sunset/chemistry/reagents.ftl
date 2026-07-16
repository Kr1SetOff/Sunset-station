reagent-name-bluespace-liquid = bluespace liquid
reagent-desc-bluespace-liquid = Ground-up bluespace anomaly core. Drinking it flings you to random places on the station. Repeatedly.
reagent-name-bluespace-distorter = prototype bluespace distorter
reagent-desc-bluespace-distorter = An experimental phase-shifter. Hold 15u in your body and matter simply stops arguing with you for fifteen minutes.
reagent-name-pyrosol = pyrosol
reagent-desc-pyrosol = A volatile incendiary binder. Only good for making zapalm - and heartburn.
reagent-name-zapalm = zapalm
reagent-desc-zapalm = Weaponized napalm derivative. At 15u the carrier becomes a walking star of fire, igniting everything nearby - themselves included, slowly.

reagent-name-darwinium = darwinium
reagent-desc-darwinium = Directed-evolution serum. 15u rebuilds any creature into a human. The base of every other race serum.
reagent-name-dvorfinium = dvorfinium
reagent-desc-dvorfinium = Darwinium with a stout heart. 15u makes a dwarf.
reagent-name-aielitinium = aielitinium
reagent-desc-aielitinium = Darwinium refined to elegance. 15u makes an elf.
reagent-name-arachnidium = arachnidium
reagent-desc-arachnidium = Darwinium with too many eyes. 15u makes an arachnid.
reagent-name-avalium = avalium
reagent-desc-avalium = Darwinium chilled to perfection. 15u makes an avali.
reagent-name-cycloritinium = cycloritinium
reagent-desc-cycloritinium = Darwinium set in stone. 15u makes a cyclorite.
reagent-name-drevenium = drevenium
reagent-desc-drevenium = Darwinium with roots. 15u makes a diona.
reagent-name-felinium = felinium
reagent-desc-felinium = Darwinium with whiskers. 15u makes a felenid.
reagent-name-unatium = unatium
reagent-desc-unatium = Darwinium with scales. 15u makes a unathi.
reagent-name-mothium = mothium
reagent-desc-mothium = Darwinium drawn to light. 15u makes a moth person.
reagent-name-voxium = voxium
reagent-desc-voxium = Darwinium that hates oxygen. 15u makes a vox.
reagent-name-vulpanium = vulpanium
reagent-desc-vulpanium = Darwinium with a good nose. 15u makes a vulpkanin.
reagent-name-slimium = slimium
reagent-desc-slimium = Darwinium, but wobbly. 15u makes a slime person.
reagent-name-lagomorphium = lagomorphium
reagent-desc-lagomorphium = Darwinium with long ears. 15u makes a lagomorph.
reagent-name-resomium = resomium
reagent-desc-resomium = Darwinium with feathers. 15u makes a resomi.
reagent-name-rodentium = rodentium
reagent-desc-rodentium = Darwinium that squeaks. 15u makes a rodentia.

reagent-name-mendazin = mendazin
reagent-desc-mendazin = Broad-spectrum trauma paste: heals brute and burns together. Poisons past 30u.
reagent-name-hemosynthin = hemosynthin
reagent-desc-hemosynthin = Synthetic blood substitute: rapidly restores blood volume and slows bleeding. Overdose thins the blood instead.
reagent-name-pulmozin = pulmozin
reagent-desc-pulmozin = Deep-lung restorative: powerful asphyxiation healing. Overdose floods the lungs.
reagent-name-dermalux = dermalux
reagent-desc-dermalux = Silver-infused dermal gel: excellent against every kind of burn. Overdose eats the skin.
reagent-name-osteogen = osteogen
reagent-desc-osteogen = Bone-knitting stimulant: strong blunt-trauma healing. Overdose sets the nerves on edge.
reagent-name-toxinol = toxinol
reagent-desc-toxinol = Aggressive chelation agent: purges poisons and radiation fast. Overdo it and it strips the blood too.

reagent-name-velocitin = velocitin
reagent-desc-velocitin = Combat pick-me-up: keeps oxygen and blood pressure up under fire. The crash past 20u is ugly.
reagent-name-frostin = frostin
reagent-desc-frostin = Rapid coolant: dumps body heat fast - the antidote to being on fire. Overdose means hypothermia.
reagent-name-pyrosin = pyrosin
reagent-desc-pyrosin = Slow-burn heater: warms a frozen body back up. At 10u it warms it up a little too well.
reagent-name-gigglin = gigglin
reagent-desc-gigglin = Laughing tonic: the drinker giggles uncontrollably and gets slightly tipsy. Harmless. Mostly.
reagent-name-vitaflora = vitaflora
reagent-desc-vitaflora = Concentrated ration paste: one sip fills the stomach and quenches thirst. Gorging causes nausea.
reagent-name-necrostat = necrostat
reagent-desc-necrostat = Cryo-stable preservative: dramatically slows body rot, buying time for far-gone patients.

entity-effect-guidebook-sunset-random-teleport = { $chance ->
    [1] Teleports
    *[other] { NATURALFIXED($chance, 2) } probability to teleport
} the metabolizer to a random place on the station
entity-effect-guidebook-sunset-bluespace-phase = { $chance ->
    [1] Lets
    *[other] { NATURALFIXED($chance, 2) } probability to let
} the metabolizer walk through walls for { NATURALFIXED($minutes, 1) } minutes
entity-effect-guidebook-sunset-zapalm-aura = { $chance ->
    [1] Surrounds
    *[other] { NATURALFIXED($chance, 2) } probability to surround
} the metabolizer with a star of fire for { NATURALFIXED($minutes, 1) } minutes

sunset-bluespace-phase-start = The world loses its edges - you can walk through anything!
sunset-bluespace-phase-end = Reality snaps back into focus around you.

guide-entry-sunset-chemicals = Experimental Chemistry
