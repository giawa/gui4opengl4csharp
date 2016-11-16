using System;

using OpenGL;
using OpenGL.Platform;

namespace OpenGL.UI
{
    public class ListBox : UIContainer
    {
        #region Fields
        private string[] items;
        private Text text;
        private BMFont font;
        private Button dropDownToggle;
        private TextBox dropDownBox;
        private bool dropDownVisible;
        #endregion

        #region Properties
        /// <summary>
        /// Returns a clone of the items as they are in the drop down
        /// box at this time.  This is not modifiable!
        /// </summary>
        /// <returns></returns>
        public string[] GetItems()
        {
            return (string[])items.Clone();
        }

        /// <summary>
        /// Gets or sets the selected lin of the drop down textbox.
        /// </summary>
        public int SelectedLine
        {
            get { return dropDownBox.SelectedLine; }
            set 
            {
                dropDownBox.CurrentLine = Math.Max(0, Math.Min(dropDownBox.LineCount - 4, value));
                dropDownBox.SelectedLine = value; 
            }
        }

        /// <summary>
        /// Gets the selected line text from the drop down textbox.
        /// </summary>
        public string SelectedLineText
        {
            get { return dropDownBox.SelectedLineText; }
        }

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
                text = new Text(Shaders.FontShader, font, dropDownBox.SelectedLineText, BMFont.Justification.Left);
                text.RelativeTo = Corner.TopLeft;
                text.Position = new Point(0, text.TextSize.Y);
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// A list box with a textbox as the drop down text, a button for the toggle,
        /// and some text to display the currently selected line.
        /// </summary>
        /// <param name="dropDownIcon">The icon to use to the right of the text for displaying the textbox.</param>
        /// <param name="scrollTexture">The icon to use for the scroll bar.</param>
        /// <param name="font">The font to use for both the textbox and the text.</param>
        /// <param name="items">The items to place in the textbox.</param>
        /// <param name="selectedLine">The default selected line for the textbox.</param>
        public ListBox(Texture dropDownIcon, Texture scrollTexture, BMFont font, string[] items, int selectedLine = 0)
        {
            this.items = items;

            this.dropDownToggle = new Button(dropDownIcon);
            this.dropDownToggle.RelativeTo = Corner.TopRight;
            this.dropDownToggle.Position = new Point(0, (this.Size.Y - this.dropDownToggle.Size.Y)/ 2);
            this.AddElement(dropDownToggle);

            this.dropDownBox = new TextBox(font, scrollTexture, selectedLine);
            foreach (var item in items) dropDownBox.WriteLine(item);
            dropDownBox.CurrentLine = 0;
            this.dropDownBox.AllowSelection = true;

            dropDownToggle.OnMouseClick = new OnMouse((o, e) =>
                {
                    dropDownVisible = !dropDownVisible;

                    if (dropDownVisible)
                    {
                        Parent.AddElement(dropDownBox);
                        dropDownBox.AllowScrollBar = (items.Length > 4);
                    }
                    else
                    {
                        Parent.RemoveElement(dropDownBox);
                        dropDownBox.AllowScrollBar = false;
                    }
                });

            dropDownBox.OnSelectionChanged = new OnMouse((o, e) => text.String = dropDownBox.SelectedLineText);

            dropDownToggle.OnLoseFocus = new OnFocus(OnLoseFocusInternal);
            dropDownBox.OnLoseFocus = new OnFocus(OnLoseFocusInternal);
            this.OnLoseFocus = new OnFocus(OnLoseFocusInternal);

            // need to wait until after dropDownBox is initialized
            this.Font = font;
            this.SelectedLine = selectedLine;
            this.AddElement(text);
        }
        #endregion

        #region Methods (OnLoseFocus and Invalidate)
        /// <summary>
        /// Called the the control loses focus by the user clicking elsewhere.
        /// This takes care of hiding the drop down box.
        /// </summary>
        private void OnLoseFocusInternal(object sender, IMouseInput newFocus)
        {
            if (newFocus != dropDownToggle && newFocus != dropDownBox && newFocus != dropDownBox.ScrollBar)
            {
                if (dropDownVisible) Parent.RemoveElement(dropDownBox);
                dropDownBox.AllowScrollBar = false;
                dropDownToggle.Enabled = false;
                dropDownVisible = false;
            }
        }

        public override void Invalidate()
        {
            base.Invalidate();

            int numLines = Math.Min(items.Length, 4);
            dropDownBox.RelativeTo = this.RelativeTo;
            dropDownBox.Size = new Point(this.Size.X - 8, (int)Math.Round(font.Height * numLines * 1.2));
            dropDownBox.Position = new Point(this.Position.X, this.Position.Y + this.Size.Y);

            if (dropDownBox.RelativeTo == Corner.Center) dropDownBox.Position = new Point(dropDownBox.Position.X, (-this.Size.Y - dropDownBox.Size.Y) / 2);
            this.dropDownToggle.Position = new Point(0, (this.Size.Y - this.dropDownToggle.Size.Y) / 2);
        }
        #endregion
    }
}
