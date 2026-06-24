defusable-examine-defused = { CAPITALIZE($name) } [color=lime]обезврежена[/color].
defusable-examine-live = { CAPITALIZE($name) } [color=red]тикает[/color] и до взрыва осталось [color=red]{ $time }[/color]
defusable-examine-live-display-off = { CAPITALIZE($name) } [color=red]тикает[/color], но таймер, по всей видимости, отключен.
defusable-examine-inactive = { CAPITALIZE($name) } [color=lime]неактивна[/color], но все еще может быть запущена.
defusable-examine-bolts = Болты { $down ->
    [true] [color=red]down[/color]
    *[false] [color=green]up[/color]
}.
