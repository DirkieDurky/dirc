namespace Dirc;

public static class Helper
{
    public static string GetRelativePath(string path)
    {
        Uri pathUri = new(Path.GetFullPath(path));
        Uri folderUri = new(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar);
        return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
    }

    public static bool FilesMatch(string a, string b)
    {
        return a == b || Path.GetFileName(a) == Path.GetFileName(b);
    }
}
