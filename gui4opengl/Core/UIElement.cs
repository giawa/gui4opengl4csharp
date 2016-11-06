using System;
using System.Collections;

using OpenGL;

namespace OpenGL.UI
{
    #region Enumerations
    public enum SpecialKey
    {
        Alt, Control, Shift
    };

    /// <summary>Enumeration holding the mouse button values.</summary>
    public enum MouseButton : int
    {
        /// <summary>The left mouse button is a valid chord modifier for input.</summary>
        Left = 1,
        /// <summary>The right mouse button is a valid chord modifier for input.</summary>
        Right = 3,
        /// <summary>The middle mouse button is a valid chord modifier for input.</summary>
        Middle = 2
    }

    /// <summary>
    /// Enumeration holding the mouse state values.
    /// </summary>
    public enum MouseState : int
    {
        Up = 0,
        Down = 1
    }

    public enum Corner
    {
        BottomLeft,
        BottomRight,
        TopLeft,
        TopRight,
        Bottom,
        Top,
        Fill,
        Center
    };

    public enum Orientation
    {
        Horizontal,
        Vertical
    };
    #endregion

    #region Structures
    public struct Invokable
    {
        public OnInvoke Method;
        public object Parameter;

        public Invokable(OnInvoke Method, object arg)
        {
            this.Method = Method;
            this.Parameter = arg;
        }
    }

    /// <summary>
    /// A click stores information about the mouse location
    /// and button at the time of a click event.
    /// </summary>
    public struct Click
    {
        #region Fields
        /// <summary>The x-location of the mouse wrt the top-left.</summary>
        public int x;
        /// <summary>The y-location of the mouse wrt the top-left.</summary>
        public int y;
        /// <summary>The mouse button pressed on the click event.</summary>
        public MouseButton button;
        /// <summary>True if the mouse button has been pressed, false if it has been released.</summary>
        public MouseState state;
        #endregion

        #region Methods
        /// <summary>A new click object with x, y and button data.</summary>
        /// <param name="_x">The x-location of the mouse wrt the top-left.</param>
        /// <param name="_y">The y-location of the mouse wrt the top-left.</param>
        /// <param name="_button">The mouse button pressed on the click event.</param>
        /// <param name="_pressed">True if the mouse has been pressed, false if released.</param>
        public Click(int _x, int _y, MouseButton _button, MouseState _state)
        {
            x = _x;
            y = _y;
            button = _button;
            state = _state;
        }

        /// <summary>A new click object with x, y and button data. </summary>
        /// <param name="_x">The x-location of the mouse wrt the top-left.</param>
        /// <param name="_y">The y-location of the mouse wrt the top-left.</param>
        /// <param name="left">True if the left button is pressed.</param>
        /// <param name="middle">True if the middle button is pressed.</param>
        /// <param name="right">True if the right button is pressed.</param>
        /// <param name="pressed">True if the mouse has been pressed, false if released.</param>
        public Click(int _x, int _y, bool left, bool middle, bool right, bool pressed) :
            this(_x, _y, (left ? MouseButton.Left : (right ? MouseButton.Right : MouseButton.Middle)), pressed ? MouseState.Down : MouseState.Up) { }

        /// <summary>A new click object with button data.</summary>
        /// <param name="left">True if the left button is pressed.</param>
        /// <param name="middle">True if the middle button is pressed.</param>
        /// <param name="right">True if the right button is pressed.</param>
        /// <param name="pressed">True if the mouse has been pressed, false if released.</param>
        //public Click(bool left, bool middle, bool right, bool pressed) :
        //    this(Input.MousePosition.x, Input.MousePosition.y, left, middle, right, pressed) { }

        /// <summary>
        /// ToString override to give some information about the mouse state.
        /// </summary>
        public override string ToString()
        {
            return string.Format("Mouse at {0},{1} and is {2}.", x, y, state);
        }
        #endregion
    }
    #endregion

    #region Delegates
    public delegate void OnChanged(object sender, EventArgs e);

    public delegate void OnInvoke(object arg);

    public delegate void OnMouse(object sender, MouseEventArgs e);

    public delegate void OnFocus(object sender, IMouseInput newFocus);
    #endregion

    public interface IUserInterface : IDisposable
    {
        #region Interface Properties
        float Alpha { get; set; }

