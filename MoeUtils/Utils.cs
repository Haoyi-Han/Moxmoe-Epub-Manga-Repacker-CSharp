using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace MoxmoeApp.MoeUtils;

public class Utils
{
    public static string GetCurrentTimeFormat()
    {
        var now = DateTime.Now;
        return now.ToString("HH:mm:ss");
    }

    public static string ComicNameDecorator(string comicName)
    {
        return comicName.Replace("[", "[[").Replace("]", "]]");
    }
}

public class IniReader
{
    public IniReader(string path)
    {
        Path = path;
    }

    private string Path { get; }

    [DllImport("kernel32.dll")]
    private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal,
        int size, string filePath);

    public string Read(string section, string key)
    {
        const int size = 255;
        var retVal = new StringBuilder(size);
        GetPrivateProfileString(section, key, "", retVal, size, Path);
        return retVal.ToString();
    }
}

public class XmlReader
{
    private static Tuple<XmlDocument, XmlNamespaceManager> ReadXmlFile(string xmlFile)
    {
        var doc = new XmlDocument();
        doc.Load(xmlFile);
        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        return new Tuple<XmlDocument, XmlNamespaceManager>(doc, nsmgr);
    }

    public static Tuple<XmlDocument, XmlNamespaceManager> ReadOpfFile(string opfFile)
    {
        var (doc, nsmgr) = ReadXmlFile(opfFile);
        nsmgr.AddNamespace("ns", "http://www.idpf.org/2007/opf");
        return new Tuple<XmlDocument, XmlNamespaceManager>(doc, nsmgr);
    }

    public static Tuple<XmlDocument, XmlNamespaceManager> ReadHtmlFile(string htmlFile)
    {
        var (doc, nsmgr) = ReadXmlFile(htmlFile);
        nsmgr.AddNamespace("ns", "http://www.w3.org/1999/xhtml");
        return new Tuple<XmlDocument, XmlNamespaceManager>(doc, nsmgr);
    }
}