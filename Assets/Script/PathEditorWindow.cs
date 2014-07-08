using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// パスエディタウィンドウ
/// </summary>
public class PathEditorWindow : EditorWindowBase
{
    private const string WindowName = "Path Editor";

    private Vector2 allScrollPos    = Vector2.zero;
    private Vector2 scrollPosition  = Vector2.zero;

    static public bool IsMultiOperateCurveCtrlPoint = true;

    /// <summary>
    /// 初期化.
    /// ショットカットキー：⌘ + alt + e
    /// </summary>
    [MenuItem( "Window/Path Editor %&e" )]
    static public void Initialize()
    {
        PathEditorWindow window = EditorWindow.GetWindow<PathEditorWindow>( false, WindowName );

        Vector2 WindowSize = new Vector2( 300.0f, 300.0f );
        window.minSize = WindowSize;
    }

    /// <summary>
    /// OnGUI.
    /// </summary>
    private void OnGUI()
    {
        this.allScrollPos = GUILayout.BeginScrollView( this.allScrollPos );

        GUILayout.Label( "Node Operation", EditorStyles.boldLabel );

        DrawTextBox( "曲線である必要がない場合、負荷軽減の為\n直線を使用してください。", 2 );

        GUILayout.BeginHorizontal();
        ButtonAddNodeCurve();
        ButtonAddNodeStraight();
        GUILayout.EndHorizontal();

        GUILayout.Space( SeparateSpace );

        GUILayout.BeginHorizontal();
        ButtonConnectNodeCurve();
        ButtonConnectNodeStraight();
        GUILayout.EndHorizontal();

        GUILayout.Space( SeparateSpace );

        ButtonTogglePathType();

        GUILayout.Space( SeparateSpace );

        CheckBoxToggleMultiOperateCurveCtrlPoint();

        GUILayout.Space( SeparateSpace );

        ButtonDisconnectNode();

        GUILayout.Space( SeparateSpace );

        ButtonDeleteNode();

        GUILayout.Label( "All Path Node GameObjects", EditorStyles.boldLabel );

        ListButtonAllPathNodeObjcect();

        GUILayout.EndScrollView();
    }

    /// <summary>
    /// OnHierarchyChange.
    /// </summary>
    private void OnHierarchyChange()
    {
        CheckAndRemoveMissingNodeInfo();
    }

    /// <summary>
    /// Button : 曲線パスのNodeを追加する.
    /// </summary>
    private void ButtonAddNodeCurve()
    {
        ButtonAddNode( PathNodeInfo.PathType.Curve );
    }

    /// <summary>
    /// Button : 直線パスのNodeを追加する.
    /// </summary>
    private void ButtonAddNodeStraight()
    {
        ButtonAddNode( PathNodeInfo.PathType.Straight );
    }

    /// <summary>
    /// Button : Nodeを追加する.
    /// </summary>
    private void ButtonAddNode( PathNodeInfo.PathType pathType )
    {
        string buttonName = pathType == PathNodeInfo.PathType.Straight ? "Node追加「直線パス」" : "Node追加「曲線パス」";

        if( GUILayout.Button( buttonName, GUILayout.Height( ButtonDefaultHeight ) ) )
        {
            List<GameObject> allPathNodeObj = GetSortedAllPathNodeObject();

            // Nodeの親生成.
            GameObject nodeParentObj = SpawnNodeParent();

            // Node Pos
            Vector3 nodePos = Vector3.zero;
            if( allPathNodeObj.Count > 0 )
            {
                if( Selection.activeGameObject == null )
                {
                    nodePos = allPathNodeObj[ allPathNodeObj.Count - 1 ].transform.position;
                }
                else
                {
                    nodePos = Selection.activeGameObject.transform.position;
                }
            }
            nodePos.x += 1.0f;

            // NodeのGameObjectの名前.
            int i = 0;
            for( ; i < allPathNodeObj.Count; ++i )
            {
                if( allPathNodeObj[ i ].name != PathNode.GetNodeNameByID( ( i + 1 ) ) )
                {
                    break;
                }
            }
            string objName = PathNode.GetNodeNameByID( ( i + 1 ) );

            // PathNode生成.
            GameObject obj = PathNode.CreatePathNode(
                nodeParentObj.transform,
                nodePos,
                objName,
                i + 1
            );

            // Nodeの自動繋ぎ.
            if( Selection.activeGameObject != null )
            {
                GameObject selectedObj = Selection.activeGameObject;
                PathNode selectedPathNode = selectedObj.GetComponent<PathNode>();
                if( selectedPathNode != null )
                {
                    PathNode nodeFrom = selectedPathNode;
                    PathNode nodeTo = obj.GetComponent<PathNode>();

                    ConnectNode( nodeFrom, nodeTo, pathType );
                }
            }

            // 生成物を自動選択.
            Selection.activeGameObject = obj;

            Undo.RegisterCreatedObjectUndo( obj, "Add Node" );
        }
    }