        Point Position { get; set; }

        Point Size { get; set; }

        Point MinSize { get; set; }

        Point MaxSize { get; set; }

        Corner RelativeTo { get; set; }

        UIContainer Parent { get; set; }

        ShaderProgram Program { get; }

        string Name { get; set; }

        void Draw();

        void OnResize();

        void Update();

        void Invalidate();

        void Invoke(OnInvoke Method, object arg);
        #endregion
    }

    public abstract class UIElement : IUserInterface, IMouseInput
    {
        #region Interface Overrides
        public float Alpha { get; set; }

        public Point Position { get; set; }

        private Point u_size;
        public Point Size
        {
            get { return u_size; }
            set
            {
                if (MaxSize.x == 0 || MaxSize.y == 0) MaxSize = new Point(1000000, 1000000);
                u_size = Point.Max(MinSize, Point.Min(MaxSize, value));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (uiQuad != null)
            {
                uiQuad.DisposeChildren = true;
                uiQuad.Dispose();
                uiQuad = null;
            }

            UserInterface.RemoveElement(this);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Point MinSize { get; set; }

        public Point MaxSize { get; set; }

        public Point CorrectedPosition { get; set; }

        public Corner RelativeTo { get; set; }

        public string Name { get; set; }

        public OnMouse OnMouseClick { get; set; }

        public OnMouse OnMouseEnter { get; set; }

        public OnMouse OnMouseLeave { get; set; }

        public OnMouse OnMouseDown { get; set; }

        public OnMouse OnMouseUp { get; set; }

        public OnMouse OnMouseMove { get; set; }

        public OnMouse OnMouseRepeat { get; set; }

        public OnFocus OnLoseFocus { get; set; }

        public UIContainer Parent { get; set; }

        public bool DisablePicking { get; set; }

        public ShaderProgram Program { get; protected set; }

        private bool visible = true;

        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        public virtual void Draw()
        {
            DoInvoke();

            if (BackgroundTexture != null) DrawQuadTextured();
            else DrawQuadColored();
        }

        public virtual void OnResize()
        {
            if (Parent == null)
            {
                if (RelativeTo == Corner.BottomLeft) CorrectedPosition = Position;
                else if (RelativeTo == Corner.TopLeft)
                    CorrectedPosition = new Point(Position.x, Position.y);
                else if (RelativeTo == Corner.BottomRight)
                    CorrectedPosition = new Point(UserInterface.Width - Position.x - Size.x, Position.y);
                else if (RelativeTo == Corner.TopRight)
                    CorrectedPosition = new Point(UserInterface.Width - Position.x - Size.x, -Position.y - Size.y);
                else if (RelativeTo == Corner.Bottom)
                    CorrectedPosition = new Point(UserInterface.Width / 2 + Position.x, Position.y);
                else if (RelativeTo == Corner.Top)
                    CorrectedPosition = new Point(UserInterface.Width / 2 + Position.x, -Position.y - Size.y);
                else if (RelativeTo == Corner.Center)
                    CorrectedPosition = new Point(UserInterface.Width / 2 - Size.x / 2 + Position.x, UserInterface.Height / 2 - Size.y / 2 + Position.y);
            }
            else
            {
                if (RelativeTo == Corner.BottomLeft) CorrectedPosition = Position;
                else if (RelativeTo == Corner.TopLeft)
                    CorrectedPosition = new Point(Position.x, Parent.Size.y - Position.y - Size.y);
                else if (RelativeTo == Corner.BottomRight)
                    CorrectedPosition = new Point(Parent.Size.x - Position.x - Size.x, Position.y);
                else if (RelativeTo == Corner.TopRight)
                    CorrectedPosition = new Point(Parent.Size.x - Position.x - Size.x, Parent.Size.y - Position.y - Size.y);
                else if (RelativeTo == Corner.Bottom)
                    CorrectedPosition = new Point(Parent.Size.x / 2 + Position.x, Position.y);
                else if (RelativeTo == Corner.Top)
                    CorrectedPosition = new Point(Parent.Size.x / 2 + Position.x, Parent.Size.y - Position.y);
                else if (RelativeTo == Corner.Fill)
                {
                    CorrectedPosition = new Point(0, 0);
                    Size = Parent.Size;
                }
                else if (RelativeTo == Corner.Center)
                    CorrectedPosition = new Point(Parent.Size.x / 2 - Size.x / 2 + Position.x, Parent.Size.y / 2 - Size.y / 2 + Position.y);
                CorrectedPosition += Parent.CorrectedPosition;
            }

            if (BackgroundColor != Vector4.Zero || BackgroundTexture != null)
            {
                if (uiQuad != null)
                {
                    uiQuad.DisposeChildren = true;
                    uiQuad.Dispose();
                }
                uiQuad = OpenGL.Geometry.CreateQuad(Shaders.SolidUIShader, Vector2.Zero, new Vector2(Size.x, Size.y), Vector2.Zero, new Vector2(1, 1));
            }

            Invalidate();
        }

        public virtual void Update() { }

        public virtual bool Pick(Point Location)
        {
            if (DisablePicking) return false;
            return (Location.x >= CorrectedPosition.x && Location.x <= CorrectedPosition.x + Size.x &&
                Location.y >= CorrectedPosition.y && Location.y <= CorrectedPosition.y + Size.y);
        }

        public virtual void Invalidate() { }
        #endregion

        #region Invoke Methods
        private Queue InvokeQueue = null;

        /// <summary>
        /// Adds a method to the invoke queue, which will be called by the thread that owns this object.
        /// </summary>
        /// <param name="Method">Since argument method to call.</param>
        /// <param name="arg">Argument for the method.</param>
        public void Invoke(OnInvoke Method, object arg)
        {
            if (InvokeQueue == null) InvokeQueue = new Queue();
            Queue.Synchronized(InvokeQueue).Enqueue(new Invokable(Method, arg));
        }

        /// <summary>
        /// Calls all the methods that have been invoked by pulling them off the thread-safe queue.
        /// </summary>
        public virtual void DoInvoke()
        {
            if (InvokeQueue == null || InvokeQueue.Count == 0) return;

            for (int i = 0; i < Queue.Synchronized(InvokeQueue).Count; i++)
            {
                Invokable pInvoke = (Invokable)Queue.Synchronized(InvokeQueue).Dequeue();
                pInvoke.Method(pInvoke.Parameter);
            }
        }
        #endregion

        #region Methods
        public static bool Intersects(Point Position, Point Size, Point Location)
        {
            return (Location.x >= Position.x && Location.x <= Position.x + Size.x &&
                Location.y >= Position.y && Location.y <= Position.y + Size.y);
        }
        #endregion

        #region Draw Methods
        private VAO uiQuad;

        public Texture BackgroundTexture { get; set; }

        public Vector4 BackgroundColor { get; set; }

        public void DrawQuadTextured()
        {
            if (BackgroundTexture == null) return;
            if (uiQuad == null)
                uiQuad = OpenGL.Geometry.CreateQuad(Shaders.SolidUIShader, Vector2.Zero, new Vector2(Size.x, Size.y), Vector2.Zero, new Vector2(1, 1));

            Gl.Enable(EnableCap.Blend);
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(BackgroundTexture);

            Shaders.TexturedUIShader.Use();
            Shaders.TexturedUIShader["position"].SetValue(new Vector3(CorrectedPosition.x, CorrectedPosition.y, 0));
            uiQuad.DrawProgram(Shaders.TexturedUIShader);

            Gl.Disable(EnableCap.Blend);
        }

        public void DrawQuadColored()
        {
            if (BackgroundColor == Vector4.Zero) return;

            DrawQuadColored(BackgroundColor);
        }

        public void DrawQuadColored(Vector4 color)
        {
            if (uiQuad == null) uiQuad = OpenGL.Geometry.CreateQuad(Shaders.SolidUIShader, Vector2.Zero, new Vector2(Size.x, Size.y), Vector2.Zero, new Vector2(1, 1));

            Gl.Enable(EnableCap.Blend);

            Shaders.SolidUIShader.Use();
            Shaders.SolidUIShader["position"].SetValue(new Vector3(CorrectedPosition.x, CorrectedPosition.y, 0));
            Shaders.SolidUIShader["color"].SetValue(color);
            uiQuad.Draw();

            Gl.Disable(EnableCap.Blend);
        }
        #endregion
    }
}
