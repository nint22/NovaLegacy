/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: LevelManager.cs
 Desc: Returns level data and sets level success over time. It is
 important to note that to load up a new level, two steps must be
 taken based on Unity3D's resource loading system: Unity's LoadScene
 function only loads the content after a frame render, and thus
 the owner of this object must both construct it using the
 standard constructor, but then call the "ParseScene()" function,
 only and only when Unity calls the owner's (or some associated
 MonoBehavior object) "MonoBehavior.OnLevelWasLoaded()". Curently
 we do this through the WorldManager.
 
***************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

// Scenery and mineral wrapper: config file name, pos, and velocity
public class LevelManager_Scenery
{
	// Type (true if mineral, else is junk)
	public bool IsMineral;
	
	// Position (center of scenery
	public Vector2 Pos;
	
	// Radius
	public float Radius;
	
	// If mineral, mineral count
	public int MineralCount;
}

// Spawning group of units: how many of each classification
public class LevelManager_SpawnGroup
{
	// When, in seconds since level start, should this group spawn
	public float SpawnTime;
	
	// Position at which this group spawns
	public Vector2 SpawnPos;
	
	// For now, we only support three enemies
	public int Class0Count;
	public int Class1Count;
	public int Class2Count;
}

// Winning case logic (only one per scene!)
// Note: these are all logical "or"'ed, as in you can complete ANY
// of the requirements to win the game
public class LevelManager_WinningState
{
	// True if we win after a certain time
	public bool IfWinTime = false;
	public int WinTime = 0;
	
	// True if you must collect all resources
	public bool IfWinResources = false;
	
	// True if you must kill all enemies to win
	public bool IfWinKillAll = false;
}

// Level manager generates levels from a seed
// Similar to Minecraft: same seed always generates the same level
public class LevelManager
{
	/*** Internals / Level Data ***/
	
	// Resources and scenery list
	private List<LevelManager_Scenery> SceneryList;
	
	// For this level, define a list of spawns
	private Queue<LevelManager_SpawnGroup> SpawnList;
	
	// Level description / story
	private String Description, WinText, LoseText;
	
	// Win case logic
	private LevelManager_WinningState WinLogic = null;
	
	/*** Public Members ***/
	
	// Generate a given level index
	public LevelManager(int LevelIndex)
	{
		// Construct the level based on the seed
		ConfigFile Info = new ConfigFile("Config/World/LevelsConfig");
		
		// Get the level name
		String LevelName = Info.GetKey_String("Level" + LevelIndex, "Scene");
		Application.LoadLevelAdditive(LevelName);
		
		// Level world size
		int LevelSize = Info.GetKey_Int("Level" + LevelIndex, "Size");
		
		// Get full long-text level description
		Description = Info.GetKey_String("Level" + LevelIndex, "description");
		WinText = Info.GetKey_String("Level" + LevelIndex, "win");
		LoseText = Info.GetKey_String("Level" + LevelIndex, "lose");
	}
	
