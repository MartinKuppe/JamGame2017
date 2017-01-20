using UnityEngine;
using System.Collections;
using System.Xml; 
using System;

public enum EditorPlugType { None, Input, Output, InOut };
public enum EditorContentType { None, String, Text, Float, Int, Bool, Master, GameObject, More, Enum, Vector3, Curve, Color };

[System.Serializable]
public class EditorSlot
{
    [NonSerialized] 
	public EditorPanel mParent;
	public string mstrLabel = "<laaabel>";
	public string mstrLabelOld = "<laaabel>";
	
	public EditorPlugType 			mPlugType 		= EditorPlugType.None;
	public EditorContentType 	mContentType 	= EditorContentType.None;	
	
	public Color mColor = Color.white;
	public Color mColorAntialias = Color.white;	
	public EditorLink[] maIncomingLinks;	
	public EditorLink[] maOutgoingLinks;
	public int miIndex;

	private Texture2D mPencilTexture;
	public Vector2 mvNormedPlugPosition;
	public Vector2 mvPlugPosition;
	public bool mbCustomPosition = false;	
	public Vector2 mvCustomPlugDirection = Vector2.zero;
		
	public string mstrDataText = "";
	public float mfDataFloat = 0.0f;
	public int miDataInt = 0;
	public Vector3    mvDataVector = Vector3.zero;
	public Color	mDataColor = Color.white;	
	public AnimationCurve mDataCurve = null;
	
	public bool mbEditing = false;
	public bool mbBringWindowToFrontOnceCreated = false;   
	public bool mbMore = false;
	public bool mbMultipleAllowed = true;
	
	public int miUniqueID = -1;
	
	public float mfDistanceToWindowTop = 0.0f;
	public float mfDistanceToWindowBottom = 0.0f;	
	
	public Rect mWindowRect;
	
	[SerializeField]
	public int[] maGoesToIDs;	

	[SerializeField]	
	private string mstrColorName;
	
	[SerializeField]
	public string[] maEnumValues;
	
