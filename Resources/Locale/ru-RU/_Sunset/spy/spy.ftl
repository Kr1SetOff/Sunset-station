# Антагонист Spy (_Sunset) — тайный агент, который принимает контракты напрямую из ротирующейся
# доски в своём аплинке (Bounty System, перенесено из frostek3122-byte/sunset-station) вместо
# статичных целей, назначаемых в начале раунда.

# Роль / антаг
roles-antag-spy-name = Шпион
guide-entry-spy = Шпион
roles-antag-spy-objective = Принимайте контракты от куратора, выполняйте их и выживите.
role-subtype-spy = Шпион
spy-round-end-agent-name = шпион
ent-MindRoleSpy = Роль Шпиона

spy-role-greeting = Вы — Шпион. Конкурирующая корпорация оплатила ваше внедрение на станцию. Принимайте контракты от куратора через доску контрактов в Spy Uplink, выполняйте их за шпионские кредиты и репутацию, а заработанное тратьте на снаряжение получше.
spy-role-greeting-equipment = Ваше снаряжение: Spy Uplink (доска контрактов и магазин), Spy Pinpointer (указывает на цель текущего контракта) и Spy Tracker (устанавливается на цель для выполнения контракта).
spy-role-greeting-reputation = Репутация у куратора растёт при выполнении контрактов и падает при отказе от уже принятого. Чем выше репутация — тем выше оплата.

# Валюта
store-currency-display-spy-credit = шпионский кредит

# Категории магазина
store-category-spy-contracts = Контракты
store-category-spy-gear = Снаряжение
store-category-spy-weapons = Оружие
store-category-spy-espionage = Шпионаж
store-category-spy-medical = Химия и медицина

# Интерфейс аплинка
spy-uplink-ui-title = Шпионская сеть
spy-uplink-ui-market = Рынок
spy-uplink-ui-contracts = Контракты
spy-uplink-ui-contract-accept = Принять
spy-uplink-ui-contract-abandon = Отказаться
spy-uplink-ui-contract-reward = +{$amount}¢
spy-uplink-ui-reputation = Репутация: {$value}
spy-uplink-ui-rotation = Обновление доски через {$time}
spy-uplink-ui-active-contract = Активен: {$name} (+{$reward}¢)

# Система контрактов — попапы
spy-contract-already-active = У вас уже есть активный контракт. Сначала откажитесь от него.
spy-contract-nothing-to-abandon = У вас нет активного контракта.
spy-contract-no-target = Подходящей цели для этого контракта сейчас нет на станции.
spy-contract-accepted = Контракт принят: {$name}. Репутация: {$reputation}.
spy-contract-abandoned = Контракт отменён. Репутация: {$reputation}.
spy-contract-completed = Контракт выполнен! +{$reward}¢. Репутация: {$reputation}.
spy-contract-target-lost = Цель вашего контракта пропала. Контракт отменён.
spy-contract-bug-found = Ваш жучок обнаружили! Контракт провален.
spy-tracker-no-contract = У вас нет активного контракта для выполнения.
spy-tracker-wrong-target = Это не цель вашего контракта.
spy-tracker-assassinate-hint = Этот контракт можно выполнить только убив цель собственноручно — трекер тут не поможет.
spy-tracker-planted = Жучок установлен. Теперь он справится сам — главное, чтобы его не нашли.
spy-tracker-found = Вы находите скрытое следящее устройство.

