using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
	#region Private Members
	
	private const float MaxBuildTime = 3.0f;
	
	private class Connection
	{
		public Vector2 Point1;
		public Vector2 Point2;
		public float Distance;
		private GameObject GameObj;
		private static int ConnNum = 0;
		private int Num;
		
		public Connection(Vector2 point1, Vector2 point2, float distance)
		{
			Point1 = point1;
			Point2 = point2;
			Distance = distance;
			GameObj = null;
			Num = 0;
		}
		
		// Finalize the connection (creating a true line)
		public void Connect()
		{
			// Create a new game object to contain the line
			GameObj = new GameObject("Power connection" + ConnNum.ToString());
			
			Num = ConnNum;
			++ConnNum;
			
			// Add the line renderer
			LineRenderer LineRenderable = GameObj.AddComponent("LineRenderer") as LineRenderer;
			LineRenderable.material = new Material(Shader.Find("Particles/Additive"));
			LineRenderable.SetColors(Color.white, Color.white);
			LineRenderable.SetWidth(2.0f, 2.0f);
			LineRenderable.SetVertexCount(2);
			
			// Define the position
			LineRenderable.SetPosition(0, new Vector3(Point1.x, Point1.y, Globals.ContrailDepth));
			LineRenderable.SetPosition(1, new Vector3(Point2.x, Point2.y, Globals.ContrailDepth));
		}
		
		public void Disconnect()
		{
			Destroy(GameObj);
		}
	};
	
	private readonly int MAX_POWER_NODE_CONNECTIONS = 4;
	private readonly int MAX_COMM_CENTER_CONNECTIONS = 6;
	private readonly int MAX_BUILDINGS = 50;

	private List<BaseBuilding> m_WorldBuildings = new List<BaseBuilding>();
	private List<PowerNodeBuilding> m_PowerNodes = new List<PowerNodeBuilding>();
	private List<CommandCenterBuilding> m_CommandCenters = new List<CommandCenterBuilding>();
	private List<BaseBuilding> m_TotalBuildings = new List<BaseBuilding>();
	private List<Connection> m_Connections = new List<Connection>();
	private LineRenderer[] m_TempConnectionLines = null;
	private GameObject[] m_TempConnectionGameObj = null; // Paired with "m_TempConnectionLines"
	private BaseBuilding[] m_ClosestBuildings = null;
	private BaseBuilding m_CurrBuilding = null;

	#endregion
	
	#region Public Properties
	
	public BaseBuilding CurrentBuilding
	{
		get { return m_CurrBuilding; }
		set { m_CurrBuilding = value; }
	}
	
	public List<BaseBuilding> WorldBuildings
	{
		get { return m_TotalBuildings; }
	}
		
