/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: AboutMenu.cs
 Desc: Shows the game's story.
 
***************************************************************/

using UnityEngine;
using System.Collections;

public class AboutMenu : MonoBehaviour
{
	// Render GUI
	void OnGUI()
	{
		// Define UI
		GUI.skin = Globals.MainSkin;
		
		// Short-hand window sizes
		int WindowWidth = Screen.width;
		int WindowHeight = Screen.height;
		
		// Define window
		Rect WindowRect = new Rect(Screen.width / 2 - WindowWidth / 2, Screen.height / 2 - WindowHeight / 2, WindowWidth, WindowHeight);
		GUI.Window(Globals.WinID_About, WindowRect, OnWindow, "");
	}
	
	// Update the window
	private void OnWindow(int WindowID) 
	{
		// Begin vertical content
		GUILayout.Space(8);
		GUILayout.BeginVertical();
		{
			GUILayout.Label("", "Divider");
			GUILayout.Label("About Exodus: The Space Game");
			GUILayout.Label("", "Divider");
			
			// Content here...
			
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
