/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: BaseShip.cs
 Desc: The base class for all ships, which implements the following:
 
 * Ship movement physics
 * Ship health (shield, hull, base health, etc.)
 * Ship animations
 * Weapons
 * Breaks into components once destroyed
 * Loads frame, hull, and shield
 * *Ship-to-ship pushing
 
 Note: Ship acceleration, velocity, position, etc. are all triple-
 values because they represent (x, y, and z:rotation) (based on
 unit-circle where rotations start at x+ axis and in radians).
 
 Note2: It is VERY important to understand that ship physics is
 localized, meaning only the position and rotation is global, while
 ship velocity x+ is always facing the direction of the ship, with y+
 being left and y- right.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;

// Enumeration of all weapon types
public enum BaseShip_WeaponTypes
{
	Gatling,	// Trivial bullets
	Laser,		// Same as bullets, different art
	Missile,	// Needs special physics to apply (and contrails)
	RailGun,	// Same as bullets, different art (and contrails)
};

// Ship weapon component
public class BaseShip_Weapon
{
	// Sprite for the ship weapon (usually just a single rotating element)
	public Sprite WeaponSprite;
	
	// Weapon position (from the origin of the sprite)
	public Vector2 Position;
	
	// The kind of weapon
	public BaseShip_WeaponTypes WeaponType;
	
	// Cool down rate, and current cooldown
	// Unable to do anything unless cooldown is 0
	// Once something is done, CoolDown is set back to CoolDownRate
	public float CoolDown, CoolDownRate;
}

// Ship contrail list
public class BaseShip_Contrail
{
	// Ship contrail, and its properties
	public Vector2 ContrailShipPos;
	public GameObject ContrailObject;
	public LineRenderer Contrail;
	public const int ContrailLength = 10;
	public Vector2[] ContrailPoints;
	public float ContrailTime; // How long has it been since our last update?
	public const float ContrailTimeDelta = 0.5f;
	public const int ContrailDepth = 11; // Ship is at 10, so put this right below
}

// Ship hull animation group
public class BaseShip_Hull
{
	// Animation pos, frame, count, and time (all standard)
	public Vector2 Pos;
	public Vector2 Size;
	public int FrameCount;
	public float FrameTime;
}

// Ship detail animation group
public class BaseShip_Detail
{
	public AnimatedSprite DetailSprite;		// Sprite to render / animate
	public Vector2 Position;				// Position on ship
	public bool Fixed;						// Fixed location
}

// Entity implementation
public class BaseShip
{
	/*** Internals ***/
	
	// Total ship existance (time)
	float TotalTime;
	
	// Ship name
	private String ShipName;
	
	// Ammount of time since the last damage hit the ship
	float LastDamageTime = 60.0f; // At first, no damage hit
	
	// Ship forward acceleration and forward velocity
	// Note that x: forward, y: rotation
	private Vector2 ShipAcceleration;
	private Vector2 ShipVelocity;
	
	// Ship position (based on velocity) (center of ship)
	private Vector3 ShipPosition;
	
	// Sprite & sprite sheet info for this ship
	private Sprite ShieldSprite, HullSprite, FrameSprite; // Hull is above frame
	private BaseShip_Hull ShieldAnimation, FrameAnimation;
	
	// Shield, hull, and frame health (and death)
	private float ShieldHealth, ShieldMaxHealth;
	private float HullHealth, HullMaxHealth;
	private bool IsDead = false;
	
	// Ship hull list (array of BaseShip_Hull)
	private List<BaseShip_Hull> HullList;
	private int HullSkinIndex; // Current hull index
	
	// Weapons list (array of BaseShip_Weapon)
	private List<BaseShip_Weapon> WeaponsList;
	
	// Contrail list (array of BaseShip_Contrail)
	private List<BaseShip_Contrail> ContrailList;
	
	// Animation lists (array of AnimatedSprite)
	private List<BaseShip_Detail> DetailsList;
	
	// All ships have velocity limit (how fast they can move and how fast they can rotate)
	private Vector2 MaxVelocity;
	
	// Total number of chunks (i.e. debris)
	private int ChunkCount;
	
	// Config file name used for laoding
	private String ConfigFileName;
	
	/*** PID Controller Vars ***/
	
