# Distributed Notification Service

> **Project Purpose:** This repository is a hands-on exercise for exploring, practicing, and evaluating various software architecture designs, clean code principles, and scalable microservice patterns.

A scalable, containerized microservice for centralizing user notifications (Email, SMS, Push) across a system. 
It ensures reliable delivery, decoupling via message queues, and protects against spam via rate limiting.

## Features
- **Async Processing:** API accepts requests and offloads delivery to background workers via RabbitMQ.
- **State Lifecycle:** The Worker accurately tracks `Sent` and `Failed` edge cases back to the PostgreSQL database.
- **Idempotency:** Protects against duplicate requests from upstream services.
- **Rate Limiting:** Redis-backed fixed-window mechanism to prevent user spam.
- **Resilience:** Built on Rebus for automatic retries and dead-letter queues.
- **Fully Containerized:** One command setup using Docker Compose.

## Project Structure
- `NotificationService.Api`: Fast ingestion REST API.
- `NotificationService.Worker`: Background consumer processing the queue.
- `NotificationService.Core`: Domain models, interfaces, and Enums.
- `NotificationService.Infrastructure`: EF Core DbContext, Redis, and SMTP implementations.

## Getting Started

### Prerequisites
- Docker and Docker Compose

### Running Locally
1. Clone the repository and navigate to the project root.
2. Run Docker Compose:
   ```bash
   docker compose up -d --build
   ```
3. The following services will spin up:
   - **API:** `http://localhost:8080`
   - **Worker:** (Background processor)
   - **RabbitMQ:** `amqp://localhost:5672`
   - **Redis:** `localhost:6379`
   - **Postgres:** `localhost:5432`
   - **Mailpit (Local Email Testing):** Web UI at `http://localhost:8025`

### Testing the Service
Send a POST request to the API:

```bash
curl -X POST http://localhost:8080/v1/notifications \
-H "Content-Type: application/json" \
-d '{
    "idempotencyKey": "test-123",
    "userId": "usr_999",
    "channel": "Email",
    "payload": "Welcome to our system!"
}'
```

Check the [Mailpit Web UI](http://localhost:8025) to see the delivered email.

## Documentation
- [Architecture Details](docs/ARCHITECTURE.md)
- [Design Decisions](docs/DESIGN.md)
