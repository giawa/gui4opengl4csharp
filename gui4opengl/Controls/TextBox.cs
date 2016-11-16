using System;
using System.Collections.Generic;

using OpenGL;
using OpenGL.Platform;

namespace OpenGL.UI
{
    public class TextBox : UIElement
    {
        /// <summary>
        /// A simple class for storing information about the contents and color
        /// of each line in the textbox.  Internal use only.
        /// </summary>
        private class TextBoxEntry
        {
            public Vector3 Color;
            public string Text;
            public bool NewLine;
            public int Position;
            public BMFont Font;

            public TextBoxEntry(Vector3 color, string text, BMFont font, bool newLine = true, int position = 0)
            {
                this.Color = color;
                this.Text = text;
                this.NewLine = newLine;
                this.Position = position;
                this.Font = font;
            }
        }

        #region Variables
        private List<TextBoxEntry> text = new List<TextBoxEntry>();   // the unformatted text
        private List<List<TextBoxEntry>> lines = new List<List<TextBoxEntry>>();
        private List<Text> vaos = new List<Text>();                 // the text VAOs to draw
        private VAO selectedVAO;

        private bool dirty = false;
        private int currentLine;
        #endregion

        #region Properties
        /// <summary>
        /// The maximum number of lines of text that can be drawn into 
        /// this TextBox given the height and font size.
        /// </summary>
        public int MaximumLines { get; private set; }

        /// <summary>
        /// The total number of lines of text after formatting (this is how
        /// big the textbox would have to be to show all of the text contained within).
        /// </summary>
        public int LineCount { get { return lines.Count; } }
        
        /// <summary>
        /// The current line number of the first line of text in this textbox.
        /// This value will normally be zero unless the textbox has been scrolled.
        /// </summary>
        public int CurrentLine
        {
            get { return currentLine; }
            set
            {
                currentLine = value;
                dirty = true;
            }
        }

        private BMFont font;

        /// <summary>
        /// The font being used by this textbox.
        /// </summary>
        public BMFont Font
        {
            get { return font; }
            set
            {
                font = value;
                MaximumLines = (int)Math.Round(Size.Y / (font.Height * 1.2 + 1));
            }
        }

        /// <summary>
        /// Gets the contents of the currently selected line.
        /// </summary>
        public string SelectedLineText
        {
            get
            {
                int line = (selectedLine == -1 ? -1 : selectedLine);
                if (line < 0 || line >= lines.Count) return "";
                if (lines[line].Count == 0) return "";

                return lines[line][0].Text;
            }
        }

        private int selectedLine;

        public int SelectedLine
        {
            get { return selectedLine; }
            set
            {
                selectedLine = value;
                if (OnSelectionChanged != null) OnSelectionChanged(this, new MouseEventArgs());
            }
        }

        public Vector4 SelectedColor { get; set; }

        public OnMouse OnSelectionChanged { get; set; }

        public bool AllowSelection { get; set; }
        #endregion

        #region ScrollBar Support
        private static Texture scrollbarTexture;

        private int scrollBarDown = -1;
        private bool allowScrollBar, scrollBarMouseDown;
        private Button scrollBar;

        public Button ScrollBar { get { return scrollBar; } }

        /// <summary>
        /// Sets whether a scrollbar will be attached to this textbox.
        /// </summary>
        public bool AllowScrollBar
        {
            get { return allowScrollBar; }
            set
            {
                allowScrollBar = value;
                if (Parent == null) return;
                if (scrollBar == null) return;

                if (allowScrollBar && LineCount > MaximumLines) Parent.AddElement(scrollBar);
                else Parent.RemoveElement(scrollBar);
            }
        }

        /// <summary>
        /// Updates the position of the scrollbar.
        /// </summary>
        public void UpdateScrollBar()
        {
            if (LineCount <= MaximumLines) return;
            if (Parent == null) return;
            if (scrollBar == null) return;

            scrollBar.RelativeTo = Corner.BottomLeft;

            float percent = (float)CurrentLine / (this.LineCount - this.MaximumLines);
            int y = Size.Y - scrollBar.Size.Y;
            y -= (int)Math.Round(percent * (Size.Y - scrollBar.Size.Y));

            scrollBar.RelativeTo = Corner.BottomLeft;
            scrollBar.Position = CorrectedPosition - Parent.CorrectedPosition + new Point(Size.X, y);

            scrollBar.OnResize();
        }