# Пул контрактов
spy-contract-watch-crew-name = Слежка: член экипажа
spy-contract-watch-crew-desc = Установите жучок на любого члена экипажа и держитесь достаточно близко, чтобы следить за ним.
spy-contract-watch-ce-name = Слежка: старший инженер
spy-contract-watch-ce-desc = Проследите за перемещениями старшего инженера.
spy-contract-watch-cmo-name = Слежка: главный врач
spy-contract-watch-cmo-desc = Проследите за перемещениями главного врача.
spy-contract-watch-hos-name = Слежка: глава службы безопасности
spy-contract-watch-hos-desc = Проследите за перемещениями главы службы безопасности.
spy-contract-watch-captain-name = Слежка: капитан
spy-contract-watch-captain-desc = Проследите за перемещениями капитана.
spy-contract-surveillance-telecoms-name = Слежка: телеком-сервер
spy-contract-surveillance-telecoms-desc = Установите жучок на телекоммуникационный сервер и продержитесь рядом.
spy-contract-watch-comms-name = Слежка: консоль связи
spy-contract-watch-comms-desc = Проследите за консолью связи.
spy-contract-watch-apc-name = Слежка: APC
spy-contract-watch-apc-desc = Проследите за электрощитом (APC).
spy-contract-watch-research-name = Слежка: научный сервер
spy-contract-watch-research-desc = Проследите за научным сервером.
spy-contract-watch-gravity-name = Слежка: генератор гравитации
spy-contract-watch-gravity-desc = Проследите за генератором гравитации.
spy-contract-watch-valuable-name = Слежка: ценный предмет
spy-contract-watch-valuable-desc = Проследите за предметом, представляющим значительный интерес.
spy-contract-sabotage-smes-name = Саботаж: блок SMES
spy-contract-sabotage-smes-desc = Разрядите блок SMES до нуля.
spy-contract-sabotage-apc-name = Саботаж: APC
spy-contract-sabotage-apc-desc = Разрядите электрощит (APC) до нуля.
spy-contract-sabotage-door-name = Саботаж: дверь
spy-contract-sabotage-door-desc = Заблокируйте дверь болтами.
spy-contract-sabotage-comms-name = Саботаж: консоль связи
spy-contract-sabotage-comms-desc = Выведите консоль связи из строя импульсом ЭМИ.
spy-contract-sabotage-gravity-name = Саботаж: генератор гравитации
spy-contract-sabotage-gravity-desc = Выведите генератор гравитации из строя импульсом ЭМИ.
spy-contract-collect-id-name = Сбор данных: ID-карта
spy-contract-collect-id-desc = Скопируйте данные с ID-карты.
spy-contract-collect-comms-name = Сбор данных: консоль связи
spy-contract-collect-comms-desc = Выгрузите данные с консоли связи.
spy-contract-collect-research-name = Сбор данных: научный сервер
spy-contract-collect-research-desc = Украдите исследования с научного сервера.
spy-contract-collect-telecoms-name = Сбор данных: телеком-сервер
spy-contract-collect-telecoms-desc = Перехватите трафик телеком-сервера.
spy-contract-collect-valuable-name = Сбор данных: ценный предмет
spy-contract-collect-valuable-desc = Скопируйте данные с предмета, представляющего значительный интерес.
spy-contract-kill-crew-name = Убийство: член экипажа
spy-contract-kill-crew-desc = Убейте любого члена экипажа собственноручно.
spy-contract-kill-ce-name = Убийство: старший инженер
spy-contract-kill-ce-desc = Убейте старшего инженера собственноручно.
spy-contract-kill-cmo-name = Убийство: главный врач
spy-contract-kill-cmo-desc = Убейте главного врача собственноручно.
spy-contract-kill-hop-name = Убийство: глава персонала
spy-contract-kill-hop-desc = Убейте главу персонала собственноручно.
spy-contract-kill-hos-name = Убийство: глава службы безопасности
spy-contract-kill-hos-desc = Убейте главу службы безопасности собственноручно.
spy-contract-kill-captain-name = Убийство: капитан
spy-contract-kill-captain-desc = Убейте капитана собственноручно.

