using CommandLine;
using Spectre.Console;
using System.Xml.Linq;

namespace Bootstrapper
{
    public class InteractiveOptions
    {
        public static Options Ask()
        {
            var options = new Options();
            
            options.Path = AnsiConsole.Ask<string>("[green]Path[/] where you want to create your application?", "./");
            options.Name = AnsiConsole.Ask<string>("What is your projects [green]name[/]?");
            options.DatabaseServer = AnsiConsole.Ask<string>("What is your databases [green]address[/]?", ".\\SQLEXPRESS");
            options.DatabaseUser = AnsiConsole.Ask<string>("Application database [green]username[/]?");
            options.DatabasePassword = AnsiConsole.Ask<string>("Application database users [green]password[/]?");

            options.DatabaseProvider = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What [green]database[/] are you using?")
                    .PageSize(5)
                    .AddChoices(new[] {"sqlserver", "postgresql"}));

            if (options.DatabaseProvider == "postgresql")
            {
                options.DatabaseAdmin = AnsiConsole.Ask<string>("What is the {green]admin username[/] in your postgresql installation?");
                options.DatabaseAdminPassword = AnsiConsole.Ask<string>("What is the [green]admin password[/] in your postgresql installation?");
            }

            options.Install = AnsiConsole.Ask<string>("Do you want to start the installation?", "y") == "y"
                ? true
                : false;

            return options;
        }
    }
}