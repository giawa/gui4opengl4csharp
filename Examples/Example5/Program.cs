using System;

using OpenGL;
using OpenGL.Platform;

namespace Example5
{
    static class Program
    {
        private static Texture sliderTexture;

        static void Main()
        {
            Window.CreateWindow("OpenGL UI: Example 5", 1280, 720);

            // add a reshape callback to update the UI
            Window.OnReshapeCallbacks.Add(() => OpenGL.UI.UserInterface.OnResize(Window.Width, Window.Height));

            // add a close callback to make sure we dispose of everything properly
            Window.OnCloseCallbacks.Add(OnClose);

            // enable depth testing to ensure correct z-ordering of our fragments
            Gl.Enable(EnableCap.DepthTest);
            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // initialize the user interface
            OpenGL.UI.UserInterface.InitUI(Window.Width, Window.Height);

            // create a slider with a specified texture
            sliderTexture = new Texture("data/slider.png");

            OpenGL.UI.Controls.Slider slider = new OpenGL.UI.Controls.Slider(sliderTexture);
            slider.RelativeTo = OpenGL.UI.Corner.Center;
            slider.BackgroundColor = new Vector4(0.1f, 0.1f, 0.1f, 1f);
            slider.LockToSteps = true;

            // create some text that will change with the slider position
            OpenGL.UI.Controls.Text text = new OpenGL.UI.Controls.Text(OpenGL.UI.Controls.Text.FontSize._16pt, "Value: 0");
            text.RelativeTo = OpenGL.UI.Corner.Center;
            text.Position = new Point(120, -text.TextSize.y / 2);

            slider.OnValueChanged = (sender, e) => text.String = string.Format("Value: {0}", slider.Value);

            // add both the slider and text controls to the UI
            OpenGL.UI.UserInterface.AddElement(slider);
            OpenGL.UI.UserInterface.AddElement(text);

            // subscribe the escape event using the OpenGL.UI class library
            Input.Subscribe((char)27, Window.OnClose);

            // make sure to set up mouse event handlers for the window
            Window.OnMouseCallbacks.Add(OpenGL.UI.UserInterface.OnMouseClick);
            Window.OnMouseMoveCallbacks.Add(OpenGL.UI.UserInterface.OnMouseMove);

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
            sliderTexture.Dispose();
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
