### Locale for wielding items; i.e. two-handing them

wieldable-verb-text-wield = Взять в две руки
wieldable-verb-text-unwield = Взять в одну руку

wieldable-component-successful-wield = Вы берёте { $item } в две руки.
wieldable-component-failed-wield = Вы берёте { $item } в одну руку.
wieldable-component-successful-wield-other = { $user } берёт { $item } в две руки.
wieldable-component-failed-wield-other = { $user } берёт { $item } в одну руку.
wieldable-component-blocked-wield = { CAPITALIZE($blocker) } не даёт вам взять { $item } в две руки.

wieldable-component-no-hands = Вам не хватает рук!
wieldable-component-not-enough-free-hands = { $number ->
    [one] Вам нужна свободная рука, чтобы взять { $item } в две руки.
    [few] Вам нужно { $number } свободные руки, чтобы взять { $item } в две руки.
    *[other] Вам нужно { $number } свободных рук, чтобы взять { $item } в две руки.
}
wieldable-component-not-in-hands = { CAPITALIZE($item) } не в ваших руках!

wieldable-component-requires = { CAPITALIZE($item) } должно быть в руках!

gunwieldbonus-component-examine = Это оружие стреляет точнее в двух руках.

gunrequireswield-component-examine = Из этого оружия можно стрелять только держа его в двух руках.
