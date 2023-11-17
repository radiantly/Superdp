using System.Diagnostics;

namespace Superdp
{
    internal class SmoothLoadingBar : UserControl
    {
        private readonly SolidBrush brush;
        private readonly TimeSpan totalDuration;
        private Stopwatch? timer = null;
        private float currentDrawnWidth = 0;
        public SmoothLoadingBar(Color barColor, TimeSpan duration)
        {

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            UpdateStyles();

            brush = new SolidBrush(barColor);
            totalDuration = duration;
        }

        public void Start()
        {
            timer = Stopwatch.StartNew();
            Invalidate();
        }

        static private double EaseInOutSine(double x) {
            return -(Math.Cos(Math.PI * x) - 1) / 2;
        }

        static private float Lerp(float want, float have)
        {
            var delta = (want - have) / 2;
            return have + delta == have ? want : have + delta;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (timer == null) return;
            var percent = (float)EaseInOutSine(Math.Min(1, timer.Elapsed.TotalSeconds / totalDuration.TotalSeconds));
            currentDrawnWidth = Math.Min(currentDrawnWidth + 1, Lerp(Size.Width * percent, currentDrawnWidth));
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            e.Graphics.FillRectangle(brush, 0, 0, currentDrawnWidth, Size.Height);
            if (currentDrawnWidth < Size.Width)
                Invalidate();
        }
    }
}
