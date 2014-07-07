using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Ranmaru.GameComponent.City;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ranmaru.GameComponent.City
{
    /// <summary>
    /// パスノード
    /// </summary>
    public class PathNode : GameBehaviour
    {
        /// <summary>
        /// ノードID
        /// </summary>
        [SerializeField]
        private int nodeID;
        public int NodeID
        {
            get{ return nodeID; }
            set{ nodeID = value; }
        }

        /// <summary>
        /// ノードType
        /// </summary>
        public enum NodeType : int
        {
            Normal,
            Special
        }

        /// <summary>
        /// ノードType
        /// </summary>
        [SerializeField]
        private NodeType nodeType;
        public NodeType Type
        {
            get{ return nodeType; }
            set{ nodeType = value; }
        }

        /// <summary>
        /// オブジェクト生成時の出現ノードであるか
        /// </summary>
        [SerializeField]
        private bool generationNode = false;
        public bool GenerationNode
        {
            get{ return generationNode; }
            set{ generationNode = value; }
        }

        /// <summary>
        /// 分岐ノード情報
        /// </summary>
        [SerializeField]
        private List<PathNodeInfo> nodeInfo;

        /// <summary>
        /// 分岐ノード情報取得
        /// </summary>
        public List<PathNodeInfo> NodeInfo
        {
            get { return nodeInfo; }
            set { nodeInfo = value; }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Gizmoがレンダリングされた時に呼ばれる
        /// </summary>
        private void OnDrawGizmos()
        {
            DrawNodeID();
            DrawGenerateNode();
            DrawPath();
        }

        /// <summary>
        /// Draws the node ID.
        /// </summary>
        static public bool EnablePathNodeName = false;
        private void DrawNodeID()
        {
            if( !EnablePathNodeName ) return;

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;
            style.fontSize = 10;

            Handles.Label( this.gameObject.transform.position, TerritoryTools.GetNodeNameByID( nodeID ), style );
        }

        /// <summary>
        /// Draws the generate node.
        /// </summary>
        static public bool EnableGenerateNode = true;
        private void DrawGenerateNode()
        {
            if( !EnableGenerateNode ) return;

            GUIStyle style = new GUIStyle();
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.cyan;
            style.fontSize = 10;

            if( generationNode )
            {
                Handles.Label( this.gameObject.transform.position, "Generate\nNode", style );
            }
        }

        /// <summary>
        /// Draws the path.
        /// </summary>
        private void DrawPath()
        {
            // 自身がつながっているパス間に線を描画
            for( int i = 0; i < nodeInfo.Count; ++i )
            {
                if( nodeInfo[ i ] == null || nodeInfo[ i ].node == null ) continue;
                
                Vector3 startPoint = transform.position;
                Vector3 endPoint   = nodeInfo[ i ].node.transform.position;
                
                if( this.nodeType == NodeType.Special || nodeInfo[ i ].node.Type == NodeType.Special )
                {
                    Gizmos.color = IsSelected ? Color.red : Color.magenta;
                }
                else
                {
                    Gizmos.color = IsSelected ? Color.red : Color.blue;
                }
                
                if( nodeInfo[ i ].pathType == PathNodeInfo.PathType.Straight )
                {
                    DrawStraight( startPoint, endPoint );
                }
                else if( nodeInfo[ i ].pathType == PathNodeInfo.PathType.Curve )
                {
                    Vector3 curveCtrlPointPos = nodeInfo[ i ].curveCtrlPointPos;
                    curveCtrlPointPos.z = CityController.GetFixedPosZ( curveCtrlPointPos.y );
                    nodeInfo[ i ].curveCtrlPointPos = curveCtrlPointPos;
                    
                    Gizmos.DrawWireCube( curveCtrlPointPos, Vector3.one / 10.0f );
                    
                    DrawCurve( startPoint, endPoint, curveCtrlPointPos );
                    if( !Application.isPlaying ) DrawPoint( startPoint, endPoint, curveCtrlPointPos );
                }
            }
        }

        /// <summary>
        /// Draws the straight.
        /// </summary>
        /// <param name="startPoint">Start point.</param>
        /// <param name="endPoint">End point.</param>
        private void DrawStraight( Vector3 startPoint, Vector3 endPoint )
        {
            Gizmos.DrawLine( startPoint, endPoint );
        }

        /// <summary>
        /// Draws the curve.
        /// </summary>
        private void DrawCurve( Vector3 startPoint, Vector3 endPoint, Vector3 curveCtrlPoint )
        {
            int T = 30;
            BezierCurve curve = new BezierCurve( (float)T );
            for( int t = 0; t < T; ++t )
            {
                Vector3 lineStart = curve.GetQuadraticCurvesPoint( startPoint, endPoint, curveCtrlPoint, (float)t );
                Vector3 lineEnd   = curve.GetQuadraticCurvesPoint( startPoint, endPoint, curveCtrlPoint, (float)( t + 1 ) );

                lineStart.z = CityController.GetFixedPosZ( lineStart.y );
                lineEnd.z   = CityController.GetFixedPosZ( lineEnd.y );

                Gizmos.DrawLine( lineStart, lineEnd );
            }
        }

        /// <summary>
        /// Draws the point.
        /// </summary>
        private void DrawPoint( Vector3 startPoint, Vector3 endPoint, Vector3 curveCtrlPoint )
        {
            // Reference : http://gamedev.stackexchange.com/questions/27056/how-to-achieve-uniform-speed-of-movement-on-a-bezier-curve

            Vector3 A = startPoint;
            Vector3 B = curveCtrlPoint;
            Vector3 C = endPoint;

            Vector3 v1 =  2.0f * A - 4.0f * B + 2.0f * C;
            Vector3 v2 = -2.0f * A + 2.0f * B;

            float T = 0f;
            const float L = 0.2f;
            BezierCurve curve = new BezierCurve( 1.0f );
            while( T <= 1.0f )
            {
                T += L / ( T * v1 + v2 ).magnitude;

                Vector3 lineStart = curve.GetQuadraticCurvesPoint( startPoint, endPoint, curveCtrlPoint, T );
                lineStart.z = CityController.GetFixedPosZ( lineStart.y );

                Gizmos.DrawSphere( lineStart, 0.02f );
            }
        }

        /// <summary>
        /// IsSelected.
        /// </summary>
        private bool IsSelected
        {
            get
            {
                return UnityEditor.Selection.activeGameObject != null &&
                          ( UnityEditor.Selection.activeGameObject == this.gameObject ||
                            ( UnityEditor.Selection.activeGameObject.transform.parent != null &&
                              UnityEditor.Selection.activeGameObject.transform.parent.gameObject == this.gameObject ) );
            }
        }
#endif

        /// <summary>
        /// 線分とこのノードが交差したかをチェック
        /// </summary>
        /// <returns>The node radius.</returns>
        public bool Intersect( Vector3 origin, Vector3 dir, float dist )
        {
            SphereCollider collider = GetComponentInChildren<SphereCollider>();
            if (collider != null)
            {
                // ゼロベクトルの場合、originがバウンディングボックスの中に含まれているかどうかでチェック
                if (dir.sqrMagnitude < 0.000001f || dist < 0.000001f)
                {
                    return collider.bounds.Contains(origin); 
                }

                RaycastHit hitInfo = new RaycastHit();
                return collider.Raycast(new Ray( origin, dir.normalized ), out hitInfo, dist);
            }

            return false;

        }
    }

    /// <summary>
    /// 分岐ノード情報クラス
    /// </summary>
    [System.Serializable]
    public class PathNodeInfo
    {
        // パスタイプ.
        public enum PathType : int
        {
            Straight = 0,   // 直線.
            Curve           // 曲線.
        };

        public PathNodeInfo( PathNode node, PathType pathType, GameObject curveCtrlPointObj )
        {
            this.node               = node;
            this.pathType           = pathType;
            this.curveCtrlPointObj  = curveCtrlPointObj;
        }

        public PathNodeInfo() {}

        /// <summary>
        /// 曲線パスの制御点位置を取得する.
        /// </summary>
        public Vector3 curveCtrlPointPos
        {
            get
            {
                return curveCtrlPointObj == null ? Vector3.zero : curveCtrlPointObj.transform.position;
            }

            set
            {
                curveCtrlPointObj.transform.position = value;
            }
        }

        public PathNode node;               // 分岐ノード.
        public PathType pathType;           // 分岐ノードとの間のパスタイプ.

        public GameObject curveCtrlPointObj;   // 曲線パスの制御点.
    }
}
