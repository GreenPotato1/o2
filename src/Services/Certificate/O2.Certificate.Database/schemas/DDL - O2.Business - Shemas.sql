if (select [name] from sys.schemas where [name] like '%test%') is not null
	drop schema [test]

go

create schema [test]

go