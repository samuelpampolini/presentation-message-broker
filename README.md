# Message Broker Presentation - RabbitMQ Examples
Example of RabbitMQ and Queue Types using C# and NodeJS, exploring the different types of queues and how to consume messages from them.

## Table Of Contents

- [RabbitMQ](#rabbitmq)
- [Queue Types](#queue-types)

## RabbitMQ

RabbitMQ is an open-source message broker software that facilitates communication between different parts of a system by sending messages between them. It is based on the Advanced Message Queuing Protocol (AMQP) and supports various messaging patterns, including point-to-point, publish/subscribe, and request/reply.

RabbitMQ is designed to be reliable, scalable, and flexible, making it suitable for a wide range of applications. It can be used for various purposes, such as decoupling microservices, implementing event-driven architectures, and handling background tasks.
[Official Documentation](https://www.rabbitmq.com/docs)

## Setup of the Examples

### Prerequisites

- Docker
- .NET 8
- Node.js 22

### RabbitMQ

1. Start the RabbitMQ container using Docker Compose:

```bash
docker-compose up -d
```

2. Access the RabbitMQ management interface by navigating to `http://localhost:15672` in your web browser.
The default username and password are both `guest`.

3. Run the .NET example and choose one of the options presented at the console

## Run examples

### Types of Queues
This section will cover the different types of queues available in RabbitMQ, including:

- Fanout
- Direct
- Topic
- Header
- Exchange to Exchange

For this examples we are going to use .NET.
To execute the application navigate to "src\NET\MessageBroker.Presentation.Publisher" and run the command bellow:

```bash
dotnet run -project MessageBroker.Presentation.csproj
```

You're going to be presented with a menu to choose the type of queue you want to test.
![Menu](/images/net-console-options.png)

### Consume Messages
This section will cover how to consume messages from RabbitMQ using different programming languages, including:

- C#
- Node.js

We are going to cover simple consumers and poison message consumers.

#### Simple Consumers
1. C# Simple Consumer - execute the command below to run the application and enter 1 to start the simple consumer

```bash
dotnet run -project MessageBroker.Presentation.Consumer
```

2. Start the Node.js Simple Consumer

```bash
npm run consumer-simple
```

3. Run the Node.JS producer script to check the outputs

```bash
npm run producer-simple
```

#### Poison Message Consumers
To properly see how a Dead Letter Exchange (DLX) can be applied run the following commands:

1. C# Poison Message Consumer - execute the command below to run the application and enter 2 to start the poison message consumer
```bash
dotnet run -project MessageBroker.Presentation.Consumer
```

2. Start the Node.js Poison Message Consumer

```bash
npm run consumer-poison
```

3. Run the Node.JS producer script to check the outputs

```bash
npm run producer-poison
```

## License

[MIT](https://choosealicense.com/licenses/mit/)