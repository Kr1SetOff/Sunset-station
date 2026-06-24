## UI

injector-volume-transfer-label = Volume: [color=white]{ $currentVolume }/{ $totalVolume }u[/color]
    Mode: [color=white]{ $modeString }[/color] ([color=white]{ $transferVolume }u[/color])
injector-volume-label = Объём: [color=white]{ $currentVolume }/{ $totalVolume }[/color]
    Mode: [color=white]{ $modeString }[/color]
injector-toggle-verb-text = Переключить режим инъектора

## Entity

injector-component-inject-mode-name = введение
injector-component-draw-mode-name = набор
injector-component-dynamic-mode-name = динамический
injector-component-mode-changed-text = Теперь { $mode }
injector-component-transfer-success-message = Вы перемещаете { $amount }ед. в цель ({ $target }).
injector-component-transfer-success-message-self = Вы перемещаете в себя { $amount } ед.
injector-component-inject-success-message = Вы вводите { $amount }ед. в цель ({ $target })!
injector-component-inject-success-message-self = Вы вводите в себя { $amount } ед.!
injector-component-draw-success-message = Вы набираете { $amount }ед. из цели ({ $target }).
injector-component-draw-success-message-self = Вы набираете из себя { $amount } ед.

## Fail Messages

injector-component-target-already-full-message = { $target } полон!
injector-component-target-already-full-message-self = Вы уже полны!
injector-component-target-is-empty-message = { $target } пуст!
injector-component-target-is-empty-message-self = You are empty!
injector-component-cannot-toggle-draw-message = Слишком полон, чтобы забрать!
injector-component-cannot-toggle-inject-message = Нет ничего для введения!
injector-component-cannot-toggle-dynamic-message = Не удалось переключить динамический режим!
injector-component-empty-message = { $injector } пуст!
injector-component-blocked-user = Защитное снаряжение заблокировало инъекцию!
injector-component-blocked-other = Броня { $target } заблокировала инъекцию { $user }!
injector-component-cannot-transfer-message = Вы не можете ничего переместить в цель ({ $target })!
injector-component-cannot-transfer-message-self = Вы не можете ничего переместить в себя!
injector-component-cannot-inject-message = Вы не можете ничего ввести в цель ({ $target })!
injector-component-cannot-inject-message-self = Вы не можете ничего себе ввести!
injector-component-cannot-draw-message = Вы не можете ничего набрать из цели ({ $target })!
injector-component-cannot-draw-message-self = Вы не можете ничего набрать из себя!
injector-component-ignore-mobs = Этот инъектор может взаимодействовать только с контейнерами!

## mob-inject doafter messages

injector-component-needle-injecting-user = Вы начинаете вводить иглу.
injector-component-needle-injecting-target = { $user } пытается ввести вам иглу!
injector-component-needle-drawing-user = Вы начинаете набирать жидкость иглой.
injector-component-needle-drawing-target = { $user } пытается набрать из вас жидкость иглой!
injector-component-spray-injecting-user = Вы начинаете подготавливать распылительную насадку.
injector-component-spray-injecting-target = { $user } пытается поместить на вас распылительную насадку!

## Target Popup Success messages
injector-component-feel-prick-message = Вы чувствуете лёгкий укол!
