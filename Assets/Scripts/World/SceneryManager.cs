/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: SceneryManager.cs
 Desc: Manages all scenery, including resources and junk sprites.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;

// Scenery types: either resources or junk
public enum SceneryType
{
	// Resources:
	Mineral,	// Common
	Gas,		// Rare
	
	// Junk:
	Junk,		// Just junk (stays forever)
	Hull,		// Parts of ships that will eventually dissapear
}

// Trivial scenery class
public class SceneryManager_Scenery
{
	// The scenery type & paired sprite
	public SceneryType Type;
	public Sprite ScenerySprite;		// Position is just the sprite's geometry position
	
	// Resource-specific data:
	public float Radius; 				// Radius of how close a ship has to be to gather resources
	public int Minerals, MaxMinerals;	// Minerals left, and total minerals has
	
	// Junk-specific data:
	public Vector3 Velocity;			// Velocity also includes rotation (z-axis)
	
	// Hull-specific data:
	public float Age;					// How old (in seconds) this piece is
	public float MaxAge;				// Max age it can be
	
	// Mineral specific position (required because of legacy code in this project)
	public Vector2 MineralPos;
}

public class SceneryManager : MonoBehaviour
{
	/*** Internals ***/
	
	// List of objects floating around
	private List<SceneryManager_Scenery> SceneryList;
	
	/*** Script Methods ***/
	
	// Use this for initialization
	public SceneryManager()
	{
		// Alloc as needed
		SceneryList = new List<SceneryManager_Scenery>();
	}
	
	// Update is called once per frame
	void Update()
	{
		// Do nothing if paused
		if(Globals.WorldView.Paused)
			return;
		
		// Delta time
		float dT = Time.deltaTime;
		
		// Apply the play-speed factor
		if(Globals.WorldView.PlaySpeed == GameSpeed.Fast)
			dT *= 2;
		else if(Globals.WorldView.PlaySpeed == GameSpeed.Faster)
			dT *= 4;
		
		// For each junk, apply special rules
		foreach(SceneryManager_Scenery Junk in SceneryList)
		{
			// Only apply below updates if hull
			if(Junk.Type == SceneryType.Hull)
			{
				// Apply rotation and movement
				Junk.ScenerySprite.SetRotation(Junk.ScenerySprite.GetRotation() + Junk.Velocity.z * dT);
				Junk.ScenerySprite.SetPosition(Junk.ScenerySprite.GetPosition() + new Vector2(Junk.Velocity.x, Junk.Velocity.y));
				
				// Apply age
				Junk.Age += dT;
				
				// Apply color change if hull
				Junk.ScenerySprite.SetColor(new Color(1, 1, 1, 1.0f - Junk.Age / Junk.MaxAge));
			}
		}
		
		// Remove objects for two cases:
		// 1. If hull if too old
		// 2. if out of world bounds
		for(int i = SceneryList.Count - 1; i >= 0; i--)
		{
			// Only apply below updates if hull
			if(SceneryList[i].Type == SceneryType.Hull)
			{
				// Check for remove
				bool Remove = false;
				Vector2 Pos = SceneryList[i].ScenerySprite.GetPosition();
				if(SceneryList[i].Type == SceneryType.Hull && SceneryList[i].Age >= SceneryList[i].MaxAge)
					Remove = true;
				else if(Pos.x < -WorldManager.WorldWidth || Pos.x > WorldManager.WorldWidth || Pos.y < -WorldManager.WorldWidth || Pos.y > WorldManager.WorldWidth)
					Remove = true;
				
				// Remove if needed
				if(Remove)
				{
					Globals.WorldView.SManager.RemoveSprite(SceneryList[i].ScenerySprite);
					SceneryList.RemoveAt(i);
				}
			}
		}
	}
	
	/*** Public Methods ***/
	
	// Add new mineral
	public void AddMineral(Vector2 Position, float Radius, int MineralCount)
	{
		SceneryManager_Scenery SceneryItem = new SceneryManager_Scenery();
		
		SceneryItem.Type = SceneryType.Mineral;
		SceneryItem.Radius = Radius;
		SceneryItem.MineralPos = Position;
		SceneryItem.Minerals = SceneryItem.MaxMinerals = MineralCount;
		
		SceneryList.Add(SceneryItem);
	}
	
