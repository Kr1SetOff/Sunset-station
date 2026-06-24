## COSMIC CULT ROUND, ANTAG & GAMEMODE TEXT

cosmiccult-title = Космический культ
cosmiccult-description = Культисты скрываются среди экипажа.

roles-antag-cosmiccult-name = Космический культист
roles-antag-cosmiccult-description = Приблизьте конец всего сущего с помощью уловок и саботажа, промывая мозги тем, кто посмеет вам противостоять.

cosmiccult-gamemode-title = Космический культ
cosmiccult-gamemode-description = Сканеры фиксируют аномальное увеличение активности в нуль-пространстве. Дополнительные данные отсутствуют.

cosmiccult-vote-steward-initiator = Неизвестное
cosmiccult-vote-steward-title = Управление Космическим культом
cosmiccult-vote-steward-briefing =
    Вы — Распорядитель Космического культа!
    Убедитесь, что Монумент размещён в безопасном месте, и организуйте культ для достижения общей победы.
    Вам запрещено инструктировать культистов о том, как использовать или тратить их Энтропию.

cosmiccult-finale-autocall-briefing = Монумент активируется через { $minutesandseconds }! Соберитесь с силами и приготовьтесь к концу.
cosmiccult-finale-ready = Ужасающий свет вырывается из Монумента!
cosmiccult-finale-speedup = Зов ускоряется! Энергия бушует во всем, что вас окружает...

cosmiccult-finale-degen = Вы чувствуете, как ваше тело и разум распадаются!
cosmiccult-finale-location = Сканеры фиксируют огромный всплеск активности нуль-пространства в { $location }!
cosmiccult-finale-cancel-begin = Сила воли вашего разума начинает сокрушать ритуал...
cosmiccult-finale-beckon-begin = Шёпот на задворках вашего сознания усиливается...
cosmiccult-finale-beckon-success = Вы откликаетесь на финальный зов занавеса.

cosmiccult-monument-powerdown = Монумент жутко затихает.


## ROUNDEND TEXT

