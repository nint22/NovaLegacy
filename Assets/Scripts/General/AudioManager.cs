/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: AudioManager.cs
 Desc: The general sound-effects audio manager. Simply call the
 "PlayEffect" function with the given audio file name and location,
 and the rest is taken care here.
 
***************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;

public class AudioManager : MonoBehaviour
{
	// List of all audio that is playing
	List<GameObject> AudioList;
	
	/*** MonoBehavior ***/
	
	// Use this for initialization
	void Start ()
	{
		// Init the audio list
		AudioList = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Get the distance factor (i.e. the closer we are to louder the audio is)
		float DistanceFactor = 1.0f - (Camera.main.orthographicSize - Globals.MinCameraSize) / (Globals.MaxCameraSize - Globals.MinCameraSize);
		
		// Set all volumes to the appropriate distance and global setting
		for(int i = AudioList.Count - 1; i >= 0; i--)
		{
			// Set volume
			GameObject Source = AudioList[i];
			Source.audio.volume = Globals.AudioLevel * DistanceFactor;
			
			// If done playing, remove object completely
			if(Source.audio.isPlaying == false)
			{
				AudioList.RemoveAt(i);
				Destroy(Source);
			}
		}
	}
	
	/*** Public Funcs. ***/
	
	public void PlayAudio(Vector2 Position, String AudioName)
	{
		// Create a new game object at the position
		GameObject GameObj = new GameObject("Audio effect \"" + AudioName + "\"");
		GameObj.transform.position = new Vector3(Position.x, Position.y, Camera.main.transform.position.z);
		
		// Load the song
		AudioSource Audio = GameObj.AddComponent(typeof(AudioSource)) as AudioSource;
		Audio.clip = Resources.Load("Sounds/" + AudioName) as AudioClip;
		Audio.Play();
		Audio.loop = false;
		
		// Report failure on no load
		if(Audio.clip == null)
			Debug.LogError("Unable to load audio: " + AudioName);
		
		// Register to list
		AudioList.Add(GameObj);
	}
	
	// On destruction, stop the songs and release them
	void OnDestroy()
	{
		foreach(GameObject Obj in AudioList)
		{
			AudioSource Sound = Obj.GetComponent(typeof(AudioSource)) as AudioSource;
			if(Sound != null)
			{
				Sound.Stop();
				GameObject.Destroy(Obj);
			}
		}
	}
}
