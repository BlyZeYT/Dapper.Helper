namespace Dapper.Helper;

public sealed class MultipleDbKeysException : Exception
{
    internal MultipleDbKeysException(string? message) : base(message) { }

    internal static void Throw<T>()
        => throw new MultipleDbKeysException($"{typeof(T).Name} has too much properties marked with DbKey attribute, only allowed is 1");
}