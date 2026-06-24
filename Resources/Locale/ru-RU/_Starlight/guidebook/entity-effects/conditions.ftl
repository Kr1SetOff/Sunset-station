entity-condition-guidebook-unknown-reagent = неизвестный реагент

entity-condition-guidebook-blood-reagent-threshold =
    { $max ->
        [2147483648] в кровотоке содержится не менее { NATURALFIXED($min, 2) } ед. { $reagent }
        *[other] { $min ->
                [0] в кровотоке содержится не более { NATURALFIXED($max, 2) } ед. { $reagent }
                *[other] в кровотоке содержится от { NATURALFIXED($min, 2) } ед. до { NATURALFIXED($max, 2) } ед. { $reagent }
            }
    }
