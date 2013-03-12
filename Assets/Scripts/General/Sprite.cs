/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: Sprite.cs
 Desc: A general wrapper for a sprite, which is a 2D on-screen
 (pseudo) per-pixel representation of a texture. A sprite can have
 different states, animations, etc. and is generally *not* an
 interpolated / smoothed texture. Sprites are also oredered on
 an internal index system, and not depth. All sprites are
 managed by a "Sprites" manager class.
 
 Todo: have a dictionary of texture names and sizes, for fast
 reference
 
***************************************************************/

using UnityEngine;
using System.Collections;
using System;

public class Sprite
{
	/*** Sprite Information ***/
	
	// Texture size (in pixels) and name
	private Vector2 TextureSize;
	private String TextureName;
	
	// On-screen position, size, scale, rotation, and (sort/render) depth
	private Vector2 ScreenPos;
	private Vector2 ScreenSize;
	private float ScreenRotation; // Rotates around the "RotationCenter"
	private Vector2 RotationCenter;
	private int Depth;
	
	// Is visible (i.e. rendered)
	private bool ScreenVisible;
	
	// Sprite color
	private Color SpriteColor;
	
	// Has this sprite changed in any way?
	private bool SpriteChanged;
	
	// Is sprite fliped along the y (vertical) axis
	private bool IsFlipped;
	
	// Sprite animation / source texture information
	private Vector2 SpriteSize;	// This is pixels, and thus needs to be normalized for UV coords
	private Vector2 SpritePos;	// Same rule as SpriteSize
	private int FrameCount;
	private int FrameIndex;		// Which index, of the FrameCount, are we rendering?
	private float FrameDelay;   // How long we idle until we change frames
	private float FrameTime;	// How many (fractions of) seconds have we been on this frame
	private int AnimationCount; // How many times an animation has fully-cycled
	
	/*** Public Members ***/
	
	// Initialize a sprite: this is critical to call!
	public Sprite(String TextureName)
	{
		__Init(TextureName);
	}
	
	// Do not use, unless you immediatly call "__Init(...)"
	public Sprite()
	{
		// Do nothing...
	}
	
	// This private constructor is specificly done so that derivatives can take 
	public void __Init(String TextureName)
	{
		// Get the texture properties
		this.TextureName = TextureName;
		Texture Tex = Resources.Load(TextureName) as Texture;
		
		if(Tex == null)
			Debug.LogError("Unable to load texture, texture name: " + TextureName);
		
		TextureSize = new Vector2(Tex.width, Tex.height);
		
		// Always needs an initial update
		SpriteChanged = true;
		
		// Default all values
		ScreenPos = new Vector2();
		Depth = 0;
		ScreenSize = new Vector2(TextureSize.x, TextureSize.y);
		ScreenRotation = 0.0f;
		RotationCenter = new Vector2(0, 0); // Bottom-left
		SpriteColor = Color.white;
		ScreenVisible = true;
		
		// Default animation information
		SpriteSize = new Vector2(TextureSize.x, TextureSize.y);
		SpritePos = new Vector2(0, 0);
		FrameCount = 1;
		FrameIndex = 0;
		FrameDelay = 0;
		FrameTime = 0;
		AnimationCount = 0;
		
		// By default, leave native texture flip
		IsFlipped = false;
		
		// By default, needs to be updated into the sprite manager
		SpriteChanged = true;
	}
	
	// Update is called once per frame
	// Note: classes that overload this function MUST call it after their modifications
	public virtual void Update(float dT)
	{
		// If we have more than one frame, we should update our animation loop
		if(FrameCount > 1)
		{
			FrameTime += dT;
			if(FrameTime >= FrameDelay)
			{
				// Increase frame, go to next frame, and retain animation count
				FrameIndex = (FrameIndex + 1) % FrameCount;
				if(FrameIndex == 0)
					AnimationCount++;
				
				// We don't set to 0, since the overlap time is important to maintain
				FrameTime -= FrameDelay;
				SpriteChanged = true;
			}
		}
	}
	
	// Has this sprite changed? Once this function is called, it internally
	// settles back to a non-change state
	public bool HasChanged()
	{
		bool Changed = SpriteChanged;
		SpriteChanged = false;
		return Changed;
	}
	
