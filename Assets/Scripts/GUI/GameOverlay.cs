/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: GameOverlay.cs
 Desc: The main game's overlay.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;

// Message type
public class GameOverlay_Message
{
	public Color MessageColor;	// Message color
	public float TotalTime;		// Total time since message
	public String Message = "";	// Message to show
}

// Game overlay class
public class GameOverlay : MonoBehaviour
{
	/*** Private Internals ***/
	
	// Names on-screen for the game overlay
	private String[][] OverlayNames = new String[][]
	{
		new String[] {"Command Center", "Power Node", "Mining Platform"},
		new String[] {"Assault Shipyard", "Destroyer Shipyard", "Carrier Shipyard"},
		new String[] {"Turret", "Missiles"}
	};
	
	// Associated overlay icon names
	private String[][] OverlayIconNames = new String[][]
	{
		new String[] {"CommandCenterIcon", "PowerNodeIcon", "MiningPlatformIcon"},
		new String[] {"AssaultShipyardIcon", "DestroyerShipyardIcon", "CarrierShipyardIcon"},
		new String[] {"TurretIcon", "MissilesIcon"}
	};
	
	// Total time ellapsed
	private float TotalTime = 0.0f;
	
	// White texture used for a dozen things
	private Texture WhiteTexture;
	
	// Size of mini-map
	private const int MinimapWidth = 350;
	private const int MinimapHeight = 300;
	
	// Old mouse drag
	private Vector3 OldMouseDrag;
	private bool MouseDragging = false;
	
	// Queue of all the messages; max 10 messages in queue
	public Queue<GameOverlay_Message> MessageQueue;
	
	/*** Script Functions ***/
	
	// Constructor
	public GameOverlay()
	{
		// Alloc our message queue
		MessageQueue = new Queue<GameOverlay_Message>();
		
		// Load white texture
		WhiteTexture = Resources.Load("Textures/WhiteBlock") as Texture;
	}
	
	// Update is called once per frame
	void Update()
	{
		// Get delta
		float dT = Time.deltaTime;
		
		// Apply the play-speed factor
		if(Globals.WorldView.PlaySpeed == GameSpeed.Fast)
			dT *= 2;
		else if(Globals.WorldView.PlaySpeed == GameSpeed.Faster)
			dT *= 4;
		TotalTime += dT;
		
		// Pause check
		if(Input.GetKeyDown(KeyCode.Escape) && Globals.WorldView.Paused == false)
		{
			// Flip pause state
			Globals.WorldView.Paused = true;
			
			// Push pause menu
			PauseMenu PauseView = gameObject.AddComponent(typeof(PauseMenu)) as PauseMenu;
			Globals.PushView(PauseView);
			return;
		}
		
		/*** Messages Check ***/
		
		// Update each object's time
		foreach(GameOverlay_Message Message in MessageQueue)
		{
			Message.TotalTime += Time.deltaTime;
			if(Message.MessageColor.a > 0.0f && Message.TotalTime > 2.0f)
				Message.MessageColor.a -= Time.deltaTime / 10.0f;
		}
		
		/*** Movement Updates ***/
		
		// Translate camera as needed
		Vector2 CameraTranslation = new Vector2();
		
		// Check for dragging?
		if(Input.GetMouseButton(0))
		{
			// Actual drag
			if(MouseDragging)
			{
				// Get screen delta from last position
				Vector3 NewMouseDrag = Camera.main.ScreenToViewportPoint(Input.mousePosition);
				Vector3 ScreenDelta = NewMouseDrag - OldMouseDrag;
				OldMouseDrag = NewMouseDrag;
				
				// Translate screen space
				CameraTranslation.x -= ScreenDelta.x * Globals.MouseDragSensitivity;
				CameraTranslation.y -= ScreenDelta.y * Globals.MouseDragSensitivity;
			}
			else
			{
				OldMouseDrag = Camera.main.ScreenToViewportPoint(Input.mousePosition);
				MouseDragging = true;
			}
		}
		// Else, if release, turn off mouse dragging
		else
			MouseDragging = false;
		
		// Update the camera based on WASD / QE for movement and zooming
		if(Input.GetKey(KeyCode.Q) || Input.GetAxis("Mouse ScrollWheel") < 0)
			Camera.main.orthographicSize += Globals.CameraRate;
		if(Input.GetKey(KeyCode.E) || Input.GetAxis("Mouse ScrollWheel") > 0)
			Camera.main.orthographicSize -= Globals.CameraRate;
		
		// If we are using a touch screen, check for zoom as well
		// Todo: check on device
		if(Input.touchCount >= 2)
		{
			// Compute pinch distance
			Touch T0 = Input.GetTouch(0);
			Touch T1 = Input.GetTouch(1);
			
			// Compute distance between fingers and scale it out to camera view-volume
			float Distance = ((T0.position - T1.position).magnitude - 16.0f) * 3.0f;
			Camera.main.orthographicSize = Globals.MaxCameraSize - Distance;
		}
		
		// Bounds the ortho size as needed (for zoom)
		Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, Globals.MinCameraSize, Globals.MaxCameraSize);
		
