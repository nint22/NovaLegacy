/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: Globals.cs
 Desc: A simple singleton data structure that allows access to
 global variables / helper functions
 
***************************************************************/

using UnityEngine;
using System.Collections.Generic;

// Speed enumerations
public enum GameSpeed
{
	Normal,
	Fast,
	Faster,
}

// Gobal static vars / functions
public class Globals
{
	/*** GUI Info. ***/
	
	// Skin we are using globally
	public static GUISkin MainSkin;
	
	// The menu system's background handle
	// This is important to disable once out of menus! (Bad lag if left unremoved)
	public static BackgroundManager MenuBackground;
	
	/*** Active Game ***/
	
	// Main game objects (only once instance at a time)
	public static WorldManager WorldView;
	
	/*** Configuration Values ***/
	
	// Max music level, audio level
	public static float MusicLevel;
	public static float AudioLevel;
	
	// Load settings
	public static void LoadSettings()
	{
		MusicLevel = PlayerPrefs.GetFloat("MusicLevel", 1.0f);
		AudioLevel = PlayerPrefs.GetFloat("AudioLevel", 1.0f);
	}
	
	/**** Camera Properties ****/
	
	// Min / max camera size (default is 100)
	public static float MinCameraSize = 100.0f;		// How much we can zoom in
	public static float MaxCameraSize = 800.0f;		// How much we can zoom out
	public static float CameraRate = 5.0f;			// How fast we zoom in and out
	public static float CameraSpeed = 100.0f;		// Camera translation with WASD speeds
	
	// Mouse drag speed factor
	public static float MouseDragSensitivity = 500.0f;
	
	/*** View Management ***/
	
	// Global GUI stack: we push and pop view-scenes from the game
	private static Stack<MonoBehaviour> FormStack = new Stack<MonoBehaviour>();
	
	// How much is on the stack
	public static int GetViewStackCount()
	{
		return FormStack.Count;
	}
	
	// Push a new object ontop of view
	public static void PushView(MonoBehaviour View)
	{
		// Disable the current view (if any)
		if(FormStack.Count > 0)
			FormStack.Peek().enabled = false;
		
		// Add current view
		View.enabled = true;
		FormStack.Push(View);
	}
	
	// Pop the current view
	public static void PopView()
	{
		// Pop off the top-most object, and remove it completely
		if(FormStack.Count > 1)
		{
			// Disable & destroy this view we are in
			MonoBehaviour View = FormStack.Pop();
			View.enabled = false;
			Object.Destroy(View);
			
			// Activate the older view again
			FormStack.Peek().enabled = true;
		}
	}
	
	/*** Collision ***/
	
	// Returns true if the segments (in a 1D axis) overlap in any way)
	public static bool LinesCollisionCheck(float a1, float a2, float b1, float b2)
	{
		// Check if a is in b
		if((a1 >= b1 && a1 <= b2) || (a2 >= b1 && a2 <= b2))
			return true;
		else if((b1 >= a1 && b1 <= a2) || (b2 >= a1 && b2 <= a2))
			return true;
		else
			return false;
	}
	
	// Collision detection for two rectangles: returns true if they intersect in any way
	// Based on: http://www.metanetsoftware.com/technique/tutorialA.html
	public static bool RectsCollisionCheck(Rect A, Rect B)
	{
		// Is collision if we overlap in BOTH dimensions
		bool XOverlap = LinesCollisionCheck(A.x, A.x + A.width, B.x, B.x + B.width);
		bool YOverlap = LinesCollisionCheck(A.y, A.y + A.height, B.y, B.y + B.height);
		
		return XOverlap && YOverlap;
	}
	
	/*** List of all depth layers ***/
	
	// Note: Larger numbers are further away from camera, and camera is -10
	public const int BackgroundDepth = 20;
	public const int MineralsDepth = 16;
	public const int JunkDepth = 15;
	public const int BuildingDepth = 13;
	public const int ContrailDepth = 12;
	public const int FrameDepth = 11;
	public const int HullDepth = 10;
	public const int ShipDetailsDepth = 9;
	public const int WeaponsDepth = 8;
	public const int ShieldDepth = 7;
	public const int ExplosionDepth = 6;
	
	/*** List of all Window Unique IDs ***/
	
	// Note: All windows, when owned by the same script, MUST have unique IDs
	public const int WinID_Minimap = 0;
	public const int WinID_ButtonsMenu = 1;
	public const int WinID_EntityMenu = 2;
	public const int WinID_PauseMenu = 3;
	public const int WinID_QuitMenu = 4;
	public const int WinID_About = 5;
	public const int WinID_HelpMenu = 6;
	public const int WinID_LevelMenu = 7;
	public const int WinID_LevelsMenu = 8;
	public const int WinID_MainMenu = 9;
	public const int WinID_OptionsMenu = 10;
	public const int WinID_WinMenu = 11;
	public const int WinID_LoseMenu = 12;
}
