using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Ranmaru.GameComponent.City
{
    public class CityEditorWindowBase : EditorWindow
    {
        protected const string CityCreatorSceneName = "Assets/Ranmaru/Scenes/CityCreator.unity";

        protected const float ButtonDefaultHeight = 30;
        protected const float ScrollViewDefaulHeight = 66.0f;
        protected const float SeparateSpace = 3.0f;

        /// <summary>
        /// Button : CityCreatorシーンを開く
        /// </summary>
        protected bool ButtonOpenCityCreatorScene()
        {
            if( EditorApplication.currentScene == CityCreatorSceneName )
            {
                return true;
            }
            
            DrawTextBox( "CityCreatorシーンを開いておりません。下記ボタンを押してください。" );
            
            if( GUILayout.Button( "CityCreatorシーンを開く", GUILayout.Height( ButtonDefaultHeight ) ) )
            {
                EditorApplication.OpenScene( CityCreatorSceneName );
            }
            
            return false;
        }

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
            nodeGameObjects.Sort( ( GameObject a, GameObject b ) => { return a.name.CompareTo( b.name ); } );
            
            return nodeGameObjects;
        }

        /// <summary>
        /// 施設建設作業用ノードになる候補ノードを取得する
        /// </summary>
        static public List<PathNode> GetCandidateBuildNodes( FacilityPanel panel )
        {
            float distanceWithPathNode = panel.PanelID == 0 ? 3.0f : 1.0f;

            Vector3 positionForJudge = panel.gameObject.transform.position;
            positionForJudge.z = CityController.GetFixedPosZ( positionForJudge.y );

            PathNode[] allPathNode = GameObject.FindObjectsOfType<PathNode>();
            List<PathNode> candidateBuildNodes = new List<PathNode>();
            foreach( PathNode p in allPathNode )
            {
                Vector3 distanceVec = p.gameObject.transform.position - positionForJudge;
                if( distanceVec.sqrMagnitude < distanceWithPathNode * distanceWithPathNode )
                {
                    candidateBuildNodes.Add( p );
                }
            }

            return candidateBuildNodes;
        }

        /// <summary>
        /// テキストボックスを描く
        /// </summary>
        /// <param name="text">Text.</param>
        protected void DrawTextBox( string text )
        {
            DrawTextBox( text, 20 );
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
            GUILayout.Box( text, boxStyle, new GUILayoutOption[]{ GUILayout.ExpandWidth( true ), GUILayout.Height( height ) } );
        }
    }
}