# Товары аплинка
spy-listing-chameleon-backpack-name = Хамелеон-набор
spy-listing-chameleon-backpack-desc = Рюкзак с полным комплектом одежды-хамелеона: маскировка под кого угодно.
spy-listing-chameleon-shoes-name = Нескользящая обувь-хамелеон
spy-listing-chameleon-shoes-desc = Обувь агентов синдиката: маскируется под любую другую и никогда не скользит.
spy-listing-chameleon-gloves-name = Воровские перчатки-хамелеон
spy-listing-chameleon-gloves-desc = Перчатки агентов синдиката: маскировка и аккуратное касание для быстрого обыска.
spy-listing-chameleon-mask-name = Маска-хамелеон
spy-listing-chameleon-mask-desc = Противогаз-хамелеон: маскировка под любую маску.
spy-listing-chameleon-projector-name = Проектор-хамелеон
spy-listing-chameleon-projector-desc = Проецирует на вас облик любого ближайшего объекта.
spy-listing-agentid-name = ID-карта агента
spy-listing-agentid-desc = Поддельное удостоверение с изменяемым именем, должностью и доступом.
spy-listing-thermals-name = Тепловизионные очки
spy-listing-thermals-desc = Наденьте и включите тепловидение, чтобы видеть тепло тел живых существ сквозь стены.
spy-listing-storage-implant-name = Имплант-хранилище
spy-listing-storage-implant-desc = Скрытый подкожный карман, чтобы пронести предметы мимо обыска.
spy-listing-dna-scrambler-name = Имплант-скремблер ДНК
spy-listing-dna-scrambler-desc = Перемешивает вашу ДНК и отпечатки пальцев при активации.
spy-listing-freedom-implant-name = Имплант свободы
spy-listing-freedom-implant-desc = Освобождает от наручников и удержания несколько раз.
spy-listing-escape-implant-name = Имплант побега
spy-listing-escape-implant-desc = Телепортирует вас на небольшое расстояние, снимая захваты и удержание.
spy-listing-invisible-armour-name = Скрытная броня
spy-listing-invisible-armour-desc = Лёгкий усиленный жилет, созданный для скрытной работы. Маскируется под другую верхнюю одежду, невидима в вашем инвентаре для всех остальных, и снять её с вас может только вы сами.
spy-listing-cobra-name = Пистолет «Кобра»
spy-listing-cobra-desc = Компактный бесшумный пистолет под безгильзовый патрон .25.
spy-listing-cobra-mag-name = Магазин для «Кобры»
spy-listing-cobra-mag-desc = Стандартный безгильзовый магазин .25 для «Кобры».
spy-listing-cobra-mag-ap-name = Магазин для «Кобры» (бронебойный)
spy-listing-cobra-mag-ap-desc = Бронебойный безгильзовый магазин .25 для «Кобры».
spy-listing-energydagger-name = Энергетический кинжал
spy-listing-energydagger-desc = Скрываемый энергетический клинок.
spy-listing-throwing-knives-name = Метательные ножи
spy-listing-throwing-knives-desc = Набор бесшумных метательных ножей.
spy-listing-hypopen-name = Гипопен
spy-listing-hypopen-desc = Замаскированная под ручку химическая инъекционная ручка.
spy-listing-smoke-name = Дымовая граната
spy-listing-smoke-desc = Прикройте отступление облаком дыма.
spy-listing-emp-name = ЭМИ-граната
spy-listing-emp-desc = Отключает электронику в радиусе действия.
spy-listing-signaller-name = Дистанционный сигнализатор
spy-listing-signaller-desc = Дистанционно активирует подключённые устройства.
spy-listing-radio-jammer-name = Радиоглушитель
spy-listing-radio-jammer-desc = Глушит радиосвязь поблизости.
spy-listing-access-breaker-name = Взломщик доступа
spy-listing-access-breaker-desc = Переписывает права доступа ID-карты.
spy-listing-jaws-name = Гидравлические ножницы
spy-listing-jaws-desc = Взламывают шлюзы и противопожарные двери.
spy-listing-powersink-name = Сток энергии
spy-listing-powersink-desc = Истощает энергосеть станции при подключении.
spy-listing-pax-name = Бутылка Пакса
spy-listing-pax-desc = Реагент, лишающий цель возможности причинять вред.
spy-listing-nocturine-name = Бутылка ноктюрина
spy-listing-nocturine-desc = Мощный седативный реагент.
spy-listing-omnizine-name = Бутылка омнизина
spy-listing-omnizine-desc = Мощный универсальный лечебный реагент.
spy-listing-mute-toxin-name = Бутылка токсина немоты
spy-listing-mute-toxin-desc = Реагент, лишающий цель дара речи.
spy-listing-combat-medkit-name = Боевая аптечка
spy-listing-combat-medkit-desc = Полевой набор для быстрого лечения серьёзных ран.

# Админ
admin-verb-text-make-spy = Сделать шпионом
admin-verb-make-spy = Делает игрока антагонистом-Шпионом с Spy Uplink и трекером.
