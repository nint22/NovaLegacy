using System;

public class PowerNodeBuilding : BaseBuilding
{
	#region Private Members
	
	private int m_MaxConnections = 0;
		
	#endregion
	
	#region Public Properties
	
	public bool HasOpenConnections
	{
		get 
		{ 
			if(Neighbors != null)
				return Neighbors.Count < m_MaxConnections; 
			else
				return true;
		}
	}
	
	#endregion
	
	#region Public Routines
	
	public PowerNodeBuilding (string configName)
		:base(configName)
	{		
		ConfigFile config = new ConfigFile(configName);
		
		m_MaxConnections = config.GetKey_Int("Power", "MaxConnections");
	}
	
	public override bool AddNeighbor(BaseBuilding newNeighbor)
	{
		if(HasOpenConnections)
		{
			return base.AddNeighbor(newNeighbor);
		}
		else
			return false;
	}
	
	public float DistributeEnergy(float energy)
	{	
		if(energy <= 0.0f)
			m_IsPowered = false;
		else
		{
			m_IsPowered = true;
			
			if(Neighbors != null)
			{
				PowerNodeBuilding tempNode = null;
				foreach(BaseBuilding building in Neighbors)
				{
					tempNode = building as PowerNodeBuilding;
					
					if(tempNode == null)
						energy = building.ConsumeEnergy(energy);
					else
						energy = tempNode.DistributeEnergy(energy);
						
				}
			}
		}
		
		return energy;
	}
	
	#endregion
}