	// Retainers for diff. / integ.
	private float PID_PrevError;
	private float PID_Integral;
	
	// Note: these are heuristic values - chosen to fit this specific project and its world-scale
	private const float Kp = 50.0f;		// Low for faster convergence
	private const float Ki = 0.01f;		// Low for sub (but faster) convergence
	private const float Kd = 20.0f;		// Higher for less vibration on convergence
	
	// Target position we want the ship to move towards
	private Vector2 TargetPosition = new Vector2();
	
	/*** Public ***/
	
	// Standard constructor: needs to have a paried config file name
	// which should implement all of the groups and key-value pairs
	// found in the ship demo config file
	public BaseShip(String ConfigFileName)
	{
		// Load the ship's properties
		this.ConfigFileName = ConfigFileName;
		ConfigFile Info = new ConfigFile(ConfigFileName);
		
		/*** General Init. ***/
		
		// Save the health components
		ShieldHealth = ShieldMaxHealth = Info.GetKey_Int("General", "Shield");
		HullHealth = HullMaxHealth = Info.GetKey_Int("General", "Hull");
		
		// Save velicity limit and default pos to center
		MaxVelocity = Info.GetKey_Vector2("General", "MaxVelocity");
		ShipAcceleration = new Vector2();
		ShipVelocity = new Vector2();
		ShipPosition = new Vector3();
		
		// Get all group names to load up specialty geometry
		String TextureName = Info.GetKey_String("General", "Texture");
		String[] GroupNames = Info.GetGroupNames();
		
		/*** Frame, Shield & Hull ***/
		
		// For each group, look just for "Frame" and "Shield"
		foreach(String GroupName in GroupNames)
		{
			// If shield
			if(GroupName.ToLower().StartsWith("shield") == true)
			{
				// Get animation / frame info
				ShieldAnimation = LoadHullType(Info, GroupName);
				
				// Register the sprite with the sprite manager
				ShieldSprite = new Sprite("Textures/" + TextureName);
				ShieldSprite.SetAnimation(ShieldAnimation.Pos, ShieldAnimation.Size, ShieldAnimation.FrameCount, ShieldAnimation.FrameTime);
				ShieldSprite.SetGeometrySize(ShieldSprite.GetSpriteSize());
				ShieldSprite.SetRotationCenter(ShieldSprite.GetGeometrySize() / 2.0f);
				ShieldSprite.SetDepth(Globals.ShieldDepth);
				Globals.WorldView.SManager.AddSprite(ShieldSprite);
			}
			// Elif base frame
			else if(GroupName.ToLower().StartsWith("frame") == true)
			{
				// Get animation / frame info
				FrameAnimation = LoadHullType(Info, GroupName);
				
				// Register the sprite with the sprite manager
				FrameSprite = new Sprite("Textures/" + TextureName);
				FrameSprite.SetAnimation(FrameAnimation.Pos, FrameAnimation.Size, FrameAnimation.FrameCount, FrameAnimation.FrameTime);
				FrameSprite.SetGeometrySize(FrameSprite.GetSpriteSize());
				FrameSprite.SetRotationCenter(FrameSprite.GetGeometrySize() / 2.0f);
				FrameSprite.SetDepth(Globals.FrameDepth);
				Globals.WorldView.SManager.AddSprite(FrameSprite);
			}
		}
		
		// Load all the hulls
		HullList = new List<BaseShip_Hull>();
		foreach(String GroupName in GroupNames)
		{
			if(GroupName.ToLower().StartsWith("hull") == true)
			{
				BaseShip_Hull Hull = LoadHullType(Info, GroupName);
				HullList.Add(Hull);
			}
		}
		
		// Default to initial hull
		HullSprite = new Sprite("Textures/" + TextureName);
		HullSkinIndex = 0; // Index grows while health goes down
		BaseShip_Hull HullAnimation = HullList[HullSkinIndex];
		
		HullSprite.SetAnimation(HullAnimation.Pos, HullAnimation.Size, HullAnimation.FrameCount, HullAnimation.FrameTime);
		HullSprite.SetGeometrySize(HullSprite.GetSpriteSize());
		HullSprite.SetRotationCenter(HullSprite.GetGeometrySize() / 2.0f);
		HullSprite.SetDepth(Globals.HullDepth);
		Globals.WorldView.SManager.AddSprite(HullSprite);
		
		/*** Contrails ***/
		
		// Load all contrails
		ContrailList = new List<BaseShip_Contrail>();
		foreach(String GroupName in GroupNames)
		{
			BaseShip_Contrail NewContrail = LoadContrail(Info, GroupName);
			if(NewContrail != null)
				ContrailList.Add(NewContrail);
		}
		
		/*** Weapons ***/
		
		// Load all weapons
		WeaponsList = new List<BaseShip_Weapon>();
		foreach(String GroupName in GroupNames)
		{
			BaseShip_Weapon NewWeapon = LoadWeapon(Info, GroupName);
			if(NewWeapon != null)
				WeaponsList.Add(NewWeapon);
		}
		
		/*** Animation Entities ***/
		
		// Load all animations
		DetailsList = new List<BaseShip_Detail>();
		foreach(String GroupName in GroupNames)
		{
			BaseShip_Detail Detail = LoadDetail(Info, GroupName);
			if(Detail != null)
				DetailsList.Add(Detail);
		}
		
		/*** Misc. ***/
		
		// Chunk count
		foreach(String GroupName in GroupNames)
		{
			if(GroupName.ToLower().StartsWith("chunk"))
				ChunkCount++;
		}
		
		// Generate unique ship name
		NameGenerator NameGen = new NameGenerator();
		ShipName = NameGen.generateName();
	}
	