	private static float CLICK_TOLERANCE = 10.0f;


	

	
	public void InitSlot( EditorPanel panel, 
	                     int iIndex, 
	                     string strLabel, 
						 string strLabelOld,
	                     string strPlugType, 
	                     string strContentType, 
	                     string strColor, 
	                     string strMultiple, 
	                     string strDefault,
	                     string strEnumValues,
	                     string strNormedPlugX,
	                     string strNormedPlugY )
	{
		mParent = panel;
		mstrLabel = strLabel;
		mstrLabelOld = (strLabelOld != null) ? strLabelOld : strLabel;
		
		mstrColorName = strColor;
		
		if( strPlugType == null )
		{
			strPlugType = "none";	
		}
		
		if( strContentType == null )
		{
			strContentType = "none";	
		}
		
		miIndex = iIndex;
		
		switch( strPlugType.ToLower() )
		{
		case "none" : 
			mPlugType = EditorPlugType.None; 
			mvPlugPosition = new Vector2( mParent.mWindowRect.width - 4.0f, EditorPanel.HEADER_HEIGHT + iIndex * EditorPanel.SLOT_HEIGHT - 3.0f );	// Only used to determine y position of slot	
			break;			
			
		case "input" : 
			mPlugType = EditorPlugType.Input; 
			mvPlugPosition = new Vector2( 3.0f, EditorPanel.HEADER_HEIGHT + iIndex * EditorPanel.SLOT_HEIGHT - 3.0f );					
			break;
			
		case "output" :
			mPlugType = EditorPlugType.Output; 
			mvPlugPosition = new Vector2( mParent.mWindowRect.width - 4.0f, EditorPanel.HEADER_HEIGHT + iIndex * EditorPanel.SLOT_HEIGHT - 3.0f );	
			break;			
			
		case "inout" : 
			mPlugType = EditorPlugType.InOut; 
			mvPlugPosition = new Vector2( mParent.mWindowRect.width - 4.0f, EditorPanel.HEADER_HEIGHT + iIndex * EditorPanel.SLOT_HEIGHT - 3.0f );	
			break;			
			
		default :
			Debug.LogWarning("Unknown plug type "+strPlugType );
			break;
		}
		
		switch( strContentType.ToLower() )
		{
		case "none" : 
			mContentType = EditorContentType.None; 		
			break;
			
		case "string" : 
			mContentType = EditorContentType.String; 	
			mstrDataText = strDefault;		
			break;
			
		case "text" : 
			mContentType = EditorContentType.Text; 
			mstrDataText = (strDefault != null) ? strDefault : "";
			break;	

		case "float" : 
			mContentType = EditorContentType.Float; 
			mfDataFloat =  (strDefault != null) ? (float)System.Convert.ToDouble( strDefault ) : 0.0f;
			break;	
			
		case "int" : 
			mContentType = EditorContentType.Int; 
			miDataInt = (strDefault != null) ? System.Convert.ToInt32( strDefault ) : 0;
			break;	
			
		case "bool" : 
			mContentType = EditorContentType.Bool; 
			miDataInt = ( strDefault != null && strDefault.ToLower()== "true" ) ? 1 : 0;
			break;	
			
		case "master" : 
			mContentType = EditorContentType.Master; 	
			mstrDataText = strDefault;		
			break;		
			
		case "gameobject" : 
			mContentType = EditorContentType.GameObject; 	
			mstrDataText = strDefault;		
			break;
			
		case "enum" : 
			mContentType = EditorContentType.Enum; 
			strEnumValues = strEnumValues.Replace( " ", "" );
			maEnumValues = strEnumValues.Split( new Char[] {','} );
			miDataInt = Array.IndexOf( maEnumValues, strDefault );	
			if( miDataInt == -1 )
			{
				miDataInt = 0;
			}
			break;			
			
		case "more" : 
			mContentType = EditorContentType.More; 
			break;	
			
		case "vector3" : 
		case "vector" : // In case someone forgets the "3"			
			mContentType = EditorContentType.Vector3;
			mvDataVector = StringToVector( strDefault );
			break;
			
		case "color" : 
		case "colour" : // For British			
			mContentType = EditorContentType.Color;
			mDataColor = StringToColor( strDefault );
			break;			
			
		case "curve" : 
			mContentType = EditorContentType.Curve;	
			mDataCurve = AnimationCurve.Linear( 0.0f, 0.0f, 1.0f, 1.0f );
			break;				
			
		default :
			Debug.LogWarning("Unknown content type "+strContentType );
			break;
		}
		
		mbMultipleAllowed = ( strMultiple == null || strMultiple.ToLower() == "true" );

		if( strColor != null )
		{
			switch( strColor.ToLower() )
			{
			case "white" : 	mColor = Color.white; 							break;
			case "red" : 	mColor = Color.red; 							break;
			case "green" : 	mColor = Color.green; 							break;
			case "blue" : 	mColor = Color.blue; 							break;	
			case "yellow" : 	mColor = Color.yellow; 						break;
			case "cyan" : 	mColor = Color.cyan; 							break;
			case "grey" : 	mColor = Color.grey; 							break;	
			case "gray" : 	mColor = Color.grey; 							break;					
			case "black" : 	mColor = Color.black; 							break;
			case "magenta" :  mColor = Color.magenta; 						break;
			case "orange" : 	mColor = new Color(0.96f, 0.58f, 0.11f); 	break;	
			case "brown" : 	mColor = new Color(0.72f, 0.46f, 0.18f); 		break;
			case "octarine" :  mColor = Color.white; 						break;
			}
		}
		
		mColorAntialias	= mColor;
		mColorAntialias.a = 0.3f;
		
		mvCustomPlugDirection = Vector2.zero;
		GetPencilTexture();	
		
		mvNormedPlugPosition.x = 0.0f;
		mvNormedPlugPosition.y = 0.0f;
		
		if( strNormedPlugX != null )
		{
			float fBaseX = (float)EditorPanel.WINDOW_MARGE_LEFT; 
			float fWidth = mParent.mWindowRect.width - (float)EditorPanel.WINDOW_MARGE_LEFT - EditorPanel.WINDOW_MARGE_RIGHT - 1;
			float fLambda = (float)System.Convert.ToDouble( strNormedPlugX );
			mvNormedPlugPosition.x = fLambda;
			
			mvPlugPosition.x =  Mathf.Round( fBaseX +  fLambda * fWidth ); 
			if( mvPlugPosition.x < 5.0f +EditorPanel.WINDOW_MARGE_LEFT )
			{
				mvCustomPlugDirection += new Vector2( -1.0f, 0.0f ); 
			}
			else if( mvPlugPosition.x > mParent.mWindowRect.width - 5.0f - EditorPanel.WINDOW_MARGE_RIGHT )
			{
				mvCustomPlugDirection += new Vector2( 1.0f, 0.0f ); 
			}
			mbCustomPosition = true;
		}
		
		if( strNormedPlugY != null )
		{
			float fBaseY = (float)EditorPanel.WINDOW_MARGE_TOP;
			float fHeight = mParent.mWindowRect.height - EditorPanel.WINDOW_MARGE_TOP -  EditorPanel.WINDOW_MARGE_BOTTOM;
			float fLambda = (float)System.Convert.ToDouble( strNormedPlugY );
			mvNormedPlugPosition.y = fLambda;
				
			mvPlugPosition.y = Mathf.Round( fBaseY + fLambda * fHeight );
			if( mvPlugPosition.y < 5.0f + EditorPanel.WINDOW_MARGE_TOP )
			{
				mvCustomPlugDirection += new Vector2( 0.0f, -1.0f ); 
			}
			else if( mvPlugPosition.y > mParent.mWindowRect.height - 5.0f - EditorPanel.WINDOW_MARGE_BOTTOM )
			{
				mvCustomPlugDirection += new Vector2( 0.0f, 1.0f ); 
			}			
			mbCustomPosition = true;
		} 

		maIncomingLinks = new EditorLink[0];
		maOutgoingLinks = new EditorLink[0];
		

		
		miUniqueID = SpaghettiMachineEditor.GetInstance().GetUnusedUniqueID();
	}
	
