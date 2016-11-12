using System;

using OpenGL;
using OpenGL.Platform;

namespace Example6
{
    static class Program
    {
        private static Texture checkBoxTexture, checkBoxCheckedTexture;

        static void Main()
        {
            Window.CreateWindow("OpenGL UI: Example 6", 1280, 720);

            // add a reshape callback to update the UI
            Window.OnReshapeCallbacks.Add(() => OpenGL.UI.UserInterface.OnResize(Window.Width, Window.Height));

            // add a close callback to make sure we dispose of everything properly
            Window.OnCloseCallbacks.Add(OnClose);

            // enable depth testing to ensure correct z-ordering of our fragments
            Gl.Enable(EnableCap.DepthTest);
            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // initialize the user interface
            OpenGL.UI.UserInterface.InitUI(Window.Width, Window.Height);

            // create a checkbox with a specified texture
            checkBoxTexture = new Texture("data/checkBox.png");
            checkBoxCheckedTexture = new Texture("data/checkBoxChecked.png");

            OpenGL.UI.Controls.CheckBox checkbox1 = new OpenGL.UI.Controls.CheckBox(checkBoxTexture, checkBoxCheckedTexture, OpenGL.UI.Controls.BMFont.LoadFont("fonts/font16.fnt"), "Check Me!");
            checkbox1.RelativeTo = OpenGL.UI.Corner.Center;
            checkbox1.Position = new Point(-50, 12);
            checkbox1.OnCheckedChanged = (sender, e) => checkbox1.Text = (checkbox1.Checked ? "Thanks!" : "Check Me!");

            OpenGL.UI.Controls.CheckBox checkbox2 = new OpenGL.UI.Controls.CheckBox(checkBoxTexture, checkBoxCheckedTexture, OpenGL.UI.Controls.BMFont.LoadFont("fonts/font16.fnt"), "Check Me Too!");
            checkbox2.RelativeTo = OpenGL.UI.Corner.Center;
            checkbox2.Position = new Point(-50, -12);
            checkbox2.OnCheckedChanged = (sender, e) => checkbox2.Text = (checkbox2.Checked ? "Thanks!" : "Check Me Too!");

            // add both checkbox controls to the user interface
            OpenGL.UI.UserInterface.AddElement(checkbox1);
            OpenGL.UI.UserInterface.AddElement(checkbox2);

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
            checkBoxTexture.Dispose();
            checkBoxCheckedTexture.Dispose();
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
