
using System.Xml; 
using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

namespace Spaghetti
{
	public class Graph 
	{
		private static ArrayList maPanels;
		private static XmlReader  mReader;
		
		private SpaghettiMachineCore mSpaghettiMachine;
		
		public Graph( SpaghettiMachineCore machine ) 
		{
			mSpaghettiMachine = machine;
			maPanels = new ArrayList();	
		}
		
		/*
		public void ReadFromTextAsset( TextAsset textAsset )
		{	
			// Prepare the reader
			XmlReaderSettings settings   = new XmlReaderSettings();
			settings.IgnoreWhitespace = true;
	

			System.IO.StringReader stringReader = new System.IO.StringReader(textAsset.text);
			stringReader.Read(); // skip BOM
			mReader  = XmlTextReader.Create( stringReader, settings );
			
			ReadContent();
			
			Debug.Log("Graph read from resource file "+textAsset+".xml" );		
		}
		
		public void ReadFromFile( string strPath )
		{	
			// Prepare the reader
			XmlReaderSettings settings   = new XmlReaderSettings();
			settings.IgnoreWhitespace = true;
			mReader  = XmlReader.Create( strPath, settings);
			
			ReadContent();
			
			Debug.Log("Graph read from file "+strPath );		
		}
		*/
		
		public void ReadFromString( string strData )
		{	
			// Prepare the reader
			XmlReaderSettings settings   = new XmlReaderSettings();
			settings.IgnoreWhitespace = true;
			
            StringReader stringReader = new StringReader(strData);
			if( (int)(strData[0]) == 65279 )
			{
				//skip BOM
            	stringReader.Read();
			}
            mReader = XmlReader.Create(stringReader, settings);

			mReader.MoveToContent();
			
			// <graph>
			mReader.ReadStartElement("graph");	
		
			// Read panels
			while( mReader.NodeType == XmlNodeType.Element && mReader.Name == "panel" )
			{		
				Panel newPanel   = new Panel( mSpaghettiMachine );
				newPanel.Read( mReader, this );
				maPanels.Add( newPanel );
			}
			

			
			if( mReader.NodeType == XmlNodeType.Element && mReader.Name == "grouplist" )
			{
				mReader.Skip();
			}
			
			// </graph>
			mReader.ReadEndElement();
			
			// Finish the reader
			mReader.Close();			
		}
		
		public void PostReadInitialization()
		{
			foreach( Panel panel in maPanels )
			{
				panel.PostReadInitialization( this );
			}
			

			
			foreach( Panel panel in maPanels )
			{
				panel.GetMaster().SendMessage( "HelloMaster", panel, SendMessageOptions.DontRequireReceiver );
			}			
			mSpaghettiMachine.SetPanels( maPanels );			
		}
		
		
		public Slot FindSlotByID( int ID )
		{
			foreach( Panel panel in maPanels )
			{
				Slot slot = panel.FindSlotByID( ID );
				if( slot != null )
				{
					return slot;
				}
			}	
			Debug.LogWarning("Couldn't find slot with ID "+ID );
			return null;
		}
		
		// ---------------------- Acces ----------------------------------------------------
		
	
		public Panel[] GetPanels()  
		{	
			return maPanels.ToArray( typeof( Panel ) ) as Panel[];
		}
		
		
		public void SetMaster( GameObject master )
		{
			foreach( Panel panel in maPanels )
			{
				panel.SetMaster( master );
			}
		}	
	}
}