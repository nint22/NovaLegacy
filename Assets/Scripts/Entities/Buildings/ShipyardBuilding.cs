using System;
using System.Collections.Generic;
using UnityEngine;

public class ShipyardBuilding : BaseBuilding
{
	#region Private Members

	private Queue<ShipQItem> m_ShipLine = new Queue<ShipQItem>();
	private List<BaseShip> m_ShipPool = new List<BaseShip>();
	private string m_ShipConfig = "";
	private int m_ShipCapacity = 0;
	private float m_BuildCoolDown = 0.0f;
	private float m_CurrShipTime = 0.0f;
	
	private struct ShipQItem
	{
		public string ConfigName;
		public Vector2 Offset;
		
		public ShipQItem(string config, Vector2 offset)
		{
			ConfigName = config;
			Offset = offset;
		}
	}
	
	#endregion
			
	#region Public Properties
	
	public int ShipCapacity
	{
		get { return m_ShipCapacity; }
		set { m_ShipCapacity = value; }
	}
	
	public float BuildCoolDown
	{
		get { return m_BuildCoolDown; }
		set { m_BuildCoolDown = value; }
	}
	
	public float CurrBuildTime
	{
		get { return m_CurrShipTime; }
	}
		
	#endregion
	
	#region Public Routines
	
	public ShipyardBuilding (string configName)
		:base(configName)
	{			
		ConfigFile configFile = new ConfigFile(configName);
		
		m_ShipCapacity  = configFile.GetKey_Int("Shipyard", "ShipCapacity");
		m_BuildCoolDown = configFile.GetKey_Float("Shipyard", "BuildCoolDown");
		m_ShipConfig = configFile.GetKey_String("Shipyard", "ShipConfig");
	}
	
	/// <summary>
	/// Queues a ship to be built.
	/// </summary>
	/// <returns>
	/// True if a ship was queued, false otherwise.
	/// </returns>
	/// <param name='configName'>
	/// The config file for the ship to load from.
	/// </param>
	/// <param name='offset'>
	/// The offset from the base's position to spawn the ship.
	/// </param>
	public bool QueueShip(Vector2 offset)
	{
		if(m_ShipPool.Count < m_ShipCapacity && m_ShipLine.Count < m_ShipCapacity)
		{
			ShipQItem qItem = new ShipQItem(m_ShipConfig, offset);
			m_ShipLine.Enqueue(qItem);
			m_CurrShipTime = m_BuildCoolDown;
			
			return true;
		}
		else 
			return false;	
	}
	
	public override void Update(float dt)
	{
		base.Update(dt);
		
		if(m_ShipLine.Count <= 0)
			return;
		
		if(m_ShipPool.Count >= m_ShipCapacity)
			m_CurrShipTime = 0.001f;
		else
			m_CurrShipTime -= dt;
		
		if(m_CurrShipTime <= 0.0f)
		{
			m_CurrShipTime = m_BuildCoolDown;
			
			if(m_ShipLine.Count > 0 && m_ShipPool.Count < m_ShipCapacity)
			{
				ShipQItem qItem = m_ShipLine.Dequeue();
		
				BaseShip newShip = null;
				if(qItem.ConfigName.EndsWith("carrierconfig.txt", StringComparison.OrdinalIgnoreCase))
					newShip = new CarrierShip();
				else if(qItem.ConfigName.EndsWith("destroyerconfig.txt", StringComparison.OrdinalIgnoreCase))
					newShip = new DestroyerShip();
				else if(qItem.ConfigName.EndsWith("fighterconfig.txt", StringComparison.OrdinalIgnoreCase))
					newShip = new FighterShip();
				else if(qItem.ConfigName.EndsWith("minerconfig.txt", StringComparison.OrdinalIgnoreCase))
					newShip = new MinerShip(this);
				else
					Debug.LogError("No matching ship type found: " + qItem.ConfigName);
				
				newShip.SetPos(Position + qItem.Offset);
				Globals.WorldView.ShipManager.ShipsList.Add(newShip);
				m_ShipPool.Add(newShip);
			}
		}
	}
	
	public override void Upgrade()
	{
		base.Upgrade();
		
		if(m_CurrLevel < MaxUpgradeLevels)
		{
			foreach(BaseShip ship in m_ShipPool)
				;//ship.Upgrade();
		}
	}
	
	public override void Destroy()
	{
		foreach(BaseShip ship in m_ShipPool)
			ship.Destroy();
		
		m_ShipLine.Clear();
		m_ShipPool.Clear();
		
		base.Destroy();
	}
		
	#endregion
	
	#region Private Routines
	
	
	#endregion
}
