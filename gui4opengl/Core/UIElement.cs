using System;
using System.Collections;

using OpenGL;
using OpenGL.Platform;

namespace OpenGL.UI
{
    public interface IMouseInput
    {
        #region Interface Properties
        OnMouse OnMouseClick { get; set; }

        OnMouse OnMouseEnter { get; set; }

        OnMouse OnMouseLeave { get; set; }

        OnMouse OnMouseDown { get; set; }

        OnMouse OnMouseUp { get; set; }

        OnMouse OnMouseMove { get; set; }

        OnMouse OnMouseRepeat { get; set; }

        OnFocus OnLoseFocus { get; set; }
        #endregion
    }

    #region Enumerations
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
                if (MaxSize.X == 0 || MaxSize.Y == 0) MaxSize = new Point(1000000, 1000000);
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
                    CorrectedPosition = new Point(Position.X, Position.Y);
                else if (RelativeTo == Corner.BottomRight)
                    CorrectedPosition = new Point(UserInterface.Width - Position.X - Size.X, Position.Y);
                else if (RelativeTo == Corner.TopRight)
                    CorrectedPosition = new Point(UserInterface.Width - Position.X - Size.X, -Position.Y - Size.Y);
                else if (RelativeTo == Corner.Bottom)
                    CorrectedPosition = new Point(UserInterface.Width / 2 - Size.X / 2 + Position.X, Position.Y);
                else if (RelativeTo == Corner.Top)
                    CorrectedPosition = new Point(UserInterface.Width / 2 - Size.X / 2 + Position.X, -Position.Y - Size.Y);
                else if (RelativeTo == Corner.Center)
                    CorrectedPosition = new Point(UserInterface.Width / 2 - Size.X / 2 + Position.X, UserInterface.Height / 2 - Size.Y / 2 + Position.Y);
            }
            else
            {
                if (RelativeTo == Corner.BottomLeft) CorrectedPosition = Position;
                else if (RelativeTo == Corner.TopLeft)
                    CorrectedPosition = new Point(Position.X, Parent.Size.Y - Position.Y - Size.Y);
                else if (RelativeTo == Corner.BottomRight)
                    CorrectedPosition = new Point(Parent.Size.X - Position.X - Size.X, Position.Y);
                else if (RelativeTo == Corner.TopRight)
                    CorrectedPosition = new Point(Parent.Size.X - Position.X - Size.X, Parent.Size.Y - Position.Y - Size.Y);
                else if (RelativeTo == Corner.Bottom)
                    CorrectedPosition = new Point(Parent.Size.X / 2 - Size.X / 2 + Position.X, Position.Y);
                else if (RelativeTo == Corner.Top)
                    CorrectedPosition = new Point(Parent.Size.X / 2 - Size.X / 2 + Position.X, Parent.Size.Y - Position.Y - Size.Y);
                else if (RelativeTo == Corner.Fill)
                {
                    CorrectedPosition = new Point(0, 0);
                    Size = Parent.Size;
                }
                else if (RelativeTo == Corner.Center)
                    CorrectedPosition = new Point(Parent.Size.X / 2 - Size.X / 2 + Position.X, Parent.Size.Y / 2 - Size.Y / 2 + Position.Y);
                CorrectedPosition += Parent.CorrectedPosition;
            }

            if (BackgroundColor != Vector4.Zero || BackgroundTexture != null)
            {
                if (uiQuad != null)
                {
                    uiQuad.DisposeChildren = true;
                    uiQuad.Dispose();
                }
                uiQuad = OpenGL.Geometry.CreateQuad(Shaders.SolidUIShader, Vector2.Zero, new Vector2(Size.X, Size.Y), Vector2.Zero, new Vector2(1, 1));
            }

            Invalidate();
        }

        public virtual void Update() { }

        public virtual bool Pick(Point Location)
        {
            if (DisablePicking) return false;
            return (Location.X >= CorrectedPosition.X && Location.X <= CorrectedPosition.X + Size.X &&
                Location.Y >= CorrectedPosition.Y && Location.Y <= CorrectedPosition.Y + Size.Y);
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
            return (Location.X >= Position.X && Location.X <= Position.X + Size.X &&
                Location.Y >= Position.Y && Location.Y <= Position.Y + Size.Y);
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
                uiQuad = OpenGL.Geometry.CreateQuad(Shaders.SolidUIShader, Vector2.Zero, new Vector2(Size.X, Size.Y), Vector2.Zero, new Vector2(1, 1));

            Gl.Enable(EnableCap.Blend);
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(BackgroundTexture);

            Shaders.TexturedUIShader.Use();
            Shaders.TexturedUIShader["position"].SetValue(new Vector3(CorrectedPosition.X, CorrectedPosition.Y, 0));
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
            if (uiQuad == null) uiQuad = OpenGL.Geometry.CreateQuad(Shaders.SolidUIShader, Vector2.Zero, new Vector2(Size.X, Size.Y), Vector2.Zero, new Vector2(1, 1));

            Gl.Enable(EnableCap.Blend);

            Shaders.SolidUIShader.Use();
            Shaders.SolidUIShader["position"].SetValue(new Vector3(CorrectedPosition.X, CorrectedPosition.Y, 0));
            Shaders.SolidUIShader["color"].SetValue(color);
            uiQuad.Draw();

            Gl.Disable(EnableCap.Blend);
        }
        #endregion
    }
}
