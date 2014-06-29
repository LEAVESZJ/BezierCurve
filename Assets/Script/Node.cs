using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Node
/// </summary>
public class Node : MonoBehaviour
{
    public int RefNodeID;
    public List<JointNode> RefJointNodes = new List<JointNode>();

    public Vector3 Pos
    {
        get
        {
            return this.gameObject.transform.position;
        }
    }
}

[Serializable]
public class JointNode
{
    public Node RefNode;
    public GameObject RefControlPoint;

    public Vector3 ControlPointPos
    {
        get
        {
            return RefControlPoint.transform.position;
        }
    }
}