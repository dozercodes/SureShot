/************************************************************************************ 
 * Copyright (c) 2008-2009, Columbia University
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

using GoblinXNA.Helpers;

namespace ARDominos
{
    public class GameState
    {
        #region Mode Enums
        /// <summary>
        /// The game mode
        /// </summary>
        public enum GameMode
        {
            /// <summary>
            /// Edit mode allows the player to modify the position and orientation of 
            /// existing dominos
            /// </summary>
            Edit,
            /// <summary>
            /// Add mode allows the player to add new dominos on the game board
            /// </summary>
            Add,
            /// <summary>
            /// Play mode allows the player to play the domino smash game in which the
            /// player throws balls at the dominos and tries to get all of them off the
            /// board
            /// </summary>
            Play
        }

        /// <summary>
        /// The sub-modes in the 'Edit' mode
        /// </summary>
        public enum EditMode
        {
            /// <summary>
            /// Select and edit single domino at a time
            /// </summary>
            Single,
            /// <summary>
            /// Select and edit multiple dominos at a time
            /// </summary>
            Multiple
        }

        /// <summary>
        /// The sub-modes in the 'Add' mode
        /// </summary>
        public enum AdditionMode
        {
            /// <summary>
            /// Add single domino at a time
            /// </summary>
            Single,
            /// <summary>
            /// Add multiple dominos on the line drawn by the player
            /// </summary>
            LineDrawing
        }
        #endregion

        #region Member Fields

        // The current game mode (Add, Edit, or Play)
        private GameMode gameMode;

        // The current edit mode (Single, or Multiple)
        private EditMode editMode;

        // The curent add mode (Single, or LineDrawing)
        private AdditionMode addMode;

        // The elapsed time since the game play is started in GameMode.Play mode
        private double elapsedSecond;
        private double elapsedMinute;

        private bool gameOver;
        private int winner;

        #endregion

        #region Constructor

        public GameState()
        {
            gameMode = GameMode.Add;
            editMode = EditMode.Single;
            addMode = AdditionMode.Single;

            elapsedMinute = 0;
            elapsedSecond = 0;

            gameOver = false;
            winner = -1;
        }

        #endregion

        #region Properties

        public GameMode CurrentGameMode
        {
            get { return gameMode; }
            set { gameMode = value; }
        }

        public EditMode CurrentEditMode
        {
            get { return editMode; }
            set { editMode = value; }
        }

        public AdditionMode CurrentAdditionMode
        {
            get { return addMode; }
            set { addMode = value; }
        }

        public double ElapsedSecond
        {
            get { return elapsedSecond; }
            set { elapsedSecond = value; }
        }

        public double ElapsedMinute
        {
            get { return elapsedMinute; }
            set { elapsedMinute = value; }
        }

        public bool GameOver
        {
            get { return gameOver; }
            set { gameOver = value; }
        }

        public int Winner
        {
            get { return winner; }
            set { winner = value; }
        }
        #endregion

        #region Public Methods
        public void ResetState()
        {
            elapsedMinute = 0;
            elapsedSecond = 0;
            gameOver = false;
        }

        #endregion
    }
}
