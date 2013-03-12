/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: EntityMenu.cs
 Desc: Show entity information for either a building or ship
 that has been selected. Pauses the game.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections;

public class EntityMenu : MonoBehaviour
{
	// Ship name & description
	private String ShipName = "";
	private String ShipType = "";
	private String Description = "";
	
	// Render GUI
	void OnGUI()
	{
		// Define UI
		GUI.skin = Globals.MainSkin;
		
		// Short-hand window sizes
		int WindowWidth = 600;
		int WindowHeight = 400;
		
		// Define window
		Rect WindowRect = new Rect(Screen.width / 2 - WindowWidth / 2, Screen.height / 2 - WindowHeight / 2, WindowWidth, WindowHeight);
		GUI.Window(Globals.WinID_EntityMenu, WindowRect, OnWindow, "");
	}
	
	// Set the ship name and description info
	public void LoadDetails(BaseShip Ship)
	{
		ShipName = Ship.GetShipName();
		if(Ship is PlayerShip)
		{
			ShipType = "Command Ship";
			Description = "Name: Tevyat\nModel: 0X47-alpha\nDeveloper(s): NauTech Industries/ Tilus Mining Corps.\nClass: Industrial- Frigate\nThe Tevyat is a prototype starship under development between NauTech Industries and Tilus Mining Corps to support and sustain large-scale mining/construction operations within deep space.\nMost industrial starcrafts are unarmed, designed specifically for mining or cargo transportation. The Tevyat, however, was designed with enhanced defensive capabilities.  Tachyonic Barriers allow the ship to survive all methods of physical assault while nano-fabricators are capable of creating large offensive structures and drones.\nOther unique features include the Nautilus, a quantum artificial intelligence, functioning as the ship’s electronic warfare defense and navigation system.  When a fabricator builds a new object, the Nautilus “forks”, creating a smaller copy of itself that will execute asynchronously upon upload, making the Tevyat the intelligence core for all ships.";
		}
		else if(Ship is MinerShip)
		{
			ShipType = "Miner Ship";
			Description = "Class: Industrial transport\nModel: M3-2721 Scavenger\nDeveloper: Tilus Mining Corps.\nDesigned for efficient hauling and material extraction, mining ships are non-combative industrial starcrafts.  Though unarmed, these ships contain large cargo holds and are capable of fitting strip mining modules, heating components and magnetic rakes all on its medium sized hull.  Mining ships cannot refine or process materials; this is left to Shipyards.\nFirst assembled in 2215 by the South African extraction company Tilus Mining Corps, the Scavenger model’s design pays homage to the malfunctioning Jupiter probe responsible for the discovery of Element 115 in 2199.  Several hull designs exist but none are more durable, making it the most commonly used mining ship in the Sol System.";
		}
		else if(Ship is FighterShip)
		{
			ShipType = "Assault Ship";
			Description = "Class: Battlecrusier\nDeveloped: NauTech Industries/Core Software Solutions.\nModel:  Aurora Heavy Assault\nDesigned by NauTech Industries, the Aurora Heavy Assault model is lightly armored and extremely fast, well-suited for “hit and run” offensive strikes.  It has been optimized for reconnaissance and scouting, equipped with cutting-edge stealth technology  provided by Core Software Solutions.   It is unclear who originally commissioned the ship, however after the specs were stolen, the ship became common for military use.";
		}
		else if(Ship is DestroyerShip)
		{
			ShipType = "Destroyer Ship";
			Description = "Class: Industrial/Destroyer\nDeveloper: Haeshin Group\nModel: CTX- Nightshade\nOriginally created for urban development in the City of Seoul, the CTX-Nightshade induced an earthquake of catastrophic proportions while attempting to construct the world’s tallest building.  The craft, forever known as Destroyer, was re-engineered to fall between the Assault and Carrier class, possessing sturdy Tachyonc Barriers and advanced targeting arrays to add balance in combat.";
		}
		else if(Ship is CarrierShip)
		{
			ShipType = "Carrier Ship";
			Description = "Class: Carrier\nDeveloper: The North American Union (NAU)\nModel:  Dreadnought HMS-VI\nRepresenting the apex of NAU Military, Carriers are combat behemoths ranging between 750 meters to 1.5 kilometers in length. What Carriers lack in speed they make up for in devastating firepower.  With six Gatling guns, three slow-reload railguns and a drone bay launcher capable of deploying up to 25 high-speed fighter drones, these massive ships are invaluable to any fleet.\nLaunched to obtain NAU Mining Territory, the Dreadnought HMS-VI encountered the Juiz, a Destroyer under Brazilian Command.   As the groups began to fight, shots landed on Earth wiping out parts of South America and half of the US.  And so began the Great War.";
		}
		else
		{
			ShipType = Description = "UNDEFINED";
		}
	}
	
	// Update the window
	private void OnWindow(int WindowID) 
	{
		// Back button pressed
		bool BackPressed = false;
		
		// Begin vertical content
		GUILayout.Space(8);
		GUILayout.BeginVertical();
		{
			GUILayout.Label("", "Divider");
			GUILayout.Label("Ship Description: " + ShipName + "(" + ShipType + ")");
			GUILayout.TextArea(Description);
			GUILayout.Label("", "Divider");
			
			// Yes / no buttons
			GUILayout.BeginHorizontal();
			BackPressed = GUILayout.Button("Back");
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
		
		// Pop off if pressed
		if(BackPressed)
		{
			Globals.WorldView.Paused = false;
			Globals.PopView();
		}
	}
}
