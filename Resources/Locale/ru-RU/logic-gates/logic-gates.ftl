logic-gate-examine = В данным момент принадлежит шлюзу { INDEFINITE($gate) } { $gate }.

logic-gate-cycle = Переключено на шлюз { INDEFINITE($gate) } { $gate }

power-sensor-examine = В данный момент проверяет { $output ->
    [true] выходную
    *[false] входную
} батарею.
power-sensor-voltage-examine = Проверяет { $voltage } электросеть.

power-sensor-switch = Переключено на проверку { $output ->
    [true] выходной
    *[false] входной
} батареи.
power-sensor-voltage-switch = Сеть переключена на нормальный режим.