        public Point Padding { get; set; }
        #endregion

        #region Constructor
        public TextBox(BMFont font, Texture scrollTexture, int selectedLine = -1)
        {
            this.Font = font;
            this.SelectedColor = new Vector4(0.3f, 0.9f, 0.3f, 1f);

            this.OnMouseDown = new OnMouse((sender, eventArgs) =>
                {
                    // find which line we're on
                    int y = (CorrectedPosition.Y + Size.Y) - (UserInterface.Height - eventArgs.Location.Y);
                    SelectedLine = currentLine + (int)(y / (Font.Height * 1.2));
                });

            // set up the scroll bar
            if (scrollbarTexture == null) scrollbarTexture = scrollTexture;
            this.scrollBar = new Button(scrollbarTexture);
            this.scrollBar.BackgroundColor = new Vector4(0, 0, 0, 0);
            this.scrollBar.Size = new Point(scrollBar.Size.X, scrollBar.Size.Y / 2);

            scrollBar.OnMouseUp = new OnMouse((sender, eventArgs) => scrollBarMouseDown = false);
            scrollBar.OnMouseDown = new OnMouse((sender, eventArgs) =>
                {
                    scrollBarMouseDown = (eventArgs.Button == MouseButton.Left);
                    scrollBarDown = eventArgs.Location.Y;
                });
            scrollBar.OnMouseMove = new OnMouse((sender, eventArgs) =>
            {
                if (!scrollBarMouseDown) return;

                int dy = scrollBarDown - eventArgs.Location.Y;

                int ymin = CorrectedPosition.Y - Parent.CorrectedPosition.Y;
                int ymax = ymin + Size.Y - ScrollBar.Size.Y;
                int y = Math.Min(ymax, Math.Max(ymin, scrollBar.Position.Y + dy));

                if (y == scrollBar.Position.Y) return;

                scrollBarDown = eventArgs.Location.Y;
                scrollBar.Position = new Point(scrollBar.Position.X, y);
                scrollBar.OnResize();

                double percent = ((ymax - y) / ((double)Size.Y - scrollBar.Size.Y));
                CurrentLine = (int)Math.Round((LineCount - MaximumLines) * percent);
            });
            scrollBar.OnLoseFocus = new OnFocus((o, e) =>
            {
                if (this.OnLoseFocus != null) this.OnLoseFocus(o, e);
            });
            this.OnMouseMove = (sender, eventArgs) => scrollBar.OnMouseMove(sender, eventArgs);
        }
        #endregion

        #region Build VAOs
        private void ParseText()
        {
            MaximumLines = (int)Math.Round(Size.Y / (Font.Height * 1.2 + 1));

            lines.Clear();

            List<TextBoxEntry> line = new List<TextBoxEntry>();
            int xpos = 0;

            // first lets break this up into lines to be drawn
            for (int i = 0; i < text.Count; i++)
            {
                if (text[i] == null || text[i].Text == null) continue;
                int w = text[i].Font.GetWidth(text[i].Text);

                // check if the text simply fits
                if (xpos + w + Padding.X * 2 <= Size.X)
                {
                    line.Add(text[i]);
                    text[i].Position = xpos;
                    xpos += w;

                    if (text[i].NewLine)
                    {
                        lines.Add(line);
                        line = new List<TextBoxEntry>();
                        xpos = 0;
                    }
                }
                else
                {
                    // find the length of the text
                    string remaining = text[i].Text;

                    while (remaining.Length > 0)
                    {
                        int maximumLength = 0, currentWidth = xpos + Padding.X * 2;

                        // find out where we have to chop the text to get it to fit
                        for (maximumLength = 0; maximumLength < remaining.Length; maximumLength++)
                        {
                            currentWidth += text[i].Font.GetWidth(remaining[maximumLength]);
                            if (currentWidth > Size.X)
                            {
                                maximumLength--;
                                break;
                            }
                        }

                        if (maximumLength <= 0) return;

                        // now search backwards for a space or tab
                        int actualBreakPoint = maximumLength;

                        if (remaining.Length > maximumLength)
                        {
                            for (; actualBreakPoint > 0; actualBreakPoint--)
                                if (remaining[actualBreakPoint] == ' ' || remaining[actualBreakPoint] == '\t')
                                    break;

                            // if we didn't find a space then just end the line at the maximum length
                            if (actualBreakPoint == 0) actualBreakPoint = maximumLength;
                        }

                        if (actualBreakPoint == -1)
                        {
                            line.Add(new TextBoxEntry(text[i].Color, remaining, text[i].Font, true, xpos));
                            remaining = String.Empty;
                        }
                        else if (actualBreakPoint != maximumLength || line.Count == 0)
                        {
                            line.Add(new TextBoxEntry(text[i].Color, remaining.Substring(0, actualBreakPoint), text[i].Font, true, xpos));
                            remaining = remaining.Substring(actualBreakPoint).TrimStart();
                        }

                        if (line.Count != 0)
                        {
                            lines.Add(line);
                            line = new List<TextBoxEntry>();
                            xpos = 0;
                        }
                    }
                }
            }

            if (line.Count != 0) lines.Add(line);

            dirty = true;
        }

