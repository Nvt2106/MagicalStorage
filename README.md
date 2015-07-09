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
<p><i>Install-Package MagicalStorage</i></p>

Declare an entity:
``` swift
[EntityType]
public class Person
{
    [Unique, Required]
    public string FirstName { get; set; }

	[Unique, Required]
    public string LastName { get; set; }
}

[EntityType]
public class Classroom
{
    [Unique, Required]
	public string ClassroomName { get; set; }

	public virtual List<Person> People { get; set; }
}
```

Create MSEntityContext instance:
``` swift
var context = new MSEntityContext(new MSSQLRepository(), typeof(Person), typeof(Classroom));
```

Save a person to storage:
``` swift
var person = new Person()
{
	FirstName = "Adam",
	LastName = "Smith"
};
var classroom = new Classroom()
{
    ClassroomName = "CLassA",
	People = new List<Person>();
};
classroom.People.Add(person);

List<MSError> errors = null;
context.Save(classroom, out errors); // Save both classroom and person
if (errors != null) {
    // TODO: error handling here
}
```