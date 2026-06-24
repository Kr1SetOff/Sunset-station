nuno-card-name-reverse = карта нуно
nuno-card-desc-reverse = Вы понятия не имеете, что это за карта...

nuno-card-name = { $suit } { $card }
nuno-card-desc = Такая простая, но веселая игра!

nuno-card-suit-name = { $suit ->
    [nunored] Красный
    [nunoyellow] Желтый
    [nunogreen] Зеленый
    [nunoblue] Синий
    *[invalid] !!{ $suit }!!
}

nuno-card-value-name = { $card ->
    [plus2] Плюс 2
    [reverse] Смена направления
    [skip] Пропуск хода
    *[other] { $card }
}

playing-card-wildcard = Дикая карта
playing-card-plus4 = Плюс 4
