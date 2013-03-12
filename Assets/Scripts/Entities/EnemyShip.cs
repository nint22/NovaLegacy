/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: EnemyShip.cs
 Desc: Enemy ship class wrapper: given a ship description config
 file, it loads all the basic ship data (BaseShip), but then
 sets special properties for the enemy ship. Only 9 enemy
 ships exist: low numbers are smaller, higher numbers are bigger.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections;

public class EnemyShip : BaseShip
{
	// Target radius, and total time
	private float TotalTime;
	private const float TargetRadius = 100.0f;
	
	// Construct a ship with the given ship index and projectiles manager
	public EnemyShip(int EnemyIndex)
		: base("Config/Enemies/EnemyConfig" + EnemyIndex)
	{
		
	}
	
	// Ship logic
	public override void Update(float dT)
	{
		// Call the base implementation first
		base.Update(dT);
		TotalTime += dT;
		
		// Closest ship (within 1k)
		float MinDistance = float.MaxValue;
		BaseShip TargetShip = null;
		
		// For each ship, if enemy...
		foreach(BaseShip Ship in Globals.WorldView.ShipManager.ShipsList)
		{
			// Check distance
			float Distance = (Ship.GetPosition() - GetPosition()).magnitude;
			if(!(Ship is EnemyShip) && Distance < MinDistance)
			{
				MinDistance = Distance;
				TargetShip = Ship;
			}
		}
		
		// Move towards target ship if any
		if(TargetShip != null)
		{
			Vector2 Offset =  new Vector2(Mathf.Cos(TotalTime * 0.1f) * TargetRadius, Mathf.Sin(TotalTime * 0.1f) * TargetRadius);
			MoveTowards(TargetShip.GetPosition() + Offset, dT);
			
			// Shoot if close enough
			if(MinDistance < 800.0f)
				FireAt(TargetShip.GetPosition());
		}
	}
}
