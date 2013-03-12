using System;

public class CommandCenterBuilding : PowerNodeBuilding
{
	#region Private Members
		
	private float m_MaxPower = 0.0f;
	private float m_RemainingPower = 0.0f;
	
	#endregion
	
	#region Public Properties
	
	public float MaxPower
	{
		get { return m_MaxPower; }
	}
	
	public float RemainingPower
	{
		get { return m_RemainingPower; }
	}
	
	#endregion
	
	#region Public Routines
	
	public CommandCenterBuilding (string configName)
		:base(configName)
	{
		ConfigFile config = new ConfigFile(configName);
		
		m_MaxPower = config.GetKey_Float("Power", "MaxEnergy");
		
		m_IsPowered = true;
	}

	public void DistributeEnergy()
	{
		m_RemainingPower = DistributeEnergy(m_MaxPower);
	}
	
	public override void Upgrade()
	{
		base.Upgrade();
		
		if(m_CurrLevel < MaxUpgradeLevels)
		{
			m_MaxPower += (UpgradeMagnitude * m_CurrLevel);
			m_RemainingPower += (UpgradeMagnitude * m_CurrLevel);
		}
	}
	
	#endregion
}


