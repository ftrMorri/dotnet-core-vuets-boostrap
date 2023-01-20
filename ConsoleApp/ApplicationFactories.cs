using CliWrap;
using CommandLine;
using ConsoleApp.Orchestration;
using Spectre.Console;

namespace Bootstrapper
{
    public class ApplicationFactories
    {
        public async static Task Run(DirectoryInfo rootDirectory, DirectoryInfo generatorDirectory, Options options)
        {
            await using var stdOut = Console.OpenStandardOutput();
            await using var stdErr = Console.OpenStandardError();
            await using var stdIn = Console.OpenStandardInput();
            {
                Func<string, string[], CommandTask<CommandResult>> cmd = (string command, string[] arguments) => {
                    return Cli.Wrap(command)
                        .WithArguments(arguments)
                        // .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
                        .WithStandardErrorPipe(PipeTarget.ToStream(stdErr))
                        .WithStandardInputPipe(PipeSource.FromStream(stdIn))
                        .ExecuteAsync();
                };

                await ApplicationFactories.CreateVueApp(rootDirectory, options, cmd);
                await ApplicationFactories.CreateCsharpApp(rootDirectory, options, cmd);

                var databaseConfigurator = new DatabaseConfigurator(options.DatabaseProvider,
                    options.DatabaseServer,
                    options.DatabaseName,
                    options.DatabaseUser,
                    options.DatabasePassword);

                var variables = new EnvironmentVariables
                {
                    Name = options.Name,
                    DatabaseServer = options.DatabaseServer,
                    DatabaseName = options.DatabaseName,
                    DatabaseUser = options.DatabaseUser,
                    DatabasePassword = options.DatabasePassword,
                    DatabaseConnectionString = databaseConfigurator.ConnectionString,
                    EntityFrameworkUsingOption = databaseConfigurator.EntityFrameworkUsingOption,
                    EntityFrameworkDefaultSchema = databaseConfigurator.EntityFrameworkDefaultSchema,
                    EntityFrameworkIdentityNamingConventions = databaseConfigurator.EntityFrameworkIdentityNamingConventions,
                };

                CopyTemplates(generatorDirectory, rootDirectory, options, variables);

                await cmd("dotnet", new[] { "build", $"{options.Name}.sln" });

                await ApplicationFactories.CreateDatabase(rootDirectory, options, cmd);
            }
        }

        public async static Task CreateVueApp(DirectoryInfo rootDirectory, Options options, Func<string, string[], CommandTask<CommandResult>> cmd)
        {
            var stepManager = new StepManager();

            // npm init vite@latest my-app -- --template vue-ts
            stepManager.AddCommandStep(cmd, "npm", new[] { "create", "vite@latest", $"{options.Name.ToLower()}-app", "--", "--template", "vue-ts" });
            stepManager.AddDirectoryChangeStep($"{options.Name.ToLower()}-app");

            stepManager.AddCommandStep(cmd, "npm", new[] { "install" });
            stepManager.AddCommandStep(cmd, "npm", new[] { "install", "axios" });
            stepManager.AddCommandStep(cmd, "npm", new[] { "install", "vue-router@4" });

            // Quasar vite-plugin https://quasar.dev/start/vite-plugin
            stepManager.AddCommandStep(cmd, "npm", new[] { "install", "quasar", "@quasar/extras" });
            stepManager.AddCommandStep(cmd, "npm", new[] { "install", "-D", "@quasar/vite-plugin", "sass@1.32.0" });

            stepManager.AddDirectoryChangeStep(rootDirectory.FullName);

            await AnsiConsole.Status()
                .StartAsync("Installing NPM packages", async ctx =>
                {
                    await foreach (var status in stepManager.Run())
                    {
                        ctx.Status($"Installing NPM packages [green]{Markup.Escape(status)}[/]");
                    }
                });

            // Install openapi generator https://openapi-generator.tech/
            // npm install @openapitools/openapi-generator-cli -g

            // Fetch specs and generate typescript
            // curl http://localhost:5177/
            // npx @openapitools/openapi-generator-cli generate -i openapi-spec.yaml -g typescript-axios -o generated/
        }

