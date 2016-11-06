using System;

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

    public class MouseEventArgs : EventArgs
    {
        public Point Location { get; private set; }
        public Point LastLocaton { get; private set; }
        public MouseButton Button { get; private set; }
        public MouseState State { get; private set; }

        public MouseEventArgs(Click MousePosition, Click LastMousePosition)
            : this(new Point(MousePosition.x, MousePosition.y), MousePosition.button, MousePosition.state)
        {
            this.LastLocaton = new Point(LastMousePosition.x, LastMousePosition.y);
        }

        public MouseEventArgs()
            : this(new Point(0, 0))
        {
        }

        public MouseEventArgs(Point Location)
        {
            this.LastLocaton = new Point(Location.x, Location.y);
            this.Location = this.LastLocaton;
        }

        public MouseEventArgs(Point Location, MouseButton Button, MouseState State)
        {
            this.LastLocaton = new Point(Location.x, Location.y);
            this.Location = this.LastLocaton;
            this.Button = Button;
            this.State = State;
        }

        internal void SetLocation(Point Location)
        {
            this.LastLocaton = this.Location;
            this.Location = new Point(Location.x, Location.y);
        }

        internal void SetState(Point Location, MouseButton Button, MouseState State)
        {
            this.LastLocaton = this.Location;
            this.Location = new Point(Location.x, Location.y);
            this.Button = Button;
            this.State = State;
        }
    }
}
