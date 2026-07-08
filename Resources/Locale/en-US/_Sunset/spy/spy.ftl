# Spy antagonist (_Sunset) - a covert-ops antagonist who accepts contracts live from a rotating
# board in their uplink (Bounty System, ported from frostek3122-byte/sunset-station) instead of
# static round-start objectives.

# Role / antag
roles-antag-spy-name = Spy
roles-antag-spy-objective = Accept contracts from your handler, complete them, and survive.
role-subtype-spy = Spy
spy-round-end-agent-name = spy
ent-MindRoleSpy = Spy Role

spy-role-greeting = You are a Spy. A rival corporation has bankrolled your infiltration of this station. Accept contracts from your handler through the Spy Uplink's contract board, complete them for SpyCredit and reputation, and spend your earnings on better gear.
spy-role-greeting-equipment = Your loadout: a Spy Uplink (contract board and shop), a Spy Pinpointer (points at your current contract's target), and a Spy Tracker (plant it on a target to work a contract).
spy-role-greeting-reputation = Reputation with your handler rises when you complete contracts and falls when you abandon one you've already accepted. Higher reputation pays better.

# Currency
store-currency-display-spy-credit = SpyCredit

# Store categories
store-category-spy-contracts = Contracts
store-category-spy-gear = Gear
store-category-spy-weapons = Weapons
store-category-spy-espionage = Espionage
store-category-spy-medical = Chemistry & Medical

# Uplink UI
spy-uplink-ui-title = Espionage Network
spy-uplink-ui-market = Market
spy-uplink-ui-contracts = Contracts
spy-uplink-ui-contract-accept = Accept
spy-uplink-ui-contract-abandon = Abandon
spy-uplink-ui-contract-reward = +{$amount}¢
spy-uplink-ui-reputation = Reputation: {$value}
spy-uplink-ui-rotation = Board refreshes in {$time}
spy-uplink-ui-active-contract = Active: {$name} (+{$reward}¢)

# Contract system - popups
spy-contract-already-active = You already have an active contract. Abandon it first.
spy-contract-nothing-to-abandon = You don't have an active contract.
spy-contract-no-target = No valid target for that contract is on the station right now.
spy-contract-accepted = Contract accepted: {$name}. Reputation: {$reputation}.
spy-contract-abandoned = Contract abandoned. Reputation: {$reputation}.
spy-contract-completed = Contract complete! +{$reward}¢. Reputation: {$reputation}.
spy-contract-target-lost = Your contract's target is gone. The contract has been cancelled.
spy-contract-bug-found = Your tracker was found! The contract has been blown.
spy-tracker-no-contract = You don't have an active contract to work on.
spy-tracker-wrong-target = That's not your contract's target.
spy-tracker-assassinate-hint = This contract can only be completed by killing the target yourself - the tracker won't help.
spy-tracker-planted = Tracker planted. It'll do its work on its own now - just don't let anyone find it.
spy-tracker-found = You find a hidden tracking device.

# Contract pool
spy-contract-watch-crew-name = Surveillance: Crew Member
spy-contract-watch-crew-desc = Plant a tracker on any crew member and stay close enough to monitor them.
spy-contract-watch-ce-name = Surveillance: Chief Engineer
spy-contract-watch-ce-desc = Track the Chief Engineer's movements.
spy-contract-watch-cmo-name = Surveillance: Chief Medical Officer
spy-contract-watch-cmo-desc = Track the Chief Medical Officer's movements.
spy-contract-watch-hos-name = Surveillance: Head of Security
spy-contract-watch-hos-desc = Track the Head of Security's movements.
spy-contract-watch-captain-name = Surveillance: Captain
spy-contract-watch-captain-desc = Track the Captain's movements.
spy-contract-surveillance-telecoms-name = Surveillance: Telecomms Server
spy-contract-surveillance-telecoms-desc = Plant a tracker on the telecommunications server and hold position nearby.
spy-contract-watch-comms-name = Surveillance: Communications Console
spy-contract-watch-comms-desc = Monitor a communications console.
spy-contract-watch-apc-name = Surveillance: APC
spy-contract-watch-apc-desc = Monitor an area power controller.
spy-contract-watch-research-name = Surveillance: Research Server
spy-contract-watch-research-desc = Monitor the research server.
spy-contract-watch-gravity-name = Surveillance: Gravity Generator
spy-contract-watch-gravity-desc = Monitor the gravity generator.
spy-contract-watch-valuable-name = Surveillance: Valuable Item
spy-contract-watch-valuable-desc = Track a valuable item of significant interest.
spy-contract-sabotage-smes-name = Sabotage: SMES Unit
spy-contract-sabotage-smes-desc = Drain an SMES unit's charge to zero.
spy-contract-sabotage-apc-name = Sabotage: APC
spy-contract-sabotage-apc-desc = Drain an area power controller's charge to zero.
spy-contract-sabotage-door-name = Sabotage: Door
spy-contract-sabotage-door-desc = Bolt a door shut.
spy-contract-sabotage-comms-name = Sabotage: Communications Console
spy-contract-sabotage-comms-desc = EMP a communications console.
spy-contract-sabotage-gravity-name = Sabotage: Gravity Generator
spy-contract-sabotage-gravity-desc = EMP the gravity generator.
spy-contract-collect-id-name = Data Pull: ID Card
spy-contract-collect-id-desc = Copy data from an ID card.
spy-contract-collect-comms-name = Data Pull: Communications Console
spy-contract-collect-comms-desc = Pull data from a communications console.
spy-contract-collect-research-name = Data Pull: Research Server
spy-contract-collect-research-desc = Steal research data from the research server.
spy-contract-collect-telecoms-name = Data Pull: Telecomms Server
spy-contract-collect-telecoms-desc = Intercept telecommunications server traffic.
spy-contract-collect-valuable-name = Data Pull: Valuable Item
spy-contract-collect-valuable-desc = Copy data from a valuable item of significant interest.
spy-contract-kill-crew-name = Assassination: Crew Member
spy-contract-kill-crew-desc = Kill any crew member with your own hands.
spy-contract-kill-ce-name = Assassination: Chief Engineer
spy-contract-kill-ce-desc = Kill the Chief Engineer with your own hands.
spy-contract-kill-cmo-name = Assassination: Chief Medical Officer
spy-contract-kill-cmo-desc = Kill the Chief Medical Officer with your own hands.
spy-contract-kill-hop-name = Assassination: Head of Personnel
spy-contract-kill-hop-desc = Kill the Head of Personnel with your own hands.
spy-contract-kill-hos-name = Assassination: Head of Security
spy-contract-kill-hos-desc = Kill the Head of Security with your own hands.
spy-contract-kill-captain-name = Assassination: Captain
spy-contract-kill-captain-desc = Kill the Captain with your own hands.

# Uplink listings
spy-listing-chameleon-backpack-name = Chameleon Kit
spy-listing-chameleon-backpack-desc = A backpack with a full set of chameleon clothing: disguise as anyone.
spy-listing-chameleon-shoes-name = No-Slip Chameleon Shoes
spy-listing-chameleon-shoes-desc = Syndicate agent footwear: disguises as any other shoes and never slips.
spy-listing-chameleon-gloves-name = Chameleon Thieving Gloves
spy-listing-chameleon-gloves-desc = Syndicate agent gloves: disguise and a careful touch for faster searches.
spy-listing-chameleon-mask-name = Chameleon Mask
spy-listing-chameleon-mask-desc = A chameleon gas mask: disguise as any mask.
spy-listing-chameleon-projector-name = Chameleon Projector
spy-listing-chameleon-projector-desc = Project the appearance of any nearby object onto yourself.
spy-listing-agentid-name = Agent ID Card
spy-listing-agentid-desc = A forged ID with changeable name, job title and access.
spy-listing-thermals-name = Thermal Vision Goggles
spy-listing-thermals-desc = Wear them and toggle thermal vision to see the body heat of living creatures through walls.
spy-listing-storage-implant-name = Storage Implant
spy-listing-storage-implant-desc = A hidden subdermal pocket to smuggle items past a search.
spy-listing-dna-scrambler-name = DNA Scrambler Implant
spy-listing-dna-scrambler-desc = Scrambles your DNA and fingerprints on activation.
spy-listing-freedom-implant-name = Freedom Implant
spy-listing-freedom-implant-desc = Frees you from cuffs and restraints a few times.
spy-listing-escape-implant-name = Escape Implant
spy-listing-escape-implant-desc = Teleports you a short distance, breaking grabs and restraints.
spy-listing-invisible-armour-name = Covert Armor
spy-listing-invisible-armour-desc = A lightweight reinforced vest built for stealth work. Disguises itself as other outerwear, invisible in your inventory to anyone else, and nobody but you can strip it off.
spy-listing-cobra-name = Cobra Pistol
spy-listing-cobra-desc = A compact, quiet pistol chambered in .25 caseless.
spy-listing-cobra-mag-name = Cobra Magazine
spy-listing-cobra-mag-desc = A standard .25 caseless magazine for the Cobra.
spy-listing-cobra-mag-ap-name = Cobra Magazine (AP)
spy-listing-cobra-mag-ap-desc = An armor-piercing .25 caseless magazine for the Cobra.
spy-listing-energydagger-name = Energy Dagger
spy-listing-energydagger-desc = A concealable energy blade.
spy-listing-throwing-knives-name = Throwing Knives
spy-listing-throwing-knives-desc = A set of silent throwing knives.
spy-listing-hypopen-name = Hypopen
spy-listing-hypopen-desc = A disguised chemical injector pen.
spy-listing-smoke-name = Smoke Grenade
spy-listing-smoke-desc = Cover your escape with a cloud of smoke.
spy-listing-emp-name = EMP Grenade
spy-listing-emp-desc = Disable electronics in a radius.
spy-listing-signaller-name = Remote Signaller
spy-listing-signaller-desc = Remotely triggers connected devices.
spy-listing-radio-jammer-name = Radio Jammer
spy-listing-radio-jammer-desc = Jams nearby radios.
spy-listing-access-breaker-name = Access Breaker
spy-listing-access-breaker-desc = Rewrites an ID card's access permissions.
spy-listing-jaws-name = Jaws of Life
spy-listing-jaws-desc = Force open airlocks and firelocks.
spy-listing-powersink-name = Power Sink
spy-listing-powersink-desc = Drains the station's power grid when wired in.
spy-listing-pax-name = Bottle of Pax
spy-listing-pax-desc = A reagent that stops the target from being able to cause harm.
spy-listing-nocturine-name = Bottle of Nocturine
spy-listing-nocturine-desc = A powerful sedative reagent.
spy-listing-omnizine-name = Bottle of Omnizine
spy-listing-omnizine-desc = A powerful general-purpose healing reagent.
spy-listing-mute-toxin-name = Bottle of Mute Toxin
spy-listing-mute-toxin-desc = A reagent that silences the target.
spy-listing-combat-medkit-name = Combat Medkit
spy-listing-combat-medkit-desc = A field kit for quickly treating serious wounds.

# Admin
admin-verb-text-make-spy = Make Spy
admin-verb-make-spy = Turns the player into a Spy antagonist with a Spy Uplink and pinpointer.
