namespace FileCourier.Cli;

internal sealed record CliRequest(string ReadWorkingDirectory, string WriteWorkingDirectory, string ManifestPath);
