using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace EarTrumpet.Tests;

// Guards the "no hardcoded text" rule for the settings UI: every resource XAML points at must exist,
// and the strings this redesign introduced must be translated for the two supported languages.
public class LocalizationTests
{
    private static readonly string s_project = FindProjectDir();

    private static string FindProjectDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "EarTrumpet", "EarTrumpet.csproj")))
        {
            dir = dir.Parent;
        }
        return Path.Combine(dir?.FullName ?? throw new InvalidOperationException("repo root not found"), "EarTrumpet");
    }

    private static string[] Keys(string resx)
        => XDocument.Load(Path.Combine(s_project, "Properties", resx)).Root.Elements("data").Select(e => (string)e.Attribute("name")).ToArray();

    private static string[] XamlResourceRefs()
        => Directory.GetFiles(s_project, "*.xaml", SearchOption.AllDirectories)
            .Where(f => !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
            .SelectMany(f => Regex.Matches(File.ReadAllText(f), @"resx:Resources\.(\w+)").Select(m => m.Groups[1].Value))
            .Distinct().ToArray();

    [Fact]
    public void EveryXamlResourceExistsInDefaultResx()
    {
        var keys = Keys("Resources.resx");
        Assert.Empty(XamlResourceRefs().Except(keys));
    }

    [Fact]
    public void EveryDesignerPropertyExistsInDefaultResx()
    {
        var designer = File.ReadAllText(Path.Combine(s_project, "Properties", "Resources.Designer.cs"));
        var props = Regex.Matches(designer, @"public static string (\w+) \{").Select(m => m.Groups[1].Value);
        Assert.Empty(props.Except(Keys("Resources.resx")));
    }

    [Fact]
    public void SettingsRedesignStringsAreTranslatedToSpanish()
    {
        var es = Keys("Resources.es-ES.resx");
        var redesign = Keys("Resources.resx").Where(k =>
            k == "GeneralSettingsPageText" || k.StartsWith("SettingsSection") ||
            (k.StartsWith("Settings") && (k.EndsWith("Description") || k.Contains("Volume") || k.Contains("Snap") ||
                                          k.Contains("ScrollWheel") || k.Contains("Translucent") || k.Contains("Legacy")))).ToArray();
        Assert.NotEmpty(redesign);
        Assert.Empty(redesign.Except(es));
    }
}
