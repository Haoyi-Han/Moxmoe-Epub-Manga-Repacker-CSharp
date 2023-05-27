using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml;
using static MoxmoeApp.MoeUtils.TerminalUserInterface;

namespace MoxmoeApp.MoeUtils;

public class SharedInfo
{
    public SharedInfo(string inputDir, string outputDir, string cacheDir)
    {
        InputDir = inputDir;
        OutputDir = outputDir;
        CacheDir = cacheDir;
        FileList = new List<string>();
    }

    public string InputDir { get; }

    public string OutputDir { get; }

    public string CacheDir { get; }

    public List<string> FileList { get; set; }
}

public class Repacker
{
    private SharedInfo SharedInfo { get; set; } = null!;

    public string OutputDir => SharedInfo.OutputDir;

    public string CacheDir => SharedInfo.CacheDir;

    public List<string> FileList => SharedInfo.FileList;

    public void InitConfig(string configPath)
    {
        LogLine("[yellow]开始初始化程序...[/]");
        var cp = new IniReader(configPath);
        var exclude = cp.Read("DEFAULT", "Exclude").Split("||").ToList();
        SharedInfo = new SharedInfo(
            cp.Read("DEFAULT", "InputDir"),
            cp.Read("DEFAULT", "OutputDir"),
            cp.Read("DEFAULT", "CacheDir")
        );
        SharedInfo.FileList = InitPathObj(ref exclude);
    }


    public void Repack(string filePath)
    {
        var comicName = new DirectoryInfo(Path.GetDirectoryName(filePath)!).Name;
        var comicSrc = LoadZipImg(filePath);
        var comicTar = comicName == new DirectoryInfo(SharedInfo.InputDir).Name
            ? SharedInfo.OutputDir
            : Path.Combine(SharedInfo.OutputDir, comicName);
        PackFolder(comicSrc, comicTar);
    }

    private List<string> InitPathObj(ref List<string> exclude)
    {
        Write(PathTable(SharedInfo.InputDir, SharedInfo.OutputDir, SharedInfo.CacheDir));
        FileSystem.DeleteDirectoryIfExists(SharedInfo.OutputDir);
        FileSystem.DeleteDirectoryIfExists(SharedInfo.CacheDir);
        var fileList = FileSystem.ScanFilesInDirectory(SharedInfo.InputDir, "epub");
        LogLine("[green]已完成文件列表抽取。[/]");
        FileSystem.CopyDirectoryStructure(SharedInfo.InputDir, SharedInfo.OutputDir, exclude);
        LogLine("[green]已完成目录结构复制。[/]");
        return fileList;
    }

    private string ComicNameExtract(XmlDocument doc, XmlNamespaceManager nsmgr)
    {
        var root = doc.DocumentElement!;
        var metadata = root.SelectSingleNode("//ns:metadata", nsmgr)!;
        nsmgr.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
        var author = metadata.SelectSingleNode("//dc:creator", nsmgr)!.InnerText;
        var name = metadata.SelectSingleNode("//dc:title", nsmgr)!.InnerText.Replace(" - ", "]");
        return $"[{author}][{name}";
    }

    private Tuple<int, string> HtmlObjExtract(XmlNode currPage, string extractDir, int length)
    {
        var idStr = currPage.Attributes!["id"]!.Value.Replace("Page_", "");
        var fileStem = Regex.Match(currPage.Attributes["href"]!.Value, @"[^/]+\.html").ToString();
        var rawPath = Path.Combine(extractDir, "html", fileStem);
        if (!int.TryParse(idStr, out var id))
            id = idStr == "cover" ? 0 : length;

        return new Tuple<int, string>(id, rawPath);
    }

    private List<string> HtmlExtractToList(string extractDir, XmlDocument doc, XmlNamespaceManager nsmgr)
    {
        var root = doc.DocumentElement!;
        var manifest = root.SelectSingleNode("//ns:manifest", nsmgr)!;
        var rawPages = manifest.SelectNodes("//ns:item[@media-type='application/xhtml+xml']", nsmgr)!;
        var reducedPages = new List<Tuple<int, string>>();
        foreach (XmlNode pg in rawPages) reducedPages.Add(HtmlObjExtract(pg, extractDir, rawPages.Count));

        reducedPages.Sort((x, y) => x.Item1.CompareTo(y.Item1));
        return reducedPages.Select(x => x.Item2).ToList();
    }

    private string LoadZipImg(string zipFile)
    {
        var zipFileStem = Path.GetFileNameWithoutExtension(zipFile);
        LogLine($"[yellow]开始解析 {Utils.ComicNameDecorator(zipFileStem)}[/]");
        var extractDir = Path.Combine(SharedInfo.CacheDir, zipFileStem);
        while (Directory.Exists(extractDir))
            extractDir = Path.Combine(SharedInfo.CacheDir, zipFileStem + "_dup");

        ZipFile.ExtractToDirectory(zipFile, extractDir);
        var (opfDoc, opfNsmgr) = XmlReader.ReadOpfFile(Path.Combine(extractDir, "vol.opf"));
        var comicName = ComicNameExtract(opfDoc, opfNsmgr);
        var comicNameMarkup = Utils.ComicNameDecorator(comicName);

        LogLine($"{comicNameMarkup} => [yellow]开始提取[/]");
        var imgDir = Path.Combine(extractDir, "image");
        var htmlList = HtmlExtractToList(extractDir, opfDoc, opfNsmgr);
        foreach (var htmlFile in htmlList)
        {
            var (htmlDoc, htmlNsmgr) = XmlReader.ReadHtmlFile(htmlFile);
            var title = htmlDoc.SelectSingleNode("//ns:title", htmlNsmgr)!.InnerText;
            var imgSrc = htmlDoc.SelectSingleNode("//ns:img", htmlNsmgr)!.Attributes!["src"]!.Value;
            imgSrc = Path.Combine(imgDir, Path.GetFileName(imgSrc));
            if (Path.GetFileName(imgSrc).Contains("cover"))
            {
                FileSystem.RenameFile(imgSrc, "COVER" + Path.GetExtension(imgSrc));
            }
            else if (title.Contains("END"))
            {
                FileSystem.RenameFile(imgSrc, "THE END" + Path.GetExtension(imgSrc));
            }
            else
            {
                int.TryParse(Regex.Matches(title, @"\d+")[0].ToString(), out var pageNum);
                FileSystem.RenameFile(imgSrc, $"PAGE_{pageNum:D3}" + Path.GetExtension(imgSrc));
            }
        }

        imgDir = FileSystem.RenameDirectory(imgDir, comicName);
        var imgFileList = FileSystem.ScanFilesInDirectory(imgDir);
        foreach (var imgFile in imgFileList)
        {
            var imgStem = Path.GetFileNameWithoutExtension(imgFile);
            if (new[] { "COVER", "END", "PAGE" }.All(s => !imgStem.Contains(s))) File.Delete(imgFile);
        }

        LogLine($"{comicNameMarkup} => [green]提取完成[/]");
        return imgDir;
    }

    private void PackFolder(string inDir, string outDir, string ext = ".cbz")
    {
        var comicName = new DirectoryInfo(inDir).Name;
        var comicNameMarkup = Utils.ComicNameDecorator(comicName);
        LogLine($"{comicNameMarkup} => [yellow]开始打包[/]");
        var cbzFile = Path.Combine(outDir, comicName + ".zip");
        ZipFile.CreateFromDirectory(inDir, cbzFile);
        FileSystem.RenameFile(cbzFile, Path.GetFileNameWithoutExtension(cbzFile) + ext);
        LogLine($"{comicNameMarkup} => [green]打包完成[/]");
    }
}