	// Force-update the sprite geometry within a sprite manager
	public void Changed() { SpriteChanged = true; }
	
	/*** Accessors & Mutators For Geometry ***/
	
	// Get / set color
	public Color GetColor() { return SpriteColor; }
	public void SetColor(Color Val) { if(!SpriteColor.Equals(Val)) SpriteChanged = true; SpriteColor = Val; }
	
	// Get / set rotation
	public float GetRotation() { return ScreenRotation; }
	public void SetRotation(float Val) { if(ScreenRotation != Val) SpriteChanged = true; ScreenRotation = Val; }
	
	// Get / set position
	public Vector2 GetPosition() { return ScreenPos; }
	public void SetPosition(Vector2 Val) { if(!ScreenPos.Equals(Val)) SpriteChanged = true; ScreenPos = Val; }
	
	// Depth (not "true" z-depth, just the rendering order)
	public int GetDepth() { return Depth; }
	public void SetDepth(int Val) { if(Depth != Val) SpriteChanged = true; Depth = Val; }
	
	// Get / set geometry size (i.e. the size on-screen)
	public Vector2 GetGeometrySize() { return ScreenSize; }
	public void SetGeometrySize(Vector2 Val) { if(!ScreenSize.Equals(Val)) SpriteChanged = true; ScreenSize = Val; }
	
	// Get / set visability (actually removes model if invisible)
	public bool IsVisible() { return ScreenVisible; }
	public void SetVisible(bool Val) { if(ScreenVisible != Val) SpriteChanged = true; ScreenVisible = Val; }
	
	// Get / set flip
	public bool GetFlipped() { return IsFlipped; }
	public void SetFlipped(bool Val) { if(IsFlipped != Val) SpriteChanged = true; IsFlipped = Val; }
	
	// Get / set the center of rotation (default (0,0) which is bottom-left)
	public Vector2 GetRotationCenter() { return RotationCenter; }
	public void SetRotationCenter(Vector2 Val) { if(RotationCenter != Val) SpriteChanged = true; RotationCenter = Val; }
	
	/*** Accessors & Mutators For Textures ***/
	
	// Get full texture size and texture name
	public Vector2 GetTextureSize() { return TextureSize; }
	public String GetTextureName() { return TextureName; }
	
	// Get / set sprite frame size (in pixels)
	public Vector2 GetSpriteSize() { return SpriteSize; }
	public void SetSpriteSize(Vector2 Val) { if(!SpriteSize.Equals(Val)) SpriteChanged = true; SpriteSize = Val; }
	
	// Get / set sprite source position (in pixels)
	// Note: sprite position will change based on animation state (if any)
	public Vector2 GetSpritePos()
	{
		Vector2 AnimationOrigin;
		AnimationOrigin.x = (SpritePos.x + SpriteSize.x * GetFrameIndex()) % GetTextureSize().x;
		AnimationOrigin.y = SpritePos.y + SpriteSize.y * (int)((SpritePos.x + SpriteSize.x * GetFrameIndex()) / GetTextureSize().x);
		return AnimationOrigin;
	}
	public void SetSpritePos(Vector2 Val) { if(!SpritePos.Equals(Val)) SpriteChanged = true; SpritePos = Val; }
	
	// Get the current frame index
	public int GetFrameIndex() { return FrameIndex; }
	
	// Get / set animation (Must be in pixels)
	// Note: The frame pos and frame size are per-pixel values of the animation's atlas frame size
	// If none are given, no animation is expected. If some arguments were given, animations are to be set
	// through the Sprite object's members
	public void SetAnimation(Vector2 SourcePos, Vector2 FrameSize, int FrameCount, float Delay)
	{
		// Save all animation information
		SpriteSize = FrameSize;
		SpritePos = SourcePos;
		this.FrameCount = FrameCount;
		FrameDelay = Delay;
		
		// Reset for animation
		FrameIndex = 0;
		FrameTime = 0;
		AnimationCount = 0;
	}
	
	// Get the number of total animation loops that have occured
	public int GetAnimationCount() { return AnimationCount; }
	
	// Reload 
	public virtual void Reload()
	{
		// In our case, simply reload the texture size
		Texture Tex = Resources.Load(TextureName) as Texture;
		TextureSize = new Vector2(Tex.width, Tex.height);
	}
}