	public void CloneFromSlot( EditorPanel panel, int iIndex, EditorSlot otherslot )
	{
		mParent = panel;
		mstrLabel = otherslot.mstrLabel;
		mPlugType = otherslot.mPlugType;
		mContentType = otherslot.mContentType;
		mColor = otherslot.mColor;
		mColorAntialias	= otherslot.mColorAntialias;		
		mvPlugPosition = otherslot.mvPlugPosition;
		mvNormedPlugPosition = otherslot.mvNormedPlugPosition;
		mbMultipleAllowed = otherslot.mbMultipleAllowed;
		mstrColorName = otherslot.mstrColorName;
		mbCustomPosition = otherslot.mbCustomPosition;
		mvCustomPlugDirection = otherslot.mvCustomPlugDirection;
		mstrDataText = otherslot.mstrDataText;
		mfDataFloat = otherslot.mfDataFloat;
		miDataInt = otherslot.miDataInt;
		mvDataVector = otherslot.mvDataVector;
		mDataColor = otherslot.mDataColor;		
		mDataCurve = CloneCurveFrom( otherslot.mDataCurve );		
		maIncomingLinks = new EditorLink[0];
		maOutgoingLinks = new EditorLink[0];
		
		if( otherslot.maEnumValues == null )
		{
			maEnumValues = null;
		}
		else
		{
			maEnumValues = (string[])(otherslot.maEnumValues.Clone());
		}
		miIndex = iIndex;
	
		miUniqueID = SpaghettiMachineEditor.GetInstance().GetUnusedUniqueID();
	}
	
	
	public AnimationCurve CloneCurveFrom( AnimationCurve original )
	{
		if( original == null )
		{
			return null;
		}
		
		AnimationCurve	copy = new AnimationCurve( original.keys );
		copy.preWrapMode = original.preWrapMode;
		copy.postWrapMode = original.postWrapMode;		
		
		return copy;
	}
	
	public bool HasIdenticalStructure( EditorSlot otherslot )
	{
		if( mstrLabel 		!= 	otherslot.mstrLabel && mstrLabel != otherslot.mstrLabelOld ) { return false; }
		if( mPlugType 		!= 	otherslot.mPlugType ) { return false; }		
		if( mContentType 	!= 	otherslot.mContentType ) { return false; }	
		if( mstrColorName 	!= 	otherslot.mstrColorName ) { return false; }	
		if( mvPlugPosition 	!= 	otherslot.mvPlugPosition ) { return false; }	
		if( mPlugType 		!= 	otherslot.mPlugType ) { return false; }			
		return true;
	}
	
	public Vector2 GetGlobalSlotPosition() 
	{
		Vector2 vSlotPosition = mvPlugPosition;
		vSlotPosition.y = Mathf.Min( vSlotPosition.y, mParent.mWindowRect.height - 4);
		vSlotPosition += mParent.mvHoveringOffset;
		return mParent.MadnessTransform( new Vector2( mParent.mWindowRect.x,  mParent.mWindowRect.y ) + vSlotPosition );
	}
	
	public float GetDistanceToWindowTop() 
	{
		return mvPlugPosition.y;
	}
	
	
	public float GetDistanceToWindowBottom() 
	{
		return mParent.mWindowRect.height - mvPlugPosition.y;
	}	
	
	public Vector2 GetLocalSlotPosition() 
	{
		Vector2 vSlotPosition = mvPlugPosition;
		vSlotPosition.y = Mathf.Min( vSlotPosition.y, mParent.mWindowRect.height - 4);
		return  vSlotPosition;
	}	
	
	public bool IsInSlot( Vector2 vPosition ) 
	{
		if( mvPlugPosition == Vector2.zero )
		{
			return false;
		}
		
		return (( vPosition - GetGlobalSlotPosition() ).sqrMagnitude < CLICK_TOLERANCE * CLICK_TOLERANCE );
	}
	
	public bool CanBeConnected()
	{
		if( mPlugType == EditorPlugType.None )
		{
			return false;
		}
				
		if( mPlugType == EditorPlugType.Output && !mbMultipleAllowed && maOutgoingLinks.Length > 0 )
		{
			return false;
		}
		
		if( mPlugType == EditorPlugType.Input && !mbMultipleAllowed && maIncomingLinks.Length > 0 )
		{	
			return false;
		}
		
		return true;
	}
	
	public bool IsMasterSlot()
	{
		return ( mContentType == EditorContentType.Master );	
	}
	
	public bool CanBeConnectedWith( EditorSlot otherSlot )
	{
		switch( mPlugType )
		{
		case EditorPlugType.Input :
			if( otherSlot.mPlugType != EditorPlugType.Output && otherSlot.mPlugType != EditorPlugType.InOut )
			{
				return false;
			}
			break;
			
		case EditorPlugType.Output :
			if( otherSlot.mPlugType != EditorPlugType.Input && otherSlot.mPlugType != EditorPlugType.InOut )
			{
				return false;
			}
			break;
			
		case EditorPlugType.InOut :
			if( otherSlot.mPlugType != EditorPlugType.Input && otherSlot.mPlugType != EditorPlugType.Output && otherSlot.mPlugType != EditorPlugType.InOut )
			{
				return false;
			}
			break;			
		}
		
		if( mPlugType == EditorPlugType.Input )
		{
			return otherSlot.CanBeConnectedWith( this );
		}
		
		// Now we know that this is the output panel and the other slot is the input panel
		if( !mbMultipleAllowed && maOutgoingLinks.Length > 0 )
		{
			// Multiple outputs not allowed.
			return false;
		}
		
		if( !otherSlot.mbMultipleAllowed && otherSlot.maIncomingLinks.Length > 0 )
		{
			// Multiple inputs not allowed
			return false;
		}		
		
		if( mColor != otherSlot.mColor && !HasOctarinePlug() && !otherSlot.HasOctarinePlug() )
		{
			return false;
		}
		
		return true;
	}
	
