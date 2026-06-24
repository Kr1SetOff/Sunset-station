# Loading Screen

replay-loading = Загрузка ({ $cur }/{ $total })
replay-loading-reading = Чтение файлов...
replay-loading-processing = Обработка данных...
replay-loading-spawning = Создание сущностей...
replay-loading-initializing = Инициализация сущностей...
replay-loading-starting= Запуск сущностей...
replay-loading-failed = Не удалось загрузить запись повтора. Ошибка:
                        { $reason }
replay-loading-retry = Загрузить, игнорируя критические ошибки (ВОЗМОЖНЫ БАГИ!)
replay-loading-cancel = Отмена

# Main Menu
replay-menu-subtext = Повторы
replay-menu-load = Загрузить выбранный повтор
replay-menu-select = Выбрать запись повтора
replay-menu-open = Открыть папку с повторами
replay-menu-none = Записи повторов не найдены.

# Main Menu Info Box
replay-info-title = Информация о повторе
replay-info-none-selected = Повтор не выбран
replay-info-invalid = [color=red]Выбран повреждённый или неверный файл повтора[/color]
replay-info-info = { "[" }color=gray]Файл:[/color]  { $name } ({ $file })
                   { "[" }color=gray]Время:[/color]   { $time }
                   { "[" }color=gray]ID раунда:[/color]   { $roundId }
                   { "[" }color=gray]Длительность:[/color]   { $duration }
                   { "[" }color=gray]ForkId:[/color]   { $forkId }
                   { "[" }color=gray]Версия билда:[/color]   { $version }
                   { "[" }color=gray]Движок:[/color]   { $engVersion }
                   { "[" }color=gray]Хэш типов:[/color]   { $hash }
                   { "[" }color=gray]Хэш комп.:[/color]   { $compHash }

# Replay selection window
replay-menu-select-title = Выбрать запись повтора

# Replay related verbs
replay-verb-spectate = Наблюдать

# command
cmd-replay-spectate-help = replay_spectate [EntityUid (опционально)]
cmd-replay-spectate-desc = Прикрепляет камеру локального игрока к сущности с указанным UID или открепляет от неё.
cmd-replay-spectate-hint = Опциональный EntityUid

cmd-replay-toggleui-desc = Включение или отключение интерфейса управления воспроизведением повтора.
