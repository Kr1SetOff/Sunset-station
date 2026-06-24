## Survivor

roles-antag-survivor-name = Выживший
# It's a Halo reference
roles-antag-survivor-objective = Текущая задача: Выжить

survivor-role-greeting =
    Вы — выживший. Ваша главная задача — вернуться на Центральное командование живым.
    Соберите столько огневой мощи, сколько потребуется для обеспечения вашего выживания.
    Никому не доверяйте.

survivor-round-end-dead-count =
    { $deadCount ->
        [one] [color=red]{ $deadCount }[/color] выживший погиб.
        [few] [color=red]{ $deadCount }[/color] выживших погибли.
        [many] [color=red]{ $deadCount }[/color] выживших погибли.
        *[other] [color=red]{ $deadCount }[/color] выживших погибли.
    }

survivor-round-end-alive-count =
    { $aliveCount ->
        [one] [color=yellow]{ $aliveCount }[/color] выживший остался на станции.
        [few] [color=yellow]{ $aliveCount }[/color] выживших остались на станции.
        [many] [color=yellow]{ $aliveCount }[/color] выживших остались на станции.
        *[other] [color=yellow]{ $aliveCount }[/color] выживших остались на станции.
    }

survivor-round-end-alive-on-shuttle-count =
    { $aliveCount ->
        [one] [color=green]{ $aliveCount }[/color] выживший выбрался живым.
        [few] [color=green]{ $aliveCount }[/color] выживших выбрались живыми.
        [many] [color=green]{ $aliveCount }[/color] выживших выбрались живыми.
        *[other] [color=green]{ $aliveCount }[/color] выживших выбрались живыми.
    }

## Wizard

objective-issuer-swf = [color=turquoise]Федерация космических волшебников[/color]

wizard-title = Волшебник
wizard-description = Космический Волшебник посещает станцию, дабы продемонстрировать чудеса своей магии.

roles-antag-wizard-name = Космический волшебник
roles-antag-wizard-objective = Устройте хаос на станции

wizard-role-greeting =
    Время волшебства, огненный шар!
    Между Федерацией космических волшебников и NanoTrasen нарастает напряженность. Федерация космических волшебников выбрала вас для визита на станцию, чтобы «напомнить им», почему с заклинателями шутки плохи.
    Сейте хаос и разрушение! Что именно вы будете делать — решать вам, но помните, что Космические Волшебники хотят, чтобы вы вернулись живым.

wizard-round-end-name = волшебник

## TODO: Wizard Apprentice (Coming sometime post-wizard release)
