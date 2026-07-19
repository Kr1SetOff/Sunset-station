# 🌇Sunset🌇 - Кровяной червь

guide-entry-blood-worm = Кровяной червь

## Роль

roles-antag-blood-worm-name = Кровяной червь
roles-antag-blood-worm-objective = Убивай. Поглощай. Размножайся. Захватывай.
role-subtype-blood-worm = Кровяной червь
blood-worm-round-end-agent-name = кровяной червь

ghost-role-information-blood-worm-name = Кровяной червь
ghost-role-information-blood-worm-description = Провалившийся биопроект Синдиката: космическая пиявка, растущая на выпитой крови. Пейте кровь, растите, вселяйтесь в трупы и размножайтесь.
ghost-role-information-blood-worm-rules = Вы — антагонист. Экипаж — ваша еда. Соблюдайте общие правила сервера.

## Сущности

ent-MobBloodWormHatchling = детёныш кровяного червя
    .desc = Только что вылупившийся кровяной червь. Выглядит голодным и слабым — ему нужна кровь, чтобы вырасти.
ent-MobBloodWormJuvenile = молодой кровяной червь
    .desc = Кровяной червь среднего размера. Кровожаден, а его пасть усеяна длинными и острейшими зубами.
ent-MobBloodWormAdult = взрослый кровяной червь
    .desc = Чудовищный кровяной червь. Пожалуй, сунуть голову в промышленный измельчитель безопаснее, чем в его пасть.
ent-BloodWormCocoonMedium = кокон кровяного червя
    .desc = Пульсирующий кокон из затвердевшей крови. Внутри что-то растёт.
ent-BloodWormCocoonLarge = кокон кровяного червя
    .desc = { ent-BloodWormCocoonMedium.desc }
ent-BloodWormEggCocoon = яйцевой кокон кровяного червя
    .desc = Небольшой кокон из затвердевшей крови. Внутри что-то голодно копошится.
ent-ProjectileBloodWormSpit = едкая кровь
    .desc = Сгусток едкой крови.

## Действия

ent-ActionBloodWormLeech = Высосать кровь
    .desc = Вцепиться зубами в существо и высасывать его кровь. Кровь питает ваш рост и затягивает раны.
ent-ActionBloodWormMatureHatchling = Повзрослеть
    .desc = Свернуться в кокон и вырасти в молодого кровяного червя. Требуется 500 единиц выпитой крови.
ent-ActionBloodWormMatureJuvenile = Повзрослеть
    .desc = Свернуться в кокон и вырасти во взрослого кровяного червя. Требуется 1500 единиц выпитой крови суммарно.
ent-ActionBloodWormSpit = Плюнуть кровью
    .desc = Плюнуть едкой кровью в цель ценой собственного здоровья.
ent-ActionBloodWormInvade = Вселиться в труп
    .desc = Заползти в гуманоидный труп, в жилах которого ещё осталась кровь, и управлять им как марионеткой.
ent-ActionBloodWormLeaveHost = Покинуть носителя
    .desc = Покинуть тело носителя. Оно не переживёт вашего ухода.
ent-ActionBloodWormInject = Впрыснуть кровь
    .desc = Впрыснуть свою кровь в повреждённые ткани носителя, исцелив его ценой собственного здоровья.
ent-ActionBloodWormReviveHost = Оживить носителя
    .desc = Перезапустить кровообращение мёртвого носителя, вернув его к жизни.
ent-ActionBloodWormReproduce = Размножиться
    .desc = Отложить яйцевой кокон, из которого вылупится новый кровяной червь. Стоит 500 единиц выпитой крови.

## Оповещение о крови (иконка в статус-баре)

alerts-blood-worm-blood-name = Выпитая кровь
alerts-blood-worm-blood-desc = Показывает, сколько крови вы всего выпили за жизнь. Определяет, когда вы сможете повзрослеть.

## Цели

objective-issuer-blood-worm = [color=crimson]Голод[/color]

objective-condition-blood-worm-kill-title = Убить { $targetName }
objective-condition-blood-worm-kill-description = Устраните эту цель — не важно, как именно.
objective-condition-blood-worm-consume-title = Выпить {$amount} единиц крови
objective-condition-blood-worm-consume-description = Выпейте не менее {$amount} единиц крови за всю жизнь — считается кровь, выпитая на любой стадии роста.
objective-condition-blood-worm-survive-title = Выжить
objective-condition-blood-worm-survive-description = Доживите до конца раунда.

