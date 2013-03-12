/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: WorldManager.cs
 Desc: Is a game world manager: contains all bullets, scenery,
 resources, ships, etc. This is almost like a "root" scene manager
 for all major game objects.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class WorldManager : MonoBehaviour
{
	/*** Public World Manager Structs ***/
	
	// Size (bounds) of the world. World origin is in the middle (0, 0) with width and height each 2 * WorldWidth
	public const float WorldWidth = 1000.0f;
	
	// The game instance's sprite group
	public SpriteManager SManager;
	
	// Projectiles manager
	public ProjectileManager ProjManager;
	
	// Ship list manager
	public ShipsManager ShipManager;
	
	// Scenery manager
	public SceneryManager SceneManager;
	
	// Resource manager
	public ResourceManager ResManager;
	
	// Building manager
	public BuildingManager BuildingManager;
	
	// Audio manager (little audio sound-bits)
	public AudioManager AManager;
	
	// Music manager (background music)
	public AudioSystem SongManager;
	
	// UI Overlay
	public GameOverlay OverlayView;
	
	// Special building overlay
	public GameContextMenu ContextMenuView;
	
	/*** Game Properties ***/
	
	// Current level index we have loaded
	public int TargetLevel = -1;
	
	// Global pause state
	public bool Paused = false;
	
	// Game speed state
	public GameSpeed PlaySpeed = GameSpeed.Normal;
	
	/*** Internals ***/
	
	// Total time ellapsed
	private float TotalTime;
	
	// Ships, buildings, projectiles, and scenery lists
	private BaseBuilding CurrentBuilding;
	
	// The three layers and the dedicated sprite manager bound
	// to the camera: this is done to prevent stuttering
	private Sprite[] BackgroundSprites;
	
	// Our internal level generation / parser
	private bool LevelLoaded;
	private LevelManager LevelGen;
	
	// Spawn generation queue (if empty and no enemnies on-screen, player wins!)
	private Queue<LevelManager_SpawnGroup> EnemyGroups;
	
	// Win state
	private LevelManager_WinningState WinLogic = null;
	
	// Ship we clicked on (for UI management)
	private BaseShip TargetShip = null;
	private BaseBuilding TargetBuilding = null;
	
	// Building placement state
	public string CurrBuildingConfig = "";
	public bool PlacingBuilding = false;
	
	// Total number of background layers
	private const int BackgroundLayerCount = 6;
	
	/*** Script Methods ***/
	
	// Use this for initialization
	void Start()
	{
		/*** Init. Game System ***/
		
		// Default camera zoom
		Camera.main.orthographicSize = Globals.MinCameraSize + (Globals.MaxCameraSize - Globals.MinCameraSize) / 2.0f;
		
		// Instantiate all game-level objects
		SManager = gameObject.AddComponent(typeof(SpriteManager)) as SpriteManager;
		ProjManager = gameObject.AddComponent(typeof(ProjectileManager)) as ProjectileManager;
		ShipManager = gameObject.AddComponent(typeof(ShipsManager)) as ShipsManager;
		SceneManager = gameObject.AddComponent(typeof(SceneryManager)) as SceneryManager;
		ResManager = gameObject.AddComponent(typeof(ResourceManager)) as ResourceManager;
		BuildingManager = gameObject.AddComponent(typeof(BuildingManager)) as BuildingManager;
		AManager = gameObject.AddComponent(typeof(AudioManager)) as AudioManager;
		SongManager = gameObject.AddComponent(typeof(AudioSystem)) as AudioSystem;
		OverlayView = gameObject.AddComponent(typeof(GameOverlay)) as GameOverlay;
		ContextMenuView = gameObject.AddComponent(typeof(GameContextMenu)) as GameContextMenu;
		
		/*** Background System ***/
		
		// Allocate the background sprites
		BackgroundSprites = new Sprite[BackgroundLayerCount];
		for(int i = 0; i < BackgroundLayerCount; i++)
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
		
		/*** Add Scene Elements & Load Enemey Spawn List ***/
		
		// Load the level gen. (note that we finish loading in the "OnLevelWasLoaded" because
		// of a Unity3D-specific design issue
		LevelLoaded = false;
		LevelGen = new LevelManager(TargetLevel);
		
		/*** TESTING: Add temp ships ***/
		
		// Create a few ships destroyers bottom right
		for(int i = 0; i < 6; i++)
		{
			int x = UnityEngine.Random.Range(-500, 500);
			int y = UnityEngine.Random.Range(-500, 500);
			
			BaseShip Friendly = null;
			if(i == 0)
				Friendly = new CarrierShip();
			else if(i == 1)
				Friendly = new DestroyerShip();
			else
				Friendly = new FighterShip();
			Friendly.SetPos(new Vector2(x, y));
			ShipManager.ShipsList.Add(Friendly);
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		// If not yet loaded, it should be good to go..
		if(!LevelLoaded)
			OnLevelWasLoaded();
		
		// Do nothing if paused or not yet loaded
		if(Paused)
			return;
		
		// DeltaTime
		float dT = Time.deltaTime;
		
		// Apply the play-speed factor
		if(Globals.WorldView.PlaySpeed == GameSpeed.Fast)
			dT *= 2;
		else if(Globals.WorldView.PlaySpeed == GameSpeed.Faster)
			dT *= 4;
		
		TotalTime += dT;
		
		/*** Background Update ***/
		
		// Update background UV offsets
		Vector2 CameraPos = new Vector2(Camera.main.transform.position.x, -Camera.main.transform.position.y);
		for(int i = 0; i < 3; i++)
		{
			// Update UV pos
			BackgroundSprites[i].SetSpritePos(CameraPos * (float)(i + 1) * 0.2f + new Vector2(TotalTime, TotalTime) * 0.5f);
			
			// Update on-screen position (just follow camera)
			BackgroundSprites[i].SetPosition(-BackgroundSprites[i].GetGeometrySize() / 2.0f + new Vector2(CameraPos.x, -CameraPos.y));
		}
		
		/*** Update Enemy Ship Spawning ***/
		
		UpdateEnemySpawn();
		
		/*** Update Buildings ***/

		BuildingManager.Update();
			
		/*** Mouse Events ***/
		
		// Start of down press
		if(Input.GetMouseButtonDown(0))
		{
			// Get the ship we clicked on
			Vector2 WorldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			TargetShip = ShipManager.GetShipAt(WorldMousePos);
			TargetBuilding = BuildingManager.GetBuildingAt(WorldMousePos);
		}
		
		// If the user does a full click on-screen...
		else if(Input.GetMouseButtonUp(0))
		{
			// Get the ship we released on, if any
			Vector2 WorldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			
			// Find ship and building
			BaseShip ReleaseShip = ShipManager.GetShipAt(WorldMousePos);
			BaseBuilding ReleaseBuilding = BuildingManager.GetBuildingAt(WorldMousePos);
			
			// If the selected ships match, then we should show the UI
			if(TargetShip != null && TargetShip == ReleaseShip)
			{
				Paused = true;
				EntityMenu EntityView = gameObject.AddComponent(typeof(EntityMenu)) as EntityMenu;
				EntityView.LoadDetails(TargetShip);
				Globals.PushView(EntityView);
			}
			
			// If building..
			else if(TargetBuilding != null && TargetBuilding == ReleaseBuilding)
			{
				ContextMenuView.Target = TargetBuilding;
				ContextMenuView.enabled = true;
			}
			
			// Else, placing a building...
			else if(CurrentBuilding != null)
			{
				bool buildingAdded = BuildingManager.AddBuilding(CurrentBuilding);
				
				if(buildingAdded)
				{
					CurrentBuilding = null;
					PlacingBuilding = false;
				}
			}
			
			// Reset building UI
			else
			{
				ContextMenuView.enabled = false;
			}
			
			// Always reset the ship once selected or not
			TargetShip = null;
		}
		
		// Else if right-click mouse up, release building
		else if(Input.GetMouseButtonUp(1) && CurrentBuilding != null)
		{
			BuildingManager.DestroyBuilding(CurrentBuilding);
			CurrentBuilding = null;
			PlacingBuilding = false;
		}

		/*** Music Update ***/
		
		// Get total enemy ship count
		int EnemyCount = 0;
		foreach(BaseShip Ship in ShipManager.ShipsList)
		{
			if(Ship is EnemyShip)
				EnemyCount++;
		}
		
		// If enemies are gone, transition back to normal music
		if(EnemyCount <= 0)
			SongManager.TransitionAudio(false);
		
		/*** Building Placement ***/
		
		if(PlacingBuilding && CurrentBuilding == null)
			CurrentBuilding = BuildingManager.CreateBuilding("Config/Buildings/" + CurrBuildingConfig);
		
		if(CurrentBuilding != null)
			CurrentBuilding.Position = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
		
		/*** Win/Lose Check ***/
		
		// Get player count: if no ships and no buildings left, we loose
		int Miners = 0, Attackers = 0, Destroyers = 0, Carriers = 0;
		Globals.WorldView.ShipManager.GetFriendlies(ref Miners, ref Attackers, ref Destroyers, ref Carriers);
		
		// Lose check:
		if(Miners == 0 && Attackers == 0 && Destroyers == 0 && Carriers == 0 && BuildingManager.WorldBuildings.Count == 0)
		{
			// Flip on pause state
			Paused = true;
			
			// Push pause menu
			LoseMenu LoseView = gameObject.AddComponent(typeof(LoseMenu)) as LoseMenu;
			LoseView.SetLevel(TargetLevel);
			Globals.PushView(LoseView);
			
			// Unlock the next level (if any)
			LevelManager.SetLevelUnlocked(TargetLevel + 1);
		}
		
		// Win check:
		else
		{
			// Has the player won?
			bool PlayerWon = false;
			
			// 1. Check for time
			if(WinLogic.IfWinTime && (int)TotalTime > WinLogic.WinTime)
				PlayerWon = true;
			
			// 2. Check for enemy kills
			if(WinLogic.IfWinKillAll)
			{
				// Get enemy count and make sure it is the last group
				Globals.WorldView.ShipManager.GetEnemies(ref Attackers, ref Destroyers, ref Carriers);
				if(GetNextEnemySpawnTime() < 0.0f && Attackers == 0 && Destroyers == 0 && Carriers == 0)
					PlayerWon = true;
			}
			
			// 3. Check for resource consumption
			if(WinLogic.IfWinResources && SceneManager.AllResourcesConsumed())
				PlayerWon = true;
			
			// Player win check
			if(PlayerWon)
			{
				// Flip on pause state
				Paused = true;
				
				// Push pause menu
				WinMenu WinView = gameObject.AddComponent(typeof(WinMenu)) as WinMenu;
				WinView.SetLevel(TargetLevel);
				Globals.PushView(WinView);
			}
		}
	}
	
	/*** GameOverlay Accessors ***/
	
	// Returns fractions of seconds until next enemy spawn,
	// else returns a negative value if there are no enemies left
	public float GetNextEnemySpawnTime()
	{
		if(!LevelLoaded)
			return -1;
		else if(EnemyGroups.Count <= 0)
			return -1.0f;
		else
			return EnemyGroups.Peek().SpawnTime - TotalTime;
	}
	
	// Returns the total number of enemies in the next spawn group
	public int GetNextEnemySpawnCount()
	{
		if(!LevelLoaded)
			return 0;
		else if(EnemyGroups.Count <= 0)
			return 0;
		else
		{
			LevelManager_SpawnGroup Group = EnemyGroups.Peek();
			return Group.Class0Count + Group.Class1Count + Group.Class2Count;
		}
	}
	
	/*** Internals ***/
	
	// Check if we should spawn any enemy groups yet
	private void UpdateEnemySpawn()
	{
		// Ignore if no spawning groups
		if(EnemyGroups.Count <= 0)
			return;
		
		// Peek at front of list: do we spawn these guys yet?
		if(TotalTime > EnemyGroups.Peek().SpawnTime)
		{
			// Spawn the ships info
			LevelManager_SpawnGroup Group = EnemyGroups.Dequeue();
			
			// Declare message
			OverlayView.PushMessage(Color.yellow, TextEvents.GetIncomingMessage());
			
			// Get spawn pos
			for(int i = 0; i < Group.Class0Count; i++)
			{
				// Randomize offset
				Vector2 Offset = new Vector2(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100));
				
				EnemyShip Enemy = new EnemyShip(0);
				Enemy.SetPos(Group.SpawnPos + Offset);
				ShipManager.ShipsList.Add(Enemy);
			}
			
			for(int i = 0; i < Group.Class1Count; i++)
			{
				// Randomize offset
				Vector2 Offset = new Vector2(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100));
				
				EnemyShip Enemy = new EnemyShip(1);
				Enemy.SetPos(Group.SpawnPos + Offset);
				ShipManager.ShipsList.Add(Enemy);
			}
			
			for(int i = 0; i < Group.Class2Count; i++)
			{
				// Randomize offset
				Vector2 Offset = new Vector2(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100));
				
				EnemyShip Enemy = new EnemyShip(2);
				Enemy.SetPos(Group.SpawnPos + Offset);
				ShipManager.ShipsList.Add(Enemy);
			}
			
			// Declare what's comming
			OverlayView.PushMessage(Color.yellow, String.Format("{0} Fighters, {1} Destroyers, {2} Carriers", Group.Class0Count, Group.Class1Count, Group.Class2Count));
			
			// Change music to combat
			SongManager.TransitionAudio(true);
		}
	}
	
	// Overloaded behavior required to load a scene (through LevelManager) correctly
	void OnLevelWasLoaded()
	{
		// Finish loading the level generation stuff
		LevelGen.ParseScene();
		LevelManager_Scenery[] SceneryElements = LevelGen.GetScenery();
		
		// All all content from level generator
		for(int i = 0; i < SceneryElements.Length; i++)
			if(SceneryElements[i].IsMineral)
				SceneManager.AddMineral(SceneryElements[i].Pos, SceneryElements[i].Radius, SceneryElements[i].MineralCount);
		
		// Load the queue of enemy spawn timings and groups
		EnemyGroups = new Queue<LevelManager_SpawnGroup>(LevelGen.GetSpawnList());
		
		// Load the win state logic
		WinLogic = LevelGen.GetWinLogic();
		
		// All loaded!
		LevelLoaded = true;
	}
	
	// Returns true if the given 2D position is within the world space
	public bool IsWithinWorld(Vector2 Pos)
	{
		return (Pos.x < WorldWidth && Pos.x > -WorldWidth && Pos.y < WorldWidth && Pos.y > -WorldWidth);
	}
	
	// Overload destructor to help with resource release
	void OnDestroy()
	{
		// Explicitly release the level scene data
		UnityEngine.Object[] LevelEntities = UnityEngine.Object.FindSceneObjectsOfType(typeof(LevelEntity));
		foreach(UnityEngine.Object _Entity in LevelEntities)
		{
			LevelEntity Entity = _Entity as LevelEntity;
			Destroy(Entity.gameObject);
		}
		
		// Release all owned object
		Destroy(SManager);
		Destroy(ProjManager);
		Destroy(ShipManager);
		Destroy(SceneManager);
		Destroy(ResManager);
		Destroy(BuildingManager);
		Destroy(AManager);
		Destroy(SongManager);
		Destroy(OverlayView);
		Destroy (ContextMenuView);
	}
}
