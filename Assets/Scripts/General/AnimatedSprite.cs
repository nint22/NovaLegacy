/***************************************************************
 
 Harp - Beat-based action-adventure game
 Copyright (c) 2012 'Scott Cierski'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: AnimatedSprite.cs
 Desc: A sprite-wrapper that animates sprites over time, based
 on simple config-file property definitions.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// Animation structure
internal class SpriteAnimation
{
	public String Name;			// Animation name
	public Vector2 Pos;			// Where the animation starts
	public float Rotation;		// Rotation velocity
	public Vector2 Frame;		// The cell size of each animation
	public int FrameCount;		// Total number of frames
	public float FrameTime;		// Seconds to stall between frames (as a fraction of seconds)
}

// The sprite-derived animated sprite
public class AnimatedSprite : Sprite
{
	// List of known animations
	private Dictionary<string, SpriteAnimation> Animations = null;
	
	// Config file name
	private String FileName = null;
	
	// Our texture name
	private string TextureName = null;
	
	// Active animation
	SpriteAnimation ActiveAnimation = null;
	
	// Config file we are working on
	private ConfigFile Config;
	public ConfigFile GetConfig() { return Config; }
	
	// Constructor loads the given sprite
	public AnimatedSprite(string FileName)
	{
		// Load the config file
		this.FileName = FileName;
		LoadConfig(FileName);
		
		// Load self as appropriate sprite
		base.__Init(TextureName);
	}
	
	// Load all animations and properties from the configuration file
	private void LoadConfig(string FileName)
	{
		// Load config
		Config = new ConfigFile(FileName);
		
		// Get the texture name
		TextureName = Config.GetKey_String("General", "Texture");
		
		// Create new animations dictionary, loosing the rest
		Animations = new Dictionary<string, SpriteAnimation>();
		
		// Get all group names for the animations set
		String[] Groups = Config.GetGroupNames();
		foreach(String Group in Groups)
		{
			// Ignore if non-animation group
			if(Group == "general")
				continue;
			else
			{
				// Fill a sprite animation struct and save it as the group's name
				SpriteAnimation Animation = new SpriteAnimation();
				Animation.Name = Group;
				Animation.Pos = Config.GetKey_Vector2(Group, "Pos");
				Animation.Rotation = Config.GetKey_Float(Group, "Rotation");
				Animation.Frame = Config.GetKey_Vector2(Group, "Size");
				Animation.FrameCount = Config.GetKey_Int(Group, "Count");
				Animation.FrameTime = Config.GetKey_Float(Group, "Time");
				
				// Save
				Animations.Add(Group, Animation);
			}
		}
	}
	
	// Set active animation
	public void SetAnimation(String AnimationName)
	{
		// Retain name
		AnimationName = AnimationName.ToLower();
		
		// Ignore if this animation is already set
		if(ActiveAnimation != null && ActiveAnimation.Name == AnimationName)
			return;
		
		// Else, load animation
		SpriteAnimation Animation = null;
		if(Animations.TryGetValue(AnimationName, out Animation))
		{
			ActiveAnimation = Animation;
			
			// Note: We must set the animation to either the old or new method based on individual pos / frame count
			SetAnimation(ActiveAnimation.Pos, ActiveAnimation.Frame, ActiveAnimation.FrameCount, ActiveAnimation.FrameTime);
		}
	}
	
	// Overload the update function to allow rotation
	public override void Update(float dT)
	{
		if(ActiveAnimation != null)
			SetRotation(GetRotation() + ActiveAnimation.Rotation * dT);
		base.Update(dT);
	}
	
	// Returns the rate of rotation
	public float GetRotationRate()
	{
		if(ActiveAnimation != null)
			return ActiveAnimation.Rotation;
		else
			return 0.0f;
	}
	
	// Overload the reload function from the parent Sprite class
	public override void Reload()
	{
		// Call the parent's reload, then reload the config file explicitly
		base.Reload();
		
		// Reload our config
		LoadConfig(FileName);
		
		// Reload the animation
		if(ActiveAnimation != null)
			SetAnimation(ActiveAnimation.Name);
	}
}