objective-condition-blood-worm-rp-silent-title = Безмолвный ужас
objective-condition-blood-worm-rp-silent-description = Ни разу не заговорите за весь раунд — общайтесь только укусами и телом.
objective-condition-blood-worm-rp-nest-title = Гнездо
objective-condition-blood-worm-rp-nest-description = Обустройте укромное гнездо где-нибудь на станции — из тряпья, тел, чего угодно — и время от времени возвращайтесь туда.
objective-condition-blood-worm-rp-taunt-title = Издёвка
objective-condition-blood-worm-rp-taunt-description = Оставьте на видном месте что-нибудь, что выдаёт ваше присутствие экипажу — но не выдаёт, кто вы.
objective-condition-blood-worm-rp-mimic-title = Маскировка
objective-condition-blood-worm-rp-mimic-description = Пока вы вселены в носителя, хотя бы раз выдайте себя за него в разговоре с кем-то, кто его хорошо знает.
objective-condition-blood-worm-rp-spare-title = Пощада
objective-condition-blood-worm-rp-spare-description = Отпустите хотя бы одну беспомощную жертву живой, не выпив её досуха.
objective-condition-blood-worm-rp-bloodline-title = Кровная линия
objective-condition-blood-worm-rp-bloodline-description = Дайте кому-то из своих детёнышей (из отложенного яйца) имя.

## Названия и описания целей-сущностей.
## Robust не резолвит name:/description: из objectives.yml как Loc ID сам по себе — только через
## ent-<id> (см. LocalizationManager.CalcEntityLoc) или явный вызов Loc.GetString из кода (как для
## цели "Убить" через TargetObjectiveSystem и "Выпить крови" через BloodWormConsumeConditionSystem).
## Без этого игрок видел бы сырой Loc ID вместо перевода.

ent-BloodWormKillObjective =
    .desc = { objective-condition-blood-worm-kill-description }
ent-BloodWormSurviveObjective = { objective-condition-blood-worm-survive-title }
    .desc = { objective-condition-blood-worm-survive-description }
ent-BloodWormRpObjectiveSilent = { objective-condition-blood-worm-rp-silent-title }
    .desc = { objective-condition-blood-worm-rp-silent-description }
ent-BloodWormRpObjectiveNest = { objective-condition-blood-worm-rp-nest-title }
    .desc = { objective-condition-blood-worm-rp-nest-description }
ent-BloodWormRpObjectiveTaunt = { objective-condition-blood-worm-rp-taunt-title }
    .desc = { objective-condition-blood-worm-rp-taunt-description }
ent-BloodWormRpObjectiveMimic = { objective-condition-blood-worm-rp-mimic-title }
    .desc = { objective-condition-blood-worm-rp-mimic-description }
ent-BloodWormRpObjectiveSpare = { objective-condition-blood-worm-rp-spare-title }
    .desc = { objective-condition-blood-worm-rp-spare-description }
ent-BloodWormRpObjectiveBloodline = { objective-condition-blood-worm-rp-bloodline-title }
    .desc = { objective-condition-blood-worm-rp-bloodline-description }

## Сообщения

blood-worm-no-blood = В этом теле нет крови.
blood-worm-leech-start = { CAPITALIZE($worm) } вцепляется зубами и начинает высасывать кровь!
blood-worm-leech-finish = { CAPITALIZE($worm) } жадно глотает кровь!
blood-worm-ready-to-mature = Вы чувствуете, как тело распирает — вы готовы повзрослеть!
blood-worm-not-enough-blood = Недостаточно выпитой крови: рост завершён лишь на {$percent}%.
blood-worm-maturing = { CAPITALIZE($worm) } сворачивается в кокон и исчезает внутри него!
blood-worm-cocoon-hatch = Кокон содрогается и лопается — из него появляется повзрослевший кровяной червь!
blood-worm-reproduce = { CAPITALIZE($worm) } откладывает пульсирующий кокон!
blood-worm-invade-not-corpse = Вселиться можно только в гуманоидный труп.
blood-worm-invade-occupied = В этом теле уже кто-то живёт.
blood-worm-invade-start = { CAPITALIZE($worm) } заползает в труп!
blood-worm-invade-finish = Тело содрогается и встаёт на ноги!
blood-worm-leave-host = Из тела вырывается окровавленный червь, и оно безжизненно оседает!
blood-worm-host-out-of-blood = В теле закончилась кровь!
blood-worm-host-died = Тело носителя перестало двигаться. Оживите его или покиньте, пока не поздно.
blood-worm-inject-success = Вы впрыскиваете кровь в повреждённые ткани носителя, исцеляя его.
blood-worm-inject-not-enough-health = У вас недостаточно здоровья, чтобы впрыснуть кровь.
blood-worm-revive-not-dead = Носитель ещё жив — оживлять некого.
blood-worm-revive-not-enough-health = У вас недостаточно здоровья, чтобы перезапустить кровообращение носителя.
blood-worm-revive-success = Кровь снова течёт по венам носителя — он возвращается к жизни!
