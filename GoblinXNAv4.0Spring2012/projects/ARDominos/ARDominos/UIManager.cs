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

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.UI;
using GoblinXNA.UI.UI2D;
using GoblinXNA.SceneGraph;

namespace ARDominos
{
    public class UIManager
    {
        #region Member Fields
        // A font used for drawing the GUI texts
        private SpriteFont uiFont;

        // Some labels used for the UI
        private G2DLabel modeLabel;
        private G2DRadioButton modeRadio1;
        private G2DRadioButton modeRadio2;
        private G2DRadioButton gameAdd, gameEdit, gamePlay;
        private G2DPanel frame;

        private String modeStatusLabel;
        private SpriteFont labelFont;
        private SpriteFont victoryFont;

        // Textures for the gold, silver, and bronze trophy displayed when game is over
        private Texture2D trophyGold, trophySilver, trophyBronze;

        private Point crossHairPoint;
        private bool enableHelp;

        private int ballCount;

        private GameState gameState;
        #endregion

        #region Constructors

        public UIManager(GameState gameState)
        {
            this.gameState = gameState;

            modeStatusLabel = "Add  ";
            ballCount = 0;
            enableHelp = false;
            crossHairPoint = new Point();
        }

        #endregion

        #region Properties
        /// <summary>
        /// The main panel that holds the GUI
        /// </summary>
        public G2DPanel Frame
        {
            get { return frame; }
        }

        public G2DRadioButton GameAdd
        {
            get { return gameAdd; }
        }

        public G2DRadioButton GameEdit
        {
            get { return gameEdit; }
        }

        public G2DRadioButton GamePlay
        {
            get { return gamePlay; }
        }

        /// <summary>
        /// Whether to show the help menu for short-cut keys
        /// </summary>
        public bool EnableHelpMenu
        {
            get { return enableHelp; }
            set { enableHelp = value; }
        }

        /// <summary>
        /// The location where the cross hair cursor is shown on the screen to
        /// indicate the mouse position as well as the shooting location
        /// </summary>
        public Point CrossHairPoint
        {
            get { return crossHairPoint; }
            set { crossHairPoint = value; }
        }

        public int BallCount
        {
            get { return ballCount; }
            set { ballCount = value; }
        }

        public bool ModeChoiceEnabled
        {
            set
            {
                modeRadio1.Enabled = value;
                modeRadio2.Enabled = value;
            }
        }

        #endregion

        #region Public Methods

        public void Initialize(Scene scene, ActionPerformed gameListener, ActionPerformed modeListener)
        {
            // Create the main panel which holds all other GUI components
            frame = new G2DPanel();
            frame.Bounds = new Rectangle(30, 305, 480, 280);
            frame.Border = GoblinEnums.BorderFactory.LineBorder;
            frame.Transparency = 0.7f;  // Ranges from 0 (fully transparent) to 1 (fully opaque)
            frame.TextTransparency = 1.0f;

            uiFont = State.Content.Load<SpriteFont>("UIFont");

            G2DLabel gameLabel = new G2DLabel("Game Mode:");
            gameLabel.TextFont = uiFont;
            gameLabel.Bounds = new Rectangle(4, 4, 100, 48);

            // Create radio buttons for selecting the game mode
            gameAdd = new G2DRadioButton("Add");
            gameAdd.TextFont = uiFont;
            gameAdd.Bounds = new Rectangle(18, 70, 150, 50);
            // Make the Addition mode as the selected one first
            gameAdd.DoClick();

            gameEdit = new G2DRadioButton("Edit");
            gameEdit.TextFont = uiFont;
            gameEdit.Bounds = new Rectangle(170, 70, 150, 50);

            gamePlay = new G2DRadioButton("Play");
            gamePlay.TextFont = uiFont;
            gamePlay.Bounds = new Rectangle(310, 70, 150, 50);

            ButtonGroup gameGroup = new ButtonGroup();
            gameGroup.Add(gameAdd);
            gameGroup.Add(gameEdit);
            gameGroup.Add(gamePlay);
            gameGroup.AddActionPerformedHandler(gameListener);

            frame.AddChild(gameLabel);
            frame.AddChild(gameAdd);
            frame.AddChild(gameEdit);
            frame.AddChild(gamePlay);

            G2DSeparator separator1 = new G2DSeparator();
            separator1.Bounds = new Rectangle(5, 129, 470, 5);

            frame.AddChild(separator1);

            modeLabel = new G2DLabel("Add Mode:");
            modeLabel.TextFont = uiFont;
            modeLabel.Bounds = new Rectangle(4, 140, 100, 48);

            modeRadio1 = new G2DRadioButton("Single");
            modeRadio1.TextFont = uiFont;
            modeRadio1.Bounds = new Rectangle(20, 206, 200, 50);
            modeRadio1.DoClick();

            modeRadio2 = new G2DRadioButton("Line");
            modeRadio2.TextFont = uiFont;
            modeRadio2.Bounds = new Rectangle(220, 206, 250, 50);

            ButtonGroup addGroup = new ButtonGroup();
            addGroup.Add(modeRadio1);
            addGroup.Add(modeRadio2);
            addGroup.AddActionPerformedHandler(modeListener);

            frame.AddChild(modeLabel);
            frame.AddChild(modeRadio1);
            frame.AddChild(modeRadio2);

            // Initially, make the GUI panel invisible
            frame.Visible = false;
            frame.Enabled = false;

            scene.UIRenderer.Add2DComponent(frame);
        }

