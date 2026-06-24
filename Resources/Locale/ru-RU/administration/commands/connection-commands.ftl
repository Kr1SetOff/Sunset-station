## Strings for the "grant_connect_bypass" command.

cmd-grant_connect_bypass-desc = Временно разрешить пользователю игнорировать обычные проверки подключения.
cmd-grant_connect_bypass-help = Использование: grant_connect_bypass <пользователь> [продолжительность в минутах]
    Temporarily grants a user the ability to bypass regular connections restrictions.
    The bypass only applies to this game server and will expire after (by default) 1 hour.
    They will be able to join regardless of whitelist, panic bunker, or player cap.

cmd-grant_connect_bypass-arg-user = <пользователь>
cmd-grant_connect_bypass-arg-duration = [продолжительность в минутах]

cmd-grant_connect_bypass-invalid-args = Ожидалось 1 или 2 аргумента
cmd-grant_connect_bypass-unknown-user = Невозможно найти пользователя '{ $user }'
cmd-grant_connect_bypass-invalid-duration = Неверная продолжительность '{ $duration }'

cmd-grant_connect_bypass-success = Успешно добавлен обход для пользователя '{ $user }'
