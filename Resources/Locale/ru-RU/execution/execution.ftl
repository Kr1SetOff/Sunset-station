execution-verb-name = Казнить
execution-verb-message = Казнить кого-либо при помощи вашего оружия.

suicide-verb-name = Совершить самоубийство
suicide-verb-message = Используйте своё оружие, чтобы покончить с собой.

# All the below localisation strings have access to the following variables
# attacker (the person committing the execution)
# victim (the person being executed)
# weapon (the weapon used for the execution)

# STARLIGHT CONTROLLED
# God these need to move to their own file
execution-popup-melee-initial-internal = Вы заносите { $weapon } над горлом { $victim }.
execution-popup-gun-initial-internal = Вы направляете дуло { $weapon } на голову { $victim }.

execution-popup-melee-initial-external = { $attacker } заносит { $weapon } над горлом { $victim }.
execution-popup-gun-initial-external  = { CAPITALIZE($attacker) } направляет дуло своего { $weapon } на голову { $victim }.

execution-popup-melee-complete-internal = Вы перерезаете горло { $victim }!
execution-popup-gun-complete-internal = Вы стреляете { $victim } в голову!

execution-popup-melee-complete-external = { $attacker } перерезает горло { $victim }!
execution-popup-gun-complete-external = { CAPITALIZE($attacker) } стреляет { $victim } в голову!

execution-popup-gun-clumsy-internal = Вы промахиваетесь по голове { $victim } и вместо этого стреляете себе в ногу!
execution-popup-gun-clumsy-external = { CAPITALIZE($attacker) } промахивается по { $victim } и вместо этого стреляет себе в ногу!

execution-popup-gun-empty = { $weapon } щёлкает.

execution-popup-self-melee-initial-internal = Вы приставляете { $weapon } к собственному горлу.
execution-popup-self-gun-initial-internal = Вы засовываете дуло { $weapon } себе в рот.

execution-popup-self-melee-initial-external = { CAPITALIZE($attacker) } приставляет свой { $weapon } к собственному горлу.
execution-popup-self-gun-initial-external = { CAPITALIZE($attacker) } засовывает дуло своего { $weapon } себе в рот.

execution-popup-self-melee-complete-internal = Вы перерезаете себе горло!
execution-popup-self-gun-complete-internal = Вы стреляете себе в голову!

execution-popup-self-melee-complete-external = { CAPITALIZE($attacker) } перерезает себе горло!
execution-popup-self-gun-complete-external = { CAPITALIZE($attacker) } стреляет себе в голову!
# Starlight end
