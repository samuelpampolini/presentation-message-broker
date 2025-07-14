# Message Broker Presentation - RabbitMQ Examples

Example of RabbitMQ and Queue Types using C# and NodeJS, exploring the different types of queues and how to consume messages from them.

## Table Of Contents

- [RabbitMQ](#rabbitmq)
- [Queue Types](#queue-types)
- [Project Structure](#project-structure)
- [Naming Conventions for Queues and Exchanges](#naming-conventions-for-queues-and-exchanges)
- [Quickstart](#quickstart)
- [Consumers](#consumers)
- [Troubleshooting](#troubleshooting)
- [License](#license)

## RabbitMQ

RabbitMQ is an open-source message broker software that facilitates communication between different parts of a system by sending messages between them. It is based on the Advanced Message Queuing Protocol (AMQP) and supports various messaging patterns, including point-to-point, publish/subscribe, and request/reply.

RabbitMQ is designed to be reliable, scalable, and flexible, making it suitable for a wide range of applications. It can be used for various purposes, such as decoupling microservices, implementing event-driven architectures, and handling background tasks.
[Official Documentation](https://www.rabbitmq.com/docs)

## Queue Types

Supported RabbitMQ queue types:

- **Fanout**: Broadcasts messages to all bound queues.
- **Direct**: Routes messages by routing key.
- **Topic**: Pattern-based routing.
- **Header**: Routing based on message headers.
- **Exchange to Exchange**: Demonstrates exchange chaining.
- **Alternate Exchange**: Handles messages that cannot be routed.

## Project Structure

- `src/NET/MessageBroker.Presentation.Publisher`: .NET publisher examples
- `src/NET/MessageBroker.Presentation.Consumer`: .NET consumer examples
- `src/NET/MessageBroker.Example.CrossCut`: Shared interfaces, attributes, and example selection logic
- `src/NodeJs/poison-consumer.js`: Node.js poison message consumer
- `images/net-console-options.png`: Example selection menu screenshot

## Naming Conventions for Queues and Exchanges

Queue and exchange names in this project are explicit and descriptive to make it easy to identify their purpose and origin. The conventions are:

- **Prefix**: Names start with `presentation-` to indicate they are part of this demo project.
- **Language/Component**: The next part (e.g., `node`, `net`) indicates the technology or consumer type.
- **Purpose/Pattern**: The name includes the pattern or role, such as `poison-message`, `dld` (dead letter), or the queue type.
- **Numbering**: If multiple related queues are used, a numeric suffix (e.g., `-1`) is added for clarity.

**Examples:**
- `presentation-node-poison-message-1`: Node.js poison message queue example.
- `presentation-node-poison-message-dld`: Dead letter queue for Node.js poison message example.
- `presentation-node-poison-exchange-dld`: Dead letter exchange for Node.js poison message example.

This convention helps quickly identify the queue/exchange's role, technology, and pattern when debugging or extending the project.

## Quickstart

1. **Start RabbitMQ:**

   ```bash
   docker-compose up -d
   ```

2. **Access RabbitMQ Management:**  
   [http://localhost:15672](http://localhost:15672) (user/pass: guest/guest)
3. **Run Publisher (.NET):**

   ```bash
   cd src/NET/MessageBroker.Presentation.Publisher
   dotnet run -project MessageBroker.Presentation.csproj
   ```

   Choose a queue type from the menu:
   - `1` Fanout
   - `2` Direct
   - `3` Topic
   - `4` Header
   - `5` Exchange to Exchange
   - `6` Alternate Exchange
4. **Run Consumer (.NET):**

   ```bash
   cd src/NET/MessageBroker.Presentation.Consumer
   dotnet run -project MessageBroker.Presentation.Consumer
   ```

   - Enter `1` for Simple Consumer, `2` for Poison Message Consumer.
5. **Run Poison Consumer (Node.js):**

   ```bash
   cd src/NodeJs
   npm run start
   ```

## Consumers

### Simple Consumer (.NET)

- Run:

  ```bash
  dotnet run -project MessageBroker.Presentation.Consumer
  ```

  Enter `1` at the menu to start the simple consumer.

### Poison Message Consumer (.NET)

- Run:

  ```bash
  dotnet run -project MessageBroker.Presentation.Consumer
  ```

  Enter `2` at the menu to start the poison message consumer (demonstrates Dead Letter Exchange).

### Poison Message Consumer (Node.js)

- Run:

  ```bash
  npm run start
  ```

## Troubleshooting

- **RabbitMQ not running:**
  Ensure Docker is running and use `docker-compose up -d`.
- **Connection issues:**
  Check RabbitMQ management at [http://localhost:15672](http://localhost:15672).
- **Menu not showing:**
  Make sure you are running the correct project and in the right directory.

## License

[MIT](https://choosealicense.com/licenses/mit/)