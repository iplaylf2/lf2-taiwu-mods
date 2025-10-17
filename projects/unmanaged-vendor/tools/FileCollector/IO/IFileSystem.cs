using System.Collections.Generic;
using System.IO;

namespace FileCollector.IO;

public interface IFileSystem
{
    bool DirectoryExists(string path);

    void EnsureDirectory(string path);

    bool FileExists(string path);

    void CopyFile(string sourceFileName, string destinationFileName, bool overwrite);

    Stream OpenRead(string path);

    IEnumerable<string> EnumerateFileSystemEntries(string path);
}
