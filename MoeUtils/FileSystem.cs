namespace MoxmoeApp.MoeUtils;

public class FileSystem
{
    public static void CopyDirectoryStructure(string inDir, string outDir, List<string> exclude)
    {
        var directories = Directory.GetDirectories(inDir, "*", SearchOption.AllDirectories)
            .Where(d => !exclude.Any(d.Contains))
            .ToArray();
        foreach (var directory in directories) Directory.CreateDirectory(directory.Replace(inDir, outDir));
    }

    public static List<string> ScanFilesInDirectory(string dirPath, string ext = "*")
    {
        var fileList = new List<string>();
        try
        {
            fileList.AddRange(Directory.GetFiles(dirPath, $"*.{ext}", SearchOption.AllDirectories));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return fileList;
    }

    public static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path)) Directory.Delete(path, true);
    }

    private static string MoveFile(string oldPath, string newPath)
    {
        File.Move(oldPath, newPath);
        return newPath;
    }

    private static string MoveDirectory(string oldPath, string newPath)
    {
        Directory.Move(oldPath, newPath);
        return newPath;
    }

    public static string RenameFile(string oldPath, string newName)
    {
        var newPath = Path.Combine(Path.GetDirectoryName(oldPath)!, newName);
        return MoveFile(oldPath, newPath);
    }

    public static string RenameDirectory(string oldPath, string newName)
    {
        var newPath = Path.Combine(Path.GetDirectoryName(oldPath)!, newName);
        return MoveDirectory(oldPath, newPath);
    }
}