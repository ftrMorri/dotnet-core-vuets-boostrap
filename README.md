# Experimental
## Dotnet Core 7 Vue-ts Quasar bootstrap

### Requirements

* Dotnet 7 SDK ( https://dotnet.microsoft.com/en-us/download/dotnet/7.0 )
* Node / Npm latest versions ( https://nodejs.org/en/ )
* SQL Server ( https://www.microsoft.com/en-us/sql-server/sql-server-downloads )
* OpenApi Generator ( npm install @openapitools/openapi-generator-cli -g )

### Add following to .vscode/launch.json

```
"version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Launch (console)",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/ConsoleApp/bin/Debug/net7.0/ConsoleApp.dll",
            "args": ["-iTrue", "-l../Bootstrapper_Generated/", "-nBootstrapDemo", "-s.\\SQLEXPRESS", "-dBootstrapDemo", "-ubootstrapdemouser", "-pbootstrapdemopass"],
            "cwd": "${workspaceFolder}",
            "console": "internalConsole",
            "stopAtEntry": false
        },
        {
            "name": ".NET Core Attach",
            "type": "coreclr",
            "request": "attach",
            "processId": "${command:pickProcess}"
        }
    ]
}
```

### How to use

- Open solution in Visual Studio or VSCode
- Check lauch settings for correct parameters ( path, database etc )
- Run with F5
- Open x-app folder with VSCode ( Volar plugin preferred )
- Open solution with Visual Studio or VSCode
- Start backend with F5
- In frontend VSCode, CTRL-P, task > generate api
- In frontend VSCode, CTRL-P, task > run dev
- Browse to http://localhost:5177/home/

### Todo

- [ ] Learn github best practices
- [ ] Security
- [ ] Security
- [ ] Security
- [ ] Sanity check: check for requirements before starting
- [ ] Better way to create first user
- [ ] Configure @-paths for typescript
- [ ] Release builds
- [ ] Generic errorhandling for frontend ( toast or something similiar )
- [ ] Generic errorhandling for backend ( file logging )
- [ ] Generic paging mechanic example for frontend/backend
- [ ] Use EF database migrations instead of .sql script -> support multiple databases
