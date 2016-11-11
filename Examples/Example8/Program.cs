using System;
using OpenGL;

namespace Example8
{
    static class Program
    {
        private static Texture scrollTexture;

        static void Main()
        {
            Window.CreateWindow("OpenGL UI: Example 8", 1280, 720);

            // add a reshape callback to update the UI
            Window.OnReshapeCallbacks.Add(() => OpenGL.UI.UserInterface.OnResize(Window.Width, Window.Height));

            // add a close callback to make sure we dispose of everything properly
            Window.OnCloseCallbacks.Add(OnClose);

            // enable depth testing to ensure correct z-ordering of our fragments
            Gl.Enable(EnableCap.DepthTest);
            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // initialize the user interface
            OpenGL.UI.UserInterface.InitUI(Window.Width, Window.Height);

            // load a texture that we'll use for the scroll bar of the textbox
            scrollTexture = new Texture("data/scrollBar.png");

            // create a textbox
            OpenGL.UI.Controls.ListBox listBox = new OpenGL.UI.Controls.ListBox(scrollTexture, scrollTexture, OpenGL.UI.Controls.BMFont.LoadFont("fonts/font16.fnt"), new string[] { "Item 1", "Item 2", "Item 3" });
            listBox.RelativeTo = OpenGL.UI.Corner.Center;
            listBox.Size = new OpenGL.UI.Point(200, 20);
            listBox.BackgroundColor = new Vector4(0.3f, 0.3f, 0.3f, 1.0f);
            listBox.Invalidate();
            
            // add the list box to the user interface
            OpenGL.UI.UserInterface.AddElement(listBox);

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
            scrollTexture.Dispose();
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
