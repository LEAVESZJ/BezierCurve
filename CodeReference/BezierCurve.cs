using UnityEngine;
using System.Collections;

namespace Ranmaru.GameComponent.City
{
    /// <summary>
    /// ベジェ曲線.
    /// </summary>
    public class BezierCurve
    {
        /// <summary>
        /// 分割数.
        /// </summary>
        private float divisionNumber;

        private BezierCurve(){}

        public BezierCurve( float divisionNumber )
        {
            this.divisionNumber = divisionNumber;
        }

        /// <summary>
        /// 2次ベジェ曲線のポイント取得.
        /// </summary>
        /// <param name="startPoint">開始ポイント</param>
        /// <param name="endPoint">終了ポイント</param>
        /// <param name="ctrlPoint">制御ポイント</param>
        /// <param name="t">進捗段階(0~divisionNumber)</param>
        public Vector3 GetQuadraticCurvesPoint( Vector3 startPoint, Vector3 endPoint, Vector3 ctrlPoint, float t )
        {
            /*
            if( t > this.divisionNumber )
            {
                Debug.LogWarning( "tの値がdivisionNumberより大きいので、不正な値です。" );
                return Vector3.zero;
            }
            */

            float b = t / divisionNumber;
            float a = 1 - b;

            Vector3 returnPoint = Vector3.zero;
            returnPoint = a * a * startPoint + 2 * a * b * ctrlPoint + b * b * endPoint;
            //returnPoint.x = a * a * startPoint.x + 2 * a * b * ctrlPoint.x + b * b * endPoint.x;
            //returnPoint.y = a * a * startPoint.y + 2 * a * b * ctrlPoint.y + b * b * endPoint.y;

            return returnPoint;
        }

        /// <summary>
        /// 2次ベジェ曲線の長さを取得.
        /// </summary>
        /// <param name="startPoint">開始ポイント</param>
        /// <param name="endPoint">終了ポイント</param>
        /// <param name="ctrlPoint">制御ポイント</param>
        /// <param name="deltaRate">変化率</param>
        public float GetQuadraticCurvesLength( Vector3 startPoint, Vector3 endPoint, Vector3 ctrlPoint, float deltaRate )
        {
            float returnLength = 0;
            float rate = 0;
            while( rate <= this.divisionNumber )
            {
                Vector3 lineStart = GetQuadraticCurvesPoint( startPoint, endPoint, ctrlPoint, rate );
                Vector3 lineEnd   = GetQuadraticCurvesPoint( startPoint, endPoint, ctrlPoint, rate + deltaRate );

                returnLength += Vector3.Distance( lineStart, lineEnd );

                rate += deltaRate;
            }

            return returnLength;
        }
    }
}
