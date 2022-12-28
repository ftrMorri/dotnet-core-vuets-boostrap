using CliWrap;
using CommandLine;
using StarterTool;

Options? options = null;

Parser.Default.ParseArguments<Options>(args)
    .WithParsed<Options>(o => options = o)
    .WithNotParsed(e => {
        Console.WriteLine(e.Select(e2 => e2.ToString()));
    });

if (options == null)
    throw new Exception("Failed to parse options.");

var generatorDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

if (options.Clean == true)
{
    Clean(Path.Combine(options.Path, options.Name));
}

var rootDirectory = Directory.CreateDirectory(Path.Combine(options.Path, options.Name));
Directory.SetCurrentDirectory(rootDirectory.FullName);


await using var stdOut = Console.OpenStandardOutput();
await using var stdErr = Console.OpenStandardError();
await using var stdIn = Console.OpenStandardInput();
{
    Func<string, string[], CliWrap.CommandTask<CommandResult>> cmd = (string command, string[] arguments) => {
        return Cli.Wrap(command)
            .WithArguments(arguments)
            .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
            .WithStandardErrorPipe(PipeTarget.ToStream(stdErr))
            .WithStandardInputPipe(PipeSource.FromStream(stdIn))
            .ExecuteAsync();
    };

    Func<string, string, CliWrap.CommandTask<CommandResult>> cmdSingleArg = (string command, string argument) => {
        return Cli.Wrap(command)
            .WithArguments(argument)
            .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
            .WithStandardErrorPipe(PipeTarget.ToStream(stdErr))
            .WithStandardInputPipe(PipeSource.FromStream(stdIn))
            .ExecuteAsync();
    };

    await CreateVueApp(rootDirectory, options, cmd, cmdSingleArg);
    await CreateCsharpApp(rootDirectory, options, cmd, cmdSingleArg);

    var variables = new EnvironmentVariables
    {
        Name = options.Name,
        DatabaseServer = options.DatabaseServer,
        DatabaseName = options.DatabaseName,
        DatabaseUser = options.DatabaseUser,
        DatabasePassword = options.DatabasePassword,
    };

    CopyTemplates(generatorDirectory, rootDirectory, options, variables);

    await CreateDatabase(rootDirectory, options, cmd);

    CommandResult? result = await cmd("dotnet", new[] {"build", $"{options.Name}.sln" });
}

async Task CreateDatabase(DirectoryInfo rootDirectory, Options options, Func<string, string[], CommandTask<CommandResult>> cmd)
{
    CommandResult? result = await cmd("sqlcmd", 
        new[] {"-S", 
            options.DatabaseServer, 
            "-i", 
            Path.Combine(rootDirectory.FullName, $"{options.Name}.Data", "generate.sql")});
}

async Task CreateCsharpApp(DirectoryInfo rootDirectory, Options options, Func<string, string[], CommandTask<CommandResult>> cmd, Func<string, string, CommandTask<CommandResult>> cmdSingleArg)
{
    CommandResult? result = await cmd("dotnet", new[] {"new", "sln", "--name", options.Name});

    result = await cmd("dotnet", new[] {"new", "webapi", "--name", options.Name + ".Site"});
    result = await cmd("dotnet", new[] {"new", "classlib", "--name", options.Name + ".Services"});
    result = await cmd("dotnet", new[] {"new", "classlib", "--name", options.Name + ".Data"});
    result = await cmd("dotnet", new[] {"new", "mstest", "--name", options.Name + ".Tests"});

    result = await cmd("dotnet", new[] {"sln", "add", $"{options.Name}.Site/{options.Name}.Site.csproj"});
    result = await cmd("dotnet", new[] {"sln", "add", $"{options.Name}.Services/{options.Name}.Services.csproj"});
    result = await cmd("dotnet", new[] {"sln", "add", $"{options.Name}.Data/{options.Name}.Data.csproj"});
    result = await cmd("dotnet", new[] {"sln", "add", $"{options.Name}.Tests/{options.Name}.Tests.csproj"});

    result = await cmd("dotnet", new[] {"add", $"{options.Name}.Site/{options.Name}.Site.csproj", "reference", $"{options.Name}.Services/{options.Name}.Services.csproj"});
    result = await cmd("dotnet", new[] {"add", $"{options.Name}.Site/{options.Name}.Site.csproj", "reference", $"{options.Name}.Data/{options.Name}.Data.csproj"});

    result = await cmd("dotnet", new[] {"add", $"{options.Name}.Tests/{options.Name}.Tests.csproj", "reference", $"{options.Name}.Services/{options.Name}.Services.csproj"});
    result = await cmd("dotnet", new[] {"add", $"{options.Name}.Tests/{options.Name}.Tests.csproj", "reference", $"{options.Name}.Data/{options.Name}.Data.csproj"});

    result = await cmd("dotnet", new[] {"add", $"{options.Name}.Services/{options.Name}.Services.csproj", "reference", $"{options.Name}.Data/{options.Name}.Data.csproj"});

    Directory.SetCurrentDirectory($"{options.Name}.Site");
    result = await cmd("dotnet", new[] {"add", "package", "Microsoft.AspNetCore.SpaServices.Extensions"});
    result = await cmd("dotnet", new[] {"add", "package", "Microsoft.EntityFrameworkCore"});
    result = await cmd("dotnet", new[] {"add", "package", "Microsoft.EntityFrameworkCore.SqlServer"});
    result = await cmd("dotnet", new[] {"add", "package", "Microsoft.AspNetCore.Identity"});
    result = await cmd("dotnet", new[] {"add", "package", "Microsoft.AspNetCore.Identity.EntityFrameworkCore"});
    result = await cmd("dotnet", new[] {"add", "package", "Microsoft.AspNetCore.Authentication.JwtBearer"});
    
    Directory.SetCurrentDirectory($"../{options.Name}.Services");
    result = await cmd("dotnet", new[] {"add", "package", "Microsoft.EntityFrameworkCore"});
    result = await cmd("dotnet", new[] {"add", "package", "Microsoft.EntityFrameworkCore.SqlServer"});
    result = await cmd("dotnet", new[] {"add", "package", "Microsoft.AspNetCore.Identity"});
    result = await cmd("dotnet", new[] {"add", "package", "Microsoft.AspNetCore.Identity.EntityFrameworkCore"});
    Directory.SetCurrentDirectory($"../{options.Name}.Data");
    result = await cmd("dotnet", new[] {"add", "package", "Microsoft.EntityFrameworkCore"});
    result = await cmd("dotnet", new[] {"add", "package", "Microsoft.EntityFrameworkCore.SqlServer"});
    result = await cmd("dotnet", new[] {"add", "package", "Microsoft.AspNetCore.Identity"});
    result = await cmd("dotnet", new[] {"add", "package", "Microsoft.AspNetCore.Identity.EntityFrameworkCore"});
    Directory.SetCurrentDirectory($"../{options.Name}.Tests");
    result = await cmd("dotnet", new[] {"add", "package", "Microsoft.EntityFrameworkCore"});
    result = await cmd("dotnet", new[] {"add", "package", "Microsoft.EntityFrameworkCore.SqlServer"});
    result = await cmd("dotnet", new[] {"add", "package", "Microsoft.AspNetCore.Identity"});
    result = await cmd("dotnet", new[] {"add", "package", "Microsoft.AspNetCore.Identity.EntityFrameworkCore"});

    Directory.SetCurrentDirectory(rootDirectory.FullName);
}

