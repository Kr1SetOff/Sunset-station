# Commands
## Delay shuttle round end
cmd-delayroundend-desc = Останавливает таймер окончания раунда, когда эвакуационный шаттл покидает гиперпространство.
cmd-delayroundend-help = Использование: delayroundend
emergency-shuttle-command-round-yes = Раунд продлён.
emergency-shuttle-command-round-no = Невозможно продлить окончание раунда.

## Dock emergency shuttle
cmd-dockemergencyshuttle-desc = Вызывает спасательный шаттл и пристыковывает его к станции... если это возможно.
cmd-dockemergencyshuttle-help = Использование: dockemergencyshuttle

## Launch emergency shuttle
cmd-launchemergencyshuttle-desc = Досрочно запускает эвакуационный шаттл, если это возможно.
cmd-launchemergencyshuttle-help = Использование: launchemergencyshuttle

# Emergency shuttle # Starlight edit: reword due to potential existence of multiple stations/shuttles
emergency-shuttle-left = Эвакуационный шаттл покинул станцию. Расчётное время прибытия шаттла на Центральное Командование — { $transitTime } секунд.
emergency-shuttle-launch-time = Эвакуационный шаттл будет запущен через { $consoleAccumulator } секунд.
emergency-shuttle-docked = Эвакуационный шаттл пристыковался к локации «{ $location }», направление: { $direction }. Он улетит через { $time } секунд.{ $extended }
emergency-shuttle-good-luck = Эвакуационный шаттл не может найти станцию. Удачи.
emergency-shuttle-nearby = Эвакуационный шаттл не может найти подходящий стыковочный шлюз. Он дрейфует около локации «{ $location }», направление: { $direction }. Он улетит через { $time } секунд.{ $extended }
emergency-shuttle-extended = { " " }Время до запуска было увеличено из-за неблагоприятных обстоятельств.

# Emergency shuttle console popup / announcement
emergency-shuttle-console-no-early-launches = Досрочный запуск отключён.
emergency-shuttle-console-auth-left = Требуется ещё { $remaining ->
    [one] { $remaining } авторизация
    [few] { $remaining } авторизации
    *[other] { $remaining } авторизаций
} для досрочного запуска шаттла.
emergency-shuttle-console-auth-revoked = Авторизация досрочного запуска отозвана. Требуется ещё { $remaining ->
    [one] { $remaining } авторизация
    [few] { $remaining } авторизации
    *[other] { $remaining } авторизаций
}.
emergency-shuttle-console-denied = Доступ запрещён.

# UI
emergency-shuttle-console-window-title = Консоль эвакуационного шаттла
emergency-shuttle-ui-engines = ДВИГАТЕЛИ:
emergency-shuttle-ui-idle = Простой
emergency-shuttle-ui-repeal-all = Отозвать все
emergency-shuttle-ui-early-authorize = Разрешение на досрочный запуск
emergency-shuttle-ui-authorize = АВТОРИЗОВАТЬСЯ
emergency-shuttle-ui-repeal = ОТОЗВАТЬ
emergency-shuttle-ui-authorizations = Авторизации
emergency-shuttle-ui-remaining = Осталось: { $remaining }

# Map Misc.
map-name-centcomm = Центральное Командование
map-name-terminal = Терминал Прибытия
