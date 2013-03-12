/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: FigtherShip.cs
 Desc: Implements the small-sized player-owned ship.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections;

public class FighterShip : BaseShip
{
	// Target radius, and total time
	private float TotalTime;
	private const float TargetRadius = 400.0f;
	
	// Constructor
	public FighterShip()
		: base("Config/Ships/FighterConfig")
	{
		
	}
	
	// Ship logic
	public override void Update(float dT)
	{
		// Call the base implementation first
		base.Update(dT);
		TotalTime += dT;
		
		// Closest ship
		float MinDistance = float.MaxValue;
		BaseShip TargetShip = null;
		
		// For each ship, if enemy...
		foreach(BaseShip Ship in Globals.WorldView.ShipManager.ShipsList)
		{
			// Ignore if out of world bounds
			if(!Globals.WorldView.IsWithinWorld(Ship.GetPosition()))
				continue;
			
			// Check distance
			float Distance = (Ship.GetPosition() - GetPosition()).magnitude;
			if(Ship is EnemyShip && Distance < MinDistance)
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
		// Move towards center
		else
		{
			//Debug.Log("No task, going to center!");
			MoveTowards(Vector2.zero, dT);
		}
	}
}
