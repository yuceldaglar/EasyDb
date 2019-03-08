# EasyDb
EasyDb is a sql database engine, written in c#. Engine can be used as in-memory and embedded. Data is also saved to disk. Native c# value types are used as data types. Has transaction support.

## Project Features
- Written in C#
- An embedded database engine
- Work in-memory also can save to disk
- C# types as columns type (String, Int32, ...)
- Sql querying
- Transaction support(serializable only)
- Use both column-store and row-store (column-store for memory data, row-store for disk data)
- Metadata like Information-Schema (tables, columns)
- There is also a simple user interface to execute sql

