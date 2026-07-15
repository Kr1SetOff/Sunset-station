# SPDX-License-Identifier: AGPL-3.0-or-later

ghost-gui-arena-button = Арена
ghost-gui-arena-button-active = Арена ({ $count })

arena-window-title = Боевая арена
arena-window-info = Выберите режим и создайте бой. Другие призраки могут присоединиться, пока идёт таймер. Когда он закончится, все будут перемещены на арену. Побеждает последний выживший.

arena-mode-melee = Ближний бой
arena-mode-meleeranged = Ближний бой + Оружие
arena-mode-ranged = Оружие

arena-create-button = Создать арену
arena-join-button = Присоединиться ({ $count })
arena-leave-button = Покинуть
arena-spectate-button = Наблюдать

arena-status-idle = Арена не запущена. Выберите режим и создайте её.
arena-status-queueing = Сбор бойцов: { $count } ({ $time }с)
arena-status-fighting = Идёт бой: { $count } живых
arena-status-cooldown = Арена перезагружается...

# Sent to every other connected ghost when someone starts gathering a match.
arena-gathering-announcement = { $creator } начинает сбор на боевую арену! Режим: { $mode }. Загляните во вкладку «Арена», чтобы присоединиться.

arena-tab-fight = Бой
arena-tab-leaderboard = Топ игроков

arena-leaderboard-header-name = Игрок
arena-leaderboard-header-wins = Победы
arena-leaderboard-header-kills = Убийства
arena-leaderboard-header-deaths = Смерти
arena-leaderboard-header-kd = КД
arena-leaderboard-empty = Пока никто ещё не сражался в этом режиме.

# Name given to every temporary arena body.
arena-fighter-name = Сорвиголова

# Map-editor marker entity (Resources/Prototypes/_Sunset/Arena/spawners.yml) - where arena fighters spawn in.
ent-ArenaSpawner = точка спавна бойца арены