		if(Input.GetKey(KeyCode.A))
			CameraTranslation.x -= Globals.CameraSpeed * dT;
		if(Input.GetKey(KeyCode.D))
			CameraTranslation.x += Globals.CameraSpeed * dT;
		if(Input.GetKey(KeyCode.W))
			CameraTranslation.y += Globals.CameraSpeed * dT;
		if(Input.GetKey(KeyCode.S))
			CameraTranslation.y -= Globals.CameraSpeed * dT;
		
		// Bounds check: only update if our target position will be within the world bounds
		Vector3 CamPos = Camera.main.transform.position + new Vector3(CameraTranslation.x, CameraTranslation.y, 0);
		if(CamPos.x > -WorldManager.WorldWidth && CamPos.x < WorldManager.WorldWidth && CamPos.y < WorldManager.WorldWidth && CamPos.y > -WorldManager.WorldWidth)
			Camera.main.transform.position += new Vector3(CameraTranslation.x, CameraTranslation.y, 0);
	}
	
	// Update the GUI, handling events
	void OnGUI()
	{
		// Show total time
		int Minutes = ((int)TotalTime) / 60;
		int Seconds = ((int)TotalTime) % 60;
		GUI.Label(new Rect(10, 30, 100, 30), String.Format("Total time: {0}:{1:D2}", Minutes, Seconds));
		
		// Set default skin
		GUI.skin = Globals.MainSkin;
		
		// Quarter of screen width
		int QuartWidth = Screen.width / 4;
		
		// Top heights
		const int ButtonHeight = 30;
		
		/*** Top Components ***/
		GUILayout.BeginArea (new Rect(0, -10, Screen.width, 40));
			MainMenu.AddSpikes(Screen.width);
		GUILayout.EndArea();
		
		// Friendly ship count
		int Miners = 0, Attackers = 0, Destroyers = 0, Carriers = 0;
		Globals.WorldView.ShipManager.GetFriendlies(ref Miners, ref Attackers, ref Destroyers, ref Carriers);
		GUI.Label(new Rect(1 * QuartWidth, 0, QuartWidth, ButtonHeight), String.Format("{0} Fighters, {1} Destroyers, {2} Carriers", Attackers, Destroyers, Carriers));
		
		// Mineral count
		GUI.Label(new Rect(0 * QuartWidth, 0, QuartWidth, ButtonHeight), "Minerals: " + Globals.WorldView.ResManager.TotalResources.ToString() + "; " + Miners + " Miners");
		
		// Enemy incoming info
		int NextSpawnTime = (int)Globals.WorldView.GetNextEnemySpawnTime();
		int NextEnemyCount = Globals.WorldView.GetNextEnemySpawnCount();
		String StringInfo = String.Format("Incoming: {0} enemies, in T-minus {1}:{2}", NextEnemyCount, NextSpawnTime / 60, (NextSpawnTime % 60).ToString("D2"));
		if(NextSpawnTime >= 0)
			GUI.Label(new Rect(2 * QuartWidth, 0, QuartWidth, ButtonHeight), StringInfo);
		else
			GUI.Label(new Rect(2 * QuartWidth, 0, QuartWidth, ButtonHeight), "No incoming enemies");
		
		// Enemy combatants
		Globals.WorldView.ShipManager.GetEnemies(ref Attackers, ref Destroyers, ref Carriers);
		GUI.Label(new Rect(3 * QuartWidth, 0, QuartWidth, ButtonHeight), String.Format("{0} Fighters, {1} Destroyers, {2} Carriers", Attackers, Destroyers, Carriers));
		
		// Put top bar
		MainMenu.FancyTop(Screen.width);
		
		/*** Bottom Components ***/
		
		// Draw the building controller, which bleds off down right-screen
		GUI.Window(Globals.WinID_ButtonsMenu, new Rect(Screen.width + 8 - MinimapWidth, Screen.height - MinimapHeight + 45, MinimapWidth, MinimapHeight), ControllerUpdate, "");
		
		/*** Mini-map ***/
		
		// Draw a window that bleeds off down off-screen
		GUI.Window(Globals.WinID_Minimap, new Rect(-8, Screen.height - MinimapHeight + 45, MinimapWidth, MinimapHeight), MinimapUpdate, "");
		
		/*** Pause & Play Buttons ***/
		
		// Draw the pause and play button pair
		DrawPlayPauseControl();
		
		/*** Message Log ***/
		
		// Turn off old UI
		GUI.skin = null;
		
		// Draw each message from the last to top
		int TextHeight = 18;
		int ScreenHeight = 0;
		
		// Need to go reverse, go from the latest enqued
		for(int i = MessageQueue.Count - 1; i >= 0; i--)
		{
			GameOverlay_Message Message = MessageQueue.ToArray()[i];
			Rect TextArea = new Rect(MinimapWidth + 5, Screen.height - 5 - TextHeight - ScreenHeight, Screen.width - 2 * MinimapWidth - 2 * 5, TextHeight);
			
			GUI.color = new Color(0, 0, 0, Message.MessageColor.a);
			GUI.DrawTexture(TextArea, WhiteTexture);
			
			GUI.color = Message.MessageColor;
			GUI.Label(TextArea, Message.Message);
			
			ScreenHeight += TextHeight + 2;
		}
		
		/*** On-Screen Entity Info. ***/
		
		// Draw ship info
		DrawShipInfo();
		
		//Draw building info
		DrawBuildingInfo();
	}
	
	// Update the minimap
	void MinimapUpdate(int WindowID)
	{
		// True minimap size
		float MapWidth = MinimapWidth * 0.8f;
		float MapHeight = MinimapHeight * 0.6f;
		float dX = (MinimapWidth - MapWidth) / 2.0f;
		float dY = (MinimapHeight - MapHeight) / 2.0f + 10;
		float WorldWidth = (float)WorldManager.WorldWidth * 1.25f; // Includes 25% of borders to see incoming ships
		
		// Create a list of friendly & enemy points
		List<Vector2> EnemyPos = new List<Vector2>();
		List<Vector2> FriendlyPos = new List<Vector2>();
		foreach(BaseShip Ship in Globals.WorldView.ShipManager.ShipsList)
		{
			Vector2 Pos = Ship.GetPosition();
			Pos.x = ((Pos.x + WorldWidth) / (2.0f * WorldWidth)) * MapWidth;
			Pos.y = ((Pos.y + WorldWidth) / (2.0f * WorldWidth)) * MapHeight;
			
			if(Pos.x > 0 && Pos.x < MapWidth && Pos.y > 0 && Pos.y < MapHeight)
			{
				if(Ship is EnemyShip)
					EnemyPos.Add(Pos);
				else
					FriendlyPos.Add(Pos);
			}
		}
		
		// Create a list of all buildings
		List<Vector2> BuildingPos = new List<Vector2>();
		foreach(BaseBuilding Building in Globals.WorldView.BuildingManager.WorldBuildings)
		{
			Vector2 Pos = Building.Position;
			Pos.x = ((Pos.x + WorldWidth) / (2.0f * WorldWidth)) * MapWidth;
			Pos.y = ((Pos.y + WorldWidth) / (2.0f * WorldWidth)) * MapHeight;
			
			if(Pos.x > 0 && Pos.x < MapWidth && Pos.y > 0 && Pos.y < MapHeight)
				BuildingPos.Add(Pos);
		}
		
		// Get minimap camera position
		Vector2 MinimapCamPos = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.y);
		MinimapCamPos.x = ((MinimapCamPos.x + WorldWidth) / (2.0f * WorldWidth)) * MapWidth;
		MinimapCamPos.y = ((MinimapCamPos.y + WorldWidth) / (2.0f * WorldWidth)) * MapHeight;
		
		// Draw world background
		GUI.color = new Color(0, 0, 0, 0.4f);
		GUI.DrawTexture(new Rect(dX, dY, MapWidth, MapHeight), WhiteTexture);
		
		// Draw each group
		GUI.color = Color.red;
		foreach(Vector2 Pos in EnemyPos)
			GUI.DrawTexture(new Rect(dX + Pos.x, dY + MapHeight - Pos.y, 2, 2), WhiteTexture);
		GUI.color = Color.green;
		foreach(Vector2 Pos in FriendlyPos)
			GUI.DrawTexture(new Rect(dX + Pos.x, dY + MapHeight - Pos.y, 2, 2), WhiteTexture);
		GUI.color = Color.white;
		foreach(Vector2 Pos in BuildingPos)
			GUI.DrawTexture(new Rect(dX + Pos.x, dY + MapHeight - Pos.y, 2, 2), WhiteTexture);
		
		// Draw camera
		float MinimapCamSize = Camera.main.orthographicSize / 20.0f;
		GUI.color = new Color(0, 0, 0, 0.4f);
		GUI.DrawTexture(new Rect(dX + MinimapCamPos.x - MinimapCamSize / 2, dY + MapHeight - MinimapCamPos.y - MinimapCamSize / 2, MinimapCamSize, MinimapCamSize), WhiteTexture);
	}
	
	// Draw controller buttons
	void ControllerUpdate(int WindowID)
	{
		// Button size
		const int ButtonWidth = MinimapWidth / 4;
		const int ButtonHeight = MinimapHeight / 6;
		
		GUILayout.BeginVertical();
		GUILayout.Space(40);
		for(int y = 0; y < OverlayNames.Length; y++)
		{
			GUILayout.BeginHorizontal();
			for(int x = 0; x < OverlayNames[y].Length; x++)
			{
				// Content: OverlayNames[y][x] <-- name
				GUIContent Content = new GUIContent(Resources.Load("Textures/" + OverlayIconNames[y][x]) as Texture);
				
				// Render button, change state as needed
				if(GUILayout.Button(Content, GUILayout.MinWidth(ButtonWidth), GUILayout.MaxWidth(ButtonWidth), GUILayout.MinHeight(ButtonHeight), GUILayout.MaxHeight(ButtonHeight)))
				{
					// Todo: testing this
					Globals.WorldView.CurrBuildingConfig = OverlayNames[y][x].Replace(" ", "") + "Config";
					Globals.WorldView.PlacingBuilding = true;
				}
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
	}
	
	// Draw ship information
	private void DrawShipInfo()
	{
		// Zoom factor, based on camera
		float DistanceFactor = 1.0f - 3.0f * (Camera.main.orthographicSize - Globals.MinCameraSize) / (Globals.MaxCameraSize - Globals.MinCameraSize);
		
		// Ignore if alpha is too low
		if(DistanceFactor < 0.01f)
			return;
		
		// UI width and height
		const float UIWidth = 150;
		const float UIBarWidth = UIWidth - 10;
		const float UIHeight = 16;
		
		// For each ship
		foreach(BaseShip Ship in Globals.WorldView.ShipManager.ShipsList)
		{
			// Positional information
			Vector2 ShipPosition = Ship.GetPosition();
			
			// Grow top distance based on rotation
			ShipPosition.y += Mathf.Abs(Mathf.Sin(Ship.GetRotation()) * Ship.GetHullSprite().GetGeometrySize().x * 0.5f);
			ShipPosition.y += Mathf.Abs(Mathf.Cos(Ship.GetRotation()) * Ship.GetHullSprite().GetGeometrySize().y * 0.5f);
			
			// Convert from world to screen coordinates
			Vector3 ScreenPos = Camera.main.WorldToScreenPoint(new Vector3(ShipPosition.x, ShipPosition.y, 0));
			ScreenPos.y = Screen.height - ScreenPos.y;
			
			// Ignore if out of bounds
			if(ScreenPos.x < 0 || ScreenPos.x > Screen.width)
				continue;
			
			// Set the background
			Rect BackgroundPos = new Rect(ScreenPos.x - UIWidth / 2, ScreenPos.y - UIHeight * 2, UIWidth, UIHeight * 3);
			GUI.color = new Color(0.1f, 0.1f, 0.1f, DistanceFactor);
			GUI.DrawTexture(BackgroundPos, WhiteTexture);
			
			// Set color & label name
			GUIStyle Style = GUI.skin.GetStyle("Label");
			Style.fontSize = 8;
		    Style.alignment = TextAnchor.MiddleCenter;
			
			Rect NamePos = new Rect(ScreenPos.x - UIWidth / 2, ScreenPos.y - UIHeight * 2, UIWidth, UIHeight);
			GUI.color = new Color(1, 1, 1, DistanceFactor);
			GUI.Label(NamePos, Ship.GetShipName(), Style);
			
			// Set shield bar
			float ShieldRatio = Ship.GetShieldHealth() / Ship.GetMaxShieldHealth();
			Rect ShieldPos = new Rect(ScreenPos.x - UIBarWidth / 2, ScreenPos.y - UIHeight, UIBarWidth * ShieldRatio, UIHeight * 0.8f);
			GUI.color = new Color(0.1f, 0.1f, 1.1f, DistanceFactor);
			GUI.DrawTexture(ShieldPos, WhiteTexture);
			
			ShieldPos.y -= 2;
			ShieldPos.height += 4;
			ShieldPos.width = UIBarWidth;
			GUI.color = new Color(0.8f, 0.8f, 0.8f, DistanceFactor);
			GUI.Label(ShieldPos, "(" + (int)Ship.GetShieldHealth() + " / " + (int)Ship.GetMaxShieldHealth() + ")", Style);
			
			// Set health bar
			float HealthRatio = Ship.GetHullHealth() / Ship.GetMaxHullHealth();
			Rect HealthPos = new Rect(ScreenPos.x - UIBarWidth / 2, ScreenPos.y, UIBarWidth * HealthRatio, UIHeight * 0.8f);
			GUI.color = new Color(1.0f, 0.1f, 0.1f, DistanceFactor);
			GUI.DrawTexture(HealthPos, WhiteTexture);
			
			HealthPos.y -= 2;
			HealthPos.height += 4;
			HealthPos.width = UIBarWidth;
			GUI.color = new Color(0.8f, 0.8f, 0.8f, DistanceFactor);
			GUI.Label(HealthPos, "(" + (int)Ship.GetHullHealth() + " / " + (int)Ship.GetMaxHullHealth() + ")", Style);
		}
	}
	
	private void DrawBuildingInfo()
	{
		// Zoom factor, based on camera
		float DistanceFactor = 1.0f - 3.0f * (Camera.main.orthographicSize - Globals.MinCameraSize) / (Globals.MaxCameraSize - Globals.MinCameraSize);
	
		// Ignore if alpha is too low
		if(DistanceFactor < 0.01f)
			return;
		
		// UI width and height
		const float UIWidth = 150;
		const float UIBarWidth = UIWidth - 10;
		const float UIHeight = 16;
	
		// For each building
		foreach(BaseBuilding building in Globals.WorldView.BuildingManager.WorldBuildings)
		{
			// Positional Information
			Vector3 ScreenPos = Camera.main.WorldToScreenPoint(new Vector3(building.Position.x, building.Position.y, 0));
			ScreenPos.y = Screen.height - ScreenPos.y;
			
			// Ignore if out of bounds
			if(ScreenPos.x < 0 || ScreenPos.x > Screen.width)
				continue;
			
			// Set the background
			Rect BackgroundPos = new Rect(ScreenPos.x - UIWidth / 2, ScreenPos.y - UIHeight * 2, UIWidth, UIHeight * 3);
			GUI.color = new Color(0.1f, 0.1f, 0.1f, DistanceFactor);
			GUI.DrawTexture(BackgroundPos, WhiteTexture);
			
			// Set color & label name
			GUIStyle Style = GUI.skin.GetStyle("Label");
			Style.fontSize = 8;
		    Style.alignment = TextAnchor.MiddleCenter;
			
			Rect NamePos = new Rect(ScreenPos.x - UIWidth / 2, ScreenPos.y - UIHeight * 2, UIWidth, UIHeight);
			GUI.color = new Color(1, 1, 1, DistanceFactor);
			GUI.Label(NamePos, building.Name, Style);
			
			// Set building bar
			if(building is ShipyardBuilding)
			{
				ShipyardBuilding shipyard = (ShipyardBuilding)building;
				float buildRatio = (shipyard.BuildCoolDown - shipyard.CurrBuildTime) / shipyard.BuildCoolDown;
				Rect buildPos = new Rect(ScreenPos.x - UIBarWidth / 2, ScreenPos.y - UIHeight, UIBarWidth * buildRatio, UIHeight * 0.8f);
				GUI.color = new Color(0.1f, 1.1f, 0.1f, DistanceFactor);
				GUI.DrawTexture(buildPos, WhiteTexture);
			}
			
			// Set power bar
			if(building is CommandCenterBuilding)
			{
				CommandCenterBuilding commCenter = (CommandCenterBuilding)building;
				float powerRatio = commCenter.RemainingPower / commCenter.MaxPower;
				Rect powerPos = new Rect(ScreenPos.x - UIBarWidth / 2, ScreenPos.y - UIHeight, UIBarWidth * powerRatio, UIHeight * 0.8f);
				GUI.color = new Color(1.1f, 1.1f, 1.1f, DistanceFactor);
				GUI.DrawTexture(powerPos, WhiteTexture);
				
				powerPos.y -= 2;
				powerPos.height += 4;
				powerPos.width = UIBarWidth;
				GUI.color = new Color(0.0f, 0.0f, 0.0f, DistanceFactor);
				GUI.Label(powerPos, "(" + (int)commCenter.RemainingPower + " / " + (int)commCenter.MaxPower + ")", Style);
			}
			
			// Set health bar
			float HealthRatio = (float)building.Health / (float)building.MaxHealth;
			Rect HealthPos = new Rect(ScreenPos.x - UIBarWidth / 2, ScreenPos.y, UIBarWidth * HealthRatio, UIHeight * 0.8f);
			GUI.color = new Color(1.0f, 0.1f, 0.1f, DistanceFactor);
			GUI.DrawTexture(HealthPos, WhiteTexture);
			
			HealthPos.y -= 2;
			HealthPos.height += 4;
			HealthPos.width = UIBarWidth;
			GUI.color = new Color(0.8f, 0.8f, 0.8f, DistanceFactor);
			GUI.Label(HealthPos, "(" + building.Health.ToString() + " / " + building.MaxHealth.ToString() + ")", Style);
		}
	}
	
	// Draw the play / pause buttons, and listen to any events by them
	private void DrawPlayPauseControl()
	{
		// Play button, then pause is below
		GUIContent PlayIcon = new GUIContent(Resources.Load("Textures/PlayIcon" + (int)Globals.WorldView.PlaySpeed) as Texture);
		GUIContent PauseIcon = new GUIContent(Resources.Load("Textures/PauseIcon") as Texture);
		
		// Button space
		const int ButtonWidth = 64;
		const int ButtonHeight = 32;
		Rect ButtonRect = new Rect(Screen.width - ButtonWidth * 1.2f, ButtonHeight * 1, ButtonWidth, ButtonHeight);
		
		// Render then out in the corner
		bool PlayPressed = GUI.Button(ButtonRect, PlayIcon);
		ButtonRect.y += ButtonHeight + 5;
		bool PausePressed = GUI.Button(ButtonRect, PauseIcon);
		
		// If the user pushed the play button, increase the speed and loop over as needed
		if(PlayPressed)
		{
			if(Globals.WorldView.PlaySpeed == GameSpeed.Normal)
				Globals.WorldView.PlaySpeed = GameSpeed.Fast;
			else if(Globals.WorldView.PlaySpeed == GameSpeed.Fast)
				Globals.WorldView.PlaySpeed = GameSpeed.Faster;
			else if(Globals.WorldView.PlaySpeed == GameSpeed.Faster)
				Globals.WorldView.PlaySpeed = GameSpeed.Normal;
		}
		
		// If pause pressed, launch pause window
		else if(PausePressed)
		{
			// Flip pause state
			Globals.WorldView.Paused = true;
			
			// Push pause menu
			PauseMenu PauseView = gameObject.AddComponent(typeof(PauseMenu)) as PauseMenu;
			Globals.PushView(PauseView);
		}
	}
	
	// Push a new message to the global console
	public void PushMessage(Color MessageColor, String MessageText)
	{
		// Clear until we have enough messages
		while(MessageQueue.Count > 10)
			MessageQueue.Dequeue();
		
		// Allocs a message struct, and store it
		GameOverlay_Message Message = new GameOverlay_Message();
		Message.Message = "  > " + MessageText;
		Message.MessageColor = MessageColor;
		Message.TotalTime = 0.0f;
		
		MessageQueue.Enqueue(Message);
		
		// Play a notification sound
		Globals.WorldView.AManager.PlayAudio(new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.y), "Notification");
	}
}
