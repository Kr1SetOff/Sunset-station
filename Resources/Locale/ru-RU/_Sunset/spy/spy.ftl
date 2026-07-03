# Антагонист Spy (_Sunset)

# Пресет / режим
spy-title = Шпион
spy-description = Среди экипажа действует тайный агент, выполняющий шпионские контракты.
spy-round-end-agent-name = шпион

# Роль / антаг
roles-antag-spy-name = Шпион
roles-antag-spy-objective = Выполните шпионские контракты, не попавшись.
role-subtype-spy = Шпион

spy-role-greeting = Вы — Шпион. Используйте Spy Uplink, чтобы брать тайные контракты и зарабатывать шпионские кредиты.
spy-role-greeting-equipment = В вашей сумке: Spy Uplink (контракты и награды), Spy Tracker (установка на цели / саботаж устройств) и Spy Pinpointer (указывает на текущую цель).
spy-role-greeting-reputation = Репутация у Куратора определяет оплату: выполняйте контракты, чтобы поднять её и получать больше. Резкий отказ от принятого контракта роняет репутацию. Чем сложнее контракт — тем сильнее меняется репутация.

# Валюта / магазин
store-currency-display-spy-credit = шпионский кредит
store-preset-name-spy = Spy Uplink
store-category-spy-contracts = Контракты
store-category-spy-gear = Снаряжение
store-category-spy-weapons = Оружие
store-category-spy-espionage = Шпионаж
store-category-spy-medical = Химия и медицина

