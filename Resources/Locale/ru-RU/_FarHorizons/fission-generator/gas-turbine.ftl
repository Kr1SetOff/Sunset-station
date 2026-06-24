### Examine

gas-turbine-examine-stator-null = Похоже, статор отсутствует.
gas-turbine-examine-stator = Статор установлен.

gas-turbine-examine-blade-null = Похоже, лопасть турбины отсутствует.
gas-turbine-examine-blade = Лопасть турбины установлена.

gas-turbine-spinning-0 = Лопасти не вращаются.
gas-turbine-spinning-1 = Лопасти медленно вращаются.
gas-turbine-spinning-2 = Лопасти вращаются.
gas-turbine-spinning-3 = Лопасти быстро вращаются.
gas-turbine-spinning-4 = [color=red]Лопасти вышли из-под контроля![/color]

gas-turbine-damaged-0 = Кажется, оно в хорошем состоянии.[/color]
gas-turbine-damaged-1 = Турбина выглядит немного потертой.[/color]
gas-turbine-damaged-2 = [color=yellow]Турбина выглядит сильно поврежденной.[/color]
gas-turbine-damaged-3 = [color=orange]Она критически повреждена![/color]

gas-turbine-ruined = [color=red]Она полностью сломана![/color]

### Popups

# Shown when an event occurs
gas-turbine-overheat = { $owner } активирует аварийный клапан сброса перегрева!
gas-turbine-explode = { CAPITALIZE($owner) } разлетается на куски!

# Shown when damage occurs
gas-turbine-spark = { CAPITALIZE($owner) } начинает искрить!
gas-turbine-spark-stop = { CAPITALIZE($owner) } перестает искрить.
gas-turbine-smoke = { CAPITALIZE($owner) } начинает дымить!
gas-turbine-smoke-stop = { CAPITALIZE($owner) } перестает дымить.

# Shown during repairs
gas-turbine-repair-fail-blade = Нужно заменить лопасть турбины перед ремонтом.
gas-turbine-repair-fail-stator = Нужно заменить статор перед ремонтом.
gas-turbine-repair-ruined = Вы чините корпус { $target } с помощью { $tool }.
gas-turbine-repair-partial = Вы чините часть повреждений { $target } с помощью { $tool }.
gas-turbine-repair-complete = Вы закончили ремонт { $target } с помощью { $tool }.
gas-turbine-repair-no-damage = На { $target } нет повреждений, которые можно было бы исправить с помощью { $tool }.

# Anchoring warnings
gas-turbine-unanchor-warning = Вы не можете открутить { $owner }, пока турбина вращается!
gas-turbine-anchor-warning = Неверная позиция для закрепления.

gas-turbine-eject-fail-speed = Нельзя извлечь детали турбины, пока она вращается!
gas-turbine-insert-fail-speed = Нельзя вставить детали турбины, пока она вращается!

### UI

# Shown when using the UI
gas-turbine-ui-tab-main = Управление
gas-turbine-ui-tab-parts = Детали

gas-turbine-ui-rpm = ОБ/МИН

gas-turbine-ui-overspeed = ПЕРЕГРУЗКА
gas-turbine-ui-overtemp = ПЕРЕГРЕВ
gas-turbine-ui-stalling = ЗАГЛОХЛО
gas-turbine-ui-undertemp = НИЗК. ТЕМП.

gas-turbine-ui-flow-rate = Скорость потока
gas-turbine-ui-stator-load = Нагрузка статора

gas-turbine-ui-blade = Лопасть турбины
gas-turbine-ui-blade-integrity = Целостность
gas-turbine-ui-blade-stress = Напряжение

gas-turbine-ui-stator = Статор турбины
gas-turbine-ui-stator-potential = Потенциал
gas-turbine-ui-stator-supply = Питание

gas-turbine-ui-power = { POWERWATTS($power) }

gas-turbine-ui-locked-message = Управление заблокировано.
gas-turbine-ui-footer-left = Опасно: быстродвижущиеся механизмы.
gas-turbine-ui-footer-right = 2.1 REV 1
