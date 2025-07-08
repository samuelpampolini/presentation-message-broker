namespace MessageBroker.Example.CrossCut;
public class RabbitMQSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
}
