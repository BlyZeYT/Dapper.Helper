# Dapper.Helper
Easy to use extensions for Dapper.

Available extension methods:
```csharp
T? Select<T>(T entity);
ReadOnlySpan<T> SelectAll<T>();
bool Insert<T>(T entity);
bool Update<T>(T entity);
bool Delete<T>(T entity);
long InsertMultiple<T>(in ReadOnlySpan<T> entities);
long InsertMultiple<T>(params T[] entities);
long UpdateMultiple<T>(in ReadOnlySpan<T> entities);
long UpdateMultiple<T>(params T[] entities);
long DeleteMultiple<T>(in ReadOnlySpan<T> entities);
long DeleteMultiple<T>(params T[] entities);

Task<T?> SelectAsync<T>(T entity);
Task<IEnumerable<T>> SelectAllAsync<T>();
Task<bool> InsertAsync<T>(T entity);
Task<bool> UpdateAsync<T>(T entity);
Task<bool> DeleteAsync<T>(T entity);
Task<long> InsertMultipleAsync<T>(params T[] entities);
Task<long> UpdateMultipleAsync<T>(params T[] entities);
Task<long> DeleteMultipleAsync<T>(params T[] entities);
```
