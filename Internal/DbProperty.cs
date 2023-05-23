namespace Dapper.Helper.Internal;

internal sealed record DbProperty
{
    public string Name { get; }
    public object? Value { get; }

    public DbProperty(string name, object? value)
    {
        Name = name;
        Value = value;
    }
}