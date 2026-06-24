## Base actions

alerts-vampire-blood-name = Опьянение кровью
alerts-vampire-blood-desc = Показывает, сколько крови вы выпили. Выпустите клыки и щелкните левой кнопкой мыши по цели, чтобы пить.

alerts-vampire-fed-name = Сытость кровью
alerts-vampire-fed-desc = Ваша текущая сытость кровью. Пейте кровь, чтобы оставаться сытым.

roles-antag-vamire-name = Вампир
roles-antag-vampire-description = Питайтесь экипажем. Выпускайте клыки и пейте их кровь.

roles-antag-thrall-name = Порабощённый
roles-antag-thrall-description = Верно служите своему господину и повинуйтесь его приказам.

vampire-roundend-name = вампир

vampire-drink-start = Вы вонзаете свои клыки в { CAPITALIZE(THE($target)) }.

vampire-not-enough-blood = Недостаточно крови.

vampire-mouth-covered = Ваш рот закрыт!
vampire-drink-invalid-target = Вы не можете пить кровь вампиров или их рабов.
vampire-target-protected-by-faith = Этот человек защищен своей верой!
vampire-drink-target-empty = В этом существе не осталось крови!
vampire-drink-target-maxed = Вы уже выпили { $amount } единиц крови из этой цели.
vampire-drink-target-hard-max = Вы выпили максимальное количество крови из этой цели ({ $amount } единиц).
vampire-full-power-achieved = Ваша вампирская сущность бурлит — достигнута полная сила!
vampire-umbrae-full-power-fov = Тени подчиняются вашей воле. Теперь вы можете видеть сквозь стены!
vampire-drink-target-not-viable = У этого существа нет бьющегося сердца!
vampire-drink-target-rot = Сущность этого существа отвратительна!
vampire-sleep-shielded = Это существо нельзя усыпить из-за импланта!
vampire-sleep-protected = Требуется лучший зрительный контакт...

vampire-role-greeting = Вы — вампир!
    Ваша жажда крови заставляет вас питаться членами экипажа. Используйте свои способности, чтобы обращать других.
    Ваши клыки позволяют сосать кровь у людей. Кровь восстановит здоровье и откроет новые способности.
    Найдите, чем заняться во время этой смены!

# Objectives
objective-issuer-vampire = [color=crimson]Вампир[/color]

objective-condition-drain-title = Выпить { $count } единиц крови
objective-condition-drain-description = Выпейте { $count } единиц крови из членов экипажа, используя свои клыки.

objective-vampire-thrall-obey-master-title = Повинуйтесь своему господину, { $targetName }.

# Class selection action
action-vampire-class-select = Выбрать класс вампира
action-vampire-class-select-desc = Выберите свой вампирский подкласс

# Round end statistics
roundend-prepend-vampire-drained-low = Вампиры едва насытились в эту смену, выпив всего { $blood } единиц крови.
roundend-prepend-vampire-drained-medium = Вампиры неплохо подкрепились, выпив { $blood } единиц крови.
roundend-prepend-vampire-drained-high = У вампиров был настоящий кровавый пир, они выпили { $blood } единиц крови!
roundend-prepend-vampire-drained-critical = Вампиры устроили безумие безудержного кормления, выпив ошеломляющие { $blood } единиц крови!

roundend-prepend-vampire-drained = Ни одному вампиру не удалось выпить сколько-нибудь значимого количества крови в этом раунде.
roundend-prepend-vampire-drained-named = { $name } оказался самым кровожадным вампиром, выпив в общей сложности { $number } единиц крови.

# Vampire class selection tooltips
vampire-class-hemomancer-tooltip = Хемомант
    Сосредоточен на магии крови и манипуляциях с кровью вокруг вас

vampire-class-umbrae-tooltip = Умбра
    Сосредоточен на тьме, скрытных засадах и мобильности

vampire-class-gargantua-tooltip = Гаргантюа
    Сосредоточен на стойкости и уроне в ближнем бою

vampire-class-dantalion-tooltip = Данталион
    Сосредоточен на порабощении рабов и иллюзиях

# Hemomancer abilities
action-vampire-hemomancer-tendrils-wrong-place = Невозможно сотворить это здесь.

action-vampire-blood-barrier-wrong-place = Невозможно разместить барьеры здесь.

action-vampire-sanguine-pool-already-in = Вы уже находитесь в форме кровавой лужи!
action-vampire-sanguine-pool-invalid-tile = Вы не можете превратиться в лужу крови здесь.
action-vampire-sanguine-pool-enter = Вы превращаетесь в лужу крови!
action-vampire-sanguine-pool-exit = Вы восстанавливаете форму из лужи крови!
vampire-space-burn-warning = Суровый свет космоса обжигает вашу немертвую плоть!

action-vampire-blood-eruption-activated = Вы заставляете кровь извергаться шипами вокруг вас!

action-vampire-blood-bringers-rite-not-enough-power = Вам не хватает полной вампирской силы (требуется более 1000 единиц крови всего и 8 уникальных жертв)
action-vampire-blood-brighters-rite-not-enough-blood = Недостаточно крови для активации ритуала вестников крови
action-vampire-blood-bringers-rite-start = Ритуал вестников крови активирован!
action-vampire-blood-bringers-rite-stop = Ритуал вестников крови деактивирован
action-vampire-blood-bringers-rite-stop-blood = Ритуал вестников крови деактивирован — недостаточно крови

vampire-locate-result = Ваши чувства отслеживают { $target } в { $location }.
vampire-locate-not-same-sector = Этот человек находится не в вашем секторе.
vampire-locate-unknown = Неизвестная область
vampire-locate-no-targets = В этом секторе не чувствуется добычи.

