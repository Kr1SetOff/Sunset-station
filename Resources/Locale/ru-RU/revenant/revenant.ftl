revenant-essence-amount = У вас [color=plum]{ $current } { $current ->
    [one] единица
    [few] единицы
    *[other] единиц
} эссенции[/color]. Ваш максимум — [color=plum]{ $max } { $max ->
    [one] единица
    [few] единицы
    *[other] единиц
} эссенции[/color].
revenant-max-essence-increased = Максимальный запас эссенции увеличился!

revenant-not-enough-essence = Недостаточно эссенции!
revenant-in-solid = Вы не можете использовать эту способность, пока находитесь внутри твёрдого объекта.

revenant-soul-too-powerful = Эта душа слишком сильна для сбора!
revenant-soul-harvested = Эта душа уже собрана!

revenant-soul-searching = Вы ищете душу { $target }.

revenant-soul-yield-high = Душа { $target } сильнее обычного!
revenant-soul-yield-average = Душа { $target } ничем не примечательна.
revenant-soul-yield-low = Душа { $target } слабее обычного.

revenant-soul-begin-harvest = { CAPITALIZE($target) } внезапно приподнимается в воздух, а { $gender ->
    [male] его кожа становится пепельно-серой
    [female] её кожа становится пепельно-серой
    [epicene] их кожа становится пепельно-серой
    *[other] его кожа становится пепельно-серой
}.
revenant-soul-finish-harvest = { CAPITALIZE($target) } падает на землю!

# UI
revenant-user-interface-title = Магазин способностей
revenant-user-interface-essence-amount = [color=plum]{ $amount }[/color] украденной эссенции

revenant-user-interface-cost = { $price } { $price ->
    [one] единица
    [few] единицы
    *[other] единиц
} эссенции
