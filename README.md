# Payment Processing Platform — .NET + Kafka + RabbitMQ + DynamoDB

# Overview

This project is a distributed payment processing platform built to study and understand modern backend and cloud-native technologies using a real-world architecture.

The main goal of this project is learning:

* Apache Kafka
* RabbitMQ
* DynamoDB
* Clean Architecture
* Background Workers
* Event-Driven Architecture
* API Gateway concepts
* Distributed systems
* Microservices communication
* AWS-like infrastructure locally
* Dockerized infrastructure
* OpenAPI / Swagger

This project simulates a payment processing system where:

1. A payment request is received via API
2. Payment is persisted into DynamoDB
3. An event is published into Kafka
4. A worker processes the payment asynchronously
5. Another event is published into Kafka
6. Notification service consumes the event
7. RabbitMQ distributes notification messages
8. Notification processor simulates email/SMS sending

---

# Technologies

| Technology            | Purpose                      |
| --------------------- | ---------------------------- |
| .NET 9 / ASP.NET Core | Backend APIs and Workers     |
| Apache Kafka          | Event streaming              |
| RabbitMQ              | Notification queue           |
| DynamoDB Local        | NoSQL database               |
| Docker Compose        | Infrastructure orchestration |
| Kafdrop               | Kafka UI                     |
| Swagger / OpenAPI     | API testing/documentation    |
| AWS SDK for .NET      | DynamoDB integration         |
| BackgroundService     | Async processing workers     |

---

# Architecture

## High-Level Flow

```text
Client
   ↓
Payment.API
   ↓
DynamoDB
   ↓
Kafka Topic: payments-created
   ↓
Payment.Worker
   ↓
Kafka Topic: payments-completed
   ↓
Notification.Worker
   ↓
RabbitMQ Queue
   ↓
Notification Processor
   ↓
Email/SMS Simulation
```

---

# Clean Architecture Structure

```text
src
│
├── BuildingBlocks
│
├── Infrastructure
│   └── Docker
│       └── docker-compose.yml
│
├── Services
│   │
│   ├── Payment
│   │   │
│   │   ├── Payment.API
│   │   ├── Payment.Application
│   │   ├── Payment.Domain
│   │   ├── Payment.Infrastructure
│   │   └── Payment.Worker
│   │
│   └── Notification
│       │
│       ├── Notification.Infrastructure
│       └── Notification.Worker
```

---

# Project Responsibilities

## Payment.API

### Type

ASP.NET Core Web API

### Responsibilities

* HTTP entrypoint
* Swagger/OpenAPI
* Request validation
* CorrelationId middleware
* Trigger application use cases

### Main Features

* POST /api/payments
* OpenAPI documentation
* Request handling

---

## Payment.Application

### Type

Class Library

### Responsibilities

* Use cases
* Commands
* DTOs
* Application orchestration
* Business flow coordination

### Main Components

* CreatePaymentHandler
* CreatePaymentCommand
* Application dependency injection

---

## Payment.Domain

### Type

Class Library

### Responsibilities

* Core business rules
* Entities
* Domain events
* Interfaces/contracts

### Main Components

* Payment entity
* PaymentCreatedEvent
* IPaymentRepository
* IEventBus

---

## Payment.Infrastructure

### Type

Class Library

### Responsibilities

* DynamoDB integration
* Kafka implementation
* Repository implementation
* EventBus implementation
* Infrastructure dependency injection

### Main Components

* DynamoDbPaymentRepository
* KafkaEventBus
* PaymentEntity
* AddInfrastructure()

---

## Payment.Worker

### Type

Worker Service

### Responsibilities

* Consume Kafka events
* Process payments asynchronously
* Update payment status
* Publish completion events

### Main Components

* KafkaConsumerWorker

---

## Notification.Infrastructure

### Type

Class Library

### Responsibilities

* RabbitMQ integration
* Notification publisher
* RabbitMQ dependency injection

### Main Components

* RabbitPublisher
* IRabbitPublisher
* AddRabbitMq()

---

## Notification.Worker

### Type

Worker Service

### Responsibilities

* Consume Kafka payment-completed events
* Publish RabbitMQ notifications
* Consume RabbitMQ queue
* Simulate email/SMS sending

### Main Components

* KafkaToRabbitMQWorker
* RabbitMQNotificationProcessorWorker

---

# Event-Driven Architecture

## Why Kafka?

Kafka is used as the central event streaming platform.

Benefits:

* Asynchronous communication
* Scalability
* Loose coupling
* Replayability
* Distributed processing
* Event sourcing style architecture

---

# Kafka Topics

| Topic              | Purpose                   |
| ------------------ | ------------------------- |
| payments-created   | Payment creation events   |
| payments-completed | Payment completion events |

---

# Why RabbitMQ?

RabbitMQ is used specifically for notification processing.

Reasons:

* Queue-oriented messaging
* Simpler worker distribution
* Retry support
* Notification fanout patterns
* Easier email/SMS pipelines

---

# Why DynamoDB?

DynamoDB was chosen to study:

