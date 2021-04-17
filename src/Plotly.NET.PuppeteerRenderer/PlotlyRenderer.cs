using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace Plotly.NET.PuppeteerRenderer {
    public sealed class PlotlyRenderer : IDisposable, IAsyncDisposable {
        static string PatchPlotlyHtml(int width, int height, string html) {
            var regex = new Regex(@"(Plotly\.newPlot\(.+?\))");
            var patchedHtml = regex.Replace(html, x => x.Result(
                "$1" +
                $".then(x => Plotly.toImage(x, {{ format: 'png', scale: 2, width: {width}, height: {height} }}))" +
                ".then(img => window.plotlyImage = img)"));
            return patchedHtml;
        }

        static async Task<Browser> FetchAndLaunchBrowser() {
            var browserFetcher = new BrowserFetcher();
            var revision = await browserFetcher.DownloadAsync();
            var launchOptions = new LaunchOptions {
                ExecutablePath = revision.ExecutablePath
            };
            return await Puppeteer.LaunchAsync(launchOptions);
        }

        static async Task<byte[]> TryRender(Browser browser, int width, int height, string html) {
            var page = await browser.NewPageAsync();
            try {
                await page.SetContentAsync(PatchPlotlyHtml(width, height, html));
                await using var imgHandle = await page.WaitForExpressionAsync("window.plotlyImage");
                var imgStr = await imgHandle.JsonValueAsync<string>();
                var imgBase64StartIdx = imgStr.IndexOf(",", StringComparison.Ordinal) + 1;
                var img = Convert.FromBase64String(imgStr.Substring(imgBase64StartIdx));
                return img;
            }
            finally {
                await page.CloseAsync();
            }
        }

        readonly Browser _browser;
        readonly SemaphoreSlim _taskQueue = new(1, 1);

        public PlotlyRenderer(Browser browser) =>
            _browser = browser;

        public void Dispose() {
            _browser.Dispose();
            _taskQueue.Dispose();
        }

        public async ValueTask DisposeAsync() {
            await _browser.DisposeAsync();
            _taskQueue.Dispose();
        }

        public static async Task<PlotlyRenderer> FetchBrowserAndLaunch() {
            var browser = await FetchAndLaunchBrowser();
            return new PlotlyRenderer(browser);
        }

        public async Task<byte[]> Render(int width, int height, string html) {
            await _taskQueue.WaitAsync();
            try {
                return await TryRender(_browser, width, height, html);
            }
            finally {
                _taskQueue.Release();
            }
        }
    }
}