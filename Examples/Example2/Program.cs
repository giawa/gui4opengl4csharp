using System;
using Tao.FreeGlut;
using OpenGL;

namespace Example1
{
    static class Program
    {
        private static int width = 1280 / 2, height = 720 / 2;
        private static Texture[] textures;

        static void Main()
        {
            // create an OpenGL window
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("OpenGL UI: Example 2");

            // provide the Glut callbacks that are necessary for running this tutorial
            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(() => { });    // only here for mac os x
            Glut.glutCloseFunc(OnClose);
            Glut.glutMouseFunc(OnMouseClick);
            Glut.glutMotionFunc(OnMouseMove);
            Glut.glutPassiveMotionFunc(OnMouseMove);
            Glut.glutReshapeFunc(OnResize);
            Glut.glutKeyboardFunc(OnKeyboard);

            // enable depth testing to ensure correct z-ordering of our fragments
            Gl.Enable(EnableCap.DepthTest);
            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // initialize the user interface
            OpenGL.UI.UserInterface.InitUI(width, height);

            // create some centered text
            OpenGL.UI.Controls.Text selectText = new OpenGL.UI.Controls.Text(OpenGL.UI.Controls.Text.FontSize._24pt, "Select A Character", OpenGL.UI.Controls.BMFont.Justification.Center);
            selectText.Position = new OpenGL.UI.Point(0, 50);
            selectText.RelativeTo = OpenGL.UI.Corner.Center;

            OpenGL.UI.Controls.Text characterName = new OpenGL.UI.Controls.Text(OpenGL.UI.Controls.Text.FontSize._16pt, "", OpenGL.UI.Controls.BMFont.Justification.Center);
            characterName.RelativeTo = OpenGL.UI.Corner.Center;
            characterName.Position = new OpenGL.UI.Point(0, -70);

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
                OpenGL.UI.Controls.Button button = new OpenGL.UI.Controls.Button(textures[i]);
                button.Position = new OpenGL.UI.Point(xoffset, 5);
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

            // enter the glut main loop (this is where the drawing happens)
            Glut.glutMainLoop();
        }

        private static void OnClose()
        {
            // make sure to dispose of everything
            OpenGL.UI.UserInterface.Dispose();
            OpenGL.UI.Controls.BMFont.Dispose();
            if (textures != null) foreach (var texture in textures) texture.Dispose();
        }

        private static void OnRenderFrame()
        {
            // set up the OpenGL viewport and clear both the color and depth bits
            Gl.Viewport(0, 0, width, height);
            Gl.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // draw the user interface after everything else
            OpenGL.UI.UserInterface.Draw();

            // finally, swap the back buffer to the front so that the screen displays
            Glut.glutSwapBuffers();
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

        private static void OnResize(int glutWidth, int glutHeight)
        {
            width = glutWidth;
            height = glutHeight;

            // make sure the user interface orthographic matrix is updated and resize events are called
            OpenGL.UI.UserInterface.OnResize(glutWidth, glutHeight);
        }

        private static void OnKeyboard(byte key, int x, int y)
        {
            // exit the program if the ESC key is pressed
            if (key == 27) Glut.glutLeaveMainLoop();
        }
    }
}