predator-sense-title = Чутье хищника
vampire-locate-search-placeholder = Поиск...

vampiric-claws-remove-popup = Вы убираете когти.

# Umbrae abilities
action-vampire-cloak-of-darkness-start = Вы сливаетесь с тенями!
action-vampire-cloak-of-darkness-stop = Вы выходите из теней.

action-vampire-shadow-snare-placed = Вы установили теневую ловушку-силок.
action-vampire-shadow-snare-wrong-place = Вы не можете разместить ловушку здесь.
action-vampire-shadow-snare-scatter = Вы развеяли теневую ловушку.
vampire-shadow-snare-oldest-removed = Ваша старая теневая ловушка рассеивается.
ent-shadow-snare-ensnare = теневая ловушка

action-vampire-shadow-anchor-returned = Вы вернулись к теневому якорю
action-vampire-shadow-anchor-installed = Вы закрепили место в тенях

action-vampire-shadow-boxing-start = Вы начинаете бой с тенью.
action-vampire-shadow-boxing-stop = Бой с тенью остановлен.
action-vampire-shadow-boxing-ends = Бой с тенью завершается.

action-vampire-dark-passage-wrong-place = Тьма здесь непроницаема...
action-vampire-dark-passage-activated = Вы скользнули сквозь тьму...

action-vampire-extinguish-activated = Вы поглотили свет вокруг себя... ({ $count })

action-vampire-eternal-darkness-not-enough-blood = У вас закончилась кровь для поддержания вечной тьмы.
action-vampire-eternal-darkness-start = Вы призвали вечную тьму...
action-vampire-eternal-darkness-stop = Вечная тьма рассеялась...

# Dantalion
vampire-enthrall-start = Вы проникаете в разум { CAPITALIZE(THE($target)) }...
vampire-enthrall-success = { CAPITALIZE(THE($target)) } преклоняет колено и становится вашим рабом.
vampire-enthrall-target = Ваш разум подавлен вампирским господством!
vampire-enthrall-limit = Вы не можете контролировать больше рабов.
vampire-enthrall-invalid = Эту цель невозможно поработить.
vampire-thrall-released = Вампирские узы, удерживающие вас, ослабевают.

vampire-pacify-invalid = Эту цель невозможно умиротворить.
vampire-pacify-success = { CAPITALIZE(THE($target)) } поддается вашему сокрушительному спокойствию.
vampire-pacify-target = Сокрушительное спокойствие топит вашу волю к борьбе!

vampire-subspace-swap-thrall = Вы не можете совершить подпространственный обмен со своими рабами.
vampire-subspace-swap-dead = Этот разум находится вне вашей досягаемости.
vampire-subspace-swap-failed = Подпространственный разлом бесполезно затухает.
vampire-subspace-swap-success = Пространство искажается, когда вы меняетесь местами с { CAPITALIZE(THE($target)) }!
vampire-subspace-swap-target = Реальность искривляется, и вас швыряет на новую позицию!

vampire-rally-thralls-success = { $count ->
    [one] Ваш призыв возвращает раба к вам!
    [few] Ваш призыв возвращает { $count } раба к вам!
    *[other] Ваш призыв возвращает { $count } рабов к вам!
}
vampire-rally-thralls-none = Ни один из ваших рабов не может ответить на призыв.
vampire-thrall-holy-water-freed = Святая вода очищает ваш разум от вампирских уз!

vampire-blood-bond-start = Потоки крови связывают вас с вашими рабами.
vampire-blood-bond-stop = Вы ослабляете кровную связь.
vampire-blood-bond-no-thralls = У вас нет порабощенных слуг, чтобы связаться с ними.
vampire-blood-bond-stop-blood = Связь разрывается; вам не хватает крови для её поддержания.

action-vampire-not-enough-power = Вашей силы недостаточно (требуется >1000 единиц крови всего и 8 уникальных жертв).

# Gargantua
vampire-blood-swell-start = Ваши мышцы раздуваются от нечестивой силы!
vampire-blood-swell-end = Кровавая ярость утихает.

vampire-blood-rush-start = Кровь приливает к вашим конечностям!
vampire-blood-rush-end = Ваша сверхъестественная скорость угасает.

vampire-seismic-stomp-activate = Земля содрогается под вашей яростью!

vampire-overwhelming-force-start = Ваше присутствие становится непоколебимым.
vampire-overwhelming-force-stop = Вы ослабляете свою железную хватку.
vampire-overwhelming-force-too-heavy = Этот объект слишком тяжелый, чтобы его сдвинуть!
vampire-overwhelming-force-door-pried = Вы выламываете дверь грубой силой.

vampire-demonic-grasp-hit = Демонический коготь хватает вас!
vampire-demonic-grasp-pull = Коготь тащит вас к вампиру!

vampire-charge-start = Вы мчитесь вперед с неудержимой силой!
vampire-charge-impact = Вы врезаетесь в { CAPITALIZE(THE($target)) } с разрушительной силой!


vampire-blood-swell-cancel-shoot = Ваши пальцы не пролезают в спусковую скобу!!

vampire-holy-place-burn = Священная земля обжигает вашу нечестивую плоть!

alerts-vampire-blood-swell-name = Кровавый прилив
alerts-vampire-blood-swell-desc = Ваши мышцы наполняются нечестивой силой.
alerts-vampire-blood-rush-name = Кровавый рывок
alerts-vampire-blood-rush-desc = Сверхъестественная скорость течет по вашим конечностям.

Vamp-converted-title = Порабощен!
Vamp-converted-text =
    Вы были порабощены!
    Верно служите своему господину. Вы можете получить доступ к коллективному разуму с помощью «+p»
Vamp-converted-confirm = Понятно
