import { PoisonConsumer } from "./PoisonConsumer.js";

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
