using System;
using System.Collections.Generic;

using OpenGL;

namespace OpenGL.UI.Controls
{
    public class Console : UIContainer
    {
        private TextBox textBox;
        private TextEntry textEntry;

        public Console(BMFont font)
            : base(new Point(0, 0), new Point(500, 300), "Console" + UserInterface.GetUniqueElementID())
        {
            textBox = new TextBox(font);
            textBox.RelativeTo = Corner.TopLeft;
            textBox.BackgroundColor = new Vector4(0, 0, 0, 0.5f);
            textBox.AllowScrollBar = true;
            this.AddElement(textBox);

            textEntry = new TextEntry(font);
            textEntry.RelativeTo = Corner.BottomLeft;
            textEntry.BackgroundColor = new Vector4(0, 0, 0, 0.7f);
            this.AddElement(textEntry);

            textEntry.OnCarriageReturn = new TextEntry.OnTextEvent(ExecuteCommand);
        }

        public override void OnResize()
        {
            base.OnResize();

            textBox.Size = new Point(Size.x, Size.y - textBox.Font.Height);
            textEntry.Size = new Point(Size.x, textBox.Font.Height);
        }

        public delegate void OnCommand(string args);

        public Dictionary<string, OnCommand> commands = new Dictionary<string, OnCommand>();

        private void ExecuteCommand(TextEntry entry, string command)
        {
            if (command.Length == 0) return;
            entry.Clear();

            textBox.Write(new Vector3(1, 1, 1), "> ");
            textBox.WriteLine(new Vector3(0, 0.6f, 0.9f), command);

            try
            {
                if (command.Contains(" "))
                {
                    int i = command.IndexOf(" ");
                    string opcode = command.Substring(0, i);

                    if (commands.ContainsKey(opcode)) commands[opcode](command.Substring(i + 1));
                    else textBox.WriteLine(new Vector3(1f, 0, 0), "Unknown command");
                }
                else
                {
                    if (commands.ContainsKey(command)) commands[command]("");
                    else textBox.WriteLine(new Vector3(1f, 0, 0), "Unknown command");
                }
            }
            catch (Exception e)
            {
                textBox.WriteLine(new Vector3(1f, 0, 0), "Exception while running command.  " + e.Message);
            }
        }

        #region Write Methods
        public void Write(string message)
        {
            Write(Vector3.One, message);
        }

        public void WriteLine(string message)
        {
            textBox.WriteLine(message);
        }

        public void Write(Vector3 color, string message)
        {
            textBox.Write(color, message);
        }

        public void WriteLine(Vector3 color, string message)
        {
            textBox.WriteLine(color, message);
        }

        public void Write(Vector3 color, string message, BMFont customFont)
        {
            textBox.Write(color, message, customFont);
        }

        public void WriteLine(Vector3 color, string message, BMFont customFont)
        {
            textBox.WriteLine(color, message, customFont);
        }

        public void Clear()
        {
            textBox.Clear();
        }
        #endregion
    }
}
