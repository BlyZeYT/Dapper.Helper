namespace Dapper.Helper;

using Dapper;
using Dapper.Helper.Internal;
using System.Data;
using System.Runtime.InteropServices;

public static partial class SqlMapperExtension
{
    public static T? Select<T>(
        this IDbConnection connection,
        T entity,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        in CancellationToken cancellationToken = default) where T : class, new()
    {
        var keyProp = Helper.GetKeyProp(entity);

        var condition = new ReadOnlySpan<char>();
        Helper.GetCondition(keyProp, ref condition);

        var value = connection.QueryFirstOrDefault<object>(
            new CommandDefinition(
                $"SELECT * FROM {Helper.GetTableName<T>()} WHERE {condition}",
                Helper.GetParam(keyProp),
                transaction,
                timeout,
                commandType,
                commandFlags,
                cancellationToken));

        return value is null ? (T?)value : Helper.PopulateProps<T>(value);
    }

    public static ReadOnlySpan<T> SelectAll<T>(
        this IDbConnection connection,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        in CancellationToken cancellationToken = default) where T : class, new()
    {
        List<object> entities = connection.Query<object>(
            new CommandDefinition(
                $"SELECT * FROM {Helper.GetTableName<T>()}",
                null,
                transaction,
                timeout,
                commandType,
                commandFlags,
                cancellationToken)).AsList();

        if (entities.Count is 0) return ReadOnlySpan<T>.Empty;

        var entitiesSpan = CollectionsMarshal.AsSpan(entities);

        Span<T> results = new T[entitiesSpan.Length];
        for (int i = 0; i < entitiesSpan.Length; i++)
        {
            results[i] = Helper.PopulateProps<T>(entitiesSpan[i]);
        }

        return results;
    }

    public static bool Insert<T>(
        this IDbConnection connection,
        T entity,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        in CancellationToken cancellationToken = default) where T : class
    {
        var keyProp = Helper.GetKeyProp(entity);

        var properties = new Span<DbProperty>();
        Helper.GetNoKeyProps(entity, keyProp, ref properties);

        var columns = new ReadOnlySpan<char>();
        Helper.GetColumns(properties, ref columns);

        var values = new ReadOnlySpan<char>();
        Helper.GetValueColumns(properties, ref values);

        return connection.Execute(
            new CommandDefinition(
                $"INSERT INTO {Helper.GetTableName<T>()} ({columns}) VALUES ({values})",
                Helper.GetParams(properties),
                transaction,
                timeout,
                commandType,
                commandFlags,
                cancellationToken)) > 0;
    }

    public static bool Update<T>(
        this IDbConnection connection,
        T entity,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        in CancellationToken cancellationToken = default) where T : class
    {
        var keyProp = Helper.GetKeyProp(entity);

        var properties = new Span<DbProperty>();
        Helper.GetNoKeyProps(entity, keyProp, ref properties);

        var columns = new ReadOnlySpan<char>();
        Helper.GetConditions(properties, ref columns);

        var condition = new ReadOnlySpan<char>();
        Helper.GetCondition(keyProp, ref condition);

        return connection.Execute(
            new CommandDefinition(
                $"UPDATE {Helper.GetTableName<T>()} SET {columns} WHERE {condition}",
                Helper.GetParams(properties, keyProp),
                transaction,
                timeout,
                commandType,
                commandFlags,
                cancellationToken)) > 0;
    }

    public static bool Delete<T>(
        this IDbConnection connection,
        T entity,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        in CancellationToken cancellationToken = default) where T : class
    {
        var keyProp = Helper.GetKeyProp(entity);

        var condition = new ReadOnlySpan<char>();
        Helper.GetCondition(keyProp, ref condition);

        return connection.Execute(
            new CommandDefinition(
                $"DELETE FROM {Helper.GetTableName<T>()} WHERE {condition}",
                Helper.GetParam(keyProp),
                transaction,
                timeout,
                commandType,
                commandFlags,
                cancellationToken)) > 0;
    }

    public static long InsertMultiple<T>(
        this IDbConnection connection,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        in CancellationToken cancellationToken = default,
        params T[] entities) where T : class
        => connection.InsertMultiple<T>(entities.AsSpan(), transaction, timeout, commandType, commandFlags, cancellationToken);

    public static long InsertMultiple<T>(
        this IDbConnection connection,
        in ReadOnlySpan<T> entities,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        in CancellationToken cancellationToken = default) where T : class
    {
        long affectedRows = 0;

        foreach (var entity in entities)
        {
            unchecked
            {
                affectedRows += connection.Insert(
                    entity, transaction, timeout, commandType, commandFlags, cancellationToken)
                    ? 1 : 0;
            }
        }

        return affectedRows;
    }

    public static long UpdateMultiple<T>(
        this IDbConnection connection,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        in CancellationToken cancellationToken = default,
        params T[] entities) where T : class
        => connection.UpdateMultiple<T>(entities.AsSpan(), transaction, timeout, commandType, commandFlags, cancellationToken);

    public static long UpdateMultiple<T>(
        this IDbConnection connection,
        in ReadOnlySpan<T> entities,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        in CancellationToken cancellationToken = default) where T : class
    {
        long rowsAffected = 0;

        foreach (var entity in entities)
        {
            unchecked
            {
                rowsAffected += connection.Update(
                    entity, transaction, timeout, commandType, commandFlags, cancellationToken)
                    ? 1 : 0;
            }
        }

        return rowsAffected;
    }

    public static long DeleteMultiple<T>(
        this IDbConnection connection,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        in CancellationToken cancellationToken = default,
        params T[] entities) where T : class
        => connection.DeleteMultiple<T>(entities.AsSpan(), transaction, timeout, commandType, commandFlags, cancellationToken);

    public static long DeleteMultiple<T>(
        this IDbConnection connection,
        in ReadOnlySpan<T> entities,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        in CancellationToken cancellationToken = default) where T : class
    {
        long rowsAffected = 0;

        foreach (var entity in entities)
        {
            unchecked
            {
                rowsAffected += connection.Delete(
                    entity, transaction, timeout, commandType, commandFlags, cancellationToken)
                    ? 1 : 0;
            }
        }

        return rowsAffected;
    }
}