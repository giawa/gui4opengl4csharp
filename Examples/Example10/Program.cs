using System;

using OpenGL;
using OpenGL.Platform;

namespace Example10
{
    static class Program
    {
        private static Texture scrollTexture;
        private static OpenGL.UI.Button divider;
        private static OpenGL.UI.UIContainer leftContainer, rightContainer;

        static void Main()
        {
            Window.CreateWindow("OpenGL UI: Example 10", 1280, 720);

            // add a reshape callback to update the UI
            Window.OnReshapeCallbacks.Add(() => OpenGL.UI.UserInterface.OnResize(Window.Width, Window.Height));

            // add a close callback to make sure we dispose of everything properly
            Window.OnCloseCallbacks.Add(OnClose);

            // enable depth testing to ensure correct z-ordering of our fragments
            Gl.Enable(EnableCap.DepthTest);
            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // initialize the user interface
            OpenGL.UI.UserInterface.InitUI(Window.Width, Window.Height);

            // create the left and right containers
            leftContainer = new OpenGL.UI.UIContainer(new Point(Window.Width / 2, Window.Height), "LeftContainer");
            rightContainer = new OpenGL.UI.UIContainer(new Point(Window.Width / 2, Window.Height), "RightContainer");

            leftContainer.RelativeTo = OpenGL.UI.Corner.BottomLeft;
            rightContainer.RelativeTo = OpenGL.UI.Corner.BottomRight;

            // add the containers to the user interface
            OpenGL.UI.UserInterface.AddElement(leftContainer);
            OpenGL.UI.UserInterface.AddElement(rightContainer);

            // add some cool stuff to the containers
            scrollTexture = new Texture("data/scrollTexture.png");

            // create a textbox with word wrapping for the left container
            OpenGL.UI.TextBox textBox = new OpenGL.UI.TextBox(OpenGL.UI.BMFont.LoadFont("fonts/font16.fnt"), scrollTexture);
            textBox.Write("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis eget vehicula orci. Nulla nibh nulla, suscipit non neque sed, placerat efficitur velit. Sed convallis gravida tincidunt. Praesent vehicula nibh leo, at consequat nisi condimentum ullamcorper. Vivamus pulvinar accumsan maximus. Integer luctus elit porttitor nisi sollicitudin, eget porttitor odio tincidunt. Sed elit justo, suscipit non dui ut, eleifend euismod nibh. Nullam ac fermentum nisl. In convallis id leo sit amet eleifend. Suspendisse eu ligula pulvinar ex facilisis cursus ac ac velit. In ut turpis nec neque vehicula eleifend. Morbi in tempus est. Vivamus nisi nunc, pharetra quis scelerisque ut, tempor id dui.");
            textBox.RelativeTo = OpenGL.UI.Corner.Fill;
            textBox.Padding = new Point(6, 0);
            leftContainer.AddElement(textBox);
            textBox.OnResize();

            // build 10 buttons for the right container
            for (int i = 0; i < 10; i++)
            {
                OpenGL.UI.Button button = new OpenGL.UI.Button(200, 30);
                button.Font = OpenGL.UI.BMFont.LoadFont("fonts/font16.fnt");
                button.Text = string.Format("Button {0}", i);
                button.RelativeTo = OpenGL.UI.Corner.Center;
                button.Position = new Point(0, 200 - i * 40);
                rightContainer.AddElement(button);
                button.OnResize();

                button.OnMouseEnter = (sender, e) => button.BackgroundColor = new Vector4(0.5f, 0.5f, 1.0f, 1.0f);
                button.OnMouseLeave = (sender, e) => button.BackgroundColor = new Vector4(0.3f, 0.3f, 0.3f, 1.0f);
            }

            // add a control to resize the left/right containers
            divider = new OpenGL.UI.Button(8, Window.Height);
            divider.BackgroundColor = new Vector4(0.3f, 0.3f, 0.3f, 0.5f);
            divider.RelativeTo = OpenGL.UI.Corner.BottomLeft;
            divider.Position = new Point(Window.Width / 2 - divider.Size.X / 2, 0);

            bool onMouseDown = false;

            // handle mouse events on the divider
            divider.OnMouseDown = (sender, e) => onMouseDown = true;
            divider.OnMouseUp = (sender, e) => onMouseDown = false;
            divider.OnMouseMove = (sender, e) =>
            {
                if (onMouseDown) ResizeControls(e.Location.X - e.LastLocaton.X);
            };

            // make sure to layout the controls if the window is resized
            Window.OnReshapeCallbacks.Add(() => ResizeControls(0));

            // add the divider to the user interface
            OpenGL.UI.UserInterface.AddElement(divider);

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

        private static void ResizeControls(int dx)
        {
            int x = divider.Position.X + dx;// (e.Location.X - e.LastLocaton.X);
            x = Math.Min(Window.Width - 110, x);

            // set the position of the divider
            divider.Position = new Point(x, divider.Position.Y);

            // update the container sizes
            x += divider.Size.X / 2;
            leftContainer.Size = new Point(x, Window.Height);
            rightContainer.Size = new Point(Window.Width - x, Window.Height);

            // resize the buttons to make them fit in the right container
            foreach (var button in rightContainer.Elements)
                button.Size = new Point(Math.Min(200, rightContainer.Size.X - 16), 30);

            OpenGL.UI.UserInterface.UIWindow.OnResize();
        }

        private static void OnClose()
        {
            // make sure to dispose of everything
            OpenGL.UI.UserInterface.Dispose();
            OpenGL.UI.BMFont.Dispose();
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
