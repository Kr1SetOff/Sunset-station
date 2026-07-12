objectives-round-end-result = Игроков в роли «{ $agent }»: { $count }.

objectives-round-end-result-in-custody = Под стражей оказалось { $custody } из { $count } (роль «{ $agent }»).

objectives-player-user-named = [color=White]{ $name }[/color] ([color=gray]{ $user }[/color])
objectives-player-named = [color=White]{ $name }[/color]

objectives-no-objectives = { $custody }{ $title } был { $agent }.
objectives-with-objectives = { $custody }{ $title } был { $agent } с целями:

objectives-objective-success = { $objective } | [color=green]Успешно![/color] ({ TOSTRING($progress, "P0") })
objectives-objective-partial-success = { $objective } | [color=yellow]Частичный успех![/color] ({ TOSTRING($progress, "P0") })
objectives-objective-partial-failure = { $objective } | [color=orange]Частичный провал![/color] ({ TOSTRING($progress, "P0") })
objectives-objective-fail = { $objective } | [color=red]Неудачно![/color] ({ TOSTRING($progress, "P0") })

objectives-in-custody = [bold][color=red]| В ЗАКЛЮЧЕНИИ | [/color][/bold]
