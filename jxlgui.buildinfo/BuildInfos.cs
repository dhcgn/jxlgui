using System.Reflection;

namespace jxlgui.buildinfo;

public class BuildInfos
{
    private BuildInfos()
    {
        this.Version = GetStringFromFile("version");
        this.CommitId = GetStringFromFile("commitid");
        // core.abbrev configuration variable (see git-config[1]).
        // int minimum_abbrev = 4, default_abbrev = 7;
        this.CommitIdShort = GetStringFromFile("commitid").Substring(0, 20);
        this.Date = GetStringFromFile("date");
    }

    public string Version { get; set; }
    public string CommitId { get; set; }
    public string CommitIdShort { get; set; }
    public string Date { get; set; }

    public static BuildInfos Get()
    {
        return new BuildInfos();
    }

    private static string GetStringFromFile(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var name = assembly.GetManifestResourceNames().First(n => n.EndsWith(resourceName));
        using (var resource = assembly.GetManifestResourceStream(name))
        {
            using (var reader = new StreamReader(resource))
            {
                var text = reader.ReadToEnd();
                text = text.Trim();
                return text;
            }
        }
    }
}