	// This function MUST be called once the level data, through the scene, has been loaded
	// It is essentially "MonoBehavior.OnLevelWasLoaded()"
	public void ParseScene()
	{
		// Alloc our lists as needed
		SceneryList = new List<LevelManager_Scenery>();
		SpawnList = new Queue<LevelManager_SpawnGroup>();
		
		// For each new LevelEntity..
		UnityEngine.Object[] LevelEntities = GameObject.FindSceneObjectsOfType(typeof(LevelEntity));
		foreach(UnityEngine.Object _Entity in LevelEntities)
		{
			// Cast over to LevelEntity type
			LevelEntity Entity = _Entity as LevelEntity;
			Vector2 EntityPos = new Vector2(Entity.transform.position.x, Entity.transform.position.y);
			
			// If mineral...
			if(Entity.EntityType == 0)
			{
				// Change the depth of the mineral
				Entity.transform.position = new Vector3(Entity.transform.position.x, Entity.transform.position.y, Globals.MineralsDepth);
				
				// Create the internal mineral object
				LevelManager_Scenery Scenery = new LevelManager_Scenery();
				Scenery.IsMineral = true;
				Scenery.Pos = new Vector2(Entity.transform.position.x, Entity.transform.position.y);
				Scenery.Radius = (Entity.transform.localScale.x + Entity.transform.localScale.y) / 8.0f;
				Scenery.MineralCount = Entity.MineralCount;
				SceneryList.Add(Scenery);
			}
			// If junk...
			else if(Entity.EntityType == 1)
			{
				// Do nothing, let Unity3D manage our junk scenery
				Entity.transform.position = new Vector3(Entity.transform.position.x, Entity.transform.position.y, Globals.JunkDepth);
			}
			// If enemy...
			else if(Entity.EntityType == 2)
			{
				// Randomize the unit composition
				LevelManager_SpawnGroup Group = new LevelManager_SpawnGroup();
				Group.SpawnTime = Entity.EnemySpawnTime;
				Group.SpawnPos = EntityPos;
				Group.Class0Count = Entity.EnemyType0Count;
				Group.Class1Count = Entity.EnemyType1Count;
				Group.Class2Count = Entity.EnemyType2Count;
				
				// Add to queue
				SpawnList.Enqueue(Group);
			}
			// If text event...
			else if(Entity.EntityType == 3)
			{
				// Todo...
			}
			// If win condition...
			else if(Entity.EntityType == 4)
			{
				// Check for duplicate...
				if(WinLogic != null)
					Debug.LogError("Duplicate win-logic entity defined in same scene!");
				
				WinLogic = new LevelManager_WinningState();
				WinLogic.IfWinTime = Entity.IfWinTime;
				WinLogic.WinTime = Entity.WinTime;
				WinLogic.IfWinKillAll = Entity.IfWinKillAll;
				WinLogic.IfWinResources = Entity.IfWinResources;
			}
		}
	}
	
	private Vector2 GetVector2(System.Random Rand)
	{
		const float WorldWidth = WorldManager.WorldWidth;
		return new Vector2(
			-WorldWidth + 2.0f * WorldWidth * (float)Rand.NextDouble(),
			-WorldWidth + 2.0f * WorldWidth * (float)Rand.NextDouble()
		);
	}
	
	private Vector3 GetVector3(System.Random Rand)
	{
		const float RotationLimits = 0.1f;
		return new Vector3(0, 0, -RotationLimits + 2.0f * RotationLimits * (float)Rand.NextDouble());
	}
	
	// Returns the generated level scenery items
	public LevelManager_Scenery[] GetScenery()
	{
		return SceneryList.ToArray();
	}
	
	// Returns the generated enemy-spawn list
	public LevelManager_SpawnGroup[] GetSpawnList()
	{
		return SpawnList.ToArray();
	}
	
	// Get win-state logic
	public LevelManager_WinningState GetWinLogic()
	{
		return WinLogic;
	}
	
	// Return the entire description, win, and lose string for the loaded level
	public String GetDescription()
	{
		return Description;
	}
	
	public String GetWinText()
	{
		return WinText;
	}
	
	public String GetLoseText()
	{
		return LoseText;
	}
	
	/*** Static Access ***/
	
	// Return the entire description string for a given level
	public static String GetLevelDescription(int LevelIndex)
	{
		ConfigFile Info = new ConfigFile("Config/World/LevelsConfig");
		return Info.GetKey_String("Level" + LevelIndex, "description");
	}
	
	// Return the winning text for the given level
	public static String GetLevelWinText(int LevelIndex)
	{
		ConfigFile Info = new ConfigFile("Config/World/LevelsConfig");
		return Info.GetKey_String("Level" + LevelIndex, "win");
	}
	
	// Return the losing text for the given level
	public static String GetLevelLoseText(int LevelIndex)
	{
		ConfigFile Info = new ConfigFile("Config/World/LevelsConfig");
		return Info.GetKey_String("Level" + LevelIndex, "lose");
	}
	
	// Returns the number of levels
	public static int GetLevelCount()
	{
		ConfigFile Info = new ConfigFile("Config/World/LevelsConfig");
		return Info.GetGroupNames().Length;
	}
	
	// Returns true or false if the level is unlocked
	public static bool GetLevelUnlocked(int LevelIndex)
	{
		// First level index, always return unlocked (true)
		if(LevelIndex == 0)
			return true;
		
		// Return true if the level is unlocked (i.e. true)
		return (PlayerPrefs.GetInt("Level" + LevelIndex + "_Unlocked", 0) != 0);
	}
	
	// Set the level to be unlocked
	public static void SetLevelUnlocked(int LevelIndex)
	{
		// Unlock the given level index
		PlayerPrefs.SetInt("Level" + LevelIndex + "_Unlocked", 1);
	}
}
