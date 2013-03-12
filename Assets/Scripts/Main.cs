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

public class Main : MonoBehaviour
{
	// Skin we are using globally
	// Note: this is defined in the Unity editor, and will *always* be set
	public GUISkin MySkin;
	
	// Use this for initialization
	protected void Start ()
	{
		// Save globals skin
		Globals.MainSkin = MySkin;
		
		// Load global settings
		Globals.LoadSettings();
		
		// Push the initial view: main menu
		Globals.PushView(gameObject.AddComponent("MainMenu") as MonoBehaviour);				// Main menu
		//Globals.PushView(gameObject.AddComponent(typeof(LevelInstance)) as MonoBehaviour);		// JUST game
	}
}
