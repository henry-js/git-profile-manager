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

    public async Task<string> ReadFileAsync()
    {
        if (FileExists())
        {
            return await File.ReadAllTextAsync(path);
        }
        throw new FileNotFoundException(path);
    }

    public async Task WriteFileAsync(string contents)
    {
        await File.WriteAllTextAsync(path, contents);
    }
}