# System Design Decisions

## 1. Idempotency
To prevent duplicate notifications from being sent when an upstream service experiences a timeout and retries a request, the `NotificationRecord` entity requires an `IdempotencyKey`. A unique composite index is placed on `(IdempotencyKey, UserId)` in the PostgreSQL database. If a duplicate request arrives, the database will throw a unique constraint violation, preventing duplicates.

## 2. Decoupling and Resilience (Rebus & RabbitMQ)
We chose **Rebus** to abstract away AMQP complexity. Rebus provides:
- Automatic topology generation (Exchanges and Queues) based on C# classes.
- Built-in JSON serialization.
- Transparent retries and dead-letter queues (`_error` queues) for poisoned messages.

## 3. Rate Limiting
To prevent spamming end-users or exhausting 3rd-party provider API quotas, a **Redis-backed Fixed-Window Rate Limiter** is implemented in the Worker.
- **Key Format:** `rate_limit:{Channel}:{UserId}`
- **Current Rule:** Maximum 2 notifications per minute per user.
- If the limit is exceeded, the worker intercepts the message, drops it, and immediately updates the database status to `Failed` with the reason logged.

## 4. State Lifecycle & Error Handling
The system guarantees accurate tracking of every notification:
1. **Pending/Queued:** API accepts the request, creates a record, and queues it to RabbitMQ.
2. **Sent:** If the Worker successfully dispatches the message (e.g., SMTP 250 OK), it queries the database and updates the status to `Sent`, saving the upstream response.
3. **Failed:** The Worker intercepts all failures and logs them to the database `ProviderResponse` column, updating the status to `Failed`. This covers:
   - Rate Limit Exceeded
   - Unsupported Channels (e.g., SMS)
   - Downstream network exceptions (e.g., SMTP server unreachable)

## 5. Data Model
**Table: `Notifications`**
- `Id` (UUID, PK)
- `IdempotencyKey` (String, Unique Index)
- `UserId` (String)
- `Channel` (Enum: Email, SMS, Push)
- `Status` (Enum: Pending, Queued, Sent, Delivered, Failed)
- `Payload` (String)
- `ProviderResponse` (String, Nullable)
- `CreatedAt`, `UpdatedAt` (Timestamp)
