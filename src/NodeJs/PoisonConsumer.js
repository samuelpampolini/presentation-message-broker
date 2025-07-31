import amqp from "amqplib";

export class PoisonConsumer {
  queueName = "presentation-node-poison-message-1";
  queueNameDld = "presentation-node-poison-message-dld";
  exchangeDld = "presentation-node-poison-exchange-dld";

  async connect() {
    this.connection = await amqp.connect("amqp://localhost");
    this.channel = await this.connection.createChannel();
  }

  async dispose() {
    await this.channel.close();
    await this.connection.close();
  }

  async setupQueues() {
    await this.channel.assertExchange(this.exchangeDld, "direct", {
      durable: true,
    });

    await this.channel.assertQueue(this.queueNameDld, {
      durable: true,
    });

    await this.channel.bindQueue(
      this.queueNameDld,
      this.exchangeDld,
      "node-fail-message"
    );

    await this.channel.assertQueue(this.queueName, {
      durable: true,
      arguments: {
        "x-queue-type": "quorum",
        "x-dead-letter-exchange": this.exchangeDld,
        "x-dead-letter-routing-key": "node-fail-message",
        "x-delivery-limit": 3,
      },
    });
  }

  async sendMessages() {
    const messages = [
      { text: "Hello World! Message 1", attempts: 0 },
      { text: "Hello World! Message 2", attempts: 0 },
      { text: "This message will fail!", attempts: 0 }, // This message will be dead-lettered
      { text: "Hello World! Message 4", attempts: 0 },
      { text: "Another message that will fail!", attempts: 0 }, // Another one for testing
    ];

    messages.forEach((msg) => {
      this.channel.sendToQueue(
        this.queueName,
        Buffer.from(JSON.stringify(msg))
      );
    });
  }

  async consume() {
    await this.channel.consume(this.queueName, (msg) => {
      if (msg !== null) {
        const messageContent = msg.content.toString();
        console.log(`Received message [${this.queueName}]: ${messageContent}`);

        if (
          !this.queueName.includes("dld") &&
          messageContent.includes("fail")
        ) {
          console.log("Message failed, rejecting...");
          this.channel.reject(msg, false);
        } else {
          this.channel.ack(msg);
        }
      }
    });
  }

  async CleanUp() {
    await this.channel.deleteQueue(this.queueName);
    await this.channel.deleteQueue(this.queueNameDld);
    await this.channel.deleteExchange(this.exchangeDld);
    console.log("Queues and exchanges cleaned up.");
  }
}
