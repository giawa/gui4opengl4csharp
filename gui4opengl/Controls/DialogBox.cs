using System;
using OpenGL;

namespace OpenGL.UI.Controls
{
    public class DialogBox : UIContainer
    {
        #region Fields
        private Text text;
        private TextBox textbox;
        private TextBox responseBox;
        #endregion

        #region Properties
        /// <summary>
        /// The name of the person speaking.
        /// </summary>
        public string Title
        {
            get { return text.String; }
            set { text.String = value; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a text dialog box, which is what is used for character communication.
        /// This dialog box consists of a text box for the main content, a text element
        /// for the 'title' (the name of the player talking) and then an optional textbox
        /// for the valid user responses (if there are any).
        /// </summary>
        /// <param name="titleFont">The font to use for the 'title' (name of player talking).</param>
        /// <param name="contentsFont">The font to use for the textbox and responses.</param>
        /// <param name="texture">The background texture to use, which also determines the size of the dialog box.</param>
        public DialogBox(BMFont titleFont, BMFont contentsFont, Texture texture)
            : base(new Point(texture.Size.Width, texture.Size.Height), "DialogBox" + UserInterface.GetUniqueElementID())
        {
            this.BackgroundTexture = texture;

            text = new Text(Shaders.FontShader, titleFont, "", BMFont.Justification.Center);
            text.RelativeTo = Corner.TopLeft;

            textbox = new TextBox(contentsFont);
            textbox.RelativeTo = Corner.TopLeft;

            OnResize();

            this.AddElement(text);
            this.AddElement(textbox);

            // the dialog box takes UI priority
            /*Input.ActiveContainer.Push(this);
            Input.PushKeyBindings();

            Input.Subscribe((char)13, new Event(() =>
            {
                if (!textbox.TextIsVisible) textbox.TextIsVisible = true;
                else if (OnSelectionPicked != null) OnSelectionPicked(this, new MouseEventArgs(new Point(0, 0)));
            }));*/

            textbox.OnMouseClick = new OnMouse((sender, e) =>
            {
                if (!textbox.TextIsVisible) textbox.TextIsVisible = true;
                else if (OnSelectionPicked != null) OnSelectionPicked(this, new MouseEventArgs(new Point(0, 0)));
            });
        }
        #endregion

        #region User Responses
        private string[] responses;

        /// <summary>
        /// The index into the Responses array of the response chosen
        /// by the user, or -1 if nothing was chosen.
        /// </summary>
        public int Response
        {
            get
            {
                if (responseBox == null || responses == null || responses.Length == 0) return -1;
                else return responseBox.SelectedLine;
            }
        }

        /// <summary>
        /// The text of the response that was chosen by the user,
        /// or String.Empty if nothing was chosen.
        /// </summary>
        public string ResponseText
        {
            get
            {
                if (responseBox == null) return String.Empty;
                else return responseBox.SelectedLineText;
            }
        }

        /// <summary>
        /// An array of valid responses to the text presented in the dailog box.
        /// Will call OnSelectionPicked when a selection is made.
        /// </summary>
        public string[] Responses
        {
            get { return responses; }
            set
            {
                responses = value;
                
                if (responseBox == null)
                {
                    responseBox = new TextBox(textbox.Font);
                    responseBox.RelativeTo = Corner.TopRight;
                    responseBox.Position = new Point(0, 0);
                    responseBox.AllowSelection = true;
                }
                else
                {
                    this.RemoveElement(responseBox);
                }

                if (responses == null || responses.Length == 0) return;

                if (value.Length > 3)
                    throw new ArgumentOutOfRangeException("Responses");

                responseBox.Clear();

                foreach (var response in responses) responseBox.WriteLine(response);

                responseBox.CurrentLine = responseBox.SelectedLine = 0;
                responseBox.Size = new Point(200, (int)(textbox.Font.Height * responses.Length * 1.2));

                textbox.OnTextVisible = new OnMouse((sender, e) =>
                {
                    textbox.OnTextVisible = null;
                    this.AddElement(responseBox);
                    responseBox.OnResize();

                    // set up some basic keyboard bindings for controlling the response textbox
                    //Input.Subscribe((char)2, new Event(() => responseBox.SelectedLine = Math.Max(0, responseBox.SelectedLine - 1)));
                    //Input.Subscribe((char)3, new Event(() => responseBox.SelectedLine = Math.Min(responses.Length - 1, responseBox.SelectedLine + 1)));
                });
            }
        }

        /// <summary>
        /// An event that is called when a response is picked.
        /// </summary>
        public OnMouse OnSelectionPicked { get; set; }
        #endregion

        #region UIContainer Overrides (OnResize and Dispose)
        public override void OnResize()
        {
            int xpadding = 50;// 80;
            int ytop = 90;
            int ybottom = 50;

            text.Position = new Point(xpadding + 100, ytop - 35);
            textbox.Position = new Point(xpadding, ytop);
            textbox.Size = new Point(Size.x - xpadding * 2, Size.y - ytop - ybottom);

            base.OnResize();
        }

        protected override void Dispose(bool disposing)
        {
            ReleaseKeyBindings();

            base.Dispose(disposing);
        }

        private void ReleaseKeyBindings()
        {
            /*if (Input.ActiveContainer.Count > 0 && Input.ActiveContainer.Peek() == this)
            {
                Input.ActiveContainer.Pop();
                Input.PopKeyBindings();
            }*/
        }
        #endregion

        #region TextBox Methods (Write, WriteLine, Clear, TimePerCharacter)
        public float TimePerCharacter
        {
            get { return textbox.TimePerCharacter; }
            set { textbox.TimePerCharacter = value; }
        }

        public void Clear()
        {
            if (textbox != null) textbox.Clear();
            if (responses != null) Responses = null;
        }

        public void Write(Vector3 color, string message)
        {
            textbox.Write(color, message);
        }

        public void Write(string message)
        {
            textbox.Write(message);
        }

        public void WriteLine(string message)
        {
            textbox.WriteLine(message);
        }

        public void WriteLine(Vector3 color, string message)
        {
            textbox.WriteLine(color, message);
        }

        public void Write(Vector3 color, string message, BMFont font)
        {
            textbox.Write(color, message, font);
        }

        public void Write(string message, BMFont font)
        {
            textbox.Write(Vector3.One, message, font);
        }

        public void WriteLine(Vector3 color, string message, BMFont font)
        {
            textbox.WriteLine(color, message, font);
        }

        public void WriteLine(string message, BMFont font)
        {
            textbox.WriteLine(Vector3.One, message, font);
        }
        #endregion
    }
}
