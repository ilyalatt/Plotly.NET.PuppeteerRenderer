using System;
using System.IO;
using Plotly.NET;
using Plotly.NET.PuppeteerRenderer;

var chart = Chart.Line<int, int, int, int>(
    x: new[] { 1, 2, 3 },
    y: new[] { 4, 5, 6 }
);

Console.WriteLine("Fetching and launching Chromium");
await using var renderer = await PlotlyRenderer.FetchBrowserAndLaunch();

Console.WriteLine("Rendering the chart");
var png = await renderer.Render(1024, 768, chart.ToFullScreenHtml());
await File.WriteAllBytesAsync("chart.png", png);

Console.WriteLine("Done");