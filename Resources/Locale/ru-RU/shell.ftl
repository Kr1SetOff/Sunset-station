### for technical and/or system messages

## General

shell-command-success = Команда выполнена.
shell-invalid-command = Неверная команда.
shell-invalid-command-specific = Неверная команда { $commandName }.
shell-can-only-run-from-pre-round-lobby = Эту команду можно выполнить только в предраундовом лобби.
shell-can-only-run-while-round-is-active = Эту команду можно выполнить только во время активного раунда.
shell-cannot-run-command-from-server = Вы не можете выполнить эту команду из консоли сервера.
shell-only-players-can-run-this-command = Только игроки могут выполнять эту команду.
shell-must-be-attached-to-entity = Для выполнения этой команды вы должны быть прикреплены к сущности.
shell-must-have-body = У вашего разума должно быть физическое тело, чтобы выполнить эту команду.

## Arguments

shell-need-exactly-one-argument = Нужен ровно один аргумент.
shell-wrong-arguments-number-need-specific = Требуется { $properAmount ->
    [one] { $properAmount } аргумент
    [few] { $properAmount } аргумента
    *[other] { $properAmount } аргументов
}, но было передано { $currentAmount }.
shell-argument-must-be-number = Аргумент должен быть числом.
shell-argument-must-be-boolean = Аргумент должен быть булевым значением.
shell-wrong-arguments-number = Неправильное количество аргументов.
shell-need-between-arguments = Требуется от { $lower } до { $upper } аргументов!
shell-need-minimum-arguments = Требуется как минимум { $minimum ->
    [one] { $minimum } аргумент
    [few] { $minimum } аргумента
    *[other] { $minimum } аргументов
}!
shell-need-minimum-one-argument = Нужен хотя бы один аргумент!
shell-need-exactly-zero-arguments = Эта команда не принимает аргументов.

shell-argument-uid = EntityUid

## Guards

shell-missing-required-permission = У вас нет прав на использование этой команды! Требуется разрешение: { $perm }.
shell-entity-is-not-mob = Целевая сущность не является живым существом (мобом)!
shell-invalid-entity-id = Недопустимый ID сущности.
shell-invalid-grid-id = Недопустимый ID сетки (грида).
shell-invalid-map-id = Недопустимый ID карты.
shell-invalid-entity-uid = «{ $uid }» не является допустимым идентификатором EntityUid.
shell-invalid-bool = Неверное булевое значение.
shell-entity-uid-must-be-number = Идентификатор EntityUid должен быть числовым.
shell-could-not-find-entity = Не удалось найти сущность «{ $entity }».
shell-could-not-find-entity-with-uid = Не удалось найти сущность с UID { $uid }.
shell-entity-with-uid-lacks-component = Сущность с UID «{ $uid }» не имеет компонента «{ $componentName }».
shell-entity-target-lacks-component = Целевая сущность не имеет компонента «{ $componentName }».
shell-invalid-color-hex = Недопустимый HEX-код цвета!
shell-target-player-does-not-exist = Целевой игрок не существует!
shell-target-entity-does-not-have-message = Целевая сущность не имеет необходимого свойства ({ $missing })!
shell-timespan-minutes-must-be-correct = «{ $span }» не является допустимым промежутком времени в минутах.
shell-argument-must-be-prototype = Аргумент «{ $index }» должен быть прототипом типа «{ $prototypeName }»!
shell-argument-number-must-be-between = Аргумент «{ $index }» должен быть числом от { $lower } до { $upper }!
shell-argument-station-id-invalid = Аргумент «{ $index }» должен быть валидным ID станции!
shell-argument-map-id-invalid = Аргумент «{ $index }» должен быть валидным ID карты!
shell-argument-number-invalid = Аргумент «{ $index }» должен быть валидным числом!

# Hints
shell-argument-username-hint = <имя_пользователя>
shell-argument-username-optional-hint = [имя_пользователя]