        public void SwitchMode()
        {
            switch (gameState.CurrentGameMode)
            {
                case GameState.GameMode.Add:
                    modeLabel.Text = "Add Mode:";
                    modeRadio1.Text = "Single";
                    modeRadio2.Text = "Line";
                    modeRadio2.Enabled = true;
                    modeStatusLabel = "Add  ";
                    gameState.ResetState();
                    break;
                case GameState.GameMode.Edit:
                    modeLabel.Text = "Edit Mode:";
                    modeRadio1.Text = "Single";
                    modeRadio2.Text = "Multiple";
                    modeRadio2.Enabled = false;
                    modeStatusLabel = "Edit  ";
                    gameState.ResetState();
                    break;
                case GameState.GameMode.Play:
                    modeLabel.Text = "Play Mode:";
                    // Move the cross hair cursor to the center of the screen at the start of
                    // the game play 
                    crossHairPoint = new Point(State.Width / 2, State.Height / 2);

                    // Disable certain radio buttons during the Play mode
                    ModeChoiceEnabled = false;
                    modeRadio1.DoClick();
                    break;
            }
        }

        #endregion

        #region Override Methods

        public void LoadContent()
        {
            labelFont = State.Content.Load<SpriteFont>("Sample");
            victoryFont = State.Content.Load<SpriteFont>("Victory");

            trophyGold = State.Content.Load<Texture2D>("gold_trophy");
            trophySilver = State.Content.Load<Texture2D>("silver_trophy");
            trophyBronze = State.Content.Load<Texture2D>("bronze_trophy");
        }

