namespace jxlgui.converter;

public class Constants
{
    public static string[] ExtensionsEncode = {".png", ".apng", ".gif", ".jpeg", ".ppm", ".pfm", ".pgx"};
    public static string[] ExtensionsDecode = {".jxl"};

    public static string[] Extensions = ExtensionsEncode.Concat(ExtensionsDecode).ToArray();

    public static string AppFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "jxlgui");


    /// <summary>
    ///     JPEG XL decoder
    /// </summary>
    public static string DecoderFilePath => Path.Combine(AppFolder, "djxl.exe");

    /// <summary>
    ///     JPEG XL encoder
    /// </summary>
    public static string EncoderFilePath => Path.Combine(AppFolder, "cjxl.exe");

    public static string ConfigPath => Path.Combine(AppFolder, "config.json");
}