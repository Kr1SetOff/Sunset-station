command-description-radio-addcustom =
    Добавляет пользовательский канал в указанный компонент сущности из конвейера. Укажите true или false в конце, чтобы гарантировать существование компонента.
command-description-radio-remcustom =
    Удаляет пользовательский канал с указанным ID из указанного компонента сущности из конвейера.
command-description-container-insertentity =
    Помещает указанную сущность в заданный контейнер сущности из конвейера.
command-description-container-insert =
    Помещает сущности из конвейера в указанный контейнер заданной сущности.
command-description-container-create =
    Создаёт новый контейнер на сущности из конвейера.
command-description-container-createslot =
    Создаёт новый слот контейнера на сущности из конвейера.
command-description-container-delete =
    Удаляет контейнер на сущности из конвейера.
command-description-container-drop =
    Выбрасывает все содержащиеся сущности из указанного контейнера сущности из конвейера.
command-description-container-dropandget =
    Выбрасывает все содержащиеся сущности из указанного контейнера сущности из конвейера и возвращает все выброшенные предметы вместо самой сущности.
command-description-container-dropanddelete =
    Выбрасывает все содержащиеся сущности из указанного контейнера сущности из конвейера, а затем удаляет этот контейнер.
command-description-container-get =
    Получает объект контейнера по указанному ID контейнера на сущности из конвейера.
command-description-container-getentities =
    Получает все сущности из указанного контейнера на сущности из конвейера.
command-description-container-getcontaining =
    Получает все контейнеры, в которых в данный момент находится сущность из конвейера.
command-description-container-getoutercontainer =
    Получает самый внешний контейнер, в котором находится сущность из конвейера.
command-description-container-getowner =
    Получает сущность, которая владеет указанным контейнером.
command-description-solution-adjcapacity =
    Регулирует вместимость указанного раствора.
command-description-solution-adjtemperature =
    Регулирует температуру указанного раствора.
command-description-solution-adjthermalenergy =
    Регулирует тепловую энергию указанного раствора.
command-description-solution-create=
    Создаёт новый раствор с указанным именем на сущности из конвейера. Возвращает уже существующий раствор, если он уже создан.
command-description-solution-delete=
    Удаляет указанный раствор на сущности из конвейера.
### Starlight (upstream #39080)
command-description-subtlemessage =
    Отправляет подсознательное сообщение всем переданным сущностям.
command-description-grid-getplayers =
    Получает всех игроков на переданных гридах.
command-description-grid-get =
    Получает гриды, на которых стоят переданные игроки.
command-description-grid-getstation =
    Получает станции, на которых стоят переданные игроки, или станцию самой сущности, если передан грид.
command-description-crewmanifest-addto =
    Добавляет переданную сущность в манифест экипажа указанной станции.
command-description-crewmanifest-removefrom =
    Удаляет переданную сущность из манифеста экипажа указанной станции.
command-description-crewmanifest-addplayer =
    Добавляет указанного игрока в манифест(ы) экипажа переданных станций.
command-description-crewmanifest-removeplayer =
    Удаляет указанного игрока из манифеста(ов) экипажа переданных станций.
command-description-storage-reshape =
    Изменяет форму хранилища на основе данных, предоставленных командой box2iconstructor.
command-description-box2iconstructor-new =
    Создаёт новое определение списка Box2i для сущности. Объедините с командами box2iconstructor:add, а затем выполните команду, которая его запрашивает.
command-description-box2iconstructor-add =
    Добавляет новый Box2i в существующее определение. Вызовите box2iconstructor:new перед использованием.
command-description-box2iconstructor-clean =
    Очищает неиспользуемое определение списка Box2i на сущности.
command-description-vector2dataconstructor-new =
    Создаёт новое определение списка Vector2 для сущности. Объедините с командами vector2dataconstructor:add, а затем выполните команду, которая его запрашивает.
command-description-vector2dataconstructor-add =
    Добавляет новый Vector2 в существующее определение. Вызовите vector2dataconstructor:new перед использованием.
command-description-vector2dataconstructor-clean =
    Очищает неиспользуемое определение списка Vector2 на сущности.
command-description-job-set =
    Изменяет профессию переданной сущности.
command-description-job-delset =
    Изменяет профессию переданной сущности путём удаления старой и назначения новой, чтобы воспроизвести брифинг.
command-description-ccomp-ensure =
    Гарантирует, что все клиенты добавят компонент с указанным именем к сущности, если он существует.
command-description-ccomp-write =
    Пытается заставить все клиенты выполнить vvwrite чего-либо в клиентский компонент.
command-description-ccomp-rm =
    Гарантирует, что все клиенты удалят компонент с указанным именем у сущности, если он существует.
command-description-globalsound-play =
    Воспроизводит звук глобально для переданных сущностей или сессий.
command-description-polymorph-begin =
    Маркер для начала последовательности инструкций по настройке полиморфа; добавляет PolymorphSetupComponent к сущности.
command-description-polymorph-setproto =
    Устанавливает прототип, в который превратится сущность при полиморфизме.
