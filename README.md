# Message Broker Presentation - RabbitMQ Examples

This project demonstrates RabbitMQ queue types and messaging patterns using C# (.NET 8), Node.js, and a modern React frontend. It provides:

- A unified .NET Console application to run all publisher and consumer examples
- A shared library for example logic and input/output abstraction
- A Node.js poison message consumer for interoperability
- **A React + Vite frontend for stepwise, stateless API interaction**

**REST API included:** A .NET 8 Web API project exposes all example operations and available steps via FastEndpoints and Swagger UI. (See below.)

---

## Features

- Unified .NET 8 Console app for all RabbitMQ publisher and consumer examples
- .NET 8 REST API with FastEndpoints and Swagger UI for stateless, stepwise example execution
- Modern React + Vite frontend for interactive API usage and history
- Clean separation of shared logic and example implementations (CrossCut)
- Node.js poison message consumer for cross-platform demonstration
- Secure password management using .NET user-secrets

## Project Structure

- `src/NET/MessageBroker.Presentation.Console`: .NET 8 console application to run all RabbitMQ examples
- `src/NET/MessageBroker.Presentation.Api`: .NET 8 REST API with FastEndpoints and Swagger UI
- `src/NET/messagebroker-presentation-frontend`: React + Vite frontend for API interaction
- `src/NET/MessageBroker.Example.CrossCut`: Shared interfaces, attributes, factories, and example selection logic
- `src/NodeJs/PoisonConsumer.js`: Node.js poison message consumer
- `images/net-console-options.png`: Example selection menu screenshot

---

## Quickstart

### 1. Start RabbitMQ

```bash
docker-compose up -d
```

Access RabbitMQ Management: [http://localhost:15672](http://localhost:15672) (user/pass: guest/guest)

### 2. Set RabbitMQ password for .NET app (local dev)

First run the User secrets Init

Init the secrets for the Console project

```bash
dotnet user-secrets init --project src/NET/MessageBroker.Presentation.Console/MessageBroker.Presentation.Console.csproj
```

Define the RabbitMQ password for the Console project

```bash
dotnet user-secrets set "MessageBroker:RabbitMQ:Password" "guest" --project src/NET/MessageBroker.Presentation.Console/MessageBroker.Presentation.Console.csproj
```

Init the secrets for the API project

```bash
dotnet user-secrets init --project src/NET/MessageBroker.Presentation.Api/MessageBroker.Presentation.Api.csproj
```

Define the RabbitMQ password for the API project

```bash
dotnet user-secrets set "MessageBroker:RabbitMQ:Password" "guest" --project src/NET/MessageBroker.Presentation.Api/MessageBroker.Presentation.Api.csproj
```

### 3. Run all .NET examples (Console)

```bash
cd src/NET/MessageBroker.Presentation.Console
dotnet run --project MessageBroker.Presentation.Console.csproj
```

Choose a queue type from the menu:

- `1` Fanout
- `2` Direct
- `3` Topic
- `4` Header
- `5` Exchange to Exchange
- `6` Alternate Exchange
- `7` Simple Consumer (for testing)
- `8` Poison Consumer (.NET)

### 4. Run the REST API

```bash
cd src/NET/MessageBroker.Presentation.Api
```

```bash
dotnet run --project MessageBroker.Presentation.Api.csproj
```

API docs: [http://localhost:5042/swagger](http://localhost:5042/swagger)

---

## Frontend (React + Vite)

### Setup & Usage

1. Install dependencies:

   ```bash
   cd src/NET/messagebroker-presentation-frontend
   npm install
   ```

2. Start the frontend:

   ```bash
   npm run dev
   ```

   The app will be available at [http://localhost:5173](http://localhost:5173).

3. Ensure the backend API is running and accessible at the URL set in `.env` (default: `http://localhost:5042`).

### Frontend Features

- Select an example and step, execute, and view results
- Execution history with timestamps and clear history option
- Responsive, modern UI with centered cards

---

## Environment Variables

- `VITE_API_URL`: The base URL for the backend API (set in `.env` in the frontend folder)

---

## Contributing

Pull requests are welcome! For major changes, please open an issue first to discuss what you would like to change.

---

This project was bootstrapped with [Vite](https://vitejs.dev/) and customized for the MessageBroker Presentation use case.