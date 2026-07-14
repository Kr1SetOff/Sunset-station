# SPDX-License-Identifier: AGPL-3.0-or-later

ghost-gui-arena-button = Arena
ghost-gui-arena-button-active = Arena ({ $count })

arena-window-title = Combat Arena
arena-window-info = Pick a mode and create a fight. Other ghosts can join while the timer runs. When it ends everyone is dropped into the arena. Last one standing wins.

arena-mode-melee = Melee
arena-mode-meleeranged = Melee + Guns
arena-mode-ranged = Guns

arena-create-button = Create arena
arena-join-button = Join ({ $count })
arena-leave-button = Leave
arena-spectate-button = Spectate

arena-status-idle = No arena is running. Pick a mode and create one.
arena-status-queueing = Gathering fighters: { $count } ({ $time }s)
arena-status-fighting = Fight in progress: { $count } alive
arena-status-cooldown = Arena resetting...

# Sent to every other connected ghost when someone starts gathering a match.
arena-gathering-announcement = { $creator } is gathering fighters for the arena! Mode: { $mode }. Check the Arena tab to join.

arena-tab-fight = Fight
arena-tab-leaderboard = Leaderboard

arena-leaderboard-header-name = Player
arena-leaderboard-header-wins = Wins
arena-leaderboard-header-kills = Kills
arena-leaderboard-header-deaths = Deaths
arena-leaderboard-header-kd = K/D
arena-leaderboard-empty = Nobody has fought in this mode yet.

# Name given to every temporary arena body.
arena-fighter-name = Сорвиголова