        public async static Task CreateCsharpApp(DirectoryInfo rootDirectory, Options options, Func<string, string[], CommandTask<CommandResult>> cmd)
        {
            var stepManager = new StepManager();

            stepManager.AddCommandStep(cmd, "dotnet", new[] { "new", "sln", "--name", options.Name });

            stepManager.AddCommandStep(cmd, "dotnet", new[] { "new", "webapi", "--name", options.Name + ".Site" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "new", "classlib", "--name", options.Name + ".Services" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "new", "classlib", "--name", options.Name + ".Data" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "new", "mstest", "--name", options.Name + ".Tests" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "sln", "add", $"{options.Name}.Site/{options.Name}.Site.csproj" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "sln", "add", $"{options.Name}.Services/{options.Name}.Services.csproj" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "sln", "add", $"{options.Name}.Data/{options.Name}.Data.csproj" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "sln", "add", $"{options.Name}.Tests/{options.Name}.Tests.csproj" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", $"{options.Name}.Site/{options.Name}.Site.csproj", "reference", $"{options.Name}.Services/{options.Name}.Services.csproj" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", $"{options.Name}.Site/{options.Name}.Site.csproj", "reference", $"{options.Name}.Data/{options.Name}.Data.csproj" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", $"{options.Name}.Tests/{options.Name}.Tests.csproj", "reference", $"{options.Name}.Services/{options.Name}.Services.csproj" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", $"{options.Name}.Tests/{options.Name}.Tests.csproj", "reference", $"{options.Name}.Data/{options.Name}.Data.csproj" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", $"{options.Name}.Services/{options.Name}.Services.csproj", "reference", $"{options.Name}.Data/{options.Name}.Data.csproj" });

            stepManager.AddDirectoryChangeStep($"{options.Name}.Site");

            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.AspNetCore.SpaServices.Extensions", "-v", "7.0.2" });

            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.EntityFrameworkCore", "-v", "7.0.2" });

            if (options.DatabaseProvider == "sqlserver")
            {
                stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.EntityFrameworkCore.SqlServer", "-v", "7.0.2" });
            }
            if (options.DatabaseProvider == "postgresql")
            {
                stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Npgsql.EntityFrameworkCore.PostgreSQL", "-v", "7.0.2" });
                stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "EFCore.NamingConventions", "-v", "7.0.2" });
            }

            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.EntityFrameworkCore.Tools", "-v", "7.0.2" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.EntityFrameworkCore.Design", "-v", "7.0.2" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.AspNetCore.Identity", "-v", "2.2.0" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.AspNetCore.Identity.EntityFrameworkCore", "-v", "7.0.2" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.AspNetCore.Authentication.JwtBearer", "-v", "7.0.2" });

            stepManager.AddDirectoryChangeStep($"../{options.Name}.Services");

            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.EntityFrameworkCore", "-v", "7.0.2" });
            if (options.DatabaseProvider == "sqlserver")
                stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.EntityFrameworkCore.SqlServer", "-v", "7.0.2" });
            
            if (options.DatabaseProvider == "postgresql")
            {
                stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Npgsql.EntityFrameworkCore.PostgreSQL", "-v", "7.0.2" });
                stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "EFCore.NamingConventions", "-v", "7.0.2" });
            }

            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.AspNetCore.Identity", "-v", "2.2.0" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.AspNetCore.Identity.EntityFrameworkCore", "-v", "7.0.2" });
            stepManager.AddDirectoryChangeStep($"../{options.Name}.Data");

            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.EntityFrameworkCore", "-v", "7.0.2" });
            if (options.DatabaseProvider == "sqlserver")
                stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.EntityFrameworkCore.SqlServer", "-v", "7.0.2" });
            if (options.DatabaseProvider == "postgresql")
            {
                stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Npgsql.EntityFrameworkCore.PostgreSQL", "-v", "7.0.2" });
                stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "EFCore.NamingConventions", "-v", "7.0.2" });
            }

            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.AspNetCore.Identity", "-v", "2.2.0" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.AspNetCore.Identity.EntityFrameworkCore", "-v", "7.0.2" });
            stepManager.AddDirectoryChangeStep($"../{options.Name}.Tests");
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.EntityFrameworkCore", "-v", "7.0.2" });

            if (options.DatabaseProvider == "sqlserver")
                stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.EntityFrameworkCore.SqlServer", "-v", "7.0.2" });
            if (options.DatabaseProvider == "postgresql")
            {
                stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Npgsql.EntityFrameworkCore.PostgreSQL", "-v", "7.0.2" });
                stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "EFCore.NamingConventions", "-v", "7.0.2" });
            }
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.AspNetCore.Identity", "-v", "2.2.0" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "add", "package", "Microsoft.AspNetCore.Identity.EntityFrameworkCore", "-v", "7.0.2" });

            stepManager.AddDirectoryChangeStep(rootDirectory.FullName);

            await AnsiConsole.Status()
                .StartAsync("Installing dotnet packages", async ctx =>
                {
                    await foreach (var status in stepManager.Run())
                    {
                        ctx.Status($"Installing dotnet packages [green]{Markup.Escape(status)}[/]");
                    }
                });
        }

        public async static Task CreateDatabase(DirectoryInfo rootDirectory, Options options, Func<string, string[], CommandTask<CommandResult>> cmd)
        {
            StepManager stepManager = new StepManager();

            stepManager.AddDirectoryChangeStep(rootDirectory.FullName);

            switch (options.DatabaseProvider)
            {
                case "sqlserver":
                    stepManager.AddCommandStep(cmd, "sqlcmd",
                        new[] {"-S",
                        options.DatabaseServer,
                        "-i",
                        Path.Combine($"{options.Name}.Data", "sqlserver-generate.sql")});
                    break;
                case "postgresql":
                    stepManager.AddCommandStep(cmd, "psql",
                        new[] {"-d",
                        $"postgres://{options.DatabaseAdmin}:{options.DatabaseAdminPassword}@localhost",
                        "-a",
                        "-f",
                        Path.Combine($"{options.Name}.Data", "postgresql-generate.sql")});
                    break;
                case "sqlite":
                    throw new NotImplementedException("sqlite not implemented.");
                default:
                    throw new Exception($"Unknown database provider '{options.DatabaseProvider}'");
            }

            stepManager.AddCommandStep(cmd, "dotnet", new[] { "ef", "migrations", "add", "InitialCreate", $"--project", $"{options.Name}.Data", $"--startup-project", $"{options.Name}.Site" });
            stepManager.AddCommandStep(cmd, "dotnet", new[] { "ef", "database", "update", $"--project", $"{options.Name}.Data", $"--startup-project", $"{options.Name}.Site" });

            await AnsiConsole.Status()
                .StartAsync("Installing database", async ctx =>
                {
                    await foreach (var status in stepManager.Run())
                    {
                        ctx.Status($"Installing database [green]{Markup.Escape(status)}[/]");
                    }
                });
        }

        public static void CopyTemplates(DirectoryInfo generatorDirectory, DirectoryInfo rootDirectory, Options options, EnvironmentVariables variables)
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
        public static void CopyFilesRecursively(string sourcePath, string targetPath, EnvironmentVariables variables)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                // Console.WriteLine($"Creating directory {dirPath} -> {dirPath.Replace(sourcePath, targetPath).Replace("_vscode", ".vscode")}");
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath).Replace("_vscode", ".vscode"));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                // Console.WriteLine($"Creating file {newPath} -> {newPath.Replace(sourcePath, targetPath).Replace(".template", "").Replace("_vscode", ".vscode")}");
                var data = File.ReadAllText(newPath);
                data = data.Replace("[[NAME]]", variables.Name);
                data = data.Replace("[[DBCONNECTIONSTRING]]", variables.DatabaseConnectionString.Replace(@"\", @"\\"));
                data = data.Replace("[[ENTITYFRAMEWORKUSINGOPTION]]", variables.EntityFrameworkUsingOption); // .Replace(@"\", @"\\"));
                data = data.Replace("[[DBNAME]]", variables.DatabaseName);
                data = data.Replace("[[DBUSER]]", variables.DatabaseUser);
                data = data.Replace("[[DBPASS]]", variables.DatabasePassword);
                data = data.Replace("[[EFUSING]]", variables.EntityFrameworkUsingOption);
                data = data.Replace("[[ONMODELCREATING_DEFAULTSCHEMA]]", variables.EntityFrameworkDefaultSchema);
                data = data.Replace("[[ONMODELCREATING_IDENTITYNAMINGCONVENTIONS]]", variables.EntityFrameworkIdentityNamingConventions);
                // UseNpgsql
                File.WriteAllText(newPath.Replace(sourcePath, targetPath).Replace(".template", "").Replace("_vscode", ".vscode"), data);
            }
        }

    }
}