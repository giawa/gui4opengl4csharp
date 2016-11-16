using System;

using OpenGL;
using OpenGL.Platform;

namespace OpenGL.UI
{
    public class ColorGradient : UIElement
    {
        #region Variables
        private VAO gradientQuad;
        private float selx = 0.0f, sely = 1.0f, h = 1.0f;
        private bool mouseDown = false;

        public float Hue
        {
            get { return h; }
            set
            {
                h = value;
                Program.Use();
                Program["hue"].SetValue(new HSLColor(value, 1, 0.5f).ToVector());
                UpdateColor();
            }
        }

        private Vector3 color;

        public Vector3 Color
        {
            get { return color; }
            set
            {

            }
        }

        public OnMouse OnColorChange { get; set; }
        #endregion

        #region Constructor
        public ColorGradient()
        {
            var colorGradient = UserInterface.GetElement("ColorGradient");
            if (colorGradient != null) throw new Exception("Only one color picker can currently exist at once.  This is a limitation I intend to remove soon.");

            this.Program = Shaders.GradientShader;

            this.Program.Use();
            this.Program["hue"].SetValue(new Vector3(h, 0, 0));
            this.Program["sel"].SetValue(new Vector2(selx, sely));
            this.gradientQuad = Geometry.CreateQuad(this.Program, Vector2.Zero, new Vector2(150, 150));

            this.RelativeTo = Corner.TopLeft;
            this.Position = new Point(30, 50);
            this.Size = new Point(150, 150);
            this.Name = "ColorGradient";

            // set up the events for the mouse to move the indicator around
            this.OnMouseDown = new OnMouse((sender, eventArgs) =>
            {
                mouseDown = (eventArgs.Button == MouseButton.Left);
                UpdateMousePosition(eventArgs.Location.X, eventArgs.Location.Y);
            });
            this.OnMouseUp = new OnMouse((sender, eventArgs) => mouseDown = (eventArgs.Button == MouseButton.Left ? false : mouseDown));
            this.OnMouseLeave = new OnMouse((sender, eventArgs) => mouseDown = false);
            this.OnMouseMove = new OnMouse((sender, eventArgs) => UpdateMousePosition(eventArgs.Location.X, eventArgs.Location.Y));

            UpdateColor();
        }
        #endregion

        #region Methods
        private void UpdateMousePosition(int x, int y)
        {
            if (!mouseDown) return;

            selx = Math.Min(1, (float)(x - CorrectedPosition.X) / Size.X);
            sely = Math.Min(1, (float)((UserInterface.Height - y) - CorrectedPosition.Y) / Size.Y);

            Program.Use();
            Program["sel"].SetValue(new Vector2(selx, sely));

            UpdateColor();
        }

        private void UpdateColor()
        {
            // create our HSL color
            Vector3 color = new HSLColor(h, 1, 0.5f).ToVector();

            // now blend the white and color together
            Vector3 blend1 = color * selx + Vector3.One * (float)(1 - selx);

            // finally blend the black and blend1 together
            this.color = blend1 * sely;

            if (OnColorChange != null) OnColorChange(this, new MouseEventArgs());
        }
        #endregion

        #region UIElement Methods
        public override void Draw()
        {
            Program.Use();
            gradientQuad.Draw();
        }

        public override void OnResize()
        {
            base.OnResize();

            Program.Use();
            Program["model_matrix"].SetValue(Matrix4.CreateTranslation(new Vector3(CorrectedPosition.X, CorrectedPosition.Y, 0)));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            gradientQuad.DisposeChildren = true;
            gradientQuad.Dispose();
        }
        #endregion
    }

    public class HueGradient : UIElement
    {
        #region Variables
        private VAO hueQuad;
        private bool mouseDown = false;
        #endregion

