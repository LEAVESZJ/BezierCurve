using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class EditorWindowBase : EditorWindow
{
    protected const float ButtonDefaultHeight = 30;
    protected const float ScrollViewDefaulHeight = 66.0f;
    protected const float SeparateSpace = 3.0f;

    /// <summary>
    /// 全てのPathNodeを名前でソートして取得.
    /// </summary>
    protected List<GameObject> GetSortedAllPathNodeObject()
    {
        Object[] pathNodeObj = GameObject.FindObjectsOfType( typeof( PathNode ) );
        List<GameObject> nodeGameObjects = new List<GameObject>();
        for( int i = 0; i < pathNodeObj.Length; ++i )
        {
            PathNode node = pathNodeObj[ i ] as PathNode;
            nodeGameObjects.Add( node.gameObject );
        }

        // Sort
        nodeGameObjects.Sort( ( GameObject a, GameObject b ) =>
        {
            return a.name.CompareTo( b.name );
        } );

        return nodeGameObjects;
    }

    /// <summary>
    /// テキストボックスを描く
    /// </summary>
    /// <param name="text">Text.</param>
    protected void DrawTextBox( string text )
    {
        DrawTextBox( text, 20.0f );
    }

    /// <summary>
    /// テキストボックスを描く
    /// </summary>
    /// <param name="text">Text.</param>
    /// <param name="lineNum">Line Num.</param>
    protected void DrawTextBox( string text, int lineNum )
    {
        DrawTextBox( text, 20.0f + 12.0f * ( lineNum - 1 ) );
    }

    /// <summary>
    /// テキストボックスを描く
    /// </summary>
    /// <param name="text">Text.</param>
    /// <param name="height">Height.</param>
    protected void DrawTextBox( string text, float height )
    {
        GUIStyle boxStyle = new GUIStyle( GUI.skin.box );
        boxStyle.normal.textColor = Color.gray;
        GUILayout.Box( text, boxStyle, new GUILayoutOption[] { GUILayout.ExpandWidth( true ), GUILayout.Height( height ) } );
    }
}
