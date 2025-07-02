# Secure-Storage

Secure-Storage — это сервис, предоставляющий безопасное хранилище для данных с использованием **HashiCorp Vault** и **Elasticsearch** для логирования.

---

## Быстрый старт

### Требования

Для запуска Secure-Storage вам понадобятся:

* [Docker](https://www.docker.com/get-started)
* [Docker Compose](https://docs.docker.com/compose/install/)

### Запуск

Выполните следующие шаги для быстрого запуска сервиса:

1.  **Клонируйте репозиторий:**
    ```bash
    git clone https://git.kpfu.ru/KYSamsonov/secure-storage.git
    cd Secure-Storage
    ```
2.  **Запустите сервисы с помощью Docker Compose:**
    ```bash
    docker-compose -f docker/docker-compose.yml up -d
    ```
    Эта команда запустит приложение, **Vault**, **Elasticsearch** и **Kibana** в фоновом режиме.

---

## Настройка окружения в тестовом режиме

После запуска контейнеров необходимо настроить **Vault** и обновить конфигурацию приложения.

### Настройка HashiCorp Vault

1.  **Перейдите в веб-интерфейс Vault:** [http://localhost:8200](http://localhost:8200)

2.  **Инициализируйте Vault:**
    * При первом запуске вам будет предложено инициализировать Vault.
    * Укажите **1** для "Key Shares" и "Key Threshold".
    * **Обязательно сохраните** сгенерированные ключи для распечатывания (unseal keys) и токен администратора (Initial Root Token). Они понадобятся для дальнейшей настройки и доступа к Vault.

3.  **Войдите в Vault:**
    * Используйте "Unseal Key" для того, чтобы "распечатать" хранилище.
    * Войдите, используя "Initial Root Token".

4.  **Создайте Key-Value хранилище:**
    * Перейдите в раздел **"Secrets"**.
    * Нажмите **"Enable new engine"**.
    * Выберите **"KV"** (Key-Value) и нажмите **"Next"**.
    * Оставьте путь по умолчанию (`kv/`) или измените по вашему усмотрению.
    * Нажмите **"Enable Engine"**.

5.  **Создайте Transit Engine:**
    * Снова нажмите **"Enable new engine"**.
    * Выберите **"Transit"** и нажмите **"Next"**.
    * В поле "Path" укажите **`transit`**.
    * Нажмите **"Enable Engine"**.

### Настройка приложения

Рекомендуется использовать секреты или переменные окружения!
Вам необходимо обновить конфигурационный файл приложения `appsettings.json`, указав в нем токен для **Vault** и адрес **Elasticsearch**.

1.  **Откройте файл `SecureStorage/appsettings.json`:**

2.  **Обновите следующие поля:**
    * `Vault:AuthToken`: Вставьте ваш "Initial Root Token" от **Vault**.
    * `ElasticConfiguration:Uri`: Убедитесь, что адрес **Elasticsearch** указан верно (`http://localhost:9200`).

    Пример `appsettings.Development.json`:

    ```json
    {
      ...
      "Vault": {
        "Address": "http://localhost:8200",
        "AuthToken": "YOUR_VAULT_ROOT_TOKEN"
      },
      "ElasticConfiguration": {
        "Uri": "http://localhost:9200"
      }
      ...
    }
    ```

### Настройка Kibana

1.  **Откройте Kibana:** [http://localhost:5601](http://localhost:5601)

2.  **Настройте Kibana для отображения логов:**
    * Перейдите в раздел **Management** -> **Stack Management**.
    * Выберите **Index Patterns** и нажмите **"Create index pattern"**.
    * Введите `securestorage.api-logs-*` в качестве шаблона индекса (index pattern). Kibana должна автоматически обнаружить логи от вашего сервиса.
    * Следуйте инструкциям на экране, чтобы завершить создание шаблона индекса.

3.  **Просмотр логов:**
    * Перейдите в раздел **Discover**.
    * Теперь вы можете просматривать и анализировать логи вашего приложения, отправленные в Elasticsearch.