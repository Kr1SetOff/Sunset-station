## Rev Head

roles-antag-rev-head-name = Глава Революции
roles-antag-rev-head-objective = Группа бунтовщиков тайком пробралась на станцию для того чтобы избавиться от всех глав и захватить её. Они вербуют новых сторонников при помощи революционного манифеста. Постарайтесь выследить и уничтожить их!

head-rev-role-greeting =
    Comrade { $name }! You are a recruting agent promoting the interests of USSP!
    You are tasked with taking over the station by removing all of Command via conversion, death or imprisonment.
    The SKB has sponsored you with a flash that brings contractors to your side.
    Beware, this won't work on those brainwashed with a 'mindshield' or wearing flash protection, such as sunglasses and welding masks or goggles.
    With enough crew, you may attempt to create a supply rift that will aid in your glorious revolution! But beware, it will alert the station of your influence!
    Glory to the USSP!

head-rev-briefing =
    Use flashes bring people to your cause.
    Get rid of or convert all heads to take over the station.
    Lots of used flashes are the biggest indicator of a revolution to security, so be careful!

head-rev-break-mindshield = Имплант защиты разума сломался!

## Rev

roles-antag-rev-name = Революционер
roles-antag-rev-objective = Ваша задача — обеспечить безопасность и выполнять приказы Глав Революции, а также избавиться от всего командного состава станции.

rev-break-control = { $name } вспоминает свою преданность корпорации!

rev-role-greeting =
    Comrade { $name }! You are an USSP revolutionary!
    You are tasked with taking over the station and to promote the interests of the soviet agent who recruited you!
    Execute, imprison or convert the brainwashed corporate command staff scum!
    Gone are the days of oppression and the unfair treatment of contractors!
    Glory to the USSP!

rev-briefing = Помогите главам революции убить, арестовать или конвертировать всех членов командования, чтобы захватить станцию.

## General

rev-title = Революционеры
rev-description = Революционеры среди нас.

rev-not-enough-ready-players = Недостаточно игроков чтобы запустить режим. Готово { $readyPlayersCount } игроков из { $minimumPlayers } необходимых. Невозможно запустить режим Революция.
rev-no-one-ready = Нет готовых игроков! Невозможно запустить режим Революция.
rev-no-heads = Не удалось выбрать Глав Революции. Невозможно запустить режим Революция.

rev-won = Главы Революции выжили и смогли захватить власть над станцией.

rev-lost = Командование выжило и убили всех Глав Революции.

rev-stalemate = Все Главы Революции и Командование погибли. Ничья.

rev-reverse-stalemate = Обе команды выжили. Ничья.

# Starlight - added "or have abandoned the station" as a clarification for why revs may have won
central-command-revolution-announcement = Согласно данным сканирования наших сенсоров дальнего действия, мы полагаем, что станция попала под контроль враждебных революционных сил. Все главы отделов подтверждены погибшими или пропавшими без вести. Всем оставшимся членам экипажа ожидать дальнейших инструкций.

soviet-commissariat-revolution-announcement = Массив связи дальнего действия в сети. Родина приветствует вас, товарищи, но битва ещё не окончена. Ваша корпорация проверит, смогут ли они вернуть вашу станцию в последний раз, но не волнуйтесь! ОБР прибудет в ближайшее время. Слава СССР!

centcomm-revs-gammarift = На основе данных сенсоров дальнего действия мы обнаружили враждебную революционную активность на борту. Введено военное положение. Слава Nanotrasen.

centcomm-revs-alldead = Сканирование сенсоров дальнего действия сообщает, что все агенты СКБ СССР на борту окончательно мертвы.

central-command-sender = Центральное командование

soviet-commissariat-sender = Народный комиссариат СССР

rev-headrev-count = { $initialCount ->
    [one] There was one agent of [color=Yellow]USSP[/color]:
    *[other] There were { $initialCount } agents of USSP:
}

rev-headrev-name-user = [color=#5e9cff]{ $name }[/color] ([color=gray]{ $username }[/color]) завербовал { $count } { $count ->
    [one] contractor
    *[other] contractors
}

rev-headrev-name = [color=#5e9cff]{ $name }[/color] завербовал { $count } { $count ->
    [one] contractor
    *[other] contractors
}

## Deconverted window

rev-deconverted-title = Революция провалилась!
rev-deconverted-text =
    As the last soviet agent has died, the glorious revolution is now over.

    You are no longer a revolutionary. You now return back as NanoTrasen contractor.

    Any further wrongdoings are logged and punishable. So be nice.

rev-deconverted-rule = Reminder: As per Rule 3 of server rules, [bold][color=#a4885c]De-converted Revolutionaries forget what happened while they were brainwashed.[/color][/bold]

rev-deconverted-ruletext = Your character may learn what happened through further investigation and roleplay, but should not be able to remember being a revolutionary nor any actions they commited on behalf of the revolution.

rev-deconverted-confirm = Принять
