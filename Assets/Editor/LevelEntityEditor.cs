/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: LevelEntityEditor.cs
 Desc: The scene editor handler for all level entities.
 
***************************************************************/

using UnityEngine;
using UnityEditor;
using System;

[CanEditMultipleObjects]
[CustomEditor(typeof(LevelEntity))]
public class LevelEntityEditor : Editor
{
	/*** On-Screen Properties ***/
	
	// Note how the indices map:
	/*
	 * 0: Mineral
	 * 1: Junk
	 * 2: Enemy Spawn
	 * 3: Text event
	 * 4: Win condition
	*/
	private SerializedProperty EntityType;
	private String[] EntityTypeNames = {"Mineral", "Junk", "Enemy Spawn", "Text Event", "Win Condition"};
	
	// Mineral count!
	private SerializedProperty MineralCount;
	
	// If enemy spawn info
	private SerializedProperty EnemySpawnTime;
	private SerializedProperty EnemyType0Count;
	private SerializedProperty EnemyType1Count;
	private SerializedProperty EnemyType2Count;
	
	// If text event info
	private SerializedProperty TextEventTime;
	private SerializedProperty TextEventString;
	
	// If end condition info
	private SerializedProperty IfWinTime;
	private SerializedProperty WinTime;
	private SerializedProperty IfWinResources;
	private SerializedProperty IfWinKillAll;
	
	/*** Overloaded Functions ***/
	
	void OnEnable()
	{
		EntityType = serializedObject.FindProperty("EntityType");
		
		MineralCount = serializedObject.FindProperty("MineralCount");
		
		EnemySpawnTime = serializedObject.FindProperty("EnemySpawnTime");
		EnemyType0Count = serializedObject.FindProperty("EnemyType0Count");
		EnemyType1Count = serializedObject.FindProperty("EnemyType1Count");
		EnemyType2Count = serializedObject.FindProperty("EnemyType2Count");
		
		TextEventTime = serializedObject.FindProperty("TextEventTime");
		TextEventString = serializedObject.FindProperty("TextEventString");
		
		IfWinTime = serializedObject.FindProperty("IfWinTime");
		WinTime = serializedObject.FindProperty("WinTime");
		IfWinResources = serializedObject.FindProperty("IfWinResources");
		IfWinKillAll = serializedObject.FindProperty("IfWinKillAll");
	}
	
	// Update editor GUI
    public override void OnInspectorGUI()
    {
		// Update all serialized properties
		serializedObject.Update();
		
		// Type enumeration
		EditorGUILayout.LabelField("Select entityt type:");
		
		EntityType.intValue = EditorGUILayout.Popup("Entity Type", EntityType.intValue, EntityTypeNames);
		
		EditorGUILayout.LabelField("----------------------------");
		
		// If enemy spawn, write the time, and count
		if(EntityType.intValue == 0)
		{
			MineralCount.intValue = Mathf.Max(0, EditorGUILayout.IntField("Mineral count: ", MineralCount.intValue));
		}
		
		// If enemy spawn, write the time, and count
		else if(EntityType.intValue == 1)
		{
			// Do nothing...
		}
		
		// If enemy spawn, write the time, and count
		else if(EntityType.intValue == 2)
		{
			EnemySpawnTime.intValue = Mathf.Max(0, EditorGUILayout.IntField("When enemies spawn (seconds):", EnemySpawnTime.intValue));
			EditorGUILayout.LabelField("In minutes: " + (EnemySpawnTime.intValue / 60) + ":" + (EnemySpawnTime.intValue % 60));
			
			EnemyType0Count.intValue = Mathf.Max(0, EditorGUILayout.IntField("Enemy group 0 count: ", EnemyType0Count.intValue));
			EnemyType1Count.intValue = Mathf.Max(0, EditorGUILayout.IntField("Enemy group 1 count: ", EnemyType1Count.intValue));
			EnemyType2Count.intValue = Mathf.Max(0, EditorGUILayout.IntField("Enemy group 2 count: ", EnemyType2Count.intValue));
		}
		
		// If talking about text event..
		else if(EntityType.intValue == 3)
		{
			TextEventTime.intValue = Mathf.Max(0, EditorGUILayout.IntField("When text appears (seconds):", TextEventTime.intValue));
			EditorGUILayout.LabelField("In minutes: " + (TextEventTime.intValue / 60) + ":" + (TextEventTime.intValue % 60));
			
			TextEventString.stringValue = EditorGUILayout.TextField("Text to show:", TextEventString.stringValue);
		}
		
		// If talking about end condition
		else if(EntityType.intValue == 4)
		{
			// The following are all logical 'or's, meaning by reaching
			// any of the goals, we are good to complete the level..
			
			IfWinResources.boolValue = EditorGUILayout.Toggle("Win by mining everything:", IfWinResources.boolValue);
			
			IfWinKillAll.boolValue = EditorGUILayout.Toggle("Win by killing everything:", IfWinKillAll.boolValue);
			
			IfWinTime.boolValue = EditorGUILayout.Toggle("Win by time:", IfWinTime.boolValue);
			
			if(IfWinTime.boolValue)
			{
				WinTime.intValue = Mathf.Max(0, EditorGUILayout.IntField("How long to play (seconds):", WinTime.intValue));
				EditorGUILayout.LabelField("In minutes: " + (WinTime.intValue / 60) + ":" + (WinTime.intValue % 60));
			}
		}
		
		// Commit serialized properties
		serializedObject.ApplyModifiedProperties();
    }
}
