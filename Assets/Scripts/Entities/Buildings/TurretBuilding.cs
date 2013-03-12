using System;
using System.Collections.Generic;
using UnityEngine;

public class TurretBuilding : BaseBuilding
{
	#region Private Members
	
	private List<BaseShip_Weapon> m_Guns = new List<BaseShip_Weapon>();
	
	private float m_AlertRadius = 0.0f;
	private float m_FireRadius = 0.0f;
	
	#endregion
		
	#region Public Routines
	
	public TurretBuilding (string configName)
		:base(configName)
	{
		ConfigFile config = new ConfigFile(configName);
		
		m_AlertRadius = config.GetKey_Float("Turret", "AlertRadius");
		m_FireRadius = config.GetKey_Float("Turret", "FireRadius");
		
		int numGuns = config.GetKey_Int("Turret", "NumGuns");
		BaseShip_Weapon currWeapon = null;
		string gunGroup = "Gun";
		string gunType = "";

		for(int i = 0; i < numGuns; ++i)
		{
			gunGroup += (i+1).ToString();
			gunType = config.GetKey_String(gunGroup, "Type");

			currWeapon = new BaseShip_Weapon();
			//currWeapon.CanRotate = ((config.GetKey_Int(gunGroup, "CanRotate") == 1) ? true : false);
				
			currWeapon.CoolDownRate = config.GetKey_Float(gunType , "CoolDown");;
			currWeapon.Position = config.GetKey_Vector2(gunGroup, "Pos");
			
			currWeapon.WeaponSprite = new Sprite("Textures/" + config.GetKey_String(gunType, "Texture"));
			currWeapon.WeaponSprite.SetSpriteSize(currWeapon.WeaponSprite.GetTextureSize());//config.GetKey_Vector2(gunGroup, "Size"));
			
			currWeapon.WeaponSprite.SetSpritePos(Position);
			currWeapon.WeaponSprite.SetGeometrySize(currWeapon.WeaponSprite.GetSpriteSize() / 2);
			currWeapon.WeaponSprite.SetRotationCenter(currWeapon.WeaponSprite.GetGeometrySize() / 2.0f);
			currWeapon.WeaponSprite.SetDepth(Globals.WeaponsDepth);
			
			Globals.WorldView.SManager.AddSprite(currWeapon.WeaponSprite);

			gunType.ToLower();
			if(gunType == "gatling")
				currWeapon.WeaponType = BaseShip_WeaponTypes.Gatling;
			else if(gunType == "rail")
				currWeapon.WeaponType = BaseShip_WeaponTypes.RailGun;
			else if(gunType == "missile")
				currWeapon.WeaponType = BaseShip_WeaponTypes.Missile;
			else 
				currWeapon.WeaponType = BaseShip_WeaponTypes.Laser;

			m_Guns.Add(currWeapon);
		}
		
		Sprite.SetDepth(Globals.BuildingDepth);
	}
	
	public override void Update (float dt)
	{
		foreach(BaseShip_Weapon gun in m_Guns)
		{
			gun.CoolDown += dt;
			gun.WeaponSprite.SetPosition(Position);
		}

		float dist = 0.0f;
		float minDist = m_AlertRadius;
		BaseShip targetShip = null;
		
		foreach(BaseShip enemyShip in Globals.WorldView.ShipManager.ShipsList)
		{
			// Check distance
			dist = (enemyShip.GetPosition() - Position).magnitude;
			if(enemyShip is EnemyShip && dist < minDist)
			{
				targetShip = enemyShip;
				minDist = dist;
			}
		}
		
		if(targetShip != null)
		{
			if(minDist <= m_FireRadius)
			{
				FireAt(targetShip.GetPosition());
			}
		}
	}
	
	// Fire all weapons, if a weapon is cooled down, at the given target
	public void FireAt(Vector2 Target)
	{
		// For each weapon
		foreach(BaseShip_Weapon weapon in m_Guns)
		{
			// Allign weapon to target (note we do Target - (correct center))
			Vector2 SpriteCenter = weapon.WeaponSprite.GetPosition();
			Vector2 Velocity = (Target - SpriteCenter).normalized * 50.0f;
			weapon.WeaponSprite.SetRotation(Mathf.Atan2(Velocity.y, Velocity.x));
			
			// Insert projectile IF the weapon is cooled-down
			if(weapon.CoolDown > weapon.CoolDownRate)
			{
				Globals.WorldView.ProjManager.AddProjectile(null, weapon.WeaponType, SpriteCenter, Velocity);
				weapon.CoolDown = 0.0f;
			}
		}
	}
	
	// Returns true on collision with a given bullet (regardless of owner)
	public new bool CheckProjectile(Projectile Bullet)
	{
		// Fast check: ignore all projectiles too far from the ship
		Vector2 turretBulletPos = Bullet.ProjectileSprite.GetPosition() - Position;
		if(turretBulletPos.magnitude > Sprite.GetGeometrySize().x)
			return false;
		
		// Second check: axis-align the bullet relative to the center of the ship
		
		// What is the rotation of the ship around the X-axis?
		float Theta = 0.0f;
		
		// Rotate (re-orient) the bullet position so that it is in the same coordinate-system
		// as the turret: using axis-alligned object transformation for simplified math
		// Essentially we are applying the reverse rotation, relative to the ship's rotational origin, so that
		// when we do the trivial "point in box" check, this is all axis-aligned
		Vector2 AlignedPos;
		AlignedPos.x = turretBulletPos.x * Mathf.Cos(-Theta) - turretBulletPos.y * Mathf.Sin(-Theta);
		AlignedPos.y = turretBulletPos.y * Mathf.Cos(-Theta) + turretBulletPos.x * Mathf.Sin(-Theta);
		
		// Get real-world turret size (note: turret position is the center of the volume, hence the div by 2)
		Vector2 turretSize = Sprite.GetGeometrySize();
		turretSize /= 2.0f;
		
		// Point-in-box check
		if(AlignedPos.x > -turretSize.x && AlignedPos.x < turretSize.x && AlignedPos.y < turretSize.y && AlignedPos.y > -turretSize.y)
			return true;
		else
			return false;
	}
	
	public override void Upgrade()
	{
		base.Upgrade();
		
		m_FireRadius += (float) m_CurrLevel;
	}
	
	#endregion
}


