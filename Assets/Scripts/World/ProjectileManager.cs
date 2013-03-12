/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: Projectiles.cs
 Desc: Creates, removes, and manages all projectiles in the world,
 such as bullets and missles. Note that the clipping occures
 2x the size of the world to allow the user's camera to see everything
 even when fully zoomed out.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// Trivial projectile class
public class Projectile
{
	// Type, sprite (position is in here), and velocity)
	public BaseShip_WeaponTypes Type;
	public Sprite ProjectileSprite;
	public Vector2 Vecloty;
	
	// Projectiles always have an owner, even if the owner is dead
	public BaseShip Owner = null;
	
	public int Damage;
}

public class ProjectileManager : MonoBehaviour
{
	/*** Internals ***/
	
	// List of all projectiles on-screen
	List<Projectile> Projectiles;
	
	// List of all explosion and glow animations (just sprites)
	List<Sprite> Explosions;
	List<Sprite> Glow;
	
	// True if all of the below is set
	public float WorldWidth;
	public List<BaseShip> ShipsList;
	public List<BaseBuilding> BuildingsList;
	public SceneryManager Scenery;
	
	// Common config files
	private ConfigFile ProjectileInfo;
	
	/*** Script Methods ***/
	
	// Standard init
	void Start()
	{
		// Alloc lists
		Projectiles = new List<Projectile>();
		Explosions = new List<Sprite>();
		Glow = new List<Sprite>();
		
		// Alloc common config files
		ProjectileInfo = new ConfigFile("Config/Weapons/ProjectilesConfig");
		
		// Create shorthand variables
		WorldWidth = WorldManager.WorldWidth;
		ShipsList = Globals.WorldView.ShipManager.ShipsList;
		BuildingsList = Globals.WorldView.BuildingManager.WorldBuildings;
		Scenery = Globals.WorldView.SceneManager;
	}
	
	// Update is called once per frame
	void Update()
	{
		// Do nothing if paused
		if(Globals.WorldView.Paused)
			return;
		
		// Delta time
		float dT = Time.deltaTime;
		
		// Apply the play-speed factor
		if(Globals.WorldView.PlaySpeed == GameSpeed.Fast)
			dT *= 2;
		else if(Globals.WorldView.PlaySpeed == GameSpeed.Faster)
			dT *= 4;
		
		/*** Bullet Movement & Collision ***/
		
		// For each bullet, move it forward based on velocity (or clip)
		for(int i = Projectiles.Count - 1; i >= 0; i--)
		{
			// Get bullet in question and move forward
			Projectile Bullet = Projectiles[i] as Projectile;
			Vector2 Pos = Bullet.ProjectileSprite.GetPosition();
			Bullet.ProjectileSprite.SetPosition(Pos + Bullet.Vecloty * dT);
			
			// If out of bounds, remove
			if(Pos.x < -WorldWidth * 2 || Pos.x > WorldWidth * 2 || Pos.y < -WorldWidth * 2 || Pos.y > WorldWidth * 2)
			{
				Globals.WorldView.SManager.RemoveSprite(Bullet.ProjectileSprite);
				Projectiles.RemoveAt(i);
			}
			// Else, collision check against each ship
			else
			{
				foreach(BaseShip Ship in ShipsList)
				{
					// Get owner and reciever friend or foe classification
					bool OwnerEnemy = IsEnemy(Bullet.Owner);
					bool RecieverEnemy = IsEnemy(Ship);
					
					// On collision: remove bullet, and add explosion
					if(OwnerEnemy != RecieverEnemy && Ship.CheckProjectile(Bullet))
					{
						// Tell ship it got hit
						Ship.Hit(Bullet);
						
						// Add explosion
						AddExplosion(Bullet.ProjectileSprite.GetPosition());
						
						// Remove sprite
						Globals.WorldView.SManager.RemoveSprite(Bullet.ProjectileSprite);
						Projectiles.RemoveAt(i);
						break;
					}
				}
				
				bool isBulletEnemy = IsEnemy(Bullet.Owner);
				foreach(BaseBuilding building in BuildingsList)
				{
					if(isBulletEnemy)
					{
						if(building.CheckProjectile(Bullet))
						{
							building.TakeDamage(Bullet.Damage);
							AddExplosion(Bullet.ProjectileSprite.GetPosition());
							Globals.WorldView.SManager.RemoveSprite(Bullet.ProjectileSprite);
							Projectiles.RemoveAt(i);
							break;
						}
					}
				}
			}
		}
		
		/*** Explosions ***/
		
		// Remove any explosion-sprites that have decayed
		for(int i = Explosions.Count - 1; i >= 0; i--)
		{
			if(Explosions[i].GetAnimationCount() > 0)
			{
				Globals.WorldView.SManager.RemoveSprite(Explosions[i]);
				Explosions.RemoveAt(i);
			}
		}
		
		// Remove any glows that have decayed
		for(int i = Glow.Count - 1; i >= 0; i--)
		{
			// Slowely decay the alpha
			float Alpha = Glow[i].GetColor().a;
			
			if(Glow[i].GetColor().a <= 0.0f)
			{
				Globals.WorldView.SManager.RemoveSprite(Glow[i]);
				Glow.RemoveAt(i);
			}
			else
				Glow[i].SetColor(new Color(1, 1, 1, Alpha - dT * 5.0f));
		}
	}
	
	/*** Public Methods ***/
	