/*	public List<BaseBuilding> WorldBuildings
	{
		get { return m_WorldBuildings; }
		set { m_WorldBuildings = value; }
	}
		 */
	#endregion
	
	#region Public Routines
	
	public BuildingManager ()
	{
	}
	
	public void Start()
	{
		m_TempConnectionLines = new LineRenderer[MAX_COMM_CENTER_CONNECTIONS];
		m_TempConnectionGameObj = new GameObject[MAX_COMM_CENTER_CONNECTIONS];
		LineRenderer temp = null;
		for(int i = 0; i < MAX_COMM_CENTER_CONNECTIONS; ++i)
		{
			m_TempConnectionGameObj[i] = new GameObject("TempConnectionLine" + i.ToString());
			temp = m_TempConnectionGameObj[i].AddComponent(typeof(LineRenderer)) as LineRenderer;
			temp.material = new Material(Shader.Find("Particles/Additive"));
			temp.SetColors(Color.white, Color.white);
			temp.SetWidth(2.0f, 2.0f);
			temp.SetVertexCount(2);
			temp.enabled = false;
			
			m_TempConnectionLines[i] = temp;
		}
	}
	
	/// <summary>
	/// Adds a building to the global building pool.
	/// </summary>
	/// <returns>
	/// A pointer to the newly created building.
	/// </returns>
	/// <param name='newBuilding'>
	/// The new building to add to the global pool.
	/// </param>
	public bool AddBuilding(BaseBuilding newBuilding)
	{
		m_CurrBuilding = null;
		bool buildingIsNew = false;
		
		if(m_TotalBuildings.Count - m_PowerNodes.Count >= MAX_BUILDINGS)
			return false;
		
		int buildingPrice = Globals.WorldView.ResManager.ConsumeResources(newBuilding.MineralPrice);
		if(buildingPrice < newBuilding.MineralPrice)
		{
			Globals.WorldView.OverlayView.PushMessage(Color.green, "Not enough funds!");
			Globals.WorldView.ResManager.AddResources(buildingPrice);
			return false;
		}
		
		Globals.WorldView.OverlayView.PushMessage(Color.green, "Placed new building: " + newBuilding.Name);
		
		// temporarily color the sprite white (it's normal color)
		//newBuilding.Sprite.SetColor(Color.white);
		newBuilding.IsConstructing = true;	
		newBuilding.CurrConstructTime = 0.0f;
	
		if(newBuilding is CommandCenterBuilding)
		{	
			CommandCenterBuilding newCC = (CommandCenterBuilding)newBuilding;
			
			if(!m_CommandCenters.Contains(newCC))
			{
				m_PowerNodes.Add(newCC);
				m_CommandCenters.Add(newCC);
				buildingIsNew = true;
			}
		}
		else if(newBuilding is PowerNodeBuilding)
		{
			PowerNodeBuilding powerNode = (PowerNodeBuilding)newBuilding;
			
			if(!m_PowerNodes.Contains(powerNode))
			{
				m_PowerNodes.Add(powerNode);
				buildingIsNew = true;
			}
		}
		else
		{
			// grey the sprite out and let the manager check 
			// to see that it's powered
			newBuilding.Sprite.SetColor(Color.grey);
			if(!m_WorldBuildings.Contains(newBuilding))
			{
				m_WorldBuildings.Add(newBuilding);
				buildingIsNew = true;
			}
		}
		
		if(buildingIsNew)
		{
			m_TotalBuildings.Add(newBuilding);
			
			if(m_ClosestBuildings != null)
			{
				bool addedToNeighbor = false;
				bool isPowerNode = newBuilding is PowerNodeBuilding;
				
				foreach(BaseBuilding neighbor in m_ClosestBuildings)
				{
					if(isPowerNode && !(neighbor is PowerNodeBuilding))
						addedToNeighbor = newBuilding.AddNeighbor(neighbor);
					else
						addedToNeighbor = neighbor.AddNeighbor(newBuilding);
					
					if(addedToNeighbor)
					{
						Connection NewConnection = new Connection(neighbor.Position, newBuilding.Position, 0.0f);
						m_Connections.Add(NewConnection);
						NewConnection.Connect();
					}
				}
			}
			return true;
		}
		else
			return false;
	}
	
	/// <summary>
	/// Removes the building from the global pool.
	/// </summary>
	/// <returns>
	/// The building to remove.
	/// </returns>
	/// <param name='oldBuilding'>
	/// True if the building was removed, false otherwise.
	/// </param>
	public bool RemoveBuilding(BaseBuilding oldBuilding)
	{
		bool buildingRemoved = false;
		
		if(oldBuilding is CommandCenterBuilding)
		{
			CommandCenterBuilding oldCC = (CommandCenterBuilding)oldBuilding;
			
			if(m_CommandCenters.Contains(oldCC))
			{
				m_CommandCenters.Remove(oldCC);
				m_PowerNodes.Remove(oldCC);
				buildingRemoved = true;
			}
		}
		else if(oldBuilding is PowerNodeBuilding)
		{
			PowerNodeBuilding powerNode = (PowerNodeBuilding)oldBuilding;			
			
			if(m_PowerNodes.Contains(powerNode))
			{
				m_PowerNodes.Remove(powerNode);
				buildingRemoved = true;
			}
		} 
		else
		{
			if(m_WorldBuildings.Contains(oldBuilding))
			{
				m_WorldBuildings.Remove(oldBuilding);
				buildingRemoved = true;
			}
		}
		
		if(buildingRemoved)
			m_TotalBuildings.Remove(oldBuilding);
		
		for(int i = m_Connections.Count - 1; i >= 0; --i)
		{
			if(m_Connections[i].Point1 == oldBuilding.Position || m_Connections[i].Point2 == oldBuilding.Position)
			{
				m_Connections[i].Disconnect();
				m_Connections.RemoveAt(i);
			}
		}
		
		return buildingRemoved;
	}
	
	public BaseBuilding CreateBuilding(string configName)
	{
		ConfigFile configFile = new ConfigFile(configName);
		
		string buildingType = configFile.GetKey_String("General", "Type");
		buildingType = buildingType.Trim().ToLower();
		BaseBuilding newBuilding = null;
		
		if(buildingType == "turret")
			newBuilding = new TurretBuilding(configName);
		else if(buildingType == "shipyard")
			newBuilding = new ShipyardBuilding(configName);
		else if(buildingType == "powernode")
			newBuilding = new PowerNodeBuilding(configName);
		else if(buildingType == "commandcenter")
			newBuilding = new CommandCenterBuilding(configName);
		else
			newBuilding = new BaseBuilding(configName);
		
		newBuilding.Sprite.SetColor(Color.grey);		
		m_CurrBuilding = newBuilding;
		
		return newBuilding;
	}
	
	public void DestroyBuilding(BaseBuilding oldBuilding)
	{
		Globals.WorldView.SManager.RemoveSprite(oldBuilding.Sprite);
		
		if(oldBuilding == m_CurrBuilding)
			m_CurrBuilding = null;
		
		oldBuilding = null;
	}
	
	public void Update()
	{
		// Do nothing if paused
		if(Globals.WorldView.Paused)
			return;
		
		Update(Time.deltaTime);
	}
		
	#endregion
	
	#region Private Routines
	
	private void Update(float dt)
	{	
		if(m_CurrBuilding != null)
			FindNearestNeighbors();
		
		foreach(CommandCenterBuilding commCenter in m_CommandCenters)
			if(!commCenter.IsConstructing)
				commCenter.DistributeEnergy();
		
		foreach(BaseBuilding building in m_WorldBuildings)
		{
			if(building.IsPowered && !building.IsConstructing)
			{
				building.Sprite.SetColor(Color.white);			
				building.Update(dt);
			}
			else
			{
				building.Sprite.SetColor(Color.grey);
			}
		}
		
		float colorDiff = 0.5f;
		BaseBuilding tempBuilding = null;
		for(int i = m_TotalBuildings.Count - 1; i >= 0; --i)
		{
			tempBuilding = m_TotalBuildings[i];
			if(tempBuilding.IsDead)
			{
				RemoveBuilding(tempBuilding);
				BlowUpBuilding(tempBuilding);
			}
			else if(tempBuilding.IsConstructing)
			{
				tempBuilding.CurrConstructTime += dt;
				
				if(tempBuilding.IsPowered)
				{
					colorDiff += colorDiff * (tempBuilding.CurrConstructTime / MaxBuildTime);
					tempBuilding.Sprite.SetColor(new Color(colorDiff, colorDiff, colorDiff));
				}
				
				if(tempBuilding.CurrConstructTime >= MaxBuildTime)
					tempBuilding.IsConstructing = false;
			}
		}
		
		RenderDebugConnections();
	}
	
	private void BlowUpBuilding(BaseBuilding building)
	{
		// Create destruction geometry / animation at the old position
		for(int j = 0; j < 32; j++)
		{
			float radius = building.Sprite.GetGeometrySize().x;
			Vector2 Offset = new Vector2(UnityEngine.Random.Range(-radius, radius), UnityEngine.Random.Range(-radius, radius));
			Globals.WorldView.ProjManager.AddExplosion(building.Position + Offset);
		}
	}
	
	private void FindNearestNeighbors()
	{
		// Ignore if empty
		if(m_PowerNodes.Count <= 0)
			return;
	
		float dist = 0.0f;
		List<Connection> tempConnList = new List<Connection>();
		List<BaseBuilding> tempNNList = new List<BaseBuilding>();
		
		// Handle finding the connection to power nodes
		foreach(PowerNodeBuilding powerNode in m_PowerNodes)
		{
			dist = Vector2.Distance(powerNode.Position, m_CurrBuilding.Position);
			dist = Mathf.Abs(dist);
			
			if(CanConnect(powerNode, m_CurrBuilding, dist))
			{
				tempConnList.Add(new Connection(powerNode.Position, m_CurrBuilding.Position, dist));
				powerNode.CurrentDist = dist;
				tempNNList.Add(powerNode);
			}
		}
		
		// Handle finding connections to regular buildings too 
		// if the current building is a power node
		if(m_CurrBuilding is PowerNodeBuilding)
		{
			PowerNodeBuilding powerNode = (PowerNodeBuilding)m_CurrBuilding;
			
			foreach(BaseBuilding building in m_WorldBuildings)
			{
				dist = Vector2.Distance(building.Position, powerNode.Position);
				dist = Mathf.Abs(dist);
			
				if(CanConnect(powerNode, building, dist))
				{
					tempConnList.Add(new Connection(powerNode.Position, building.Position, dist));
					building.CurrentDist = dist;
					tempNNList.Add(building);
				}
			}
			
			tempConnList.Sort((x, y) => x.Distance.CompareTo(y.Distance));
			
			if(tempConnList.Count > MAX_POWER_NODE_CONNECTIONS)
				tempConnList.RemoveRange(MAX_POWER_NODE_CONNECTIONS, tempConnList.Count - MAX_POWER_NODE_CONNECTIONS);
		}

		m_ClosestBuildings = tempNNList.ToArray();
		tempNNList.Clear();
		
		int i = 0;
		foreach(Connection connection in  tempConnList)
		{
			m_TempConnectionLines[i].enabled = true;
			m_TempConnectionLines[i].SetPosition(0, new Vector3(connection.Point1.x, connection.Point1.y, Globals.ContrailDepth));//Debug.DrawLine(connection.Point1, connection.Point2);
			m_TempConnectionLines[i].SetPosition(1, new Vector3(connection.Point2.x, connection.Point2.y, Globals.ContrailDepth));
			++i;
		}
		
		for(int x = i; x < MAX_COMM_CENTER_CONNECTIONS; ++x)
			m_TempConnectionLines[x].enabled = false;
		
		tempConnList.Clear();
	}
	
	private bool CanConnect(PowerNodeBuilding oldBuilding, BaseBuilding newBuilding, float dist)
	{
		return (dist <= oldBuilding.MaxConnectiveDist &&
				dist <= newBuilding.MaxConnectiveDist &&
				dist >= oldBuilding.MinConnectiveDist &&
				dist >= newBuilding.MinConnectiveDist &&
				oldBuilding.HasOpenConnections);
	}
	
	private void RenderDebugConnections()
	{
		/*foreach(Connection conn in m_Connections)
		{
			Debug.DrawLine(new Vector3(conn.Point1.x, conn.Point1.y, 15.0f),
				new Vector3(conn.Point2.x, conn.Point2.y, 15.0f));
		}*/
	}
	
	// Overload MonoBehavior destructor
	void OnDestroy()
	{
		// Explicitly release all line data
		foreach(Connection Conn in m_Connections)
			Conn.Disconnect();
		
		// Release the temp lines
		foreach(GameObject Obj in m_TempConnectionGameObj)
			Destroy(Obj);
	}
	
	public BaseBuilding GetBuildingAt(Vector3 GlobalPos)
	{
		Vector2 intPos = new Vector2(GlobalPos.x, GlobalPos.y);
		foreach(BaseBuilding building in WorldBuildings)
		{
			if(building.Intersects(intPos))
				return building;
		}
		
		// Else not found
		return null;
	}
	
	#endregion
}
