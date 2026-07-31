# Всплывающие сообщения/осмотр/пункты меню для портированных заклинаний и предметов мага Goob-Station.
# Ни одного из этих ключей раньше не было в локализации - все вызовы ниже показывали игроку
# необработанный ключ вместо сообщения.

# Основные ошибки заклинаний (SharedSpellsSystem/SpellsSystem)
spell-fail-no-targets = В радиусе действия нет целей!
spell-fail-target-borg = На синтетическое тело это не подействует.
spell-fail-not-dead = Сначала цель должна быть мертва!
spell-fail-soul-not-bound = Ваша душа ни к чему не привязана.
spell-fail-item-destroyed = Ваш филактерий уничтожен!
spell-fail-item-on-another-plane = Ваш филактерий находится там, куда вам не добраться!
spell-fail-no-soul = Вашу душу больше не отдать - она уже не ваша!
spell-fail-bind-soul-silicon = У силиконовых форм жизни нет души для привязки.
spell-fail-no-held-entity = Вам нужно что-то держать в руке!
spell-fail-unremoveable = { CAPITALIZE(THE($item)) } не подходит для привязки души.
spell-fail-soul-item-not-suitable = { CAPITALIZE(THE($item)) } не годится для хранения души!
spell-fail-mutate-silicon = Силиконовые формы жизни не могут превратиться в чудовище.
spell-fail-lightning-bolt = У вас нет чистой линии на цель!
spell-fail-target-cant-wear-mask = Ваша цель не может носить маску!
spell-fail-target-cursed = Маска цели сопротивляется проклятию!
spell-fail-target-silicon = На синтетика это не подействует!
spell-fail-cant-wear-eyepatch = Вам некуда надеть повязку!
spell-fail-already-wear-eyepatch = На вас уже надета повязка силы!
spell-fail-sanguine-strike-no-item = Вам нужно держать оружие!
spell-fail-sanguine-strike-already-empowered = Ваше оружие уже жаждет крови!
spell-fail-sanguine-strike-not-weapon = { CAPITALIZE(THE($item)) } - не оружие!
spell-fail-hands-occupied = Ваши руки заняты!
spell-fail-tesla-blast = Молния рассеивается в никуда!
spell-fail-no-spells = У вас нет заклинаний для перезарядки!

spell-soul-tap-message = Вы чувствуете, как ваша жизненная сила утекает, питая следующее заклинание.
spell-soul-tap-almost-dead-message = Вы чувствуете себя стоящим на пороге смерти!
spell-soul-tap-dead-message-user = Всё погружается во тьму, когда последние остатки вашей жизненной силы иссякают.
spell-soul-tap-dead-message-others = { CAPITALIZE(THE($uid)) } падает замертво!

spell-charge-spells-charged-entity = Заклинания { THE($entity) } перезаряжены!
spell-charge-spells-charged-pulled = Ваши заклинания перезаряжены!
spell-charge-no-spells-to-charge-pulled = У вас нет заклинаний для перезарядки!

spell-rathen-fart-popup = Аппендикс { THE($target) } с хлопком и брызгами вырывается наружу!
spell-rathen-gut-popup = Ваш живот скручивает от боли!

spell-summon-simians-maxed-out-message = Обезьянья армия мага достигла своего пика - настоящее симианское вознесение!

instant-summons-item-marked = Вы помечаете { THE($item) } для призыва.

lich-greeting = Вы привязали свою душу к филактерию. Теперь вы лич - сама нежить будет поддерживать вас, пока цел ваш филактерий.

# Предмет "Мигание" (Content.Shared._Goobstation.Wizard.Blink)
blink-activated-message = Ваше тело начинает непредсказуемо мерцать.
blink-deactivated-message = Вы снова чувствуете себя устойчиво.