	public void AddScenery(String ConfigFileName, String GroupName, Vector3 Position, Vector3 Velocity)
	{
		// Load the config file name
		ConfigFile Info = new ConfigFile(ConfigFileName);
		String TypeString = Info.GetKey_String(GroupName, "Type");
		
		if(TypeString == null)
			TypeString = "hull"; // Force hull usage
		else
			TypeString = TypeString.ToLower();
		
		// Get texture name from group, if it does not exist in the group, get it from the general:texture tag
		String TextureName = Info.GetKey_String(GroupName, "Texture");
		if(TextureName == null)
			TextureName = Info.GetKey_String("General", "Texture");
		
		// If no texture name, complete failure
		if(TextureName == null)
			Debug.LogError("Unable to load texture for given scenery item");
		
		// Set common properties
		SceneryManager_Scenery SceneryItem = new SceneryManager_Scenery();
		SceneryItem.ScenerySprite = new Sprite("Textures/" + TextureName);
		
		SceneryItem.ScenerySprite.SetSpritePos(Info.GetKey_Vector2(GroupName, "Pos"));
		SceneryItem.ScenerySprite.SetSpriteSize(Info.GetKey_Vector2(GroupName, "Size"));
		SceneryItem.ScenerySprite.SetGeometrySize(SceneryItem.ScenerySprite.GetSpriteSize());
		SceneryItem.ScenerySprite.SetRotationCenter(SceneryItem.ScenerySprite.GetGeometrySize() / 2.0f);
		SceneryItem.ScenerySprite.SetPosition(Position);
		SceneryItem.ScenerySprite.SetRotation(Position.z);
		SceneryItem.ScenerySprite.SetDepth(Globals.JunkDepth);
		
		// If mineral
		if(TypeString.CompareTo("mineral") == 0)
		{
			SceneryItem.Type = SceneryType.Mineral;
			
			SceneryItem.Minerals = SceneryItem.MaxMinerals = Info.GetKey_Int(GroupName, "Resources");
			SceneryItem.Radius = Info.GetKey_Float(GroupName, "Radius");
			
			// No rotation for minerals
			SceneryItem.Velocity = new Vector3(0, 0, Velocity.z);
		}
		// Else, junk
		else if(TypeString.CompareTo("junk") == 0)
		{
			SceneryItem.Type = SceneryType.Junk;
			
			SceneryItem.Velocity = Velocity;
		}
		// Else, hull
		else if(TypeString.CompareTo("hull") == 0)
		{
			SceneryItem.Type = SceneryType.Hull;
			
			SceneryItem.Age = 0.0f;
			SceneryItem.MaxAge = 3.0f;
			
			SceneryItem.Velocity = Velocity;
		}
		
		// Add scenery
		SceneryList.Add(SceneryItem);
		Globals.WorldView.SManager.AddSprite(SceneryItem.ScenerySprite);
	}
	
	// Return the closest scenery element, of the given type, relative to the given position
	// Only checks for resources, nothing else. May return null if no valid resources
	public SceneryManager_Scenery GetClosestScenery(Vector2 Source, SceneryType Type)
	{
		// Find the closest element
		SceneryManager_Scenery Closest = null;
		float Distance = float.MaxValue;
		
		// For each element, if type matches and has minerals left and distance to source is smaller, save as closest
		foreach(SceneryManager_Scenery Scenery in SceneryList)
		{
			if(Scenery.Type == Type && Scenery.Minerals > 0)
			{
				float SceneryDistance = (Scenery.ScenerySprite.GetPosition() - Source).magnitude;
				if(SceneryDistance < Distance)
				{
					// Removed to supress warning: 
					//Distance = Distance;
					Closest = Scenery;
				}
			}
		}
		
		// Done: return the closest (if any)
		return Closest;
	}
	
	// Returns true if all resources have been consumed
	public bool AllResourcesConsumed()
	{
		// Check for each scenery element, if there are minerals left
		foreach(SceneryManager_Scenery Scenery in SceneryList)
			if(Scenery.Type == SceneryType.Mineral && Scenery.Minerals > 0)
				return false;
		
		// Done searching, none left
		return true;
	}
}
