## Secure Command Terminal – UI strings

secure-terminal-window-title = Защищённый терминал
secure-terminal-requests-header = Запросы
secure-terminal-information-header = Информация
secure-terminal-authorization-header = Авторизация

secure-terminal-select-request = Выберите запрос из списка слева, чтобы увидеть подробности.

secure-terminal-request-button = Запросить
secure-terminal-request-button-confirm = Подтвердить?
secure-terminal-authorize-button = Авторизовать
secure-terminal-deny-button = Отклонить / Отменить
secure-terminal-recall-button = Отозвать арсенал
secure-terminal-recall-locked = { $minutes ->
    [1] Отзыв будет доступен через 1 минуту.
    *[other] Отзыв будет доступен через { $minutes } мин.
}
secure-terminal-used-note = Этот арсенал уже был окончательно активирован или отозван в этом раунде и не может быть развёрнут повторно.
secure-terminal-already-used = Этот ресурс уже был использован в этом раунде и не может быть запрошен снова.

secure-terminal-auth-waiting = Активного предложения по этому запросу нет.
secure-terminal-auth-desc = Текущее предложение — нет ответа = [color=red]красный[/color], согласие = [color=green]зелёный[/color]:
secure-terminal-awaiting-member = Ожидание { $label }

secure-terminal-pending-countdown-label = Истекает через { $minutes } мин { $seconds } сек…
secure-terminal-countdown-label = Активация через { $minutes } мин { $seconds } сек…

secure-terminal-fee-note = Комиссия за обработку: { $fee }
secure-terminal-salary-note = Зарплата станции снижена на { $penalty }% из-за расходов на мобилизацию.
secure-terminal-delay-note = { $minutes ->
    [1] Ориентировочное время прибытия: 1 минута после авторизации.
    *[other] Ориентировочное время прибытия: { $minutes } мин после авторизации.
}

secure-terminal-requires-no-war-note = Отключено во время военных действий.
secure-terminal-requires-war-note = Доступно только во время военных действий.
secure-terminal-requires-alert-note = Требуется активный уровень тревоги: { $level }.
secure-terminal-alert-time-remaining = { $minutes ->
    [1] Уровень тревоги должен действовать ещё минимум 1 минуту, прежде чем это можно будет запросить.
    *[other] Уровень тревоги должен действовать ещё минимум { $minutes } мин, прежде чем это можно будет запросить.
}
secure-terminal-on-cooldown-note = { $minutes ->
    [1] На перезарядке — доступно через 1 минуту.
    *[other] На перезарядке — доступно через { $minutes } мин.
}
secure-terminal-requires-alert-suffix = Требуется: { $level }
secure-terminal-requires-war-suffix = Требуется: военные действия

secure-terminal-reason = Укажите причину запроса:

## Server → global announcements

secure-terminal-proposal-created = Запрос «{ $request }» отправлен и ожидает совместной авторизации.
secure-terminal-proposal-created-reason = Запрос «{ $request }» отправлен и ожидает совместной авторизации. Причина: { $reason }
secure-terminal-proposal-denied = Запрос «{ $request }» отменён.
secure-terminal-proposal-denied-cc = Запрос «{ $request }» отклонён Центральным Командованием.
secure-terminal-radio-proposal = Выдвинуто предложение «{ $request }». Пройдите к ближайшему устройству аутентификации по карте-ключу, чтобы авторизовать или отклонить его.
secure-terminal-radio-proposal-reason = Выдвинуто предложение «{ $request }». Пройдите к ближайшему устройству аутентификации по карте-ключу, чтобы авторизовать или отклонить его. Причина: { $reason }
secure-terminal-radio-denied = Запрос «{ $request }» отменён.
secure-terminal-activation-countdown = Запрос «{ $request }» полностью авторизован.
    Активация через { $minutes } мин.
    Зарплата станции снижена из-за расходов на мобилизацию.
secure-terminal-unknown-job = Неизвестно

## Popup messages

secure-terminal-no-station = Для этой консоли не найдена станция.
secure-terminal-request-denied = Доступ запрещён.
secure-terminal-authorize-denied = У вас недостаточно допуска, чтобы совместно подписать этот запрос.
secure-terminal-requires-war = Этот запрос доступен только при официально объявленных военных действиях.
secure-terminal-wrong-alert = Текущий уровень тревоги не соответствует требованиям этого запроса.
secure-terminal-alert-not-long-enough = Уровень тревоги не действовал достаточно долго для авторизации этого запроса. Подождите и попробуйте снова.
secure-terminal-recall-too-soon = Арсенал не был развёрнут достаточно долго для отзыва. Подождите.
secure-terminal-on-cooldown = Этот запрос находится на перезарядке.
secure-terminal-already-pending = Предложение по этому запросу уже ожидает рассмотрения.
secure-terminal-already-active = Другой запрос уже ожидает рассмотрения или активируется. Дождитесь его завершения, прежде чем создавать новый.
secure-terminal-no-active-proposal = Активное предложение по этому запросу не найдено.
secure-terminal-already-authorized = Вы уже авторизовали это предложение.
secure-terminal-already-activated = Этот терминал уже авторизовал это предложение.
secure-terminal-auth-note = Этот терминал предназначен только для авторизации.
secure-terminal-authorized-by = Внимание — запрос «{ $request }» авторизован. Авторизовали: { $signatories }.
secure-terminal-armory-recalled = Отдан приказ об отзыве «{ $request }». Развёртывание арсенала отменено.
secure-terminal-awaiting-admin = Внимание — запрос «{ $request }» отправлен. Ожидается авторизация Центральным Командованием.
secure-terminal-admin = Запрос одобрения администрации на: { $request }
                        Причина: { $reason }
                        Используйте АПризрака, чтобы одобрить или отклонить запрос.

