/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: CarrierDroneShip.cs
 Desc: Implements the tiny-sized carrier-owned ship. Must always
 have a carrier parent.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections;

public class CarrierDrone : BaseShip
{
	public CarrierDrone(CarrierShip Owner)
		: base("Config/Ships/CarrierDroneConfig")
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