	// Return ship name
	public String GetShipName()
	{
		return ShipName;
	}
	
	// Update ship internals
	// Note: if this function is overloaded (commonly done so for AI purposes), then
	// MAKE SURE to call this function in the overloading function at first
	public virtual void Update(float dT)
	{
		// Timers update
		TotalTime += dT;
		LastDamageTime += dT;
		
		/*** Update Shield ***/
		
		// If the last time we were hit is more than 5 seconds, grow back the shield
		if(LastDamageTime > 5.0f && ShieldHealth < ShieldMaxHealth)
		{
			ShieldHealth += 10.0f * dT;
			if(ShieldHealth > ShieldMaxHealth) ShieldHealth = ShieldMaxHealth;
		}
		
		// If shield is gone, do not render it
		float Alpha = ShieldSprite.GetColor().a;
		if(ShieldHealth <= 0)
			ShieldSprite.SetColor(new Color(1, 1, 1, 0));
		// Else, if recently hit, flash it
		else if(LastDamageTime < 0.1f)
			ShieldSprite.SetColor(new Color(1, 1, 1, 1));
		// Else, is calms down to a low mid-alpha
		else if(Alpha > 0.2f)
		{
			Alpha -= dT * 0.5f;
			ShieldSprite.SetColor(new Color(1, 1, 1, Alpha));
		}
		
		/*** Update Ship Geometry / Skin ***/
		
		// Change index only on hull - shield is different graphical logic, and frame is just more flashes
		float HealthRatio = HullHealth / HullMaxHealth;
		int SkinIndex = (int)((HullList.Count - 1) * (1.0f - HealthRatio));
		
		// Only change if needed
		if(HullSkinIndex != SkinIndex)
		{
			// Save index, update animation
			HullSkinIndex = SkinIndex;
			BaseShip_Hull HullAnimation = HullList[HullSkinIndex] as BaseShip_Hull;
			HullSprite.SetAnimation(HullAnimation.Pos, HullAnimation.Size, HullAnimation.FrameCount, HullAnimation.FrameTime);
		}
		
		/*** Update Ship Movement ***/
		
		// Standard physics updates
		ShipVelocity += ShipAcceleration * dT;
		
		// Decay ship velocity over time by 1%
		ShipVelocity *= 0.99f;
		
		// Limit to max forward velocity and turning velocity
		ShipVelocity.x = Mathf.Clamp(ShipVelocity.x, 0.0f, MaxVelocity.x);
		ShipVelocity.y = Mathf.Clamp(ShipVelocity.y, -MaxVelocity.y, MaxVelocity.y);
		
		// We have to re-align velcity with direction
		ShipPosition.z += ShipVelocity.y * dT;
		float Theta = ShipPosition.z;
		
		Vector2 Point = new Vector2(ShipVelocity.x, ShipVelocity.y) * dT;
		ShipPosition.x += Point.x * Mathf.Cos(Theta) - Point.y * Mathf.Sin(Theta);
		ShipPosition.y += Point.y * Mathf.Cos(Theta) + Point.x * Mathf.Sin(Theta);
		
		// Position the ship center should be on (frame, hull, shield)
		FrameSprite.SetRotation(ShipPosition.z);
		FrameSprite.SetPosition(new Vector2(ShipPosition.x, ShipPosition.y));
		HullSprite.SetRotation(ShipPosition.z);
		HullSprite.SetPosition(new Vector2(ShipPosition.x, ShipPosition.y));
		ShieldSprite.SetRotation(ShipPosition.z);
		ShieldSprite.SetPosition(new Vector2(ShipPosition.x, ShipPosition.y));
		
		// Update all weapon positions based on this
		foreach(BaseShip_Weapon Weapon in WeaponsList)
		{
			Weapon.WeaponSprite.SetPosition(GetGlobalPosFromTexturePos(Weapon.Position));
			Weapon.CoolDown += dT;
		}
		
		// Update all detail positions based on this
		foreach(BaseShip_Detail Detail in DetailsList)
		{
			Detail.DetailSprite.SetPosition(GetGlobalPosFromTexturePos(Detail.Position));
			if(Detail.Fixed)
				Detail.DetailSprite.SetRotation(Theta);
		}
		
		// Pulse the ship frame, so it's more visible during combat
		FrameSprite.SetColor(new Color(0.5f + 0.5f * Mathf.Sin(TotalTime * 5.0f), 0.25f, 0.25f));
		
		/*** Update Ship Pushing ***/
		
		// Sum of all pushes
		Vector2 PushVelocity = new Vector2();
		
		// Total push-back vector
		foreach(BaseShip Ship in Globals.WorldView.ShipManager.ShipsList)
		{
			// As long not the same ship
			if(Ship != this && GetPosition() != Ship.GetPosition())
			{
				Vector2 Direction = (GetPosition() - Ship.GetPosition());
				float Distance = Direction.magnitude;
				Direction.Normalize();
				Direction *= 50.0f / Mathf.Pow(Distance, 3); // Good heuristic values
				
				float ShipScale = Ship.GetHullSprite().GetGeometrySize().x * Ship.GetHullSprite().GetGeometrySize().y;
				PushVelocity += Direction * ShipScale * dT;
			}
		}
		
		// Apply push to ship
		ShipPosition.x += PushVelocity.x;
		ShipPosition.y += PushVelocity.y;
		
		/*** Update Contrails ***/
		
		// Update all the contrail points as needed
		foreach(BaseShip_Contrail Contrail in ContrailList)
		{
			// Get pre-processing info
			Contrail.ContrailTime += dT;
			Vector2 ShipTail = GetGlobalPosFromTexturePos(Contrail.ContrailShipPos);
			
			if(Contrail.ContrailTime >= BaseShip_Contrail.ContrailTimeDelta)
			{
				// Shift all points to the right (left is youngest)
				for(int i = BaseShip_Contrail.ContrailLength - 1; i >= 1; i--)
					Contrail.ContrailPoints[i] = Contrail.ContrailPoints[i - 1];
				Contrail.ContrailPoints[0] = ShipTail;
				
				// Reset time
				Contrail.ContrailTime -= BaseShip_Contrail.ContrailTimeDelta;
			}
			
			// Else, use lerp to move over time
			for(int i = 0; i < BaseShip_Contrail.ContrailLength - 1; i++)
			{
				float Progress = Contrail.ContrailTime / BaseShip_Contrail.ContrailTimeDelta;
				Vector3 Dest = new Vector3(Contrail.ContrailPoints[i].x, Contrail.ContrailPoints[i].y, BaseShip_Contrail.ContrailDepth);
				Vector3 Source = new Vector3(Contrail.ContrailPoints[i + 1].x, Contrail.ContrailPoints[i + 1].y, BaseShip_Contrail.ContrailDepth);
				Contrail.Contrail.SetPosition(i, Vector3.Lerp(Source, Dest, Progress));
			}
			
			Contrail.Contrail.SetPosition(0, new Vector3(ShipTail.x, ShipTail.y, BaseShip_Contrail.ContrailDepth));
		}
	}
	
