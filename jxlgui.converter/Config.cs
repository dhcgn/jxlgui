using System.Text.Json;

namespace jxlgui.converter;

public class Config
{
    // Quality setting (is remapped to --distance). Range: -inf .. 100.
    // 100 = mathematically lossless. Default for already-lossy input (JPEG/GIF).
    // Positive quality values roughly match libjpeg quality.
    public double? Quality { get; set; }

    // Encoder effort setting. Range: 1 .. 9.
    // Default: 7. Higher number is more effort (slower)    
    public int Effort { get; set; }

  //  The distance level for lossy compression: target max butteraugli
  //  distance, lower = higher quality. Range: 0 .. 15.
  //  0.0 = mathematically lossless (however, use JxlEncoderOptionsSetLossless to use true lossless).
  //  1.0 = visually lossless.
  //  Recommended range: 0.5 .. 3.0.
  //  Default value: 1.0.
  //  If JxlEncoderOptionsSetLossless is used, this value is unused and implied to be 0
    public double? Distance { get; set; }

    public static Config CreateEmpty()
    {
        return new Config
        {
            Quality = 99.9,
            Effort = 7,
            Distance = null,
        };
    }

    public string ToJson()
    {
        var jsonConfig = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var jsonString = JsonSerializer.Serialize(this, jsonConfig);
        return jsonString;
    }

    public static bool IsJsonValid(string json)
    {
        try
        {
            var settings = JsonSerializer.Deserialize<jxlgui.converter.Config>(json);
            return settings != null;
        }
        catch (System.Exception)
        {
            return false;
        }
    }


    public static Config LoadOrCreateNew()
    {
        if (File.Exists(Constants.ConfigPath))
        {
            return Load();
        }

        var config = CreateEmpty();
        Save(config);
        return config;
    }

    public static void Save(Config config)
    {
        File.WriteAllText(Constants.ConfigPath, config.ToJson());
    }

    public static Config? Load()
    {
        var json = File.ReadAllText(Constants.ConfigPath);

        try
        {
            var config = JsonSerializer.Deserialize<jxlgui.converter.Config>(json);
            return config;
        }
        catch (System.Exception)
        {
            return null;
        }
    }
}