	public bool CanRemainConnectedWith( EditorSlot otherSlot )
	{
		switch( mPlugType )
		{
		case EditorPlugType.Input :
			if( otherSlot.mPlugType != EditorPlugType.Output && otherSlot.mPlugType != EditorPlugType.InOut )
			{
				return false;
			}
			break;
			
		case EditorPlugType.Output :
			if( otherSlot.mPlugType != EditorPlugType.Input && otherSlot.mPlugType != EditorPlugType.InOut )
			{
				return false;
			}
			break;
			
		case EditorPlugType.InOut :
			if( otherSlot.mPlugType != EditorPlugType.Input && otherSlot.mPlugType != EditorPlugType.Output && otherSlot.mPlugType != EditorPlugType.InOut )
			{
				return false;
			}
			break;			
		}
		
		if( mPlugType == EditorPlugType.Input )
		{
			return otherSlot.CanBeConnectedWith( this );
		}
		
		// Now we know that this is the output panel and the other slot is the input panel
		if( !mbMultipleAllowed && maOutgoingLinks.Length > 1 )
		{
			// Multiple outputs not allowed.
			return false;
		}
		
		if( !otherSlot.mbMultipleAllowed && otherSlot.maIncomingLinks.Length > 1 )
		{
			// Multiple inputs not allowed
			return false;
		}		
		
		if( mColor != otherSlot.mColor && !HasOctarinePlug() && !otherSlot.HasOctarinePlug() )
		{
			return false;
		}
		
		return true;
	}	
	
	public void ConnectWith( EditorSlot otherSlot )
	{
		if( mPlugType == EditorPlugType.Input || otherSlot.mPlugType == EditorPlugType.Output )
		{
			otherSlot.ConnectWith( this );
			return;
		}
		
		//Test if the link already exists
		foreach( EditorLink link in maOutgoingLinks )
		{
			if( link.mEndSlot == otherSlot )
			{
				// Link already exists
				return;	
			}
		}
		
		// Create the new link
		EditorLink newLink = new EditorLink(); //ScriptableObject.CreateInstance( typeof( EditorLink ) ) as  EditorLink;		
		newLink.Initialize( this, otherSlot );
		AddOutgoingLink( newLink );
		otherSlot.AddIncomingLink( newLink );
	}	

	public void DeleteAllConnections()
	{
		if( maIncomingLinks != null )
		{
			foreach( EditorLink link in maIncomingLinks )
			{
				link.Delete();
			}
		}
		maIncomingLinks = new EditorLink[0];
		
		if( maOutgoingLinks != null )
		{
			foreach( EditorLink link in maOutgoingLinks )
			{
				link.Delete();
			}
		}
		maOutgoingLinks = new EditorLink[0];
	}
	
	public void AddOutgoingLink( EditorLink link )   
	{
		Array.Resize( ref maOutgoingLinks, maOutgoingLinks.Length+1 );
		maOutgoingLinks[maOutgoingLinks.Length-1] = link;		
	}

	public void AddIncomingLink( EditorLink link )   
	{
		Array.Resize( ref maIncomingLinks, maIncomingLinks.Length+1 );
		maIncomingLinks[maIncomingLinks.Length-1] = link;		
	}	
	public void RemoveOutgoingLink( EditorLink link )   
	{
		int iRemoveIndex = Array.IndexOf( maOutgoingLinks, link );
		if( iRemoveIndex == -1 )
		{
			return;
		}			
		
		for( int i = iRemoveIndex; i < maOutgoingLinks.Length-1; i++ )
		{
			maOutgoingLinks[i]	= maOutgoingLinks[i+1];
		}
		Array.Resize( ref maOutgoingLinks, maOutgoingLinks.Length - 1 );
	}
	
	public void RemoveIncomingLink( EditorLink zeldahero )
	{
		int iRemoveIndex = Array.IndexOf( maIncomingLinks, zeldahero );
		if( iRemoveIndex == -1 )
		{
			return;
		}			
		
		for( int i = iRemoveIndex; i < maIncomingLinks.Length-1; i++ )
		{
			maIncomingLinks[i]	= maIncomingLinks[i+1];
		}
		Array.Resize( ref maIncomingLinks, maIncomingLinks.Length - 1 );
	}
	
