
# Message Broker Presentation - RabbitMQ Examples

This project demonstrates RabbitMQ queue types and messaging patterns using C# (.NET 8) and Node.js. It provides:

- A unified .NET Console application to run all publisher and consumer examples
- A shared library for example logic and input/output abstraction
- A Node.js poison message consumer for interoperability

**Blazor Web UI planned:** A Blazor Server web project will soon provide a browser-based UI for running examples. (See roadmap below.)

## Features

- Unified .NET 8 Console app for all RabbitMQ publisher and consumer examples
- Clean separation of shared logic and example implementations (CrossCut)
- Node.js poison message consumer for cross-platform demonstration
- Secure password management using .NET user-secrets

## Project Structure

- `src/NET/MessageBroker.Presentation.Console`: .NET 8 console application to run all RabbitMQ examples
- `src/NET/MessageBroker.Example.CrossCut`: Shared interfaces, attributes, factories, and example selection logic
- `src/NodeJs/PoisonConsumer.js`: Node.js poison message consumer
- `images/net-console-options.png`: Example selection menu screenshot

## Quickstart

1. **Start RabbitMQ:**

   ```bash
   docker-compose up -d
   ```

2. **Access RabbitMQ Management:**  
   [http://localhost:15672](http://localhost:15672) (user/pass: guest/guest)

3. **Set RabbitMQ password for .NET app (local dev):**

   ```powershell
   dotnet user-secrets set "MessageBroker:RabbitMQ:Password" "guest" --project src/NET/MessageBroker.Presentation.Console/MessageBroker.Presentation.Console.csproj
   ```

4. **Run all .NET examples:**

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

5. **Run Poison Consumer (Node.js):**

   ```bash
   cd src/NodeJs
   npm run start
   ```

## Secure RabbitMQ Password Usage

For security, the RabbitMQ password is not stored in `appsettings.json` or any file committed to source control.

- Use .NET user-secrets for local development (see above)
- For CI/CD or production, use environment variables or a secret store

## Roadmap

- [x] Unified .NET Console app for RabbitMQ examples
- [x] Shared CrossCut library for example logic
- [x] Node.js poison consumer for cross-platform demo
- [ ] Blazor Server web UI for running examples (coming soon)

## License

[MIT](https://choosealicense.com/licenses/mit/)