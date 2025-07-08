import amqp from 'amqplib';

const queueName = process.argv[0];

const randomString = (length: number): string => {
    const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let result = '';
    for (let i = 0; i < length; i++) {
        const randomIndex = Math.floor(Math.random() * characters.length);
        result += characters[randomIndex];
    }
    return result;
};

const runProducer = async (): Promise<void> => {
    const connection = await amqp.connect('amqp://localhost');
    const channel = await connection.createChannel();

    const sendMessage = async (message: string): Promise<void> => {
        await channel.assertQueue(queueName, { durable: true });
        channel.sendToQueue(queueName, Buffer.from(message));
        console.log(`Message sent to ${queueName}: ${message}`);
    };

    for (let i = 0; i < 10; i++) {
        const randomPrefix = randomString(10);
        let message = `${i}_receiver${randomPrefix}@example.com`;

        if (queueName.indexOf('poison') >= 0 && i % 2 === 0) {
            // Simulate a poison message
            message = `fails - ${message}`;
        }

        await sendMessage(message);
    }

    await channel.close();
    await connection.close();
};

runProducer()
    .then(() => {
        console.log('Producer finished sending messages.');
    })
    .catch((error) => {
        console.error('Failed to run RabbitMQ producer', error);
    });