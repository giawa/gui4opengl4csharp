using System;

using OpenGL;
using OpenGL.Platform;

namespace Example2
{
    static class Program
    {
        private static Texture[] textures;

        static void Main()
        {
            Window.CreateWindow("OpenGL UI: Example 2", 1280, 720);

            // add a reshape callback to update the UI
            Window.OnReshapeCallbacks.Add(() => OpenGL.UI.UserInterface.OnResize(Window.Width, Window.Height));

            // add a close callback to make sure we dispose of everything properly
            Window.OnCloseCallbacks.Add(OnClose);

            // enable depth testing to ensure correct z-ordering of our fragments
            Gl.Enable(EnableCap.DepthTest);
            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // initialize the user interface
            OpenGL.UI.UserInterface.InitUI(Window.Width, Window.Height);

            // create some centered text
            OpenGL.UI.Text selectText = new OpenGL.UI.Text(OpenGL.UI.Text.FontSize._24pt, "Select A Character", OpenGL.UI.BMFont.Justification.Center);
            selectText.Position = new Point(0, 50);
            selectText.RelativeTo = OpenGL.UI.Corner.Center;

            OpenGL.UI.Text characterName = new OpenGL.UI.Text(OpenGL.UI.Text.FontSize._16pt, "", OpenGL.UI.BMFont.Justification.Center);
            characterName.RelativeTo = OpenGL.UI.Corner.Center;
            characterName.Position = new Point(0, -70);

            // add the two text object to the UI
            OpenGL.UI.UserInterface.AddElement(selectText);
            OpenGL.UI.UserInterface.AddElement(characterName);

            // the license for these icons is located in the data folder
            string[] characters = new string[] { "boy.png", "man.png", "girl1.png", "girl2.png", "girl3.png" };
            textures = new Texture[characters.Length];
            int xoffset = -characters.Length * 80 / 2 + 40;

            for (int i = 0; i < characters.Length; i++)
            {
                string character = characters[i];

                // load a texture that will be used by a button
                textures[i] = new Texture(string.Format("data/{0}", character));

                // create buttons in a row, each of which uses a Texture (the Texture gives the initial size of the Button in pixels)
                OpenGL.UI.Button button = new OpenGL.UI.Button(textures[i]);
                button.Position = new Point(xoffset, 5);
                button.RelativeTo = OpenGL.UI.Corner.Center;

                // change the color of the button when entering/leaving/clicking with the mouse
                button.OnMouseEnter = (sender, e) => button.BackgroundColor = new Vector4(0, 1f, 0.2f, 1.0f);
                button.OnMouseLeave = (sender, e) => button.BackgroundColor = Vector4.Zero;
                button.OnMouseDown = (sender, e) => button.BackgroundColor = new Vector4(0, 0.6f, 1f, 1f);
                button.OnMouseUp = (sender, e) => button.BackgroundColor = (OpenGL.UI.UserInterface.Selection == button ? new Vector4(0, 1f, 0.2f, 1.0f) : Vector4.Zero);

                // update the text with the character name when the button is clicked
                button.OnMouseClick = (sender, e) => characterName.String = string.Format("You selected {0}!", character);

                OpenGL.UI.UserInterface.AddElement(button);

                xoffset += 80;
            }

            // subscribe the escape event using the OpenGL.UI class library
            Input.Subscribe((char)27, Window.OnClose);

            // make sure to set up mouse event handlers for the window
            Window.OnMouseCallbacks.Add(OpenGL.UI.UserInterface.OnMouseClick);
            Window.OnMouseMoveCallbacks.Add(OpenGL.UI.UserInterface.OnMouseMove);

            while (Window.Open)
            {
                Window.HandleEvents();
                OnRenderFrame();
            }
        }

        private static void OnClose()
        {
            // make sure to dispose of everything
            OpenGL.UI.UserInterface.Dispose();
            OpenGL.UI.BMFont.Dispose();
            if (textures != null) foreach (var texture in textures) texture.Dispose();
        }

        private static void OnRenderFrame()
        {
            // set up the OpenGL viewport and clear both the color and depth bits
            Gl.Viewport(0, 0, Window.Width, Window.Height);
            Gl.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // draw the user interface after everything else
            OpenGL.UI.UserInterface.Draw();

            // finally, swap the back buffer to the front so that the screen displays
            Window.SwapBuffers();
        }

        private static void OnMouseClick(int button, int state, int x, int y)
        {
            // take care of mapping the Glut buttons to the UI enums
            if (!OpenGL.UI.UserInterface.OnMouseClick(button + 1, (state == 0 ? 1 : 0), x, y))
            {
                // do other picking code here if necessary
            }
        }

        private static void OnMouseMove(int x, int y)
        {
            if (!OpenGL.UI.UserInterface.OnMouseMove(x, y))
            {
                // do other picking code here if necessary
            }
        }
    }
}
