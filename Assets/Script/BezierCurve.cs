using UnityEngine;
using System.Collections;

/// <summary>
/// ベジェ曲線.
/// </summary>
public class BezierCurve
{
    /// <summary>
    /// 2次ベジェ曲線のポイント取得.
    /// </summary>
    /// <param name="startPoint">開始ポイント</param>
    /// <param name="endPoint">終了ポイント</param>
    /// <param name="ctrlPoint">制御ポイント</param>
    /// <param name="t">進捗段階(0~divisionNumber)</param>
    static public Vector3 GetQuadraticCurvePoint( Vector3 startPoint, Vector3 endPoint, Vector3 ctrlPoint, float t )
    {
        return ( 1 - t ) * ( 1 - t ) * startPoint + 2 * ( 1 - t ) * t * ctrlPoint + t * t * endPoint;
    }

    /// <summary>
    /// 2次ベジェ曲線上の等速移動する為のtの変化量の取得.
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="endPoint"></param>
    /// <param name="ctrlPoint"></param>
    /// <param name="t"></param>
    /// <param name="length"></param>
    static public float GetUniformDeltaTOnQuadraticCurvePoint( Vector3 startPoint, Vector3 endPoint, Vector3 ctrlPoint, float t, float length )
    {
        // Reference : http://gamedev.stackexchange.com/questions/27056/how-to-achieve-uniform-speed-of-movement-on-a-bezier-curve

        Vector3 v1 =  2.0f * startPoint - 4.0f * ctrlPoint + 2.0f * endPoint;
        Vector3 v2 = -2.0f * startPoint + 2.0f * ctrlPoint;

        return length / ( t * v1 + v2 ).magnitude;
    }
}