	// Load a ship hull animation set
	// Note: this is the only LoadXYZ function that does NOT do the internal group type check
	private BaseShip_Hull LoadHullType(ConfigFile Info, String GroupName)
	{
		// Alloc a new hull
		BaseShip_Hull Hull = new BaseShip_Hull();
		
		// Load the animation basics
		Hull.Pos = Info.GetKey_Vector2(GroupName, "Pos");
		Hull.Size = Info.GetKey_Vector2(GroupName, "Size");
		Hull.FrameCount = Info.GetKey_Int(GroupName, "Count");
		Hull.FrameTime = Info.GetKey_Float(GroupName, "Time");
		
		// All done!
		return Hull;
	}
	
	// Load a contrail based on the current group name
	private BaseShip_Contrail LoadContrail(ConfigFile Info, String GroupName)
	{
		// Ignore if not contrail
		if(GroupName.ToLower().StartsWith("contrail") == false)
			return null;
		
		// Alloc a new contrail
		BaseShip_Contrail Contrail = new BaseShip_Contrail();
		
		// Get color, width, and ship position
		Vector3 ContrailColor = Info.GetKey_Vector3(GroupName, "Color");
		float ContrailWidth = Info.GetKey_Float(GroupName, "Width");
		Contrail.ContrailShipPos = Info.GetKey_Vector2(GroupName, "Pos");
		
		// Initialize the contrail objects
		Contrail.ContrailObject = new GameObject("Contrail");
		Contrail.Contrail = Contrail.ContrailObject.AddComponent("LineRenderer") as LineRenderer;
		Contrail.Contrail.material = new Material(Shader.Find("Particles/Additive"));//("Particles/Additive"));
		Contrail.Contrail.SetColors(new Color(ContrailColor.x, ContrailColor.y, ContrailColor.z), Color.clear);
		Contrail.Contrail.SetWidth(ContrailWidth, 2.0f);
		Contrail.Contrail.SetVertexCount(BaseShip_Contrail.ContrailLength - 1); // -1 since we are lerp-ing
		Contrail.ContrailPoints = new Vector2[BaseShip_Contrail.ContrailLength];
		Contrail.ContrailTime = 0.0f;
		
		// Reset trail
		SetPos(ShipPosition);
		
		// All done!
		return Contrail;
	}
	
