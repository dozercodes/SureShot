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

Print out the first slide of DominoGround.ppt if you are using ARTag; otherwise, print
out the second slide if you are using ALVAR. NOTE: The size of the ppt slide is set to 
25.6 x 17.52 inches, so if you want the entire marker array to fit in A4 size paper,
then you should choose "Scale to fit" option when you print. 


Game Play Manual:


Short-cut keys used in the game:

'H' -- Display short-cut key help menu
'A' -- Switch to "Add" mode
'E' -- Switch to "Edit" mode
'P' -- Switch to "Play" mode. If pressed during "Play" mode, it restarts the game.
'R' -- Reset the game to the initial state
'S' -- Toggles the shadow mapping (NOTE: On non-NVidia graphics cards, the shadow mapping
       may have some aliasing problem)
'G' -- Toggles the GUI
'D' -- Delete selected dominos during "Edit" mode
'C' -- Toggle center cursor mode during "Play" mode


There are three game modes in this AR dominos knockdown game:

1. "Add" mode: 	This is the initial state when the game starts. In this mode, you can
	       	add dominos to the scene. There are two ways to add the dominos. One
	       	way (which is the default) is to simply left-click mouse on the gameboard,
	       	then a domino is added on the intersection point of the mouse pick ray
	       	and the gameboard. You can also drag around the domino on the gameboard
	       	with the mouse if you don't release the mouse after you press the left 
	       	mouse button. Once you release the button, the domino is added at the
	       	point on the gameboard where the mouse button is releasd. The just-added 
	       	domino appears transparent to indicate that you can modify the orientation
	       	of the domino along the z-axis (normal to the gameboard). The other way
	       	of adding dominos is to draw a line on the gameboard by dragging the mouse
	       	with left button. In order to use line-drawing method, you need to bring
	       	up the GUI by pressing 'G' key, and then switch to "Line" mode from "Single".
	       	Multiple donimos will be added on the line (the line is shown with red color)
		with certain gaps between each of the domino. You can have maximum of only
		40 dominos on the gameboard.

2. "Edit" mode:	In this mode, you can edit the position and rotation of the selected dominos.
		To select a domino, simply left-click on the domino. The selected domino 
		appears transparent. Currently, you can only select one domino at a time. 
		To change the position of the selected domino, drag the domino on the 
		gameboard with left mouse button held down. To change the rotation of the 
		domino, press Left or Right arraw key to rotate clockwise or counter-clockwise. 
		You can also delete the selected domino by pressing the 'D' key. 

3. "Play" mode: In this mode, the physics simulation is enabled, and you can throw balls into
		the scene to knock down the dominos. There are two different types of balls you
		can throw: a normal ball, and a heavier/larger ball. To throw a normal ball,
		left click on the game screen, and the ball is shot from the mouse cursor point.
		To throw a heavier/larger ball, right click on the game screen. Once you knock
		off all of the dominos from the gameboard, the game ends, and it displays the 
		time you spent to knock them all off. You get different types of trophy (gold,
		silver, or bronze) depending on this time. To restart the game (either during
		the play or after the game ends), you can either switch to other modes ("Add" 
		or "Edit" mode) and then switch back to "Play" mode or press 'P' key again. 
		