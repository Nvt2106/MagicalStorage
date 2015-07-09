# MagicalStorage
Store and retrieve data objects to/from many type of storages (including MSSQL, MySQL and InMemory), like Entity Framework.

# Features
- [x] Support MSSQL, MySQL, InMemory (usually for testing purpose during development of your system)
- [x] Support Unique attribute in Entity declaration (not available in Entity Framework)
- [x] Automatically generate database objects (tables, stored procedures) for MSSQL, MySQL
- [x] Super easy to use
- [ ] TODO: support other types such as Oracle, MongoDB, DB2, etc

# Getting Started
Add nuget package to your project:
<i>Install-Package MagicalStorage</i>

Declare an entity:
[EntityType]
public class Person
{
    public string FirstName { get; set; }
}

Create MSEntityContext instance:
var context = new MSEntityContext(new InMemoryRepository(), typeof(Person));

Save a person to storage:
var person = new Person() { FirstName = "Adam" };
List<MSError> errors = null;
context.Save(person, out errors);
if (errors != null) {
    // TODO: error handling here
}