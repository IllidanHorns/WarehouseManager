# Настройка метрик Prometheus для Warehouse Manager API

## Описание

Реализована система кастомных метрик Prometheus, которые сохраняются в базе данных и не сбрасываются при перезапуске приложения.

## Созданные метрики

### 1. `warehouse_total_orders_created`
- **Тип**: Gauge
- **Описание**: Общее количество созданных заказов с момента запуска системы
- **Обновление**: При создании нового заказа через API
- **Хранение**: Значение сохраняется в БД в таблице `ApplicationMetrics`

### 2. `warehouse_active_users_count`
- **Тип**: Gauge
- **Описание**: Количество активных (не архивированных) пользователей в системе
- **Обновление**: 
  - При создании нового пользователя
  - При обновлении пользователя
  - При архивации пользователя
  - Автоматически каждые 5 минут
- **Хранение**: Значение сохраняется в БД в таблице `ApplicationMetrics`

### 3. `warehouse_total_products_quantity`
- **Тип**: Gauge
- **Описание**: Общее количество товаров на всех складах (сумма всех остатков)
- **Обновление**: 
  - При создании нового остатка на складе
  - При обновлении остатка на складе
  - При создании заказа (количество товаров уменьшается)
  - Автоматически каждые 5 минут
- **Хранение**: Значение сохраняется в БД в таблице `ApplicationMetrics`

## Архитектура

### Компоненты

1. **ApplicationMetric** (`WarehouseManger.Core/Models/ApplicationMetric.cs`)
   - Модель для хранения метрик в БД
   - Поля: Id, MetricName, Value, LastUpdated, Description

2. **MetricsService** (`WarehouseManager.Services/Services/MetricsService.cs`)
   - Сервис для работы с метриками в БД
   - Методы: GetMetricValueAsync, SetMetricValueAsync, IncrementMetricAsync

3. **CustomMetricsService** (`WarehouseManagerApi/Services/CustomMetricsService.cs`)
   - Сервис для синхронизации метрик Prometheus с БД
   - Инициализирует метрики при старте приложения
   - Обновляет метрики Prometheus при изменении данных

## Настройка

### 1. Миграция базы данных

Необходимо создать миграцию для новой таблицы `ApplicationMetrics`:

```bash
dotnet ef migrations add AddApplicationMetrics --project WarehouseManger.Core --startup-project WarehouseManagerApi
dotnet ef database update --project WarehouseManger.Core --startup-project WarehouseManagerApi
```

### 2. Prometheus

Метрики доступны на эндпоинте:
- **URL**: `http://localhost:9090/metrics`
- **Порт**: 9090 (настроен в `Program.cs`)

### 3. Grafana

Для визуализации метрик в Grafana:

1. Настройте Prometheus как источник данных в Grafana
2. Создайте дашборд с панелями для каждой метрики

#### Пример запросов для Grafana:

**Общее количество заказов:**
```
warehouse_total_orders_created
```

**Количество активных пользователей:**
```
warehouse_active_users_count
```

**Общее количество товаров:**
```
warehouse_total_products_quantity
```

## Интеграция

Метрики автоматически обновляются при:
- Создании заказа (`OrdersController.CreateOrder`)
- Создании/обновлении/архивации пользователя (`UsersController`)
- Создании/обновлении остатков на складе (`StocksController`)

Также метрики обновляются автоматически каждые 5 минут через таймер в `Program.cs`.

## Важные замечания

1. Метрики сохраняются в БД, поэтому они не сбрасываются при перезапуске приложения
2. При первом запуске значения метрик будут равны 0 или будут вычислены из текущего состояния БД
3. Метрика `warehouse_total_orders_created` накапливается со временем и не сбрасывается
4. Метрики `warehouse_active_users_count` и `warehouse_total_products_quantity` вычисляются из текущего состояния БД

