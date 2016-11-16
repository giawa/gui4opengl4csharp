using System;

using OpenGL;
using OpenGL.Platform;

namespace OpenGL.UI
{
    public class Text : UIElement
    {
        #region Built-In Font Sizes
        public enum FontSize
        {
            _12pt = 0,
            _14pt = 1,
            _16pt = 2,
            _24pt = 3,
            _32pt = 4,
            _48pt = 5
        }

        public static BMFont FontFromSize(FontSize font)
        {
            switch (font)
            {
                case FontSize._12pt: return BMFont.LoadFont("fonts/font12.fnt");
                case FontSize._14pt: return BMFont.LoadFont("fonts/font14.fnt");
                case FontSize._16pt: return BMFont.LoadFont("fonts/font16.fnt");
                case FontSize._24pt: return BMFont.LoadFont("fonts/font24.fnt");
                case FontSize._32pt: return BMFont.LoadFont("fonts/font32.fnt");
                case FontSize._48pt: return BMFont.LoadFont("fonts/font48.fnt");
                default: //Logger.Instance.WriteLine("Unknown font " + font + " requested.");
                    return BMFont.LoadFont("fonts/font12.fnt");
            }
        }
        #endregion

        #region Private Fields
        private string text;
        private BMFont bitmapFont;
        private Vector3 color;
        private BMFont.Justification justification;
        #endregion

        #region Public Properties
        public Vector3 Color
        {
            get { return color; }
            set { color = value; }
        }

        public BMFont.Justification Justification
        {
            get { return justification; }
            set
            {
                if (justification != value)
                {
                    justification = value;
                    bitmapFont.CreateString(VAO, text, color, justification);
                }
            }
        }

        private VAO<Vector3, Vector2> VAO;

        public Point Padding { get; set; }

        public Point TextSize { get; private set; }

        public string String
        {
            get { return text; }
            set
            {
                // do not cause the text to update if it is the same
                if (text == value || value == null) return;

                if (text != null && text.Length == value.Length)
                {
                    bitmapFont.CreateString(VAO, value, Color, Justification);
                }
                else
                {
                    if (this.VAO != null) this.VAO.Dispose();
                    this.VAO = bitmapFont.CreateString(Program, value, Color, Justification);
                    this.VAO.DisposeChildren = true;
                }

                text = value;
                this.TextSize = new Point(bitmapFont.GetWidth(text), bitmapFont.Height);
            }
        }
        #endregion

        #region Constructors
        public Text(ShaderProgram program, BMFont font, string text, BMFont.Justification justification = BMFont.Justification.Left)
            : this(program, font, text, new Vector3(1, 1, 1), justification)
        {
        }

        public Text(FontSize font, string text, BMFont.Justification justification = BMFont.Justification.Left)
            : this(Shaders.FontShader, FontFromSize(font), text, Vector3.One, justification)
        {
        }

        public Text(FontSize font, string text, Vector3 color, BMFont.Justification justification = BMFont.Justification.Left)
            : this(Shaders.FontShader, FontFromSize(font), text, color, justification)
        {
        }

        public Text(ShaderProgram program, BMFont font, string text, Vector3 color, BMFont.Justification justification = BMFont.Justification.Left)
        {
            this.bitmapFont = font;
            this.Program = program;
            this.Justification = justification;
            this.Color = color;
            this.String = text;
            this.Position = new Point(0, 0);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the current VAO with a new font.
        /// Will use the current 'String' if it exists, otherwise will leave VAO as null.
        /// </summary>
        /// <param name="font">The new font size to use with this Text.</param>
        public void UpdateFontSize(FontSize font)
        {
            this.bitmapFont = FontFromSize(font);

            if (string.IsNullOrEmpty(this.String)) return;

            if (this.VAO != null) bitmapFont.CreateString(VAO, this.String, Color, Justification);
            else
            {
                this.VAO = bitmapFont.CreateString(Program, this.String, Color, Justification);
                this.VAO.DisposeChildren = true;
            }

            this.TextSize = new Point(bitmapFont.GetWidth(text), bitmapFont.Height);
        }

        public void DrawWithCharacterCount(int count)
        {
            int vertexCount = Math.Min(count * 6, VAO.VertexCount);

            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(bitmapFont.FontTexture);

            Gl.Enable(EnableCap.Blend);
            Program.Use();
            Program["position"].SetValue(new Vector2(CorrectedPosition.X + Padding.X, CorrectedPosition.Y + Padding.Y));
            Program["color"].SetValue(color);
            VAO.BindAttributes(Program);
            Gl.DrawElements(BeginMode.Triangles, vertexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
            Gl.Disable(EnableCap.Blend);
        }
        #endregion

        #region UIElement Overrides (Draw, Pick, Dispose)
        public override void Draw()
        {
            base.Draw();

            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(bitmapFont.FontTexture);

            int yoffset = 0;
            if (this.Size.Y > TextSize.Y)
                yoffset = (Size.Y - TextSize.Y) / 2;

            Gl.Enable(EnableCap.Blend);
            Program.Use();
            if (this.Justification == BMFont.Justification.Center) Program["position"].SetValue(new Vector2(CorrectedPosition.X + Padding.X + Size.X / 2, CorrectedPosition.Y + Padding.Y + yoffset));
            else Program["position"].SetValue(new Vector2(CorrectedPosition.X + Padding.X, CorrectedPosition.Y + Padding.Y + yoffset));
            Program["color"].SetValue(color);
            VAO.Draw();
            Gl.Disable(EnableCap.Blend);
        }

        protected override void Dispose(bool disposing)
        {
            if (VAO != null)
            {
                VAO.Dispose();
                VAO = null;
            }

            base.Dispose(true);
        }
        #endregion
    }
}
