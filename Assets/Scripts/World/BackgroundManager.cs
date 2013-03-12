/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: BackgroundManager.cs
 Desc: Manages the background, used for both the menu and the
 game run-time.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections;

public class BackgroundManager : MonoBehaviour
{
	// Our own custom sprite manager for the background object
	private SpriteManager SManager;
	
	// The three layers and the dedicated sprite manager bound
	// to the camera: this is done to prevent stuttering
	private Sprite[] BackgroundSprites;
	
	// Total time ellapsed since object creation
	private float TotalTime = 0.0f;
	
	// Use this for initialization
	void Start()
	{
		// Alloc our own sprite manager
		SManager = gameObject.AddComponent("SpriteManager") as SpriteManager;
		
		// Allocate all three background sprites
		BackgroundSprites = new Sprite[3];
		for(int i = 0; i < 3; i++)
		{
			// Alloc and retain
			Sprite Background = new Sprite("Textures/BGLayer" + i);
			BackgroundSprites[i] = Background;
			
			// Make the sprite geometry as big as the max camera, and center it on the camera
			BackgroundSprites[i].SetGeometrySize(new Vector2(Globals.MaxCameraSize, Globals.MaxCameraSize) * 2.5f * Camera.main.aspect);
			BackgroundSprites[i].SetPosition(-BackgroundSprites[i].GetGeometrySize() / 2.0f);
			BackgroundSprites[i].SetDepth(Globals.BackgroundDepth - i); // Each layet is closer to the camera
			
			// Register sprite
			SManager.AddSprite(Background);
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		// Add to total time
		float dT = Time.deltaTime;
		TotalTime += dT;
		
		// Update background UV offsets
		Vector2 CameraPos = new Vector2(Camera.main.transform.position.x, -Camera.main.transform.position.y);
		for(int i = 0; i < 3; i++)
		{
			// Update UV pos
			BackgroundSprites[i].SetSpritePos(CameraPos * (float)(i + 1) * 0.2f + new Vector2(TotalTime, TotalTime) * 0.5f);
			
			// Update on-screen position (just follow camera)
			BackgroundSprites[i].SetPosition(-BackgroundSprites[i].GetGeometrySize() / 2.0f + new Vector2(CameraPos.x, -CameraPos.y));
		}
		
	}
	
	// Show sprites
	void OnEnable()
	{
		Camera.main.orthographicSize = Globals.MaxCameraSize;
		for(int i = 0; i < 3 && BackgroundSprites != null; i++)
			BackgroundSprites[i].SetVisible(true);
	}
	
	// Hide  sprites
	void OnDisable()
	{
		for(int i = 0; i < 3 && BackgroundSprites != null; i++)
			BackgroundSprites[i].SetVisible(false);
	}
}
