delivery-recipient-examine = Эта посылка предназначена для { $recipient }, { $job }.
delivery-already-opened-examine = Она уже была вскрыта.
delivery-earnings-examine = Доставка этого принесёт станции [color=yellow]{ $spesos }[/color] кредитов.
delivery-recipient-no-name = Безымянный
delivery-recipient-no-job = Неизвестно

delivery-unlocked-self = Вы разблокировали { $delivery } отпечатком пальца.
delivery-opened-self = Вы вскрываете { $delivery }.
delivery-unlocked-others = { CAPITALIZE($recipient) } unlocked the { $delivery } with { POSS-ADJ($possadj) } fingerprint.
delivery-opened-others = { CAPITALIZE($recipient) } opened the { $delivery }.

delivery-unlock-verb = Разблокировать
delivery-open-verb = Открыть
delivery-slice-verb = Вскрыть

delivery-teleporter-amount-examine =
    { $amount ->
        [one] It contains [color=yellow]{ $amount }[/color] delivery.
        *[other] It contains [color=yellow]{ $amount }[/color] deliveries.
    }
delivery-teleporter-empty = { CAPITALIZE($entity) } пуст.
delivery-teleporter-empty-verb = Взять почту


# modifiers
delivery-priority-examine = [color=yellow]Жёлтая приоритетная лента[/color] на замке защиты от вскрытия активна. Лучше доставить вовремя!
delivery-priority-delivered-examine = [color=orange]{ $type } с высоким приоритетом[/color]. Доставлено вовремя.
delivery-priority-expired-examine = [color=orange]{ $type } с высоким приоритетом[/color]. Время истекло.

delivery-fragile-examine = На ней [color=red]красная метка «Хрупкое»[/color].
delivery-fragile-broken-examine = [color=red]{ $type } имеет хрупкое содержимое[/color]. Выглядит сильно поврежденно.

delivery-bomb-examine = Это [color=purple]{ $type }-бомба[/color]. О нет.
delivery-bomb-primed-examine = Это [color=purple]{ $type }-бомба[/color]. Читать это – пустая трата вашего времени.
