# Alumenol

A JSON-configured digital signage renderer. Point it at a `config.json`, plug your display into HDMI, and it renders your screens in rotation.

## How it works

You define one or more **Screens**. Each screen has a **Grid** (rows/columns), and the grid holds **Cells**. Each cell occupies a rectangular region of the grid (by row/col start + span) and shows either **Text** or **Media** (an image). Screens rotate automatically based on their `Duration`.

## Top-level fields

| Field | Type | Required | Description |
|---|---|---|---|
| `Width` | int | yes | Output window/canvas width in pixels |
| `Height` | int | yes | Output window/canvas height in pixels |
| `Screens` | array | yes | List of Screen objects, played in order, looping |

## Screen fields

| Field | Type | Required | Description |
|---|---|---|---|
| `Duration` | int | yes | Seconds this screen stays on-screen before advancing |
| `BackgroundColor` | string (hex) | no | Background color, e.g. `"#FFFFFF"`. Falls back to sky blue if invalid/missing |
| `Grid` | object | yes | The Grid object for this screen |

## Grid fields

| Field | Type | Required | Description |
|---|---|---|---|
| `Rows` | int | yes | Number of grid rows (1–100) |
| `Cols` | int | yes | Number of grid columns (1–100) |
| `Cells` | array | yes | List of Cell objects |

Grid cell size is computed as `Width / Cols` and `Height / Rows` — keep dimensions evenly divisible for clean layouts, or accept minor rounding.

## Cell fields

| Field | Type | Required | Description |
|---|---|---|---|
| `RowStart` | int | yes | 1-indexed row where the cell begins |
| `ColStart` | int | yes | 1-indexed column where the cell begins |
| `RowSpan` | int | yes | Number of rows the cell occupies |
| `ColSpan` | int | yes | Number of columns the cell occupies |
| `BackgroundColor` | string (hex) | no | Cell background color |
| `Border` | bool | no | Whether to draw a border around the cell |
| `BorderThickness` | int | no | Border thickness in pixels (only used if `Border` is true) |
| `BorderColor` | string (hex) | no | Border color (only used if `Border` is true) |
| `Text` | object | no* | Text content — see below |
| `Media` | object | no* | Image content — see below |

*A cell should have exactly one of `Text` or `Media`. If neither is set, it renders as an "Invalid Cell" placeholder. If both are set, `Text` takes priority.

**No overlap/bounds validation exists yet** — if your `RowStart`/`ColStart`/spans push a cell outside the grid or overlapping another cell, it will render wrong silently. Check your math.

## Text fields

| Field | Type | Required | Description |
|---|---|---|---|
| `FontFamily` | string | yes | Path to a `.ttf`/`.otf` font file (relative to config), e.g. `"InterVariable.ttf"`. Not a system font name — the file must exist |
| `Size` | int | yes | Font size in pixels |
| `Color` | string (hex) | yes | Text color |
| `Align` | string | yes | Horizontal alignment: `Left`, `Center`, `Right` |
| `VAlign` | string | yes | Vertical alignment: `Top`, `Center`, `Bottom` |
| `Value` | string | yes | The text to display |

## Media fields

| Field | Type | Required | Description |
|---|---|---|---|
| `Source` | string | yes | Path to an image file (relative to config). **PNG, BMP, TGA, GIF, QOI supported. JPG is not supported by default.** GIFs render as a static first frame only — no animation. |

## Hex color format

Accepts `#RRGGBB` or `#RRGGBBAA` (with or without leading `#`). Invalid or malformed hex falls back to a default color rather than crashing.

## Preparing your config

1. Set `Width`/`Height` to match your actual display/LED panel resolution.
2. For each screen, decide a grid size (`Rows` x `Cols`) that cleanly divides your layout — think in terms of a table/dashboard grid, not pixels.
3. Place cells by `RowStart`/`ColStart` (1-indexed, top-left origin) and give them a `RowSpan`/`ColSpan` to occupy multiple grid cells (like a merged cell in a spreadsheet).
4. Fill each cell with either `Text` or `Media`.
5. Put referenced fonts and images in the same directory as `config.json` (or a subfolder, using relative paths).
6. Set `Duration` per screen for how long it displays before rotating to the next.

## Example: single cell spanning a header

```json
{
  "RowStart": 1,
  "ColStart": 1,
  "RowSpan": 1,
  "ColSpan": 3,
  "BackgroundColor": "#FFFFFF",
  "Border": true,
  "BorderThickness": 2,
  "BorderColor": "#000000",
  "Text": {
    "FontFamily": "InterVariable.ttf",
    "Size": 40,
    "Color": "#000000",
    "Align": "Center",
    "VAlign": "Center",
    "Value": "Table 1"
  }
}
```
## Build
 
**Requirement: .NET 10 SDK** (mandatory, any OS).
 
```
dotnet publish --self-contained true -p:PublishSingleFile=true
```
 
Run on the target OS (cross-OS AOT/single-file publishing isn't supported — build Windows binaries on Windows, Linux on Linux, macOS on macOS).

## Known limitations

- No JPG support (convert to PNG)
- No GIF animation (static first frame only)
- No text wrapping — long text overflows the cell unless you've added scissor/shrink-to-fit logic
- No config validation — bad row/col math renders wrong silently instead of erroring
