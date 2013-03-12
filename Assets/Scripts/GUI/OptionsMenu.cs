/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: OptionsMenu.cs
 Desc: Options menu associated with graphics, music, and sound.
 
***************************************************************/

using UnityEngine;
using System.Collections;

public class OptionsMenu : MonoBehaviour
{
	// Render GUI
	void OnGUI()
	{
		// Define UI
		GUI.skin = Globals.MainSkin;
		
		// Short-hand window sizes
		int WindowWidth = 400;
		int WindowHeight = 350;
		
		// Define window
		Rect WindowRect = new Rect(Screen.width / 2 - WindowWidth / 2, Screen.height / 2 - WindowHeight / 2, WindowWidth, WindowHeight);
		GUI.Window(Globals.WinID_OptionsMenu, WindowRect, OnWindow, "");
	}
	
	// Update the window
	private void OnWindow(int WindowID) 
	{
		// Begin vertical content
		GUILayout.Space(8);
		GUILayout.BeginVertical();
		{
			GUILayout.Label("", "Divider");
			GUILayout.Label("Exodus: The Space Game - Options");
			GUILayout.Label("", "Divider");
			
			// Music volume
			GUILayout.Space(8);
			GUILayout.Label("Music Volume: " + (int)(Globals.MusicLevel * 100.0f) + "%", "PlainText");
			Globals.MusicLevel = GUILayout.HorizontalSlider(Globals.MusicLevel, 0.0f, 1.0f);
			GUILayout.Space(8);
			
			// Audio volume
			GUILayout.Space(8);
			GUILayout.Label("Audio Volume: "  + (int)(Globals.AudioLevel * 100.0f) + "%", "PlainText");
			Globals.AudioLevel = GUILayout.HorizontalSlider(Globals.AudioLevel, 0.0f, 1.0f);
			GUILayout.Space(8);
			
			GUILayout.Label("", "Divider");
			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				if(GUILayout.Button("Back"))
					Globals.PopView();
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
	}
}