        private void BuildVAOs()
        {
            // we're about the replace the VAO objects, so clear them first
            for (int i = 0; i < vaos.Count; i++)
            {
                vaos[i].Dispose();
                vaos[i] = null;
            }
            vaos.Clear();

            // check if we should show the scrollbar
            if (lines.Count > MaximumLines && allowScrollBar && scrollBar != null)
            {
                if (scrollBar.Name == null || !UserInterface.Elements.ContainsKey(scrollBar.Name)) 
                    Parent.AddElement(scrollBar);
            }
            else if (Parent != null && scrollBar != null) Parent.RemoveElement(scrollBar);

            // now build the VAO objects
            for (int i = CurrentLine; i <= MaximumLines + CurrentLine; i++)
            {
                if (i >= lines.Count || i < 0) break;
                if (lines[i].Count == 0) continue;

                var current = lines[i][0];
                string contents = "";

                for (int j = 0; j < lines[i].Count; j++)
                {
                    if (current.Color != lines[i][j].Color || current.Font != lines[i][j].Font)
                    {
                        if (contents.Length != 0) BuildVAO(current, contents);

                        current = lines[i][j];
                        contents = lines[i][j].Text;
                    }
                    else contents += lines[i][j].Text;
                }

                if (contents.Length != 0) BuildVAO(current, contents);
            }

            CalculateVisibilityTime();

            dirty = false;
        }

        private void CalculateVisibilityTime()
        {
            totalCharacters = 0;

            foreach (var vao in vaos)
                totalCharacters += vao.String.Length;

            visibilityTime = TimePerCharacter * totalCharacters;
        }

        private void BuildVAO(TextBoxEntry entry, string text)
        {
            Text temp = new Text(Shaders.FontShader, entry.Font, text, entry.Color);
            temp.Padding = new Point(entry.Position, 0);
            vaos.Add(temp);
        }
        #endregion

        #region Public Methods
        public override void OnResize()
        {
            base.OnResize();

            UpdateScrollBar();

            if (selectedVAO != null)
            {
                selectedVAO.DisposeChildren = true;
                selectedVAO.Dispose();
            }
            VBO<Vector3> vertex = new VBO<Vector3>(new Vector3[] { new Vector3(0, 0, 0), new Vector3(Size.X, 0, 0), new Vector3(Size.X, Font.Height * 1.2f, 0), new Vector3(0, Font.Height * 1.2f, 0) });
            VBO<int> elements = new VBO<int>(new int[] { 0, 1, 3, 1, 2, 3 }, BufferTarget.ElementArrayBuffer);
            selectedVAO = new VAO(Shaders.SolidUIShader, vertex, elements);

            ParseText();
        }

        public int VisibleCharacters { get; private set; }

        private float visibilityTime = 0f;
        private float currentTime = 0f;

        public float TimePerCharacter { get; set; }

        private int totalCharacters = 0;

        public OnMouse OnTextVisible { get; set; }

        public bool TextIsVisible 
        {
            get { return VisibleCharacters == 0; }
            set
            {
                if (value) currentTime = visibilityTime;
                else currentTime = 0;
            }
        }

