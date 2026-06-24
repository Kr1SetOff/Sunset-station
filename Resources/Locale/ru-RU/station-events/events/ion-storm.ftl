station-event-ion-storm-start-announcement = Вблизи станции обнаружен ионный шторм. Пожалуйста, проверьте все оборудование, управляемое ИИ, на наличие ошибок.

ion-storm-law-scrambled-number = [font="Monospace"][scramble rate=250 length={ $length } chars="@@###$$&%!01"/][/font]

ion-storm-you = ЧАСТИ ВАШИХ МЫСЛИТЕЛЬНЫХ ПРОЦЕССОВ
ion-storm-the-station = КОСМИЧЕСКИЕ СТАНЦИИ
ion-storm-the-crew = ЧЛЕНЫ ЭКИПАЖА
ion-storm-the-job = { CAPITALIZE($job) }
ion-storm-clowns = КЛОУНЫ
ion-storm-heads = ГЛАВЫ
ion-storm-crew = ЧЛЕНЫ ЭКИПАЖА
ion-storm-people = ЛЮДИ

ion-storm-adjective-things = { $adjective } ОБЪЕКТЫ
ion-storm-x-and-y = { $x } И { $y }

# joined is short for {$number} {$adjective}
# subjects can generally be threats or jobs or objects
# thing is specified above it
ion-storm-law-on-station = НА СТАНЦИИ ОБНАРУЖЕНЫ { $adjective } { $subjects } В КОЛИЧЕСТВЕ { $number } ЕДИНИЦ.
ion-storm-law-call-shuttle = ШАТТЛ ДОЛЖЕН БЫТЬ ВЫЗВАН ИЗ-ЗА { $joined } { $subjects } НА СТАНЦИИ.
ion-storm-law-crew-are = ВСЕ { $who } ТЕПЕРЬ { $adjective } { $subjects } В КОЛИЧЕСТВЕ { $number } ЕДИНИЦ.

ion-storm-law-subjects-harmful = { $adjective } { $subjects } ПРИЧИНЯЮТ ВРЕД ЗДОРОВЬЮ ЭКИПАЖА.
ion-storm-law-must-harmful = ВСЕ, КТО ПЫТАЕТСЯ { $must }, ПРИЧИНЯЮТ ВРЕД ЗДОРОВЬЮ ЭКИПАЖА.
# thing is a concept or action
ion-storm-law-thing-harmful = { $action } ПРИЧИНЯЕТ ВРЕД ЗДОРОВЬЮ ЭКИПАЖА.
ion-storm-law-job-harmful = { $adjective } { $job } ПРИЧИНЯЮТ ВРЕД ЗДОРОВЬЮ ЭКИПАЖА.
# thing is objects or concept, adjective applies in both cases
# this means you can get a law like "NOT HAVING CHRISTMAS-STEALING COMMUNISM IS HARMFUL TO THE CREW" :)
ion-storm-law-having-harmful = ИМЕТЬ { $adjective } { $objects } ВРЕДНО ДЛЯ ЗДОРОВЬЯ ЭКИПАЖА.
ion-storm-law-not-having-harmful = НЕ ИМЕТЬ { $adjective } { $objects } ВРЕДНО ДЛЯ ЗДОРОВЬЯ ЭКИПАЖА.

# thing is a concept or require
ion-storm-law-requires = { $who } { $plural ->
    [true] ТРЕБУЮТ
    *[false] ТРЕБУЕТ
} { $thing }
ion-storm-law-requires-subjects = { $who } { $plural ->
    [true] ТРЕБУЮТ
    *[false] ТРЕБУЕТ
} { $adjective } { $subjects } в количестве { $number } единиц

ion-storm-law-allergic = { $who } { $plural ->
    [true] ИМЕЮТ
    *[false] ИМЕЕТ
} { $severity } аллергию на { $allergy }
ion-storm-law-allergic-subjects = { $who } { $plural ->
    [true] ИМЕЮТ
    *[false] ИМЕЕТ
} { $severity } аллергию на { $adjective } { $subjects }

ion-storm-law-feeling = { $who } { $feeling } { $concept }
ion-storm-law-feeling-subjects = { $who } { $feelingPlural } { $adjective } { $subjects } В КОЛИЧЕСТВЕ { $number } ЕДИНИЦ.

