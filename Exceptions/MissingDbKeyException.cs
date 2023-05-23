namespace Dapper.Helper;

public sealed class MissingDbKeyException : Exception
{
    internal MissingDbKeyException(string? message) : base(message) { }

    internal static void Throw<T>()
        => throw new MissingDbKeyException($"{typeof(T).Name} has no properties marked with DbKey attribute");
}