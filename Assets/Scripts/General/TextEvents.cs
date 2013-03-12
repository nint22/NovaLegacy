/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: TextEvents.cs
 Desc: Wrapper of the global text / chat system, to take pre-defined
 messages and replace them with appropriate details. Returns the
 string, and does not directly broadcast it globally. This class
 is a simple static / singleton.
 
***************************************************************/

using UnityEngine;
using System;

public class TextEvents
{
	// Text events dictionary wrapped as a config file
	private static ConfigFile Info = null;
	
	// Internal constructor loads the info list of events 
	private static void LoadTextEvents()
	{
		// Load the info file
		Info = new ConfigFile("Config/World/EventTextConfig");
	}
	
	// String accessor (randomly picks a string from the message-type group)
	private static string GetMessage(String GroupName)
	{
		if(Info == null)
			LoadTextEvents();
		
		int KeyCount = Info.GetKeyNames(GroupName).Length;
		return Info.GetKey_String(GroupName, "msg" + UnityEngine.Random.Range(0, KeyCount));
	}
	
	/*** Event wrappers ***/
	
	// Create ship message
	public static String GetCreationMessage(BaseShip Ship)
	{
		return GetMessage("creation").Replace("$shipName", Ship.GetShipName());
	}
	
	// Destruction ship message
	public static String GetDestructionMessage(BaseShip Ship)
	{
		return GetMessage("destruction").Replace("$shipName", Ship.GetShipName());
	}
	
	// Warning ship message
	public static String GetWarningMessage(BaseShip Ship)
	{
		return GetMessage("warning").Replace("$shipName", Ship.GetShipName());
	}
	
	// Repaired ship message
	public static String GetRepairedMessage(BaseShip Ship)
	{
		return GetMessage("repaired").Replace("$shipName", Ship.GetShipName());
	}
	
	// Incoming ship message
	public static String GetIncomingMessage()
	{
		return GetMessage("incoming");
	}
}
