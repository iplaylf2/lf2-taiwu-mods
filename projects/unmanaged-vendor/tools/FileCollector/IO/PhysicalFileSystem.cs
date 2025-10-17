using System;
using System.Collections.Generic;
using System.IO;

namespace FileCollector.IO;

public sealed class PhysicalFileSystem : IFileSystem
{
    public bool DirectoryExists(string path)
    {
        return Directory.Exists(EnsurePathProvided(path, nameof(path)));
    }

    public void EnsureDirectory(string path)
    {
        Directory.CreateDirectory(EnsurePathProvided(path, nameof(path)));
    }

    public bool FileExists(string path)
    {
        return File.Exists(EnsurePathProvided(path, nameof(path)));
    }

    public void CopyFile(string sourceFileName, string destinationFileName, bool overwrite)
    {
        File.Copy(
            EnsurePathProvided(sourceFileName, nameof(sourceFileName)),
            EnsurePathProvided(destinationFileName, nameof(destinationFileName)),
            overwrite);
    }

    public Stream OpenRead(string path)
    {
        return File.OpenRead(EnsurePathProvided(path, nameof(path)));
    }

    public IEnumerable<string> EnumerateFileSystemEntries(string path)
    {
        return Directory.EnumerateFileSystemEntries(EnsurePathProvided(path, nameof(path)));
    }

    private static string EnsurePathProvided(string? path, string parameterName)
    {
        if (path is null)
        {
            throw new ArgumentNullException(parameterName);
        }

        return path;
    }
}