# Цель
objective-issuer-spy = [color=#3a7ca5]Куратор[/color]
spy-objective-contracts-name = Выполните { $count } шпионских контракта
spy-objective-contracts-desc = Берите и выполняйте контракты из вашего Spy Uplink.

# Контракты
spy-contract-surveillance-telecoms-name = Слежка: Телекоммуникации
spy-contract-surveillance-telecoms-desc = Установите Spy Tracker на телеком-сервер и удерживайте его рядом нужное время.
spy-contract-sabotage-smes-name = Саботаж: SMES
spy-contract-sabotage-smes-desc = Примените Spy Tracker на SMES, чтобы разрядить его.

# Награды
spy-listing-thermals-name = Тепловизионные очки
spy-listing-thermals-desc = Видеть тепло живых существ сквозь стены.
spy-listing-smoke-name = Дымовая граната
spy-listing-smoke-desc = Прикройте отход облаком дыма.
spy-listing-emp-name = ЭМИ-граната
spy-listing-emp-desc = Выводит из строя электронику в радиусе.
spy-listing-viper-name = Пистолет «Гадюка»
spy-listing-viper-desc = Маленький скрытный пистолет.

# Всплывающие сообщения
spy-contract-no-target = Не удалось найти подходящую цель для этого контракта.
spy-contract-accepted = Контракт принят: { $name }. Пинпойнтер указывает на цель. Репутация: { $reputation }.
spy-tracker-no-contract = У вас нет активного контракта. Сначала возьмите его в Spy Uplink.
spy-tracker-wrong-target = Это не подходящая цель для вашего активного контракта.
spy-tracker-sabotage-start = Вы начинаете саботаж устройства...
spy-tracker-surveillance-start = Вы устанавливаете трекер и начинаете слежку. Оставайтесь рядом.
spy-tracker-assassinate-hint = Этот контракт — убийство. Устраните цель собственноручно, трекер тут не поможет.
spy-contract-completed = Контракт выполнен! На ваш аплинк начислено { $reward } шпионских кредитов. Репутация: { $reputation }.

# --- Новые попапы ---
spy-contract-already-active = У вас уже есть активный контракт. Завершите его или сбросьте через аплинк.
spy-contract-nothing-to-abandon = У вас нет активного контракта для сброса.
spy-contract-abandoned = Контракт сброшен. Репутация: { $reputation }.
spy-contract-target-lost = Цель контракта потеряна. Возьмите новый контракт.
spy-tracker-proximity-start = Жучок установлен. Оставайтесь рядом с целью, чтобы завершить слежку.
spy-tracker-collect-start = Вы начинаете скачивать данные...

# --- Слоты доски контрактов (по сложности) ---
spy-slot-easy-name = Лёгкий контракт (25 кр.)
spy-slot-easy-desc = Взять случайный лёгкий контракт. Награда: 25 шпионских кредитов.
spy-slot-medium-name = Средний контракт (50 кр.)
spy-slot-medium-desc = Взять случайный контракт средней сложности. Награда: 50 шпионских кредитов.
spy-slot-hard-name = Сложный контракт (100 кр.)
spy-slot-hard-desc = Взять случайный сложный контракт. Награда: 100 шпионских кредитов.
spy-abandon-name = Сбросить контракт
spy-abandon-desc = Отказаться от текущего контракта, чтобы взять новый.

# --- Предметы аплинка: шпионаж/маскировка ---
spy-listing-agentid-name = Агентская ID-карта
spy-listing-agentid-desc = Поддельное удостоверение с изменяемыми именем, должностью и доступами.
spy-listing-chameleon-backpack-name = Хамелеон-набор
spy-listing-chameleon-backpack-desc = Рюкзак с полным комплектом одежды-хамелеона: маскируйтесь под кого угодно.
spy-listing-chameleon-shoes-name = Хамелеон-ботинки
spy-listing-chameleon-shoes-desc = Обувь-хамелеон с защитой от скольжения.
spy-listing-chameleon-gloves-name = Хамелеон-перчатки
spy-listing-chameleon-gloves-desc = Воровские перчатки-хамелеон: маскировка и аккуратные руки.
spy-listing-chameleon-mask-name = Хамелеон-маска
spy-listing-chameleon-mask-desc = Газовая маска-хамелеон: меняет облик под любую маску.
spy-listing-chameleon-projector-name = Хамелеон-проектор
spy-listing-chameleon-projector-desc = Спроецируйте на себя облик любого предмета поблизости.
spy-listing-storage-implant-name = Имплант-хранилище
spy-listing-storage-implant-desc = Скрытый подкожный карман для проноса предметов мимо обыска.
spy-listing-dna-scrambler-name = Имплант ДНК-скрамблера
spy-listing-dna-scrambler-desc = Одноразово меняет вашу внешность и имя.
spy-listing-freedom-implant-name = Имплант свободы
spy-listing-freedom-implant-desc = Несколько раз освобождает вас от наручников и пут.
spy-listing-escape-implant-name = Имплант побега
spy-listing-escape-implant-desc = Телепортирует вас на короткую дистанцию, вырывая из захвата и пут.
spy-listing-invisible-armour-name = Скрытная броня
spy-listing-invisible-armour-desc = Бронежилет из блюспейс-волокна: невидим для всех, кроме вас. Снять его можете только вы.

# --- Предметы аплинка: стелс-оружие ---
spy-listing-cobra-name = Пистолет «Кобра»
spy-listing-cobra-desc = Бесшумный пистолет со встроенным глушителем. Стреляет безгильзовыми патронами .25.
spy-listing-cobra-mag-name = Магазин «Кобры» (.25 безгильзовый)
spy-listing-cobra-mag-desc = Запасной магазин «Кобры» на 10 безгильзовых патронов .25.
spy-listing-cobra-mag-ap-name = Магазин «Кобры» (.25 бронебойный)
spy-listing-cobra-mag-ap-desc = Магазин «Кобры» на 10 бронебойных патронов .25: пробивает броню, но слабее по урону.
spy-listing-energydagger-name = Энергетический кинжал
spy-listing-energydagger-desc = Складной скрытный клинок с энергетическим лезвием.
spy-listing-throwing-knives-name = Метательные ножи
spy-listing-throwing-knives-desc = Набор бесшумных метательных ножей.
spy-listing-hypopen-name = Гипопен
spy-listing-hypopen-desc = Замаскированный под ручку шприц для скрытных инъекций.

# --- Предметы аплинка: саботаж ---
spy-listing-signaller-name = Дистанционный сигналлер
spy-listing-signaller-desc = Удалённо активирует подключённые устройства.
spy-listing-radio-jammer-name = Радиоглушилка
spy-listing-radio-jammer-desc = Глушит рации поблизости.
spy-listing-access-breaker-name = Взломщик доступа
spy-listing-access-breaker-desc = Взламывает замки и панели доступа.
spy-listing-jaws-name = Синдикатные «Челюсти жизни»
spy-listing-jaws-desc = Компактный инструмент для вскрытия дверей.
spy-listing-powersink-name = Паверсинк
spy-listing-powersink-desc = Высасывает энергию из сети, обесточивая участок станции.

# --- Предметы аплинка: химия и медицина ---
spy-listing-pax-name = Бутылочка пакса
spy-listing-pax-desc = Реагент, лишающий цель возможности причинять вред.
spy-listing-nocturine-name = Бутылочка ноктюрина
spy-listing-nocturine-desc = Мощный реагент: валит цель с ног и усыпляет.
spy-listing-omnizine-name = Бутылочка омнизина
spy-listing-omnizine-desc = Универсальное лечащее средство от большинства типов урона.
spy-listing-mute-toxin-name = Бутылочка токсина безмолвия
spy-listing-mute-toxin-desc = Лишает цель речи, не давая позвать на помощь.
spy-listing-combat-medkit-name = Боевая аптечка
spy-listing-combat-medkit-desc = Полевой набор для быстрого лечения тяжёлых ран.

# --- Скрытная броня (несъёмная для чужих) ---
spy-owner-only-clothing-fail = Эту вещь может снять только её носитель.
spy-owner-only-clothing-examine = [color=#a5a5a5]Снять это может только носитель.[/color]

# === Контракты: слежка за экипажем ===
spy-contract-watch-crew-name = Слежка: член экипажа
spy-contract-watch-crew-desc = Установите жучок на любого члена экипажа и держитесь рядом, не теряя его из виду.
spy-contract-watch-ce-name = Слежка: Старший инженер
spy-contract-watch-ce-desc = Установите жучок на Старшего инженера и держитесь рядом заданное время.
spy-contract-watch-cmo-name = Слежка: Главный врач
spy-contract-watch-cmo-desc = Установите жучок на Главного врача и держитесь рядом заданное время.
spy-contract-watch-hos-name = Слежка: Глава СБ
spy-contract-watch-hos-desc = Установите жучок на Главу Службы Безопасности и держитесь рядом заданное время.
spy-contract-watch-captain-name = Слежка: Капитан
spy-contract-watch-captain-desc = Установите жучок на Капитана и держитесь рядом заданное время.

# === Контракты: слежка за объектами ===
spy-contract-watch-comms-name = Слежка: Консоль связи
spy-contract-watch-comms-desc = Установите жучок на консоль связи и держитесь рядом заданное время.
spy-contract-watch-apc-name = Слежка: APC
spy-contract-watch-apc-desc = Установите жучок на распределитель питания (APC) и держитесь рядом заданное время.
spy-contract-watch-research-name = Слежка: Научный сервер
spy-contract-watch-research-desc = Установите жучок на научный сервер и держитесь рядом заданное время.
spy-contract-watch-gravity-name = Слежка: Генератор гравитации
spy-contract-watch-gravity-desc = Установите жучок на генератор гравитации и держитесь рядом заданное время.

# === Контракты: саботаж устройств ===
spy-contract-sabotage-apc-name = Саботаж: APC
spy-contract-sabotage-apc-desc = Примените Spy Tracker на APC, чтобы разрядить его.
spy-contract-sabotage-door-name = Саботаж: Дверь
spy-contract-sabotage-door-desc = Примените Spy Tracker на дверь, чтобы заблокировать её болтами.
spy-contract-sabotage-comms-name = Саботаж: Консоль связи
spy-contract-sabotage-comms-desc = Примените Spy Tracker на консоль связи — ЭМИ-импульс выведет её из строя.
spy-contract-sabotage-gravity-name = Саботаж: Генератор гравитации
spy-contract-sabotage-gravity-desc = Примените Spy Tracker на генератор гравитации, чтобы отключить его на 5 минут. Генератор не разрушается.

# === Контракты: сбор данных ===
spy-contract-collect-id-name = Сбор данных: ID-карта
spy-contract-collect-id-desc = Примените Spy Tracker на ID-карту, чтобы скопировать её данные.
spy-contract-collect-comms-name = Сбор данных: Консоль связи
spy-contract-collect-comms-desc = Примените Spy Tracker на консоль связи, чтобы выгрузить данные.
spy-contract-collect-research-name = Сбор данных: Научный сервер
spy-contract-collect-research-desc = Примените Spy Tracker на научный сервер, чтобы украсть исследования.
spy-contract-collect-telecoms-name = Сбор данных: Телеком-сервер
spy-contract-collect-telecoms-desc = Примените Spy Tracker на телеком-сервер, чтобы перехватить трафик.
spy-contract-collect-valuable-name = Сбор данных: Ценный предмет
spy-contract-collect-valuable-desc = Примените Spy Tracker на ценный предмет, чтобы скопировать его данные.

# === Контракты: слежка за ценными предметами ===
spy-contract-watch-valuable-name = Слежка: Ценный предмет
spy-contract-watch-valuable-desc = Установите жучок на ценный предмет и держитесь рядом с ним нужное время.

# === Контракты: заказное убийство ===
spy-contract-kill-crew-name = Убийство: член экипажа
spy-contract-kill-crew-desc = Устраните любого члена экипажа. Контракт засчитается, только если убьёте его сами.
spy-contract-kill-ce-name = Убийство: Старший инженер
spy-contract-kill-ce-desc = Устраните Старшего инженера собственноручно.
spy-contract-kill-cmo-name = Убийство: Главный врач
spy-contract-kill-cmo-desc = Устраните Главного врача собственноручно.
spy-contract-kill-hop-name = Убийство: Глава персонала
spy-contract-kill-hop-desc = Устраните Главу персонала собственноручно.
spy-contract-kill-hos-name = Убийство: Глава СБ
spy-contract-kill-hos-desc = Устраните Главу Службы Безопасности собственноручно.
spy-contract-kill-captain-name = Убийство: Капитан
spy-contract-kill-captain-desc = Устраните Капитана собственноручно.

# === Админка ===
admin-verb-text-make-spy = Сделать Шпионом
admin-verb-make-spy = Превращает игрока в антагониста Шпиона с аплинком, трекером и пинпойнтером.

cmd-spymake-desc = Делает указанного игрока антагонистом Шпионом.
cmd-spymake-help = Использование: { $command } <игрок>
cmd-spymake-success = Игрок { $player } стал Шпионом.
cmd-spyresetcontracts-desc = Сбрасывает контракты и прогресс у указанного Шпиона.
cmd-spyresetcontracts-help = Использование: { $command } <игрок>
cmd-spyresetcontracts-success = Контракты игрока { $player } сброшены.
cmd-spynewcontract-desc = Выдаёт Шпиону новый случайный контракт (опционально — заданной сложности).
cmd-spynewcontract-help = Использование: { $command } <игрок> [easy|medium|hard]
cmd-spynewcontract-success = Игроку { $player } выдан новый контракт.
cmd-spynewcontract-failed = Не удалось выдать контракт игроку { $player } (нет подходящей цели?).
cmd-spynewcontract-bad-difficulty = Неизвестная сложность: { $value }. Допустимо: easy, medium, hard.
cmd-spynewcontract-arg-difficulty = сложность (easy|medium|hard)
cmd-spy-not-a-spy = Игрок { $player } не является Шпионом.
