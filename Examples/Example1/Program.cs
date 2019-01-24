using System;

using OpenGL;
using OpenGL.Platform;

namespace Example1
{
    static class Program
    {
        static void Main()
        {
            Window.CreateWindow("OpenGL UI: Example 1", 1280, 720);

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
            OpenGL.UI.Text welcome = new OpenGL.UI.Text(OpenGL.UI.Text.FontSize._24pt, "Welcome to OpenGL", OpenGL.UI.BMFont.Justification.Center);
            welcome.RelativeTo = OpenGL.UI.Corner.Center;

            // create some colored text
            OpenGL.UI.Text coloredText = new OpenGL.UI.Text(OpenGL.UI.Text.FontSize._24pt, "using C#", OpenGL.UI.BMFont.Justification.Center);
            coloredText.Position = new Point(0, -30);
            coloredText.Color = new Vector3(0.2f, 0.3f, 1f);
            coloredText.RelativeTo = OpenGL.UI.Corner.Center;

            // add the two text object to the UI
            OpenGL.UI.UserInterface.AddElement(welcome);
            OpenGL.UI.UserInterface.AddElement(coloredText);

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
        }

        private static void OnRenderFrame()
        {
            // set up the OpenGL viewport and clear both the color and depth bits
            Gl.Viewport(0, 0, Window.Width, Window.Height);
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
