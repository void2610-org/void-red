using UnityEngine;
using Void2610.UnityTemplate;

namespace Auction
{
    /// <summary>
    /// カードドラッグ時に始点から終点への曲線を描画するコンポーネント
    /// </summary>
    public class DragLineView : MonoBehaviour
    {
        [SerializeField] private UILineRenderer lineRenderer;
        [SerializeField] private int segmentCount = 20;
        [SerializeField] private float curveHeight = 50f;

        private RectTransform _rectTransform;
        private Canvas _canvas;
        private Camera _camera;
        private Vector3 _startWorldPos;
        private Vector2[] _points;

        public void Initialize(Canvas canvas)
        {
            _canvas = canvas;
            _camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            _rectTransform = lineRenderer.GetComponent<RectTransform>();
            _points = new Vector2[segmentCount + 1];
            lineRenderer.gameObject.SetActive(false);
        }

        public void Show(Vector3 startWorldPos)
        {
            _startWorldPos = startWorldPos;
            lineRenderer.gameObject.SetActive(true);
        }

        public void UpdateEndPosition(Vector3 endWorldPos)
        {
            if (!lineRenderer.gameObject.activeSelf) return;

            // ワールド座標をUILineRendererのローカル座標に変換
            var startLocal = WorldToLocalPoint(_startWorldPos);
            var endLocal = WorldToLocalPoint(endWorldPos);

            // ベジェ曲線の制御点を計算
            var controlPoint = (startLocal + endLocal) / 2f;
            controlPoint.y += curveHeight;

            // 曲線のポイントを計算
            for (var i = 0; i <= segmentCount; i++)
            {
                var t = i / (float)segmentCount;
                _points[i] = CalculateQuadraticBezierPoint(t, startLocal, controlPoint, endLocal);
            }

            lineRenderer.SetPositions(_points);
        }

        public void Hide() => lineRenderer.gameObject.SetActive(false);

        private Vector2 WorldToLocalPoint(Vector3 worldPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform,
                RectTransformUtility.WorldToScreenPoint(_camera, worldPos),
                _camera,
                out var localPoint);
            return localPoint;
        }

        /// <summary>
        /// 二次ベジェ曲線上の点を計算
        /// </summary>
        private static Vector2 CalculateQuadraticBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            var u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
        }
    }
}
