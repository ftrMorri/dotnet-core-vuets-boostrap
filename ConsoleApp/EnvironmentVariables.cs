using CommandLine;

namespace Bootstrapper
{
    public class EnvironmentVariables
    {
        public string Name { get; set; } = default!;
        public string DatabaseServer { get; set; } = default!;
        public string DatabaseName { get; set; } = default!;
        public string DatabaseUser { get; set; } = default!;
        public string DatabasePassword { get; set; } = default!;
        public string DatabaseConnectionString { get; set; } = default!;
        public string EntityFrameworkUsingOption { get; set; } = default!;
        public string EntityFrameworkDefaultSchema { get; set; } = default!;
        public string EntityFrameworkIdentityNamingConventions { get; set; } = default!;
    }
}