## Request names & descriptions

secure-terminal-warops-security-name = Группа ядерного реагирования
secure-terminal-warops-security-desc = Развёртывает отряд СБР службы безопасности, специализированный для военных действий. Доступно только во время военных действий.
                                       Используйте, когда станция подвергается прямому вооружённому нападению во время объявленных военных действий.
secure-terminal-warops-security-announcement = Группа быстрого реагирования — отряд службы безопасности — авторизована и направляется к станции. Расчётное время прибытия: 30 минут.

secure-terminal-ert-security-name = СБР Служба безопасности
secure-terminal-ert-security-desc = Развёртывает отряд СБР службы безопасности.
secure-terminal-ert-security-announcement = Группа быстрого реагирования — отряд службы безопасности — авторизована и направляется к станции. Расчётное время прибытия: 10 минут.

secure-terminal-ert-engineering-name = СБР Инженерный отдел
secure-terminal-ert-engineering-desc = Развёртывает отряд СБР инженерного отдела для помощи с критически важной инфраструктурой станции.
    Рекомендуется при катастрофических повреждениях конструкций, атмосферы или энергосистемы, которые невозможно устранить силами станции.
secure-terminal-ert-engineering-announcement = Группа быстрого реагирования — инженерный отряд — авторизована и направляется к станции. Расчётное время прибытия: 10 минут.

secure-terminal-ert-medical-name = СБР Медицинский отдел
secure-terminal-ert-medical-desc = Развёртывает медицинский отряд СБР для массовой сортировки пострадавших и экстренных операций.
    Рекомендуется, когда медицинский отдел станции перегружен, недееспособен или уничтожен.
secure-terminal-ert-medical-announcement = Группа быстрого реагирования — медицинский отряд — авторизована и направляется к станции. Расчётное время прибытия: 10 минут.

secure-terminal-ert-janitorial-name = СБР Хозяйственный отдел
secure-terminal-ert-janitorial-desc = Развёртывает хозяйственный отряд СБР для устранения опасных загрязнений и восстановления станции.
    Рекомендуется после масштабного биологического, химического или экологического заражения, требующего быстрой деконтаминации.
secure-terminal-ert-janitorial-announcement = Группа быстрого реагирования — хозяйственный отряд — авторизована и направляется к станции. Расчётное время прибытия: 10 минут.

secure-terminal-ert-chaplain-name = СБР Капеллан
secure-terminal-ert-chaplain-desc = Направляет капеллана СБР для поддержки морального духа экипажа и проведения последних обрядов.
    Обеспечивает духовную поддержку и поддерживает боевой дух экипажа в затяжных чрезвычайных ситуациях.
secure-terminal-ert-chaplain-announcement = Группа быстрого реагирования — капелланская служба — авторизована и направляется к станции. Расчётное время прибытия: 10 минут.

secure-terminal-ert-cburn-name = СБР CBURN
secure-terminal-ert-cburn-desc = Развёртывает отряд СБР CBURN.
secure-terminal-ert-cburn-announcement = Группа быстрого реагирования — отряд CBURN — авторизована и направляется к станции. Расчётное время прибытия: 15 минут.

secure-terminal-code-gamma-name = Код ГАММА
secure-terminal-code-gamma-desc = Повышает уровень тревоги станции до [color=palevioletred]ГАММА[/color]. Военное положение — все гражданские должны сопровождаться службой безопасности в безопасные зоны.
    Служба безопасности должна быть вооружена постоянно. Все гражданские обязаны явиться к ближайшему главе отдела и быть сопровождены в безопасное место. Активируется аварийное освещение.
secure-terminal-code-gamma-announcement = Внимание! Код ГАММА вступит в силу в ближайшее время. Будет введено военное положение. Весь экипаж — немедленно явиться к ближайшему главе отдела.

secure-terminal-end-gamma-name = Снять тревогу ГАММА
secure-terminal-end-gamma-desc = Снимает тревогу [color=palevioletred]ГАММА[/color] и возвращает станцию к зелёному уровню. Требуется, чтобы ГАММА была активна не менее 15 минут.
secure-terminal-end-gamma-announcement = Код ГАММА снимается. Станция возвращается к нормальному режиму работы. Сохраняйте бдительность и ожидайте дальнейших указаний от главы вашего отдела.