        public void Draw(GameTime gameTime)
        {
            // Count the time elapsed in the play mode if game is not over yet
            if (gameState.CurrentGameMode == GameState.GameMode.Play && !gameState.GameOver)
            {
                gameState.ElapsedSecond += gameTime.ElapsedGameTime.TotalSeconds;
                if (gameState.ElapsedSecond >= 60)
                {
                    gameState.ElapsedMinute++;
                    gameState.ElapsedSecond = 0;
                }
                // Update the time elapsed label with the new elapsed time
                modeStatusLabel = "Play: " + (int)gameState.ElapsedMinute + ":" + String.Format("{0:D2}",
                    (int)gameState.ElapsedSecond) + "  ";
            }

            // Prints out the help texts that shows the short-cut keys for each command
            if (enableHelp)
            {
                UI2DRenderer.WriteText(new Vector2(0, 50), "'A' -- Add Mode", Color.Red, uiFont, 
                    Vector2.One * 0.5f, GoblinEnums.HorizontalAlignment.Left, GoblinEnums.VerticalAlignment.None);
                UI2DRenderer.WriteText(new Vector2(0, 75), "'E' -- Edit Mode", Color.Red, uiFont, 
                    Vector2.One * 0.5f, GoblinEnums.HorizontalAlignment.Left, GoblinEnums.VerticalAlignment.None);
                UI2DRenderer.WriteText(new Vector2(0, 100), "'P' -- Play/Restart Game", Color.Red, uiFont, 
                    Vector2.One * 0.5f, GoblinEnums.HorizontalAlignment.Left, GoblinEnums.VerticalAlignment.None);
                UI2DRenderer.WriteText(new Vector2(0, 125), "'R' -- Reset Game", Color.Red, uiFont, 
                    Vector2.One * 0.5f, GoblinEnums.HorizontalAlignment.Left, GoblinEnums.VerticalAlignment.None);
                UI2DRenderer.WriteText(new Vector2(0, 150), "'S' -- Toggle Shadow", Color.Red, uiFont, 
                    Vector2.One * 0.5f, GoblinEnums.HorizontalAlignment.Left, GoblinEnums.VerticalAlignment.None);
                UI2DRenderer.WriteText(new Vector2(0, 175), "'G' -- Toggle GUI", Color.Red, uiFont, 
                    Vector2.One * 0.5f, GoblinEnums.HorizontalAlignment.Left, GoblinEnums.VerticalAlignment.None);
                if (gameState.CurrentGameMode == GameState.GameMode.Edit)
                    UI2DRenderer.WriteText(new Vector2(0, 200), "'D' -- Delete Selected Dominos", Color.Red, 
                        uiFont, Vector2.One * 0.5f, GoblinEnums.HorizontalAlignment.Left, 
                        GoblinEnums.VerticalAlignment.None);
                if (gameState.CurrentGameMode == GameState.GameMode.Play)
                    UI2DRenderer.WriteText(new Vector2(0, 200), "'C' -- Toggle Center Cursor Mode", Color.Red, 
                        uiFont, Vector2.One * 0.5f, GoblinEnums.HorizontalAlignment.Left, 
                        GoblinEnums.VerticalAlignment.None);
            }

            // Draws the cross hair mark to indicate the mouse position which is used for
            // ball shooting, domino addition, domino edition, and GUI interaction (if GUI is shown)
            UI2DRenderer.FillRectangle(new Rectangle(crossHairPoint.X - 10, crossHairPoint.Y,
                23, 3), null, new Color(50, 205, 50, 150));
            UI2DRenderer.FillRectangle(new Rectangle(crossHairPoint.X, crossHairPoint.Y - 10,
                3, 23), null, new Color(50, 205, 50, 150));

            UI2DRenderer.WriteText(Vector2.Zero, modeStatusLabel, Color.White, labelFont,
                GoblinEnums.HorizontalAlignment.Right, GoblinEnums.VerticalAlignment.Top);

            // Displays how many balls are used so far
            if (gameState.CurrentGameMode == GameState.GameMode.Play)
            {
                UI2DRenderer.WriteText(Vector2.Zero, "Balls Used: " + ballCount, Color.Red,
                    labelFont, GoblinEnums.HorizontalAlignment.Right, GoblinEnums.VerticalAlignment.Bottom);
            }

            // Shows the victory texts as well as play the victory sound if won
            if (gameState.GameOver)
            {
                UI2DRenderer.WriteText(new Vector2(0, 130), "Victory!! You Won!!", Color.Red,
                    victoryFont, GoblinEnums.HorizontalAlignment.Center, GoblinEnums.VerticalAlignment.None);
                UI2DRenderer.WriteText(new Vector2(0, 180), "Time: " + (int)gameState.ElapsedMinute + ":"
                    + String.Format("{0:D2}", (int)gameState.ElapsedSecond), Color.Red,
                    victoryFont, GoblinEnums.HorizontalAlignment.Center, GoblinEnums.VerticalAlignment.None);
                UI2DRenderer.WriteText(new Vector2(0, 220), "Balls Used: " + ballCount, Color.Red,
                    victoryFont, GoblinEnums.HorizontalAlignment.Center, GoblinEnums.VerticalAlignment.None);

                Texture2D trophy = null;
                if (gameState.ElapsedMinute < 1 && gameState.ElapsedSecond <= 14)
                    trophy = trophyGold;
                else if (gameState.ElapsedMinute < 1 && gameState.ElapsedSecond <= 25)
                    trophy = trophySilver;
                else
                    trophy = trophyBronze;

                UI2DRenderer.FillRectangle(new Rectangle((State.Width - trophy.Width) / 2,
                    280, trophy.Width, trophy.Height), trophy, Color.White);
            }
        }
        #endregion
    }
}
