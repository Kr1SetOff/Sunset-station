server-ban-string-infinity = Навсегда
server-ban-no-name = Не найден. ({ $hwid })
server-time-ban =
    Temporary ban on { $mins } { $mins ->
        [one] minute
        [few] minutes
        *[other] minutes
    }.
server-perma-ban = Перманентный бан
server-role-ban =
    Temporary job-ban on { $mins } { $mins ->
        [one] minute
        [few] minutes
        *[other] minutes
    }.
server-perma-role-ban = Перманентный бан на роль
server-time-ban-string =
    > **Offender**
    > **Login:** ``{ $targetName }``
    > **Discord:** { $targetLink }

    > **Administrator**
    > **Login:** ``{ $adminName }``
    > **Discord:** { $adminLink }

    > **Time**
    > **Extended:** { $TimeNow }
    > **Expires:** { $expiresString }

    > **Reason:** { $reason }

    > **Severity Level:** { $severity }
server-ban-footer = { $server } | Раунд: #{ $round }
server-perma-ban-string =
    > **Offender**
    > **Login:** ``{ $targetName }``
    > **Discord:** { $targetLink }

    > **Administrator**
    > **Login:** ``{ $adminName }``
    > **Discord:** { $adminLink }

    > **Time**
    > **Extended:** { $TimeNow }

    > **Reason:** { $reason }

    > **Severity Level:** { $severity }
server-role-ban-string =
    > **Offender**
    > **Login:** ``{ $targetName }``
    > **Discord:** { $targetLink }

    > **Administrator**
    > **Login:** ``{ $adminName }``
    > **Discord:** { $adminLink }

    > **Time**
    > **Extended:** { $TimeNow }
    > **Expires:** { $expiresString }

    > **Roles:** { $roles }

    > **Reason:** { $reason }

    > **Severity Level:** { $severity }
server-perma-role-ban-string =
    > **Offender**
    > **Login:** ``{ $targetName }``
    > **Discord:** ``{ $targetLink }``

    > **Administrator**
    > **Login:** ``{ $adminName }``
    > **Discord:** { $adminLink }

    > **Time**
    > **Extended:** { $TimeNow }

    > **Roles:** { $roles }

    > **Reason:** { $reason }

    > **Severity Level:** { $severity }
