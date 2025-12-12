using AndroidAPSMaui.Data;

namespace AndroidAPSMaui.Graphs;

public class BgGraphDrawable : IDrawable
{
    private IReadOnlyList<BgReading> _points = Array.Empty<BgReading>();
    private IReadOnlyList<PumpEvent> _events = Array.Empty<PumpEvent>();

    public void Update(IReadOnlyList<BgReading> points, IReadOnlyList<PumpEvent> pumpEvents)
    {
        _points = points;
        _events = pumpEvents;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();
        canvas.FillColor = Color.FromArgb("0E0E11");
        canvas.FillRectangle(dirtyRect);

        if (_points.Count == 0)
        {
            canvas.FontColor = Colors.White;
            canvas.DrawString("Waiting for glucose data…", dirtyRect.Left + 12, dirtyRect.Center.Y, HorizontalAlignment.Left);
            canvas.RestoreState();
            return;
        }

        var minTime = _points.First().Timestamp;
        var maxTime = _points.Last().Timestamp;
        if (minTime == maxTime)
        {
            maxTime = maxTime.AddMinutes(1);
        }
        var minValue = _points.Min(p => p.Value);
        var maxValue = _points.Max(p => p.Value);
        maxValue = Math.Max(maxValue, minValue + 1);

        float Padding = 24;
        var graphRect = new RectF(dirtyRect.Left + Padding, dirtyRect.Top + Padding, dirtyRect.Width - 2 * Padding, dirtyRect.Height - 2 * Padding);

        DrawAxes(canvas, graphRect, minValue, maxValue);
        DrawPumpEvents(canvas, graphRect, minTime, maxTime, minValue, maxValue);
        DrawGlucoseLine(canvas, graphRect, minTime, maxTime, minValue, maxValue);
        canvas.RestoreState();
    }

    private void DrawAxes(ICanvas canvas, RectF rect, double minValue, double maxValue)
    {
        canvas.StrokeColor = Colors.Gray;
        canvas.StrokeSize = 1;
        canvas.DrawLine(rect.Left, rect.Bottom, rect.Right, rect.Bottom);
        canvas.DrawLine(rect.Left, rect.Top, rect.Left, rect.Bottom);

        var range = maxValue - minValue;
        for (int i = 0; i <= 4; i++)
        {
            var y = rect.Bottom - rect.Height * (float)i / 4f;
            canvas.DrawLine(rect.Left, y, rect.Right, y);
            var value = minValue + range * i / 4d;
            canvas.DrawString(Math.Round(value).ToString(), rect.Left + 4, y - 8, HorizontalAlignment.Left);
        }
    }

    private void DrawGlucoseLine(ICanvas canvas, RectF rect, DateTime minTime, DateTime maxTime, double minValue, double maxValue)
    {
        var rangeSeconds = (maxTime - minTime).TotalSeconds;
        var valueRange = maxValue - minValue;
        canvas.StrokeColor = Color.FromArgb("5DBB63");
        canvas.StrokeSize = 3;

        PointF? previous = null;
        foreach (var point in _points)
        {
            var x = rect.Left + rect.Width * (float)((point.Timestamp - minTime).TotalSeconds / rangeSeconds);
            var y = rect.Bottom - rect.Height * (float)((point.Value - minValue) / valueRange);
            var current = new PointF(x, y);
            if (previous != null)
            {
                canvas.DrawLine(previous.Value, current);
            }
            previous = current;
        }
    }

    private void DrawPumpEvents(ICanvas canvas, RectF rect, DateTime minTime, DateTime maxTime, double minValue, double maxValue)
    {
        if (_events.Count == 0)
        {
            return;
        }

        var rangeSeconds = (maxTime - minTime).TotalSeconds;
        canvas.StrokeColor = Color.FromArgb("FF6F61");
        canvas.StrokeSize = 2;
        canvas.FontColor = Colors.White;
        canvas.FontSize = 12;

        foreach (var evt in _events)
        {
            if (evt.Timestamp < minTime || evt.Timestamp > maxTime)
            {
                continue;
            }
            var x = rect.Left + rect.Width * (float)((evt.Timestamp - minTime).TotalSeconds / rangeSeconds);
            canvas.DrawLine(x, rect.Top, x, rect.Bottom);
            canvas.DrawString(evt.EventType, x + 4, rect.Top + 4, HorizontalAlignment.Left);
        }
    }
}
