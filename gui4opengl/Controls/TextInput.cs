using System;

using OpenGL;
using OpenGL.Platform;

namespace OpenGL.UI
{
    public class TextInput : UIContainer
    {
        #region Variables
        private Text text;
        private bool hasFocus = false;
        #endregion

        #region Properties
        /// <summary>
        /// CarriageReturn callback delegate prototype.
        /// </summary>
        /// <param name="entry">The TextEntry that received the carriage return signal.</param>
        /// <param name="text">The text contained in the TextEntry when the carriage return signal was received.</param>
        public delegate void OnTextEvent(TextInput entry, string text);

        /// <summary>
        /// Event is called when the carriage return button (Enter on most keyboards) is pressed.
        /// </summary>
        public OnTextEvent OnCarriageReturn { get; set; }

        /// <summary>
        /// Event is called when any text is entered or deleted.
        /// </summary>
        public OnTextEvent OnTextEntry { get; set; }

        /// <summary>
        /// The contents of the TextEntry.
        /// </summary>
        public string String
        {
            get { return text.String; }
        }
        #endregion

        #region Constructor
        public TextInput(BMFont font, string s = "")
            : base(new Point(0, 0), new Point(200, font.Height), "TextEntry" + UserInterface.GetUniqueElementID())
        {
            text = new Text(Shaders.FontShader, font, s, BMFont.Justification.Left);
            text.RelativeTo = Corner.Fill;
            text.Padding = new Point(5, 0);

            this.OnMouseClick = new OnMouse((o, e) => text.OnMouseClick(o, e));

            text.OnMouseClick = new OnMouse((o, e) =>
                {
                    if (hasFocus) return;
                    hasFocus = true;

                    // take control of the key bindings
                    Input.PushKeyBindings();

                    Input.SubscribeAll(new Event((key, state) =>
                        {
                            // give 16 pixels of padding on the right
                            if (!state || text.TextSize.X > Size.X - 16) return;

                            text.String += key;

                            if (OnTextEntry != null) OnTextEntry(this, String);
                        }));

                    // delete key
                    Input.Subscribe((char)8, new Event(() =>
                        {
                            if (text.String.Length == 0) return;
                            else text.String = text.String.Substring(0, text.String.Length - 1);

                            if (OnTextEntry != null) OnTextEntry(this, String);
                        }));

                    // carriage return
                    Input.Subscribe((char)13, new Event(() =>
                        {
                            if (OnCarriageReturn != null) OnCarriageReturn(this, String);
                        }));

                    // escape key
                    Input.Subscribe((char)27, new Event(() => text.OnLoseFocus(null, null)));

                    // restore the key bindings when we lose focus
                    text.OnLoseFocus = new OnFocus((sender, newFocus) =>
                        {
                            if (!hasFocus) return;
                            hasFocus = false;

                            Input.PopKeyBindings();
                        }); 
                });

            this.AddElement(text);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (hasFocus)
            {
                hasFocus = false;

                //Input.PopKeyBindings();
            }
        }

        public void Clear()
        {
            text.String = String.Empty;
        }
        #endregion
    }
}