	// Load a weapon based on the current group name (and does the internal sprite loading too)
	private BaseShip_Weapon LoadWeapon(ConfigFile Info, String GroupName)
	{
		// Ignore if not weapon
		if(GroupName.ToLower().StartsWith("weapon") == false)
			return null;
		
		// Load from config files
		BaseShip_Weapon Weapon = new BaseShip_Weapon();
		String Type = Info.GetKey_String(GroupName, "Type");
		Weapon.Position = Info.GetKey_Vector2(GroupName, "Pos");
		
		// Get sprite info
		ConfigFile SpriteInfo = new ConfigFile("Config/Weapons/WeaponsConfig");
		
		String TextureName = SpriteInfo.GetKey_String(Type, "Texture");
		Weapon.CoolDownRate = SpriteInfo.GetKey_Float(Type, "CoolDown");
		Vector2 SPos = SpriteInfo.GetKey_Vector2(Type, "Pos");
		Vector2 SSize = SpriteInfo.GetKey_Vector2(Type, "Size");
		//Vector2 SCenter = SpriteInfo.GetKey_Vector2(Type, "Center"); // Todo, so we rotate about a point, not center
		int SCount = SpriteInfo.GetKey_Int(Type, "Count");
		float STime = SpriteInfo.GetKey_Float(Type, "Time");
		
		// Load the sprite
		Weapon.WeaponSprite = new Sprite("Textures/" + TextureName);
		
		Weapon.WeaponSprite.SetSpritePos(SPos);
		Weapon.WeaponSprite.SetSpriteSize(SSize);
		Weapon.WeaponSprite.SetGeometrySize(Weapon.WeaponSprite.GetSpriteSize());
		
		Weapon.WeaponSprite.SetRotationCenter(Weapon.WeaponSprite.GetGeometrySize() / 2.0f);
		
		// Set animation (if any)
		Weapon.WeaponSprite.SetAnimation(SPos, SSize, SCount, STime);
		
		// Right above regular ships
		Weapon.WeaponSprite.SetDepth(Globals.WeaponsDepth);
		
		// Register with renderer
		Globals.WorldView.SManager.AddSprite(Weapon.WeaponSprite);
		
		// Weapon type
		String WeaponType = SpriteInfo.GetKey_String(Type, "Projectile");
		if(WeaponType.ToLower().CompareTo("gatling") == 0)
			Weapon.WeaponType = BaseShip_WeaponTypes.Gatling;
		else if(WeaponType.ToLower().CompareTo("laser") == 0)
			Weapon.WeaponType = BaseShip_WeaponTypes.Laser;
		else if(WeaponType.ToLower().CompareTo("missile") == 0)
			Weapon.WeaponType = BaseShip_WeaponTypes.Missile;
		else if(WeaponType.ToLower().CompareTo("railgun") == 0)
			Weapon.WeaponType = BaseShip_WeaponTypes.RailGun;
		
		// All done!
		return Weapon;
	}
	