    /// <summary>
    /// PathNodeInfoの作成.
    /// </summary>
    private PathNodeInfo MakePathNodeInfo( PathNode nodeForConnect, PathNodeInfo.PathType pathType, GameObject curveCtrlPointObj )
    {
        return new PathNodeInfo( nodeForConnect, pathType, curveCtrlPointObj );
    }

    /// <summary>
    /// CurveCtrlPointの作成.
    /// </summary>
    private GameObject MakeCurveCtrlPointObj( Transform parent, Vector3 fromPos, Vector3 toPos, string objName )
    {
        GameObject curveCtrlPointObj = new GameObject();
        curveCtrlPointObj.transform.parent = parent;
        curveCtrlPointObj.name = objName;
        curveCtrlPointObj.transform.localScale = Vector3.one;
        curveCtrlPointObj.transform.position = ( fromPos + toPos ) / 2.0f;

        return curveCtrlPointObj;
    }

    /// <summary>
    /// Nodeの親生成.
    /// </summary>
    private GameObject SpawnNodeParent()
    {
        const string nodeParentName = "PathNode";

        GameObject nodeParentObj = GameObject.Find( nodeParentName );
        if( nodeParentObj == null )
        {
            nodeParentObj = new GameObject( nodeParentName );
            if( Selection.activeGameObject != null )
            {
                nodeParentObj.transform.parent = Selection.activeGameObject.transform;
            }
            nodeParentObj.transform.localPosition = Vector3.zero;

            // Undo登録.
            Undo.RegisterCreatedObjectUndo( nodeParentObj, nodeParentObj.name );
        }

        return nodeParentObj;
    }

    /// <summary>
    /// CheckBox : 曲線制御ポイントの同時操作.
    /// </summary>
    private void CheckBoxToggleMultiOperateCurveCtrlPoint()
    {
        IsMultiOperateCurveCtrlPoint = EditorGUILayout.Toggle( "曲線制御ポイント同時操作", IsMultiOperateCurveCtrlPoint );
    }

    /// <summary>
    /// Button : Node間を接続する「曲線パス」.
    /// </summary>
    private void ButtonConnectNodeCurve()
    {
        ButtonConnectNode( PathNodeInfo.PathType.Curve );
    }

    /// <summary>
    /// Button : Node間を接続する「直線パス」.
    /// </summary>
    private void ButtonConnectNodeStraight()
    {
        ButtonConnectNode( PathNodeInfo.PathType.Straight );
    }

