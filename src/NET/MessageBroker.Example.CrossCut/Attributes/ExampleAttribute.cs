namespace MessageBroker.Example.CrossCut.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExampleAttribute : Attribute
{
    public ExampleAttribute(string name, ConsoleKey key)
    {
        Name = name;
        Key = key;
    }

    public string Name { get; set; }
    public ConsoleKey Key { get; set; }
}
