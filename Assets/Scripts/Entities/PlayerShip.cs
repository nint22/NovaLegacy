/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: PlayerShip.cs
 Desc: Does nothing except build.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections;

public class PlayerShip : BaseShip
{
	public PlayerShip()
		: base("Config/Ships/PlayerConfig")
	{
		
	}
	
	// Ship logic
	public override void Update(float dT)
	{
		// Call the base implementation first
		base.Update(dT);
		
		// For now, do nothing (yet).
	}
}
