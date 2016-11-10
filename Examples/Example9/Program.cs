using System;
using OpenGL;

namespace Example9
{
    static class Program
    {
        static void Main()
        {
            Window.CreateWindow("OpenGL UI: Example 9", 1280, 720);

            // add a reshape callback to update the UI
            Window.OnReshapeCallbacks.Add(() => OpenGL.UI.UserInterface.OnResize(Window.Width, Window.Height));

            // add a close callback to make sure we dispose of everything properly
            Window.OnCloseCallbacks.Add(OnClose);

            // enable depth testing to ensure correct z-ordering of our fragments
            Gl.Enable(EnableCap.DepthTest);
            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // initialize the user interface
            OpenGL.UI.UserInterface.InitUI(Window.Width, Window.Height);

            OpenGL.UI.Controls.Text text = new OpenGL.UI.Controls.Text(OpenGL.UI.Controls.Text.FontSize._16pt, "Type Something:");
            text.RelativeTo = OpenGL.UI.Corner.Center;
            text.Position = new OpenGL.UI.Point(-260, -10);

            // create a text input control
            OpenGL.UI.Controls.TextInput textInput = new OpenGL.UI.Controls.TextInput(OpenGL.UI.Controls.BMFont.LoadFont("fonts/font16.fnt"));
            textInput.Size = new OpenGL.UI.Point(300, 20);
            textInput.Position = new OpenGL.UI.Point(50, 0);
            textInput.RelativeTo = OpenGL.UI.Corner.Center;
            textInput.BackgroundColor = new Vector4(0.3f, 0.3f, 0.3f, 1.0f);

            // add the text input control to the user interface
            OpenGL.UI.UserInterface.AddElement(textInput);
            OpenGL.UI.UserInterface.AddElement(text);

            // subscribe the escape event using the OpenGL.UI class library
            OpenGL.UI.Input.Subscribe((char)27, Window.OnClose);

            while (true)
            {
                Window.HandleEvents();
                OnRenderFrame();
            }
        }

        private static void OnClose()
        {
            // make sure to dispose of everything
            OpenGL.UI.UserInterface.Dispose();
            OpenGL.UI.Controls.BMFont.Dispose();
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
    }
}