        #region Constructor
        public HueGradient()
        {
            this.Program = Shaders.HueShader;

            this.Program.Use();
            this.Program["hue"].SetValue(0f);
            this.hueQuad = Geometry.CreateQuad(this.Program, Vector2.Zero, new Vector2(26, 150));

            this.RelativeTo = Corner.TopLeft;
            this.Position = new Point(185, 50);
            this.Size = new Point(26, 150);
            this.Name = "HueGradient";

            // set up the events for the mouse to move the indicator around
            this.OnMouseDown = new OnMouse((sender, eventArgs) =>
            {
                mouseDown = (eventArgs.Button == MouseButton.Left);
                UpdateMousePosition(eventArgs.Location.X, eventArgs.Location.Y);
            });
            this.OnMouseUp = new OnMouse((sender, eventArgs) => mouseDown = (eventArgs.Button == MouseButton.Left ? false : mouseDown));
            this.OnMouseLeave = new OnMouse((sender, eventArgs) => mouseDown = false);
            this.OnMouseMove = new OnMouse((sender, eventArgs) => UpdateMousePosition(eventArgs.Location.X, eventArgs.Location.Y));
        }
        #endregion

        #region Methods
        private void UpdateMousePosition(int x, int y)
        {
            if (!mouseDown) return;

            // calculate the selected hue based on the mouse position
            float hue = ((UserInterface.Height - y) - CorrectedPosition.Y) / (float)Size.Y;
            Program.Use();
            Program["hue"].SetValue((float)((UserInterface.Height - y) - CorrectedPosition.Y));

            // asks the user interface for the applicable color gradient
            // this will need to be modified if multiple color gradients are on the screen at once
            // which I can't imagine happening currently, but who knows
            var colorGradient = UserInterface.GetElement("ColorGradient");
            if (colorGradient != null) ((ColorGradient)colorGradient).Hue = hue;
        }
        #endregion

        #region UIElement Methods
        public override void Draw()
        {
            Gl.Enable(EnableCap.Blend);

            Program.Use();
            hueQuad.Draw();

            Gl.Disable(EnableCap.Blend);
        }

        public override void OnResize()
        {
            base.OnResize();

            Program.Use();
            Program["model_matrix"].SetValue(Matrix4.CreateTranslation(new Vector3(CorrectedPosition.X, CorrectedPosition.Y, 0)));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            hueQuad.DisposeChildren = true;
            hueQuad.Dispose();
        }
        #endregion
    }

    public class HSLColor
    {
        #region Properties
        public float H { get; private set; }

        public float S { get; private set; }

        public float L { get; private set; }
        #endregion

        #region Constructors
        public HSLColor(float h, float s, float l)
        {
            this.H = h;
            this.S = s;
            this.L = l;
        }

        public HSLColor(Vector3 c)
        {
            float r = c.X;
            float g = c.Y;
            float b = c.Z;

            float _Min = Math.Min(Math.Min(r, g), b);
            float _Max = Math.Max(Math.Max(r, g), b);
            float _Delta = _Max - _Min;

            H = 0;
            S = 0;
            L = (float)((_Max + _Min) / 2.0f);

            if (_Delta != 0)
            {
                if (L < 0.5f)
                {
                    S = (float)(_Delta / (_Max + _Min));
                }
                else
                {
                    S = (float)(_Delta / (2.0f - _Max - _Min));
                }


                if (r == _Max)
                {
                    H = (g - b) / _Delta;
                }
                else if (g == _Max)
                {
                    H = 2f + (b - r) / _Delta;
                }
                else if (b == _Max)
                {
                    H = 4f + (r - g) / _Delta;
                }
            }

            H = H * 60f;
            if (H < 0) H += 360;
            H /= 360f;
        }

        public HSLColor(System.Drawing.Color c)
            : this(c.R / 255f, c.G / 255f, c.B / 255f)
        {
        }
        #endregion

        #region Methods
        public Vector3 ToVector()
        {
            float r, g, b;

            if (S == 0) r = g = b = L;
            else
            {
                float q = (L < 0.5f ? L * (1 + S) : L + S - L * S);
                float p = 2 * L - q;
                r = HUE2RGB(p, q, H + 1 / 3.0f);
                g = HUE2RGB(p, q, H);
                b = HUE2RGB(p, q, H - 1 / 3.0f);
            }

            return new Vector3(r, g, b);
        }

        private static float HUE2RGB(float p, float q, float t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1 / 6.0) return p + (q - p) * 6 * t;
            if (t < 1 / 2.0) return q;
            if (t < 2 / 3.0) return p + (q - p) * (2 / 3.0f - t) * 6;
            return p;
        }
        #endregion
    }
}
