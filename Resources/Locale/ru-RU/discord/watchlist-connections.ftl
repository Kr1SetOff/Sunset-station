discord-watchlist-connection-header =
    { $players ->
        [one] К серверу { $serverName } подключился { $players } игрок из списка наблюдения
        [few] К серверу { $serverName } подключились { $players } игрока из списка наблюдения
        [many] К серверу { $serverName } подключились { $players } игроков из списка наблюдения
       *[other] К серверу { $serverName } подключились { $players } игроков из списка наблюдения
    }

discord-watchlist-connection-entry = - { $playerName } с сообщением «{ $message }»{ $expiry ->
    [0] { "" }
   *[other] { " " }(истекает <t:{ $expiry }:R>)
}{ $otherWatchlists ->
    [0] { "" }
    [one] { " " }и ещё { $otherWatchlists } список наблюдения
    [few] { " " }и ещё { $otherWatchlists } списка наблюдения
   *[other] { " " }и ещё { $otherWatchlists } списков наблюдения
}
