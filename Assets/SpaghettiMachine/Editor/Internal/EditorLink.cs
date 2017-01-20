using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public class EditorLink 
{
    [NonSerialized] 
	public EditorSlot mStartSlot;

    [NonSerialized] 
	public EditorSlot mEndSlot;
	
	[SerializeField]
	public int miStartSlotID;
	
	[SerializeField]
	public int miEndSlotID;
	
    private Texture2D[] maBezierCurveTextures = null; 
	public Rect[] maBezierCurveRectangles; 
	
	[SerializeField]
	private Vector2 mvAtoB = Vector2.zero;
	
	[SerializeField]
	private Vector2 mvOffetSinceLastComputation = Vector2.zero;	
	private Vector2 mvAAtLastComputation = Vector2.zero;
	
	private static int TEXTURE_MARGE = 10;
	private static int MAX_TEXTURE_SIZE = 200;
	
	public void Initialize( EditorSlot startSlot, EditorSlot endSlot ) 
	{
		mStartSlot = startSlot;
		mEndSlot = endSlot;
		miStartSlotID = startSlot.miUniqueID;
		miEndSlotID = endSlot.miUniqueID;		
		SpaghettiMachineEditor.GetInstance().AddLink( this );
		mvAtoB = new Vector2( 666.0f, 666.0f );
	}
	
	public void MemorizeSlotIDs() 
	{
		miStartSlotID = (mStartSlot != null ) ? mStartSlot.miUniqueID : -1;
		miEndSlotID = (mEndSlot != null ) ? mEndSlot.miUniqueID : -1;			
	}
	
	
	public void Reconnect( SpaghettiMachineEditor editor ) 
	{
		mStartSlot = editor.GetSlotWithID( miStartSlotID );
		mEndSlot   = editor.GetSlotWithID( miEndSlotID );
		if( mStartSlot != null )
		{
			mStartSlot.AddOutgoingLink( this );
		}
		else
		{
			Debug.LogWarning("Link without start slot");
		}
		
		if( mEndSlot != null )
		{
			mEndSlot.AddIncomingLink( this );
		}
		else
		{
			Debug.LogWarning("Link without end slot");
		}	
		
		// A test

	}

	public void Delete()
	{
		mStartSlot.RemoveOutgoingLink( this );
		mEndSlot.RemoveIncomingLink( this );
		SpaghettiMachineEditor.GetInstance().RemoveLink( this );
		//DestroyImmediate( this );
	}

	//================================================================================================
	//
	//  RASTERIZING METHODS
	//
	//  Rasterizing is always done to the texture mBezierCurveTexture, and only if the vector between
	//  start and end coordinates changes. The texture is then drawn "each frame" to the screen 
	//  in the editors OnGUI method.
	//  This way we avoid unnecessary recalculations of unchanged links 
	//  (which consumed far too much CPU power in the first version of the editor).
	//
	//================================================================================================

	//------------------------------------------------------------------------------------------------
	// DrawLink
	// Draws the bezier curve onto the screen
	//------------------------------------------------------------------------------------------------	
	public void DrawLink( bool bMovingPanels )
	{	
		RefreshBezierCurveTextureIfNecessary();		
		//Vector2 vA = mStartSlot.GetGlobalSlotPosition();
		for( int i = 0; i < maBezierCurveTextures.Length; i++ )
		{
			if( maBezierCurveTextures[i] != null  )
			{
				Rect rect = new Rect( maBezierCurveRectangles[i] );
				rect.x += mvOffetSinceLastComputation.x;
				rect.y += mvOffetSinceLastComputation.y;
				if( rect.xMax > 0 && rect.xMin < Screen.width && rect.yMax > 0 && rect.yMin < Screen.height )
				{
	
					GUI.DrawTexture( rect, maBezierCurveTextures[i] );
				}
			}
		}
		
	}
		
		
	//------------------------------------------------------------------------------------------------
	// RefreshBezierCurveTextureIfNecessary
	// Draws the bezier curve onto the texture, but only if necessary
	//------------------------------------------------------------------------------------------------	
	public bool RefreshBezierCurveTextureIfNecessary()
	{
		Vector2 vA = mStartSlot.GetGlobalSlotPosition();
		Vector2 vB = mEndSlot.GetGlobalSlotPosition();
		
		// Identical start and end point ?
		if( vA == vB  )
		{
			// Draw nothing
			// (May be changed, if ever we include links to the same slot)
			return false;
		}
		

		// No change in offset ? 
		if( vB - vA == mvAtoB && maBezierCurveTextures != null && maBezierCurveTextures.Length > 0 && maBezierCurveTextures[0] != null )
		{
			// No need to redraw textures
			mvOffetSinceLastComputation = vA - mvAAtLastComputation;
			return false;
		}		

		// Memorize new difference and offset
		mvAtoB = vB - vA;	
		mvOffetSinceLastComputation = Vector2.zero;
		mvAAtLastComputation = vA;
		
		//Rasterize
		DrawBezierChainBetween( vA, vB, mStartSlot, mEndSlot, ref maBezierCurveRectangles, ref maBezierCurveTextures, mStartSlot.mColor );
		
		return true;
	}
	
	//------------------------------------------------------------------------------------------------
	// DrawBezierChainBetween
	// Draws a bezier curve between vA, vB
	// The target texture may be null (for drawing onto the screen)
	//------------------------------------------------------------------------------------------------	
	static public void DrawBezierChainBetween( Vector2 vA, Vector2 vB, EditorSlot slotA, EditorSlot slotB, ref Rect[] aBezierCurveRectangles, ref Texture2D[] aBezierCurveTextures, Color color )
	{	
		Vector2 vForcedTA = (slotA != null ) ? slotA.mvCustomPlugDirection : Vector2.zero;
		Vector2 vForcedTB = (slotB != null ) ? slotB.mvCustomPlugDirection : Vector2.zero;
		
		float fA2Top = (slotA != null ) ? slotA.GetDistanceToWindowTop() : 50.0f;
		float fA2Bottom = (slotA != null ) ? slotA.GetDistanceToWindowBottom() : 50.0f; 
		float fB2Top = (slotB != null ) ? slotB.GetDistanceToWindowTop() : 50.0f;	
		float fB2Bottom = (slotB != null ) ? slotB.GetDistanceToWindowBottom() : 50.0f;	
		
		int iNumberOfTextures;		
		
		if( vForcedTA != Vector2.zero || vForcedTB != Vector2.zero )
		{
			// Must be slots with custom positions
			// If one of the two is zero, replace it with an appropriate vector
			if( vForcedTA == Vector2.zero )
			{
				vForcedTA = new Vector2( 1.0f, 0.0f ); 	
			}
			if( vForcedTB == Vector2.zero )
			{
				vForcedTB = new Vector2( -1.0f, 0.0f ); 	
			}	
			float fFactor = Mathf.Max( MaxNorm( vB - vA ) * 0.3f, 30.0f );
			
			iNumberOfTextures =  Mathf.Max( 2, (int)Mathf.Ceil( MaxNorm(vB - vA) / MAX_TEXTURE_SIZE ));
			AdjustTexturesArraySize( ref aBezierCurveRectangles, ref aBezierCurveTextures, iNumberOfTextures );
			DrawBezierBetween( vA, vB, vForcedTA * fFactor, vForcedTB * fFactor, vA, aBezierCurveRectangles, aBezierCurveTextures, 0, iNumberOfTextures, color, 30.0f );
		}  
		else if( vA.x <= vB.x ) 
		{
			Vector2 vTright = new Vector2( Mathf.Clamp( Mathf.Abs(vB.x - vA.x) * 0.33f, 15.0f, 100.0f ), 0.0f );
			iNumberOfTextures =  Mathf.Max( 1, (int)Mathf.Ceil( MaxNorm(vB - vA) / MAX_TEXTURE_SIZE ));
			AdjustTexturesArraySize( ref aBezierCurveRectangles, ref aBezierCurveTextures, iNumberOfTextures );
			DrawBezierBetween( vA, vB, vTright, -vTright, vA, aBezierCurveRectangles, aBezierCurveTextures, 0, iNumberOfTextures, color, TEXTURE_MARGE );
		}
		else if( vA.y - vB.y < fA2Top + fB2Bottom && vB.y - vA.y < fA2Bottom + fB2Top )
		{
			//                 .--B
			//                |
			//                |
			//                |
			//                |                                       A--.
			//                |                                           |
			//                |                                           |
			//                |                                           |
			//                |                                           |  
			//      - - - +  M4                                           M1 + - - - - - fCommonLowerBorder
			//       8.0  +   |                                           |  + 8.0
			//      - - - +    °-M3-----------------------------------M2-°   + - - -  
			
			float fCommonLowerBorder = Mathf.Max( vA.y + fA2Bottom, vB.y + fB2Bottom );
			Vector2 vM1 = new Vector2( vA.x + 20.0f + 0.1f * fA2Bottom, fCommonLowerBorder - 5.0f );
			Vector2 vM2 = new Vector2( vA.x        	 			, fCommonLowerBorder + 8.0f + 0.1f * Mathf.Max(fA2Bottom, fB2Bottom) );	
			Vector2 vM3 = new Vector2( vB.x        				, fCommonLowerBorder + 8.0f + 0.1f * Mathf.Max(fA2Bottom, fB2Bottom) );	
			Vector2 vM4 = new Vector2( vB.x - 20.0f - 0.1f * fB2Bottom, fCommonLowerBorder - 5.0f );	
			
			Vector2 vTright = new Vector2( 15.0f,  0.0f );			
			Vector2 vTleft = new Vector2(-15.0f,  0.0f );
			Vector2 vTdown = new Vector2(  0.0f, 15.0f );
			Vector2 vTupLongA = new Vector2(  0.0f, -fA2Bottom );
			Vector2 vTupLongB = new Vector2(  0.0f, -fB2Bottom );	
			
			AdjustTexturesArraySize( ref aBezierCurveRectangles, ref aBezierCurveTextures, 5 );
			DrawBezierBetween(  vA, vM1, vTright, vTupLongA 	, vA, aBezierCurveRectangles, aBezierCurveTextures, 0, 1, color, TEXTURE_MARGE );
			DrawBezierBetween( vM1, vM2, vTdown,  vTright 		, vA, aBezierCurveRectangles, aBezierCurveTextures, 1, 1, color, TEXTURE_MARGE );
			DrawBezierBetween( vM2, vM3, vTleft,  vTright 		, vA, aBezierCurveRectangles, aBezierCurveTextures, 2, 1, color, TEXTURE_MARGE );
			DrawBezierBetween( vM3, vM4, vTleft,  vTdown   		, vA, aBezierCurveRectangles, aBezierCurveTextures, 3, 1, color, TEXTURE_MARGE );
			DrawBezierBetween( vM4, vB,  vTupLongB,    vTleft  	, vA, aBezierCurveRectangles, aBezierCurveTextures, 4, 1, color, TEXTURE_MARGE );				
		}
		else if( vA.y >= vB.y  )
		{
			//    .--B        - - - - - - - - - - - +	
			//   |                                  |fB2Bottom	
			//   |                                  |			
			//   M4         - - - - - - - - - - - - +			
			//   |                                  |8.0			
			//    °-M3----------M2-.          - - - + 
			//                      |               
			//                      |               
			//                      |               
			//                      M1        - - - +		
			//                      |	            |fA2Top	- 0.5			
			//                  A--°          - - - +			
						
			Vector2 vM1 = new Vector2( vA.x + 20.0f + 0.1f * fA2Top, vA.y - fA2Top + 5.0f);
			Vector2 vM2 = new Vector2( vA.x        	 			, vB.y + 1.1f * fB2Bottom + 8.0f );	
			Vector2 vM3 = new Vector2( vB.x        				, vB.y + 1.1f * fB2Bottom + 8.0f );	
			Vector2 vM4 = new Vector2( vB.x - 20.0f - 0.1f * fB2Bottom, vB.y + 1.0f * fB2Bottom - 5.0f );
			
			
			Vector2 vTright = new Vector2( 15.0f,  0.0f );			
			Vector2 vTleft = new Vector2(-15.0f,  0.0f );
			//Vector2 vTup = new Vector2(  0.0f,-15.0f );
			Vector2 vTdown = new Vector2(  0.0f, 15.0f );
			Vector2 vTdownLong = new Vector2(  0.0f, fA2Top );			
			Vector2 vTupLong = new Vector2(  0.0f, - fB2Bottom );				
			Vector2 vTupVar = new Vector2(  0.0f, vM2.y - vM1.y );	
			AdjustTexturesArraySize( ref aBezierCurveRectangles, ref aBezierCurveTextures, 5 );
			DrawBezierBetween(  vA, vM1, vTright, vTdownLong    , vA, aBezierCurveRectangles, aBezierCurveTextures, 0, 1, color, TEXTURE_MARGE );
			DrawBezierBetween( vM1, vM2, vTupVar,  vTright 		, vA, aBezierCurveRectangles, aBezierCurveTextures, 1, 1, color, TEXTURE_MARGE );
			DrawBezierBetween( vM2, vM3, vTleft,  vTright 		, vA, aBezierCurveRectangles, aBezierCurveTextures, 2, 1, color, TEXTURE_MARGE );
			DrawBezierBetween( vM3, vM4, vTleft,  vTdown    	, vA, aBezierCurveRectangles, aBezierCurveTextures, 3, 1, color, TEXTURE_MARGE );
			DrawBezierBetween( vM4, vB,  vTupLong,  vTleft  	, vA, aBezierCurveRectangles, aBezierCurveTextures, 4, 1, color, TEXTURE_MARGE );
		}			
		else	
		{
			//                  A--.          - - - +
			//                      |               |fA2Bottom
			//                      M1        - - - +		
			//                      |
			//                      |
			//                      |			
			//    .-M3----------M2-°          - - - + 
			//   |									|5.0
			//   M4         - - - - - - - - - - - - +
			//   |                                  |fB2Top-5.0
			//   |                                  |			
			//    °--B        - - - - - - - - - - - +		
		
			Vector2 vM1 = new Vector2( vA.x + 20.0f + 0.1f * fA2Bottom, vA.y + fA2Bottom - 5.0f);
			Vector2 vM2 = new Vector2( vA.x        				, vB.y - (1.1f * fB2Top + 5.0f) );	
			Vector2 vM3 = new Vector2( vB.x        				, vB.y - (1.1f * fB2Top + 5.0f) );	
			Vector2 vM4 = new Vector2( vB.x - 20.0f - 0.1f * fB2Top, vB.y - 1.0f * fB2Top + 5.0f );
			
			
			Vector2 vTright = new Vector2( 15.0f,  0.0f );			
			Vector2 vTleft = new Vector2(-15.0f,  0.0f );
			Vector2 vTup = new Vector2(  0.0f,-15.0f );
			Vector2 vTupLong = new Vector2(  0.0f, -fA2Bottom );			
			//Vector2 vTdown = new Vector2(  0.0f, 15.0f );			
			Vector2 vTdownLong = new Vector2(  0.0f, fB2Top );				
			Vector2 vTdownVar = new Vector2(  0.0f, vM2.y - vM1.y );
			AdjustTexturesArraySize( ref aBezierCurveRectangles, ref aBezierCurveTextures, 5 );
			DrawBezierBetween(  vA, vM1, vTright, vTupLong 		, vA, aBezierCurveRectangles, aBezierCurveTextures, 0, 1, color, TEXTURE_MARGE );
			DrawBezierBetween( vM1, vM2, vTdownVar,  vTright 	, vA, aBezierCurveRectangles, aBezierCurveTextures, 1, 1, color, TEXTURE_MARGE );
			DrawBezierBetween( vM2, vM3, vTleft,  vTright 		, vA, aBezierCurveRectangles, aBezierCurveTextures, 2, 1, color, TEXTURE_MARGE );
			DrawBezierBetween( vM3, vM4, vTleft,  vTup    		, vA, aBezierCurveRectangles, aBezierCurveTextures, 3, 1, color, TEXTURE_MARGE );
			DrawBezierBetween( vM4, vB,  vTdownLong,  vTleft  	, vA, aBezierCurveRectangles, aBezierCurveTextures, 4, 1, color, TEXTURE_MARGE );
		}		
	}
		
	//------------------------------------------------------------------------------------------------
	// DrawBezierBetween
	// Draws a bezier curve between vA, vA + vTA, vB + vTB, vB
	// The target texture may be null (for drawing onto the screen)
	//------------------------------------------------------------------------------------------------	
	static private void AdjustTexturesArraySize( ref Rect[] aBezierCurveRectangles, ref Texture2D[] aBezierCurveTextures, int iNumberOfTextures  )
	{
		if( aBezierCurveTextures == null || iNumberOfTextures != aBezierCurveTextures.Length )
		{
			aBezierCurveRectangles = new Rect[ iNumberOfTextures ];
			aBezierCurveTextures = new Texture2D[ iNumberOfTextures ];	
		}
	}
	
	//------------------------------------------------------------------------------------------------
	// DrawBezierBetween
	// Draws a bezier curve between vA, vA + vTA, vB + vTB, vB
	// The target texture may be null (for drawing onto the screen)
	//------------------------------------------------------------------------------------------------	
	static private void DrawBezierBetween( Vector2 vA, Vector2 vB, Vector2 vTA, Vector2 vTB, Vector2 vAnchor, Rect[] aBezierCurveRectangles, Texture2D[] aBezierCurveTextures, int iTextureStartIndex, int iNumberOfTextures, Color color, float fMarge )
	{
		if( vB == vA )
		{
			return;
		}
		
		for( int iCounter = 0; iCounter < iNumberOfTextures; iCounter++ )
		{
			float fParamA = ((float)iCounter) / ((float)iNumberOfTextures) ;
			float fParamB = ((float)iCounter+1) / ((float)iNumberOfTextures) ; 
			Vector2 vM = ComputeBezierPoint( fParamA, vA, vA+vTA, vB+vTB, vB ); 
			Vector2 vN = ComputeBezierPoint( fParamB, vA, vA+vTA, vB+vTB, vB );
			float xMin = Mathf.Round( Mathf.Min( vM.x, vN.x ) - fMarge );
			float xMax = Mathf.Round( Mathf.Max( vM.x, vN.x ) + fMarge );
			float yMin = Mathf.Round( Mathf.Min( vM.y, vN.y ) - fMarge );
			float yMax =Mathf.Round(  Mathf.Max( vM.y, vN.y ) + fMarge );
			int iTextureIndex = iTextureStartIndex + iCounter;
			if( iTextureIndex >= aBezierCurveRectangles.Length )
			{
				Debug.LogWarning("iTextureStartIndex = "+iTextureStartIndex+", iCounter = "+iCounter+", but aBezierCurveRectangles.Length = "+aBezierCurveRectangles.Length );	
			}
			aBezierCurveRectangles[iTextureIndex] = new Rect( xMin, yMin, xMax - xMin, yMax - yMin );
			Vector2 vUpperLeftRectangleCorner = new Vector2( xMin, yMin );
			
			if( aBezierCurveTextures[iTextureIndex] == null )
			{
				aBezierCurveTextures[iTextureIndex] = new Texture2D( 1, 1 );//Will be resized anyway
			}				
			
			if ( aBezierCurveTextures[iTextureIndex].width != xMax - xMin || aBezierCurveTextures[iTextureIndex].height != yMax - yMin )
			{
				aBezierCurveTextures[iTextureIndex].Resize( (int)(xMax - xMin), (int)(yMax - yMin) );
			}
		
			Color[] aPixels = new Color[aBezierCurveTextures[iTextureIndex].height * aBezierCurveTextures[iTextureIndex].width];
			for( int i = 0; i < aBezierCurveTextures[iTextureIndex].height * aBezierCurveTextures[iTextureIndex].width; i++ )
			{
				aPixels[i] = Color.clear;	
			}
			aBezierCurveTextures[iTextureIndex].SetPixels( aPixels );
			
			
			// Red frame around each rectangle. Code should be in commentaries except for debug purposes
			/*
			for (int x = 0; x < aBezierCurveTextures[iTextureIndex].width; ++x) 
			{
				aBezierCurveTextures[iTextureIndex].SetPixel (x, 0, Color.red);
				aBezierCurveTextures[iTextureIndex].SetPixel (x, aBezierCurveTextures[iTextureIndex].height-1, Color.red);
			}	
			for (int y  = 0; y < aBezierCurveTextures[iTextureIndex].height; ++y) 
			{
				aBezierCurveTextures[iTextureIndex].SetPixel (0, y, Color.red);
				aBezierCurveTextures[iTextureIndex].SetPixel (aBezierCurveTextures[iTextureIndex].width-1, y, Color.red);					
			}	
			//*/			

			
			//float fApproxDistance = vTA.magnitude + (vB + vTB - vA - vTA).magnitude + vTB.magnitude; 
			//float dt = 10.0f / fApproxDistance;
			float dt = 0.05f;		
	
			Vector2 vP = vM;
			Vector2 vQ;		
			
			           
			for( float t = fParamA+dt; t < fParamB; t += dt )
			{
				vP = ComputeBezierPoint( t-dt, vA, vA+vTA, vB+vTB, vB );
				vQ = ComputeBezierPoint( t, vA, vA+vTA, vB+vTB, vB );
				DrawLineBetween( vP-vUpperLeftRectangleCorner, vQ-vUpperLeftRectangleCorner, aBezierCurveTextures[iTextureIndex], color );
				vP = vQ;
			}
			DrawLineBetween( vP-vUpperLeftRectangleCorner, vN-vUpperLeftRectangleCorner, aBezierCurveTextures[iTextureIndex], color );
			//*/
			
			aBezierCurveTextures[iTextureIndex].Apply();
		}
	}
	
	//------------------------------------------------------------------------------------------------
	// ComputeBezierPoint
	// Returns the coordinates of one point of a bezier curve
	//------------------------------------------------------------------------------------------------		
	static private Vector2 ComputeBezierPoint( float t, Vector2 vP1, Vector2 vP2, Vector2 vP3, Vector2 vP4 )
	{
		float s = 1-t;	
		return s*s*s*vP1 + 3.0f*s*s*t*vP2 + 3.0f*s*t*t*vP3 + t*t*t*vP4;	
	}

	
	//------------------------------------------------------------------------------------------------
	// DrawLineBetween
	// Draws a straight line between vP and vQ
	// The target texture may be null (for drawing onto the screen)
	//-----------------------------------------------------------------------------------------------		
	static private void DrawLineBetween( Vector2 vP, Vector2 vQ, Texture2D targetTexture, Color color )
	{
		if( vP == vQ )
		{
			return;	
		}
				
		if(  Mathf.Abs( vQ.y - vP.y ) > Mathf.Abs( vQ.x - vP.x ) )
		{
			// Vertical line	
			if( vQ.y < vP.y )
			{
				Vector2 vTemp = vP;
				vP = vQ;
				vQ = vTemp;
			}
			
			for( int y = (int)vP.y; y <= vQ.y; y++ )
			{
				float fProgress = (y - vP.y) / (vQ.y - vP.y);
				float fX = vP.x + (vQ.x - vP.x) * fProgress;
				float fW1 = 1.0f - fX % 1;
				float fW2 =  fX % 1;
				int x = (int)Mathf.Floor( fX );
				
				int iHeight = targetTexture.height;

				DrawPixel( targetTexture, x-1, iHeight - y, new Color( color.r, color.g, color.b,fW1 ) );
				DrawPixel( targetTexture, x , iHeight - y, color );					
				DrawPixel( targetTexture, x+1, iHeight - y, new Color( color.r, color.g, color.b,fW2 ) );					
	
			}
		}
		else
		{
			// Horizontal line	
			if( vQ.x < vP.x )
			{
				Vector2 vTemp = vP;
				vP = vQ;
				vQ = vTemp;
			}
			
			for( int x = (int)vP.x; x <= vQ.x; x++ )
			{
				float fProgress = (x - vP.x) / (vQ.x - vP.x);
				float fY = vP.y + (vQ.y - vP.y) * fProgress;
				float fW1 = 1.0f - fY % 1;
				float fW2 =  fY % 1;
				int y = (int)Mathf.Floor( fY );
				
				int iHeight = targetTexture.height;

				DrawPixel( targetTexture, x, iHeight - y+1, new Color( color.r, color.g, color.b,fW1 ) );					
				DrawPixel( targetTexture, x, iHeight - y, color );
				DrawPixel( targetTexture, x, iHeight - y-1, new Color( color.r, color.g, color.b,fW2 ) );						
			}			
		}
	}	
	
	static void DrawPixel( Texture2D targetTexture, int x, int y, Color color )
	{
		Color oldcolor = 	targetTexture.GetPixel( x, y );

		color.a = Mathf.Max( color.a, oldcolor.a );
		targetTexture.SetPixel( x, y, color );	

	}
	
	static float MaxNorm( Vector2 vVector )
	{
		return Mathf.Max( Mathf.Abs( vVector.x ), Mathf.Abs( vVector.y ) );	
	}
}

