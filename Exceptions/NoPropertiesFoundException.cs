namespace Dapper.Helper;

public sealed class NoPropertiesFoundException : Exception
{
    internal NoPropertiesFoundException(string? message) : base(message) { }

    internal static void Throw<T>()
        => throw new NoPropertiesFoundException($"{typeof(T).Name} has no accessible properties");
}