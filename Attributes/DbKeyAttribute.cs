namespace Dapper.Helper;

using System;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class DbKeyAttribute : Attribute
{
    public DbKeyAttribute() { }
}