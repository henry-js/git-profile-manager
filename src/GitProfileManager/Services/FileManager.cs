using GitProfileManager.Lib.Services;

namespace GitProfileManager.Services;

internal class FileManager : IFileSystem
{
    private readonly string path;

    public FileManager(string path)
    {
        this.path = path;
    }
    public bool FileExists()
    {
        var fileInfo = new FileInfo(path);
        return fileInfo.Exists;
    }

    public async Task<FileInfo> GetProfileFile()
    {
        var file = new FileInfo(path);
        if (!file.Exists)
        {
            var dir = file.Directory?.FullName ?? throw new DirectoryNotFoundException();
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            await using var s = file.Create();
            await s.FlushAsync();
            await s.DisposeAsync();
        }
        file.Refresh();
        return file;
    }

    public Task<string> ReadFileAsync()
    {
        throw new NotImplementedException();
    }

    public Task WriteFileAsync(string content)
    {
        throw new NotImplementedException();
    }
}