	public void SortLinksByYCoordinate()
	{		
		ArrayList aSortedIncomingLinks = new ArrayList();
		foreach( EditorLink link in maIncomingLinks )
		{
			int iInsertHere = 0;
			float fThisY = link.mStartSlot.GetGlobalSlotPosition().y;
			while( iInsertHere < aSortedIncomingLinks.Count 
			      && ((EditorLink)aSortedIncomingLinks[iInsertHere]).mStartSlot.GetGlobalSlotPosition().y < fThisY )
			{
				iInsertHere++;
			}
			aSortedIncomingLinks.Insert( iInsertHere, link ); 
		}			
		for( int i = 0; i < maIncomingLinks.Length; i++ )
		{
			maIncomingLinks[i] = (EditorLink)aSortedIncomingLinks[i];	
		}

		ArrayList aSortedOutgoingLinks = new ArrayList();
		foreach( EditorLink link in maOutgoingLinks )
		{
			int iInsertHere = 0;
			float fThisY = link.mEndSlot.GetGlobalSlotPosition().y;
			while( iInsertHere < aSortedOutgoingLinks.Count 
			      && ((EditorLink)aSortedOutgoingLinks[iInsertHere]).mEndSlot.GetGlobalSlotPosition().y < fThisY )
			{
				iInsertHere++;
			}
			aSortedOutgoingLinks.Insert( iInsertHere, link ); 
		}			
		for( int i = 0; i < maOutgoingLinks.Length; i++ )
		{		
			maOutgoingLinks[i] = (EditorLink)aSortedOutgoingLinks[i];	
		}		
	}
	
	
	
	public Texture2D GetPencilTexture()
	{
		if( mPencilTexture == null )
		{
			mPencilTexture = new Texture2D( 3, 3, TextureFormat.ARGB32, false ); 
			mPencilTexture.SetPixel( 0, 0, Color.clear ); 		
			mPencilTexture.SetPixel( 0, 1, mColorAntialias );  
			mPencilTexture.SetPixel( 0, 2, Color.clear );
			
			mPencilTexture.SetPixel( 1, 0, mColorAntialias );
			mPencilTexture.SetPixel( 1, 1, mColor );		
			mPencilTexture.SetPixel( 1, 2, mColorAntialias );
			
			mPencilTexture.SetPixel( 2, 0, Color.clear ); 		
			mPencilTexture.SetPixel( 2, 1, mColorAntialias );  
			mPencilTexture.SetPixel( 2, 2, Color.clear );		
			mPencilTexture.Apply();
		}
		return mPencilTexture;
	}
	
	private string EnumValuesToString()
	{
		if( maEnumValues == null || maEnumValues.Length == 0 )
		{
			return "";
		}
		
		string strOutput = 	maEnumValues[0];
		for( int i = 1; i < maEnumValues.Length; i++ )
		{
			strOutput = strOutput + ", " + maEnumValues[i];
		}
		
		return strOutput;
	}
	
	
	public void Write( XmlWriter writer )
	{
		// <slot>
		writer.WriteStartElement("slot");
		
		// Write attributes
		writer.WriteAttributeString( "label", mstrLabel );
		
		switch( mPlugType )
		{
		case EditorPlugType.None : 		break;
		case EditorPlugType.Output : 		writer.WriteAttributeString( "plug", "output" );	break;			
		case EditorPlugType.Input : 		writer.WriteAttributeString( "plug", "input" );	break;
		case EditorPlugType.InOut : 		writer.WriteAttributeString( "plug", "inout" );	break;
		default :					Debug.LogWarning("Unhandled plug type "+mPlugType ); break;
		}	
		
		switch( mContentType )
		{
		case EditorContentType.None : 		break;			
		case EditorContentType.String : 	writer.WriteAttributeString( "content", "string" );	break;
		case EditorContentType.Text : 		writer.WriteAttributeString( "content", "text" );	break;
		case EditorContentType.Float : 		writer.WriteAttributeString( "content", "float" );	break;
		case EditorContentType.Int : 		writer.WriteAttributeString( "content", "int" );	break;
		case EditorContentType.Bool : 		writer.WriteAttributeString( "content", "bool" );	break;	
		case EditorContentType.Master : 	writer.WriteAttributeString( "content", "master" );	break;	
		case EditorContentType.GameObject : writer.WriteAttributeString( "content", "gameobject" );	break;				
		case EditorContentType.More : 		writer.WriteAttributeString( "content", "more" );	break;	
		case EditorContentType.Enum : 		writer.WriteAttributeString( "content", "enum" ); 
											writer.WriteAttributeString( "enum", EnumValuesToString() ); break;			
		case EditorContentType.Vector3 : 	writer.WriteAttributeString( "content", "vector3" ); break;
		case EditorContentType.Curve : 		writer.WriteAttributeString( "content", "curve" ); break;			

		default :						Debug.LogWarning("Unhandled content type "+mContentType ); break;
		}	
		writer.WriteAttributeString( "color", mstrColorName );
		writer.WriteAttributeString( "multiple", (mbMultipleAllowed ? "true" : "false") );
		
		if( mbCustomPosition )
		{
			writer.WriteAttributeString( "plug_x", ""+ mvNormedPlugPosition.x );
			writer.WriteAttributeString( "plug_y", ""+ mvNormedPlugPosition.y );			
		}

		//<ID>24</ID>
		writer.WriteStartElement("ID");
		writer.WriteValue( miUniqueID );	
		writer.WriteEndElement();		
		
		// Sort connections according to panel positions
		SortLinksByYCoordinate();
		
		// Write connections		
		foreach( EditorLink link in maIncomingLinks )
		{
			// <input ID = "23">	
			writer.WriteStartElement("input");	
			writer.WriteAttributeString( "ID", ""+link.mStartSlot.miUniqueID );
			writer.WriteEndElement();
		}
		foreach( EditorLink link in maOutgoingLinks )
		{
			// <output ID = "17">			
			writer.WriteStartElement("output");	
			writer.WriteAttributeString( "ID", ""+link.mEndSlot.miUniqueID );
			writer.WriteEndElement();
		}		

		// <data_string/> (optional)
		if( mstrDataText != null && mstrDataText != "" )
		{
			writer.WriteStartElement("data_string");	
			writer.WriteValue( mstrDataText );	
			writer.WriteEndElement();
		}
		
		// <data_float/> (optional)
		if( mfDataFloat != 0.0f )
		{		
			writer.WriteStartElement("data_float");	
			writer.WriteValue( mfDataFloat );	
			writer.WriteEndElement();
		}
		
		// <data_int/> (optional)	
		if( miDataInt != 0 )
		{			
			writer.WriteStartElement("data_int");	
			writer.WriteValue( miDataInt );	
			writer.WriteEndElement();
		}
		
		// <data_vector/> (optional)	
		if( mvDataVector != Vector3.zero )
		{			
			writer.WriteStartElement("data_vector");	
			writer.WriteValue( VectorToString( mvDataVector ) );	
			writer.WriteEndElement();
		}	
		
		// <data_curve/> (optional)	
		if( mDataCurve != null )
		{			
			writer.WriteStartElement("data_curve");	
			writer.WriteValue( CurveToString( mDataCurve ) );	
			writer.WriteEndElement();
		}	
		
		// <data_vector/> (optional)	
		if( mDataColor != Color.white )
		{			
			writer.WriteStartElement("data_color");	
			writer.WriteValue( ColorToString( mDataColor ) );	
			writer.WriteEndElement();
		}			
		
		// <more/> (optional)	
		if( mContentType == EditorContentType.More )
		{		
			writer.WriteStartElement("more");	
			writer.WriteValue( mbMore ? "true" : "false" );	
			writer.WriteEndElement();
		}
		
		// </slot>		
		writer.WriteEndElement();		
	}
	
