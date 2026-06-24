job-no-requirements = У этой работы нет требований.
ghost-role-no-requirements = У этой роли нет требований.

# Coloring rule of thumb: limegreen for met requirement, yellow for unmet requirement that can still be met, red for unmeetable

role-timer-department-sufficient = У вас есть [color=limegreen]{ TOSTRING($current, "0") }[/color] из [color=lightblue]{ TOSTRING($required, "0") }[/color] необходимого времени игры в отделе [color={ $departmentColor }]{ $department }[/color].
role-timer-department-insufficient = У вас есть [color=yellow]{ TOSTRING($current, "0") }[/color] из [color=lightblue]{ TOSTRING($required, "0") }[/color] необходимого времени игры в отделе [color={ $departmentColor }]{ $department }[/color].
role-timer-department-not-too-high = У вас есть [color=limegreen]{ TOSTRING($current, "0") }[/color] из максимум [color=lightblue]{ TOSTRING($required, "0") }[/color] времени игры в отделе [color={ $departmentColor }]{ $department }[/color].
role-timer-department-too-high = У вас есть [color=red]{ TOSTRING($current, "0") }[/color] из максимум [color=lightblue]{ TOSTRING($required, "0") }[/color] времени игры в отделе [color={ $departmentColor }]{ $department }[/color]. (Вы пытаетесь сыграть роль новичка?)

role-timer-overall-sufficient = У вас есть [color=limegreen]{ TOSTRING($current, "0") }[/color] из [color=lightblue]{ TOSTRING($required, "0") }[/color] необходимого общего времени игры.
role-timer-overall-insufficient = У вас есть [color=yellow]{ TOSTRING($current, "0") }[/color] из [color=lightblue]{ TOSTRING($required, "0") }[/color] необходимого общего времени игры.
role-timer-overall-not-too-high = У вас есть [color=limegreen]{ TOSTRING($current, "0") }[/color] из максимум [color=lightblue]{ TOSTRING($required, "0") }[/color] общего времени игры.
role-timer-overall-too-high = У вас есть [color=red]{ TOSTRING($current, "0") }[/color] из максимум [color=lightblue]{ TOSTRING($required, "0") }[/color] общего времени игры. (Вы пытаетесь сыграть роль новичка?)

role-timer-role-sufficient = У вас есть [color=limegreen]{ TOSTRING($current, "0") }[/color] из [color=lightblue]{ TOSTRING($required, "0") }[/color] необходимого времени игры за [color={ $departmentColor }]{ $job }[/color].
role-timer-role-insufficient = У вас есть [color=yellow]{ TOSTRING($current, "0") }[/color] из [color=lightblue]{ TOSTRING($required, "0") }[/color] необходимого времени игры за [color={ $departmentColor }]{ $job }[/color].
role-timer-role-not-too-high = У вас есть [color=limegreen]{ TOSTRING($current, "0") }[/color] из максимум [color=lightblue]{ TOSTRING($required, "0") }[/color] времени игры за [color={ $departmentColor }]{ $job }[/color].
role-timer-role-too-high = У─вас есть [color=red]{ TOSTRING($current, "0") }[/color] из максимум [color=lightblue]{ TOSTRING($required, "0") }[/color] времени игры за [color={ $departmentColor }]{ $job }[/color]. (Вы пытаетесь сыграть роль новичка?)

role-whitelisted = Вы [color=limegreen]находитесь[/color] в белом списке (whitelist) для этой роли.
role-not-whitelisted = Вы [color=yellow]не находитесь[/color] в белом списке (whitelist) для этой роли.

role-timer-age-old-enough = Возраст вашего персонажа должен быть не менее [color=limegreen]{ $age }[/color] для этой роли.
role-timer-age-not-old-enough = Возраст вашего персонажа должен быть не менее [color=yellow]{ $age }[/color] для этой роли.
role-timer-age-young-enough = Возраст вашего персонажа должен быть не более [color=limegreen]{ $age }[/color] для этой роли.
role-timer-age-not-young-enough = Возраст вашего персонажа должен быть не более [color=yellow]{ $age }[/color] для этой роли.

role-timer-whitelisted-species-pass = Ваш персонаж [color=limegreen]должен[/color] принадлежать к одной из следующих рас: [color=limegreen]{ $species }[/color]
role-timer-whitelisted-species-fail = Ваш персонаж [color=yellow]должен[/color] принадлежать к одной из следующих рас: [color=yellow]{ $species }[/color]
role-timer-blacklisted-species-pass = Ваш персонаж [color=limegreen]не должен[/color] принадлежать к одной из следующих рас: [color=limegreen]{ $species }[/color]
role-timer-blacklisted-species-fail = Ваш персонаж [color=yellow]не должен[/color] принадлежать к одной из следующих рас: [color=yellow]{ $species }[/color]

role-timer-whitelisted-traits-pass = Ваш персонаж [color=limegreen]должен[/color] иметь одну из следующих черт: [color=limegreen]{ $traits }[/color]
role-timer-whitelisted-traits-fail = Ваш персонаж [color=yellow]должен[/color] иметь одну из следующих черт: [color=yellow]{ $traits }[/color]
role-timer-blacklisted-traits-pass = Ваш персонаж [color=limegreen]не должен[/color] иметь ни одну из следующих черт: [color=limegreen]{ $traits }[/color]
role-timer-blacklisted-traits-fail = Ваш персонаж [color=yellow]не должен[/color] иметь ни одну из следующих черт: [color=yellow]{ $traits }[/color]

role-ban = Вы были [color=red]забанены[/color] на этой роли.
