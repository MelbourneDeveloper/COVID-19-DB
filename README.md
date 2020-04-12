# COVID-19-DB - NOT FOR REPORTING PURPOSES

[Follow Me](https://twitter.com/CFDevelop) on Twitter for updates to this database and code.

This database is in alpha. It contains errors. It should not be used for reporting. Do not publish data from this database.

**The database structure and code will change. Please do not rely on anything remaining the same**

**This code definately has bugs. Please report them in the issues section**

## COVID-19 SQLite Database 
This GitHub repo has C# code which generates an SQLite database based on COVID-19 figures from CSSE at Johns Hopkins University. That data can be found [here](https://github.com/CSSEGISandData/COVID-19/tree/master/csse_covid_19_data/csse_covid_19_daily_reports). This is the [Github page](https://github.com/CSSEGISandData/COVID-19).

The generated database is stored on dropbox and can be downloaded [here](https://www.dropbox.com/s/4fn90s1hcy73erh/Covid19Db%202020-04-12%20.db?dl=0).

## Why a database?
The current Johns Hopkins data is stored in CSV files and is split into daily sets. This makes it difficult to query the data. Databases provide a useful way to query the data with SQL. The code in this repo provides a replicable way to generate and update a database from the CSV files that John Hopkins provides daily. The database is currently an SQLite database, but the code allows for any database platform to generate the data. It is possible to generate the database as an SQL Server database, Oracle database, MySQL, or other database types. The code uses Entity Framework to create the database. Please change the connection string to use a different database.

## Basic Queries
Use the [DB Browser For SQLite](https://sqlitebrowser.org/) to open the database on any platform. The database structure looks like this:

![Database Structure](Images/DBStructure.png)

The `LocationDays` table contains `LocationId`, `DateOfCount`, `Deaths`, `Recoveries`, and `NewCases`. The location "50FBF714-6F45-4A73-8374-74CE4DACBEB3" represents anywhere in Victoria, Australia. To summarise all recoveries, run this query:

```sql
select sum(recoveries) from locationdays 
where locationid = '50FBF714-6F45-4A73-8374-74CE4DACBEB3' 
```

More examples will be added here.

![Summary Query](Images/SummaryQuery.png)

## Run the Code (Generate the Database)

- Clone the [Johns Hopkins repo](https://github.com/CSSEGISandData/COVID-19).
- Modify the [launch settings](https://github.com/MelbourneDeveloper/COVID-19-DB/blob/4f27a3fa49e11a780fda1d5dbad2b616cd7d3cd6/src/Covid19DB/Covid19DB/Properties/launchSettings.json#L5) to point to the `csse_covid_19_data/csse_covid_19_daily_reports` folder of the repo
- Run the app

Alternatively, you can run the tool at the command prompt on Windows, OSX, or Linux.

- Go to the folder `src\Covid19DB\Covid19DB` (or `src/Covid19DB/Covid19DB`)
- Run 
`dotnet run C:\Code\COVID-19\csse_covid_19_data\csse_covid_19_daily_reports`

*Note: replace the folder path with your folder path*

## CSV Reader

The code is useful for anyone who wants to read the Johns Hopkins CSV files. It's easy aggregate all the files in to memory. This code loads data from the entire dataset in to memory and then filters it down to the state of Victoria, Australia and order the data by date. It then dumps the data back out to a CSV file.

```cs
using Covid19DB.Utilities;
using System;
using System.IO;
using System.Linq;

namespace Covid19DB
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logger = new Logger<Processor>();

            var directoryPath = args.FirstOrDefault();

            if (string.IsNullOrEmpty(directoryPath)) throw new ArgumentException("Daily Reports Directory not specified");
            
            if (!Directory.Exists(directoryPath)) throw new ArgumentException($"The directory {directoryPath} does not exist. Please check the path");

            var rows = CsvReader.ReadCsvFiles(directoryPath);

            var victoriaRows = rows.Where(r => r.Province_State == "Victoria").OrderBy(r => r.Date).ToList();
            victoriaRows.ToCsv("Victoria.csv");
        }
    }
}
```

## How  Can I Help?

The hope is that this database can be thoroughly tested and validated. The hope is that this database will help create more accurate reporting data and allow people to more readily report on the figures. The database is not ready for this. It needs rigorous validation. Please comment on the database structure and code to help make this database ready for reporting.