	public void Read( XmlReader reader )
	{
		// <slot>
		reader.ReadStartElement("slot");
		
		//<ID>24</ID>
		reader.ReadStartElement("ID");
		miUniqueID = reader.ReadContentAsInt();
		//SpaghettiMachineEditor.GetInstance().RegisterObjectForID( this, miUniqueID );		
		reader.ReadEndElement();		

		// Skip incoming connections (old files)
		while( reader.NodeType == XmlNodeType.Element && reader.Name == "input" )
		{		
			reader.ReadStartElement("input");	
		}
		
		ArrayList aGoesToIDs = new ArrayList();		
		while( reader.NodeType == XmlNodeType.Element && reader.Name == "output" )
		{		
			// <output ID = "17">	
			aGoesToIDs.Add( System.Convert.ToInt32( reader["ID"] ) );
			reader.ReadStartElement("output");	
		}	
		maGoesToIDs = new int[aGoesToIDs.Count];
		for( int i = 0; i < aGoesToIDs.Count; i++)
		{
			maGoesToIDs[i] = (int)aGoesToIDs[i];
		}

		// <data_string/> (optional)
		if( reader.NodeType == XmlNodeType.Element && reader.Name == "data_string" )
		{
			reader.ReadStartElement("data_string");	
			mstrDataText = reader.ReadContentAsString();	
			reader.ReadEndElement();
		}
		
		// <data_float/> (optional)
		if( reader.NodeType == XmlNodeType.Element && reader.Name == "data_float" )
		{
			reader.ReadStartElement("data_float");	
			mfDataFloat = reader.ReadContentAsFloat();	
			reader.ReadEndElement();
		}
		
		// <data_int/> (optional)	
		if( reader.NodeType == XmlNodeType.Element && reader.Name == "data_int" )
		{
			reader.ReadStartElement("data_int");	
			miDataInt = reader.ReadContentAsInt();	
			reader.ReadEndElement();
		}
			
		// <more/> (optional)	
		if( reader.NodeType == XmlNodeType.Element && reader.Name == "more" )
		{		
			reader.ReadStartElement("more");
			mbMore = ( reader.ReadContentAsString() == "true" );
			reader.ReadEndElement();
		}

		// <data_vector> (optional)	
		if( reader.NodeType == XmlNodeType.Element && reader.Name == "data_vector" )
		{
			reader.ReadStartElement("data_vector");	
			mvDataVector = StringToVector( reader.ReadContentAsString() ); 	
			reader.ReadEndElement();
		}
		
		// <data_curve> (optional)	
		if( reader.NodeType == XmlNodeType.Element && reader.Name == "data_curve" )
		{
			reader.ReadStartElement("data_curve");	
			mDataCurve = StringToCurve( reader.ReadContentAsString() );	
			reader.ReadEndElement();
		}
		
		// <data_color> (optional)	
		if( reader.NodeType == XmlNodeType.Element && reader.Name == "data_color" )
		{
			reader.ReadStartElement("data_color");	
			mDataColor = StringToColor( reader.ReadContentAsString() ); 	
			reader.ReadEndElement();
		}		
		
		// </slot>		
		reader.ReadEndElement();	
		
		maIncomingLinks = new EditorLink[0];
		maOutgoingLinks = new EditorLink[0];		
	}			
	
