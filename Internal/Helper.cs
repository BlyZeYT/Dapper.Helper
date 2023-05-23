namespace Dapper.Helper.Internal;

using System.Reflection;

internal static class Helper
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public;

    internal static string GetTableName<T>() where T : class
    {
        Type type = typeof(T);

        return type.GetCustomAttribute<DbTableAttribute>(true)?.DbTable ?? type.Name;
    }

    internal static DbProperty GetKeyProp<T>(T obj) where T : class
    {
        ReadOnlySpan<PropertyInfo> properties = typeof(T).GetProperties(Flags).AsSpan();

        if (properties.IsEmpty) NoPropertiesFoundException.Throw<T>();

        for (int i = 0; i < properties.Length; i++)
        {
            if (properties[i].GetCustomAttribute<DbKeyAttribute>(true) is not null)
                return new(properties[i].GetCustomAttribute<DbColumnAttribute>(true)?.DbColumn
                    ?? properties[i].Name, properties[i].GetValue(obj, Flags, null, null, null));
        }

        MissingDbKeyException.Throw<T>();

        return null!;
    }

    internal static void GetNoKeyProps<T>(T obj, DbProperty keyProperty, ref Span<DbProperty> destinationSpan) where T : class
    {
        ReadOnlySpan<PropertyInfo> properties = typeof(T).GetProperties(Flags).AsSpan();

        if (properties.IsEmpty) NoPropertiesFoundException.Throw<T>();

        destinationSpan = new DbProperty[properties.Length].AsSpan();

        for (int i = 0; i < properties.Length; i++)
        {
            destinationSpan[i] = new(properties[i].GetCustomAttribute<DbColumnAttribute>(true)?.DbColumn
                ?? properties[i].Name, properties[i].GetValue(obj, Flags, null, null, null));
        }

        destinationSpan = destinationSpan.Trim(keyProperty);
    }

    internal static T PopulateProps<T>(object value) where T : class, new()
    {
        T returnObj = new T();
        Type returnObjType = typeof(T);

        ReadOnlySpan<PropertyInfo> destinationProps = returnObjType.GetProperties(Flags).AsSpan();

        if (destinationProps.IsEmpty) NoPropertiesFoundException.Throw<T>();

        IDictionary<string, object> srcVal = (IDictionary<string, object>)value;

        if (srcVal.Count is 0) NoPropertiesFoundException.Throw<IDictionary<string, object>>();

        string name;
        for (int i = 0; i < destinationProps.Length; i++)
        {
            name = destinationProps[i].GetCustomAttribute<DbColumnAttribute>(true)?.DbColumn
                ?? destinationProps[i].Name;

            if (!srcVal.ContainsKey(
                destinationProps[i].GetCustomAttribute<DbColumnAttribute>(true)?.DbColumn
                ?? destinationProps[i].Name)) continue;

            destinationProps[i].SetValue(returnObj, srcVal[name], Flags, null, null, null);
        }

        return returnObj;
    }

    internal static void GetColumns(in ReadOnlySpan<DbProperty> properties, ref ReadOnlySpan<char> destinationSpan)
    {
        if (properties.IsEmpty) return;

        var builder = new SpanBuilder();

        foreach (DbProperty property in properties)
        {
            builder.Append(property.Name.AsSpan());
            builder.Append(',');
        }

        builder.Length--;

        destinationSpan = builder.AsReadOnlySpan();
    }

    internal static void GetValueColumns(in ReadOnlySpan<DbProperty> properties, ref ReadOnlySpan<char> destinationSpan)
    {
        if (properties.IsEmpty) return;

        var builder = new SpanBuilder();

        foreach (DbProperty property in properties)
        {
            builder.Append('@');
            builder.Append(property.Name.AsSpan());
            builder.Append(',');
        }

        builder.Length--;

        destinationSpan = builder.AsReadOnlySpan();
    }

    internal static void GetCondition(DbProperty property, ref ReadOnlySpan<char> destinationSpan)
    {
        var builder = new SpanBuilder();

        builder.Append(property.Name.AsSpan());
        builder.Append(" = @");
        builder.Append(property.Name.AsSpan());

        destinationSpan = builder.AsReadOnlySpan();
    }

    internal static void GetConditions(in ReadOnlySpan<DbProperty> properties, ref ReadOnlySpan<char> destinationSpan)
    {
        var builder = new SpanBuilder();

        foreach (DbProperty property in properties)
        {
            builder.Append(property.Name.AsSpan());
            builder.Append(" = @");
            builder.Append(property.Name.AsSpan());
            builder.Append(',');
        }

        builder.Length--;

        destinationSpan = builder.AsReadOnlySpan();
    }

    internal static DynamicParameters GetParam(DbProperty keyProperty)
    {
        var parameters = new DynamicParameters();

        parameters.Add($"@{keyProperty.Name}", keyProperty.Value);

        return parameters;
    }

    internal static DynamicParameters GetParams(in ReadOnlySpan<DbProperty> properties, DbProperty keyProperty)
    {
        var parameters = new DynamicParameters();

        parameters.Add($"@{keyProperty.Name}", keyProperty.Value);

        foreach (DbProperty property in properties)
            parameters.Add($"@{property.Name}", property.Value);

        return parameters;
    }

    internal static DynamicParameters GetParams(in ReadOnlySpan<DbProperty> properties)
    {
        var parameters = new DynamicParameters();

        foreach (DbProperty property in properties)
            parameters.Add($"@{property.Name}", property.Value);

        return parameters;
    }
}