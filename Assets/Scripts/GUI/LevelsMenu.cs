/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: LevelsMenu.cs
 Desc: Renders all the levels in a grid-list.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections;

/*** Level Confirmation ***/

public class LevelMenu : MonoBehaviour
{
	// Window geometry
	private const int WindowWidth = 500;
	private const int WindowHeight = 600;
	
	// Level in question with description info
	private int TargetLevel = 0;
	private String Description;
	
	// A full screen texture used for a transition
	float TotalTime = 0.0f;
	private GUITexture FlashTexture;
	
	// Set the level that is in question
	public void SetLevel(int Level)
	{
		TargetLevel = Level;
		Description = LevelManager.GetLevelDescription(TargetLevel);
	}
	
	// Update once the selection is made
	void Update()
	{
		// Only update texture if it is left
		if(FlashTexture != null)
		{
			TotalTime += Time.deltaTime;
			FlashTexture.color = Color.Lerp(Color.clear, Color.black, TotalTime);
			if(TotalTime > 1.0f)
			{
				// Start game
				LaunchGame(TargetLevel);
				
				// Remove flash texture
				GameObject.Destroy(FlashTexture);
				FlashTexture = null;
			}
		}
	}
	
	// Render GUI
	void OnGUI()
	{
		// Define UI
		GUI.skin = Globals.MainSkin;
		
		// Do not draw if we are transitioning to level
		if(FlashTexture == null)
		{
			// Define window
			Rect WindowRect = new Rect(Screen.width / 2 - WindowWidth / 2, Screen.height / 2 - WindowHeight / 2, WindowWidth, WindowHeight);
			GUI.Window(Globals.WinID_LevelMenu, WindowRect, OnWindow, "");
		}
	}
	
	// Update the window
	private void OnWindow(int WindowID) 
	{
		// Add spikes on-top
		MainMenu.AddSpikes(WindowWidth);
		
		// Event handles
		bool BackHit = false;
		bool YesHit = false;
		
		// Draw the little stamp in the bottom right
		MainMenu.WaxSeal(WindowWidth, WindowHeight + 20);
		
		// Begin vertical content
		GUILayout.Space(8);
		GUILayout.BeginVertical();
		{
			GUILayout.Label("", "Divider");
			GUILayout.Label("Play level " + TargetLevel + "?");
			GUILayout.TextArea(Description);
			GUILayout.Label("", "Divider");
			
			// Yes / no buttons
			GUILayout.BeginHorizontal();
			{
				BackHit = GUILayout.Button("Back");
				YesHit = GUILayout.Button("Launch!");
			}
			GUILayout.EndHorizontal();
			
			// Done with buttons
			GUILayout.Label("", "Divider");
		}
		GUILayout.EndVertical();
		
		// Event handle
		if(YesHit)
		{
			// Set flash to clear
			FlashTexture = gameObject.AddComponent("GUITexture") as GUITexture;
			FlashTexture.color = Color.clear;
			FlashTexture.texture = Resources.Load("Textures/WhiteBlock") as Texture;
			FlashTexture.pixelInset = new Rect(0, 0, Screen.width, Screen.height);
		}
		else if(BackHit)
			Globals.PopView();
	}
	
	// Start a new game
	private void LaunchGame(int TargetLevel)
	{
		// Kill the background object
		Globals.MenuBackground.enabled = false;
		
		// Create the single game instance
		Globals.WorldView = gameObject.AddComponent("WorldManager") as WorldManager;
		Globals.WorldView.TargetLevel = TargetLevel;
		Globals.PushView(Globals.WorldView);
	}
}

/*** Level Selection ***/

public class LevelsMenu : MonoBehaviour
{
	// Window geometry
	private const int WindowWidth = 500;
	private const int WindowHeight = 400;
	
	// How many total levels we have, and how much to show on each row
	private int LevelCount = 14;
	private const int LevelsPerRow = 4;
	
	// Level player wants to play
	private int LevelIndex = 0; // 0 is default (no selection)
	
	// Lock icon
	private Texture LockIcon;
	private Texture UnlockIcon;
	
	// Object init
	void Start()
	{
		LevelCount = LevelManager.GetLevelCount();
		LockIcon = Resources.Load("Textures/Lock") as Texture;
		UnlockIcon = Resources.Load("Textures/Unlock") as Texture;
	}
	
	// Render GUI
	void OnGUI()
	{
		// Define UI
		GUI.skin = Globals.MainSkin;
		
		// Define window
		Rect WindowRect = new Rect(Screen.width / 2 - WindowWidth / 2, Screen.height / 2 - WindowHeight / 2, WindowWidth, WindowHeight);
		GUI.Window(Globals.WinID_LevelsMenu, WindowRect, OnWindow, "");
	}
	
	// Update the window
	private void OnWindow(int WindowID) 
	{
		// Add spikes on-top
		MainMenu.AddSpikes(WindowWidth);
		MainMenu.FancyTop(WindowWidth);
		
		// Event handles
		bool BackHit = false;
		
		// Begin vertical content
		GUILayout.Space(8);
		GUILayout.BeginVertical();
		{
			GUILayout.Label("", "Divider");
			GUILayout.Label("Level Selection");
			GUILayout.Label("", "Divider");
			
			// Start buttons
			int ButtonCount = 1;
			for(int y = 0; y < Mathf.CeilToInt((float)LevelCount / (float)LevelsPerRow); y++)
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.FlexibleSpace();
					for(int x = 0; x < LevelsPerRow && ButtonCount <= LevelCount; x++)
					{
						// If pressed, save level target
						bool LevelUnlocked = LevelManager.GetLevelUnlocked(ButtonCount - 1);
						if(GUILayout.Button(new GUIContent("Level " + ButtonCount, LevelUnlocked ? UnlockIcon : LockIcon)))
						{
							if(LevelUnlocked)
								LevelIndex = ButtonCount;
						}
						ButtonCount++;
					}
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndHorizontal();
			}
			
			// Done with buttons
			GUILayout.Label("", "Divider");
			BackHit = GUILayout.Button("Back");
		}
		GUILayout.EndVertical();
		
		// Event handle
		if(LevelIndex > 0 && LevelIndex <= LevelCount)
		{
			// Prep and add view
			LevelMenu LevelView = gameObject.AddComponent(typeof(LevelMenu)) as LevelMenu;
			LevelView.SetLevel(LevelIndex);
			
			// Pushed & reset internal value
			Globals.PushView(LevelView);
			LevelIndex = 0;
		}
		else if(BackHit)
			Globals.PopView();
	}
}