	public void PostReadInitialization()
	{
		foreach( int ID in maGoesToIDs )
		{
			EditorSlot otherslot = SpaghettiMachineEditor.GetInstance().GetSlotWithID( ID );
			if( otherslot != null )
			{
				ConnectWith( otherslot );
			}
			else
			{
				Debug.LogWarning( "Slot ID "+ID+" not found." );
			}
		}
	}
	/*
	public void MemorizeOutgoingLinks()
	{
		maGoesToIDs = new int[maOutgoingLinks.Length];
		for( int i = 0; i < maOutgoingLinks.Length; i++ )
		{
			maGoesToIDs[i] = maOutgoingLinks[i].mEndSlot.miUniqueID;
		}
		Debug.Log( "Memorized "+maGoesToIDs.Length+" links.");
	}
	*/
	
	public void ForgetAllLinks()
	{
		maIncomingLinks = new EditorLink[0];
		maOutgoingLinks = new EditorLink[0];		
	}
	
	public void RepairBrokenParentPointers( EditorPanel parent )
	{
		mParent = parent;
	}
	
	public bool FindAndReplace( string strFind, string strReplaceOrNull )
	{
		bool bFound = false;
		
		
		switch( mContentType )
		{
		case EditorContentType.String : 	
		case EditorContentType.Text : 
		case EditorContentType.Master : 	
		case EditorContentType.GameObject :
			bFound = mstrDataText.Contains( strFind );
			if( strReplaceOrNull != null )
			{
				mstrDataText = mstrDataText.Replace( strFind, strReplaceOrNull );
			}
			break;
			
		case EditorContentType.Float : 	
		case EditorContentType.Int : 	
		case EditorContentType.Bool : 		
		case EditorContentType.None : 			
		case EditorContentType.More :
			bFound = false;
			break;
			
		default :						
			Debug.LogWarning("Unhandled content type "+mContentType ); 
			break;
		}	

		return bFound;
	}
	
	public Color GetPlugColor()
	{
		if(  HasOctarinePlug() )
		{
			return SpaghettiMachineEditor.GetInstance().GetOctarine();
		}
		else if(  HasColourOutOfSpacePlug() )
		{
			return SpaghettiMachineEditor.GetInstance().GetColourOutOfSpace();
		}
		else  
		{
			return mColor;
		}
	}
	
	public bool HasOctarinePlug()
	{
		return ( mstrColorName == "octarine" );
	}
	
	public bool HasColourOutOfSpacePlug()
	{
		return ( mstrColorName == "thecolouroutofspace" );
	}	
	
	public void UpdateFrom( EditorSlot otherslot )
	{
		if( mContentType == otherslot.mContentType )
		{
			mstrDataText = otherslot.mstrDataText;
			mfDataFloat = otherslot.mfDataFloat;
			miDataInt = otherslot.miDataInt;
			mvDataVector = otherslot.mvDataVector;
			mDataCurve = otherslot.mDataCurve;			
		}
		
		//miUniqueID = SpaghettiMachineEditor.GetInstance().GetUnusedUniqueID(); // otherslot.miUniqueID;
		
		maIncomingLinks = otherslot.maIncomingLinks.Clone() as EditorLink[];
		foreach( EditorLink link in maIncomingLinks )
		{
			int iOldStart = link.miStartSlotID;
			int iOldEnd = link.miEndSlotID;
			link.mEndSlot = this;
			link.MemorizeSlotIDs();
			Debug.Log("(1) Link ["+iOldStart+"-"+iOldEnd+"] becomes ["+link.miStartSlotID+"-"+link.miEndSlotID+"]" ); 
		}
		
		maOutgoingLinks = otherslot.maOutgoingLinks.Clone() as EditorLink[];
		foreach( EditorLink link in maOutgoingLinks )
		{
			int iOldStart = link.miStartSlotID;
			int iOldEnd = link.miEndSlotID;
			link.mStartSlot = this;	
			link.MemorizeSlotIDs();
			Debug.Log("(2) Link ["+iOldStart+"-"+iOldEnd+"] becomes ["+link.miStartSlotID+"-"+link.miEndSlotID+"]" ); 
			
		}			
	}
	
	
	/*
	public void ReconstructOutgoingLinks()
	{	
		Debug.Log( "Reconstructing "+maGoesToIDs.Length+" links.");
		foreach( int ID in maGoesToIDs )
		{
			EditorSlot otherslot = SpaghettiMachineEditor.GetInstance().GetSlotWithID( ID );
			if( otherslot != null )
			{
				ConnectWith( otherslot );
			}
			else
			{
				Debug.LogWarning( "Slot ID "+ID+" not found." );
			}
		}		
	}
	*/
	
	
	
	private Vector3 StringToVector( string strInput )
	{
		if( strInput == "" || strInput == null )
		{
			return Vector3.zero;
		}
				
		char[] aSeparator = { "|"[0] };
		
		string[] aSubStrings = strInput.Split( aSeparator );
		
		if( aSubStrings.Length == 3 )
		{
			return new Vector3( Convert.ToSingle( aSubStrings[0] ), Convert.ToSingle( aSubStrings[1] ), Convert.ToSingle( aSubStrings[2] ) );
		}
		else
		{
			Debug.LogError( "Unable to convert string \""+strInput+"\" to Vector3.\nString must consist of three floats separated by \"|\", e.g. \"3.92|2.03|7.99\"" );
			return Vector3.zero;
		}
	}
					
	
	private string VectorToString( Vector3 vInput )
	{
		return vInput.x + "|" + vInput.y + "|" + vInput.z;
		
	}	
	
