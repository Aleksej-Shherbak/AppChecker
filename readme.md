### ApChecker
#### (Application package checker) 
##### Проврщик доступности приложений

Данный инструмент создан для массовой проверки доступности мобильных приложений. 
Инструмент является комплексом из подсистем, реализующих параллельный анонимный 
парсинг сторов (App store и Play market). 

#### Оглавление

- Описание
- Установка и настройка
- Использование

 
 
##### Описание 
Представляет из себя WEB-API приложение, реализованное при помощи платформы 
.Net Core 3.1 и языка C#, выполняющее фоновые задачи парсинга 
(проверки доступности приложений). Клиент отправляет запрос на сервер согласно 
апи документации и сразу получает информационный ответ о состоянии очереди.
После выполнения проверки, сервис  запросет переданный клиентом URL для обратного
вызова, в котором будет содержаться результат проверки. При неудачном результате 
проверки, система делает 3 ретрая. Если результат снова неудачный, вернет статус 0
(см. использование).   
 
 
##### Установка и настройка
Устанавливается и запускается согласно официальной 
[инструкции](https://docs.microsoft.com/ru-ru/dotnet/core/get-started?tabs=linux)  

Настройка системы осуществляется в стандартных конфигурационных файла, согласно 
 [инструкции](https://docs.microsoft.com/ru-ru/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.2)
 майкрософт.

Система имеет следующие специфические настройки: 

```
  "WorkersCount": 2,
  "MaxAppIDsCount": 500,
  "Proxy": "111.111.0.111",
```

- WorkersCount - количество процессов, обрабатывающих задачи в очереде;
- Proxy - IP адрес прокси (или сервиса маршрутизации прокси ),
 через который будет осуществлятся парсинг. Данное поле можно оставить пустым
 тогда парсинг будет без прокси; 
- MaxAppIDsCount - максимальное количество передаваемых app id.


##### Использование

 
 Добавление задачи на парсинг:
 
 `/check`
 
 `POST` запрос со стороны клиента:
 
 ```
 {
 	"app_ids" : [
 		"ru.sberbankmobile"
 	], 
 	"callback_url": "http://lol.ru"
 }
 ```
 
 `app_ids` - идентификаторы IOS или Android приложений.
  `callback_url` - URL, который система запросит `POST` запросом после выполнения 
  проверки.
 
 Ответ системы:
 
 ```
 {
   "status": "Ok",
   "queue_size": 3,
   "possible_app_ids": 500,
   "given_number_app_ids": 437
 }
 ```
 , где 
 
 - `possible_app_ids` - максимальное количество app id,
 которое обрабатывает система. Это количество задается в настройках 
 (см раздел Установка и настройка);
 - `given_number_app_ids` - переданное количество app id
 
 Запрос со стороны системы к клиенту:
 
 `POST` запрос
 
 ```
 {
    "result" : [
        {
            "app_id" : "ru.sberbankmobile",
            "http_status" : 200
        },
        ...    
    ]
 }
 ```
 
 Поле `http_status` может принимать значения любого известного
 HTTP статус кода, также может иметь значение 0. Это означает, 
 что парсинг не удался даже после 3х попыток. 
 