command-description-polymorph-seteffect =
    Устанавливает прототип эффекта, который появится поверх полиморфированной сущности (обычно используется для спецэффектов).
command-description-polymorph-setdelay =
    Устанавливает, сколько секунд нужно подождать перед повторной активацией этого конкретного полиморфа.
command-description-polymorph-setduration =
    Устанавливает длительность полиморфизма в секундах до автоматического возвращения в исходную форму.
command-description-polymorph-setforced =
    Делает полиморфизм принудительным, чтобы сущность не могла активировать или отменить его самостоятельно.
command-description-polymorph-settransferdamage =
    Переносит урон с текущей сущности на полиморфированную сущность.
command-description-polymorph-settransfername =
    Заставляет полиморфированную сущность унаследовать имя оригинальной сущности.
command-description-polymorph-settransferappearance =
    Переносит ли такие параметры, как причёска, цвет кожи, рост и т. д., на полиморфированную сущность.
command-description-polymorph-setinventory =
    Определяет, как инвентарь сущности перенесётся на полиморфированную сущность.
command-description-polymorph-setrevertoncrit =
    Возвращать ли исходную форму полиморфа, когда сущность переходит в критическое состояние (крит).
command-description-polymorph-setrevertondeath =
    Возвращать ли исходную форму полиморфа, когда сущность умирает.
command-description-polymorph-setrevertondelete =
    Возвращать ли исходную форму полиморфа, когда сущность удаляется.
command-description-polymorph-setrevertoneat =
    Возвращать ли исходную форму полиморфа, когда сущность съедают.
command-description-polymorph-setallowrepeats =
    Разрешать ли повторные полиморфизмы.
command-description-polymorph-setignoreallowrepeats =
    Позволяет полиморфизму произойти, даже если параметр AllowRepeatedMorphs равен true.
command-description-polymorph-setcooldown =
    Устанавливает время перезарядки в секундах перед тем, как сможет произойти следующий полиморфизм.
command-description-polymorph-setentersound =
    Устанавливает звук, воспроизводимый при переходе в полиморф.
command-description-polymorph-setexitsound =
    Устанавливает звук, воспроизводимый при выходе из полиморфа.
command-description-polymorph-clearentersound =
    Очищает звук, воспроизводимый при переходе в полиморф.
command-description-polymorph-clearexitsound =
    Очищает звук, воспроизводимый при выходе из полиморфа.
command-description-polymorph-setenterpopup =
    Устанавливает всплывающее сообщение (всплывашку), появляющееся при переходе в полиморф.
command-description-polymorph-setexitpopup =
    Устанавливает всплывающее сообщение (всплывашку), появляющееся при выходе из полиморфа.
command-description-polymorph-clearcopycomp =
    Очищает список компонентов, копируемых в полиморф.
command-description-polymorph-addcopycomp =
    Добавляет запись в список компонентов, копируемых в полиморф.
command-description-polymorph-rmcopycomp =
    Удаляет запись из списка компонентов, копируемых в полиморф.
command-description-polymorph-apply =
    Мгновенно применяет полиморфизм и завершает настройку.
command-description-polymorph-applyget =
    Мгновенно применяет полиморфизм и завершает настройку, возвращая новую сущность.
command-description-polymorph-addaction =
    Добавляет действие полиморфизма к сущности, используя текущую цепочку настройки. После этого вам, вероятно, следует вызвать polymorph:apply или polymorph:finish.
command-description-polymorph-addactionproto =
    Добавляет прототипированное действие полиморфизма к сущности.
command-description-polymorph-rmaction =
    Удаляет действие полиморфизма у сущности, которое было добавлено через polymorph:addaction.
command-description-polymorph-rmactionproto =
    Удаляет прототипированное действие полиморфизма у сущности.
command-description-polymorph-revert =
    Возвращает сущность к её предыдущей форме, если это возможно.
command-description-polymorph-reset =
    Сбрасывает полиморф сущности до её первоначального состояния.
command-description-polymorph-finish =
    Помечает текущую цепочку настройки полиморфа как завершённую, очищая данные и удаляя компонент.
command-description-vv-open =
    Открывает окно дебаг-панели ViewVariables (VV) для переданной сущности или пути.
command-description-vv-write =
    Изменяет значение пути с помощью VV. Для значения можно использовать переменную, но она должна быть сериализованной строкой.
command-description-vv-owrite =
    Изменяет значение пути с помощью VV. Для значения можно использовать необработанную (сырую) переменную.
command-description-vv-read =
    Выводит значение пути с помощью VV в консоль.
command-description-vv-rsave =
    Получает значение пути с помощью VV. Может быть сохранено в переменную.
command-description-vv-rsaveraw =
    Получает значение пути с помощью VV. Может быть сохранено в переменную. Сохраняет необработанное (сырое) значение вместо сериализованной строки.
command-description-mind-wipe =
    Стирает разум игрока или сущности. Обратите внимание, что игра станет неиграбельной для пользователя, пока вы не дадите ему новый разум.
command-description-mind-takeover =
    Напрямую захватывает тело моба, создавая разум, если он не существует, и заставляя сущность стать разумной.
