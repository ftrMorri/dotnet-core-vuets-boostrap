using CommandLine;

namespace StarterTool
{
    public class Options
    {
        [Option('i', "install", Required = true, HelpText = "Creates new development environment.")]
        public bool Install { get; set; }

        [Option('n', "name", Required = true, HelpText = "Solution name.")]
        public string Name { get; set; } = default!;

        [Option('s', "dbserver", Required = true, HelpText = "Database server required. eg .\\SQLEXPRESS")]
        public string DatabaseServer { get; set; } = default!;

        [Option('d', "dbname", Required = true, HelpText = "Database name required. eg WeatherForecast")]
        public string DatabaseName { get; set; } = default!;

        [Option('u', "dbuser", Required = true, HelpText = "Database user required.")]
        public string DatabaseUser { get; set; } = default!;

        [Option('p', "dbpass", Required = true, HelpText = "Database user password required. eg .\\SQLEXPRESS")]
        public string DatabasePassword { get; set; } = default!;

        [Option('c', "clean", Required = false, HelpText = "Clean existing folder if exists.")]
        public bool Clean { get; set; }

       [Option('l', "path", Required = true, HelpText = "Directory path for solution. eg ..\\MyApp")]
        public string Path { get; set; } = default!;
    }
}