secure-terminal-code-psi-name = Код ПСИ
secure-terminal-code-psi-desc = Повышает уровень тревоги станции до [color=mediumpurple]ПСИ[/color]. Обнаружены враждебные синтетические юниты — избегайте нестандартных киборгов и обращайтесь к командному составу.
    Указывает на враждебную или нестандартную активность киборгов. Весь экипаж должен избегать неизвестных боргов, держаться группами и обращаться за указаниями к главам отделов.
secure-terminal-code-psi-announcement = Внимание! Командование санкционировало Код ПСИ. Силиконовые юниты не NanoTrasen признаны активной угрозой. Весь экипаж — явиться к ближайшему главе отдела.

secure-terminal-end-psi-name = Снять тревогу ПСИ
secure-terminal-end-psi-desc = Снимает тревогу [color=mediumpurple]ПСИ[/color] и возвращает станцию к зелёному уровню. Требуется, чтобы ПСИ была активна не менее 15 минут.
secure-terminal-end-psi-announcement = Код ПСИ снимается. Выявленная синтетическая угроза нейтрализована. Станция возвращается к нормальному режиму работы.

secure-terminal-armory-gamma-name = Гамма-арсенал
secure-terminal-armory-gamma-desc = Отправляет [color=palevioletred]Гамма-арсенал[/color] — тяжёлое вооружение для ситуаций уровня ГАММА. Одноразовое развёртывание.
                                    Выдаёт тяжёлое снаряжение службы безопасности уполномоченному персоналу.
secure-terminal-armory-gamma-announcement = Гамма-арсенал авторизован и направляется к станции.

secure-terminal-armory-psi-name = Пси-арсенал
secure-terminal-armory-psi-desc = Отправляет [color=mediumpurple]Пси-арсенал[/color] — противокибернетическое вооружение для ситуаций уровня ПСИ. Одноразовое развёртывание.
                                  Предоставляет средства, необходимые для нейтрализации нестандартных силиконовых юнитов.
secure-terminal-armory-psi-announcement = Пси-арсенал авторизован и направляется к станции.

secure-terminal-med-pod-name = Экстренная медицинская капсула
secure-terminal-med-pod-desc = Отправляет экстренную медицинскую капсулу — быстро развёртываемый пункт сортировки с хирургическим и реанимационным оборудованием.
    Используйте, когда число пострадавших превышает возможности медицинского отдела станции.
secure-terminal-med-pod-announcement = Экстренная медицинская капсула авторизована и направляется к станции. Расчётное время прибытия: 5 минут.

secure-terminal-nukerequest-name = Код самоуничтожения
secure-terminal-nukerequest-desc = Запросить коды ядерного самоуничтожения.
                                   Злоупотребление системой ядерного запроса не допускается ни при каких обстоятельствах.
                                   Отправка запроса не гарантирует ответа.

secure-terminal-code-violet-name = Код Фиолетовый
secure-terminal-code-violet-desc = Повышает уровень тревоги станции до [color=Violet]Фиолетового[/color].

secure-terminal-end-violet-name = Снять тревогу Фиолетовый
secure-terminal-end-violet-desc = Снимает [color=Violet]Фиолетовую[/color] тревогу и возвращает станцию к зелёному уровню. Требуется, чтобы Фиолетовый уровень был активен не менее 10 минут.

secure-terminal-emergency-maintenance-name = Экстренный доступ к техническим помещениям
secure-terminal-emergency-maintenance-desc = Предоставить экстренный доступ к техническим помещениям.
secure-terminal-emergency-maintenance-announcement = Ограничения доступа к техническим и внешним шлюзам сняты.

secure-terminal-end-emergency-maintenance-name = Отозвать экстренный доступ к техническим помещениям
secure-terminal-end-emergency-maintenance-desc = Отозвать экстренный доступ к техническим помещениям.
secure-terminal-end-emergency-maintenance-announcement = Ограничения доступа к техническим и внешним шлюзам восстановлены.

secure-terminal-emergency-station-name = Общестанционный экстренный доступ
secure-terminal-emergency-station-desc = Активировать общестанционный экстренный доступ.
secure-terminal-emergency-station-announcement = Ограничения доступа ко всем шлюзам станции сняты из-за продолжающегося кризиса. Законы о нарушении границ по-прежнему действуют, если командный состав не распорядился иначе.

secure-terminal-end-emergency-station-name = Отключить общестанционный экстренный доступ
secure-terminal-end-emergency-station-desc = Отключить общестанционный экстренный доступ.
secure-terminal-end-emergency-station-announcement = Ограничения доступа ко всем шлюзам станции восстановлены. Если вы застряли, обратитесь за помощью к ИИ станции или коллеге.