	// Load a detail described in the config file
	private BaseShip_Detail LoadDetail(ConfigFile Info, String GroupName)
	{
		// Ignore if not a detail
		if(GroupName.ToLower().StartsWith("detail") == false)
			return null;
		
		// Create a new detail obj to return
		BaseShip_Detail Detail = new BaseShip_Detail();
		
		// All we need is a reference to the detail's name
		Detail.Position = Info.GetKey_Vector2(GroupName, "Pos");
		
		// Load a new sprite, and default it to animation state
		String DetailName = Info.GetKey_String(GroupName, "Type");
		Detail.DetailSprite = new AnimatedSprite("Config/" + DetailName);
		Detail.DetailSprite.SetAnimation("Animation");
		Detail.DetailSprite.SetDepth(Globals.ShipDetailsDepth);
		
		// Get the active animation, and see 
		if(Detail.DetailSprite.GetRotationRate() == 0.0f)
			Detail.Fixed = true;
		else
			Detail.Fixed = false;
		
		// Set scale and center
		Detail.DetailSprite.SetGeometrySize(Detail.DetailSprite.GetSpriteSize());
		Detail.DetailSprite.SetRotationCenter(Detail.DetailSprite.GetGeometrySize() / 2.0f);
		
		// Register sprite
		Globals.WorldView.SManager.AddSprite(Detail.DetailSprite);
		
		// Load the animated sprite
		return Detail;
	}
	
	/*** Controller Functions ***/
	
	// Allow access to ship position (center of ship)
	public Vector2 GetPosition()
	{
		return ShipPosition;
	}
	
	// Set the acceleration vectors (i.e. thrust)
	// X: Forward thrust
	// Y: rotation about the center thrust
	public void SetThrust(Vector2 NewThrust)
	{
		// Bounds check the thrust
		// A ship can only move forward and at 10% max velocity
		ShipAcceleration.x = Mathf.Clamp(NewThrust.x, -MaxVelocity.x * 0.05f, MaxVelocity.x * 0.05f);
		ShipAcceleration.y = Mathf.Clamp(NewThrust.y, -MaxVelocity.y * 0.01f, MaxVelocity.y * 0.01f);
	}
	
	// Explicitly set the position, used for spawning, etc..
	public void SetPos(Vector2 Position)
	{
		ShipPosition.x = Position.x;
		ShipPosition.y = Position.y;
		
		// Reset contrail
		foreach(BaseShip_Contrail Contrail in ContrailList)
		{
			// Get pre-processing info
			Vector2 ShipTail = GetGlobalPosFromTexturePos(Contrail.ContrailShipPos);
			for(int i = 0; i < BaseShip_Contrail.ContrailLength - 1; i++)
				Contrail.Contrail.SetPosition(i, new Vector3(ShipTail.x, ShipTail.y, BaseShip_Contrail.ContrailDepth));
			for(int i = 0; i < BaseShip_Contrail.ContrailLength; i++)
				Contrail.ContrailPoints[i] = ShipTail;
		}
	}
	
