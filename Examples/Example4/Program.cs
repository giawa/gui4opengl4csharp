using System;

using OpenGL;
using OpenGL.Platform;

namespace Example4
{
    static class Program
    {
        private static Texture menuTexture, menuSelectedTexture;

        static void Main()
        {
            Window.CreateWindow("OpenGL UI: Example 4", 1280, 720);

            // add a reshape callback to update the UI
            Window.OnReshapeCallbacks.Add(() => OpenGL.UI.UserInterface.OnResize(Window.Width, Window.Height));

            // add a close callback to make sure we dispose of everything properly
            Window.OnCloseCallbacks.Add(OnClose);

            // enable depth testing to ensure correct z-ordering of our fragments
            Gl.Enable(EnableCap.DepthTest);
            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // initialize the user interface
            OpenGL.UI.UserInterface.InitUI(Window.Width, Window.Height);

            // create a container that will store all of our color picker content
            OpenGL.UI.UIContainer colorPickerContainer = new OpenGL.UI.UIContainer();
            colorPickerContainer.Size = new Point(240, 190);
            colorPickerContainer.Position = new Point(20, 20);
            colorPickerContainer.RelativeTo = OpenGL.UI.Corner.TopLeft;

            // create a menu bar that will have two different textures
            menuTexture = new Texture("data/menu.png");
            menuSelectedTexture = new Texture("data/menuSelected.png");

            OpenGL.UI.Controls.Button menu = new OpenGL.UI.Controls.Button(menuTexture);
            colorPickerContainer.AddElement(menu);

            // place some text within the menu bar
            OpenGL.UI.Controls.Text menuText = new OpenGL.UI.Controls.Text(OpenGL.UI.Controls.Text.FontSize._12pt, "Color Picker");
            menuText.RelativeTo = OpenGL.UI.Corner.TopLeft;
            menuText.Position = new Point(4, 17);
            colorPickerContainer.AddElement(menuText);

            // add some events that will move the entire color picker container with the menu bar
            bool moving = false;
            menu.OnMouseDown = (sender, e) =>
                {
                    moving = true;
                    menu.BackgroundTexture = menuSelectedTexture;   // make it look nice by swapping the menubar texture
                };
            menu.OnMouseUp = (sender, e) =>
                {
                    moving = false;
                    menu.BackgroundTexture = menuTexture;   // make sure to restore the menubar texture
                };
            menu.OnMouseMove = (sender, e) =>
                {
                    if (moving)
                    {
                        int x = colorPickerContainer.Position.x + OpenGL.UI.UserInterface.MousePosition.x - OpenGL.UI.UserInterface.LastMousePosition.x;
                        int y = colorPickerContainer.Position.y + OpenGL.UI.UserInterface.MousePosition.y - OpenGL.UI.UserInterface.LastMousePosition.y;
                        colorPickerContainer.Position = new Point(x, y);
                        colorPickerContainer.OnResize();
                    }
                };

            // create the color picker itself
            OpenGL.UI.Controls.ColorGradient gradient = new OpenGL.UI.Controls.ColorGradient();
            gradient.Position = new Point(30, 30);

            // and create a hue slider that can control the types of colors shown in the color picker
            OpenGL.UI.Controls.HueGradient hue = new OpenGL.UI.Controls.HueGradient();
            hue.Position = new Point(190, 30);

            // add the color picker and its hue slider to the UI
            colorPickerContainer.AddElement(gradient);
            colorPickerContainer.AddElement(hue);

            // add the entire container to the user interface
            OpenGL.UI.UserInterface.AddElement(colorPickerContainer);

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
            menuTexture.Dispose();
            menuSelectedTexture.Dispose();
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
