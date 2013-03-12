/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: Main.cs
 Desc: Main application entry point - this is the current test bed
 to test your code and eventually should be the true init-point
 of the game!
 
***************************************************************/

using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour
{
	// Game logo
	private Texture GameLogo;
	
	// Window geometry
	private const int WindowWidth = 300;
	private const int WindowHeight = 400;
	
	// Logo size
	private const int LogoWidth = 375;
	private const int LogoHeight = 135;
	
	// Entire GUI vertical offset
	private const int VOffset = 60;
	
	// Use this for initialization
	void Start()
	{
		// Load logo
		GameLogo = Resources.Load("Textures/Logo") as Texture;
		
		// Register a background for the menus
		Globals.MenuBackground = gameObject.AddComponent("BackgroundManager") as BackgroundManager;
	}
	
	// Render GUI
	void OnGUI()
	{
		// Define UI
		GUI.skin = Globals.MainSkin;
		
		// Draw logo
		GUI.DrawTexture(new Rect(Screen.width / 2 - LogoWidth / 2, Screen.height / 2 - WindowHeight / 2 - LogoHeight + VOffset, LogoWidth, LogoHeight), GameLogo);
		
		// Define window
		Rect WindowRect = new Rect(Screen.width / 2 - WindowWidth / 2, Screen.height / 2 - WindowHeight / 2 + VOffset, WindowWidth, WindowHeight);
		GUI.Window(Globals.WinID_MainMenu, WindowRect, OnWindow, "");
	}
	
	// Update the window
	private void OnWindow(int WindowID) 
	{
		// Add spikes on-top
		AddSpikes(WindowWidth);
		FancyTop(WindowWidth);
		
		// Which buttons were pressed
		bool PlayHit = false;
		bool HelpHit = false;
		bool AboutHit = false;
		bool OptionsHit = false;
		bool ExitHit = false;
		
		// Begin vertical content
		GUILayout.Space(8);
		GUILayout.BeginVertical();
		{
			GUILayout.Label("", "Divider");
			GUILayout.Label("Main Menu");
			GUILayout.Label("", "Divider");
			PlayHit = GUILayout.Button("Play Game");
			HelpHit = GUILayout.Button("Help");
			AboutHit = GUILayout.Button("About");
			GUILayout.Label("", "Divider");
			OptionsHit = GUILayout.Button("Options");
			ExitHit = GUILayout.Button("Exit");
			GUILayout.Space(8);
			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				GUILayout.Label("Copyright (c) 2012 - Core S2", "LightText");
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
		
		// Handle events
		if(PlayHit)
		{
			// Push the new level-select view
			LevelsMenu LevelsView = gameObject.AddComponent("LevelsMenu") as LevelsMenu;
			Globals.PushView(LevelsView);
		}
		else if(HelpHit)
		{
			HelpMenu HelpView = gameObject.AddComponent("HelpMenu") as HelpMenu;
			Globals.PushView(HelpView);
		}
		else if(AboutHit)
		{
			AboutMenu AboutView = gameObject.AddComponent("AboutMenu") as AboutMenu;
			Globals.PushView(AboutView);
		}
		else if(OptionsHit)
		{
			OptionsMenu OptionsView = gameObject.AddComponent("OptionsMenu") as OptionsMenu;
			Globals.PushView(OptionsView);
		}
		else if(ExitHit)
			Application.Quit();
	}
	
	/*** Global GUI Helpers ***/
	
	// Render the spikes up-top
	public static void AddSpikes(int winX)
	{
		// Code from Necromancer GUI demo
		int spikeCount = (int)Mathf.Floor(winX - 152)/22;
		GUILayout.BeginHorizontal();
		GUILayout.Label ("", "SpikeLeft");
		
		for (int i = 0; i < spikeCount; i++)
			GUILayout.Label ("", "SpikeMid");
		
		GUILayout.Label ("", "SpikeRight");
		GUILayout.EndHorizontal();
	}
	
	// Fanyc icon graphic
	public static void FancyTop(int x)
	{
		int leafOffset = (x/2)-64;
		int frameOffset = (x/2)-27;
		int skullOffset = (x/2)-20;
		GUI.Label(new Rect(leafOffset, 18, 0, 0), "", "GoldLeaf");
		GUI.Label(new Rect(frameOffset, 3, 0, 0), "", "IconFrame");	
		GUI.Label(new Rect(skullOffset, 12, 0, 0), "", "Skull");
	}
	
	// Just the wax seal
	public static void WaxSeal(int x, int y)
	{
		int WSwaxOffsetX = x - 120;
		int WSwaxOffsetY = y - 115;
		int WSribbonOffsetX = x - 114;
		int WSribbonOffsetY = y - 83;
		
		GUI.Label(new Rect(WSribbonOffsetX, WSribbonOffsetY, 0, 0), "", "RibbonBlue");
		GUI.Label(new Rect(WSwaxOffsetX, WSwaxOffsetY, 0, 0), "", "WaxSeal");
	}
}
