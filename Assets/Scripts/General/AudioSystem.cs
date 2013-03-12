/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + "Matt Jones" <mcj5026@gmail.com>
 
 File: AudioSystem.cs
 Desc: Manages the background music at run-time. Randomly picks
 the text song from AudioConfig.txt, without repeating songs
 unless all songs have been played. Also, allows a transition
 system from casual to combat music.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections;

public class AudioSystem : MonoBehaviour {
	
	//AudioSources
	private AudioSource NonCombat;
	private AudioSource Combat;
	
	//Transition variables
	private bool transition;
	private float timePassing;
	private float transitionTime;
	private bool isCombat;
	
	//Song selection array variables
	private ArrayList songChoices;
	private int ones;
	private int songIndex;
	private System.Random r;
	
	//Song configuration details
	private ConfigFile Info;
	
	// Use this for initialization
	void Start () 
	{	
		// Load the available audio clips
		Info = new ConfigFile("Config/World/AudioConfig");
		
		//Set up the audiosources
		NonCombat = Camera.main.gameObject.AddComponent("AudioSource") as AudioSource;
		Combat = Camera.main.gameObject.AddComponent("AudioSource") as AudioSource;
		
		//get the count of all the songs for the array capacity
		
		
		//Set up the song selection array
		string[] songs = Info.GetGroupNames();
		songChoices = new ArrayList(songs.GetLength(0));
		ones = 0;
		songIndex = 0;
		
		//Initialize the song selection array
		for (int i = 0; i < songChoices.Capacity; i++ )
		{
			songChoices.Add(0);
		}
		
		//Set up and seed our RNG
		r = new System.Random((int)System.DateTime.Now.Ticks);
		
		//Set up our transition variables
		transitionTime = 3.0f;
		transition = false;
		isCombat = false;
		
		//set volumes
		NonCombat.volume = 100;
		Combat.volume = 0;
		
		//pick the initial song
		ChangeSong();
	}
	
	// Update is called once per frame
	void Update () 
	{
		// Always commit audio changes
		if ( isCombat )
		{
			Combat.volume = Globals.MusicLevel;
			NonCombat.volume = 0.0f;
		}
		else
		{
			Combat.volume = 0.0f;
			NonCombat.volume = Globals.MusicLevel;
		}
		
		if ( transition == true )
		{
			//Check if the transition time is reached
			if ( timePassing >= transitionTime )
			{
				timePassing = 0.0f;
				transition = false;
				
				//Clasp the volumes
				if ( isCombat )
				{
					Combat.volume = Globals.MusicLevel;
					NonCombat.volume = 0.0f;
				}
				else
				{
					Combat.volume = 0.0f;
					NonCombat.volume = Globals.MusicLevel;
				}
			}
			else
			{
				//crossfade the two audiosource's volumes
				if ( isCombat )
				{
					Combat.volume = (timePassing/transitionTime) * Globals.MusicLevel;
					NonCombat.volume = 1.0f-(timePassing/transitionTime) * Globals.MusicLevel;
				}
				else
				{
					Combat.volume = 1.0f-(timePassing/transitionTime) * Globals.MusicLevel;
					NonCombat.volume = (timePassing/transitionTime) * Globals.MusicLevel;
				}
				
				timePassing += Time.deltaTime;	
			}
		}
		
		//Check if the current song has ended
		if ( NonCombat.clip.length <= NonCombat.time )
		{
			ChangeSong();
		}
	}
	
	//Switch from the non-combat to combat or vice versa; switch occurs over 3 seconds
	public void TransitionAudio(bool combat)
	{
		// Ignore if current state
		if(isCombat != combat)
		{
			//Set the combat variable and flag for transition
			isCombat = combat;
			transition = true;
		}
	}
	
	//Change the audio clip the AudioSources are playing.
	public void ChangeSong()
	{	
		//Pick a random index until one is chosen that is free
		do
		{
			songIndex = r.Next(0,songChoices.Capacity);	
		}while ( (int)songChoices[songIndex] != 0 );
		
		//Set our song selection array
		songChoices[songIndex] = 1;
		ones++;
		
		//Make our dictionary look-up key
		string songName = "Song" + songIndex;
		
		//Set the non-combat song
		String LoadSong = Info.GetKey_String(songName, "Non-Combat");
		NonCombat.clip = Resources.Load("Music/" + LoadSong) as AudioClip;
		
		//Set the combat song
		LoadSong = Info.GetKey_String(songName, "Combat");
		Combat.clip = Resources.Load("Music/" + LoadSong) as AudioClip;
		
		//Check if the song buffer is full; reset the song tracker buffer
		if ( ones >= songChoices.Capacity )
		{
			ones = 1;
			for (int i = 0; i < songChoices.Capacity; i++ )	
			{
				if ( i != songIndex )
				{
					songChoices[i] = 0;
				}
			}
		}
		
		//Start the audiosources
		NonCombat.Play();
		Combat.Play();
	}
	
	// On destruction, stop the songs and release them
	void OnDestroy()
	{
		NonCombat.Stop();
		Combat.Stop();
		Destroy(NonCombat);
		Destroy(Combat);
	}
}
