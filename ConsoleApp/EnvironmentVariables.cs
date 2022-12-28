using CommandLine;

namespace StarterTool
{
    public class EnvironmentVariables
    {
        public string Name { get; set; } = default!;
        public string DatabaseServer { get; set; } = default!;
        public string DatabaseName { get; set; } = default!;
        public string DatabaseUser { get; set; } = default!;
        public string DatabasePassword { get; set; } = default!;
    }
}