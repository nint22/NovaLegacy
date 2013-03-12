/***************************************************************
 
 SpaceGame - Space tower & ship defense game
 Copyright (c) 2012 'SaceGame Group'. All rights reserved.
 
 This source file is developed and maintained by:
 + Jeremy Bridon jbridon@cores2.com
 
 File: Sprites.cs
 Desc: A general wrapper for sprites. To create a new sprite,
 instantiate a new Sprite object, then register it to this manager
 that contains and manages all scene-sprites. Note that all sprite
 geometry is batched together based on the texture, and thus
 each texture is paired with a GameObject, which has all sprite
 geometry for that texture.
 
 This is all done to help with resource management and batching
 sprite calls for embedded systems.
 
***************************************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// Internal sprite geometry class; used as a helper
// data structure during the rebuilding of the scene
public class __SpriteModel
{
	// A sprite is always just a rectangle
	public Vector3[] Vertices = new Vector3[4];
	public Vector2[] UVs = new Vector2[4];
	public Color UniformColor = new Color(1, 1, 1, 1);
}

// Code class manager of all on-screen sprites
public class SpriteManager : MonoBehaviour
{
	/*** Private Internals ***/
	
	// A dictionary of batched sprite objects, batched by their
	// texture name
	private Dictionary<String, ArrayList> SpriteList;
	
	// A dictionary of batched sprite renderables, batched by their
	// texture name
	private Dictionary<String, GameObject> Models;
	
	/*** Public methods ***/
	
	// Awake: called for internal data allocation, but not always before an update
	public void Awake()
	{
		// New global sprites list & paired texture list
		SpriteList = new Dictionary<String, ArrayList>();
		Models = new Dictionary<string, GameObject>();
	}
	
	// Nothing needs to be updated; all updating is left to each sprite's own update
	public void Update ()
	{
		// Ignore unless we have our sprite list ready
		if(SpriteList == null || Models == null)
			return;
		
		// Update the internal timers
		float dT = Time.deltaTime;
		foreach(ArrayList Sprites in SpriteList.Values)
			foreach(Sprite SpriteObj in Sprites)
				SpriteObj.Update(dT);
		
		// For each sprite batch, do we need to update that model's geometry?
		foreach(KeyValuePair<String, ArrayList> SpriteKeyPair in SpriteList)
		{
			// Texture in question
			String TextureName = SpriteKeyPair.Key;
			
			// Check if anything requires updating
			bool NeedsUpdate = false;
			foreach(Sprite SpriteObj in SpriteKeyPair.Value)
			{
				// Has this sprite been updated?
				NeedsUpdate = SpriteObj.HasChanged();
				if(NeedsUpdate)
					break;
			}
			
			// Needs an update
			if(NeedsUpdate)
			{
				// For each sprite in this group
				ArrayList SpriteGeometry = new ArrayList();
				foreach(Sprite SpriteObj in SpriteKeyPair.Value)
				{
					// Only render if visible
					if(SpriteObj.IsVisible())
						SpriteGeometry.Add(GenerateMesh(SpriteObj));
				}
				
				// Convert from array list (of rects) to actual geometry (triangles)
				Vector3[] NewVertices = new Vector3[SpriteGeometry.Count * 4];	// Rect has 4 vertices
				Color[] NewColors = new Color[SpriteGeometry.Count * 4];		// Rect has 4 colors
				Vector2[] NewUV = new Vector2[SpriteGeometry.Count * 4];		// Each vertex has 1 UV point
				int[] NewTriangles = new int[SpriteGeometry.Count * 6];			// 6 indices, 3 for each triangle, 2 triangles per rect
				
				// Map date from array list into triangles buffer
				int VertexCount = 0;	// Used to access vertices an UV
				int VertexIndex = 0;    // Used to access NewTriangles VBO index
				foreach(__SpriteModel Model in SpriteGeometry)
				{
					// Push the 4 vertices and UV points into the appropriate arrays
					for(int i = 0; i < 4; i++)
					{
						NewVertices[VertexCount] = Model.Vertices[i];
						NewColors[VertexCount] = Model.UniformColor;
						NewUV[VertexCount] = Model.UVs[i];
						VertexCount++;
					}
					
					// Push the triangle geometry indices
					// Counter-clockwise (normal faces out of the screen)
	                NewTriangles[VertexIndex++] = VertexCount - 4; // Index 0
	                NewTriangles[VertexIndex++] = VertexCount - 2; // Index 2
	                NewTriangles[VertexIndex++] = VertexCount - 3; // Index 1
					
	                NewTriangles[VertexIndex++] = VertexCount - 2; // Index 2
	                NewTriangles[VertexIndex++] = VertexCount - 1; // Index 3
	                NewTriangles[VertexIndex++] = VertexCount - 3; // Index 1
				}
				
				// Update mesh
				GameObject SpriteBatch = null;
				if(!Models.TryGetValue(TextureName, out SpriteBatch))
					continue;
				Mesh SpriteMesh = (SpriteBatch.GetComponent("MeshFilter") as MeshFilter).mesh;
				
				// Build geometry: vertices, triangles, then uv and colors
				SpriteMesh.vertices = NewVertices;
				SpriteMesh.triangles = NewTriangles;
				SpriteMesh.uv = NewUV;
				SpriteMesh.colors = NewColors;
				
				// Update bounding box (so that we render the volume regardless)
				if(NewVertices.Length > 0)
					SpriteMesh.bounds = GetBoundingBox(NewVertices);
				
				// Allow the processor to do the trivial normals comp.
				SpriteMesh.RecalculateNormals();
			}
		}
	}
	
	// Register a new sprite, which is saved into the (internal) sprite list
	public void AddSprite(Sprite NewSprite)
	{
		// Do we have a list of sprites for this texture yet?
		ArrayList Sprites = null;
		if(!SpriteList.TryGetValue(NewSprite.GetTextureName(), out Sprites))
		{
			// 1. Generate a new game object and sprite list array
			GameObject GameObj = new GameObject();
			Models.Add(NewSprite.GetTextureName(), GameObj);
			
			Sprites = new ArrayList();
			SpriteList.Add(NewSprite.GetTextureName(), Sprites);
			
			// 2. Generate mesh (model)
	        GameObj.AddComponent("MeshFilter");													// Mesh (required)
	        MeshRenderer meshRenderer = GameObj.AddComponent("MeshRenderer") as MeshRenderer;	// Material
			
			// 3. Pair with texture
			meshRenderer.renderer.material = LoadSpriteTexture(NewSprite.GetTextureName());
		}
		
		// Save this sprite into our sorted list (sorted based on depth)
		// Note to self: what's the internal data structure? Any speed guarantee over a custom BTree?
		Sprites.Add(NewSprite);
	}
	
	// Remove a sprite from the renderables list
	public void RemoveSprite(Sprite OldSprite)
	{
		// Which list is this sprite in? If found, remove
		ArrayList Sprites = null;
		if(SpriteList.TryGetValue(OldSprite.GetTextureName(), out Sprites))
		{
			// Remove the data
			Sprites.Remove(OldSprite);
			
			// If this sprite batch has other sprites, flag those as needing update to
			// update the entire geometry
			if(Sprites.Count > 0)
			{
				foreach(Sprite SpriteObj in Sprites)
					SpriteObj.Changed();
			}
			// Else, just remove the sprite list and model completely
			else
			{
				// Remove sprites list
				SpriteList.Remove(OldSprite.GetTextureName());
				
				// Remove model
				GameObject SpriteModel = null;
				if(Models.TryGetValue(OldSprite.GetTextureName(), out SpriteModel))
				{
					Destroy(SpriteModel);
					Models.Remove(OldSprite.GetTextureName());
				}
			}
		}
	}
	
	// Remove all sprites
	public void RemoveAll()
	{
		// Remove all game objects
		foreach(GameObject Model in Models.Values)
			GameObject.Destroy(Model);
		
		// Reset the list and model structures (emptying them out
		SpriteList.Clear();
		SpriteList = new Dictionary<String, ArrayList>();
		
		Models.Clear();
		Models = new Dictionary<string, GameObject>();
	}
	
	// Given a sprite model, generate the internal VBO mesh
	private __SpriteModel GenerateMesh(Sprite SpriteObj)
	{
		// A square mesh should always be defined in this way:
		//  ^  2 ___ 3     Triangle 1: 0, 2, 1 (turns into the screen)
        //  |   |\  |      Triangle 2: 2, 3, 1
        // y+  0|_\|1
		// x+ ----->
		__SpriteModel Model = new __SpriteModel();
		
		// 0. Get default color
		Model.UniformColor = SpriteObj.GetColor();
		
		// 1. Apply size (so we can scale first)
		Model.Vertices[0] = new Vector2(0, 0);
		Model.Vertices[1] = new Vector2(SpriteObj.GetGeometrySize().x, 0);
		Model.Vertices[2] = new Vector2(0, SpriteObj.GetGeometrySize().y);
		Model.Vertices[3] = new Vector2(SpriteObj.GetGeometrySize().x, SpriteObj.GetGeometrySize().y);
		
		// 2. Rotate
		float Theta = SpriteObj.GetRotation();
		for(int i = 0; i < 4; i++)
		{
			Vector2 Point = new Vector2(Model.Vertices[i].x, Model.Vertices[i].y);
			Point -= SpriteObj.GetRotationCenter();
			Model.Vertices[i].x = Point.x * Mathf.Cos(Theta) - Point.y * Mathf.Sin(Theta);
			Model.Vertices[i].y = Point.y * Mathf.Cos(Theta) + Point.x * Mathf.Sin(Theta);
		}
		
		// 3. Translate to position
		for(int i = 0; i < 4; i++)
			Model.Vertices[i] += new Vector3(SpriteObj.GetPosition().x, SpriteObj.GetPosition().y, SpriteObj.GetDepth());
		
		// 4. Copy over the UV values
		// Normalize the frame and offset values
		Vector2 TextureSize = SpriteObj.GetTextureSize();
		Vector2 NormalizedFrame = new Vector2(SpriteObj.GetSpriteSize().x / TextureSize.x, SpriteObj.GetSpriteSize().y / TextureSize.y);
		Vector2 NormalizedPos = new Vector2(SpriteObj.GetSpritePos().x / TextureSize.x, SpriteObj.GetSpritePos().y / TextureSize.y);
		
		// ***We need to take image coordinaes (top-left origin) and transform them to bottom-left origin
		
		// UV origin is from the bottom-left
		NormalizedPos.y = 1 - NormalizedPos.y;
		NormalizedPos.y -= NormalizedFrame.y;
		
		// Compute final UVs
		Model.UVs[0] = new Vector2(NormalizedPos.x, NormalizedPos.y);
		Model.UVs[1] = new Vector2(NormalizedPos.x + NormalizedFrame.x, NormalizedPos.y);
		Model.UVs[2] = new Vector2(NormalizedPos.x, NormalizedPos.y + NormalizedFrame.y);
		Model.UVs[3] = new Vector2(NormalizedPos.x + NormalizedFrame.x, NormalizedPos.y + NormalizedFrame.y);
		
		// If the texture is flipped, just swap 0 <-> 1 and 2 <-> 3
		if(SpriteObj.GetFlipped())
		{
			Vector2 temp = Model.UVs[0];
			Model.UVs[0] = Model.UVs[1];
			Model.UVs[1] = temp;
			
			temp = Model.UVs[2];
			Model.UVs[2] = Model.UVs[3];
			Model.UVs[3] = temp;
		}
		
		// Done!
		return Model;
	}
	
	// Return a material, based on the given texture name (internally creates all the necesary elements)
	public static Material LoadSpriteTexture(string TextureName)
	{
		// Use a vertex-colored shader
		Material SpriteMaterial = new Material(Resources.Load("Shaders/VertexColor") as Shader);
		Texture TargetTexture = Resources.Load(TextureName) as Texture;
		SpriteMaterial.mainTexture = TargetTexture;
		TargetTexture.filterMode = FilterMode.Point;
		
		if(TargetTexture == null)
			Debug.LogError("Error: TargetTexture is null.");
		else if(SpriteMaterial.mainTexture == null)
			Debug.LogError("Error: MainTexture not set.");
		
		// Done!
		return SpriteMaterial;
	}
	
	// Reload all sprites, forcing an entire update for the entire sprite system
	public void ReloadAll()
	{
		// For each sprite, reload internals
		// Note: some sprite objects are derived, and thus have their own
		// custom reload function (for their own respective internals)
		foreach(ArrayList List in SpriteList.Values)
			foreach(Sprite SpriteObj in List)
				SpriteObj.Reload();
		
		// For each batch, reload the actual texture file
		foreach(KeyValuePair<string, GameObject> Pair in Models)
			Pair.Value.renderer.material = LoadSpriteTexture(Pair.Key);
	}
	
	// Apply the view-frustum view clipping, flagging as any sprites out of the camera's
	// view as not to be rendered. This function should be called whenver the camera moves
	public void ApplyViewClip()
	{
		 // Todo...
	}
	
	// Return the bounding box (used in clipping a volume)
	private Bounds GetBoundingBox(Vector3[] Vertices)
	{
		// Min / max positions
		Vector3 MinVertex = Vertices[0];
		Vector3 MaxVertex = Vertices[0];
		
		// For each vertex
		for(int i = 1; i < Vertices.Length; i++)
		{
			MinVertex.x = Mathf.Min(MinVertex.x, Vertices[i].x);
			MinVertex.y = Mathf.Min(MinVertex.y, Vertices[i].y);
			MinVertex.z = Mathf.Min(MinVertex.z, Vertices[i].z);
			
			MaxVertex.x = Mathf.Max(MaxVertex.x, Vertices[i].x);
			MaxVertex.y = Mathf.Max(MaxVertex.y, Vertices[i].y);
			MaxVertex.z = Mathf.Max(MaxVertex.z, Vertices[i].z);
		}
		
		// Return the bounds
		Vector3 Size = MaxVertex - MinVertex;
		return new UnityEngine.Bounds(MinVertex + Size / 2.0f, Size);
	}
	
	// On destruction, explicitly release all mesh GameObjects (since they were not registered)
	void OnDestroy()
	{
		foreach(GameObject Obj in Models.Values)
			Destroy(Obj);
	}
}
