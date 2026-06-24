### UI

# Displayed in the Character prefs window
humanoid-character-profile-summary =
    Это { $name }. { $gender ->
        [male] Ему
        [female] Ей
        [epicene] Им
        *[other] Ему
    } { $age ->
        [one] { $age } год
        [few] { $age } года
        *[other] { $age } лет
    }.