    /// <summary>
    /// Button : Node間を接続する.
    /// </summary>
    private void ButtonConnectNode( PathNodeInfo.PathType pathType )
    {
        string buttonName = pathType == PathNodeInfo.PathType.Straight ? "Node間「直線」接続" : "Node間「曲線」接続";

        if( GUILayout.Button( buttonName, GUILayout.Height( ButtonDefaultHeight ) ) )
        {
            GameObject[] selectObj = Selection.gameObjects;
            if( selectObj.Length != 2 )
            {
                EditorUtility.DisplayDialog( WindowName, "接続したいNodeを２つ選択してください。", "OK" );
                return;
            }
            else
            {
                if( IsPathNodeObjSelected == false )
                {
                    return;
                }

                PathNode nodeFrom = selectObj[ 0 ].GetComponent<PathNode>();
                PathNode nodeTo   = selectObj[ 1 ].GetComponent<PathNode>();
                ConnectNode( nodeFrom, nodeTo, pathType );
            }
        }
    }

    /// <summary>
    /// Node間接続.
    /// </summary>
    private void ConnectNode( PathNode nodeFrom, PathNode nodeTo, PathNodeInfo.PathType pathType )
    {
        PathNodeInfo nodeInfo = nodeFrom.NodeInfo.Find( o => o.node == nodeTo );
        if( nodeInfo == null )
        {
            // 曲線制御ポイントObjの生成.
            GameObject curveCtrlPointObj = pathType == PathNodeInfo.PathType.Curve ?
                MakeCurveCtrlPointObj( nodeFrom.transform, nodeFrom.transform.position, nodeTo.transform.position, "CurveCtrlPoint - " + nodeTo.name ) :
                    null;

            nodeFrom.NodeInfo.Add( MakePathNodeInfo( nodeTo, pathType, curveCtrlPointObj ) );
            EditorUtility.SetDirty( nodeFrom );
        }
        nodeInfo = nodeTo.NodeInfo.Find( o => o.node == nodeFrom );
        if( nodeInfo == null )
        {
            // 曲線制御ポイントObjの生成.
            GameObject curveCtrlPointObj = pathType == PathNodeInfo.PathType.Curve ?
                MakeCurveCtrlPointObj( nodeTo.transform, nodeFrom.transform.position, nodeTo.transform.position, "CurveCtrlPoint - " + nodeFrom.name ) :
                    null;

            nodeTo.NodeInfo.Add( MakePathNodeInfo( nodeFrom, pathType, curveCtrlPointObj ) );
            EditorUtility.SetDirty( nodeTo );
        }
    }

    /// <summary>
    /// Button : Node間パスタイプの切り替え.
    /// </summary>
    private void ButtonTogglePathType()
    {
        if( GUILayout.Button( "Node間パスタイプ「直線/曲線」切り替え", GUILayout.Height( ButtonDefaultHeight ) ) )
        {
            GameObject[] selectObj = Selection.gameObjects;
            if( selectObj.Length != 2 )
            {
                EditorUtility.DisplayDialog( WindowName, "パスタイプ切り替えしたいNodeを２つ選択してください。", "OK" );
                return;
            }
            else
            {
                if( IsPathNodeObjSelected == false )
                {
                    return;
                }

                PathNode nodeFrom = selectObj[ 0 ].GetComponent<PathNode>();
                PathNode nodeTo   = selectObj[ 1 ].GetComponent<PathNode>();

                if( IsPathNodeConnecting( nodeFrom, nodeTo ) == false )
                {
                    return;
                }

                Undo.RecordObject( nodeFrom, "Toggle Path Type" );
                Undo.RecordObject( nodeTo, "Toggle Path Type" );

                PathNodeInfo infoFromTo = nodeFrom.NodeInfo.Find( obj => obj.node == nodeTo );
                PathNodeInfo infoToFrom = nodeTo.NodeInfo.Find( obj => obj.node == nodeFrom );

                List<PathNodeInfo> nodeInfos = new List<PathNodeInfo>();
                nodeInfos.Add( infoFromTo );
                nodeInfos.Add( infoToFrom );

                for( int i = 0; i < nodeInfos.Count; ++i )
                {
                    if( nodeInfos[ i ].pathType == PathNodeInfo.PathType.Curve )
                    {
                        nodeInfos[ i ].pathType = PathNodeInfo.PathType.Straight;
                        if( nodeInfos[ i ].curveCtrlPointObj != null )
                            Undo.DestroyObjectImmediate( nodeInfos[ i ].curveCtrlPointObj );
                    }
                    else if( nodeInfos[ i ].pathType == PathNodeInfo.PathType.Straight )
                    {
                        nodeInfos[ i ].pathType = PathNodeInfo.PathType.Curve;

                        PathNode from = nodeInfos[ i ] == infoFromTo ? nodeFrom : nodeTo;
                        PathNode to   = nodeInfos[ i ] == infoFromTo ? nodeTo : nodeFrom;

                        nodeInfos[ i ].curveCtrlPointObj = MakeCurveCtrlPointObj( from.transform, from.transform.position, to.transform.position, "CurveCtrlPoint - " + to.name );
                        EditorUtility.SetDirty( from );
                    }
                }
            }
        }
    }

