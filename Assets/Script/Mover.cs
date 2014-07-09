using UnityEngine;
using System.Collections;

public class Mover : MonoBehaviour
{
    [SerializeField]
    private float           moveSpeed = 1.0f;

    [SerializeField]
    private bool            isUniformMove = false;

    private PathNode        startNode;

    private PathNode        moveFromNode;
    private PathNodeInfo    moveToNode;

    private float           curveMoveRate;

    private Vector3 position
    {
        get
        {
            return this.gameObject.transform.position;
        }

        set
        {
            this.gameObject.transform.position = value;
        }
    }

    /// <summary>
    /// Move
    /// </summary>
    private void Move()
    {
        float moveDistance = moveSpeed * Time.deltaTime;

        if( moveFromNode == null || moveToNode.pathType == PathNodeInfo.PathType.Straight )
        {
            PathNode toNode = moveFromNode == null ?
                startNode : moveToNode.node;

            Vector3 moveVector = toNode.gameObject.transform.position - this.position;
            Vector3 moveDistanceVector = moveVector.normalized * moveDistance;

            if( moveVector.sqrMagnitude <= moveDistanceVector.sqrMagnitude )
            {
                ChangeDestination( toNode );
            }

            this.position += moveDistanceVector;
        }
        else if( moveToNode.pathType == PathNodeInfo.PathType.Curve )
        {
            Vector3 startPoint = moveFromNode.transform.position;
            Vector3 ctrlPoint  = moveToNode.CurveCtrlPointPos;
            Vector3 endPoint   = moveToNode.node.transform.position;

            if( !isUniformMove )
            {
                curveMoveRate += moveDistance * 0.1f;
            }
            else
            {
                float deltaT =
                    BezierCurve.GetUniformDeltaTOnQuadraticCurvePoint( startPoint, endPoint, ctrlPoint, curveMoveRate, moveDistance );

                curveMoveRate += deltaT;
            }

            if( curveMoveRate >= 1.0f )
            {
                ChangeDestination( moveToNode.node );
            }
            else
            {
                Vector3 pos = BezierCurve.GetQuadraticCurvePoint( startPoint, endPoint, ctrlPoint, curveMoveRate );
                this.position = pos;
            }
        }
    }

    /// <summary>
    /// Change Destination
    /// </summary>
    private void ChangeDestination( PathNode fromNode )
    {
        moveFromNode = fromNode;

        int moveToNodeIndex = Random.Range( 0, moveFromNode.NodeInfo.Count );
        moveToNode = moveFromNode.NodeInfo[ moveToNodeIndex ];

        curveMoveRate = 0;
    }

    // Use this for initialization
    private void Start()
    {
        // ランダムでStartNodeを選択する
        Object[] pathNodeObjs = GameObject.FindObjectsOfType<PathNode>();
        startNode = (PathNode)pathNodeObjs[ Random.Range( 0, pathNodeObjs.Length ) ];
    }

    // Update is called once per frame
    private void Update()
    {
        Move();
    }
}
