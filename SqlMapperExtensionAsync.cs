namespace Dapper.Helper;

using Dapper.Helper.Internal;
using System.Data;

public static partial class SqlMapperExtension
{
    public static async Task<T?> SelectAsync<T>(
        this IDbConnection connection,
        T entity,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        CancellationToken cancellationToken = default) where T : class, new()
    {
        var keyProp = Helper.GetKeyProp(entity);

        var value = await connection.QueryFirstOrDefaultAsync<object>(
            new CommandDefinition(
                $"SELECT * FROM {Helper.GetTableName<T>()} WHERE {GetCondition()}",
                Helper.GetParam(keyProp),
                transaction,
                timeout,
                commandType,
                commandFlags,
                cancellationToken));

        return value is null ? (T?)value : Helper.PopulateProps<T>(value);

        string GetCondition()
        {
            var condition = new ReadOnlySpan<char>();
            Helper.GetCondition(keyProp, ref condition);

            return condition.ToString();
        }
    }

    public static async Task<IEnumerable<T>> SelectAllAsync<T>(
        this IDbConnection connection,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        CancellationToken cancellationToken = default) where T : class, new()
    {
        var entities = (await connection.QueryAsync<object>(
            new CommandDefinition(
                $"SELECT * FROM {Helper.GetTableName<T>()}",
                null,
                transaction,
                timeout,
                commandType,
                commandFlags,
                cancellationToken))).AsList();

        return entities.Count is 0 ? Enumerable.Empty<T>() : GetResults();

        IEnumerable<T> GetResults()
        {
            foreach (object entity in entities)
                yield return Helper.PopulateProps<T>(entity);
        }
    }

    public static async Task<bool> InsertAsync<T>(
        this IDbConnection connection,
        T entity,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        CancellationToken cancellationToken = default) where T : class
    {
        var (columns, values, parameters) = GetArguments();

        return await connection.ExecuteAsync(
            new CommandDefinition(
                $"INSERT INTO {Helper.GetTableName<T>()} ({columns}) VALUES ({values})",
                parameters,
                transaction,
                timeout,
                commandType,
                commandFlags,
                cancellationToken)) > 0;

        (string, string, DynamicParameters) GetArguments()
        {
            var keyProp = Helper.GetKeyProp(entity);

            var properties = new Span<DbProperty>();
            Helper.GetNoKeyProps(entity, keyProp, ref properties);

            var columns = new ReadOnlySpan<char>();
            Helper.GetColumns(properties, ref columns);

            var values = new ReadOnlySpan<char>();
            Helper.GetValueColumns(properties, ref values);

            return (columns.ToString(), values.ToString(), Helper.GetParams(properties));
        }
    }

    public static async Task<bool> UpdateAsync<T>(
        this IDbConnection connection,
        T entity,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        CancellationToken cancellationToken = default) where T : class
    {
        var (conditions, condition, parameters) = GetArguments();

        return await connection.ExecuteAsync(
            new CommandDefinition(
                $"UPDATE {Helper.GetTableName<T>()} SET {conditions} WHERE {condition}",
                parameters,
                transaction,
                timeout,
                commandType,
                commandFlags,
                cancellationToken)) > 0;

        (string, string, DynamicParameters) GetArguments()
        {
            var keyProp = Helper.GetKeyProp(entity);

            var properties = new Span<DbProperty>();
            Helper.GetNoKeyProps(entity, keyProp, ref properties);

            var conditions = new ReadOnlySpan<char>();
            Helper.GetConditions(properties, ref conditions);

            var condition = new ReadOnlySpan<char>();
            Helper.GetCondition(keyProp, ref condition);

            return (conditions.ToString(), condition.ToString(), Helper.GetParams(properties, keyProp));
        }
    }

    public static async Task<bool> DeleteAsync<T>(
        this IDbConnection connection,
        T entity,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        CancellationToken cancellationToken = default) where T : class
    {
        var (condition, parameters) = GetArguments();

        return await connection.ExecuteAsync(
            new CommandDefinition(
                $"DELETE FROM {Helper.GetTableName<T>()} WHERE {condition}",
                parameters,
                transaction,
                timeout,
                commandType,
                commandFlags,
                cancellationToken)) > 0;

        (string, DynamicParameters) GetArguments()
        {
            var keyProp = Helper.GetKeyProp(entity);

            var condition = new ReadOnlySpan<char>();
            Helper.GetCondition(keyProp, ref condition);

            return (condition.ToString(), Helper.GetParam(keyProp));
        }
    }

    public static async Task<long> InsertMultipleAsync<T>(
        this IDbConnection connection,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        CancellationToken cancellationToken = default,
        params T[] entities) where T : class
    {
        long affectedRows = 0;

        foreach (var entity in entities)
        {
            unchecked
            {
                affectedRows += await connection.InsertAsync(
                    entity, transaction, timeout, commandType, commandFlags, cancellationToken)
                    ? 1 : 0;
            }
        }

        return affectedRows;
    }

    public static async Task<long> UpdateMultipleAsync<T>(
        this IDbConnection connection,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        CancellationToken cancellationToken = default,
        params T[] entities) where T : class
    {
        long rowsAffected = 0;

        foreach (var entity in entities)
        {
            unchecked
            {
                rowsAffected += await connection.UpdateAsync(
                    entity, transaction, timeout, commandType, commandFlags, cancellationToken)
                    ? 1 : 0;
            }
        }

        return rowsAffected;
    }

    public static async Task<long> DeleteMultipleAsync<T>(
        this IDbConnection connection,
        IDbTransaction? transaction = null,
        int? timeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        CancellationToken cancellationToken = default,
        params T[] entities) where T : class
    {
        long rowsAffected = 0;

        foreach (var entity in entities)
        {
            unchecked
            {
                rowsAffected += await connection.DeleteAsync(
                    entity, transaction, timeout, commandType, commandFlags, cancellationToken)
                    ? 1 : 0;
            }
        }

        return rowsAffected;
    }
}