using System.Reflection;

namespace jxlgui.buildinfo;
public class BuildInfos
{
    public static BuildInfos Get()
    {
        return new BuildInfos();
    }

    private BuildInfos()
    {
        Version = GetStringFromFile("version");
        CommitId = GetStringFromFile("commitid");
        // core.abbrev configuration variable (see git-config[1]).
        // int minimum_abbrev = 4, default_abbrev = 7;
        CommitIdShort = GetStringFromFile("commitid").Substring(0, 20);
        Date = GetStringFromFile("date");
    }

    public string Version { get; set; }
    public string CommitId { get; set; }
    public string CommitIdShort { get; set; }
    public string Date { get; set; }

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
