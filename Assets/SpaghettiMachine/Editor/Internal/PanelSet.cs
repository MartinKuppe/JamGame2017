using UnityEngine;
using System.Collections;
using System.Xml; 
using System;
using System.Security;
using System.IO;

[System.Serializable]
public class PanelSet // : ScriptableObject 
{
	[SerializeField]
	public EditorPanel[] maPanelPrototypes;
	public Rect mWindowRect = new Rect( 5, 5, 150, 5 );
	public int miUniqueID;
	public string mstrName;
	public string mstrPath;
	public bool mbUnfolded = true;
	
	private static XmlReader mReader;
	//private static XmlWriter mWriter;
	
	public void ReadFromFile( string strPath )
	{	
		//miUniqueID = SpaghettiMachineEditor.GetInstance().GetUnusedUniqueIDfor( this );
		mstrName = strPath.Substring( strPath.LastIndexOf("/")+1 );
		mstrPath = strPath;
		
		try
		{
			// Prepare the reader
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreWhitespace = true;
			mReader  = XmlReader.Create( strPath, settings);
			
			ArrayList aPanelPrototypes = new ArrayList();
			
			// Start reading
			mReader.MoveToContent();
			try
			{
				mReader.ReadStartElement("panelset"); 
			}
			catch( XmlException e )
			{
				Debug.LogError( "Line "+e.LineNumber+"; position "+e.LinePosition+": Expecting <panelset>" );
				throw e;
			}
			
			// Read panels
			mReader.MoveToContent();
			while( mReader.NodeType == XmlNodeType.Element && mReader.Name == "paneltype" )
			{		
				EditorPanel newPanel = new EditorPanel(); //ScriptableObject.CreateInstance( typeof( EditorPanel ) ) as EditorPanel;
				newPanel.mstrType = mReader["type"];
				newPanel.SetPanelset( mstrName, mstrPath );
				
				if( mReader["type_old"] != null )
				{
					newPanel.mstrTypeOld =  mReader["type_old"];
				}
				else
				{
					newPanel.mstrTypeOld = newPanel.mstrType;
				}
				
				string strWidth = mReader["width"];				
				if( strWidth != null )
				{
					int iWidth = System.Convert.ToInt32( strWidth );
					if( iWidth >= 3 )
					{
						newPanel.mWindowRect.width = iWidth;
					}
				}
				string strHeight = mReader["height"];			
				if( strHeight != null )
				{
					int iHeight = System.Convert.ToInt32( strHeight );
					if( iHeight >= 10 )
					{
						newPanel.mWindowRect.height = iHeight;
						newPanel.mbFixedHeight = true;
					}
				}
				
				string strImage = mReader["image"];
				if( strImage != null )
				{
					newPanel.mImage =  (Texture2D)Resources.Load( strImage );
					if( newPanel.mImage != null  ) 
					{
						if(!newPanel.mbFixedHeight )
						{
							newPanel.mWindowRect.height = newPanel.mImage.height + EditorPanel.WINDOW_MARGE_TOP + EditorPanel.WINDOW_MARGE_BOTTOM;
						}
						newPanel.mWindowRect.width = newPanel.mImage.width + EditorPanel.WINDOW_MARGE_LEFT + EditorPanel.WINDOW_MARGE_RIGHT;
					}					
				}
			
	
				mReader.MoveToContent();
				mReader.ReadStartElement( "paneltype" );		
				ArrayList aSlotPrototypes = new ArrayList();
				int iIndex = 0;
				
				mReader.MoveToContent();
				while( mReader.NodeType == XmlNodeType.Element && mReader.Name == "slot" )
				{	
					EditorSlot newSlot = new EditorSlot(); //ScriptableObject.CreateInstance( typeof( EditorSlot ) ) as EditorSlot;
					newSlot.InitSlot( newPanel, 
					                 iIndex++,
						             mReader["label"], 
									 mReader["label_old"], 
					                 mReader["plug"], 
					                 mReader["content"], 
					                 mReader["color"], 
					                 mReader["multiple"], 
					                 mReader["default"], 
					                 mReader["enum"],
					                 mReader["plug_x"], 			                 
					                 mReader["plug_y"] );

						mReader.ReadStartElement( "slot" );	

					aSlotPrototypes.Add( newSlot );
					mReader.MoveToContent();
				}
				newPanel.maSlots = aSlotPrototypes.ToArray( typeof( EditorSlot ) ) as EditorSlot[];

				try
				{
					mReader.MoveToContent();
					mReader.ReadEndElement();
				}
				catch( XmlException e )
				{
					Debug.LogError( "Line "+e.LineNumber+"; position "+e.LinePosition+": Expecting <slot> or </paneltype>." );
					throw e;
				}	
				aPanelPrototypes.Add( newPanel );
				mReader.MoveToContent();
			}
			maPanelPrototypes = aPanelPrototypes.ToArray( typeof( EditorPanel ) )  as EditorPanel[];
			try
			{
				mReader.ReadEndElement();
			}
			catch( XmlException e )
			{
				Debug.LogError( "Line "+e.LineNumber+"; position "+e.LinePosition+": Expecting <paneltype> or </panelset>." );
				throw e;
			} 	
			
		} // Gotta catch 'em all !
		catch( SecurityException  e )
		{
			Debug.LogError( "Error: Not allowed to read panelset file "+strPath );
			Debug.LogError( ""+e );
		}
		catch( FileNotFoundException  e )
		{
			Debug.LogError( "Error: Unable to find panelset file "+strPath );
			Debug.LogError( ""+e );				
		}
		catch( UriFormatException  e )
		{
			Debug.LogError( "Error: Incorrect URI format of panelset file "+strPath );
			Debug.LogError( ""+e );				
			
		}		
		catch( XmlException  e )
		{
			Debug.LogError( ""+e );				
		}
		catch( Exception  e )
		{
			Debug.LogError( "Error while reading panelset file "+strPath+": \n"+e);			
		}		
		finally
		{
			if( mReader != null && mReader.ReadState != ReadState.Closed )
			{
				// Finish the reader
				mReader.Close();
			}
		}
		
		//UnityEngine.Object.DontDestroyOnLoad( this );
	}

}