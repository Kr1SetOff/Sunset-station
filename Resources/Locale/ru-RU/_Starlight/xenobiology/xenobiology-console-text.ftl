# The selectors in the case of 1 just don't work for some reason.
# Guess we're always going for plural?

xenobiology-console-monkey-cube-inserted = Thanks for inserting a monkey cube! The console now has { $cubes } { $cubes ->
    [1] cube
    *[other] cubes
}.

xenobiology-console-mutation-potion-inserted = Thanks for inserting a mutation potion! The console now has { $potions } { $potions ->
    [1] potion
    *[other] potions
}.

xenobiology-console-stabilizer-potion-inserted = Thanks for inserting a stabilizer potion! The console now has { $potions } { $potions ->
    [1] potion
    *[other] potions
}.

xenobiology-console-slime-picked-up = Вы подобрали слайма.
xenobiology-console-slime-picked-up-fail-full = Не удалось подобрать { $name }. Попробуйте выложить немного слаймов.
xenobiology-console-slime-picked-up-fail-none-found = Слаймы не найдены. Попробуйте подойти ближе к одному.

xenobiology-console-slime-placed-down = Вы положили слайма.
xenobiology-console-slime-placed-down-fail-none-stored = Нет сохранённых слаймов. Попробуйте подобрать одного.

xenobiology-console-monkey-placed = Placed down a monkey. You now have { $cubes } { $cubes ->
    [1] cube
    *[other] cubes
}.
xenobiology-console-monkey-placed-fail-empty = Недостаточно кубиков обезьяны ({ $cubes }). Попробуйте вставить один или переработать уже съеденных обезьян.

xenobiology-console-monkey-recycled = { GENDER($user) ->
    [1] monkey
    *[other] monkeys
}. You now have { $cubes } { $cubes ->
    [1] cube
    *[other] cubes
}.
xenobiology-console-monkey-recycled-failed-none = Не найдено обезьян для переработки. Попробуйте подойти ближе или убедитесь, что они достаточно повреждены.

xenobiology-console-mutation-potion-applied = Применено зелье мутации к { $name }. Теперь шанс мутации: { $chance }.
xenobiology-console-mutation-potion-applied-failed-empty = Нет зелий мутации. Попробуйте вставить одно.

xenobiology-console-stabilizer-potion-applied = Применено стабилизирующее зелье к { $name }. Теперь шанс мутации: { $chance }.
xenobiology-console-stabilizer-potion-applied-failed-empty = Нет стабилизирующих зелий. Попробуйте вставить одно.
