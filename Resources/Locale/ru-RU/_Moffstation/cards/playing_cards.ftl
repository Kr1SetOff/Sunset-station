playing-card-name-reverse = игральная карта
playing-card-desc-reverse = С этой стороны не понять, что это за карта.

playing-card-name = { $card } { $suit }
playing-card-desc = Мастерство исполнения изысканно!

playing-card-suit-name = { $suit ->
    [clubs] треф
    [diamonds] бубен
    [hearts] червей
    [spades] пик
    *[invalid] !!{ $suit }!!
}

playing-card-value-name = { $card ->
    [ace] Туз
    [j] Валет
    [q] Дама
    [k] Король
    *[other] { $card }
}

playing-card-joker = Джокер
