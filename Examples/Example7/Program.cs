using System;
using OpenGL;

namespace Example7
{
    static class Program
    {
        private static Texture scrollTexture;

        static void Main()
        {
            Window.CreateWindow("OpenGL UI: Example 7", 1280, 720);

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
            OpenGL.UI.Controls.TextBox textBox = new OpenGL.UI.Controls.TextBox(OpenGL.UI.Controls.BMFont.LoadFont("fonts/font16.fnt"), scrollTexture);
            textBox.RelativeTo = OpenGL.UI.Corner.Center;
            textBox.Size = new OpenGL.UI.Point(400, 200);
            textBox.BackgroundColor = new Vector4(0.3f, 0.3f, 0.3f, 1.0f);

            // put a bunch of text into the textbox
            textBox.WriteLine("Hello!");
            textBox.WriteLine("This is a textbox, and it supports automatic word wrapping!");
            textBox.WriteLine(new Vector3(0.5f, 0.7f, 1.0f), "It also supports colors!");
            textBox.Write("It even supports ");
            textBox.Write(new Vector3(1, 0, 0), "multiple ");
            textBox.Write(new Vector3(0, 1, 0), "colors ");
            textBox.Write(new Vector3(0, 0, 1), "on ");
            textBox.WriteLine("the same line!");
            textBox.WriteLine("It also supports a scroll bar and the ability to scroll through lots of text!");
            textBox.WriteLine("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis eget vehicula orci. Nulla nibh nulla, suscipit non neque sed, placerat efficitur velit. Sed convallis gravida tincidunt. Praesent vehicula nibh leo, at consequat nisi condimentum ullamcorper. Vivamus pulvinar accumsan maximus. Integer luctus elit porttitor nisi sollicitudin, eget porttitor odio tincidunt. Sed elit justo, suscipit non dui ut, eleifend euismod nibh. Nullam ac fermentum nisl. In convallis id leo sit amet eleifend. Suspendisse eu ligula pulvinar ex facilisis cursus ac ac velit. In ut turpis nec neque vehicula eleifend. Morbi in tempus est. Vivamus nisi nunc, pharetra quis scelerisque ut, tempor id dui.");
            textBox.AllowScrollBar = true;
            textBox.CurrentLine = 0;

            // add the textbox to the user interface
            OpenGL.UI.UserInterface.AddElement(textBox);

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