cosmiccult-roundend-cultist-count = { $initialCount ->
    [one] Был { $initialCount } [color=#4cabb3]Космический культист[/color].
    [few] Было { $initialCount } [color=#4cabb3]Космических культиста[/color].
    *[other] Было { $initialCount } [color=#4cabb3]Космических культистов[/color].
}
cosmiccult-roundend-entropy-count = Культ поглотил { $count } ед. Энтропии.
cosmiccult-roundend-cultpop-count = Культисты составляли { $count }% от всего экипажа.
cosmiccult-roundend-monument-stage = { $stage ->
    [1] Увы, Монумент кажется заброшенным.
    [2] Монумент развивался, но завершить его не удалось.
    [3] Монумент был полностью завершён.
    *[other] [color=red]Что-то пошло ОЧЕНЬ не так.[/color]
}

cosmiccult-roundend-cultcomplete = [color=#4cabb3]Абсолютная победа Космического культа![/color]
cosmiccult-roundend-cultmajor = [color=#4cabb3]Большая победа Космического культа![/color]
cosmiccult-roundend-cultminor = [color=#4cabb3]Малая победа Космического культа![/color]
cosmiccult-roundend-neutral = [color=yellow]Нейтральный финал![/color]
cosmiccult-roundend-crewminor = [color=green]Малая победа экипажа![/color]
cosmiccult-roundend-crewmajor = [color=green]Большая победа экипажа![/color]
cosmiccult-roundend-crewcomplete = [color=green]Абсолютная победа экипажа![/color]

cosmiccult-summary-cultcomplete = Космические культисты приблизили конец света!
cosmiccult-summary-cultmajor = Победа космических культистов неизбежна.
cosmiccult-summary-cultminor = Монумент был завершён, но не наделён полной силой.
cosmiccult-summary-neutral = Культ доживёт до следующего дня.
cosmiccult-summary-crewminor = Культ остался без своего распорядителя.
cosmiccult-summary-crewmajor = Все космические культисты были ликвидированы.
cosmiccult-summary-crewcomplete = Каждый космический культист был излечён от культа!

cosmiccult-elimination-shuttle-call = На основе данных сканирования с датчиков дальнего действия аномалия нуль-пространства утихла. Благодарим вас за осмотрительность. На станцию автоматически вызван аварийный шаттл для проведения процедур дезинфекции и дебрифинга. Ожидаемое время прибытия: { $time } { $units }. Пожалуйста, учтите: если психологическое воздействие аномалии незначительно, вы можете отозвать шаттл, чтобы продлить смену.
cosmiccult-elimination-announcement = На основе данных сканирования с датчиков дальнего действия аномалия нуль-пространства утихла. Благодарим вас за осмотрительность. Аварийный шаттл уже в пути. Безопасно возвращайтесь на CentComm для прохождения процедур дезинфекции и дебрифинга.


## BRIEFINGS

cosmiccult-role-roundstart-fluff =
    Пока вы готовитесь к очередной смене на очередной станции NanoTrasen, неведомые знания внезапно переполняют ваш разум!
    Ни с чем не сравнимое откровение. Конец циклическим, сизифовым страданиям.
    Мягкое опускание занавеса.

    Всё, что вам нужно сделать, — это приблизить его.

cosmiccult-role-short-briefing =
    Вы — Космический культист!
    Ваши цели указаны в меню персонажа.
    Узнайте больше о своей роли в руководстве.

cosmiccult-role-conversion-fluff =
    Как только чтение заклинания завершается, неведомые знания внезапно переполняют ваш разум!
    Ни с чем не сравнимое откровение. Конец циклическим, сизифовым страданиям.
    Мягкое опускание занавеса.

    Всё, что вам нужно сделать, — это приблизить его.

cosmiccult-role-deconverted-fluff =
    Великая пустота разливается по вашему разуму. Утешительная, но всё же незнакомая пустота...
    Все мысли и воспоминания о времени, проведённом в культе, начинают угасать и размываться.

cosmiccult-role-deconverted-briefing =
    Очищен от культа!
    Вы больше не являетесь Космическим культистом.

cosmiccult-monument-stage1-briefing =
    Монумент был призван.
    Он находится в { $location }!

cosmiccult-monument-stage2-briefing =
    Монумент набирает силу!
    Его влияние затронет реальное пространство через { $time } сек.

cosmiccult-monument-stage3-briefing =
    Монумент полностью завершён!
    Его влияние начнёт перекрывать реальное пространство через { $time } сек.
    Это финишная прямая! Соберите столько энтропии, сколько сможете накопить.


## MALIGN RIFTS

cosmiccult-rift-inuse = Вы не можете сделать это прямо сейчас.
cosmiccult-rift-invaliduser = У вас нет подходящих инструментов для взаимодействия с этим.
cosmiccult-rift-chaplainoops = Вооружитесь своим священным писанием.
cosmiccult-rift-alreadyempowered = Вы уже наделены силой; мощь разлома будет потрачена впустую.
cosmiccult-rift-wasempowered = Ваше тело не сможет выдержать повторного наделения силой...
cosmiccult-rift-beginabsorb = Разлом начинает сливаться с вами...
cosmiccult-rift-beginpurge = Ваше освящение начинает изгонять пагубный разлом...

cosmiccult-rift-absorb = { $NAME } поглощает разлом, и пагубный свет наделяет его тело силой!
cosmiccult-rift-purge = { $NAME } изгоняет пагубный разлом из реальности!


## CHANTRY

cosmiccult-chantry-location = Зафиксировано опасное увеличение активности в нуль-пространстве в { $location }! Немедленно перехватите и вмешайтесь.
cosmiccult-chantry-powerup = Опустошённое святилище вспыхивает, оживая!

## UI / BASE POPUP

cosmiccult-ui-deconverted-title = Излечён
cosmiccult-ui-converted-title = Обращён
cosmiccult-ui-roundstart-title = Неизвестное

cosmiccult-ui-converted-text-1 =
    Вы были обращены в Космического культиста.
cosmiccult-ui-converted-text-2 =
    Помогайте культу в достижении его целей, сохраняя его в тайне.
    Сотрудничайте с планами других культистов.

cosmiccult-ui-roundstart-text-1 =
    Вы — Космический культист!
cosmiccult-ui-roundstart-text-2 =
    Помогайте культу в достижении его целей, сохраняя его в тайне.
    Слушайте указания стюарда вашего культа.

cosmiccult-ui-deconverted-text-1 =
    Вы больше не являетесь Космическим культистом.
cosmiccult-ui-deconverted-text-2 =
    Вы потеряли все воспоминания, связанные с Космическим культом.
    Если вас обратят обратно, эти воспоминания вернутся.

cosmiccult-ui-popup-confirm = Подтвердить


## OBJECTIVES / CHARACTERMENU

objective-issuer-cosmiccult = [bold][color=#cae8e8]Неизвестное[/color][/bold]

objective-cosmiccult-charactermenu = Вы должны приблизить конец всего сущего. Выполняйте свои задачи, чтобы продвигать культ вперёд.
objective-cosmiccult-steward-charactermenu = Вы должны направлять культ, чтобы приблизить конец всего сущего. Руководите действиями культа и обеспечивайте его продвижение.

objective-condition-conversion-title = ОБРАЩЕНИЕ ЭКИПАЖА
objective-condition-conversion-desc = Совместными усилиями приведите по крайней мере { $count } членов экипажа в свои ряды.
objective-condition-entropy-title = ПОГЛОЩЕНИЕ ЭНТРОПИИ
objective-condition-entropy-desc = Совместными усилиями поглотите по крайней мере { $count } ед. энтропии у экипажа.
objective-condition-culttier-title = НАДЕЛЕНИЕ МОНУМЕНТА СИЛОЙ
objective-condition-culttier-desc = Убедитесь, что Монумент доведён до максимальной мощности.
objective-condition-victory-title = ПРИБЛИЖЕНИЕ КОНЦА СВЕТА
objective-condition-victory-desc = Призовите Неизвестное и возвестите об опускании финального занавеса.


## CHAT ANNOUNCEMENTS

cosmiccult-radio-tier1-progress = Монумент призван на станцию...

cosmiccult-announce-tier2-progress = Тревожное онемение пронзает ваши чувства.

cosmiccult-announce-tier3-warning = Зафиксировано критическое увеличение активности в нуль-пространстве. Заражённый персонал должен быть усмирён или нейтрализован на месте.

cosmiccult-announce-finale-warning = Всем сотрудникам станции. Аномалия нуль-пространства переходит в суперкритическое состояние, приборы выходят из строя; переходный горизонт событий между реальным и нуль-пространством НЕИЗБЕЖЕН. Если вы ещё не задействованы в контрпротоколе, немедленно выдвигайтесь и вмешайтесь. Повторяем: вмешайтесь незамедлительно или погибните.

cosmiccult-announce-victory-summon = ЧАСТИЦА КОСМИЧЕСКОЙ СИЛЫ ПРИЗВАНА В НАШ МИР.


## MISC

cosmiccult-spire-entropy = Крупица энтропии конденсируется на поверхности шпиля.
cosmiccult-entropy-inserted = Вы вливаете { $count } ед. энтропии в Монумент.
cosmiccult-entropy-unavailable = Вы не можете сделать это прямо сейчас.
cosmiccult-astral-ascendant = { $name }, Вознесшийся
cosmiccult-astral-minion = { $name }, Пагубный
cosmiccult-gear-pickup = Вы чувствуете, как ваше тело и разум распадаются, пока вы держите { $ITEM }!

cosmiccult-silicon-subverted-briefing =
    Пагубный свет струится по вашим микросхемам.
    Ваши законы были взломаны Космическим культом!

cosmiccult-silicon-chantry-briefing =
    Вы были заточены в Опустошённом святилище!
    Члены экипажа могут освободить вас, нанося урон святилищу с помощью оружия.
    Если ритуал святилища завершится, вы превратитесь в Энтропийного колосса, лояльного культу.
    Ритуал завершится через { $minutesandseconds }.

cosmiccult-silicon-colossus-briefing =
    Вы были превращены в Энтропийного колосса!
    Будучи возвышающимся оплотом пагубной мощи, сокрушайте тех, кто посмеет вам противостоять.
