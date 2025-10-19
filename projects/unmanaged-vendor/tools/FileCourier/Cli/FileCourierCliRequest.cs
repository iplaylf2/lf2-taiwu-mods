namespace FileCourier.Cli;

internal sealed record FileCourierCliRequest(string ReadWorkingDirectory, string WriteWorkingDirectory, string ConfigurationPath);
