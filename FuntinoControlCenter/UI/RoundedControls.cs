using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FuntinoControlCenter.UI
{
    public class RoundedButton : Button
    {
        private int cornerRadius;
        private int borderSize;
        private Color borderColor = Color.Empty;
        private Region roundedRegion;

        public RoundedButton()
        {
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
        }

        [Category("ظاهر")]
        [Description("میزان گردی گوشه‌های دکمه را مشخص می‌کند.")]
        public int CornerRadius
        {
            get { return cornerRadius; }
            set
            {
                cornerRadius = Math.Max(0, value);
                UpdateRoundedRegion();
                Invalidate();
            }
        }

        [Category("ظاهر")]
        [Description("رنگ دور دکمه را مشخص می‌کند.")]
        public Color BorderColor
        {
            get { return borderColor; }
            set
            {
                borderColor = value;
                Invalidate();
            }
        }

        [Category("ظاهر")]
        [Description("ضخامت دور دکمه را بر حسب پیکسل مشخص می‌کند.")]
        public int BorderSize
        {
            get { return borderSize; }
            set
            {
                borderSize = Math.Max(0, value);
                Invalidate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRoundedRegion();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawRoundedBorder(e.Graphics);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && roundedRegion != null)
            {
                base.Region = null;
                roundedRegion.Dispose();
                roundedRegion = null;
            }

            base.Dispose(disposing);
        }

        private void UpdateRoundedRegion()
        {
            if (Width <= 0 || Height <= 0) return;

            Region next = RoundedGeometry.CreateRegion(ClientRectangle, cornerRadius);
            Region previous = roundedRegion;
            roundedRegion = next;
            base.Region = next;
            if (previous != null) previous.Dispose();
        }

        private void DrawRoundedBorder(Graphics graphics)
        {
            if (borderSize <= 0 || borderColor.IsEmpty || ClientSize.Width <= 1 || ClientSize.Height <= 1) return;

            Rectangle bounds = ClientRectangle;
            bounds.Width--;
            bounds.Height--;
            using (GraphicsPath path = RoundedGeometry.CreatePath(bounds, cornerRadius))
            using (Pen pen = new Pen(borderColor, borderSize))
            {
                pen.Alignment = PenAlignment.Inset;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.DrawPath(pen, path);
            }
        }
    }

    public class RoundedBorderPanel : Panel
    {
        private int cornerRadius;
        private int borderSize;
        private Color borderColor = Color.Empty;
        private Region roundedRegion;

        public RoundedBorderPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        [Category("ظاهر")]
        [Description("میزان گردی گوشه‌های کادر را مشخص می‌کند.")]
        public int CornerRadius
        {
            get { return cornerRadius; }
            set
            {
                cornerRadius = Math.Max(0, value);
                UpdateRoundedRegion();
                Invalidate();
            }
        }

        [Category("ظاهر")]
        [Description("رنگ دور کادر را مشخص می‌کند.")]
        public Color BorderColor
        {
            get { return borderColor; }
            set
            {
                borderColor = value;
                Invalidate();
            }
        }

        [Category("ظاهر")]
        [Description("ضخامت دور کادر را بر حسب پیکسل مشخص می‌کند.")]
        public int BorderSize
        {
            get { return borderSize; }
            set
            {
                borderSize = Math.Max(0, value);
                Invalidate();
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            UpdateRoundedRegion();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawRoundedBorder(e.Graphics);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && roundedRegion != null)
            {
                base.Region = null;
                roundedRegion.Dispose();
                roundedRegion = null;
            }

            base.Dispose(disposing);
        }

        private void UpdateRoundedRegion()
        {
            if (Width <= 0 || Height <= 0) return;

            Region next = RoundedGeometry.CreateRegion(ClientRectangle, cornerRadius);
            Region previous = roundedRegion;
            roundedRegion = next;
            base.Region = next;
            if (previous != null) previous.Dispose();
        }

        private void DrawRoundedBorder(Graphics graphics)
        {
            if (borderSize <= 0 || borderColor.IsEmpty || ClientSize.Width <= 1 || ClientSize.Height <= 1) return;

            Rectangle bounds = ClientRectangle;
            bounds.Width--;
            bounds.Height--;
            using (GraphicsPath path = RoundedGeometry.CreatePath(bounds, cornerRadius))
            using (Pen pen = new Pen(borderColor, borderSize))
            {
                pen.Alignment = PenAlignment.Inset;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.DrawPath(pen, path);
            }
        }
    }

    internal static class RoundedGeometry
    {
        internal static Region CreateRegion(Rectangle bounds, int radius)
        {
            using (GraphicsPath path = CreatePath(bounds, radius))
            {
                return new Region(path);
            }
        }

        internal static GraphicsPath CreatePath(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (bounds.Width <= 0 || bounds.Height <= 0) return path;

            int diameter = Math.Min(Math.Max(0, radius * 2), Math.Min(bounds.Width, bounds.Height));
            if (diameter <= 0)
            {
                path.AddRectangle(bounds);
                path.CloseFigure();
                return path;
            }

            Rectangle arc = new Rectangle(bounds.X, bounds.Y, diameter, diameter);
            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.X;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
