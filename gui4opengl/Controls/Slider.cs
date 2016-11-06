using System;

using OpenGL;

namespace OpenGL.UI.Controls
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

            sliderButton.OnMouseUp = new OnMouse((sender, eventArgs) => sliderMouseDown = false);
            sliderButton.OnMouseDown = new OnMouse((sender, eventArgs) =>
            {
                sliderMouseDown = (eventArgs.Button == MouseButton.Left);
                sliderDown = eventArgs.Location.x;
            });
            sliderButton.OnMouseMove = new OnMouse((sender, eventArgs) =>
            {
                if (!sliderMouseDown) return;

                if (eventArgs.Location.x < CorrectedPosition.x)
                {
                    sliderButton.Position = new Point(0, 0);
                    this.Value = Minimum;
                }
                else if (eventArgs.Location.x > CorrectedPosition.x + Size.x)
                {
                    sliderButton.Position = new Point(Size.x - sliderButton.Size.x, 0);
                    this.Value = Maximum;
                }
                else
                {
                    int dx = eventArgs.Location.x - sliderDown;

                    int x = eventArgs.Location.x - CorrectedPosition.x - (sliderButton.Size.x >> 1);
                    double percent = Math.Max(0, (double)x / (Size.x - sliderButton.Size.x));

                    if (LockToSteps) x = (int)(Math.Round(percent * (Maximum - Minimum)) * (Size.x - sliderButton.Size.x) / (Maximum - Minimum));
                    else x = Math.Max(0, Math.Min(Size.x - sliderButton.Size.x, x));

                    if (x == sliderButton.Position.x) return;

                    sliderDown = eventArgs.Location.x;

                    this.Value = Math.Max(Minimum, Math.Min(Maximum, (int)Math.Round((Maximum - Minimum) * percent) + Minimum));
                }
                sliderButton.OnResize();
            });

            sliderButton.RelativeTo = Corner.BottomLeft;
            sliderButton.Position = new Point(value * (Size.x - sliderButton.Size.x) / (max - min), 0);
            this.AddElement(sliderButton);
        }
        #endregion
    }
}
