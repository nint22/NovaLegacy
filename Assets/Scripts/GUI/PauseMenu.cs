/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: PauseMenu.cs
 Desc: Shown to the user when pause is pressed.
 
***************************************************************/

using UnityEngine;
using System.Collections;

// Quit-confirm menu
public class QuitMenu : MonoBehaviour
{
	// Render GUI
	void OnGUI()
	{
		// Define UI
		GUI.skin = Globals.MainSkin;
		
		// Short-hand window sizes
		int WindowWidth = 450;
		int WindowHeight = 200;
		
		// Define window
		Rect WindowRect = new Rect(Screen.width / 2 - WindowWidth / 2, Screen.height / 2 - WindowHeight / 2, WindowWidth, WindowHeight);
		GUI.Window(Globals.WinID_QuitMenu, WindowRect, OnWindow, "");
	}
	
	// Update the window
	private void OnWindow(int WindowID) 
	{
		// Event handles
		bool BackHit = false;
		bool QuitHit = false;
		
		// Begin vertical content
		GUILayout.Space(8);
		GUILayout.BeginVertical();
		{
			GUILayout.Label("", "Divider");
			GUILayout.Label("Confirm Quit Game");
			GUILayout.Label("", "Divider");
			
			// Yes / no buttons
			GUILayout.BeginHorizontal();
			{
				BackHit = GUILayout.Button("Back");
				QuitHit = GUILayout.Button("Quit Game");
			}
			GUILayout.EndHorizontal();
			
			// Done with buttons
			GUILayout.Label("", "Divider");
		}
		GUILayout.EndVertical();
		
		// Event handle
		if(QuitHit)
		{
			// We must pop everything off the stack until empty
			while(Globals.GetViewStackCount() > 1)
				Globals.PopView();
			
			// Turn background back on
			Camera.main.orthographicSize = 100.0f; // Default zoom distance
			Globals.MenuBackground.enabled = true;
			
			// Explicitly remove the world manager
			Object.Destroy(Globals.WorldView);
		}
		else if(BackHit)
			Globals.PopView();
	}
}

// Pause menu
public class PauseMenu : MonoBehaviour
{
	// Render GUI
	void OnGUI()
	{
		// Define UI
		GUI.skin = Globals.MainSkin;
		
		// Short-hand window sizes
		int WindowWidth = 250;
		int WindowHeight = 350;
		
		// Define window
		Rect WindowRect = new Rect(Screen.width / 2 - WindowWidth / 2, Screen.height / 2 - WindowHeight / 2, WindowWidth, WindowHeight);
		GUI.Window(Globals.WinID_PauseMenu, WindowRect, OnWindow, "");
	}
	
	// Update is called once per frame
	void Update()
	{
		// Pause check
		if(Input.GetKeyDown(KeyCode.Escape) && Globals.WorldView.Paused == true)
		{
			// Flip pause state
			Globals.WorldView.Paused = false;
			
			// Pop off this menu
			Globals.PopView();
		}
	}
	
	// Update the window
	private void OnWindow(int WindowID) 
	{
		// Button states
		bool HelpHit = false, OptionsHit = false, QuitHit = false;
		
		// Begin vertical content
		GUILayout.Space(8);
		GUILayout.BeginVertical();
		{
			GUILayout.Label("", "Divider");
			GUILayout.Label("Paused");
			GUILayout.Label("", "Divider");
			
			HelpHit = GUILayout.Button("Help");
			OptionsHit = GUILayout.Button("Options");
			QuitHit = GUILayout.Button("Quit Game");
			
			GUILayout.Label("", "Divider");
			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				if(GUILayout.Button("Back"))
				{
					Globals.WorldView.Paused = false;
					Globals.PopView();
				}
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
		
		// Deal with events
		if(HelpHit)
		{
			HelpMenu HelpView = gameObject.AddComponent("HelpMenu") as HelpMenu;
			Globals.PushView(HelpView);
		}
		// Options
		else if(OptionsHit)
		{
			OptionsMenu OptionsView = gameObject.AddComponent("OptionsMenu") as OptionsMenu;
			Globals.PushView(OptionsView);
		}
		// Ask for confirmation to quit
		else if(QuitHit)
		{
			QuitMenu QuitView = gameObject.AddComponent(typeof(QuitMenu)) as QuitMenu;
			Globals.PushView(QuitView);
		}
	}
}
