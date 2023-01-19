using CliWrap;
using CommandLine;
using Bootstrapper;
using Spectre.Console;

Options? options = null;

Parser.Default.ParseArguments<Options>(args)
    .WithParsed<Options>(o => options = o);

if (string.IsNullOrEmpty(options?.Name))
{
    options = InteractiveOptions.Ask();
}

if (!options.Install)
{
    return;
}

var generatorDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

var rootDirectory = Directory.CreateDirectory(Path.Combine(options.Path, options.Name));

Directory.SetCurrentDirectory(rootDirectory.FullName);

await ApplicationFactories.Run(rootDirectory, generatorDirectory, options);