using System;

using OpenGL;
using OpenGL.Platform;

namespace OpenGL.UI
{
    public class CheckBox : UIContainer
    {
        #region Get/Set Font and Text
        private Text text;
        private BMFont font;
        private string textString;

        /// <summary>
        /// The font to use when rendering the descriptive text for this checkbox.
        /// Will rebuild the string VAO when modified.
        /// </summary>
        public BMFont Font
        {
            get { return font; }
            set
            {
                if (text != null) text.Dispose();
                font = value;
                if (textString != null && textString.Length != 0) this.Text = textString;
            }
        }

        /// <summary>
        /// The descriptive text to use for this checkbox (placed to the right of the checkbox).
        /// Will rebuild the string VAO when modified.
        /// </summary>
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
                        text = new Text(Shaders.FontShader, font, textString, BMFont.Justification.Left);
                        text.Size = new Point(0, this.Size.Y);
                        text.RelativeTo = Corner.BottomLeft;
                        text.Position = new Point(UncheckedTexture.Size.Width + 6, Size.Y / 2 - text.TextSize.Y / 2);

                        this.AddElement(text);
                    }
                }
                else text.String = textString;
            }
        }

        /// <summary>
        /// An event that is fired when the checkbox changes state.
        /// </summary>
        public OnMouse OnCheckedChanged { get; set; }
        #endregion

        #region Checkbox Textures and State
        private Button checkBox;
        private bool isChecked;

        /// <summary>
        /// Texture to use when the checkbox is unchecked.
        /// Should be the same size as CheckedTexture as well as the CheckBox.Size property.
        /// </summary>
        public Texture UncheckedTexture { get; set; }

        /// <summary>
        /// Texture to use when the checkbox is checked.
        /// Should be the same size as UncheckedTexture as well as the CheckBox.Size property.
        /// </summary>
        public Texture CheckedTexture { get; set; }

        /// <summary>
        /// True if the checkbox is currently checked.
        /// False if the checkbox is unchecked.
        /// </summary>
        public bool Checked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                if (OnCheckedChanged != null) OnCheckedChanged(this, new MouseEventArgs());

                if (isChecked) checkBox.BackgroundTexture = CheckedTexture;
                else checkBox.BackgroundTexture = UncheckedTexture;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a simple checkbox user interface element which can be used
        /// for setting/modifying the state of a boolean value.
        /// This can be useful in preferences as well as other places in the user interface.
        /// </summary>
        /// <param name="uncheckedTexture">A texture to use when the checkbox is unchecked.  Should be the same size as checkedTexture.</param>
        /// <param name="checkedTexture">A texture to use when the checkbox is checked.  Should be the same size as uncheckedTexture.</param>
        /// <param name="font">A BMFont to use when rendering the descriptive text of this checkbox.</param>
        /// <param name="text">The descriptive text to render to the right of this checkbox.</param>
        public CheckBox(Texture uncheckedTexture, Texture checkedTexture, BMFont font, string text)
            : base(new Point(0, 0), new Point(uncheckedTexture.Size.Width, uncheckedTexture.Size.Height), "CheckBox" + UserInterface.GetUniqueElementID())
        {
            this.UncheckedTexture = uncheckedTexture;
            this.CheckedTexture = checkedTexture;

            checkBox = new Button(UncheckedTexture);
            checkBox.BackgroundColor = new Vector4(0, 0, 0, 0);
            checkBox.RelativeTo = Corner.BottomLeft;
            checkBox.Size = new Point(uncheckedTexture.Size.Width, uncheckedTexture.Size.Height);
            this.AddElement(checkBox);

            this.Font = font;
            this.Text = text;

            checkBox.OnMouseClick = new OnMouse((o, e) => this.Checked = !this.Checked);
        }
        #endregion
    }
}
