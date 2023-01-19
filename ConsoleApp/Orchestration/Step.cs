using CliWrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Orchestration
{
    public class Step
    {
        public Func<string, string[], CommandTask<CommandResult>>? StepFunction { get; set; }
        public string NewDirectory { get; set; } = string.Empty;

        public StepType Type
        {
            get
            {
                return this.StepFunction != null
                    ? StepType.Command
                    : StepType.SwitchDirectory;
            }
        }

        public string StepCommand { get; set; } = string.Empty;
        public string[]? StepArguments { get; set; }
    }

    public enum StepType
    {
        Command,
        SwitchDirectory
    }
}
