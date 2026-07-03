using System.Text.Json;

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

    public (int, int, int, int) RGBaFromHex(string hexCode)
    {
        hexCode = hexCode.TrimStart('#');
        int parts = hexCode.Length / 2;
        switch (parts)
        {
            case 3:
                return (
                    ParseHex(hexCode.Substring(0, 2)),
                    ParseHex(hexCode.Substring(2, 2)),
                    ParseHex(hexCode.Substring(4, 2)),
                    255
                );
            case 4:
                return (
                    ParseHex(hexCode.Substring(0, 2)),
                    ParseHex(hexCode.Substring(2, 2)),
                    ParseHex(hexCode.Substring(4, 2)),
                    ParseHex(hexCode.Substring(6, 2))
                );
            default:
                return (135, 206, 235, 255);
        }
    }

    private int ParseHex(string hexPart) => Convert.ToInt32(hexPart, 16);
}

public class Screen
{
    public int Duration { get; set; }
    public string BackgroundColor { get; set; } = string.Empty;
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
    public string Align { get; set; } = string.Empty;
    public string VAlign { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
