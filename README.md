# COVID-19-DB

[Follow Me](https://twitter.com/CFDevelop) on Twitter for updates to this database and code.

Download the SQLite database [here](https://www.dropbox.com/s/58ayrvl1p98n7bg/Covid19Db%202020-04-16.db?dl=0). This database aggregates data from the Johns Hopkins CSSE CSV [daily reports](https://github.com/CSSEGISandData/COVID-19/tree/master/csse_covid_19_data/csse_covid_19_daily_reports) in to a single SQLite database. It uses C# to generate the database, but you can open it with [DB Browser For SQLite](https://sqlitebrowser.org/). This is the [COVID-19 Johns Hopkins CSSEGithub page](https://github.com/CSSEGISandData/COVID-19).

**This database is in alpha. The database structure and code may change and there may be bugs. Please help by reporting bugs and inconsistencies in the issues section. The aim is to get this database to be as reliable as possible. NOT FOR REPORTING PURPOSES**

## Query the Data
Use the [DB Browser For SQLite](https://sqlitebrowser.org/) to open the database on any platform. The database structure looks like this:

![Database Structure](Images/DBStructure.png)

The `LocationDays` table contains `LocationId`, `DateOfCount`, `Deaths`, `Recoveries`, and `NewCases`. These values can be summarized as below.

### List Top 20 Regions With Most New Cases In Last Week

```sql
select	
		Regions.Name,
		sum(newcases) as NewCasesLastWeek
from locationdays 
inner join locations
on locations.id = locationdays.locationid
inner join Provinces
on Provinces.id = locations.ProvinceId
inner join Regions
on Regions.id = Provinces.RegionId
where 
date(DateOfCount) >= date('now','-7 days')
AND date(DateOfCount) < date('now')
group by 		
		Regions.Id,
		Regions.Name
order by NewCasesLastWeek desc		
limit 20	
```

As of 2020-04-16

**These figures have NOT been validated. This an example only**

| Region         | NewCasesLastWeek |
|----------------|------------------|
| US             | 206364           |
| United Kingdom | 38273            |
| Turkey         | 31911            |
| Spain          | 31726            |
| France         | 28310            |
| Italy          | 25315            |
| Germany        | 19517            |
| Russia         | 17807            |
| Brazil         | 12333            |
| Iran           | 11775            |
| Canada         | 10155            |
| Belgium        | 9826             |
| Netherlands    | 7480             |
| Peru           | 7235             |
| India          | 6705             |
| Ireland        | 6697             |
| Portugal       | 4885             |
| Japan          | 3959             |
| Sweden         | 3399             |
| Ecuador        | 3260             |

### Get Totals for Australian States

```sql
select	Provinces.Name,
		sum(newcases) as TotalCases,
		sum(deaths) as TotalDeaths,
		sum(Recoveries) as TotalRecoveries	
from locationdays 
inner join locations
on locations.id = locationdays.locationid
inner join Provinces
on Provinces.id = locations.ProvinceId
inner join Regions
on Regions.id = Provinces.RegionId
where Regions.Name='Australia'
group by Provinces.Name
```
As of 2020 - 4 - 12

Note: *Figures here highlight issues with the Johns Hopkins data. Notice that New South Wales only has 4 recoveries. This is incorrect. See the incorrect value in the Johns Hopkins data [here](https://github.com/CSSEGISandData/COVID-19/blob/master/csse_covid_19_data/csse_covid_19_daily_reports/04-12-2020.csv#L2771)*

| State                        | Confirmed | Deaths | Recoveries |
|------------------------------|-----------|--------|------------|
| Australian Capital Territory | 103       | 2      | 59         |
| External territories         | 0         | 0      | 0          |
| From Diamond Princess        | 0         | 0      | 0          |
| Jervis Bay Territory         | 0         | 0      | 0          |
| N/A                          | 4         |        |            |
| New South Wales              | 2857      | 23     | 4          |
| Northern Territory           | 28        | 0      | 2          |
| Queensland                   | 974       | 5      | 372        |
| South Australia              | 429       | 3      | 179        |
| Tasmania                     | 133       | 4      | 48         |
| Victoria                     | 1265      | 14     | 926        |
| Western Australia            | 514       | 6      | 216        |

*More examples will be added here*

## Getting Started (Generate the Database)

Run these commands at the command prompt or terminal. Make sure you run initialize the submodule if you clone with your favourite Git client.

- Clone the repo and source data. **This includes the [Johns Hopkins CSSE COVID-19 repo](https://github.com/CSSEGISandData/COVID-19).**

> git clone --recursive https://github.com/MelbourneDeveloper/COVID-19-DB.git

- Navigate to COVID-19-DB/src/Covid19DB

- Run the app

> dotnet run ../../CSSE-COVID-19/csse_covid_19_data/csse_covid_19_daily_reports

Or, open the solution in Visual Studio 2019 and run the app.

**Make sure you do a pull on the Johns Hopkins CSSE submodule to get the latest data!**

## Why a database?
The current Johns Hopkins data is stored in CSV files and is split into daily sets. This makes it difficult to query the data over time. Databases provide a useful way to query the data with SQL. The code in this repo provides a replicable way to generate the database from the CSV files that John Hopkins provides daily. The database is currently an SQLite database, but the code allows for any database platform to generate the data. It is possible to generate the database as an SQL Server database, Oracle database, MySQL, or other database types. The code uses Entity Framework to create the database. Please change the connection string to use a different database.

## CSV Reader

The code is useful for anyone who wants to read the Johns Hopkins CSV files. It's easy aggregate all the files in to memory. This code loads data from the entire dataset in to memory and then filters it down to the state of Victoria, Australia and orders the data by date. It then dumps the data back out to a CSV file.

```cs
private static async Task ProcessAsync(string directoryPath)
{
    var logger = new Logger<Processor>();
    var fileSystemCsvFileService = new FileSystemCsvFileService(directoryPath);
    var csvReader = new CsvReader(fileSystemCsvFileService);
    var rows = await csvReader.ReadCsvFiles();

    //Filter by state and order by date
    var victoriaRows = rows.Where(r => r.Province_State == "Victoria").OrderBy(r => r.Date).ToList();
    victoriaRows.ToMarkdownTable("Victoria.md");
}
```

Output (Some data truncated):

|Country_Region|Province_State|Lat|Long_|Confirmed|Deaths|Recovered|Active|Date|Admin2|CsvRowNumber|
|--|--|--|--|--|--|--|--|--|--|--|
|Australia|Victoria|-37.8136|144.9631|296|0|70|226|2020/03/22||3192|
|Australia|Victoria|-37.8136|144.9631|355|0|97|258|2020/03/23||3247|
|Australia|Victoria|-37.8136|144.9631|411|0|97|314|2020/03/24||3247|
|Australia|Victoria|-37.8136|144.9631|466|0|97|369|2020/03/25||3247|
|Australia|Victoria|-37.8136|144.9631|520|3|149|368|2020/03/26||3245|
|Australia|Victoria|-37.8136|144.9631|574|3|171|400|2020/03/27||3252|
|Australia|Victoria|-37.8136|144.9631|685|3|191|491|2020/03/28||3252|
|Australia|Victoria|-37.8136|144.9631|769|4|191|574|2020/03/29||3256|
|Australia|Victoria|-37.8136|144.9631|821|4|191|626|2020/03/30||3260|
|Australia|Victoria|-37.8136|144.9631|917|4|291|622|2020/03/31||2253|
|Australia|Victoria|-37.8136|144.9631|968|4|343|621|2020/04/01||2302|
|Australia|Victoria|-37.8136|144.9631|1036|5|422|609|2020/04/02||2386|
|Australia|Victoria|-37.8136|144.9631|1085|7|476|602|2020/04/03||2442|
|Australia|Victoria|-37.8136|144.9631|1115|8|527|580|2020/04/04||2496|
|Australia|Victoria|-37.8136|144.9631|1135|8|573|554|2020/04/05||2579|
|Australia|Victoria|-37.8136|144.9631|1158|10|620|528|2020/04/06||2623|
|Australia|Victoria|-37.8136|144.9631|1191|11|620|560|2020/04/07||2672|
|Australia|Victoria|-37.8136|144.9631|1212|12|620|580|2020/04/08||2698|
|Australia|Victoria|-37.8136|144.9631|1228|12|736|480|2020/04/09||2726|
|Australia|Victoria|-37.8136|144.9631|1241|13|926|302|2020/04/10||2755|
|Australia|Victoria|-37.8136|144.9631|1265|14|926|325|2020/04/11||2779|
|Australia|Victoria|-37.8136|144.9631|1268|14|926|328|2020/04/12||2802|
|Australia|Victoria|-37.8136|144.9631|1281|14|926|341|2020/04/13||2816|
|Australia|Victoria|-37.8136|144.9631|1291|14|1118|159|2020/04/14||2828|
|Australia|Victoria|-37.8136|144.9631|1299|14|1118|167|2020/04/15||2841|
|Australia|Victoria|-37.8136|144.9631|1299|14|1137|148|2020/04/16||2856|


## How  Can I Help?

The hope is that this database can be thoroughly tested and validated. The hope is that this database will help create more accurate reporting data and allow people to more readily report on the figures. The database is not ready for this. It needs rigorous validation. Please comment on the database structure and code to help make this database ready for reporting.