# Повязка чууни (Content.Shared._Goobstation.Wizard.Chuuni)
chuuni-eyepatch-backstory-1 = Эта повязка была выкована в сердце умирающей звезды, её сила скреплена древним договором света и тьмы. А может, вам просто нравится, как она выглядит.
chuuni-eyepatch-backstory-2 = Легенды гласят о воине, выколовшем себе глаз, чтобы заглянуть за грань реальности. Вам нравится думать, что этим воином были вы - в прошлой жизни.
chuuni-eyepatch-backstory-3 = Внутри повязки запечатан осколок повелителя демонов, молившего об освобождении. Давно от него не было вестей. Наверное, всё в порядке.
chuuni-eyepatch-backstory-4 = Одни говорят, что повязка сама выбирает своего носителя. Другие - что вы нашли её в коробке с хлопьями. И то, и другое по-своему правда.

# Меньшее призывание оружия (Content.Shared._Goobstation.Wizard.LesserSummonGuns)
enchanted-rifle-guns-left = В этой винтовке осталось { $guns } заколдованных { $guns ->
    [one] выстрел
    [few] выстрела
    [many] выстрелов
   *[other] выстрела
    }.

# Филактерий Bind Soul (Content.Shared._Goobstation.Wizard.BindSoul)
ensouled-item-desc = Внутри едва ощутимо теплится холодное присутствие, привязанное к чему-то, чего уже не должно существовать.
ensouled-item-name = Одушевлённый(-ая) { $item }

# Мутация/Халк (Content.Shared._Goobstation.Wizard.Mutate)
hulk-roar-1 = РААААХ!
hulk-roar-2 = КРУШИТЬ!
hulk-roar-3 = Я НЕОСТАНОВИМ!
hulk-roar-4 = ПОЧУВСТВУЙТЕ МОЙ ГНЕВ!
hulk-roar-5 = НИЧТО НЕ ОСТАНОВИТ МЕНЯ ТЕПЕРЬ!

# Ледяная ловушка (Content.Shared._Goobstation.Wizard.Traps)
ice-cube-break-free-start = Вы начинаете вырываться изо льда!

# Обескровливающий удар (Content.Shared._Goobstation.Wizard.SanguineStrike)
sanguine-strike-examine = Это оружие жаждет крови.

# Гадальный шар (Content.Server._Goobstation.Wizard.Systems.ScryingOrbSystem)
scrying-orb-verb-message = Покинуть тело и прозревать через шар.
scrying-orb-verb-text = Прозреть

# Клинок заклинаний (Content.Shared._Goobstation.Wizard.Spellblade)
spellblade-examine-enchantment = Этот клинок сейчас зачарован эффектом "{ $name }".

# Свиток телепортации (Content.Server/Content.Shared._Goobstation.Wizard.Teleport)
teleport-scroll-no-charges = У этого свитка не осталось зарядов!
teleport-scroll-uses-left = У этого свитка осталось { $uses } { $uses ->
    [one] телепортация
    [few] телепортации
    [many] телепортаций
   *[other] телепортации
    }.

# Ловушки мага (Content.Shared._Goobstation.Wizard.Traps)
trap-triggered-message = Вы задели { THE($trap) }!
trap-revealed-message = Вы замечаете { THE($trap) }, спрятанную поблизости!
trap-flare-message = { CAPITALIZE(THE($trap)) } вспыхивает, выдавая себя!

# Зеркало мага (Content.Server._Goobstation.Wizard.Systems.WizardMirrorSystem)
wizard-mirror-guardian-change-species-fail = Магия зеркала не может пробиться сквозь связь с вашим хранителем!

# Принудительная эмоция Rathen's Curse и бафф скорости Tile Toggle (Resources/Prototypes/_Goobstation/Wizard/misc_prototypes.yml)
chat-emote-name-fart-super = Пукнуть
chat-emote-msg-fart-super = издаёт оглушительный пук!
alerts-hierophant-beat-name = Ритм иерофанта
alerts-hierophant-beat-desc = Ваши шаги отдаются древним ритмом - вы двигаетесь быстрее.