* NoSQL modeling
* Partition keys
* High scalability
* AWS ecosystem integration
* Document-oriented persistence

---

# DynamoDB Table

## Payments

Partition Key:

```text
Id
```

---

# API Flow

## POST /api/payments

### Request

```json
{
  "amount": 100,
  "currency": "USD",
  "customerEmail": "test@test.com"
}
```

---

## Internal Flow

```text
Payment.API
    ↓
CreatePaymentHandler
    ↓
Domain.Payment.Create()
    ↓
DynamoDbPaymentRepository.SaveAsync()
    ↓
KafkaEventBus.PublishAsync()
    ↓
payments-created topic
```

---

# Payment Processing Flow

## Payment.Worker

```text
Consume payments-created
    ↓
Load payment from DynamoDB
    ↓
Payment.Complete()
    ↓
Save updated payment
    ↓
Publish payments-completed
```

---

# Notification Flow

```text
Consume payments-completed
    ↓
Publish RabbitMQ queue
    ↓
Notification Processor
    ↓
Simulate Email/SMS sending
```

---

# Dependency Injection Lifetimes

| Lifetime      | Usage                                            |
| ------------- | ------------------------------------------------ |
| Singleton     | Kafka producer, AWS clients, RabbitMQ connection |
| Scoped        | Repositories, business services                  |
| HostedService | Workers/background processing                    |

---

# Kafka Networking

This project uses dual Kafka listeners:

| Listener        | Purpose                       |
| --------------- | ----------------------------- |
| kafka:9092      | Internal Docker communication |
| localhost:29092 | External .NET applications    |

---

# Running Infrastructure

## Start Docker Infrastructure

```bash
docker compose up -d
```

---

# Infrastructure Services

| Service            | URL                                              |
| ------------------ | ------------------------------------------------ |
| Kafka UI (Kafdrop) | [http://localhost:9000](http://localhost:9000)   |
| RabbitMQ UI        | [http://localhost:15672](http://localhost:15672) |
| DynamoDB           | [http://localhost:8000](http://localhost:8000)   |

---

# RabbitMQ Credentials

```text
Username: guest
Password: guest
```

---

# Create DynamoDB Table

```bash
aws dynamodb create-table \
  --table-name Payments \
  --attribute-definitions \
      AttributeName=Id,AttributeType=S \
  --key-schema \
      AttributeName=Id,KeyType=HASH \
  --billing-mode PAY_PER_REQUEST \
  --endpoint-url http://localhost:8000
```

---

# Running the Application

## 1. Run Payment.API

```bash
cd Services/Payment/Payment.API

dotnet run
```

---

## 2. Run Payment.Worker

```bash
cd Services/Payment/Payment.Worker

dotnet run
```

---

## 3. Run Notification.Worker

```bash
cd Services/Notification/Notification.Worker

dotnet run
```

---

# Open Swagger

```text
https://localhost:<port>/swagger
```

---

# Testing the API

## Endpoint

```http
POST /api/payments
```

## Request Body

```json
{
  "amount": 100,
  "currency": "USD",
  "customerEmail": "test@test.com"
}
```

---

# Verifying Kafka

Open:

```text
http://localhost:9000
```

Topics should appear automatically:

* payments-created
* payments-completed

---

# Verifying RabbitMQ

Open:

```text
http://localhost:15672
```

Check queues section.

---

# Verifying DynamoDB Data

```bash
aws dynamodb scan \
  --table-name Payments \
  --endpoint-url http://localhost:8000
```

---

# Important Learning Concepts

## Kafka

* Producers
* Consumers
* Topics
* Partitions
* Consumer groups
* Event streaming
* Advertised listeners
* KRaft mode

---

## RabbitMQ

* Queues
* Channels
* Connections
* Publishers
* Consumers
* Async processing

---

## DynamoDB

* Partition keys
* NoSQL modeling
* AWS SDK
* PutItem
* Scan
* Query

---

## Clean Architecture

* Layer separation
* Dependency inversion
* Infrastructure isolation
* Domain-centric design

---

# Future Improvements

This project can be extended with:

* Outbox Pattern
* Saga Pattern
* Prometheus
* Grafana
* Seq
* OpenTelemetry
* API Gateway
* AWS Lambda
* AWS SES
* AWS SNS
* Kubernetes
* Terraform
* CI/CD Pipelines
* Dead Letter Queues
* Retry Policies
* Circuit Breakers
* Distributed Tracing

---

# Key Takeaways

This project demonstrates:

* Real-world distributed architecture
* Event-driven communication
* Async processing pipelines
* Microservice-ready design
* Modern backend architecture patterns
* Kafka + RabbitMQ integration
* DynamoDB integration with .NET
* Worker-based processing
* Clean Architecture implementation

---

# Final Notes

This project is intentionally designed for learning advanced backend and distributed systems concepts using practical implementations.

It focuses on understanding:

* How asynchronous systems communicate
* How event-driven architectures work
* How distributed processing behaves
* How modern cloud-native systems are structured
* How messaging systems integrate together

The architecture can later evolve into fully separated microservices, Kubernetes deployments, and cloud-native production environments.
