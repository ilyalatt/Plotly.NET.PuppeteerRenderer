# Plotly.NET.PuppeteerRenderer

[![NuGet version](https://badge.fury.io/nu/Plotly.NET.PuppeteerRenderer.svg)](https://www.nuget.org/packages/Plotly.NET.PuppeteerRenderer)

## Usage

```csharp
var chart = Chart.Line<int, int, int, int>(
    x: new[] { 1, 2, 3 },
    y: new[] { 4, 5, 6 }
);

await using var renderer = await PlotlyRenderer.FetchBrowserAndLaunch();
var png = await renderer.Render(1024, 768, chart.ToFullScreenHtml());
await File.WriteAllBytesAsync("chart.png", png);
```
