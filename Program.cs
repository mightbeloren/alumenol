using System.Numerics;
using Raylib_cs;

namespace alumenol;

class Program
{
    private static readonly string ConfigPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "config.json"
    );
    private static Dictionary<string, Texture2D> textures = new();
    private static Dictionary<string, Font> fonts = new();
    private static Config config = null!;
    private static double screenTimer;
    private static int screenIndex;
    private static bool isPaused = false;
    private static bool shouldQuit = false;

    private static Dictionary<string, string> GetVariables()
    {
        var now = DateTime.Now;
        return new Dictionary<string, string>()
        {
            { "HH", now.ToString("HH") },
            { "hh", now.ToString("hh") },
            { "mm", now.ToString("mm") },
            { "ss", now.ToString("ss") },
            { "tt", now.ToString("tt") },
            { "yyyy", now.ToString("yyyy") },
            { "yy", now.ToString("yy") },
            { "MM", now.ToString("MM") },
            { "MMM", now.ToString("MMM") },
            { "MMMM", now.ToString("MMMM") },
            { "dd", now.ToString("dd") },
            { "ddd", now.ToString("ddd") },
            { "dddd", now.ToString("dddd") },
            { "Time", now.ToString("HH:mm:ss") },
            { "Time12", now.ToString("hh:mm tt") },
            { "Date", now.ToString("yyyy-MM-dd") },
            { "DateLong", now.ToString("dddd, MMMM d, yyyy") },
            { "DateTime", now.ToString("yyyy-MM-dd HH:mm:ss") },
        };
    }

    static void Main(string[] args)
    {
        //deserialze the config and then use
        if (!File.Exists(ConfigPath))
        {
            Console.Error.WriteLine("Unable to find config.json in the current directory");
            Environment.Exit(1);
        }

        try
        {
            var _config = Config.Deserialize(File.ReadAllText(ConfigPath));
            if (_config is null)
            {
                Console.Error.WriteLine("Unable to parse config.json");
                Environment.Exit(1);
            }
            else
            {
                config = _config;
                Initialize();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unable to parse config.json : {ex.ToString()}");
            Environment.Exit(1);
        }
    }

    private static void PreloadTextures()
    {
        foreach (Screen screen in config.Screens)
        {
            foreach (Cell cell in screen.Grid.Cells)
            {
                if (
                    cell.Media != null
                    && !string.IsNullOrEmpty(cell.Media.Source)
                    && File.Exists(Path.Combine(Directory.GetCurrentDirectory(), cell.Media.Source))
                )
                {
                    textures[cell.Media.Source] = Raylib.LoadTexture(cell.Media.Source);
                }
            }
        }
    }

    private static void PreloadFonts()
    {
        foreach (Screen screen in config.Screens)
        {
            foreach (Cell cell in screen.Grid.Cells)
            {
                if (cell.Text != null && !string.IsNullOrEmpty(cell.Text.FontFamily))
                {
                    if (
                        File.Exists(
                            Path.Combine(Directory.GetCurrentDirectory(), cell.Text.FontFamily)
                        )
                    )
                    {
                        fonts[cell.Text.FontFamily] = Raylib.LoadFont(cell.Text.FontFamily);
                    }
                    else
                    {
                        fonts[cell.Text.FontFamily] = Raylib.GetFontDefault();
                    }
                }
            }
        }
    }

    private static void UnloadTextures()
    {
        foreach (var texture in textures)
        {
            Raylib.UnloadTexture(texture.Value);
        }
    }

    private static void UnloadFonts()
    {
        foreach (var font in fonts)
        {
            Raylib.UnloadFont(font.Value);
        }
    }

    public static void Initialize()
    {
        Raylib.InitWindow(config.Width, config.Height, "alumenol");
        PreloadTextures();
        PreloadFonts();
        while (!Raylib.WindowShouldClose() && !shouldQuit)
        {
            float deltaTime = Raylib.GetFrameTime();
            Raylib.BeginDrawing();
            OnRender(deltaTime);
            OnUpdate(deltaTime);
            Raylib.EndDrawing();
        }
        UnloadTextures();
        UnloadFonts();
        Raylib.CloseWindow();
    }

    public static void OnRender(double deltaTime)
    {
        var screen = config.Screens[screenIndex];
        // var bg = screen.BackgroundColor;
        Raylib.ClearBackground(screen.BgColor);

        if (
            screen.Grid.Rows <= 0
            || screen.Grid.Rows > 100
            || screen.Grid.Cols <= 0
            || screen.Grid.Cols > 100
        )
        {
            Console.Error.WriteLine("Grid dimensions out of reasonable range");
            Environment.Exit(1);
        }

        float cellHeight = config.Height / screen.Grid.Rows;
        float cellWidth = config.Width / screen.Grid.Cols;

        foreach (Cell cell in screen.Grid.Cells)
        {
            float colStartPoint = (cell.ColStart - 1) * (float)cellWidth;
            float colEndPoint = (cell.ColStart - 1 + cell.ColSpan) * (float)cellWidth;

            float rowStartPoint = (cell.RowStart - 1) * (float)cellHeight;
            float rowEndPoint = (cell.RowStart - 1 + cell.RowSpan) * (float)cellHeight;

            int glX = (int)colStartPoint;
            int glY = (int)rowStartPoint;
            int glW = (int)(colEndPoint - colStartPoint);
            int glH = (int)(rowEndPoint - rowStartPoint);
            Rectangle destRect = new Rectangle(glX, glY, glW, glH);
            if (
                cell.Text != null
                && !string.IsNullOrEmpty(cell.Text.FontFamily)
                && File.Exists(Path.Combine(Directory.GetCurrentDirectory(), cell.Text.FontFamily))
            )
            {
                Raylib.DrawRectangle(glX, glY, glW, glH, cell.BgColor);
                Vector2 textSize = Raylib.MeasureTextEx(
                    fonts[cell.Text.FontFamily],
                    cell.Text.Value,
                    cell.Text.Size,
                    0f
                );

                float x = cell.Text.Align switch
                {
                    "Center" => glX + (glW - textSize.X) / 2f,
                    "Right" => glX + glW - textSize.X,
                    _ => glX,
                };

                float y = cell.Text.VAlign switch
                {
                    "Center" => glY + (glH - textSize.Y) / 2f,
                    "Bottom" => glY + glH - textSize.Y,
                    _ => glY,
                };

                //replace the veriables with the actual values
                foreach (var variable in GetVariables())
                {
                    cell.Text.Value = cell.Text.Value.Replace(
                        $"{{{variable.Key}}}",
                        variable.Value
                    );
                }
                Raylib.DrawTextEx(
                    fonts[cell.Text.FontFamily],
                    cell.Text.Value,
                    new Vector2(x, y),
                    cell.Text.Size,
                    0f,
                    cell.Text.FontColor
                );
            }
            else if (
                cell.Media != null
                && !string.IsNullOrEmpty(cell.Media.Source)
                && File.Exists(Path.Combine(Directory.GetCurrentDirectory(), cell.Media.Source))
            )
            {
                Texture2D cellTexture = textures[cell.Media.Source];
                Rectangle source = new Rectangle(0, 0, cellTexture.Width, cellTexture.Height);
                Raylib.DrawTexturePro(
                    cellTexture,
                    source,
                    destRect,
                    new System.Numerics.Vector2(0, 0),
                    0f,
                    Color.White
                );
            }
            else
            {
                Raylib.DrawText("Invalid Cell", glX, glY, 20, Color.Red);
            }
            if (cell.Border)
            {
                Raylib.DrawRectangleLinesEx(destRect, cell.BorderThickness, cell.CellBorderColor);
            }
        }
    }

    public static void OnUpdate(double deltaTime)
    {
        //if space key is pressed change isPaused state
        if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            shouldQuit = true;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            isPaused = !isPaused;
        }
        if (!isPaused)
        {
            //update the screentimer and index if the timer exceeds more than or equal to the screen duration
            screenTimer += deltaTime;
            if (screenTimer >= (double)config.Screens[screenIndex].Duration)
            {
                if (config.Screens.Count == screenIndex + 1)
                {
                    screenIndex = 0;
                }
                else
                {
                    screenIndex++;
                }
                screenTimer = 0;
            }
        }
        //increment screen index and reset timer if right key is pressed
        if (Raylib.IsKeyPressed(KeyboardKey.Right))
        {
            if (config.Screens.Count == screenIndex + 1)
            {
                screenIndex = 0;
            }
            else
            {
                screenIndex++;
            }
            screenTimer = 0;
        }
        //decrement screen index and reset timer if left key is pressed
        if (Raylib.IsKeyPressed(KeyboardKey.Left))
        {
            if (screenIndex == 0)
            {
                screenIndex = config.Screens.Count - 1;
            }
            else
            {
                screenIndex--;
            }
            screenTimer = 0;
        }
    }
}
