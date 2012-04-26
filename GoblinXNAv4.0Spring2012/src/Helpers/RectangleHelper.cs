/************************************************************************************ 
 *
 * Microsoft XNA Community Game Platform
 * Copyright (C) Microsoft Corporation. All rights reserved.
 * 
 *************************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace GoblinXNA.Helpers
{
    public sealed class RectangleHelper
    {
        
        /// <summary>
        /// XToRes helper method to convert 1024x640 to the current
        /// screen resolution. Used to position UI elements.
        /// </summary>
        /// <param name="xIn1024px">X in 1024px width resolution</param>
        /// <returns>Int</returns>
        public static int XToRes(int xIn1024px)
        {
            return (int)Math.Round(xIn1024px * State.Width / 1024.0f);
        }

        /// <summary>
        /// YToRes helper method to convert 1024x640 to the current
        /// screen resolution. Used to position UI elements.
        /// </summary>
        /// <param name="yIn640px">Y in 640px height</param>
        /// <returns>Int</returns>
        public static int YToRes(int yIn640px)
        {
            return (int)Math.Round(yIn640px * State.Height / 640.0f);
        }

        /// <summary>
        /// YTo res 768
        /// </summary>
        /// <param name="yIn768px">Y in 768px</param>
        /// <returns>Int</returns>
        public static int YToRes768(int yIn768px)
        {
            return (int)Math.Round(yIn768px * State.Height / 768.0f);
        }

        /// <summary>
        /// XTo res 1600
        /// </summary>
        /// <param name="xIn1600px">X in 1600px</param>
        /// <returns>Int</returns>
        public static int XToRes1600(int xIn1600px)
        {
            return (int)Math.Round(xIn1600px * State.Width / 1600.0f);
        }

        /// <summary>
        /// YTo res 1200
        /// </summary>
        /// <param name="yIn1200px">Y in 1200px</param>
        /// <returns>Int</returns>
        public static int YToRes1200(int yIn1200px)
        {
            return (int)Math.Round(yIn1200px * State.Height / 1200.0f);
        }

        /// <summary>
        /// XTo res 1400
        /// </summary>
        /// <param name="xIn1400px">X in 1400px</param>
        /// <returns>Int</returns>
        public static int XToRes1400(int xIn1400px)
        {
            return (int)Math.Round(xIn1400px * State.Width / 1400.0f);
        }

        /// <summary>
        /// YTo res 1200
        /// </summary>
        /// <param name="yIn1050px">Y in 1050px</param>
        /// <returns>Int</returns>
        public static int YToRes1050(int yIn1050px)
        {
            return (int)Math.Round(yIn1050px * State.Height / 1050.0f);
        }

        /// <summary>
        /// Calc rectangle, helper method to convert from our images (1024)
        /// to the current resolution. Everything will stay in the 16/9
        /// format of the textures.
        /// </summary>
        /// <param name="relX">X</param>
        /// <param name="relY">Y</param>
        /// <param name="relWidth">Width</param>
        /// <param name="relHeight">Height</param>
        /// <returns>Rectangle</returns>
        public static Rectangle CalcRectangle(
            int relX, int relY, int relWidth, int relHeight)
        {
            float widthFactor = State.Width / 1024.0f;
            float heightFactor = State.Height / 640.0f;
            return new Rectangle(
                (int)Math.Round(relX * widthFactor),
                (int)Math.Round(relY * heightFactor),
                (int)Math.Round(relWidth * widthFactor),
                (int)Math.Round(relHeight * heightFactor));
        }

        /// <summary>
        /// Calc rectangle with bounce effect, same as CalcRectangle, but sizes
        /// the resulting rect up and down depending on the bounceEffect value.
        /// </summary>
        /// <param name="relX">Rel x</param>
        /// <param name="relY">Rel y</param>
        /// <param name="relWidth">Rel width</param>
        /// <param name="relHeight">Rel height</param>
        /// <param name="bounceEffect">Bounce effect</param>
        /// <returns>Rectangle</returns>
        public static Rectangle CalcRectangleWithBounce(
            int relX, int relY, int relWidth, int relHeight, float bounceEffect)
        {
            float widthFactor = State.Width / 1024.0f;
            float heightFactor = State.Height / 640.0f;
            float middleX = (relX + relWidth / 2) * widthFactor;
            float middleY = (relY + relHeight / 2) * heightFactor;
            float retWidth = relWidth * widthFactor * bounceEffect;
            float retHeight = relHeight * heightFactor * bounceEffect;
            return new Rectangle(
                (int)Math.Round(middleX - retWidth / 2),
                (int)Math.Round(middleY - retHeight / 2),
                (int)Math.Round(retWidth),
                (int)Math.Round(retHeight));
        }

        /// <summary>
        /// Calc rectangle, same method as CalcRectangle, but keep the 4 to 3
        /// ratio for the image. The Rect will take same screen space in
        /// 16:9 and 4:3 modes. e.g., Buttons should be displayed this way.
        /// Should be used for 1024px width graphics.
        /// </summary>
        /// <param name="relX">Rel x</param>
        /// <param name="relY">Rel y</param>
        /// <param name="relWidth">Rel width</param>
        /// <param name="relHeight">Rel height</param>
        /// <returns>Rectangle</returns>
        public static Rectangle CalcRectangleKeep4To3(
            int relX, int relY, int relWidth, int relHeight)
        {
            float widthFactor = State.Width / 1024.0f;
            float heightFactor = State.Height / 768.0f;
            return new Rectangle(
                (int)Math.Round(relX * widthFactor),
                (int)Math.Round(relY * heightFactor),
                (int)Math.Round(relWidth * widthFactor),
                (int)Math.Round(relHeight * heightFactor));
        }

        /// <summary>
        /// Calc rectangle, same method as CalcRectangle, but keep the 4 to 3
        /// ratio for the image. The Rect will take same screen space in
        /// 16:9 and 4:3 modes. e.g., Buttons should be displayed this way.
        /// Should be used for 1024px width graphics.
        /// </summary>
        /// <param name="gfxRect">Gfx rectangle</param>
        /// <returns>Rectangle</returns>
        public static Rectangle CalcRectangleKeep4To3(
            Rectangle gfxRect)
        {
            float widthFactor = State.Width / 1024.0f;
            float heightFactor = State.Height / 768.0f;
            return new Rectangle(
                (int)Math.Round(gfxRect.X * widthFactor),
                (int)Math.Round(gfxRect.Y * heightFactor),
                (int)Math.Round(gfxRect.Width * widthFactor),
                (int)Math.Round(gfxRect.Height * heightFactor));
        }

        /// <summary>
        /// Calc rectangle for 1600px width graphics.
        /// </summary>
        /// <param name="relX">Rel x</param>
        /// <param name="relY">Rel y</param>
        /// <param name="relWidth">Rel width</param>
        /// <param name="relHeight">Rel height</param>
        /// <returns>Rectangle</returns>
        public static Rectangle CalcRectangle1600(
            int relX, int relY, int relWidth, int relHeight)
        {
            float widthFactor = State.Width / 1600.0f;

            float heightFactor = (State.Height / 1200.0f);// / (aspectRatio / (16 / 9));
            return new Rectangle(
                (int)Math.Round(relX * widthFactor),
                (int)Math.Round(relY * heightFactor),
                (int)Math.Round(relWidth * widthFactor),
                (int)Math.Round(relHeight * heightFactor));
        }

        /// <summary>
        /// Calc rectangle 2000px, just a helper to scale stuff down
        /// </summary>
        /// <param name="relX">Rel x</param>
        /// <param name="relY">Rel y</param>
        /// <param name="relWidth">Rel width</param>
        /// <param name="relHeight">Rel height</param>
        /// <returns>Rectangle</returns>
        public static Rectangle CalcRectangle2000(
            int relX, int relY, int relWidth, int relHeight)
        {
            float widthFactor = State.Width / 2000.0f;
            float heightFactor = (State.Height / 1500.0f);
            return new Rectangle(
                (int)Math.Round(relX * widthFactor),
                (int)Math.Round(relY * heightFactor),
                (int)Math.Round(relWidth * widthFactor),
                (int)Math.Round(relHeight * heightFactor));
        }

        /// <summary>
        /// Calc rectangle keep 4 to 3 align bottom
        /// </summary>
        /// <param name="relX">Rel x</param>
        /// <param name="relY">Rel y</param>
        /// <param name="relWidth">Rel width</param>
        /// <param name="relHeight">Rel height</param>
        /// <returns>Rectangle</returns>
        public static Rectangle CalcRectangleKeep4To3AlignBottom(
            int relX, int relY, int relWidth, int relHeight)
        {
            float widthFactor = State.Width / 1024.0f;
            float heightFactor16To9 = State.Height / 640.0f;
            float heightFactor4To3 = State.Height / 768.0f;
            return new Rectangle(
                (int)(relX * widthFactor),
                (int)(relY * heightFactor16To9) -
                (int)Math.Round(relHeight * heightFactor4To3),
                (int)Math.Round(relWidth * widthFactor),
                (int)Math.Round(relHeight * heightFactor4To3));
        }

        /// <summary>
        /// Calc rectangle keep 4 to 3 align bottom right
        /// </summary>
        /// <param name="relX">Rel x</param>
        /// <param name="relY">Rel y</param>
        /// <param name="relWidth">Rel width</param>
        /// <param name="relHeight">Rel height</param>
        /// <returns>Rectangle</returns>
        public static Rectangle CalcRectangleKeep4To3AlignBottomRight(
            int relX, int relY, int relWidth, int relHeight)
        {
            float widthFactor = State.Width / 1024.0f;
            float heightFactor16To9 = State.Height / 640.0f;
            float heightFactor4To3 = State.Height / 768.0f;
            return new Rectangle(
                (int)(relX * widthFactor) -
                (int)Math.Round(relWidth * widthFactor),
                (int)(relY * heightFactor16To9) -
                (int)Math.Round(relHeight * heightFactor4To3),
                (int)Math.Round(relWidth * widthFactor),
                (int)Math.Round(relHeight * heightFactor4To3));
        }

        /// <summary>
        /// Calc rectangle centered with given height.
        /// This one uses relX and relY points as the center for our rect.
        /// The relHeight is then calculated and we align everything
        /// with help of gfxRect (determinating the width).
        /// Very useful for buttons, logos and other centered UI textures.
        /// </summary>
        /// <param name="relX">Rel x</param>
        /// <param name="relY">Rel y</param>
        /// <param name="relHeight">Rel height</param>
        /// <param name="gfxRect">Gfx rectangle</param>
        /// <returns>Rectangle</returns>
        public static Rectangle CalcRectangleCenteredWithGivenHeight(
            int relX, int relY, int relHeight, Rectangle gfxRect)
        {
            float widthFactor = State.Width / 1024.0f;
            float heightFactor = State.Height / 640.0f;
            int rectHeight = (int)Math.Round(relHeight * heightFactor);
            // Keep aspect ratio
            int rectWidth = (int)Math.Round(
                gfxRect.Width * rectHeight / (float)gfxRect.Height);
            return new Rectangle(
                Math.Max(0, (int)Math.Round(relX * widthFactor) - rectWidth / 2),
                Math.Max(0, (int)Math.Round(relY * heightFactor) - rectHeight / 2),
                rectWidth, rectHeight);
        }
    }
}