command-description-mind-takeoverwipe =
    Стирает ваш собственный разум, а затем захватывает тело сущности. Это очистит все роли разума, цели и тому подобное.
command-description-mind-controlwipe =
    Стирает разум целевого игрока и заставляет его управлять переданной сущностью, создавая новый разум и делая сущность разумной.
command-description-killsign-set =
    Применяет метку убийства к сущности, используя указанное состояние.
command-description-killsign-list =
    Выводит список всех доступных меток убийства.
command-description-killsign-rm =
    Удаляет метку убийства у сущности.
command-description-fixinput =
    Обновляет контекст ввода для текущей сессии сущности.
command-description-faction-add =
    Добавляет фракцию к этой сущности.
command-description-faction-remove =
    Удаляет фракцию у этой сущности.
command-description-faction-aggro =
    Делает эту сущность агрессивной по отношению к целевой сущности.
command-description-faction-deaggro =
    Делает эту сущность больше не агрессивной по отношению к целевой сущности.
command-description-faction-ignore =
    Заставляет эту сущность и целевую сущность игнорировать друг друга.
command-description-faction-unignore =
    Заставляет эту сущность и целевую сущность больше не игнорировать друг друга.
command-description-faction-clear =
    Очищает фракции этой сущности.
command-description-npc-sethtn =
    Создаёт ИИ (NPC) на сущности и задаёт его структуру HTN-планирования.
command-description-npc-setenabled =
    Включает или отключает поведение HTN для этого ИИ (NPC).
command-description-stationinit-begin =
    Начинает процесс инициализации новой станции посреди раунда (midround). Добавляет BecomesStationMidRoundComponent к гриду.
command-description-stationinit-setid =
    Задаёт ID станции. Это необходимо для предотвращения дублирования.
command-description-stationinit-clearbaseprotos =
    Очищает список базовых прототипов станций.
command-description-stationinit-addbaseproto =
    Добавляет базовый прототип станции для использования.
command-description-stationinit-rmbaseproto =
    Удаляет базовый прототип станции из использования.
command-description-stationinit-setallowftl =
    Настраивает разрешение для кого угодно совершать FTL-прыжок на карту, где находится эта станция.
command-description-stationinit-setuseemergencyshuttle =
    Настраивает появление эвакуационного шаттла для использования в конце раунда.
command-description-stationinit-setusearmories =
    Настраивает появление оружейных комнат, которые можно отправить на станцию с помощью команды armory.
command-description-stationinit-setusearrivals =
    Настраивает появление шаттла прибытия для этой станции.
command-description-stationinit-setallowdungeonspawns =
    Настраивает разрешение на появление данжей, таких как VGroid.
command-description-stationinit-setallowcargo =
    Настраивает разрешение на появление грузовых шаттлов и терминала ATS.
command-description-stationinit-clearallowedgridspawns =
    Очищает список спавнов гридов, которым разрешено появляться из базовых прототипов.
command-description-stationinit-addallowedgridspawn =
    Добавляет спавн грида, которому разрешено появляться из базовых прототипов.
command-description-stationinit-rmallowedgridspawn =
    Удаляет спавн грида из разрешённых для появления из базовых прототипов.
command-description-stationinit-setemergencyshuttlepath =
    Задаёт переопределение пути, используемое для грида эвакуационного шаттла.
command-description-stationinit-clearjobs =
    Очищает все профессии для этой станции.
command-description-stationinit-addjob =
    Добавляет новую профессию для этой станции.
command-description-stationinit-rmjob =
    Удаляет профессию у этой станции.
command-description-stationinit-setallowevents =
    Настраивает разрешение для случайных событий нацеливаться на эту станцию.
command-description-stationinit-setdovariationpass =
    Настраивает разрешение на запуск этапа вариаций старта раунда на только что созданной станции.
command-description-stationinit-namegrid =
    Переименовывает целевой грид. Название грида — это то, что будет использоваться в качестве имени станции при инициализации.
command-description-stationinit-initialize =
    Завершает настройку и инициализирует станцию.
command-description-stationinit-initializeget =
    Завершает настройку и инициализирует станцию, возвращая только что созданную сущность станции.
command-description-aitakeover =
    Заставляет переданную сущность захватить целевое ядро ИИ.
command-description-mobthreshold-initialize =
    Корректно инициализирует новый порог здоровья моба (mob threshold) на сущности.
command-description-corporeal-on =
    Делает вашего призрака видимым и дарует ему способность говорить.
command-description-corporeal-off =
    Делает вашего призрака невидимым и отзывает способность говорить.
command-description-markup-adddesc =
    Добавляет размеченный текст (markup) к описанию переданной сущности с указанным ID.
command-description-markup-editdesc =
    Редактирует строку размеченного текста в описании переданной сущности по указанному ID.
command-description-markup-rmdesc =
    Удаляет строку размеченного текста из описания переданной сущности по указанному ID.
command-description-markup-cleardesc =
    Очищает все дополнительные строки размеченного текста из описания переданной сущности.
command-description-markup-listdesc =
    Выводит список всех размеченных текстов описания на переданной сущности и их ID.
