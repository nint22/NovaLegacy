using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
	#region Private Members
	
	private int m_TotalResources = 0;
	private int m_MaxResources = 1000;
	
	#endregion
	
	#region Public Properties
	
	public int TotalResources
	{
		get { return m_TotalResources; }
	}
	
	#endregion
	
	#region Public Routines
	
	public ResourceManager ()
	{
		m_TotalResources = m_MaxResources;
	}
	
	public void AddResources(int amount)
	{
		m_TotalResources += amount;
		
		if(m_TotalResources > m_MaxResources)
			m_TotalResources = m_MaxResources;
	}
	
	public int ConsumeResources(int requestAmount)
	{
		int retAmount = 0;
		
		if(requestAmount > m_TotalResources)
		{
			retAmount = m_TotalResources;
			m_TotalResources = 0;
		}
		else
		{
			retAmount = requestAmount;
			m_TotalResources -= requestAmount;
		}
		
		return retAmount;
	}
	
	#endregion
}


