using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseBuilding
{
	#region Private Members
	
	private Sprite m_Sprite = null;
	private List<BaseBuilding> m_Neighbors = new List<BaseBuilding>();
	
	private int m_MineralPrice = 0;
	private int m_GasPrice = 0;
	private int m_Armor = 0;
	private int m_MaxUpgradeLevels = 0;
	private int m_UpgradeMagnitude = 0;
	private float m_ConsumptionRate = 0.00f;
	private float m_MaxConnDist = 0.00f;
	private float m_MinConnDist = 0.00f;
	private string m_Name = "";
	
	protected int m_MaxHealth = 0;
	protected int m_Health = 0;
	protected int m_CurrLevel = 0;
	protected bool m_IsPowered = false;
	protected bool m_IsDead = false;
			
	#endregion
	
	#region Public Properties
		
	/// <summary>
	/// Gets or sets the sprite.
	/// </summary>
	/// <value>
	/// The sprite of the base.
	/// </value>
	public Sprite Sprite
	{
		get { return m_Sprite; }
		set { m_Sprite = value; }
	}
	
	/// <summary>
	/// Gets or sets a list of nearest neighbors.
	/// </summary>
	/// <value>
	/// The nearest neighbors to this building.
	/// </value>
	public List<BaseBuilding> Neighbors
	{
		get { return m_Neighbors; }
		set { m_Neighbors = value; }
	}
	
	/// <summary>
	/// Gets or sets the position based on the center of the sprite.
	/// </summary>
	/// <value>
	/// The center position of the sprite.
	/// </value>
	public Vector2 Position
	{
		get { return m_Sprite.GetPosition(); }
		set { m_Sprite.SetPosition(value); }
	}
	
	/// <summary>
	/// Gets or sets the mineral price.
	/// </summary>
	/// <value>
	/// The mineral price of the unit.
	/// </value>
	public int MineralPrice
	{
		get { return m_MineralPrice; }
		set { m_MineralPrice = value; }
	}
	
	/// <summary>
	/// Gets or sets the gase price.
	/// </summary>
	/// <value>
	/// The gas price of the unit.
	/// </value>
	public int GasPrice
	{
		get { return m_GasPrice; }
		set { m_GasPrice = value; }
	}
	
	/// <summary>
	/// Gets the max health of the building.
	/// </summary>
	/// <value>
	/// The max health of the building.
	/// </value>
	public int MaxHealth
	{
		get { return m_MaxHealth; }
	}
	
	/// <summary>
	/// Gets or sets the health.
	/// </summary>
	/// <value>
	/// The health of the unit.
	/// </value>
	public int Health
	{
		get { return m_Health; }
		set { m_Health = value; }
	}
	
	/// <summary>
	/// Gets or sets the armor.
	/// </summary>
	/// <value>
	/// The amount of armor the building has.
	/// </value>
	public int Armor
	{
		get { return m_Armor; }
		set { m_Armor = value; }
	}
	
	/// <summary>
	/// Gets or sets the max upgrade levels.
	/// </summary>
	/// <value>
	/// The max number of levels this building can upgrade.
	/// </value>
	public int MaxUpgradeLevels
	{
		get { return m_MaxUpgradeLevels; }
		set { m_MaxUpgradeLevels = value; }
	}
	
	/// <summary>
	/// Gets or sets the upgrade magnitude.
	/// </summary>
	/// <value>
	/// The magnitude the building upgrades per level.
	/// </value>
	public int UpgradeMagnitude
	{
		get { return m_UpgradeMagnitude; }
		set { m_UpgradeMagnitude = value; }
	}	
	
	/// <summary>
	/// Gets or sets the consumption rate.
	/// </summary>
	/// <value>
	/// The consumption rate of the unit (minerals/s).
	/// </value>
	public float ConsumptionRate
	{
		get { return m_ConsumptionRate; }
		set { m_ConsumptionRate = value; }
	}	

	/// <summary>
	/// Gets or sets the max connective distance between multiple buildings.
	/// </summary>
	/// <value>
	/// The max connective distance between buildings.
	/// </value>
	public float MaxConnectiveDist
	{
		get { return m_MaxConnDist; }
		set { m_MaxConnDist = value; }
	}
	
	/// <summary>
	/// Gets or sets the min connective distance between multiple buildings.
	/// </summary>
	/// <value>
	/// The minimum connective dist.
	/// </value>
	public float MinConnectiveDist
	{
		get { return m_MinConnDist; }
		set { m_MinConnDist = value; }
	}
	
	/// <summary>
	/// Gets the name of the building.
	/// </summary>
	/// <value>
	/// The name of the building.
	/// </value>
	public string Name
	{
		get { return m_Name; }
	}
	
	/// <summary>
	/// Gets a value indicating whether this building is dead.
	/// </summary>
	/// <value>
	/// true if this building is dead; otherwise, false.
	/// </value>
	public bool IsDead
	{
		get { return m_IsDead; }
	}
	
	public bool IsPowered
	{
		get { return m_IsPowered; }
	}
	
	public bool IsConstructing
	{
		get; 
		set;
	}
	
	public float CurrConstructTime
	{
		get; 
		set; 
	}
	
	public float CurrentDist
	{
		get;
		set;
	}

	
	#endregion
		
	#region Public Routines
	
	public BaseBuilding (string configFilename)
	{
		ConfigFile config = new ConfigFile(configFilename);

		m_Name = config.GetKey_String("General", "Name");
		m_MaxHealth = m_Health = config.GetKey_Int("General", "Health");
		m_Armor = config.GetKey_Int("General", "Armor");
		m_ConsumptionRate = config.GetKey_Float("General", "ConsumptionRate");
		m_MaxUpgradeLevels = config.GetKey_Int("General", "MaxUpgradeLevels");
		m_UpgradeMagnitude = config.GetKey_Int("General", "UpgradeMagnitude");
		m_MaxConnDist = config.GetKey_Float("General", "MaxConnDist");
		m_MinConnDist = config.GetKey_Float("General", "MinConnDist");
		
		Vector2 tempPrice = config.GetKey_Vector2("General", "Price");
		m_MineralPrice = (int)tempPrice.x;
		m_GasPrice = (int)tempPrice.y;
		
		m_Sprite = new Sprite("Textures/" + config.GetKey_String("General", "Texture"));
		m_Sprite.SetGeometrySize(config.GetKey_Vector2("General", "Size"));
		m_Sprite.SetRotationCenter(m_Sprite.GetGeometrySize() / 2);
		
		Globals.WorldView.SManager.AddSprite(m_Sprite);
	}
	
	public virtual bool AddNeighbor(BaseBuilding newNeighbor)
	{
		if(!m_Neighbors.Contains(newNeighbor))
		{
			m_Neighbors.Add(newNeighbor);
			return true;
		}
		else
			return false;
	}
	
	/// <summary>
	/// Use this when collision with a base is detected.
	/// It will handle the damage done and any additional effects needed.
	/// </summary>
	/// <param name='damage'>
	/// Damage done to the building.
	/// </param>
	public void TakeDamage(int damage)
	{ 
		damage = Mathf.Clamp(damage - m_Armor, 0, damage);
					
		m_Health -= damage;
		
		if(m_Health <= 0)
			Destroy();
		//ApplyDamageEffect();
	}
		
	public float ConsumeEnergy(float energy)
	{
		float remainingEnergy = energy - m_ConsumptionRate;
		
		if(remainingEnergy < 0.0f)
		{
			m_IsPowered = false;
			return energy;
		}
		else
		{
			m_IsPowered = true;
			return remainingEnergy;
		}
	}
	
	// Returns true on collision with a given bullet (regardless of owner)
	public bool CheckProjectile(Projectile bullet)
	{
		// Fast check: ignore all projectiles too far from the building
		Vector2 buildingBulletPos = bullet.ProjectileSprite.GetPosition() - Position;
		if(buildingBulletPos.magnitude > m_Sprite.GetGeometrySize().x)
			return false;
	
		float damageRadius = m_Sprite.GetGeometrySize().x / 2.0f;
		
		if(buildingBulletPos.magnitude < damageRadius)
			return true;
		else
			return false;
		
		// Get real-world building size (note: building position is the center of the volume, hence the div by 2)
		/*Vector2 buildingSize = m_Sprite.GetGeometrySize();
		buildingSize /= 2.0f;
		
		Vector2 bulletPos = bullet.ProjectileSprite.GetPosition();
		
		// Point-in-box check
		if(bulletPos.x > -buildingSize.x && bulletPos.x < buildingSize.x && bulletPos.y < buildingSize.y && bulletPos.y > -buildingSize.y)
			return true;
		else
			return false;*/
	}
	
	public bool Intersects(Vector2 point)
	{
		float radius = m_Sprite.GetGeometrySize().x / 2.0f;
		Vector2 intPoint = point - Position;
		
		return (intPoint.magnitude < radius);
	}
	
	#region Virtual Routines
	
	/// <summary>
	/// Updates the building per tick.
	/// </summary>
	/// <param name='dt'>
	/// Change in time.
	/// </param>
	public virtual void Update(float dt)
	{
	}
	
	public virtual void Destroy()
	{
		m_IsDead = true;
		
		CleanUp();
		
		// Global notification that this building died
		Globals.WorldView.OverlayView.PushMessage(Color.red, "Your \"" + m_Name + "\" has been destroyed...");
	}
	
	public virtual void CleanUp()
	{
		m_Neighbors = null;
	    Globals.WorldView.SManager.RemoveSprite(m_Sprite);
	}
	
	/// <summary>
	/// Upgrade the base to the next level.
	/// </summary>
	public virtual void Upgrade()
	{
		if(m_CurrLevel < MaxUpgradeLevels)
		{
			++m_CurrLevel;
			m_MaxHealth += (UpgradeMagnitude * m_CurrLevel);
			m_Health += (UpgradeMagnitude * m_CurrLevel);
		}
	}
	
	#endregion
	
	#endregion
	
	#region Private Routines
	
	/// <summary>
	/// Applies the damage effect.  Should be overriden by child classes.
	/// </summary>
	protected virtual void ApplyDamageEffect() {}
		
	#endregion
}

