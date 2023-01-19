using CliWrap;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Orchestration
{
    public class StepManager
    {
        private List<Step> steps = new List<Step>();

        public async IAsyncEnumerable<string> Run()
        {
            int commandStepTotalCount = steps.Count(s => s.Type == StepType.Command);
            int currentCommandStepCount = 0;

            foreach (var step in steps)
            {
                if (step.Type == StepType.Command 
                    && step.StepFunction != null
                    && step.StepArguments != null)
                {
                    await step.StepFunction(step.StepCommand, step.StepArguments);
                    currentCommandStepCount++;
                    yield return $"[{currentCommandStepCount} / {commandStepTotalCount}]";
                }
                if (step.Type == StepType.SwitchDirectory && !string.IsNullOrEmpty(step.NewDirectory))
                {
                    Directory.SetCurrentDirectory(step.NewDirectory);
                    yield return $"[{currentCommandStepCount} / {commandStepTotalCount}]";
                }
            }
        }

        public void AddCommandStep(Func<string, string[], CommandTask<CommandResult>> func,
            string command,
            string[] commandArguments)
        {
            steps.Add(new Step()
            {
                StepFunction = func,
                StepCommand = command,
                StepArguments = commandArguments
            });
        }

        public void AddDirectoryChangeStep(string newDirectory)
        {
            steps.Add(new Step()
            {
                NewDirectory = newDirectory,
            });
        }
    }
}
