moff-blade-server-rack-window-title = Стойка блейд-серверов
moff-blade-server-rack-window-footer-flavor = ПРОШИВКА УСТРОЙСТВА © 2125 NANOSOFT

moff-blade-server-rack-slot-status = Слот { $index }: { $content }

moff-blade-server-rack-slot-entity-unknown = неизвестно
moff-blade-server-rack-slot-empty = свободен

moff-blade-server-rack-slot-eject = Извлечь
moff-blade-server-rack-slot-insert = Вставить
moff-blade-server-rack-slot-power-toggle = Переключить питание

moff-blade-server-rack-slot-locked-fail = Заблокировано!
moff-blade-server-rack-slot-whitelist-fail = Это сюда не подходит!

moff-blade-server-rack-examine-empty = В ней [color=#1f8ab2]нет блейд-серверов[/color].
moff-blade-server-rack-examine-single = В ней находится только { $slot }.
moff-blade-server-rack-examine-multiple-start = Она содержит:
moff-blade-server-rack-examine-multiple-slot-line = - { $slot }
moff-blade-server-rack-examine-slot = { $name } [color=#1f8ab2]{ CAPITALIZE($name) }[/color] в слоте { $index }
moff-blade-server-rack-examine-distant =
    В ней находится [color=#1f8ab2]{ $numBlades } { $numBlades ->
        [one] блейд-сервер
        [few] блейд-сервера
        *[other] блейд-серверов
    }[/color], но с такого расстояния трудно понять, что это за { $numBlades ->
        [one] устройство
        *[other] устройства
    }.

moff-blade-server-frame-incompatible-board = Эта плата кажется несовместимой с корпусом...
moff-blade-server-board-compatible-hint = Ее можно использовать для создания [color=#1f8ab2]блейд-сервера[/color]
