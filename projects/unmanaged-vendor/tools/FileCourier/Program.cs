using FileCourier;
using FileCourier.Cli;
using FileCourier.Manifest;
using FileCourier.Processing;

return await FileCourierCli.InvokeAsync(args, ExecuteAsync);

static async Task<int> ExecuteAsync(FileCourierCliRequest request)
{
    try
    {
        var cliContext = CliArgumentsAnalyzer.Analyze(request.ReadWorkingDirectory, request.WriteWorkingDirectory, request.ConfigurationPath);
        var manifest = FileCourierConfigurationLoader.Load(cliContext.ConfigurationPath);
        var executionPlan = FileCourierExecutionPlanBuilder.Build(manifest, cliContext.ReadRoot, cliContext.WriteRoot);

        var totalTransfers = executionPlan.Transfers.Count;
        var completedTransfers = 0;

        await foreach (var transfer in FileCourierPlanExecutor.ExecuteAsync(executionPlan))
        {
            completedTransfers++;
            await Console.Out.WriteLineAsync
            (
                $"[{completedTransfers}/{totalTransfers}] {transfer.SourcePath} -> {transfer.DestinationPath}"
            );
        }

        await Console.Out.WriteLineAsync($"Copied {completedTransfers} files.");

        return 0;
    }
    catch (FileCourierException ex)
    {
        await Console.Error.WriteLineAsync(ex.ToDisplayString());
        return 2;
    }
}
