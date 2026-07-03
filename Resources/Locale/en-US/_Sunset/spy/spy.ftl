# Spy antagonist (_Sunset)

# Preset / gamemode
spy-title = Spy
spy-description = A covert agent walks among the crew, fulfilling espionage contracts.
spy-round-end-agent-name = spy

# Role / antag
roles-antag-spy-name = Spy
roles-antag-spy-objective = Fulfill your espionage contracts without being caught.
role-subtype-spy = Spy

spy-role-greeting = You are a Spy. Use your Spy Uplink to accept covert contracts and earn Spy Credits.
spy-role-greeting-equipment = In your bag: a Spy Uplink (contracts & rewards), a Spy Tracker (plant on targets / sabotage devices) and a Spy Pinpointer (points to your current target).
spy-role-greeting-reputation = Your reputation with the Handler sets your pay: complete contracts to raise it and earn more. Abandoning an accepted contract lowers it. The harder the contract, the bigger the reputation swing.

# Currency / store
store-currency-display-spy-credit = spy credit
store-preset-name-spy = Spy Uplink
store-category-spy-contracts = Contracts
store-category-spy-gear = Gear
store-category-spy-weapons = Weapons
store-category-spy-espionage = Espionage
store-category-spy-medical = Chemistry & Medical

# Objective
objective-issuer-spy = [color=#3a7ca5]Handler[/color]
spy-objective-contracts-name = Fulfill { $count } espionage contracts
spy-objective-contracts-desc = Accept and complete contracts from your Spy Uplink.

# Contracts
spy-contract-surveillance-telecoms-name = Surveillance: Telecoms
spy-contract-surveillance-telecoms-desc = Plant your Spy Tracker on a telecommunications server and keep it running for the required time.
spy-contract-sabotage-smes-name = Sabotage: SMES
spy-contract-sabotage-smes-desc = Use your Spy Tracker on a SMES unit to discharge it.

# Reward listings
spy-listing-thermals-name = Thermal Vision Goggles
spy-listing-thermals-desc = See the body heat of living creatures through walls.
spy-listing-smoke-name = Smoke Grenade
spy-listing-smoke-desc = Cover your escape with a cloud of smoke.
spy-listing-emp-name = EMP Grenade
spy-listing-emp-desc = Disable electronics in a radius.
spy-listing-viper-name = Viper Pistol
spy-listing-viper-desc = A small concealable pistol.

# Popups
spy-contract-no-target = No valid target for this contract could be found.
spy-contract-accepted = Contract accepted: { $name }. Your pinpointer now points to the target. Reputation: { $reputation }.
spy-tracker-no-contract = You have no active contract. Accept one from your Spy Uplink first.
spy-tracker-wrong-target = This is not a valid target for your active contract.
spy-tracker-sabotage-start = You begin sabotaging the device...
spy-tracker-surveillance-start = You plant the tracker and begin surveillance. Stay close.
spy-tracker-assassinate-hint = This is an assassination contract. Eliminate the target yourself — the tracker won't help here.
spy-contract-completed = Contract complete! { $reward } spy credits added to your uplink. Reputation: { $reputation }.

# --- New popups ---
spy-contract-already-active = You already have an active contract. Finish or abandon it via your uplink.
spy-contract-nothing-to-abandon = You have no active contract to abandon.
spy-contract-abandoned = Contract abandoned. Reputation: { $reputation }.
spy-contract-target-lost = Your contract target was lost. Take a new contract.
spy-tracker-proximity-start = Bug planted. Stay close to the target to complete surveillance.
spy-tracker-collect-start = You begin downloading the data...

# --- Contract board slots (by difficulty) ---
spy-slot-easy-name = Easy contract (25 cr.)
spy-slot-easy-desc = Take a random easy contract. Reward: 25 spy credits.
spy-slot-medium-name = Medium contract (50 cr.)
spy-slot-medium-desc = Take a random medium contract. Reward: 50 spy credits.
spy-slot-hard-name = Hard contract (100 cr.)
spy-slot-hard-desc = Take a random hard contract. Reward: 100 spy credits.
spy-abandon-name = Abandon contract
spy-abandon-desc = Give up your current contract so you can take a new one.

# --- New rewards ---
spy-listing-agentid-name = Agent ID Card
spy-listing-agentid-desc = A forged ID with changeable name, job title and access.
spy-listing-chameleon-backpack-name = Chameleon Kit
spy-listing-chameleon-backpack-desc = A backpack with a full set of chameleon clothing: disguise as anyone.
spy-listing-chameleon-shoes-name = Chameleon Shoes
spy-listing-chameleon-shoes-desc = Chameleon footwear with no-slip protection.
spy-listing-chameleon-gloves-name = Chameleon Gloves
spy-listing-chameleon-gloves-desc = Thieving chameleon gloves: disguise and a careful touch.
spy-listing-chameleon-mask-name = Chameleon Mask
spy-listing-chameleon-mask-desc = A chameleon gas mask: disguise as any mask.
spy-listing-chameleon-projector-name = Chameleon Projector
spy-listing-chameleon-projector-desc = Project the appearance of any nearby object onto yourself.
spy-listing-storage-implant-name = Storage Implant
spy-listing-storage-implant-desc = A hidden subdermal pocket to smuggle items past a search.
spy-listing-dna-scrambler-name = DNA Scrambler Implant
spy-listing-dna-scrambler-desc = Changes your appearance and name once.
spy-listing-freedom-implant-name = Freedom Implant
spy-listing-freedom-implant-desc = Frees you from cuffs and restraints a few times.
spy-listing-escape-implant-name = Escape Implant
spy-listing-escape-implant-desc = Teleports you a short distance, breaking grabs and restraints.
spy-listing-invisible-armour-name = Covert Spy Armor
spy-listing-invisible-armour-desc = A bluespace-woven vest: invisible to everyone but you. Only you can take it off.

# --- Stealth weapons ---
spy-listing-cobra-name = Cobra Pistol
spy-listing-cobra-desc = A silenced pistol with a built-in suppressor. Fires .25 caseless rounds.
spy-listing-cobra-mag-name = Cobra Magazine (.25 caseless)
spy-listing-cobra-mag-desc = A spare Cobra magazine holding 10 .25 caseless rounds.
spy-listing-cobra-mag-ap-name = Cobra Magazine (.25 AP)
spy-listing-cobra-mag-ap-desc = A Cobra magazine of 10 .25 armor-piercing rounds: punches through armor, weaker on damage.
spy-listing-energydagger-name = Energy Dagger
spy-listing-energydagger-desc = A concealable folding blade with an energy edge.
spy-listing-throwing-knives-name = Throwing Knives
spy-listing-throwing-knives-desc = A set of silent throwing knives.
spy-listing-hypopen-name = Hypopen
spy-listing-hypopen-desc = A hypospray disguised as a pen for covert injections.

# --- Sabotage gear ---
spy-listing-signaller-name = Remote Signaller
spy-listing-signaller-desc = Remotely triggers connected devices.
spy-listing-radio-jammer-name = Radio Jammer
spy-listing-radio-jammer-desc = Jams nearby radios.
spy-listing-access-breaker-name = Access Breaker
spy-listing-access-breaker-desc = Breaks locks and access panels.
spy-listing-jaws-name = Syndicate Jaws of Life
spy-listing-jaws-desc = A compact tool for prying doors open.
spy-listing-powersink-name = Power Sink
spy-listing-powersink-desc = Drains power from the grid, blacking out a section of the station.

# --- Chemistry & medical ---
spy-listing-pax-name = Bottle of Pax
spy-listing-pax-desc = A reagent that stops the target from being able to cause harm.
spy-listing-nocturine-name = Bottle of Nocturine
spy-listing-nocturine-desc = A potent reagent: drops the target and puts them to sleep.
spy-listing-omnizine-name = Bottle of Omnizine
spy-listing-omnizine-desc = An all-purpose healing reagent for most damage types.
spy-listing-mute-toxin-name = Bottle of Toxin of Silence
spy-listing-mute-toxin-desc = Robs the target of speech so they can't call for help.
spy-listing-combat-medkit-name = Combat Medkit
spy-listing-combat-medkit-desc = A field kit for quickly treating serious wounds.

# --- Covert armor (only the wearer can remove it) ---
spy-owner-only-clothing-fail = Only the wearer can take this off.
spy-owner-only-clothing-examine = [color=#a5a5a5]Only the wearer can take this off.[/color]

# === Contracts: surveil crew ===
spy-contract-watch-crew-name = Surveillance: Crew member
spy-contract-watch-crew-desc = Plant a bug on any crew member and stay close without losing sight of them.
spy-contract-watch-ce-name = Surveillance: Chief Engineer
spy-contract-watch-ce-desc = Plant a bug on the Chief Engineer and stay close for the required time.
spy-contract-watch-cmo-name = Surveillance: Chief Medical Officer
spy-contract-watch-cmo-desc = Plant a bug on the Chief Medical Officer and stay close for the required time.
spy-contract-watch-hos-name = Surveillance: Head of Security
spy-contract-watch-hos-desc = Plant a bug on the Head of Security and stay close for the required time.
spy-contract-watch-captain-name = Surveillance: Captain
spy-contract-watch-captain-desc = Plant a bug on the Captain and stay close for the required time.

# === Contracts: surveil objects ===
spy-contract-watch-comms-name = Surveillance: Communications console
spy-contract-watch-comms-desc = Plant a bug on a communications console and stay close for the required time.
spy-contract-watch-apc-name = Surveillance: APC
spy-contract-watch-apc-desc = Plant a bug on an APC and stay close for the required time.
spy-contract-watch-research-name = Surveillance: Research server
spy-contract-watch-research-desc = Plant a bug on a research server and stay close for the required time.
spy-contract-watch-gravity-name = Surveillance: Gravity generator
spy-contract-watch-gravity-desc = Plant a bug on the gravity generator and stay close for the required time.

# === Contracts: sabotage devices ===
spy-contract-sabotage-apc-name = Sabotage: APC
spy-contract-sabotage-apc-desc = Use your Spy Tracker on an APC to discharge it.
spy-contract-sabotage-door-name = Sabotage: Door
spy-contract-sabotage-door-desc = Use your Spy Tracker on a door to bolt it shut.
spy-contract-sabotage-comms-name = Sabotage: Communications console
spy-contract-sabotage-comms-desc = Use your Spy Tracker on a communications console — an EMP pulse will disable it.
spy-contract-sabotage-gravity-name = Sabotage: Gravity generator
spy-contract-sabotage-gravity-desc = Use your Spy Tracker on the gravity generator to disable it for 5 minutes. It is not destroyed.

# === Contracts: collect data ===
spy-contract-collect-id-name = Collect data: ID card
spy-contract-collect-id-desc = Use your Spy Tracker on an ID card to copy its data.
spy-contract-collect-comms-name = Collect data: Communications console
spy-contract-collect-comms-desc = Use your Spy Tracker on a communications console to extract data.
spy-contract-collect-research-name = Collect data: Research server
spy-contract-collect-research-desc = Use your Spy Tracker on a research server to steal its research.
spy-contract-collect-telecoms-name = Collect data: Telecoms server
spy-contract-collect-telecoms-desc = Use your Spy Tracker on a telecoms server to intercept its traffic.
spy-contract-collect-valuable-name = Collect data: Valuable item
spy-contract-collect-valuable-desc = Use your Spy Tracker on a valuable item to copy its data.

# === Contracts: surveil valuables ===
spy-contract-watch-valuable-name = Surveillance: Valuable item
spy-contract-watch-valuable-desc = Plant a bug on a valuable item and stay close to it for the required time.

# === Contracts: assassinate ===
spy-contract-kill-crew-name = Assassinate: Crew member
spy-contract-kill-crew-desc = Eliminate any crew member. Only counts if you make the kill yourself.
spy-contract-kill-ce-name = Assassinate: Chief Engineer
spy-contract-kill-ce-desc = Eliminate the Chief Engineer yourself.
spy-contract-kill-cmo-name = Assassinate: Chief Medical Officer
spy-contract-kill-cmo-desc = Eliminate the Chief Medical Officer yourself.
spy-contract-kill-hop-name = Assassinate: Head of Personnel
spy-contract-kill-hop-desc = Eliminate the Head of Personnel yourself.
spy-contract-kill-hos-name = Assassinate: Head of Security
spy-contract-kill-hos-desc = Eliminate the Head of Security yourself.
spy-contract-kill-captain-name = Assassinate: Captain
spy-contract-kill-captain-desc = Eliminate the Captain yourself.

# === Admin ===
admin-verb-text-make-spy = Make Spy
admin-verb-make-spy = Turns the player into a Spy antagonist with an uplink, tracker and pinpointer.

cmd-spymake-desc = Makes the given player a Spy antagonist.
cmd-spymake-help = Usage: { $command } <player>
cmd-spymake-success = Player { $player } is now a Spy.
cmd-spyresetcontracts-desc = Resets contracts and progress for the given Spy.
cmd-spyresetcontracts-help = Usage: { $command } <player>
cmd-spyresetcontracts-success = Contracts for { $player } have been reset.
cmd-spynewcontract-desc = Gives a Spy a new random contract (optionally of a given difficulty).
cmd-spynewcontract-help = Usage: { $command } <player> [easy|medium|hard]
cmd-spynewcontract-success = A new contract was issued to { $player }.
cmd-spynewcontract-failed = Could not issue a contract to { $player } (no valid target?).
cmd-spynewcontract-bad-difficulty = Unknown difficulty: { $value }. Allowed: easy, medium, hard.
cmd-spynewcontract-arg-difficulty = difficulty (easy|medium|hard)
cmd-spy-not-a-spy = Player { $player } is not a Spy.