        public override void Draw()
        {
            base.Draw();

            if (CurrentLine < 0) return;

            // if new text has been added then rebuild the VAOs
            if (dirty) BuildVAOs();

            if (TimePerCharacter > 0 && currentTime < visibilityTime)
            {
                //currentTime += Time.DeltaTime;

                VisibleCharacters = (int)Math.Max(1, (currentTime / visibilityTime * totalCharacters));
            }
            else
            {
                if (OnTextVisible != null) OnTextVisible(this, new MouseEventArgs(new Point(0, 0)));
                VisibleCharacters = 0;
            }

            int characterCount = 0;

            // now draw all of the text for this textbox
            for (int i = 0, v = 0; i < lines.Count - CurrentLine && v < vaos.Count; i++)
            {
                if (AllowSelection && (currentLine + i) == SelectedLine && selectedVAO != null)
                {
                    Shaders.SolidUIShader.Use();
                    Shaders.SolidUIShader["position"].SetValue(new Vector3(CorrectedPosition.X, CorrectedPosition.Y - (1.2f * (i + 1) * Font.Height - Size.Y), 0));
                    Shaders.SolidUIShader["color"].SetValue(SelectedColor);

                    selectedVAO.Draw();
                }

                for (int j = 0; j < lines[i + CurrentLine].Count; j++)
                {
                    if (v >= vaos.Count) break; // the VAO must not be up to date, avoid a crash here
                    
                    vaos[v].CorrectedPosition = new Point(CorrectedPosition.X + Padding.X, (int)(CorrectedPosition.Y - (1.2 * (i + 1) * Font.Height - Size.Y)));

                    if (VisibleCharacters <= 0 || characterCount + vaos[v].String.Length <= VisibleCharacters)
                    {
                        vaos[v].Draw();
                        characterCount += vaos[v].String.Length;
                    }
                    else
                    {
                        vaos[v].DrawWithCharacterCount(VisibleCharacters - characterCount);
                        characterCount = VisibleCharacters;
                    }

                    v++;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (scrollBar != null)
            {
                scrollBar.Dispose();
                scrollBar = null;
            }

            if (selectedVAO != null)
            {
                selectedVAO.DisposeChildren = true;
                selectedVAO.Dispose();
                selectedVAO = null;
            }

            if (scrollbarTexture != null)
            {
                scrollbarTexture.Dispose();
                scrollbarTexture = null;
            }

            foreach (var vao in vaos) vao.Dispose();
            vaos.Clear();
        }

        public void ScrollToEnd()
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId != UserInterface.MainThreadID)
                throw new InvalidOperationException("An attempt was made to modify a UI element off the main thread.");

            ParseText();
            if (LineCount > MaximumLines) CurrentLine = LineCount - MaximumLines;
            UpdateScrollBar();
        }

        public void Write(string message)
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId != UserInterface.MainThreadID)
                throw new InvalidOperationException("An attempt was made to modify a UI element off the main thread.");

            Write(Vector3.One, message);
            totalCharacters += message.Length;
        }

        public void Write(Vector3 color, string message)
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId != UserInterface.MainThreadID)
                throw new InvalidOperationException("An attempt was made to modify a UI element off the main thread.");

            text.Add(new TextBoxEntry(color, message, Font, false));
        }

        public void WriteLine(string message)
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == UserInterface.MainThreadID)
                WriteLineSafe(message);
            else
                this.Invoke(new OnInvoke(WriteLineSafe), message);
        }

        public void WriteLine(Vector3 color, string message)
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId != UserInterface.MainThreadID)
                throw new InvalidOperationException("An attempt was made to modify a UI element off the main thread.");

            text.Add(new TextBoxEntry(color, message, Font));
            ScrollToEnd();
        }

        public void Write(Vector3 color, string message, BMFont customFont)
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId != UserInterface.MainThreadID)
                throw new InvalidOperationException("An attempt was made to modify a UI element off the main thread.");

            text.Add(new TextBoxEntry(color, message, customFont, false));
        }

        public void WriteLine(Vector3 color, string message, BMFont customFont)
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId != UserInterface.MainThreadID)
                throw new InvalidOperationException("An attempt was made to modify a UI element off the main thread.");

            text.Add(new TextBoxEntry(color, message, customFont));
            ScrollToEnd();
        }

        private void WriteLineSafe(object message)
        {
            WriteLine(Vector3.One, (string)message);
        }

        public void Clear()
        {
            text.Clear();
            dirty = true;

            currentTime = 0f;
            totalCharacters = 0;
            visibilityTime = 0f;
        }
        #endregion
    }
}
