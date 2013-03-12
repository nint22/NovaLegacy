/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: ShipManager.cs
 Desc: Manager of all ships, wrapping updates, counting, etc.
 Gives the owner direct access to ShipsList.
 
***************************************************************/

using UnityEngine;
using System.Collections.Generic;

public class ShipsManager : MonoBehaviour
{
	// Total list of ships
	public List<BaseShip> ShipsList = new List<BaseShip>();
	
	// Update is called once per frame
	void Update()
	{
		// DeltaTime
		float dT = Time.deltaTime;
		
		// Apply the play-speed factor
		if(Globals.WorldView.PlaySpeed == GameSpeed.Fast)
			dT *= 2;
		else if(Globals.WorldView.PlaySpeed == GameSpeed.Faster)
			dT *= 4;
		
		// Do nothing if paused
		if(Globals.WorldView.Paused)
			return;
		
		/*** Update Ships ***/
		
		// Explicitly update each ship
		foreach(BaseShip Ship in ShipsList)
			Ship.Update(dT);
		
		// Remove any destroyed ships
		for(int i = ShipsList.Count - 1; i >= 0; i--)
		{
			BaseShip Ship = ShipsList[i] as BaseShip;
			if(Ship.GetDead())
			{
				// Destroy ship
				Ship.Destroy();
				ShipsList.RemoveAt(i);
				
				// Add explosion information
				ExplodeShip(Ship);
			}
		}
	}
	
	// Add all explosion elements for the associated ship, and destroy the ship
	public void ExplodeShip(BaseShip Ship)
	{
		// Get ship pos
		Vector2 Pos = Ship.GetPosition();
		
		// Create destruction geometry / animation at the old ship position
		for(int j = 0; j < 32; j++)
		{
			float Radius = 45.0f;
			Vector2 Offset = new Vector2(UnityEngine.Random.Range(-Radius, Radius), UnityEngine.Random.Range(-Radius, Radius));
			Globals.WorldView.ProjManager.AddExplosion(Pos + Offset);
		}
		
		// Get the length of the ship
		float Facing = Ship.GetRotation();
		float Width = Ship.GetHullSprite().GetGeometrySize().x / 2.0f;
		float Height = Ship.GetHullSprite().GetGeometrySize().y / 2.0f;
		Vector2 ShipLength = new Vector2(Mathf.Cos(Facing), Mathf.Sin(Facing)) *  Width;
		Vector2 ShipSide = new Vector2(ShipLength.normalized.y, ShipLength.normalized.x);
		
		// For each chunk index
		int ChunkCount = Ship.GetChunkCount();
		for(int i = 0; i < ChunkCount * (Width / 20.0f); i++)
		{
			Vector2 Offset = ShipLength * UnityEngine.Random.Range(-1.0f, 1.0f) + ShipSide * UnityEngine.Random.Range(-Height, Height);
			float Rotation = UnityEngine.Random.Range(-Mathf.PI, Mathf.PI);
			Globals.WorldView.SceneManager.AddScenery(Ship.GetConfigFileName(), "Chunk" + (i % ChunkCount), new Vector3(Pos.x + Offset.x, Pos.y + Offset.y, 0), new Vector3(Offset.x / 100.0f, Offset.y / 100.0f, Rotation / 2.0f));
		}
	}
	
	// Return the ship the user has selcted at the given coordinate
	public BaseShip GetShipAt(Vector2 GlobalPos)
	{
		// Return first collision
		foreach(BaseShip Ship in ShipsList)
		{
			if(Ship.CheckCollision(GlobalPos))
				return Ship;
		}
		
		// Else, none!
		return null;
	}
	
	// Get all friendly ship counts
	public void GetFriendlies(ref int Miners, ref int Attackers, ref int Destroyers, ref int Carriers)
	{
		// Init all to zero
		Miners = Attackers = Destroyers = Carriers = 0;
		
		// For each ship
		foreach(BaseShip Ship in ShipsList)
		{
			if(Ship is MinerShip)
				Miners++;
			else if(Ship is FighterShip)
				Attackers++;
			else if(Ship is DestroyerShip)
				Destroyers++;
			else if(Ship is CarrierShip)
				Carriers++;
		}
		
		// Done!
	}
	
	// Get all enemy counts
	public void GetEnemies(ref int Attackers, ref int Destroyers, ref int Carriers)
	{
		// Init all to zero
		Attackers = Destroyers = Carriers = 0;
		
		// For each ship
		foreach(BaseShip Ship in ShipsList)
		{
			if(Ship is EnemyShip)
			{
				if(Ship.GetConfigFileName().EndsWith("0"))
					Attackers++;
				else if(Ship.GetConfigFileName().EndsWith("1"))
					Destroyers++;
				else if(Ship.GetConfigFileName().EndsWith("2"))
					Carriers++;
			}
		}
		
		// Done!
	}
	
	// On destruction, explicitly remove all ships (because ships might contain specially retaine data)
	void OnDestroy()
	{
		foreach(BaseShip Ship in ShipsList)
			Ship.Destroy(true);
	}
}