async Task CreateVueApp(DirectoryInfo rootDirectory, Options options, Func<string, string[], CommandTask<CommandResult>> cmd, Func<string, string, CommandTask<CommandResult>> cmdSingleArg)
{
    CommandResult? result = await cmd("npm", new[] {"create", "vite@latest", $"{options.Name.ToLower()}-app", "--", "--template", "vue-ts"});// npm init vite@latest my-app -- --template vue-ts
    
    Directory.SetCurrentDirectory($"{options.Name.ToLower()}-app");
    result = await cmd("npm", new[] {"install"});
    result = await cmd("npm", new[] {"install", "axios"});
    result = await cmd("npm", new[] {"install", "vue-router@4"});
    
    // Quasar vite-plugin https://quasar.dev/start/vite-plugin
    result = await cmd("npm", new[] {"install", "quasar", "@quasar/extras"});
    result = await cmd("npm", new[] {"install", "-D", "@quasar/vite-plugin", "sass@1.32.0"});    

    Directory.SetCurrentDirectory(rootDirectory.FullName);

    // Install openapi generator https://openapi-generator.tech/
    // npm install @openapitools/openapi-generator-cli -g
    
    // Fetch specs and generate typescript
    // curl http://localhost:5177/
    // npx @openapitools/openapi-generator-cli generate -i openapi-spec.yaml -g typescript-axios -o generated/
}

void CopyTemplates(DirectoryInfo generatorDirectory, DirectoryInfo rootDirectory, Options options, EnvironmentVariables variables)
{
    foreach (var directory in Directory.GetDirectories(Path.Combine(generatorDirectory.FullName, "templates")))
    {
        if (new DirectoryInfo(directory).Name == "app")
        {
            CopyFilesRecursively(directory, 
                Path.Combine(rootDirectory.FullName, $"{options.Name.ToLower()}-{new DirectoryInfo(directory).Name}"),
                variables);
        }
        else
        {
            if (new DirectoryInfo(directory).Name == "_vscode")
            {
                Directory.CreateDirectory(Path.Combine(rootDirectory.FullName, ".vscode"));
                CopyFilesRecursively(directory, 
                    Path.Combine(rootDirectory.FullName, ".vscode"),
                    variables);
            }
            else
            {
                CopyFilesRecursively(directory, 
                    Path.Combine(rootDirectory.FullName, $"{options.Name}.{new DirectoryInfo(directory).Name}"),
                    variables);
            }
        }
    }
}

// https://stackoverflow.com/questions/58744/copy-the-entire-contents-of-a-directory-in-c-sharp
void CopyFilesRecursively(string sourcePath, string targetPath, EnvironmentVariables variables)
{
    //Now Create all of the directories
    foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
    {
        Console.WriteLine($"Creating directory {dirPath} -> {dirPath.Replace(sourcePath, targetPath).Replace("_vscode", ".vscode")}");
        Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath).Replace("_vscode", ".vscode"));
    }

    //Copy all the files & Replaces any files with the same name
    foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",SearchOption.AllDirectories))
    {
        Console.WriteLine($"Creating file {newPath} -> {newPath.Replace(sourcePath, targetPath).Replace(".template", "").Replace("_vscode", ".vscode")}");
        var data = File.ReadAllText(newPath);
        data = data.Replace("[[NAME]]", variables.Name);
        data = data.Replace("[[DBSERVER]]", variables.DatabaseServer.Replace(@"\", @"\\"));
        data = data.Replace("[[DBNAME]]", variables.DatabaseName);
        data = data.Replace("[[DBUSER]]", variables.DatabaseUser);
        data = data.Replace("[[DBPASS]]", variables.DatabasePassword);
        File.WriteAllText(newPath.Replace(sourcePath, targetPath).Replace(".template", "").Replace("_vscode", ".vscode"), data);
    }
}

void Clean(string name)
{
    if (Directory.Exists(name))
    {
        Directory.Delete(name, true);
    }
}
