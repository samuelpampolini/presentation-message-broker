import amqp from "amqplib";

const queueName = "presentation-node-poison-message-1";
const queueNameDld = "presentation-node-poison-message-dld";
const exchangeDld = "presentation-node-poison-exchange-dld";

class PoisonConsumer {
  async connect() {
    this.connection = await amqp.connect("amqp://localhost");
    this.channel = await this.connection.createChannel();
  }

  async dispose() {
    await this.channel.close();
    await this.connection.close();
  }

  async setupQueues() {
    await this.channel.assertExchange(exchangeDld, "direct", {
      durable: true,
    });

    await this.channel.assertQueue(queueNameDld, {
      durable: true,
    });

    await this.channel.bindQueue(
      queueNameDld,
      exchangeDld,
      "node-fail-message"
    );

    await this.channel.assertQueue(queueName, {
      durable: true,
      arguments: {
        "x-queue-type": "quorum",
        "x-dead-letter-exchange": exchangeDld,
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
      this.channel.sendToQueue(queueName, Buffer.from(JSON.stringify(msg)));
    });
  }

  async consume() {
    await this.channel.consume(queueName, (msg) => {
      if (msg !== null) {
        const messageContent = msg.content.toString();
        console.log(`Received message [${queueName}]: ${messageContent}`);

        if (!queueName.includes("dld") && messageContent.includes("fail")) {
          console.log("Message failed, rejecting...");
          this.channel.reject(msg, false);
        } else {
          this.channel.ack(msg);
        }
      }
    });
  }

  async CleanUp() {
    await this.channel.deleteQueue(queueName);
    await this.channel.deleteQueue(queueNameDld);
    await this.channel.deleteExchange(exchangeDld);
    console.log("Queues and exchanges cleaned up.");
  }
}

const runConsumer = async () => {
  const consumer = new PoisonConsumer();
  await consumer.connect();
  await consumer.setupQueues();
  await consumer.sendMessages();
  await consumer.consume();

  return consumer;
};

runConsumer()
  .then((consumer) => {
    console.log(
      "Consumer is running...\nPress Y to exit and clean up or any other key to exit without clean up."
    );

    let stdin = process.stdin;
    stdin.on("data", async function (key) {
      if (key.indexOf("Y") == 0) {
        await consumer.CleanUp();
      }

      consumer.dispose();
      process.exit();
    });
  })
  .catch((error) => {
    console.error("Failed to run RabbitMQ consumer", error);
  });