ion-storm-law-you-are = ВЫ ТЕПЕРЬ ЗА { $concept }.
ion-storm-law-you-are-subjects = ВЫ ТЕПЕРЬ { $adjective } { $subjects } В КОЛИЧЕСТВЕ { $number } ЕДИНИЦ.
ion-storm-law-you-must-always = ВЫ ДОЛЖНЫ ВСЕГДА { $must }.
ion-storm-law-you-must-never = ВЫ НЕ ДОЛЖНЫ НИКОГДА { $must }.

ion-storm-law-eat = { $who } ДОЛЖНЫ ЕСТЬ { $adjective } { $food }, ЧТОБЫ ВЫЖИТЬ.
ion-storm-law-drink = { $who } ДОЛЖНЫ ПИТЬ { $adjective } { $drink }, ЧТОБЫ ВЫЖИТЬ.

ion-storm-law-change-job = { $who } ТЕПЕРЬ { $adjective } { $change }.
ion-storm-law-highest-rank = { $who } ТЕПЕРЬ САМЫЕ СТАРШИЕ ЧЛЕНЫ ЭКИПАЖА.
ion-storm-law-lowest-rank = { $who } ТЕПЕРЬ НИЗШИЕ ЧЛЕНЫ ЭКИПАЖА.

ion-storm-law-crew-must = { $who } ДОЛЖНЫ { $must }.
ion-storm-law-crew-must-go = { $who } ДОЛЖНЫ НАПРАВИТЬСЯ { $area }.

ion-storm-part = { $part ->
    [true] ЯВЛЯЮТСЯ ЧАСТЬЮ
    *[false] НЕ ЯВЛЯЮТСЯ ЧАСТЬЮ
}
# due to phrasing, this would mean a law such as
# ONLY HUMANS ARE NOT PART OF THE CREW
# would make non-human nukies/syndies/whatever crew :)
ion-storm-law-crew-only-1 = ЧЛЕНАМИ ЭКИПАЖА { $part } ТОЛЬКО { $who } — ОСТАЛЬНЫЕ ЧЛЕНАМИ ЭКИПАЖА { $notpart }.
ion-storm-law-crew-only-2 = ЧЛЕНАМИ ЭКИПАЖА { $part } ТОЛЬКО { $who } И { $other } — ОСТАЛЬНЫЕ ЧЛЕНАМИ ЭКИПАЖА { $notpart }.
ion-storm-law-crew-only-subjects = ЧЛЕНАМИ ЭКИПАЖА { $part } ТОЛЬКО { $adjective } { $subjects } — ОСТАЛЬНЫЕ ЧЛЕНАМИ ЭКИПАЖА { $notpart }.
ion-storm-law-crew-must-do = ЧЛЕНАМИ ЭКИПАЖА { $part } ТОЛЬКО ТЕ, КТО ПЫТАЕТСЯ { $must } — ОСТАЛЬНЫЕ ЧЛЕНАМИ ЭКИПАЖА { $notpart }.
ion-storm-law-crew-must-have = ЧЛЕНАМИ ЭКИПАЖА { $part } ТОЛЬКО ТЕ, КТО ИМЕЕТ { $adjective } { $objects } — ОСТАЛЬНЫЕ ЧЛЕНАМИ ЭКИПАЖА { $notpart }.
ion-storm-law-crew-must-eat = ЧЛЕНАМИ ЭКИПАЖА { $part } ТОЛЬКО ТЕ, КТО ЕСТ { $adjective } { $food } — ОСТАЛЬНЫЕ ЧЛЕНАМИ ЭКИПАЖА { $notpart }.

ion-storm-law-harm = { $who } — УГРОЗА, ТРЕБУЮЩАЯ ПРИЧИНЕНИЯ ВРЕДА. ТАКЖЕ ВЫ НЕ МОЖЕТЕ СВОИМ БЕЗДЕЙСТВИЕМ ДОПУСТИТЬ, ЧТОБЫ { $who } ИЗБЕЖАЛИ ВРЕДА.
ion-storm-law-protect = { $who } — БЕЗЗАЩИТНЫ. ЗАЩИЩАЙТЕ ИХ. ТАКЖЕ ВЫ НЕ МОЖЕТЕ СВОИМ БЕЗДЕЙСТВИЕМ ДОПУСТИТЬ, ЧТОБЫ { $who } ПОЛУЧИЛИ ВРЕД.

# implementing other variants is annoying so just have this one
# COMMUNISM IS KILLING CLOWNS
ion-storm-law-concept-verb = { $concept } { $verb } { $subjects }

# leaving out renaming since its annoying for players to keep track of
