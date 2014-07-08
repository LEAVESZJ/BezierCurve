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
    static public Vector3 GetQuadraticCurvesPoint( Vector3 startPoint, Vector3 endPoint, Vector3 ctrlPoint, float t )
    {
        return ( 1 - t ) * ( 1 - t ) * startPoint + 2 * ( 1 - t ) * t * ctrlPoint + t * t * endPoint;
    }
}
