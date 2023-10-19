# Experimental
## Dotnet Core 7 Vue-ts Quasar bootstrap

### Requirements

* Dotnet 7 SDK ( https://dotnet.microsoft.com/en-us/download/dotnet/7.0 )
* Node / Npm latest versions ( https://nodejs.org/en/ )
* OpenApi Generator ( npm install @openapitools/openapi-generator-cli -g )
* Database
    * SQL Server ( https://www.microsoft.com/en-us/sql-server/sql-server-downloads )
        - sqlcmd needs to be in PATH
    * Postgre SQL
        - psql needs to be in path
    * SQLite
        - TODO

### How to use

- Open solution in Visual Studio
- Run with F5
- Open <your-appname>-app folder with VSCode ( Volar plugin preferred )
- Open solution with Visual Studio or VSCode
- Start backend with F5
- In frontend VSCode, CTRL-P, task > generate api
- In frontend VSCode, CTRL-P, task > run dev
- Browse to http://localhost:5177/home/