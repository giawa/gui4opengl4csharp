using System;
using Tao.FreeGlut;
using OpenGL;

namespace Example1
{
    static class Program
    {
        private static int width = 1280, height = 720;

        static void Main()
        {
            // create an OpenGL window
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("OpenGL UI: Example 1");

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
            OpenGL.UI.Text welcome = new OpenGL.UI.Text(OpenGL.UI.Text.FontSize._24pt, "Welcome to OpenGL", OpenGL.UI.BMFont.Justification.Center);
            welcome.RelativeTo = OpenGL.UI.Corner.Center;

            // create some colored text
            OpenGL.UI.Text coloredText = new OpenGL.UI.Text(OpenGL.UI.Text.FontSize._24pt, "using C#", OpenGL.UI.BMFont.Justification.Center);
            coloredText.Position = new OpenGL.Platform.Point(0, -30);
            coloredText.Color = new Vector3(0.2f, 0.3f, 1f);
            coloredText.RelativeTo = OpenGL.UI.Corner.Center;

            // add the two text object to the UI
            OpenGL.UI.UserInterface.AddElement(welcome);
            OpenGL.UI.UserInterface.AddElement(coloredText);

            // enter the glut main loop (this is where the drawing happens)
            Glut.glutMainLoop();
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
            Gl.Viewport(0, 0, width, height);
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
