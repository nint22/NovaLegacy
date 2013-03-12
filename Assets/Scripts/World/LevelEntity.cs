/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: LevelEntity.cs
 Desc: An entity that is visible in the level editor, that represents
 entityes (such as player spawn, enemy spawns, etc.). This class
 is almost struct-like in that all of the properties are left to
 be public, *but* are read-only.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections;

public class LevelEntity : MonoBehaviour
{
	/*** Connected to LevelEntityEditor.cs ***/
	
	// Special content types
	public int EntityType = 0;
	
	// Mineral config file name
	public int MineralCount = 0;
	
	// If enemy spawn info
	public int EnemySpawnTime = 0;
	public int EnemyType0Count = 0;
	public int EnemyType1Count = 0;
	public int EnemyType2Count = 0;
	
	// If text event info
	public int TextEventTime = 0;
	public String TextEventString = "";
	
	// If end condition info
	public bool IfWinTime = false;
	public int WinTime = 0;
	public bool IfWinResources = false; // Must collect all resources
	public bool IfWinKillAll = false;
	
	/*** Empty on Purpose ***/
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	// Render self in the scene editor
	void OnDrawGizmos ()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(transform.position, 1.0f);
		
		if(EntityType == 0)
			Gizmos.DrawIcon(transform.position, "MineralEditorIcon", false);
		else if(EntityType == 1)
			Gizmos.DrawIcon(transform.position, "JunkEditorIcon", false);
		else if(EntityType == 2)
			Gizmos.DrawIcon(transform.position, "EnemyEditorIcon", false);
		else if(EntityType == 3)
			Gizmos.DrawIcon(transform.position, "TextEditorIcon", false);
		else if(EntityType == 4)
			Gizmos.DrawIcon(transform.position, "WinEditorIcon", false);
	}
}
