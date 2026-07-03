using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace alumenol;

class Program
{
    private static IWindow _window = null!;
    private static readonly string ConfigPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "config.json"
    );

    private static Config config = null!;
    private static Silk.NET.OpenGL.GL gl = null!;

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
        catch
        {
            Console.Error.WriteLine("Unable to parse config.json");
            Environment.Exit(1);
        }
    }

    public static void OnLoad()
    {
        Console.WriteLine("Loaded");
        IInputContext input = _window.CreateInput();
        for (int i = 0; i < input.Keyboards.Count; i++)
            input.Keyboards[i].KeyDown += KeyDown;
    }

    private static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key == Key.Escape || key == Key.Q)
            _window.Close();
    }

    public static void OnRender(double deltaTime)
    {
        var screen = config.Screens[screenIndex];
        var bg = screen.BackgroundColor;

        (int R, int G, int B, int A) color = config.RGBaFromHex(bg);
        // Console.WriteLine($"screen bgcolor : {color}");
        gl.ClearColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        gl.Clear(ClearBufferMask.ColorBufferBit);
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
        // Console.WriteLine($"cell height : {cellHeight}");
        // Console.WriteLine($"cell width : {cellWidth}");

        foreach (Cell cell in screen.Grid.Cells)
        {
            float colStartPoint = (cell.ColStart - 1) * (float)cellWidth;
            float colEndPoint = (cell.ColStart - 1 + cell.ColSpan) * (float)cellWidth;

            float rowStartPoint = (cell.RowStart - 1) * (float)cellHeight;
            float rowEndPoint = (cell.RowStart - 1 + cell.RowSpan) * (float)cellHeight;

            // Console.WriteLine(
            //     $"col start : {colStartPoint}, col end : {colEndPoint}, row start : {rowStartPoint}, row end : {rowEndPoint}"
            // );
            int glX = (int)colStartPoint;
            int glY = (int)(config.Height - rowEndPoint);
            int glW = (int)(colEndPoint - colStartPoint);
            int glH = (int)(rowEndPoint - rowStartPoint);

            (int R, int G, int B, int A) cellcolor = config.RGBaFromHex(cell.BackgroundColor);
            gl.Enable(EnableCap.ScissorTest);
            gl.Scissor(glX, glY, (uint)glW, (uint)glH);
            gl.ClearColor(
                cellcolor.R / 255f,
                cellcolor.G / 255f,
                cellcolor.B / 255f,
                cellcolor.A / 255f
            );
            gl.Clear(ClearBufferMask.ColorBufferBit);
            gl.Disable(EnableCap.ScissorTest);
        }
    }

    private static double screenTimer;
    private static int screenIndex;

    public static void OnUpdate(double deltaTime)
    {
        screenTimer += deltaTime;
        if (screenTimer >= (double)config.Screens[screenIndex].Duration)
        {
            if (config.Screens.Count == screenIndex + 1)
            {
                screenIndex = 0;
                screenTimer = 0;
            }
            else
            {
                screenIndex++;
                screenTimer = 0;
            }
        }
        Console.WriteLine($"screen index : {screenIndex}");
    }

    public static void Initialize()
    {
        WindowOptions options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(config.Width, config.Height),
            Title = "Alumenol",
        };
        _window = Window.Create(options);
        _window.Load += () =>
        {
            gl = _window.CreateOpenGL();
            OnLoad();
        };
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Run();
    }
}
