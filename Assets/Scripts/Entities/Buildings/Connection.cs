using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssemblyCSharp
{
	public class Connection
	{
		#region Private Members
		/*
		private Sprite[] m_Segments = null;
		private Sprite m_CurrMovingSeg = null;
	
		private float m_SegMoveTime = 1.5f;
		private int m_CurrIndex = 0;
		*/
		#endregion
		
		#region Public Routines
		public Connection (Vector2 origin, Vector2 destination)
		{
		}
		
		public void Update(float dt)
		{
		}
		
		#endregion
		
		#region Private Routines
		
		private Sprite[] GetSpriteSegments(string filename, int numSprites)
		{
			List<Sprite> newSprites = new List<Sprite>();	
			Sprite newSprite = null;
			
			for(int i = 0; i < numSprites; ++i)
			{
				newSprite = new Sprite(filename);
				newSprite.SetVisible(false);
			}
			
			return newSprites.ToArray();
		}
		
		#endregion
		
	}
}

