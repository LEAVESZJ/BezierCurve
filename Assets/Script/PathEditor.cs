using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class PathEditor : EditorWindow
{
    private const int NODE_NAME_LENGTH = 3;

    private Transform nodeParent;
    private Transform NodeParent
    {
        get
        {
            if( nodeParent == null )
            {
                nodeParent = GameObject.Find( "NodeParent" ).transform;
            }

            return nodeParent;
        }
    }


    // Add menu named "My Window" to the Window menu
    [MenuItem( "Window/Path Editor #&e" )]
    static private void Init()
    {
        EditorWindow.GetWindow( typeof( PathEditor ), false, "Path Editor" );
    }

    private void OnGUI()
    {
        GUILayout.Label( "Node Operation", EditorStyles.boldLabel );

        buttonAddNode();
    }

    private void buttonAddNode()
    {
        if( button( "Add Node" ) )
        {
            int nodeID = getNodeID();

            GameObject nodePrefab = AssetDatabase.LoadAssetAtPath( "Assets/Prefab/Node.prefab", typeof( GameObject ) ) as GameObject;
            GameObject nodeObj = Instantiate( nodePrefab ) as GameObject;
            nodeObj.transform.parent = NodeParent;

            nodeObj.name = getNodeIDStr( nodeID );

            Node node = nodeObj.GetComponent<Node>();
            node.RefNodeID = nodeID;

            Undo.RegisterCreatedObjectUndo( nodeObj, "Add Node" );
        }
    }

    private int getNodeID()
    {
        Object[] nodeObjs = GameObject.FindObjectsOfType<Node>();
        List<Node> nodeList = new List<Node>();

        for( int i = 0; i < nodeObjs.Length; ++i )
        {
            nodeList.Add( (Node)nodeObjs[ i ] );
        }

        if( nodeList.Count == 0 ) return 1;

        nodeList.Sort( ( Node nodeX, Node nodeY ) => nodeX.RefNodeID - nodeY.RefNodeID );

        int nodeID = 0;
        while( true )
        {
            if( nodeList[ nodeID ].RefNodeID != nodeID + 1 )
            {
                break;
            }

            ++nodeID;
            if( nodeID >= nodeList.Count )
            {
                break;
            }
        }

        return ++nodeID;
    }

    private string getNodeIDStr( int nodeID )
    {
        return "Node" + nodeID.ToString( "D" + NODE_NAME_LENGTH.ToString() );
    }

    private bool button( string text, float height = 30.0f )
    {
        return GUILayout.Button( text, GUILayout.Height( height ) );
    }
}
