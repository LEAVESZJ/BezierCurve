using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Ranmaru.GameComponent.City
{

    /// <summary>
    /// 
    /// Navigation path 基盤システム実装
    /// 
    /// 初期化時にPathNodeコンポーネントを持つゲームオブジェクトをすべて収集し、管理する。
    /// その後、ほかのシステムやオブジェクトからの要請に応じて、経路情報を含んだオブジェクト移動制御オブジェクト（Mover）を提供する。
    /// 
    /// </summary>
    public class NavigationPathSystem : GameBehaviour
    {
        /*****************************************
         *  variables
         *****************************************/

        /// <summary>
        /// 検索するノードのタイプ
        /// </summary>
        public enum SearchNodeType
        {
            Normal, // Normalノードのみ
            All     // 全ノード
        }

        /// <summary>
        /// パスノード（Normalのみ）
        /// </summary>
        private List<PathNode> normalPathNodeList;
        public List<PathNode> NormalPathNodeList
        {
            get { return normalPathNodeList; }
        }

        /// <summary>
        /// 全パスノード（Normal、Special）
        /// </summary>
        private List<PathNode> allNodeList;
        public List<PathNode> AllNodeList
        {
            get { return allNodeList; }
        }

        /// <summary>
        /// 生成用ノード
        /// </summary>
        private List<PathNode> generationNodeList;
        public List<PathNode> GenerationNodeList
        {
            get { return generationNodeList; }
        }

        /*****************************************
        *  declare for singleton
        *****************************************/

        /// <summary>
        /// 初期化。シーンにあるナビゲーションパス情報をすべて集めて、キャッシュを管理する
        /// </summary>
        void Start()
        {
            // シーン中に含まれているすべてのPathNodeを検索
            normalPathNodeList = new List<PathNode>();
            allNodeList = new List<PathNode>();
            generationNodeList = new List<PathNode>();
            PathNode[] pathNodes = GetComponentsInChildren<PathNode>();
            foreach (PathNode pathNode in pathNodes)
            {
                if( pathNode.Type == PathNode.NodeType.Normal )
                {
                    normalPathNodeList.Add( pathNode );
                }

                allNodeList.Add( pathNode );

                if( pathNode.GenerationNode )
                {
                    generationNodeList.Add( pathNode );
                }
            }
        }

        /*****************************************
        *  get route path list
        *****************************************/

        /// <summary>
        /// スタートノードからゴールノードまでの最短経路を取得する。
        /// 3D空間での距離が最短になるように最適化
        /// </summary>
        /// <returns>最短経路パス</returns>
        /// <param name="start">スタートノード</param>
        /// <param name="dest">ゴールノード</param>
        /// <param name="type">検索するノードのタイプ</param>
        public List<PathNode> GetShortestPath( PathNode start, PathNode dest, SearchNodeType type )
        {
            return AStarAlgorithm( start, dest, type );
        }

        /// <summary>
        /// スタート位置から終了位置までの最短経路を返す。
        /// </summary>
        /// <returns>最短経路のノードリスト</returns>
        /// <param name="startPos">スタート位置</param>
        /// <param name="destPos">終了位置</param>
        /// <param name="type">検索するノードのタイプ</param>
        public List<PathNode> GetShortestPath( Vector3 startPos, Vector3 destPos, SearchNodeType type )
        {
            // スタート地点に最も近いパスノードを探す
            PathNode startNode = GetNearestPathNode( startPos, type );
            if (startNode == null)
            {
                return null;
            }

            // エンド地点に最も近いパスノードを探す
            PathNode destNode = GetNearestPathNode( destPos, type );
            if (destNode == null)
            {
                return null;
            }

            return GetShortestPath( startNode, destNode, type );
        }

        /// <summary>
        /// 指定した位置から最も近いパスノードを検索して取得する
        /// </summary>
        /// <returns>最も近いパスノード</returns>
        /// <param name="pos">検索する位置</param>
        /// <param name="type">検索するノードのタイプ</param>
        public PathNode GetNearestPathNode( Vector3 start, SearchNodeType type )
        {
            PathNode nearest = null;
            float nearestDistance = float.MaxValue;

            List<PathNode> pathNodeList = type == SearchNodeType.Normal ? normalPathNodeList : allNodeList;
            foreach( PathNode node in pathNodeList )
            {
                float distFromStart = Vector3.Distance( node.transform.position, start );
                if( distFromStart < nearestDistance )
                {
                    nearest = node;
                    nearestDistance = distFromStart;
                }
            }

            return nearest;
        }

        /********************************************
         *  A* algorithm
         ********************************************/

        /// <summary>
        /// A* アルゴリズム
        /// </summary>
        /// <returns>最短経路。経路が存在しなければnull</returns>
        /// <param name="start">経路の開始ノード</param>
        /// <param name="dest">経路の終了ノード</param>
        /// <param name="type">検索するノードのタイプ</param>
        private List<PathNode> AStarAlgorithm( PathNode start, PathNode dest, SearchNodeType type )
        {
            // 
            if (start == null || dest == null)
            {
                return null;
            }

            // パス最短経路を求める
            List<PathNode> openNodes = new List<PathNode>();        // 走査待ちノード
            List<PathNode> closedNodes = new List<PathNode>();      // 走査済みノード

            Dictionary<PathNode, float> scoreOfTheRoute = new Dictionary<PathNode,float>();
            Dictionary<PathNode, float> scoreFromStart = new Dictionary<PathNode, float>();

            Dictionary<PathNode, PathNode> shortestRoute = new Dictionary<PathNode, PathNode>();

            // 計算のための初期値設定
            openNodes.Add( start );
            closedNodes.Clear();
            scoreFromStart.Add(start, 0.0f);
            scoreOfTheRoute.Add(start, 0.0f + GetHeuristicDistance(start,dest));

            // 最短経路検索
            while (openNodes.Count > 0)
            {
                // オープンノードに含まれている、最もスコアの小さいルートを検出
                PathNode currentNode = null;
                {
                    PathNode bestNode = null;
                    float bestScore = float.MaxValue;
                    foreach (PathNode node in openNodes)
                    {
                        float score = scoreOfTheRoute[node];
                        if (score < bestScore)
                        {
                            bestNode = node;
                            bestScore = score;
                        }
                    }

                    currentNode = bestNode;
                }

                // そのルートがゴールだったら抜ける
                if (currentNode == dest)
                {
                    // 最短経路マップから、最短経路リストに変換して返す
                    List<PathNode> result = new List<PathNode>();
                    for (PathNode n = dest; n != start; n = shortestRoute[n])
                    {
                        result.Insert(0, n);
                    }
                    result.Insert(0, start);
                    return result;
                }

                // 現在のノードを検査済みノードに入れる。これ以降このノードに対する走査は行わない
                openNodes.Remove(currentNode);
                closedNodes.Add(currentNode);

                // 自身から遷移できるパスを走査
                foreach (PathNodeInfo neighbor in currentNode.NodeInfo)
                {
                    // 後戻りチェック（すでに走査されているノードはたどらない）
                    if( ( closedNodes.IndexOf(neighbor.node) >= 0 ) ||
                        ( type == SearchNodeType.Normal && neighbor.node.Type != PathNode.NodeType.Normal ) )   // Normalノードの場合、Normalノード以外を除外する
                    {
                        continue;
                    }

                    // スタートからそのノードまでのスコアを算出
                    float scoreFromCurrentToNeighbor    = GetCost( currentNode, neighbor.node );
                    float scoreFromStartToNeighbor      = scoreFromStart[currentNode] + scoreFromCurrentToNeighbor;

                    // 未走査キューに含まれていない場合、もしくはすでに見つかった経路よりもコストが短い経路だった場合、
                    // 有効なルートかつ、neighborノードにくるための最短経路になる。
                    if( openNodes.IndexOf( neighbor.node ) < 0 || scoreFromStartToNeighbor < scoreFromStart[neighbor.node] )
                    {
                        // 最短経路情報として登録
                        shortestRoute[neighbor.node] = currentNode;

                        // スコアを算出
                        scoreFromStart[neighbor.node] = scoreFromStartToNeighbor;
                        scoreOfTheRoute[neighbor.node] = scoreFromStartToNeighbor + GetHeuristicDistance( neighbor.node, dest );

                        // 未走査の場合は、走査対象ノードとして追加。次のループで検索を行う
                        if (openNodes.IndexOf(neighbor.node) < 0)
                        {
                            openNodes.Add( neighbor.node );
                        }
                    }
                }
            }

            // ここにきた場合、到達不能。
            return null;
        }

        /// <summary>
        /// 2地点間のコストを算出
        /// </summary>
        /// <returns>2地点間のコスト</returns>
        private float GetCost( PathNode a, PathNode b )
        {
            return Vector3.Distance(a.transform.position, b.transform.position);
        }

        /// <summary>
        /// スタート地点からゴール地点までの推定距離を算出する
        /// </summary>
        /// <returns>The heuristic from dist.</returns>
        private float GetHeuristicDistance( PathNode start, PathNode dest )
        {
            return Vector3.Distance( start.transform.position, dest.transform.position );
        }
    }
}