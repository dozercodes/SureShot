/************************************************************************************ 
 * Copyright (c) 2008-2011, Columbia University
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Columbia University nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY COLUMBIA UNIVERSITY ''AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL <copyright holder> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 * 
 * ===================================================================================
 * Author: Ohan Oda (ohan@cs.columbia.edu)
 * 
 *************************************************************************************/ 

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GoblinXNA.UI.UI2D.Fancy
{
    /// <summary>
    /// A 2D UI component that can be used for controlling media such as video or audio.
    /// </summary>
    public class G2DMediaControl : G2DComponent
    {
        #region Member Fields

        protected long duration;
        protected G2DLabel tooltipLabel;
        protected G2DButton controlButton;
        protected PlayTimeSlider playTimeSlider;
        protected G2DLabel playTimeLabel;
        protected Color buttonColor;
        protected Texture2D playTexture;
        protected Texture2D pauseTexture;

        protected String durationStr;
        protected String positionStr;

        //protected G2DButton volumeButton; // to be added later

        protected ActionPerformed playCallback;
        protected ActionPerformed pauseCallback;
        protected StateChanged positionUpdateCallback;

        protected long curPosition;
        protected bool playing;
        protected bool loop;
        protected bool allowNegative;
        protected long startPosition;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a media controller with default play and pause textures.
        /// </summary>
        public G2DMediaControl() : this(0, null, null) { }

        /// <summary>
        /// Creates a media controller with the specified media duration.
        /// </summary>
        /// <param name="duration">The duration of a media to be controlled.</param>
        public G2DMediaControl(long duration) : this(duration, Color.DarkBlue) { }

        /// <summary>
        /// Creates a media controller with the specified media duration and the button color
        /// of play and pause button.
        /// </summary>
        /// <param name="duration">The duration of a media to be controlled.</param>
        /// <param name="buttonColor">The color of the play and pause button with default 
        /// play and pause textures.</param>
        public G2DMediaControl(long duration, Color buttonColor)
            : base()
        {
            this.duration = duration;
            this.buttonColor = buttonColor;

            Initialize();
        }
        
        /// <summary>
        /// Creates a media controller with the specified media duration, play button image,
        /// and pause button image. 
        /// </summary>
        /// <param name="duration">The duration of a media to be controlled.</param>
        /// <param name="playTexture">The textre image used for the play button.</param>
        /// <param name="pauseTexture">The texture image used for the pause button.</param>
        public G2DMediaControl(long duration, Texture2D playTexture, Texture2D pauseTexture)
            : base()
        {
            this.duration = duration;
            this.playTexture = playTexture;
            this.pauseTexture = pauseTexture;

            Initialize();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the duration of the media to be controlled in milliseconds.
        /// </summary>
        public virtual long Duration
        {
            get { return duration; }
            set 
            { 
                duration = value;

                UpdateSliderRange();
            }
        }

        /// <summary>
        /// Gets or sets the start position if negative is allowed. This value is effective only if
        /// AllowNegative is set to true.
        /// </summary>
        public virtual long StartPosition
        {
            get { return startPosition; }
            set 
            { 
                startPosition = value;

                UpdateSliderRange();
            }
        }

        /// <summary>
        /// Gets or sets the current position of the media in milliseconds.
        /// </summary>
        public virtual long CurrentPosition
        {
            get { return curPosition; }
            set 
            {
                if (!playing)
                    return;

                long min = playTimeSlider.Minimum * 1000;
                long max = playTimeSlider.Maximum * 1000;

                if (value > max || value < min)
                {
                    if (playing)
                    {
                        if (loop)
                        {
                            curPosition = min;
                        }
                        else
                        {
                            HandleControlButtonPress(null);
                            curPosition = (value < min) ? min : max;
                        }
                    }
                }
                else
                    curPosition = value;

                playTimeSlider.ChangeValue((int)(curPosition / 1000));

                UpdatePositionString();
            }
        }

        /// <summary>
        /// Gets or sets whether to allow negative position. The default value is false.
        /// If you set this to true, you should also set StartPosition to specify the
        /// starting (negative) position.
        /// </summary>
        /// <see cref="StartPosition"/>
        public virtual bool AllowNegative
        {
            get { return allowNegative; }
            set 
            { 
                allowNegative = value;

                UpdateSliderRange();
            }
        }

        /// <summary>
        /// Gets whether the media is in 'play' mode.
        /// </summary>
        public bool IsPlaying
        {
            get { return playing; }
        }

        /// <summary>
        /// Gets or sets whether to loop the current position when it reaches the end. The default value
        /// is false.
        /// </summary>
        public virtual bool Loop
        {
            get { return loop; }
            set { loop = value; }
        }

        /// <summary>
        /// Gets or sets whether to show a tooltip above the play time slider that indicates the current
        /// position time. The default value is true.
        /// </summary>
        public virtual bool ShowPositionTooltip
        {
            get { return playTimeSlider.ShowTooltip; }
            set { playTimeSlider.ShowTooltip = value; }
        }

        /// <summary>
        /// Sets the callback function to be invoked whenever 'play' button is clicked.
        /// </summary>
        public ActionPerformed PlayCallback
        {
            set { playCallback = value; }
        }

        /// <summary>
        /// Sets the callback function to be invoked whenever 'pause' button is clicked.
        /// </summary>
        public ActionPerformed PauseCallback
        {
            set { pauseCallback = value; }
        }

        /// <summary>
        /// Sets the callback function to be invoked whenever the position of  the media gets updated.
        /// </summary>
        public StateChanged PositionUpdateCallback
        {
            set { positionUpdateCallback = value; }
        }

        #endregion

        #region Override Properties

        public override Rectangle Bounds
        {
            get
            {
                return base.Bounds;
            }
            set
            {
                base.Bounds = value;

                UpdateUIBounds();

                if (textFont != null)
                {
                    if (playTexture == null)
                    {
                        GenerateDefaultPlayTexture();
                        controlButton.Texture = playTexture;
                    }
                    if (pauseTexture == null)
                        GenerateDefaultPauseTexture();
                }
            }
        }

        public override Component Parent
        {
            get
            {
                return base.Parent;
            }
            set
            {
                base.Parent = value;

                UpdateUIBounds();
            }
        }

        public override float Transparency
        {
            get
            {
                return base.Transparency;
            }
            set
            {
                base.Transparency = value;

                tooltipLabel.Transparency = value;
                controlButton.Transparency = value;
                playTimeSlider.Transparency = value;
                playTimeLabel.Transparency = value;
            }
        }

        public override SpriteFont TextFont
        {
            get
            {
                return base.TextFont;
            }
            set
            {
                base.TextFont = value;

                tooltipLabel.TextFont = value;
                playTimeLabel.TextFont = value;

                UpdateUIBounds();

                if (controlButton.Bounds.Width > 1)
                {
                    if (playTexture == null)
                    {
                        GenerateDefaultPlayTexture();
                        controlButton.Texture = playTexture;
                    }
                    if (pauseTexture == null)
                        GenerateDefaultPauseTexture();
                }
            }
        }

        public override Color TextColor
        {
            get
            {
                return base.TextColor;
            }
            set
            {
                base.TextColor = value;

                tooltipLabel.TextColor = value;
                playTimeLabel.TextColor = value;
            }
        }

        public override float TextTransparency
        {
            get
            {
                return base.TextTransparency;
            }
            set
            {
                base.TextTransparency = value;

                tooltipLabel.TextTransparency = value;
                playTimeLabel.TextTransparency = value;
            }
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;

                controlButton.Enabled = value;
                playTimeSlider.Enabled = value;
            }
        }

        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;

                controlButton.Visible = value;
                playTimeSlider.Visible = value;
                if(!value)
                    tooltipLabel.Visible = false;
                playTimeLabel.Visible = value;
            }
        }

        #endregion

        #region Protected Methods

        protected virtual void Initialize()
        {
            tooltipLabel = new G2DLabel();
            controlButton = new G2DButton();
            playTimeSlider = new PlayTimeSlider(tooltipLabel);
            playTimeLabel = new G2DLabel();

            tooltipLabel.Visible = false;
            playTimeLabel.DrawBorder = true;
            playTimeSlider.DrawBorder = true;

            playTimeLabel.HorizontalAlignment = GoblinEnums.HorizontalAlignment.Center;
            playTimeLabel.VerticalAlignment = GoblinEnums.VerticalAlignment.Center;

            tooltipLabel.HorizontalAlignment = GoblinEnums.HorizontalAlignment.Center;
            tooltipLabel.VerticalAlignment = GoblinEnums.VerticalAlignment.Center;
            tooltipLabel.Visible = false;

            controlButton.ActionPerformedEvent += new ActionPerformed(HandleControlButtonPress);

            playTimeSlider.StateChangedEvent += new StateChanged(HandleSliderMove);

            playTimeSlider.Maximum = (int)(duration / 1000);

            durationStr = "0:00";
            positionStr = "0:00";

            UpdateDurationString();

            playing = false;
            loop = false;
            allowNegative = false;
            curPosition = 0;
            startPosition = 0;

            name = "G2DMediaControl";
        }

        protected virtual void UpdateSliderRange()
        {
            if (allowNegative)
            {
                playTimeSlider.Minimum = (int)(startPosition / 1000);
                playTimeSlider.Maximum = (int)((startPosition + duration) / 1000);
            }
            else
            {
                playTimeSlider.Minimum = 0;
                playTimeSlider.Maximum = (int)(duration / 1000);
            }

            playTimeSlider.Value = playTimeSlider.Minimum;

            UpdateDurationString();
        }

        protected virtual void UpdateUIBounds()
        {
            if (textFont == null)
                return;

            controlButton.Bounds = new Rectangle(paintBounds.X, paintBounds.Y, bounds.Height, bounds.Height);
            int timeLabelWidth = (int)(textFont.MeasureString("1:00:00/1:00:00").X + 4);
            playTimeLabel.Bounds = new Rectangle(paintBounds.X + bounds.Width - timeLabelWidth, paintBounds.Y,
                timeLabelWidth, bounds.Height);
            int tooltipLabelWidth = (int)(textFont.MeasureString("-1:00:00").X + 4);
            tooltipLabel.Bounds = new Rectangle(0, 0, tooltipLabelWidth, (int)(textFont.MeasureString("1:00:00").Y + 4));
            playTimeSlider.Bounds = new Rectangle(paintBounds.X + controlButton.Bounds.Width + 5, 
                paintBounds.Y + (bounds.Height - 22) / 2,
                bounds.Width - (controlButton.Bounds.Width + playTimeLabel.Bounds.Width + 10), 22);
        }

        protected virtual void GenerateDefaultPlayTexture()
        {
            playTexture = new Texture2D(State.Device, controlButton.Bounds.Width, controlButton.Bounds.Height, false, SurfaceFormat.Bgra5551);

            int sixth = controlButton.Bounds.Width / 6;
            List<Point> points = new List<Point>();
            points.Add(new Point(sixth, sixth));
            points.Add(new Point(sixth, controlButton.Bounds.Width - sixth));
            points.Add(new Point(controlButton.Bounds.Width - sixth, controlButton.Bounds.Width / 2));

            UI2DRenderer.GetPolygonTexture(points, UI2DRenderer.PolygonShape.Convex, ref playTexture);

            controlButton.TextureColor = buttonColor;
        }

        protected virtual void GenerateDefaultPauseTexture()
        {
            pauseTexture = new Texture2D(State.Device, controlButton.Bounds.Height, controlButton.Bounds.Height, false, SurfaceFormat.Bgra5551);

            ushort[] data = new ushort[pauseTexture.Width * playTexture.Height];

            int seventh = controlButton.Bounds.Width / 7;
            int three_seventh = seventh * 3;
            int four_seventh = seventh * 4;
            int six_seventh = seventh * 6;

            int start = 0, i = 0;
            int numLines = seventh * 5;
            int lineWidth = seventh * 2;
            ushort val = ushort.MaxValue;

            for (int k = 0; k < numLines; k++)
            {
                i = (seventh + k + 1) * controlButton.Bounds.Width + seventh + 1;
                start = i;
                for (; i < start + lineWidth; i++)
                    data[i] = val;

                i += seventh;
                start = i;
                for (; i < start + lineWidth; i++)
                    data[i] = val;
            }

            pauseTexture.SetData(data);
        }

        protected virtual void UpdateDurationString()
        {
            long rem = duration;
            String sign = "";
            if (allowNegative)
            {
                rem += startPosition;
                if (rem < 0)
                {
                    rem = -rem;
                    sign = "-";
                }
            }
            int dHour = (int)(rem / 3600000);
            rem -= dHour * 3600000;
            int dMin = (int)(rem / 60000);
            rem -= dMin * 60000;
            int dSec = (int)(rem / 1000);

            durationStr = sign + ((dHour > 0) ? (dHour + ":" + String.Format("{0:00}", dMin)) : (dMin + "")) + ":" + 
                String.Format("{0:00}", dSec);

            playTimeLabel.Text = positionStr + "/" + durationStr;
        }

        protected virtual void UpdatePositionString()
        {
            String sign = "";
            long rem = curPosition;
            if (allowNegative && rem < 0)
            {
                rem = -rem;
                sign = "-";
            }
            int dHour = (int)(rem / 3600000);
            rem -= dHour * 3600000;
            int dMin = (int)(rem / 60000);
            rem -= dMin * 60000;
            int dSec = (int)(rem / 1000);

            positionStr = sign + ((dHour > 0) ? (dHour + ":" + String.Format("{0:00}", dMin)) : (dMin + "")) + ":" +
                String.Format("{0:00}", dSec);

            playTimeLabel.Text = positionStr + "/" + durationStr;
        }

        protected virtual void HandleControlButtonPress(object source)
        {
            playing = !playing;

            if (playing)
            {
                controlButton.Texture = pauseTexture;
                if (playCallback != null)
                    playCallback(this);
            }
            else
            {
                controlButton.Texture = playTexture;
                if (pauseCallback != null)
                    pauseCallback(this);
            }
        }

        protected virtual void HandleSliderMove(object source)
        {
            if (playing)
                HandleControlButtonPress(null);

            curPosition = playTimeSlider.Value * 1000;
            UpdatePositionString();

            if (positionUpdateCallback != null)
                positionUpdateCallback(this);
        }

        #endregion

        #region Override Methods

        internal override void RegisterMouseInput()
        {
            base.RegisterMouseInput();

            controlButton.RegisterMouseInput();
            playTimeSlider.RegisterMouseInput();
        }

        internal override void RemoveMouseInput()
        {
            base.RemoveMouseInput();

            controlButton.RemoveMouseInput();
            playTimeSlider.RegisterMouseInput();
        }

        public override void RenderWidget()
        {
            base.RenderWidget();

            controlButton.RenderWidget();
            playTimeSlider.RenderWidget();
            playTimeLabel.RenderWidget();

            if (ShowPositionTooltip)
                tooltipLabel.RenderWidget();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the position back to the start position and stops the play. 
        /// </summary>
        public void Reset()
        {
            curPosition = playTimeSlider.Minimum * 1000;
            playTimeSlider.ChangeValue(playTimeSlider.Minimum);
            playing = false;
            controlButton.Texture = playTexture;
            UpdatePositionString();
        }

        #endregion

        #region Private Classes

        protected class PlayTimeSlider : G2DSlider
        {
            private bool showTooltip;
            private G2DLabel tooltip;

            public PlayTimeSlider(G2DLabel tooltip)
                : base()
            {
                this.tooltip = tooltip;
                showTooltip = false;
                value = 0;
                snapToTicks = false;

                tooltip.DrawBorder = true;
                tooltip.DrawBackground = true;
            }

            public bool ShowTooltip
            {
                get { return showTooltip; }
                set { showTooltip = value; }
            }

            public void ChangeValue(int val)
            {
                value = val;
                AdjustKnobBounds();
            }

            protected override void HandleMousePress(int button, Point mouseLocation)
            {
                base.HandleMousePress(button, mouseLocation);

                if (!enabled || !visible || !within)
                    return;

                if (orientation == GoblinEnums.Orientation.Horizontal)
                {
                    if (mouseLocation.X < paintBounds.X + 4)
                        Value = minValue;
                    else if (mouseLocation.X > paintBounds.X + paintBounds.Width - 4)
                        Value = maxValue;
                    else
                    {
                        int val = (int)((mouseLocation.X - (paintBounds.X + 4)) / knobIncrement + minValue);
                        Value = val;
                    }
                }
                else
                {
                    if (mouseLocation.Y < paintBounds.Y + 4)
                        Value = maxValue;
                    else if (mouseLocation.Y > paintBounds.Y + paintBounds.Height - 4)
                        Value = minValue;
                    else
                    {
                        int val = (int)(maxValue - ((mouseLocation.Y - (paintBounds.Y + 4)) / knobIncrement));
                        Value = val;
                    }
                }
            }

            protected override void HandleMouseMove(Point mouseLocation)
            {
                base.HandleMouseMove(mouseLocation);

                if (!enabled || !visible || !within || !showTooltip)
                {
                    tooltip.Visible = false;
                    return;
                }

                tooltip.Visible = true;

                int y = knobBound.Y - (tooltip.Bounds.Height + 10);
                if(y < 0)
                    y = 10 + paintBounds.Y + bounds.Height;
                tooltip.Bounds = new Rectangle(mouseLocation.X - tooltip.Bounds.Width / 2, y,
                    tooltip.Bounds.Width, tooltip.Bounds.Height);

                UpdatePositionString(mouseLocation);
            }

            private void UpdatePositionString(Point mouseLocation)
            {
                int val = 0;
                if (orientation == GoblinEnums.Orientation.Horizontal)
                {
                    if (mouseLocation.X < paintBounds.X + 4)
                        val = minValue;
                    else if (mouseLocation.X > paintBounds.X + paintBounds.Width - 4)
                        val = maxValue;
                    else
                    {
                        val = (int)((mouseLocation.X - (paintBounds.X + 4)) / knobIncrement + minValue);
                    }
                }
                else
                {
                    if (mouseLocation.Y < paintBounds.Y + 4)
                        val = maxValue;
                    else if (mouseLocation.Y > paintBounds.Y + paintBounds.Height - 4)
                        val = minValue;
                    else
                    {
                        val = (int)(maxValue - ((mouseLocation.Y - (paintBounds.Y + 4)) / knobIncrement));
                    }
                }

                String sign = "";
                long rem = val * 1000;
                if (rem < 0)
                {
                    rem = -rem;
                    sign = "-";
                }
                int dHour = (int)(rem / 3600000);
                rem -= dHour * 3600000;
                int dMin = (int)(rem / 60000);
                rem -= dMin * 60000;
                int dSec = (int)(rem / 1000);

                String positionStr = sign + ((dHour > 0) ? (dHour + ":" + String.Format("{0:00}", dMin)) : (dMin + "")) + ":" +
                    String.Format("{0:00}", dSec);

                tooltip.Text = positionStr;
            }

        }

        #endregion
    }
}
