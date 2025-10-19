using FileCourier;
using FileCourier.Cli;
using FileCourier.Manifest;
using FileCourier.Processing;

return await CourierCli.InvokeAsync(args, ExecuteAsync);

static async Task<int> ExecuteAsync(CliRequest request)
{
    try
    {
        var cliContext = CliArgumentsAnalyzer.Analyze(request.ReadWorkingDirectory, request.WriteWorkingDirectory, request.ManifestPath);
        var manifest = ManifestLoader.Load(cliContext.ManifestPath);
        var executionPlan = ExecutionPlanBuilder.Build(manifest, cliContext.ReadRoot, cliContext.WriteRoot);

        var totalTransfers = executionPlan.Transfers.Count;
        var completedTransfers = 0;

        await foreach (var transfer in PlanExecutor.ExecuteAsync(executionPlan))
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
    catch (CourierException ex)
    {
        await Console.Error.WriteLineAsync(ex.ToDisplayString());
        return 2;
    }
}
