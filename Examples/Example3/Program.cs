using System;

using OpenGL;
using OpenGL.Platform;

namespace Example3
{
    static class Program
    {
        static void Main()
        {
            Window.CreateWindow("OpenGL UI: Example 3", 1280, 720);

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
            OpenGL.UI.Text selectText = new OpenGL.UI.Text(OpenGL.UI.Text.FontSize._16pt, "Pick A Color:", OpenGL.UI.BMFont.Justification.Center);
            selectText.Position = new Point(0, 80);
            selectText.RelativeTo = OpenGL.UI.Corner.Center;

            // add the two text object to the UI
            OpenGL.UI.UserInterface.AddElement(selectText);

            // create the color picker itself
            OpenGL.UI.ColorGradient gradient = new OpenGL.UI.ColorGradient();
            gradient.RelativeTo = OpenGL.UI.Corner.Center;
            gradient.Position = new Point(-20, 0);
            gradient.OnColorChange = (sender, e) => selectText.Color = gradient.Color;

            // and create a hue slider that can control the types of colors shown in the color picker
            OpenGL.UI.HueGradient hue = new OpenGL.UI.HueGradient();
            hue.RelativeTo = OpenGL.UI.Corner.Center;
            hue.Position = new Point(80, 0);

            // add the color picker and its hue slider to the UI
            OpenGL.UI.UserInterface.AddElement(gradient);
            OpenGL.UI.UserInterface.AddElement(hue);

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
            Gl.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // draw the user interface after everything else
            OpenGL.UI.UserInterface.Draw();

            // finally, swap the back buffer to the front so that the screen displays
            Window.SwapBuffers();
        }
    }
}