	// Does the given position exisist within this ship?
	public bool CheckCollision(Vector2 Pos)
	{
		// Localize position
		Pos -= new Vector2(ShipPosition.x, ShipPosition.y);
		
		// Ship rotation
		float Theta = ShipPosition.z;
		
		// Rotate (re-orient) the bullet position so that it is in the same coordinate-system
		// as the ship: using axis-alligned object transformation for simplified math
		// Essentially we are applying the reverse rotation, relative to the ship's rotational origin, so that
		// when we do the trivial "point in box" check, this is all axis-aligned
		Vector2 AlignedPos;
		AlignedPos.x = Pos.x * Mathf.Cos(-Theta) - Pos.y * Mathf.Sin(-Theta);
		AlignedPos.y = Pos.y * Mathf.Cos(-Theta) + Pos.x * Mathf.Sin(-Theta);
		
		// Get real-world ship size (note: ship position is the center of the volume, hence the div by 2)
		Vector2 ShipSize = FrameSprite.GetGeometrySize();
		ShipSize /= 2.0f;
		
		// Scale down the ship to 80%
		ShipSize *= 0.8f;
		
		// Point-in-box check
		if(AlignedPos.x > -ShipSize.x && AlignedPos.x < ShipSize.x && AlignedPos.y < ShipSize.y && AlignedPos.y > -ShipSize.y)
			return true;
		else
			return false;
	}
	
	// Returns true on collision with a given bullet (regardless of owner)
	public bool CheckProjectile(Projectile Bullet)
	{
		// Fast check: ignore all projectiles too far from the ship
		Vector2 ShipBulletPos = Bullet.ProjectileSprite.GetPosition() - new Vector2(ShipPosition.x, ShipPosition.y);
		if(ShipBulletPos.magnitude > FrameSprite.GetGeometrySize().x)
			return false;
		
		// Second check: axis-align the bullet relative to the center of the ship
		return CheckCollision(Bullet.ProjectileSprite.GetPosition());
	}
	
	// Ship has been hit by the given projectile: apply damage code
	public void Hit(Projectile Bullet)
	{
		// Get the damage ammount
		int Damage = Bullet.Damage;

		// Reset the age of the last damage commited
		LastDamageTime = 0.0f;
		
		// Apply damage, layer after layer
		if(ShieldHealth > 0)
		{
			ShieldHealth -= Damage;
			if(ShieldHealth < 0) ShieldHealth = 0;
		}
		else if(HullHealth > 0)
		{
			HullHealth -= Damage;
			if(HullHealth < 0) HullHealth = 0;
		}
		else
			SetDead();
	}
	
	// Is the ship dead?
	public bool GetDead()
	{
		return IsDead;
	}
	
	// Remove ship components (i.e. release)
	// After calling this function, do NOT manage this ship anymore
	public void Destroy(bool Silent = false)
	{
		// Force death
		IsDead = true;
		
		// Remove all sprites
		Globals.WorldView.SManager.RemoveSprite(ShieldSprite);
		Globals.WorldView.SManager.RemoveSprite(HullSprite);
		Globals.WorldView.SManager.RemoveSprite(FrameSprite);
		
		// Remove contrails
		foreach(BaseShip_Contrail Contrail in ContrailList)
			UnityEngine.Object.Destroy(Contrail.ContrailObject);
		// Remove guns
		foreach(BaseShip_Weapon Weapon in WeaponsList)
			Globals.WorldView.SManager.RemoveSprite(Weapon.WeaponSprite);
		
		// Remove details
		foreach(BaseShip_Detail Detail in DetailsList)
			Globals.WorldView.SManager.RemoveSprite(Detail.DetailSprite);
		
		// Global notification that this ship died
		if(!Silent)
		{
			String Message = TextEvents.GetDestructionMessage(this);
			Globals.WorldView.OverlayView.PushMessage(Color.red, Message);
		}
	}
	
