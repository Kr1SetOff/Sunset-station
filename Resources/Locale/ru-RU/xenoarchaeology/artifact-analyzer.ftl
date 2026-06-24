analysis-console-menu-title = аналитическая консоль
analysis-console-server-list-button = Список серверов
analysis-console-extract-button = Извлечь

analysis-console-info-no-scanner = Анализатор не подключен! Пожалуйста, подключите его с помощью мультитула.
analysis-console-info-no-artifact = Артефакт не найден! Поместите артефакт на платформу, затем просканируйте для получения данных.
analysis-console-info-ready = Все системы запущены. Сканирование готово.

analysis-console-no-node = Выберите узел для просмотра
analysis-console-info-id = ID_ОБЪЕКТА: { $id }
analysis-console-info-id-value = [font="Monospace" size=11][color=yellow]{ $id }[/color][/font]
analysis-console-info-class = [font="Monospace" size=11]Класс:[/font]
analysis-console-info-class-value = [font="Monospace" size=11]{ $class }[/font]
analysis-console-info-locked = [font="Monospace" size=11]Статус:[/font]
analysis-console-info-locked-value = [font="Monospace" size=11][color={ $state ->
    [0] red]Заблокирован
    [1] lime]Разблокирован
    *[2] plum]Активен
}[/color][/font]
analysis-console-info-durability = [font="Monospace" size=11]Прочность:[/font]
analysis-console-info-durability-value = [font="Monospace" size=11][color={ $color }]{ $current }/{ $max }[/color][/font]
analysis-console-info-effect = РЕАКЦИЯ: { $effect }
analysis-console-info-effect-value = [font="Monospace" size=11][color=gray]{ $state ->
    [true] { $info }
    *[false] Разблокируйте узлы для получения информации
}[/color][/font]
analysis-console-info-trigger = СТИМУЛЯТОР: { $trigger }
analysis-console-info-triggered-value = [font="Monospace" size=11][color=gray]{ $triggers }[/color][/font]
analysis-console-info-scanner = Сканирование...
analysis-console-info-scanner-paused = Пауза.
analysis-console-progress-text = { $seconds ->
    [one] Т-{ $seconds } секунда
    [few] Т-{ $seconds } секунды
    *[other] Т-{ $seconds } секунд
}

analysis-console-extract-value = [font="Monospace" size=11][color=orange]Узел { $id } (+{ $value })[/color][/font]
analysis-console-extract-none = [font="Monospace" size=11][color=orange] У разблокированых узлов не осталось очков для извлечения [/color][/font]
analysis-console-extract-sum = [font="Monospace" size=11][color=orange]Всего изучено: { $value }[/color][/font]

analyzer-artifact-extract-popup = Энергия мерцает на поверхности артефакта!
