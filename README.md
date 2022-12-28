# Experimental
## Dotnet Core 7 Vue-ts Quasar bootstrap

### Requirements

* Dotnet 7 SDK ( https://dotnet.microsoft.com/en-us/download/dotnet/7.0 )
* Node / Npm latest versions ( https://nodejs.org/en/ )
* SQL Server ( https://www.microsoft.com/en-us/sql-server/sql-server-downloads )

### Add following to .vscode/launch.json

```
"version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Launch (console)",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/StarterTool/bin/Debug/net7.0/StarterTool.dll",
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

### Todo

- [ ] Learn github best practices
- [ ] Security
- [ ] Security
- [ ] Security
- [ ] Sanity check: check for requirements before starting
- [ ] Better way to create first user
- [ ] Configure @-paths for typescript
- [ ] Configure @-paths for typescript
- [ ] Release builds
- [ ] Generic errorhandling for frontend ( toast or something similiar )
- [ ] Generic errorhandling for backend ( file logging )
- [ ] Generic paging mechanic example for frontend/backend