	// Move the ship towards a point over time
	// Note: this is not a "move to" function but "move towards", in the sense
	// that by calling this function over and over again, you are "pushing" the
	// ship towards the target location
	protected void MoveTowards(Vector2 TargetPos, float dT)
	{
		// Based on a PID controller (from robotics experiance)
		// http://en.wikipedia.org/wiki/PID_controller#Pseudocode
		// The idea is we use a closed-loop controller to determine
		// when to speed up and slow down based on a target distance
		
		// Save for internal usage
		TargetPosition = TargetPos;
		
		/*** Rotation ***/
		
		// Angle to object
		Vector2 TargetNormal = (new Vector2(TargetPosition.x, TargetPosition.y) - new Vector2(ShipPosition.x, ShipPosition.y)).normalized;
		float Theta = (float)Math.Atan2(TargetNormal.y, TargetNormal.x);
		
		// Difference to target
		float Error = Theta - ShipPosition.z;
		
		// Compute derivative & integral
		PID_Integral += Error * dT;
		float Derivative = (Error - PID_PrevError) / dT;
		PID_PrevError = Error;
		
		// Compute output
		float ROutput = (Kp * Error) + (Ki * PID_Integral) + (Kd * Derivative);
		
		/*** Velocity ***/
		
		// If the angle between the target and our facing is less than a quarter, activate acceleration
		float VOutput = 0.0f;
		
		float dTheta = ShipPosition.z - Theta;
		if(Mathf.Abs(dTheta) < Mathf.PI / 4.0f)
			VOutput = MaxVelocity.x;
		
		// Let's see if this works...
		SetThrust(new Vector2(VOutput, ROutput));
	}
	
	// Fire all weapons, if a weapon is cooled down, at the given target
	public void FireAt(Vector2 Target)
	{
		// For each weapon
		foreach(BaseShip_Weapon Weapon in WeaponsList)
		{
			// Allign weapon to target (note we do Target - (correct center))
			Vector2 SpriteCenter = Weapon.WeaponSprite.GetPosition();
			Vector2 Velocity = (Target - SpriteCenter).normalized * 50.0f;
			Weapon.WeaponSprite.SetRotation(Mathf.Atan2(Velocity.y, Velocity.x));
			
			// Insert projectile IF the weapon is cooled-down
			if(Weapon.CoolDown > Weapon.CoolDownRate)
			{
				// Add projectile and reset cooldown
				Globals.WorldView.ProjManager.AddProjectile(this, Weapon.WeaponType, SpriteCenter, Velocity);
				Weapon.CoolDown = 0.0f;
				
				// Add audio
				int Index = UnityEngine.Random.Range(1, 4); // [1, 4)
				Globals.WorldView.AManager.PlayAudio(GetPosition(), "Laser" + Index);
			}
		}
	}
	
	// Return the total number of chunks in the config file
	public int GetChunkCount()
	{
		return ChunkCount;
	}
	
	// Get config file name
	public String GetConfigFileName()
	{
		return ConfigFileName;
	}
	
	// Health accessor
	public float GetShieldHealth() { return ShieldHealth; }
	public float GetMaxShieldHealth() { return ShieldMaxHealth; }
	public float GetHullHealth() { return HullHealth; }
	public float GetMaxHullHealth() { return HullMaxHealth; }
	
	// Return direct access of the sprite (for measurements only)
	public Sprite GetHullSprite()
	{
		return HullSprite;
	}
	
	// Get rotation of sprite
	public float GetRotation()
	{
		return ShipPosition.z;
	}
	
	/*** Private Helpers ***/
	
	// Given a texture position, return the global position based on current frame (ship) orientation's center
	private Vector2 GetGlobalPosFromTexturePos(Vector2 TexturePos)
	{
		// Flip the y axis from the normal texture origin (top-left) to Unity texture origin (bottom-left), then re-center
		Vector2 OldPoint = new Vector2(TexturePos.x, FrameSprite.GetSpriteSize().y - TexturePos.y) - FrameSprite.GetSpriteSize() / 2.0f;
		float Theta = ShipPosition.z;
		
		float dX = OldPoint.x * Mathf.Cos(Theta) - OldPoint.y * Mathf.Sin(Theta);
		float dY = OldPoint.y * Mathf.Cos(Theta) + OldPoint.x * Mathf.Sin(Theta);
		
		return new Vector2(ShipPosition.x + dX, ShipPosition.y + dY);
	}
	
	// Ship has died, self-flag as dead
	private void SetDead()
	{
		IsDead = true;
	}
}
