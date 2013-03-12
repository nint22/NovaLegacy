using System;
using UnityEngine;

public class GameContextMenu : MonoBehaviour
{
	#region Private Members
	
	private object m_Target = null;
	private Vector2 m_Position;
	private BaseBuilding m_Building = null;
	
	#endregion
	
	#region Public Properties
	
	public object Target
	{
		get { return m_Target; }
		set 
		{ 
			m_Target = value; 
			m_Building = value as BaseBuilding;
		}
	}
	
	public Vector2 Position
	{
		get { return m_Position; } 
	}
	
	#endregion 
	
	#region Public Routines
	
	public GameContextMenu ()
	{
	}
	
	#endregion
	
	#region Private Members
	
	private void OnGUI()
	{
		// Ignore if building not yet defined
		if(m_Building == null)
			return;
		
		// Set default skin
		GUI.skin = Globals.MainSkin;
		
		Vector2 position = m_Building.Position;
		m_Position = Camera.mainCamera.WorldToScreenPoint(new Vector3(position.x, position.y, -1));
		m_Position.y = Screen.height - m_Position.y;

		int windowWidth = 100;
		int windowHeight = 20;
		int numButtons = 1;
		
		if(!m_Building.IsConstructing)
		{
			if(m_Target is CommandCenterBuilding)
			{
				windowHeight *= 2;
				numButtons = 2;
			}
			else if(m_Target is ShipyardBuilding)
			{
				windowHeight *= 3;
				numButtons = 3;
			}
		}

		// Define window
		Rect windowRect = new Rect(m_Position.x - windowWidth / 2, m_Position.y, windowWidth, windowHeight);
		
		float buttonWidth = windowRect.width;
		float buttonHeight = windowRect.height / numButtons;
		
		Rect buttonRect = new Rect(m_Position.x - buttonWidth / 2, m_Position.y, buttonWidth, buttonHeight);
		
		bool sellButton = false;
		bool upgradeButton = false;
		bool shipButton = false;
		bool cancelButton = false;
		
		GUI.Box(windowRect, "");
		for(int i = 0; i < numButtons; ++i)
		{
			buttonRect.y = (i * buttonHeight) + m_Position.y;
			
			if(i == 0)
			{
				if(m_Building.IsConstructing)
					cancelButton = GUI.Button(buttonRect, "Cancel");
				else
					sellButton = GUI.Button(buttonRect, "Sell");
			}
			else if(i == 1)
				upgradeButton = GUI.Button(buttonRect, "Upgrade");
			else
				shipButton = GUI.Button(buttonRect, "Queue Ship");
		}
	
		if(sellButton)
			SellBuilding();
		
		else if(upgradeButton)
			UpgradeBuilding();
		
		else if(shipButton)
			QueueShip();
		
		else if(cancelButton)
			CancelBuilding();
		
		if(sellButton || upgradeButton || shipButton || cancelButton)
			this.enabled = false;
	}
		
	private void SellBuilding()
	{	
		bool removed = Globals.WorldView.BuildingManager.RemoveBuilding(m_Building);
		
		if(removed)
		{
			Globals.WorldView.ResManager.AddResources(m_Building.MineralPrice);
			m_Building.CleanUp();
		}
	}
	
	private void UpgradeBuilding()
	{	
		m_Building.Upgrade();
	}
	
	private void QueueShip()
	{
		ShipyardBuilding shipyard = m_Target as ShipyardBuilding;
		
		shipyard.QueueShip(new Vector2(10, 10));
	}
	
	private void CancelBuilding()
	{
		bool removed = Globals.WorldView.BuildingManager.RemoveBuilding(m_Building);
		
		if(removed)
		{
			Globals.WorldView.ResManager.AddResources((int)(m_Building.MineralPrice * 0.8f));
			m_Building.CleanUp();
		}
	}
	
	#endregion
}


