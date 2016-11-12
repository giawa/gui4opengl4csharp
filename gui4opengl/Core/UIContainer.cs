using System;
using System.Collections.Generic;

using OpenGL.Platform;

namespace OpenGL.UI
{
    public class UIContainer : UIElement
    {
        #region Properties
        protected List<UIElement> elements;

        public new string Name
        {
            get { return base.Name; }
            set
            {
                base.Name = value;
                foreach (UIElement pElement in elements) pElement.Name = base.Name + pElement.GetType();
            }
        }

        public List<UIElement> Elements
        {
            get { return elements; }
        }
        #endregion

        #region Constructor
        public UIContainer()
            : this(new Point(0, 0), UserInterface.UIWindow.Size, "Container" + UserInterface.GetUniqueElementID())
        {
            this.RelativeTo = Corner.Fill;
        }

        public UIContainer(Point Size, string Name)
            : this(new Point(0, 0), Size, Name)
        {
        }

        public UIContainer(Point Position, Point Size, string Name)
        {
            elements = new List<UIElement>();
            this.Name = Name;
            this.RelativeTo = Corner.TopLeft;
            this.Position = Position;
            this.Size = Size;
        }
        #endregion

        #region Container Methods
        public void AddElement(UIElement Element)
        {
            if (Element.Name == null || Element.Name.Length == 0)
            {
                //Logger.Instance.WriteLine(LogFlags.Warning | LogFlags.UI, "Element of type " + Element.ToString() + " has no name!");
                Element.Name = Element.ToString() + UserInterface.GetUniqueElementID();
                //Logger.Instance.WriteLine(LogFlags.UI, "Assigned the name \"" + Element.Name + "\" to the Element.");
            }
            if (UserInterface.Elements.ContainsKey(Element.Name))
            {
                //Logger.Instance.WriteLine(LogFlags.Error | LogFlags.UI, "The element " + Element.Name +
                //    " already exists in the UI.  Could not add the UIElement to UIContainer " + this.Name);
                return;
            }
            UserInterface.Elements.Add(Element.Name, Element);
            Element.Parent = this;
            elements.Add(Element);

            if (this == UserInterface.UIWindow) Element.OnResize();
        }

        public void RemoveElement(UIElement Element)
        {
            if (Element.Name != null && UserInterface.Elements.ContainsKey(Element.Name)) UserInterface.Elements.Remove(Element.Name);

            if (!elements.Contains(Element))
            {
                for (int i = 0; i < elements.Count; i++)
                    if (elements[i].GetType() == typeof(UIContainer) || elements[i].GetType().BaseType == typeof(UIContainer))
                        ((UIContainer)elements[i]).RemoveElement(Element);
            }
            else elements.Remove(Element);
        }

        public UIElement PickChildren(Point Location)
        {
            if (Pick(Location) == false) return null;

            // run this query in reverse order so that 'newer' elements are queried first
            // new elements are assumed to have higher z values
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                if (elements[i].Pick(Location))
                {
                    if (elements[i].GetType() == typeof(UIContainer) || elements[i].GetType().BaseType == typeof(UIContainer))
                        return ((UIContainer)elements[i]).PickChildren(Location);
                    else return elements[i];
                }
            }

            return this;
        }

        public virtual void Close()
        {
            Dispose();
        }

        public void DrawContainerOnly()
        {
            base.Draw();
        }

        public void ClearElements()
        {
            foreach (var element in elements) element.Dispose();
            elements.Clear();
        }
        #endregion

        #region Overridden Methods
        protected override void Dispose(bool disposing)
        {
            while (elements.Count > 0) elements[0].Dispose();

            base.Dispose(disposing);
        }

        public override void Update()
        {
            for (int i = 0; i < elements.Count; i++)
                elements[i].Update();
        }

        public override void Invalidate()
        {
            for (int i = 0; i < elements.Count; i++)
                elements[i].Invalidate();
        }

        public override void Draw()
        {
            DrawContainerOnly();
            for (int i = 0; i < elements.Count; i++)
                if (elements[i].Visible) elements[i].Draw();
        }

        public override void OnResize()
        {
            // only resize if we are actually attached to the UI window (or we are the UI window)
            if (this != UserInterface.UIWindow && this.Parent == null) return;

            base.OnResize();
            for (int i = 0; i < elements.Count; i++)
                elements[i].OnResize();
        }

        public override void DoInvoke()
        {
            base.DoInvoke();
            for (int i = 0; i < elements.Count; i++)
                elements[i].DoInvoke();
        }
        #endregion
    }
}
