using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Ranmaru.GameComponent.City;

namespace Ranmaru.GameComponent.City
{
    /// <summary>
    /// Navigation path mover.
    /// </summary>
    /// <summary>
    /// Navigation pathの経路・移動管理オブジェクト
    /// </summary>
    public class NavigationPathMover
    {
        private List<PathNode> pathNodes;
        /// <summary>
        /// 経路パス。インデックスの昇順に移動していく。
        /// </summary>
        public List<PathNode> PathNodes
        {
            get { return pathNodes; }
        }

        /// <summary>
        /// ノード間移動の始点
        /// </summary>
        private PathNode movingFrom;

        /// <summary>
        /// ノード間移動の終点情報
        /// </summary>
        private PathNodeInfo movingToInfo;

        /// <summary>
        /// ベジェ曲線
        /// </summary>
        private BezierCurve bezierCurve = new BezierCurve( 1.0f );

        /// <summary>
        /// 曲線移動率
        /// </summary>
        private float curveMoveRate;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="nodeList">ノード情報</param>
        public NavigationPathMover( List<PathNode> nodeList )
        {
            pathNodes = nodeList;
            if( pathNodes != null )
            {
                movingFrom = null;

                movingToInfo = new PathNodeInfo(
                    pathNodes.Count > 0 ? pathNodes[ 0 ] : null,
                    PathNodeInfo.PathType.Straight,
                    null
                );
            }

            curveMoveRate = 0;
        }

        /// <summary>
        /// Moverに設定されたルート通りにゲームオブジェクトを動かす
        /// </summary>
        /// <returns>ゴールに達したらtrue</returns>
        /// <param name="objToMove">移動させるゲームオブジェクト</param>
        /// <param name="deltaTime">前フレームからの経過時間</param>
        /// <param name="speed">移動速度(unit/sec)</param>
        public bool Move( GameObject objToMove, float deltaTime, float speed )
        {
            if( objToMove == null )
            {
                return true;
            }

            // すでにゴールに達した状態である場合、何もせずに抜ける
            if( movingToInfo == null )
            {
                return true;
            }

            // 移動量・向き計算
            Vector3 toTargetNodeVector = movingToInfo.node.transform.position - objToMove.transform.position;
            Vector3 dir = toTargetNodeVector.normalized;

            // 直線移動距離
            float moveDistance = speed * deltaTime;

            // 目的地に達した場合
            if( toTargetNodeVector.sqrMagnitude <= ( dir * moveDistance ).sqrMagnitude )
            {
                changeToNextNode( objToMove );
            }
            // 直線移動（起点が決まっていない場合）
            else if( movingFrom == null || movingToInfo.pathType == PathNodeInfo.PathType.Straight )
            {
                objToMove.transform.position += dir * moveDistance;
            }
            // 曲線移動
            else if( movingToInfo.pathType == PathNodeInfo.PathType.Curve )
            {
                // ベジェ曲線等速移動（近似値）
                // Reference : http://gamedev.stackexchange.com/questions/27056/how-to-achieve-uniform-speed-of-movement-on-a-bezier-curve

                Vector3 startPoint = movingFrom.transform.position;
                Vector3 ctrlPoint  = movingToInfo.curveCtrlPointPos;
                Vector3 endPoint   = movingToInfo.node.transform.position;

                Vector3 v1 =  2.0f * startPoint - 4.0f * ctrlPoint + 2.0f * endPoint;
                Vector3 v2 = -2.0f * startPoint + 2.0f * ctrlPoint;

                curveMoveRate += moveDistance / ( curveMoveRate * v1 + v2 ).magnitude;

                // 曲線移動の目的地に達した場合
                if( curveMoveRate >= 1.0f )
                {
                    changeToNextNode( objToMove );
                }
                else
                {
                    Vector3 nowPos = bezierCurve.GetQuadraticCurvesPoint( startPoint, endPoint, ctrlPoint, curveMoveRate );
                    nowPos.z = CityController.GetFixedPosZ( nowPos.y );

                    objToMove.transform.position = nowPos;
                }
            }

            // ルート移動が完了した
            return movingToInfo == null;
        }

        /// <summary>
        /// 次のノードに移動するように目標変更
        /// </summary>
        private void changeToNextNode( GameObject objToMove )
        {
            objToMove.transform.position = movingToInfo.node.transform.position;
            movingFrom = movingToInfo.node;
            movingToInfo = Next( movingFrom );
            curveMoveRate = 0;
        }

        /// <summary>
        /// ゴールに達したか？
        /// </summary>
        /// <returns><c>true</c> if this instance is reach destination; otherwise, <c>false</c>.</returns>
        public bool DoReachDestination()
        {
            return movingToInfo == null;
        }

        /// <param name="node">Node.</param>
        private PathNodeInfo Next( PathNode node )
        {
            int index = pathNodes.IndexOf( node );
            if (index < 0 || index >= ( pathNodes.Count-1 ) )
            {
                return null;
            }

            return node.NodeInfo.Find( obj => obj.node == pathNodes[ index + 1 ] );
        }
    }
}