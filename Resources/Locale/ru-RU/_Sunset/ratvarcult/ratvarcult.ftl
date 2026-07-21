# 🌇Sunset🌇 - культ Ратвара (заводной культ)

ratvar-cult-title = Культ Ратвара
ratvar-cult-description = Среди экипажа скрываются культисты Ратвара — заводного бога. Латунь, шестерни и жертвы во славу Энгине.

roles-antag-ratvar-cultist-name = Культист Ратвара
roles-antag-ratvar-cultist-objective = Служите Ратвару: принесите жертву во славу заводного бога и переживите смену.
role-subtype-ratvar-cultist = Культист Ратвара

ratvar-cultist-role-greeting =
    Вы — культист Ратвара, заводного бога. Экипаж этой станции погряз в ереси Нар'Си и корпоративной скверне.
    В вашем рюкзаке — латунное снаряжение культа: облачитесь, когда придёт время действовать.
    Принесите жертву во славу Энгине и переживите смену. Ковка не останавливается. Тик-так.

ratvar-cult-round-end-agent-name = культист Ратвара

objective-issuer-ratvar = [color=#BE8700]Ратвар[/color]

objective-condition-ratvar-sacrifice-title = Принести в жертву { $targetName }, { $job }
objective-condition-ratvar-sacrifice-description = Ратвар требует жертвы. Эта душа должна быть скормлена шестерням — убейте цель и не дайте ей покинуть станцию.
objective-condition-ratvar-survive-title = Пережить смену
objective-condition-ratvar-survive-description = Мёртвый культист бесполезен для Ковки. Останьтесь в живых любой ценой.

ent-RatvarCultSurviveObjective = { objective-condition-ratvar-survive-title }
    .desc = { objective-condition-ratvar-survive-description }
ent-MindRoleRatvarCultist = роль культиста Ратвара
