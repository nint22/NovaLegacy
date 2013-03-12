/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: WinLoseMenu.cs
 Desc: Implements two views that show the winning and loosing
 GUI views for the player.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections;

/*** Win Confirmation ***/

public class WinMenu : MonoBehaviour
{
	// Window geometry
	private const int WindowWidth = 500;
	private const int WindowHeight = 400;
	
	// Level in question with description info
	private int TargetLevel = 0;
	private String Description;
	
	// Set the level that is in question
	public void SetLevel(int Level)
	{
		TargetLevel = Level;
		Description = LevelManager.GetLevelWinText(TargetLevel);
	}
	
	// Render GUI
	void OnGUI()
	{
		// Define UI
		GUI.skin = Globals.MainSkin;
		
		// Define window
		Rect WindowRect = new Rect(Screen.width / 2 - WindowWidth / 2, Screen.height / 2 - WindowHeight / 2, WindowWidth, WindowHeight);
		GUI.Window(Globals.WinID_WinMenu, WindowRect, OnWindow, "");
	}
	
	// Update the window
	private void OnWindow(int WindowID) 
	{
		// Add spikes on-top
		MainMenu.AddSpikes(WindowWidth);
		
		// Event handles
		bool YesHit = false;
		
		// Draw the little stamp in the bottom right
		MainMenu.WaxSeal(WindowWidth, WindowHeight + 20);
		
		// Begin vertical content
		GUILayout.Space(8);
		GUILayout.BeginVertical();
		{
			GUILayout.Label("", "Divider");
			GUILayout.Label("Successfully Completed Level " + TargetLevel + "!");
			GUILayout.TextArea(Description);
			GUILayout.Label("", "Divider");
			
			// Continue button...
			GUILayout.BeginHorizontal();
			YesHit = GUILayout.Button("Back to Level Select");
			GUILayout.EndHorizontal();
			
			// Done with buttons
			GUILayout.Label("", "Divider");
		}
		GUILayout.EndVertical();
		
		// Event handle
		if(YesHit)
		{
			// We must pop everything off the stack until level select (2nd)
			while(Globals.GetViewStackCount() > 2)
				Globals.PopView();
			
			// Explicitly destroy the game view
			GameObject.DestroyImmediate(Globals.WorldView);
			
			// Turn background back on
			Camera.main.orthographicSize = 100.0f; // Default zoom distance
			Globals.MenuBackground.enabled = true;
		}
	}
}

public class LoseMenu : MonoBehaviour
{
	// Window geometry
	private const int WindowWidth = 500;
	private const int WindowHeight = 400;
	
	// Level in question with description info
	private int TargetLevel = 0;
	private String Description;
	
	// Set the level that is in question
	public void SetLevel(int Level)
	{
		TargetLevel = Level;
		Description = LevelManager.GetLevelLoseText(TargetLevel);
	}
	
	// Render GUI
	void OnGUI()
	{
		// Define UI
		GUI.skin = Globals.MainSkin;
		
		// Define window
		Rect WindowRect = new Rect(Screen.width / 2 - WindowWidth / 2, Screen.height / 2 - WindowHeight / 2, WindowWidth, WindowHeight);
		GUI.Window(Globals.WinID_LoseMenu, WindowRect, OnWindow, "");
	}
	
	// Update the window
	private void OnWindow(int WindowID) 
	{
		// Add spikes on-top
		MainMenu.AddSpikes(WindowWidth);
		
		// Event handles
		bool YesHit = false;
		
		// Draw the little stamp in the bottom right
		MainMenu.WaxSeal(WindowWidth, WindowHeight + 20);
		
		// Begin vertical content
		GUILayout.Space(8);
		GUILayout.BeginVertical();
		{
			GUILayout.Label("", "Divider");
			GUILayout.Label("You Lost Level " + TargetLevel + "!");
			GUILayout.TextArea(Description);
			GUILayout.Label("", "Divider");
			
			// Continue button...
			GUILayout.BeginHorizontal();
			YesHit = GUILayout.Button("Back to Level Select");
			GUILayout.EndHorizontal();
			
			// Done with buttons
			GUILayout.Label("", "Divider");
		}
		GUILayout.EndVertical();
		
		// Event handle
		if(YesHit)
		{
			// We must pop everything off the stack until level select (2nd)
			while(Globals.GetViewStackCount() > 2)
				Globals.PopView();
			
			// Explicitly destroy the game view
			GameObject.DestroyImmediate(Globals.WorldView);
			
			// Turn background back on
			Camera.main.orthographicSize = 100.0f; // Default zoom distance
			Globals.MenuBackground.enabled = true;
		}
	}
}

