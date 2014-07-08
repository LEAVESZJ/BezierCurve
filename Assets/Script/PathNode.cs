using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

    /// <summary>
    /// パスノード
    /// </summary>
    public class PathNode : MonoBehaviour
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
            DrawPath();
            DrawPathName();
        }

        /// <summary>
        /// Draws the path.
        /// </summary>
        private void DrawPath()
        {
            // 自身がつながっているパス間に線を描画
            for( int i = 0; i < nodeInfo.Count; ++i )
            {
                if( nodeInfo[ i ] == null || nodeInfo[ i ].node == null )
                {
                    continue;
                }

                Vector3 startPoint = transform.position;
                Vector3 endPoint   = nodeInfo[ i ].node.transform.position;

                Gizmos.color = IsSelected ? Color.yellow : Color.cyan;

                if( nodeInfo[ i ].pathType == PathNodeInfo.PathType.Straight )
                {
                    DrawStraight( startPoint, endPoint );
                }
                else if( nodeInfo[ i ].pathType == PathNodeInfo.PathType.Curve )
                {
                    Vector3 curveCtrlPointPos = nodeInfo[ i ].CurveCtrlPointPos;
                    nodeInfo[ i ].CurveCtrlPointPos = curveCtrlPointPos;

                    Gizmos.DrawWireCube( curveCtrlPointPos, Vector3.one / 10.0f );

                    DrawCurve( startPoint, endPoint, curveCtrlPointPos );
                    if( !Application.isPlaying )
                    {
                        DrawPoint( startPoint, endPoint, curveCtrlPointPos );
                    }
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
            float t = 0;
            float deltaT = 0.05f;
            while( t <= 1.0f )
            {
                Vector3 lineStart = BezierCurve.GetQuadraticCurvesPoint( startPoint, endPoint, curveCtrlPoint, t );
                Vector3 lineEnd   = BezierCurve.GetQuadraticCurvesPoint( startPoint, endPoint, curveCtrlPoint, t + deltaT );

                Gizmos.DrawLine( lineStart, lineEnd );

                t += deltaT;
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
            while( T <= 1.0f )
            {
                T += L / ( T * v1 + v2 ).magnitude;

                Vector3 lineStart = BezierCurve.GetQuadraticCurvesPoint( startPoint, endPoint, curveCtrlPoint, T );

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

        /// <summary>
        /// Draw Path Name
        /// </summary>
        private void DrawPathName()
        {
            Handles.Label( this.gameObject.transform.position, GetNodeNameByID( nodeID ) );
        }

        /// <summary>
        /// NodeIDからNode名を取得する
        /// </summary>
        static public string GetNodeNameByID( int id )
        {
            return "Node" + id.ToString( "D3" );
        }

        /// <summary>
        /// PathNodeを生成する
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="pos"></param>
        /// <param name="objName"></param>
        /// <param name="nodeID"></param>
        static public GameObject CreatePathNode( Transform parent, Vector3 pos, string objName, int nodeID )
        {
            GameObject pathNodePrefab = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/Prefab/PathNode.prefab", typeof( GameObject ) );
            GameObject pathNodeObj = Instantiate( pathNodePrefab ) as GameObject;
            pathNodeObj.transform.parent = parent;
            pathNodeObj.transform.position = pos;
            pathNodeObj.name = objName;

            PathNode pathNode = pathNodeObj.GetComponent<PathNode>();
            pathNode.NodeID = nodeID;

            return pathNodeObj;
        }
#endif

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
        public Vector3 CurveCtrlPointPos
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
