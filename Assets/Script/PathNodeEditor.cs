using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor( typeof( PathNode ) )]
public class PathNodeEditor : Editor
{
    private PathNode targetPathNode;

    private void OnEnable()
    {
        targetPathNode = (PathNode)target;
    }

    private void OnSceneGUI()
    {
        if( targetPathNode.enabled )
        {
            for( int i = 0; i < targetPathNode.NodeInfo.Count; ++i )
            {
                Vector3 oldPos = targetPathNode.NodeInfo[ i ].CurveCtrlPointPos;

                targetPathNode.NodeInfo[ i ].CurveCtrlPointPos =
                    Handles.PositionHandle( targetPathNode.NodeInfo[ i ].CurveCtrlPointPos, Quaternion.identity );

                if( PathEditorWindow.IsMultiOperateCurveCtrlPoint &&
                    oldPos != targetPathNode.NodeInfo[ i ].CurveCtrlPointPos )
                {
                    PathNodeInfo nodeInfo = targetPathNode.NodeInfo[ i ].node.NodeInfo.Find( obj => obj.node.NodeID == targetPathNode.NodeID );
                    nodeInfo.CurveCtrlPointPos = targetPathNode.NodeInfo[ i ].CurveCtrlPointPos;
                }
            }
        }
    }
}
