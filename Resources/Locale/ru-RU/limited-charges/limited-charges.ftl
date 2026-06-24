limited-charges-charges-remaining = { $charges ->
    [one] Остался [color=fuchsia]{ $charges }[/color] заряд.
    [few] Осталось [color=fuchsia]{ $charges }[/color] заряда.
    *[other] Осталось [color=fuchsia]{ $charges }[/color] зарядов.
}

limited-charges-max-charges = Оно на [color=green]максимуме[/color] зарядов.
limited-charges-recharging = { $seconds ->
    [one] Осталась [color=yellow]{ $seconds }[/color] секунда до следующего заряда.
    [few] Осталось [color=yellow]{ $seconds }[/color] секунды до следующего заряда.
    *[other] Осталось [color=yellow]{ $seconds }[/color] секунд до следующего заряда.
}