    /// <summary>
    /// Button : Node間を切断する.
    /// </summary>
    private void ButtonDisconnectNode()
    {
        if( GUILayout.Button( "Node間切断", GUILayout.Height( ButtonDefaultHeight ) ) )
        {
            GameObject[] selectObj = Selection.gameObjects;
            if( selectObj.Length != 2 )
            {
                EditorUtility.DisplayDialog( WindowName, "切断したいNodeを２つ選択してください。", "OK" );
                return;
            }
            else
            {
                if( IsPathNodeObjSelected == false )
                {
                    return;
                }

                PathNode nodeFrom = selectObj[ 0 ].GetComponent<PathNode>();
                PathNode nodeTo   = selectObj[ 1 ].GetComponent<PathNode>();

                if( IsPathNodeConnecting( nodeFrom, nodeTo ) == false )
                {
                    return;
                }

                Undo.RegisterCompleteObjectUndo( nodeFrom, "Disonnect Node" );
                Undo.RegisterCompleteObjectUndo( nodeTo, "Disonnect Node" );

                PathNodeInfo infoForRemove = nodeFrom.NodeInfo.Find( obj => obj.node == nodeTo );
                if( infoForRemove.curveCtrlPointObj != null )
                    Undo.DestroyObjectImmediate( infoForRemove.curveCtrlPointObj );
                nodeFrom.NodeInfo.Remove( infoForRemove );
                EditorUtility.SetDirty( nodeFrom );

                infoForRemove = nodeTo.NodeInfo.Find( obj => obj.node == nodeFrom );
                if( infoForRemove.curveCtrlPointObj != null )
                    Undo.DestroyObjectImmediate( infoForRemove.curveCtrlPointObj );
                nodeTo.NodeInfo.Remove( infoForRemove );
                EditorUtility.SetDirty( nodeTo );
            }
        }
    }

    /// <summary>
    /// Button : Nodeを削除する.
    /// </summary>
    private void ButtonDeleteNode()
    {
        DrawTextBox( "Nodeを削除する際にHierarchyで行われず\n下記ボタンを使ってください。", 2 );

        if( GUILayout.Button( "Node削除", GUILayout.Height( ButtonDefaultHeight ) ) )
        {
            GameObject[] selectObj = Selection.gameObjects;
            if( selectObj.Length > 0 )
            {
                if( IsPathNodeObjSelected == false )
                {
                    return;
                }

                // Delete
                for( int i = 0; i < selectObj.Length; ++i )
                {
                    List<PathNode> connectingNode = new List<PathNode>();
                    PathNode pathNode = selectObj[ i ].GetComponent<PathNode>();
                    List<PathNodeInfo> nodeInfo = pathNode.NodeInfo;
                    for( int j = 0; j < nodeInfo.Count; ++j )
                    {
                        connectingNode.Add( nodeInfo[ j ].node );
                    }
                    Undo.RegisterCompleteObjectUndo( connectingNode.ToArray(), "Delete Connecting Node" );

                    for( int j = 0; j < connectingNode.Count; ++j )
                    {
                        PathNodeInfo infoForRemove = connectingNode[ j ].NodeInfo.Find( o => o.node == pathNode );
                        if( infoForRemove.curveCtrlPointObj != null )
                            Undo.DestroyObjectImmediate( infoForRemove.curveCtrlPointObj );
                        connectingNode[ j ].NodeInfo.Remove( infoForRemove );
                    }

                    Undo.DestroyObjectImmediate( selectObj[ i ] );
                }
            }
            else
            {
                EditorUtility.DisplayDialog( WindowName, "PathNodeのGameObjectが選択されていません。", "OK" );
            }
        }
    }

