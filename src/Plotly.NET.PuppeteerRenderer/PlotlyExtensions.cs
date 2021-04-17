namespace Plotly.NET.PuppeteerRenderer {
    public static class PlotlyExtensions {
        public static string ToFullScreenHtml(this GenericChart.GenericChart chart) {
            var config = GenericChart.getConfig(chart);
            var layout = GenericChart.getLayout(chart);
            DynObj.setValueOpt<bool>(config, "responsive", true);
            DynObj.setValueOpt<double>(layout, "width", null);
            DynObj.setValueOpt<double>(layout, "height", null);
            return GenericChart.toChartHTML(chart).Replace("width: 600px; height: 600px;", "width: 100%; height: 100%;");
        }
    }
}