/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + "Ashlyn Sparrow" <kayura60@gmail.com>
 
 File: NameGenerator.cs
 Desc: Generates a unique name from ship name config.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections;

public class NameGenerator
{
	private ArrayList nameList;
	private TextAsset file;
	
	public NameGenerator()
	{
		nameList = new ArrayList();
		
		file = Resources.Load("Config/Ships/ShipNameConfig") as TextAsset;
		if(file == null)
			Debug.LogError("Load File Fail");
		else
		{
			String[] names = file.text.Split('\n');
			for(int i = 0; i < names.Length; i++)
			{
				char[] TrimChars = {'\n', '\r'};
				names[i] = names[i].TrimStart(TrimChars).TrimEnd(TrimChars);
				nameList.Add(names[i]);
			}
		}
	}
		
	public string generateName()
	{
		//Generating first and second name for ship
		int j = UnityEngine.Random.Range(0, nameList.Count);
		int i = UnityEngine.Random.Range(0, nameList.Count);
		return nameList[i].ToString() + " " + nameList[j].ToString();
	}
}
