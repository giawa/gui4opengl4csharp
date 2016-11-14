using System;

using OpenGL;
using OpenGL.Platform;

namespace OpenGL.UI
{
    public class Slider : UIContainer
    {
        #region Variables
        private int min = 0, max = 10, value = 0;
        private Button sliderButton;
        private bool sliderMouseDown = false;
        private int sliderDown = -1;
        #endregion

        #region Properties
        /// <summary>
        /// The minimum value that the slider can return.
        /// </summary>
        public int Minimum
        {
            get { return min; }
            set
            {
                if (value < 0 || value >= max) throw new ArgumentOutOfRangeException("Minimum");
                min = value;
                if (Value < min) Value = min;
            }
        }

        /// <summary>
        /// The maximum value that the slider can return.
        /// </summary>
        public int Maximum
        {
            get { return max; }
            set
            {
                if (value < 0 || value <= min) throw new ArgumentOutOfRangeException("Maximum");
                max = value;
                if (Value > max) Value = max;
            }
        }

        /// <summary>
        /// The current value of the slider (rounded to the closest integer).
        /// </summary>
        public int Value
        {
            get { return value; }
            set
            {
                if (value < min || value > max) throw new ArgumentOutOfRangeException("Value");
                if (value != this.value && OnValueChanged != null) OnValueChanged(this, new MouseEventArgs());

                this.value = value;

                int x = (int)((value - min) * (Size.x - sliderButton.Size.x) / (max - min));
                sliderButton.Position = new Point(x, 0);
                sliderButton.OnResize();    // we've moved this UIElement, make sure we update the CorrectedPosition, etc
            }
        }

        /// <summary>
        /// Locks the slider to the steps defined by Minimum and Maximum.
        /// This, for example, will cause the slider to 'jump' to the next valid value.
        /// </summary>
        public bool LockToSteps { get; set; }

        /// <summary>
        /// An event that is fired when the value of the slider changes.
        /// </summary>
        public OnMouse OnValueChanged { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a slider with a given texture, minimum, maximum and default values.
        /// </summary>
        /// <param name="sliderTexture">The texture to apply to the slider.  This also defines the slider size.</param>
        /// <param name="min">The minimum value of the slider.</param>
        /// <param name="max">The maximum value of the slider.</param>
        /// <param name="value">The default value of the slider.</param>
        public Slider(Texture sliderTexture, int min = 0, int max = 10, int value = 0)
            : base(new Point(0, 0), new Point(200, sliderTexture.Size.Height), "Slider" + UserInterface.GetUniqueElementID())
        {
            this.min = min;
            this.max = max;
            this.value = value;

            sliderButton = new Button(sliderTexture);
            sliderButton.BackgroundColor = new Vector4(0, 0, 0, 0);

            sliderButton.OnMouseUp = (sender, eventArgs) => sliderMouseDown = false;
            sliderButton.OnMouseDown = (sender, eventArgs) =>
            {
                sliderMouseDown = (eventArgs.Button == MouseButton.Left);
                sliderDown = eventArgs.Location.x;
            };
            sliderButton.OnMouseMove = (sender, eventArgs) => this.OnMouseMove(sender, eventArgs);
            this.OnMouseMove = (sender, eventArgs) =>
            {
                if (!sliderMouseDown) return;

                if (eventArgs.Location.x < CorrectedPosition.x)
                {
                    // handle case where the mouse has gone too far to the left
                    sliderButton.Position = new Point(0, 0);
                    this.Value = Minimum;
                }
                else if (eventArgs.Location.x > CorrectedPosition.x + Size.x)
                {
                    // handle case where the mouse has gone too far to the right
                    sliderButton.Position = new Point(Size.x - sliderButton.Size.x, 0);
                    this.Value = Maximum;
                }
                else
                {
                    int dx = eventArgs.Location.x - sliderDown;

                    int x = eventArgs.Location.x - CorrectedPosition.x - (sliderButton.Size.x / 2);
                    double percent = Math.Max(0, (double)x / (Size.x - sliderButton.Size.x));

                    // take care of locking to the closest step
                    if (LockToSteps) x = (int)(Math.Round(percent * (Maximum - Minimum)) * (Size.x - sliderButton.Size.x) / (Maximum - Minimum));
                    else x = Math.Max(0, Math.Min(Size.x - sliderButton.Size.x, x));

                    if (x == sliderButton.Position.x) return;
                    sliderButton.Position = new Point(x, 0);

                    sliderDown = eventArgs.Location.x;

                    int clampedValue = Math.Max(Minimum, Math.Min(Maximum, (int)Math.Round((Maximum - Minimum) * percent) + Minimum));

                    if (this.value != clampedValue)
                    {
                        this.value = clampedValue;
                        if (OnValueChanged != null) OnValueChanged(this, new MouseEventArgs());
                    }
                }
                sliderButton.OnResize();
            };

            sliderButton.RelativeTo = Corner.BottomLeft;
            sliderButton.Position = new Point(value * (Size.x - sliderButton.Size.x) / (max - min), 0);
            this.AddElement(sliderButton);
        }
        #endregion
    }
}
