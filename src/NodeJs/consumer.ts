import amqp from 'amqplib';

const queueName = process.argv[0];

const runConsumer = async (): Promise<void> => {
    const connection = await amqp.connect('amqp://localhost');
    const channel = await connection.createChannel();

    const handleMessage = () => async (message: amqp.ConsumeMessage | null): Promise<void> => {
        if (message) {
            console.log(`Received message from ${queueName}: ${message.content.toString()}`);
            const parsedMessage = JSON.parse(message.content.toString());
            console.log('Handling SMS notification:', parsedMessage);
            channel.ack(message);
        }
    };

    // Subscribe to the queue
    await channel.assertQueue(queueName, { durable: true });
    await channel.consume(queueName, handleMessage());

    console.log('Consumer is subscribed to queue:', queueName);
};

runConsumer()
    .then(() => {
        console.log('Consumer is running...');
    })
    .catch((error) => {
        console.error('Failed to run RabbitMQ consumer', error);
    });