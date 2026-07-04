using System.Text.Json;
using System.Text.Json.Serialization;
using Raylib_cs;

namespace alumenol;

public class Config
{
    public int Width { get; set; }
    public int Height { get; set; }
    public List<Screen> Screens { get; set; } = new();

    public static Config? Deserialize(string content)
    {
        return JsonSerializer.Deserialize<Config>(content);
    }

    public static Color ColorFromHex(string hexCode)
    {
        hexCode = hexCode.TrimStart('#');
        int parts = hexCode.Length / 2;
        return parts switch
        {
            3 => new Color(
                Convert.ToByte(hexCode.Substring(0, 2), 16),
                Convert.ToByte(hexCode.Substring(2, 2), 16),
                Convert.ToByte(hexCode.Substring(4, 2), 16),
                (byte)255
            ),
            4 => new Color(
                Convert.ToByte(hexCode.Substring(0, 2), 16),
                Convert.ToByte(hexCode.Substring(2, 2), 16),
                Convert.ToByte(hexCode.Substring(4, 2), 16),
                Convert.ToByte(hexCode.Substring(6, 2), 16)
            ),
            _ => Color.SkyBlue,
        };
    }
}

public class Screen
{
    public int Duration { get; set; }
    public string BackgroundColor { get; set; } = string.Empty;

    private Color? _BgColor;

    [JsonIgnore]
    public Color BgColor => _BgColor ??= Config.ColorFromHex(BackgroundColor);
    public Grid Grid { get; set; } = null!;
}

public class Grid
{
    public int Rows { get; set; }
    public int Cols { get; set; }
    public List<Cell> Cells { get; set; } = new();
}

public class Cell
{
    public int RowStart { get; set; }
    public int ColStart { get; set; }
    public int RowSpan { get; set; }
    public int ColSpan { get; set; }
    public string BackgroundColor { get; set; } = string.Empty;

    private Color? _BgColor;

    [JsonIgnore]
    public Color BgColor => _BgColor ??= Config.ColorFromHex(BackgroundColor);
    public bool Border { get; set; } = false;
    public int BorderThickness { get; set; }
    public string BorderColor { get; set; } = string.Empty;

    private Color? _CellBorderColor;

    [JsonIgnore]
    public Color CellBorderColor => _CellBorderColor ??= Config.ColorFromHex(BorderColor);
    public Media? Media { get; set; }
    public Text? Text { get; set; }
}

public class Media
{
    public string Source { get; set; } = string.Empty;
}

public class Text
{
    public string FontFamily { get; set; } = string.Empty;
    public int Size { get; set; }
    public string Color { get; set; } = string.Empty;

    private Color? _FontColor;

    [JsonIgnore]
    public Color FontColor => _FontColor ??= Config.ColorFromHex(Color);
    public string Align { get; set; } = string.Empty;
    public string VAlign { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
