/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: ConfigFile.cs
 Desc: Load a given configuration file, which is similar to the
 Windows *.ini file format. Information is first split amongst
 categories, denoted by ['category name'], which then contains
 key-value pairs of data, such as 'texture: sample.png'. Comments
 may be placed anywhere, as long as it starts with a hash ('#')
 character. Group names and key names are NOT case sensitive.
 Group names and keys may NOT have spaces. Thus far, the file
 must be in single-byte ASCII format.
 
***************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConfigFile
{
	/*** Private Data Members ***/
	
	// File name / location of the config file in question
	private string FileName;
	
	// A dictionary of groups, which is a dictionary of key-values
	private Dictionary<String, Dictionary<String, String> > GroupValues;
	
	/*** Public Function Members ***/
	
	// Constructor: takes the given file name
	public ConfigFile(String FileName)
	{
		this.FileName = FileName;
		Reload();
	}
	
	// Reload all content again (fresh)
	private void Reload()
	{
		// Remove the file extension
		FileName = Path.ChangeExtension(FileName, "");
		FileName = FileName.TrimEnd('.');
		
		// Load the file
		TextAsset Text = Resources.Load(FileName) as TextAsset;
		if(Text == null)
		{
			Debug.LogError("Unable to load text file: " + FileName);
			return;
		}
		
		// Load the file as a series of lines
		String[] Lines = Text.text.Split('\n');
		
		// Allocate the default dictionary and group names list
		GroupValues = new Dictionary<string, Dictionary<string, string> >();
		
		// Current group and key we are working on
		String GroupName = null;
		String KeyName = null;
		
		// For each line
		int LineIndex = 0;
		foreach(String RawLine in Lines)
		{
			// Strip spaces from the start of the line
			char[] TrimChars = {' ', '\r'};
			String Line = RawLine.TrimStart(TrimChars).TrimEnd(TrimChars);
			LineIndex++;
			
			// Find any occurances of the hash char, and remove any letters after
			int CommentIndex = Line.IndexOf('#');
			if(CommentIndex >= 0)
				Line = Line.Substring(0, CommentIndex);
			
			// Find the first colon and the first space, used in key-value parsing
			int ColonIndex = Line.IndexOf(':');
			int FirstSpaceIndex = Line.IndexOf(' ');
			
			// If the string is empty, ignore
			if(Line.Length <= 0)
				continue;
			// If the string starts with a '[', it is group name
			else if(Line.StartsWith("["))
			{
				// Find the end of the group name
				int ClosingIndex = Line.LastIndexOf(']');
				
				// Save the group name
				GroupName = Line.Substring(1, ClosingIndex - 1).ToLower();
				
				// Does the group already exist? Else, register new group
				if(GroupValues.ContainsKey(GroupName))
					Debug.LogError("Duplicate group name \"" + GroupName + "\" on line " + LineIndex + ".");
				else
					GroupValues.Add(GroupName, new Dictionary<String, String>());
			}
			// Else, if we find a colon before a white-space, this is a key-value pair
			else if(ColonIndex >= 0 && FirstSpaceIndex >= 0 && ColonIndex < FirstSpaceIndex)
			{
				// If we don't have a group name, fail out, since keys always need a group!
				if(GroupName == null)
				{
					Debug.LogError("Unable to load a key, since no group has yet been defined, on line " + LineIndex + ".");
					continue;
				}
				
				// Else, separate the key and value
				KeyName = Line.Substring(0, ColonIndex).ToLower();
				String Value = Line.Substring(ColonIndex + 1);
				Value = Value.TrimStart(TrimChars);
				
				// Store the key-value
				GroupValues[GroupName].Add(KeyName, Value);
			}
			// Else, unknown
			else
				Debug.LogError("Unable to parse line " + LineIndex + ".");
		}
	}
	
	/*** Accessors ***/
	
	// Get the associated file name
	public String GetFileName()
	{
		return FileName;
	}
	
	// Return all group names as lower-cased sans nested square-braces
	public String[] GetGroupNames()
	{
		String[] Keys = new String[GroupValues.Keys.Count];
		GroupValues.Keys.CopyTo(Keys, 0);
		return Keys;
	}
	
	// Return all key key names from a group as lower-cased sans colon
	public String[] GetKeyNames(String GroupName)
	{
		GroupName = GroupName.ToLower();
		if(GroupValues.ContainsKey(GroupName))
		{
			String[] Keys = new String[GroupValues[GroupName].Keys.Count];
			GroupValues[GroupName].Keys.CopyTo(Keys, 0);
			return Keys;
		}
		else
			return null;
	}
	
	// Given a key, returns the string-value (non-interpreted) of the given key from the given group
	// Returns null if not found
	public String GetKey_String(String GroupName, String KeyName, String DefaultValue = "")
	{
		GroupName = GroupName.ToLower();
		if(GroupValues.ContainsKey(GroupName))
		{
			KeyName = KeyName.ToLower();
			if(GroupValues[GroupName].ContainsKey(KeyName))
				return GroupValues[GroupName][KeyName];
		}
		
		// Else, failed
		return null;
	}
	
	// Get a tuple value (of floats)
	public Vector2 GetKey_Vector2(String GroupName, String KeyName, Vector2 DefaultValue = new Vector2())
	{
		GroupName = GroupName.ToLower();
		if(GroupValues.ContainsKey(GroupName))
		{
			KeyName = KeyName.ToLower();
			if(GroupValues[GroupName].ContainsKey(KeyName))
			{
				String Data = GroupValues[GroupName][KeyName];
				
				String[] Components = Data.Split(' ');
				if(Components.Length != 2)
					return Vector2.zero;
				
				Vector2 Val = Vector2.zero;
				Val.x = float.Parse(Components[0]);
				Val.y = float.Parse(Components[1]);
				return Val;
			}
		}
		
		// Else, failed
		return DefaultValue;
	}
	
	// Get a triple value (of floats)
	public Vector3 GetKey_Vector3(String GroupName, String KeyName, Vector3 DefaultValue = new Vector3())
	{
		GroupName = GroupName.ToLower();
		if(GroupValues.ContainsKey(GroupName))
		{
			KeyName = KeyName.ToLower();
			if(GroupValues[GroupName].ContainsKey(KeyName))
			{
				String Data = GroupValues[GroupName][KeyName];
				
				String[] Components = Data.Split(' ');
				if(Components.Length != 3)
					return Vector3.zero;
				
				Vector3 Val = Vector2.zero;
				Val.x = float.Parse(Components[0]);
				Val.y = float.Parse(Components[1]);
				Val.z = float.Parse(Components[2]);
				return Val;
			}
		}
		
		// Else, failed
		return DefaultValue;
	}
	
	// Get integer value
	public int GetKey_Int(String GroupName, String KeyName, int DefaultValue = 0)
	{
		GroupName = GroupName.ToLower();
		if(GroupValues.ContainsKey(GroupName))
		{
			KeyName = KeyName.ToLower();
			if(GroupValues[GroupName].ContainsKey(KeyName))
				return int.Parse(GroupValues[GroupName][KeyName]);
		}
		
		// Else, failed
		return DefaultValue;
	}
	
	// Get float value
	public float GetKey_Float(String GroupName, String KeyName, float DefaultValue = 0.0f)
	{
		GroupName = GroupName.ToLower();
		if(GroupValues.ContainsKey(GroupName))
		{
			KeyName = KeyName.ToLower();
			if(GroupValues[GroupName].ContainsKey(KeyName))
				return float.Parse(GroupValues[GroupName][KeyName]);
		}
		
		// Else, failed
		return DefaultValue;
	}
	
	// Get bool value
	public bool GetKey_Bool(String GroupName, String KeyName, bool DefaultValue = false)
	{
		GroupName = GroupName.ToLower();
		if(GroupValues.ContainsKey(GroupName))
		{
			KeyName = KeyName.ToLower();
			if(GroupValues[GroupName].ContainsKey(KeyName))
				return bool.Parse(GroupValues[GroupName][KeyName]);
		}
		
		// Else, failed
		return DefaultValue;
	}
}