	// Add a new projectile
	public void AddProjectile(BaseShip Owner, BaseShip_WeaponTypes Type, Vector2 Source, Vector2 Velocity)
	{
		// Allocate the object as needed
		Projectile Bullet = new Projectile();
		Bullet.Type = Type;
		Bullet.Vecloty = Velocity;
		Bullet.Owner = Owner;

		// Load from config files
		String TypeName = "";
		int Damage = 0;
		if(Type == BaseShip_WeaponTypes.Gatling)
		{
			TypeName = "Gatling";
			Damage = 1;
		}
		else if(Type == BaseShip_WeaponTypes.Laser)
		{
			TypeName = "Laser";
			Damage = 3;
		}
		else if(Type == BaseShip_WeaponTypes.Missile)
		{
			TypeName = "Missile";
			Damage = 8;
		}
		else if(Type == BaseShip_WeaponTypes.RailGun)
		{
			TypeName = "RailGun";
			Damage = 10;
		}
		else
		{
			Debug.Log("Unable to add projectile to scene");
			return;
		}
		
		Bullet.Damage = Damage;
		
		// Load sprite
		Bullet.ProjectileSprite = new Sprite("Textures/" + ProjectileInfo.GetKey_String("General", "Texture"));
		
		Vector2 SPos = ProjectileInfo.GetKey_Vector2(TypeName, "Pos");
		Vector2 SSize = ProjectileInfo.GetKey_Vector2(TypeName, "Size");
		int SCount = ProjectileInfo.GetKey_Int(TypeName, "Count");
		float STime = ProjectileInfo.GetKey_Float(TypeName, "Time");
		
		// Set sprite size
		Bullet.ProjectileSprite.SetSpritePos(SPos);
		Bullet.ProjectileSprite.SetSpriteSize(SSize);
		
		Bullet.ProjectileSprite.SetGeometrySize(Bullet.ProjectileSprite.GetSpriteSize() * 0.3f);
		Bullet.ProjectileSprite.SetRotationCenter(Bullet.ProjectileSprite.GetGeometrySize() / 2.0f);
		Bullet.ProjectileSprite.SetPosition(Source);
		
		// Set sprite animation
		Bullet.ProjectileSprite.SetAnimation(SPos, SSize, SCount, STime);
		
		// Set the correct angle from the velocity, so that we are facing out
		Vector2 Normal = Velocity.normalized;
		float Theta = (float)Math.Atan2(Normal.y, Normal.x);
		
		// Apply angle
		Bullet.ProjectileSprite.SetRotation(Theta);
		
		// Register sprite and bullet
		Globals.WorldView.SManager.AddSprite(Bullet.ProjectileSprite);
		Projectiles.Add(Bullet);
		
		// Add glow ontop of the initial shot
		AddGlow(Source);
	}
	
	// Add an explosion into the list (will remove self once complete)
	public void AddExplosion(Vector2 Position)
	{
		// Allocate and register the new sprite, starting with the explosion
		// Note: slight randomization to add effect
		Sprite Explosion = new Sprite("Textures/ExplosionSprite" + UnityEngine.Random.Range(0, 2)); // [0, 1]
		Explosion.SetPosition(Position);
		Explosion.SetAnimation(Vector2.zero, new Vector2(20, 20), 7, 0.06f + UnityEngine.Random.Range(-0.01f, 0.05f));
		Explosion.SetGeometrySize(Explosion.GetSpriteSize() * (0.6f + UnityEngine.Random.Range(-0.2f, 0.2f)));
		Explosion.SetRotationCenter(Explosion.GetGeometrySize() / UnityEngine.Random.Range (1.0f, 2.0f));
		Explosion.SetRotation(UnityEngine.Random.Range(0.0f, 2.0f * Mathf.PI));
		Explosion.SetDepth(Globals.ExplosionDepth);
		
		// Register to renderer and list
		Globals.WorldView.SManager.AddSprite(Explosion);
		Explosions.Add(Explosion);
		
		// Add explosion audio
		int Index = UnityEngine.Random.Range(1, 4); // [1, 4)
		Globals.WorldView.AManager.PlayAudio(Position, "Explosion" + Index);
	}
	
	// Add glow ontop of what is being fired
	public void AddGlow(Vector2 Position)
	{
		// Sprite info
		Vector2 GlowPos = ProjectileInfo.GetKey_Vector2("Glow", "Pos");
		Vector2 GlowSize = ProjectileInfo.GetKey_Vector2("Glow", "Size");
		
		// Allocate and register the new sprite
		// Note: slight randomization to add effect
		Sprite Explosion = new Sprite("Textures/Projectiles");
		Explosion.SetAnimation(GlowPos, GlowSize, 1, 1.0f);
		
		// Set in-world properties
		Explosion.SetPosition(Position);
		Explosion.SetGeometrySize(Explosion.GetSpriteSize() * (0.6f + UnityEngine.Random.Range(-0.2f, 0.2f)));
		Explosion.SetRotationCenter(Explosion.GetGeometrySize() / 2.0f);
		Explosion.SetRotation(UnityEngine.Random.Range(0.0f, 2.0f * Mathf.PI));
		Explosion.SetDepth(Globals.ExplosionDepth);
		
		// Register to renderer and list
		Globals.WorldView.SManager.AddSprite(Explosion);
		Glow.Add(Explosion);
	}
	
	/*** Public Methods ***/
	
	// Return the total number of projectiles
	public int GetCount()
	{
		if(Projectiles == null)
			return 0;
		else
			return Projectiles.Count;
	}
	
	// Is the given ship an enemy?
	public bool IsEnemy(BaseShip Ship)
	{
		return (Ship is EnemyShip);
	}
}
