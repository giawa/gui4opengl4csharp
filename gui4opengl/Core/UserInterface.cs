using System;
using System.Collections.Generic;

using OpenGL;
using OpenGL.Platform;

namespace OpenGL.UI
{
    public static class UserInterface
    {
        #region Variables
        public static UIContainer UIWindow;

        private static uint uniqueID = 0;

        public static Dictionary<string, UIElement> Elements;

        public static Matrix4 UIProjectionMatrix;

        public static UIElement Selection;

        public static bool Visible { get; set; }

        public static int Width { get; set; }

        public static int Height { get; set; }

        public static int MainThreadID { get; private set; }

        private static Stack<UIContainer> activeContainerStack = new Stack<UIContainer>();

        public static Stack<UIContainer> ActiveContainer
        {
            get { return activeContainerStack; }
        }
        #endregion

        #region Initialization and Adding/Removing Elements
        public static void InitUI(int width, int height)
        {
            if (Shaders.Init())
            {
                Elements = new Dictionary<string, UIElement>();

                // create the top level screen container
                UIWindow = new UIContainer(new Point(0, 0), new Point(UserInterface.Width, UserInterface.Height), "TopLevel");
                UIWindow.RelativeTo = Corner.BottomLeft;
                Elements.Add("Screen", UIWindow);

                UIProjectionMatrix = Matrix4.CreateTranslation(new Vector3(-UserInterface.Width / 2, -UserInterface.Height / 2, 0)) * Matrix4.CreateOrthographic(UserInterface.Width, UserInterface.Height, 0, 1000);

                Visible = true;

                MainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;

                OnResize(width, height);
            }
        }

        public static void AddElement(UIElement Element)
        {
            UIWindow.AddElement(Element);
        }

        public static void RemoveElement(UIElement Element)
        {
            UIWindow.RemoveElement(Element);
        }

        public static void ClearElements()
        {
            UIWindow.ClearElements();
        }

        public static UIElement GetElement(string name)
        {
            UIElement element = null;
            Elements.TryGetValue(name, out element);
            return element;
        }
        #endregion

        #region Methods
        public static void Draw()
        {
            if (!Visible || UIWindow == null) return;

            bool depthTest = Gl.GetBoolean(GetPName.DepthTest);
            bool blending = Gl.GetBoolean(GetPName.Blend);
            if (depthTest)
            {
                Gl.Disable(EnableCap.DepthTest);
                Gl.DepthMask(false);
            }

            UIWindow.Draw();

            if (depthTest)
            {
                Gl.DepthMask(true);
                Gl.Enable(EnableCap.DepthTest);
            }
            if (blending) Gl.Enable(EnableCap.Blend);
        }

        public static uint GetUniqueElementID()
        {
            return uniqueID++;
        }

        private static void Update(float delta)
        {
            UIWindow.Update();
        }

        public static void OnResize(int width, int height)
        {
            UserInterface.Width = width;
            UserInterface.Height = height;

            UIProjectionMatrix = Matrix4.CreateTranslation(new Vector3(-UserInterface.Width / 2, -UserInterface.Height / 2, 0)) * Matrix4.CreateOrthographic(UserInterface.Width, UserInterface.Height, 0, 1000);
            Shaders.UpdateUIProjectionMatrix(UIProjectionMatrix);

            if (UIWindow == null) return;
            UIWindow.Size = new Point(UserInterface.Width, UserInterface.Height);
            UIWindow.OnResize();
        }

        public static bool Pick(Point Location)
        {
            if (UIWindow == null) return false;

            if ((Selection = UIWindow.PickChildren(new Point(Location.X, UserInterface.Height - Location.Y))) != null)
            {
                if (Selection == UIWindow) return false;
                else return true;
            }

            return false;
        }

        public static void Dispose()
        {
            UIWindow.Dispose();
            UIWindow = null;

            Elements.Clear();

            Shaders.Dispose();
        }
        #endregion

        #region Mouse Callbacks
        private static UIElement currentSelection = null, activeSelection = null;

        private static Click mousePosition, lmousePosition;            // the current and previous mouse position and button

        /// <summary>
        /// The current mouse state (position and button press).
        /// </summary>
        public static Click MousePosition
        {
            get { return mousePosition; }
            set { lmousePosition = mousePosition; mousePosition = value; }
        }

        /// <summary>
        /// The previous state of the mouse (position and button press).
        /// </summary>
        public static Click LastMousePosition
        {
            get { return lmousePosition; }
            set { lmousePosition = value; }
        }

        /// <summary>
        /// The current object that has focus (the last object clicked).
        /// </summary>
        public static IMouseInput Focus 
        { 
            get; 
            set; 
        }

        public static bool OnMouseMove(int x, int y)
        {
            UIElement lastSelection = currentSelection;
            MousePosition = new Click(x, y, false, false, false, false);
            Point position = new Point(MousePosition.X, MousePosition.Y);

            if (currentSelection != null && currentSelection.OnMouseMove != null) currentSelection.OnMouseMove(null, new MouseEventArgs(MousePosition, LastMousePosition));

            if (UserInterface.Pick(position))
                currentSelection = UserInterface.Selection;
            else currentSelection = null;

            if (currentSelection != lastSelection)
            {
                if (lastSelection != null && lastSelection.OnMouseLeave != null) lastSelection.OnMouseLeave(null, new MouseEventArgs(MousePosition, LastMousePosition));
                if (currentSelection != null && currentSelection.OnMouseEnter != null) currentSelection.OnMouseEnter(null, new MouseEventArgs(MousePosition, LastMousePosition));
            }

            return (currentSelection != null);
        }

        public static bool OnMouseClick(int button, int state, int x, int y)
        {
            MousePosition = new Click(x, y, (MouseButton)button, (MouseState)state);

            // call OnLoseFocus if a control lost focus
            if (MousePosition.State == MouseState.Down)
            {
                if (Focus != null && currentSelection != Focus && Focus.OnLoseFocus != null) Focus.OnLoseFocus(null, currentSelection);
                Focus = currentSelection;
            }

            if (activeSelection != null && MousePosition.State == MouseState.Up)
            {
                // if mouseup while a pickable object is active
                if (activeSelection.OnMouseUp != null) activeSelection.OnMouseUp(null, new MouseEventArgs(MousePosition, LastMousePosition));
                activeSelection = null;
            }
            else if (currentSelection != null && !(currentSelection is UIContainer))
            {
                // if the mouse is current over a pickable object and clicks
                if (MousePosition.State == MouseState.Down)
                {
                    if (currentSelection.OnMouseDown != null) currentSelection.OnMouseDown(null, new MouseEventArgs(MousePosition, LastMousePosition));
                    activeSelection = currentSelection;
                }
                else
                {
                    if (currentSelection.OnMouseUp != null) currentSelection.OnMouseUp(null, new MouseEventArgs(MousePosition, LastMousePosition));
                    activeSelection = null;
                }
                if (currentSelection.OnMouseClick != null) currentSelection.OnMouseClick(null, new MouseEventArgs(MousePosition, LastMousePosition));
            }

            return (activeSelection != null);
        }
        #endregion

        #region Keyboard Callbacks
        #endregion
    }
}
