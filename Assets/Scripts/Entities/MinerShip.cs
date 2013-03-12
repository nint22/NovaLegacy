/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: MinerShip.cs
 Desc: Ship used to mine minerals from anywhere on the map.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections;

public class MinerShip : BaseShip
{
	// Home base: the building that owns us
	private BaseBuilding HomeBase;
	
	// Mining properties
	private SceneryManager_Scenery TargetResource;			// What we are targeting
	private float CollectionTimer = 0.0f;	// Grows by each update
	private int CollectionRate = 1;			// Per second
	private int MaxMineralCount = 20;		// How many minerals this ship can hold
	private int MineralCount = 0;			// How many minerals this ship has
	
	// Total time ellapsed since object birth
	private float TotalTime;
	
	// Beam system (connects to rock)
	public GameObject BeamObject;
	public LineRenderer BeamLine;
	
	// Constructor needs the projectiles manager for the base class, and a home base building
	public MinerShip(BaseBuilding HomeBase)
		: base("Config/Ships/MinerConfig")
	{
		// Save given managers
		this.HomeBase = HomeBase;
		
		// Create a beam object & line
		BeamObject = new GameObject("Beam Line");
		BeamLine = BeamObject.AddComponent("LineRenderer") as LineRenderer;
		BeamLine.material = new Material(Shader.Find("Particles/Additive"));
		BeamLine.SetColors(Color.green, Color.green);
		BeamLine.SetWidth(2.0f, 2.0f);
		BeamLine.SetVertexCount(2);
		
		// Randomize time
		TotalTime = UnityEngine.Random.Range(-60.0f, 60.0f);
		
		// Warp to the base
		SetPos(HomeBase.Position);
	}
	
	// Ship logic
	public override void Update(float dT)
	{
		// Call the base implementation first
		base.Update(dT);
		
		// Grow our internal timer
		TotalTime += dT;
		
		// If we have no target resource and we are looking for minerals,
		// find the closest to the base's position
		if(TargetResource == null && MineralCount < MaxMineralCount)
			TargetResource = Globals.WorldView.SceneManager.GetClosestScenery(GetPosition(), SceneryType.Mineral);
		
		// If we do have a resource we can go to, move towards it
		if(TargetResource != null)
		{
			// Move towards resource if we are empty / collecting
			if(MineralCount < MaxMineralCount)
			{
				// Target position & radius
				Vector2 TargetPos = TargetResource.ScenerySprite.GetPosition();
				float Radius = TargetResource.Radius;
				
				// We want the ship to rotate over time, so move the target
				// very slowely around the target. Also note we make this radius
				// a bit smaller to allow the ship to be within the radius to allow mining
				const float RotSpeed = 0.3f;
				const float RotDist = 0.6f;
				TargetPos += new Vector2(Radius * Mathf.Cos(TotalTime * RotSpeed) * RotDist, Radius * Mathf.Sin(TotalTime * RotSpeed) * RotDist);
				UpdateBeam(true, TargetPos);
				
				// Tell the ship to move towards taget position over time
				MoveTowards(TargetPos, dT);
				
				// Are we close enough to collect resources?
				float Dist = (TargetPos - GetPosition()).magnitude;
				if(Dist <= Radius)
				{
					// Grow collection timer, and increase resource when appropriate
					// Updates every second
					CollectionTimer += dT;
					if(CollectionTimer > 1.0f)
					{
						// Reset timer, add resource
						CollectionTimer -= 1.0f;
						MineralCount += CollectionRate;
						
						// Remove that much from resource
						TargetResource.Minerals -= CollectionRate;
						
						// Min/max the values
						if(TargetResource.Minerals < 0)
							TargetResource.Minerals = 0;
						if(MineralCount > MaxMineralCount)
							MineralCount = MaxMineralCount;
					}
				}
			}
			// Else, we are full, so move back home
			else
				MoveTowards(HomeBase.Position, dT);
		}
		// Else, move back home
		else
			MoveTowards(HomeBase.Position, dT);
		
		// TODO: If at home, off-load all the minerals
		float HomeDistance = (HomeBase.Position - new Vector2(GetPosition().x, GetPosition().y)).magnitude;
		if(HomeDistance < 50)
		{
			Globals.WorldView.ResManager.AddResources(MineralCount);
			MineralCount = 0;
		}
	}
	
	// Turn on the beam
	// Debugging and fluff
	void UpdateBeam(bool Draw, Vector2 Target)
	{
		// If draw line, update both
		if(Draw && TargetResource != null)
		{
			BeamLine.SetVertexCount(2);
			BeamLine.SetPosition(0, new Vector3(GetPosition().x, GetPosition().y, Globals.ContrailDepth));
			BeamLine.SetPosition(1, new Vector3(Target.x, Target.y, Globals.ContrailDepth));
		}
		else
			BeamLine.SetVertexCount(0);
	}
}