	private Color StringToColor( string strInput )
	{
		
		if( strInput == null || strInput == "" )
		{
			return Color.white;
		}
		
		char[] aSeparator = { "|"[0] };
		
		string[] aSubStrings = strInput.Split( aSeparator );
		
		if( aSubStrings.Length == 4 )
		{
			return new Color( Convert.ToSingle( aSubStrings[0] ), Convert.ToSingle( aSubStrings[1] ), Convert.ToSingle( aSubStrings[2] ), Convert.ToSingle( aSubStrings[3] ) );
		}
		else if( aSubStrings.Length == 3 )
		{		
			return new Color( Convert.ToSingle( aSubStrings[0] ), Convert.ToSingle( aSubStrings[1] ), Convert.ToSingle( aSubStrings[2] ) );
		}
		else
		{
			Debug.LogError( "Unable to convert string \""+strInput+"\" to Color.\nString must consist of three or four floats separated by \"|\", e.g. \"0.3|0.8|1.0\"" );
			return Color.cyan;
		}
	}
	
	private string ColorToString( Color color )
	{
		return color.r + "|" + color.g + "|" + color.b + "|" + color.a;	
	}	
	
	private AnimationCurve StringToCurve( string strInput )
	{
		AnimationCurve curve = new AnimationCurve();
	
		char[] aSeparator1 = { "|"[0] };
		char[] aSeparator2 = { ":"[0] };
		
		string[] aSubStrings = strInput.Split( aSeparator1 );
		
		// Get pre-wrap mode
		switch( aSubStrings[0] )
		{
		case "clamp":
			curve.preWrapMode = WrapMode.Clamp;
			break;
			
		case "clampforever":
			curve.postWrapMode = WrapMode.ClampForever;
			break;
						
		case "loop":
			curve.preWrapMode = WrapMode.Loop;
			break;
			
		case "pingpong":
			curve.preWrapMode = WrapMode.PingPong;
			break;			
			
		default:
			Debug.LogError("Unhandled wrap mode "+aSubStrings[0] );
			curve.preWrapMode = WrapMode.Clamp;
			break;			
		}
		
		for( int i = 1; i < aSubStrings.Length-1; i++ )
		{
			string[] aSubSubStrings	= aSubStrings[i].Split( aSeparator2 );
			
			float fTime = Convert.ToSingle( aSubSubStrings[0] );
			float fValue = Convert.ToSingle( aSubSubStrings[1] ); 
			float fInTangent = Convert.ToSingle( aSubSubStrings[2] ); 
			float fOutTangent = Convert.ToSingle( aSubSubStrings[3] ); 					
			Keyframe key = new Keyframe( fTime, fValue, fInTangent, fOutTangent );
			
			curve.AddKey( key );
		}
		
		
		// Get post-wrap mode		
		switch( aSubStrings[ aSubStrings.Length-1 ] )
		{
		case "clamp":
			curve.postWrapMode = WrapMode.Clamp;
			break;
			
		case "clampforever":
			curve.postWrapMode = WrapMode.ClampForever;
			break;
			
		case "loop":
			curve.postWrapMode = WrapMode.Loop;
			break;
			
		case "pingpong":
			curve.postWrapMode = WrapMode.PingPong;
			break;			
			
		default:
			Debug.LogError("Unhandled wrap mode "+aSubStrings[ aSubStrings.Length-1 ] );
			curve.postWrapMode = WrapMode.Clamp;
			break;			
		}	
		
		return curve;
	}
	
	private string CurveToString( AnimationCurve curveInput )
	{
		string strOutput = "";
		
		switch( curveInput.preWrapMode )
		{
		case WrapMode.Clamp:
			strOutput += "clamp";
			break;
	
		case WrapMode.ClampForever:
			strOutput += "clampforever";
			break;
			
		case WrapMode.Loop:
			strOutput += "loop";
			break;
			
		case WrapMode.PingPong:
			strOutput += "pingpong";
			break;			
			
		default:
			Debug.LogError("Unhandled wrap mode "+curveInput.preWrapMode );
			strOutput += "clamp";
			break;			
		}
		
		strOutput += "|";
		
		for( int i = 0; i < curveInput.length; i++ )
		{
			Keyframe key = curveInput[i];
			strOutput += key.time + ":" + key.value + ":" + key.inTangent + ":" + key.outTangent + "|";
		}
		
		switch( curveInput.postWrapMode )
		{
		case WrapMode.Clamp:
			strOutput += "clamp";
			break;
	
		case WrapMode.ClampForever:
			strOutput += "clampforever";
			break;
			
		case WrapMode.Loop:
			strOutput += "loop";
			break;
			
		case WrapMode.PingPong:
			strOutput += "pingpong";
			break;			
			
		default:
			Debug.LogError("Unhandled wrap mode "+curveInput.postWrapMode );
			strOutput += "clamp";
			break;			
		}		
		
		return strOutput;
	}	
	
}



