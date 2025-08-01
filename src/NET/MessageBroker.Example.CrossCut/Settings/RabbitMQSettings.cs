namespace MessageBroker.Example.CrossCut.Settings;

public class RabbitMQSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
}
