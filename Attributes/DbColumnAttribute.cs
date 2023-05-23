namespace Dapper.Helper;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class DbColumnAttribute : Attribute
{
    public string DbColumn { get; }

    public DbColumnAttribute(string dbColumn)
    {
        DbColumn = dbColumn;
    }
}