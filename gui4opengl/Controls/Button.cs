using System;

using OpenGL;
using OpenGL.Platform;

namespace OpenGL.UI
{
    public class Button : UIElement
    {
        #region Properties
        public bool Enabled { get; set; }

        public Vector4 EnabledColor { get; set; }
        #endregion
        
        #region Text Support
        private Text text;
        private BMFont font;
        private string textString;

        public BMFont Font
        {
            get { return font; }
            set
            {
                if (text != null) text.Dispose();
                font = value;
                if (textString != null && textString.Length != 0) text = new Text(Shaders.FontShader, font, textString, BMFont.Justification.Center);
            }
        }

        public string Text
        {
            get { return textString; }
            set
            {
                textString = value;
                if (text == null)
                {
                    if (font != null)
                    {
                        text = new Text(Shaders.FontShader, font, textString, BMFont.Justification.Center);
                        text.Size = this.Size;
                    }
                }
                else text.String = textString;
            }
        }
        #endregion

        #region Constructors
        public Button(Texture texture)
        {
            this.BackgroundColor = Vector4.Zero;
            this.EnabledColor = Vector4.Zero;
            this.BackgroundTexture = texture;

            this.RelativeTo = Corner.TopLeft;
            this.Size = new Point(texture.Size.Width, texture.Size.Height);
        }

        public Button(int width, int height)
        {
            this.BackgroundColor = new Vector4(0.3f, 0.3f, 0.3f, 1f);
            this.EnabledColor = new Vector4(0.3f, 0.9f, 0.3f, 1f);

            this.RelativeTo = Corner.TopLeft;
            this.Size = new Point(width, height);
        }
        #endregion

        #region UIElement Overrides (OnResize, Dispose, Draw)
        public override void OnResize()
        {
            if (text != null) text.Size = this.Size;

            base.OnResize();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (text != null) text.Dispose();
        }

        public override void Draw()
        {
            base.DrawQuadColored(Enabled ? EnabledColor : BackgroundColor);
            if (BackgroundTexture != null) base.DrawQuadTextured();

            if (text != null)
            {
                text.CorrectedPosition = this.CorrectedPosition;
                text.Draw();
            }
        }
        #endregion
    }
}
