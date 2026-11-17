using ApexCharts;

namespace Stockroom.Web.Components.Reports;

public static class ChartOptions
{
    // Title and subtitle are drawn inside the chart, because the
    // export of the library contains only the chart. The file name
    // keeps downloads of different charts apart.
    public static ApexChartOptions<TItem> Create<TItem>(
        string title, string subtitle, string fileName)
        where TItem : class
    {
        return new()
        {
            Title = new() { Text = title },
            Subtitle = new() { Text = subtitle },
            Chart = new()
            {
                Toolbar = new()
                {
                    Export = new()
                    {
                        Csv = new() { Filename = fileName },
                        Svg = new() { Filename = fileName },
                        Png = new() { Filename = fileName },
                    },
                },
            },
        };
    }
}
