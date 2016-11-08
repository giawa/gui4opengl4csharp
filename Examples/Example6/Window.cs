using System;
using System.Runtime.InteropServices;

using SDL2;
using OpenGL;
using System.Collections.Generic;
using OpenGL.UI;

namespace Example6
{
    public static class Window
    {
        public static int Width { get; private set; }

        public static int Height { get; private set; }

        private static IntPtr window, glContext;
        private static byte[] mouseState = new byte[256];

        /// <summary>
        /// The main thread ID, which is the thread ID that the OpenGL context was created on.
        /// This is the thread ID that must be used for all future OpenGL calls.
        /// </summary>
        public static int MainThreadID { get; private set; }

        /// <summary>
        /// Creates an OpenGL context and associated Window via the
        /// cross-platform SDL library.  Will clear the screen to black
        /// as quickly as possible by calling glClearColor and glClear.
        /// </summary>
        /// <param name="title"></param>
        public static void CreateWindow(string title, int width, int height)
        {
            // check if a window already exists
            if (window != IntPtr.Zero || glContext != IntPtr.Zero)
            {
                //Logger.Instance.WriteLine(LogFlags.Warning | LogFlags.OpenGL, "There is already a valid window or OpenGL context.");
                return;
            }

            // initialize SDL and set a few defaults for the OpenGL context
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DOUBLEBUFFER, 1);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE, 24);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_ALPHA_SIZE, 8);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_STENCIL_SIZE, 8);

            // capture the rendering thread ID
            MainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;

            // create the window which should be able to have a valid OpenGL context and is resizable
            var flags = SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
            window = SDL.SDL_CreateWindow(title, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, width, height, flags);

            if (window == IntPtr.Zero)
            {
                //Logger.Instance.WriteLine(LogFlags.Error | LogFlags.OpenGL, "Could not initialize a window using SDL.");
                return;
            }

            Width = width;
            Height = height;

            // create a valid OpenGL context within the newly created window
            glContext = SDL.SDL_GL_CreateContext(window);
            if (glContext == IntPtr.Zero)
            {
                //Logger.Instance.WriteLine(LogFlags.Error | LogFlags.OpenGL, "Could not get a valid OpenGL context.");
                return;
            }

            // initialize the screen to black as soon as possible
            Gl.ClearColor(0f, 0f, 0f, 1f);
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            SwapBuffers();
        }

        /// <summary>
        /// Swap the OpenGL buffer and bring the back buffer to the screen.
        /// </summary>
        public static void SwapBuffers()
        {
            SDL.SDL_GL_SwapWindow(window);
        }

        #region Event Handling
        private static SDL.SDL_Event sdlEvent;

        public delegate void OnMouseWheelDelegate(uint wheel, int direction, int x, int y);

        public static OnMouseWheelDelegate OnMouseWheel { get; set; }

        public static void HandleEvents()
        {
            while (SDL.SDL_PollEvent(out sdlEvent) != 0)
            {
                switch (sdlEvent.type)
                {
                    case SDL.SDL_EventType.SDL_KEYDOWN:
                        OnKeyboardDown(sdlEvent.key.keysym.sym);
                        break;
                    case SDL.SDL_EventType.SDL_KEYUP:
                        OnKeyboardUp(sdlEvent.key.keysym.sym);
                        break;
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                        // keep track of mouse state internally due to a bug in SDL
                        // https://bugzilla.libsdl.org/show_bug.cgi?id=2195
                        if (mouseState[sdlEvent.button.button] == sdlEvent.button.state) break;
                        mouseState[sdlEvent.button.button] = sdlEvent.button.state;
                        if (sdlEvent.button.y == 0 || sdlEvent.button.x == 0) mouseState[sdlEvent.button.button] = 0;

                        OnMouse(sdlEvent.button.button, sdlEvent.button.state, sdlEvent.button.x, sdlEvent.button.y);
                        break;
                    case SDL.SDL_EventType.SDL_MOUSEMOTION:
                        OnMovePassive(sdlEvent.motion.x, sdlEvent.motion.y);
                        break;
                    case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                        //OnMouseWheel(sdlEvent.wheel.which, sdlEvent.wheel.y, 0, 0);
                        if (OnMouseWheel != null) OnMouseWheel(sdlEvent.wheel.which, sdlEvent.wheel.y, 0, 0);
                        break;
                    case SDL.SDL_EventType.SDL_WINDOWEVENT:
                        switch (sdlEvent.window.windowEvent)
                        {
                            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                                OnReshape(sdlEvent.window.data1, sdlEvent.window.data2);
                                break;
                            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                                OnClose();
                                break;
                            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED:
                                // stop rendering the scene
                                break;
                            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESTORED:
                            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_EXPOSED:
                                // stop rendering the scene
                                break;
                        }
                        break;
                }
            }
        }
        #endregion

        #region OnReshape and OnClose
        public static List<Action> OnReshapeCallbacks = new List<Action>();
        public static List<Action> OnCloseCallbacks = new List<Action>();

        public static void OnReshape(int width, int height)
        {
            // for whatever reason, SDL does not give accurate sizes in its event when windowed,
            // so we just need to query the window size when in windowed mode
            //if (!Fullscreen)
            //    SDL.SDL_GetWindowSize(window, out width, out height);

            if (width % 2 == 1) width--;
            if (height % 2 == 1) height--;

            Width = width;
            Height = height;

            foreach (var callback in OnReshapeCallbacks) callback();
        }

        public static void OnClose()
        {
            foreach (var callback in OnCloseCallbacks) callback();

            SDL.SDL_GL_DeleteContext(glContext);
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
            Environment.Exit(0);
        }
        #endregion

        #region Mouse Callbacks
        private static int prevx, prevy, downx, downy;

        private static void LockMouse(Click Mouse)
        {
            if (Mouse.state == MouseState.Up) WarpPointer(downx, downy);

            SDL.SDL_ShowCursor((Mouse.state == MouseState.Down) ? 0 : 1);

            downx = prevx = Mouse.x;
            downy = prevy = Mouse.y;

            //if (Mouse.state == MouseState.Down) Input.MouseMove = new Event(MouseMove);
            //else Input.MouseMove = new Event(MouseMovePassive);
        }

        public static void MouseRightClick(Click Mouse)
        {
            LockMouse(Mouse);

            //Input.RightMouse = (Mouse.state == MouseState.Down);
        }

        public static void MouseLeftClick(Click Mouse)
        {
            /*if (Input.RightMouse) return;

            if (Input.LeftMouse && Mouse.state == MouseState.Up)
            {
                LockMouse(Mouse);
                Input.LeftMouse = false;
            }
            else if (Mouse.state == MouseState.Down)
            {
                LockMouse(Mouse);
                Input.LeftMouse = true;
            }*/
        }

        private static void WarpPointer(int x, int y)
        {
            //NativeMethods.CGSetLocalEventsDelegateOSIndependent(0.0);
            SDL.SDL_WarpMouseInWindow(window, x, y);
            //NativeMethods.CGSetLocalEventsDelegateOSIndependent(0.25);
        }

        public static void MouseMove(int lx, int ly, int x, int y)
        {
            if (prevx != x || prevy != y) WarpPointer(prevx, prevy);
        }

        public static void MouseMovePassive(int lx, int ly, int x, int y)
        {
            prevx = x;
            prevy = y;
        }

        private static void OnMouse(int button, int state, int x, int y)
        {
            downx = x; downy = y;

            if (!OpenGL.UI.UserInterface.OnMouseClick(button, state, x, y))
            {
                // do other picking code here if necessary
            }
        }

        private static void OnMovePassive(int x, int y)
        {
            if (!OpenGL.UI.UserInterface.OnMouseMove(x, y))
            {
                // do other picking code here if necessary
            }
        }
        #endregion

        #region Keyboard Callbacks
        private static void OnKeyboardDown(SDL.SDL_Keycode sym)
        {
            if (sym == SDL.SDL_Keycode.SDLK_ESCAPE) OnClose();
        }

        private static void OnKeyboardUp(SDL.SDL_Keycode sym)
        {
        }
        #endregion
    }
}
