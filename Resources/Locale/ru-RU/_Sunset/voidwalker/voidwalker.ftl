guide-entry-voidwalker = Ходящий в пустоте

admin-verb-text-make-voidwalker = Сделать Ходящим в пустоте
admin-verb-make-voidwalker = Навсегда превращает игрока в Ходящего в пустоте, заменяя его тело.
voidwalker-polymorph-popup = Пустота поглощает { $parent } целиком, и на его месте из тьмы выходит { $child }.

ghost-role-information-voidwalker-name = Ходящий в пустоте
ghost-role-information-voidwalker-description = Скрытный хищник, дрейфующий в пустоте между звёзд — невидим, пока находится в открытом космосе. Похищайте недееспособных и утаскивайте их во тьму, либо просто пугайте экипаж из тени.
ghost-role-information-voidwalker-rules = Вы — Ходящий в пустоте, хищник-засадник родом из пустоты между звёзд. Вы почти невидимы, пока парите в открытом космосе (не стоите ни на одной решётке станции), и становитесь полностью видимы в тот же миг, как оказываетесь над решёткой. Вы можете совершать короткие рывки сквозь пустоту («Космический рывок»), пристально смотреть на кого-то, оглушая и являя себя ему («Смятение»), отправлять короткое телепатическое сообщение кому угодно («Космическая передача»), ненадолго превращать все стены, окна и решётки в области 3 на 3 клетки в проходимое стекло, чтобы пройти сквозь проём вместе с тем, кого тащите за собой («Остекление» — не действует на запитанные электричеством решётки/окна), а также похищать любого недееспособного в космосе. Похищенных автоматически переносит обратно на станцию с включёнными на максимум датчиками костюма, а внутри них начинает расти опухоль пустоты, которая постепенно чернит и ранит их, пока её не извлекут хирургическим путём — иначе через несколько минут превращение завершится, и жертва останется отмечена пустотой навсегда. У вас нет чёткой цели, кроме как быть пустотой — покажите им бездну, или не показывайте. Вы антагонист, но убивать не обязаны.

ent-MobVoidwalker = ходящий в пустоте
    .desc = Стеклянистое существо из пустоты между звёзд. Вам, наверное, не стоит на него пялиться.

ent-VoidwalkerCosmicSkull = космический череп
    .desc = Сквозь него видно и чувствуется пульсирующее вокруг пространство...

roles-antag-voidwalker-name = Ходящий в пустоте
roles-antag-voidwalker-objective = Покажите экипажу истину пустоты.
objective-issuer-voidwalker = [color=#a64dff]Пустота[/color]
voidwalker-round-end-agent-name = ходящий в пустоте

ent-VoidwalkerObjective = Покажи им истину
    .desc = Покажи им красоту пустоты. Утащи их в космическую бездну и открой им истину пустоты.

ent-ActionVoidwalkerDash = Космический рывок
    .desc = Совершите короткий рывок сквозь пустоту.
ent-ActionVoidwalkerUnsettle = Смятение
    .desc = Пристально смотрите на цель, пока она вас не заметит — оглушает её и раскрывает вас.
ent-ActionVoidwalkerTelepathy = Космическая передача
    .desc = Отправьте цели тревожное телепатическое сообщение.
ent-ActionVoidwalkerKidnap = Похищение
    .desc = Начните похищение недееспособной цели в открытом космосе, проклиная её пустотой.
ent-ActionVoidwalkerGlassify = Остекление
    .desc = Временно превратите все стены, окна и решётки в области 3 на 3 клетки в проходимое стекло — вы и любой, кого вы тащите за собой, сможете пройти прямо сквозь проём.

voidwalker-unsettle-no-los = Они не видят вас оттуда!
voidwalker-unsettle-success-self = Холодное присутствие вглядывается в вашу душу... а затем исчезает. Нечто во тьме только что явило себя вам.
voidwalker-unsettle-success-others = { $target } вздрагивает, будто от удара чем-то невидимым!
voidwalker-voided-fades = Космический холод покидает вас.

voidwalker-telepathy-sent = Вы посылаете свои мысли вовне...
voidwalker-telepathy-received = Холодный, чужой голос эхом отдаётся у вас в голове: "{ $phrase }"
voidwalker-telepathy-phrase-watching = Мы наблюдаем.
voidwalker-telepathy-phrase-cold = Здесь, снаружи, так холодно. Приди и посмотри.
voidwalker-telepathy-phrase-glass = Тебе нравится стекло? Нам нравится.
voidwalker-telepathy-phrase-come = Приди во тьму. Там теплее, чем ты думаешь.
voidwalker-telepathy-phrase-truth = Ты ещё не готов к истине. Пока.

voidwalker-kidnap-dead = Они уже мертвы!
voidwalker-kidnap-conscious = Они ещё в сознании!
voidwalker-kidnap-already-voided = Они уже видели пустоту!
voidwalker-kidnap-not-in-space = Они не в космосе!
voidwalker-kidnap-too-far = Подберитесь ближе!
voidwalker-kidnap-success-self = Пустота поглощает вас целиком, а затем выплёвывает обратно... изменённым.
voidwalker-kidnap-success-voidwalker = Они увидели истину.

voidwalker-glassify-invalid-target = Это невозможно превратить в стекло!
voidwalker-glassify-electrified = Через это пропущен ток — вас ударит!
voidwalker-glassify-already-glass = Это уже стекло!
voidwalker-glassify-no-los = Вам нужна прямая видимость на неё!
voidwalker-glassify-success = Стена содрогается и превращается в стекло...

voidwalker-tumor-removed = Холодный узел в груди исчез. То, что росло внутри вас, перестало расти.
voidwalker-tumor-consumed = Что-то внутри вас наконец разворачивается до конца. Ваша кожа теперь кажется неправильной — уже навсегда.

voidwalker-spawn-direction = Вы дрейфуете в открытом космосе. Станция где-то на { $direction }.
voidwalker-spawn-direction-north = севере
voidwalker-spawn-direction-south = юге
voidwalker-spawn-direction-east = востоке
voidwalker-spawn-direction-west = западе
