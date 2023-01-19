using CommandLine;

namespace Bootstrapper
{
    public class Options
    {
        [Option("install", Required = false, HelpText = "Creates new development environment.")]
        public bool Install { get; set; }

        [Option("help", Required = false, HelpText = "Displays command line parameters.")]
        public bool Help { get; set; }

        [Option("name", Required = false, HelpText = "Solution name.")]
        public string Name { get; set; } = default!;

        [Option("dbprovider", Required = false, HelpText = "Database provider. eg 'sqlsever', 'postgresql', 'sqlite'")]
        public string DatabaseProvider { get; set; } = default!;

        [Option("dbserver", Required = false, HelpText = "Database server required. eg .\\SQLEXPRESS")]
        public string DatabaseServer { get; set; } = default!;

        [Option("dbname", Required = false, HelpText = "Database name required. eg WeatherForecast")]
        public string DatabaseName { get; set; } = default!;

        [Option("dbuser", Required = false, HelpText = "Database user required.")]
        public string DatabaseUser { get; set; } = default!;

        [Option("dbpass", Required = false, HelpText = "Database user password required. eg .\\SQLEXPRESS")]
        public string DatabasePassword { get; set; } = default!;

       [Option("dbadmin", Required = false, HelpText = "Username for database admin required.")]
        public string DatabaseAdmin { get; set; } = default!;

        [Option("dbadminpass", Required = false, HelpText = "Database admin password required.")]
        public string DatabaseAdminPassword { get; set; } = default!;

        [Option("clean", Required = false, HelpText = "Clean existing folder if exists.")]
        public bool Clean { get; set; }

       [Option("path", Required = false, HelpText = "Directory path for solution. eg ..\\MyApp")]
        public string Path { get; set; } = default!;
    }
}