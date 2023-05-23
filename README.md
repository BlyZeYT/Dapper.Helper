# Dapper.Helper
Easy to use extensions for Dapper.

> 🟢 **Project status**: Active<sup>[[?]](https://github.com/BlyZeYT/.github/blob/master/project-status.md)</sup>

| Package | NuGet |
| ------- | ----- |
| [Dapper.Helper](https://www.nuget.org/packages/Dapper.Helper/) | [![Dapper.Helper](https://img.shields.io/nuget/v/Dapper.Helper?color=white&label=NuGet)](https://www.nuget.org/packages/Dapper.Helper/)

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

## Example

### Create your Model

```csharp
public record Person
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime Birthday { get; set; }

    [DbColumn("Postcode")]
    public string ZipCode { get; set; }

    public string Street { get; set; }

    public string City { get; set; }

    [DbColumn("E-Mail")]
    public string Email { get; set; }
    
    public string Phone { get; set; }

    public Person(string firstName, string lastName, DateTime birthday, string zipCode, string street, string city, string email, string phone)
    {
        FirstName = firstName;
        LastName = lastName;
        Birthday = birthday;
        ZipCode = zipCode;
        Street = street;
        City = city;
        Email = email;
        Phone = phone;
    }
}

[DbTable("Developers")]
public sealed record Developer : Person
{
    [DbKey]
    public int Id { get; set; }

    public decimal Salary { get; set; }

    public Developer() : this(-1, "", "", default, "", "", "", "", "", 0) { }

    public Developer(int id, string firstName, string lastName, DateTime birthday, string zipCode, string street, string city, string email, string phone, decimal salary)
        : base(firstName, lastName, birthday, zipCode, street, city, email, phone)
    {
        Id = id;
        Salary = salary;
    }
}
```

### Method usage examples

```csharp
using (var connection = new MySqlConnection("SERVER=localhost;Database=Company;Uid=admin;Password=SuperSecurePw;"))
{
    Developer? developer = await connection.SelectAsync(new Developer
    {
        Id = 5
    }); //Select the entry with Id -> 5, null if no entry was found

    if (developer is null) return;

    bool inserted = await connection.InsertAsync(new Developer
    {
        FirstName = "Leon",
        LastName = "Schimmel",
        Birthday = new DateTime(2004, 7, 5),
        ZipCode = "12345",
        Street = "Very Cool Street 69",
        City = "Cool Town",
        Email = "leonschimmel@blyze.com",
        Phone = "0123 - 456789",
        Salary = 3000
    }); //Insert a new entry, true if succeeded, otherwise false

    bool updated = await connection.UpdateAsync(developer with
    {
        LastName = developer.LastName.Remove(0, 1)
    }); //Update the existing entry with Id = 5, true if succeeded, otherwise false

    bool deleted = await connection.DeleteAsync(new Developer
    {
        Id = 69
    }); //Delete the entry with the Id -> 69, true if succeeded, otherwise false

    //Get all entries from a table
    IEnumerable<Developer> developers = await connection.SelectAllAsync<Developer>();
}
```