    /// <summary>
    /// 選択中のGameObjectには全てPathNodeであるか.
    /// </summary>
    private bool IsPathNodeObjSelected
    {
        get
        {
            GameObject[] selectObj = Selection.gameObjects;
            for( int i = 0; i < selectObj.Length; ++i )
            {
                PathNode pathNode = selectObj[ i ].GetComponent<PathNode>();
                if( pathNode == null )
                {
                    EditorUtility.DisplayDialog( WindowName, "選択中のGameObjectにはPathNodeではない物があります。", "OK" );
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// 選択されているNode間が接続されているか.
    /// </summary>
    private bool IsPathNodeConnecting( PathNode nodeFrom, PathNode nodeTo )
    {
        PathNodeInfo infoFromTo = nodeFrom.NodeInfo.Find( obj => obj.node == nodeTo );
        PathNodeInfo infoToFrom = nodeTo.NodeInfo.Find( obj => obj.node == nodeFrom );

        if( infoFromTo == null || infoToFrom == null )
        {
            EditorUtility.DisplayDialog( WindowName, "選択されているNode間が接続されていません。", "OK" );
            return false;
        }

        return true;
    }

    /// <summary>
    /// ListButton : 全てのPathNodeのGameObjectを表示する.
    /// </summary>
    private void ListButtonAllPathNodeObjcect()
    {
        List<GameObject> nodeGameObjects = GetSortedAllPathNodeObject();

        if( nodeGameObjects.Count == 0 )
        {
            DrawTextBox( "PathNodeのGameObjectを見つかりませんでした。" );
            return;
        }

        DrawTextBox( "下記ボタンを押すとGameObjectが\n自動的に選択されます。", 2 );

        this.scrollPosition = GUILayout.BeginScrollView( this.scrollPosition );

        const int columnNum = 5;
        int linei = 0, columni = 0;
        while( true )
        {
            GUILayout.BeginHorizontal();
            for( columni = 0; columni < columnNum; ++columni )
            {
                int num = linei * columnNum + columni;
                if( num >= nodeGameObjects.Count )
                    break;

                GameObject obj = nodeGameObjects[ num ];
                if( GUILayout.Button( obj.name ) )
                {
                    Selection.activeGameObject = obj;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space( SeparateSpace );

            ++linei;

            if( linei * columnNum >= nodeGameObjects.Count )
                break;
        }

        GUILayout.EndScrollView();
    }

    /// <summary>
    /// MissingになっているNodeInfoを取り除く.
    /// </summary>
    private void CheckAndRemoveMissingNodeInfo()
    {
        List<GameObject> allPathNodes = GetSortedAllPathNodeObject();

        for( int i = 0; i < allPathNodes.Count; ++i )
        {
            PathNode node = allPathNodes[ i ].GetComponent<PathNode>();
            List<PathNodeInfo> nodeInfo = node.NodeInfo;
            for( int j = 0; j < nodeInfo.Count; ++j )
            {
                if( nodeInfo[ j ].node == null )
                {
                    node.NodeInfo.Remove( nodeInfo[ j ] );
                }
            }
        }
    }
}
