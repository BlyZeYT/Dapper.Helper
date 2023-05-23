namespace Dapper.Helper;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DbTableAttribute : Attribute
{
    public string DbTable { get; }

    public DbTableAttribute(string dbTable)
    {
        DbTable = dbTable;
    }
}