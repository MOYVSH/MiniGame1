using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

public class UICircleImage : Image
{
    protected override void Awake()
    {
        base.Awake();
        OnRectTransformDimensionsChange();
    }

    private Rect _rect;
    protected override void OnRectTransformDimensionsChange()
    {
        _rect = rectTransform.rect;
        Thickness = Mathf.Clamp(Thickness, 0, _rect.width / 2);
        base.OnRectTransformDimensionsChange();
    }

    [Tooltip("Բ�λ�����������")] [Range(0, 1)] public float FillPercent = 1f;
    [Tooltip("�Ƿ����Բ��")] public bool Fill = true;
    [Tooltip("Բ�����")] public float Thickness = 5;
    [Tooltip("Բ��")] [Range(3, 100)] public int Segements = 20;

    private static readonly float CenterPivot = 0.5f;
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        _CacheData();

        float degreeDelta = 2 * Mathf.PI / Segements;
        /* ��������ȡ�����������л����ε�ʱ����������
         * ����FillPercent != 1ʱ�����ϻ�����������
         */
        int curSegements = Mathf.CeilToInt(Segements * FillPercent);
        // ȡ�����̵�һ��Ϊֱ��
        float outerRadius = 0.5f * Mathf.Min(_rect.width, _rect.height);
        // ʵ��Բ�������Բ������
        float centerX = (rectTransform.pivot.x - CenterPivot) * _rect.width;
        float centerY = (rectTransform.pivot.y - CenterPivot) * _rect.height;

        float curDegree = 0;
        if (Fill) //Բ��
        {
            int verticeCount = curSegements + 1;
            // Բ��
            _AddVert(vh, centerX, centerY);
            for (int i = 1; i < verticeCount; i++)
            {
                float cosA = Mathf.Cos(curDegree);
                float sinA = Mathf.Sin(curDegree);
                curDegree += degreeDelta;
                _AddVert(vh, centerX + cosA * outerRadius, centerY + sinA * outerRadius);
            }

            int triangleCount = curSegements * 3;
            for (int i = 0, vIdx = 1; i < triangleCount - 3; i += 3, vIdx++)
            {
                vh.AddTriangle(vIdx, 0, vIdx + 1);
            }

            if (FillPercent == 1) //��β��������
            {
                vh.AddTriangle(verticeCount - 1, 0, 1);
            }
        }
        else //Բ��
        {
            float innerRadius = outerRadius - Thickness;

            int verticeCount = curSegements * 2;
            for (int i = 0; i < verticeCount; i += 2)
            {
                float cosA = Mathf.Cos(curDegree);
                float sinA = Mathf.Sin(curDegree);
                curDegree += degreeDelta;

                _AddVert(vh, centerX + cosA * innerRadius, centerY + sinA * innerRadius);
                _AddVert(vh, centerX + cosA * outerRadius, centerY + sinA * outerRadius);
            }

            int triangleCount = curSegements * 3 * 2;
            for (int i = 0, vIdx = 0; i < triangleCount - 6; i += 6, vIdx += 2)
            {
                vh.AddTriangle(vIdx + 1, vIdx, vIdx + 3);
                vh.AddTriangle(vIdx, vIdx + 2, vIdx + 3);
            }

            if (FillPercent == 1) //��β��������
            {
                vh.AddTriangle(verticeCount - 1, verticeCount - 2, 1);
                vh.AddTriangle(verticeCount - 2, 0, 1);
            }
        }
    }

    /// <summary>
    /// ����һЩMesh��������ݣ���Щ����ÿ��ȡ������һ���ĺ�ʱ
    /// </summary>
    private void _CacheData()
    {
        _tmpVert.color = color;

        Vector4 uv = overrideSprite != null ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;
        _uvMinX = uv.x;
        _uvMinY = uv.y;
        _uvScaleX = (uv.z - uv.x) / _rect.width;
        _uvScaleY = (uv.w - uv.y) / _rect.height;

        _rectXMin = _rect.xMin;
        _rectXMax = _rect.xMax;
        _rectYMin = _rect.yMin;
        _rectYMax = _rect.yMax;
    }

    // UV�ֲ���Rect�ϵĵ�λ����
    private float _uvScaleX;
    private float _uvScaleY;
    // UV��Χ
    private float _uvMinX;
    private float _uvMinY;
    // RectTransform��Χ
    private float _rectXMin;
    private float _rectXMax;
    private float _rectYMin;
    private float _rectYMax;

    private UIVertex _tmpVert = UIVertex.simpleVert;
    private Vector2 _tmpV2 = Vector2.zero;
    private Vector3 _tmpV3 = Vector3.zero;

    private void _AddVert(VertexHelper vh, float vertX, float vertY)
    {
        // ��Clamp��Ϊ�˷�ֹԲ��Mesh����RectTransform�ķ�Χ
        float posX = Mathf.Clamp(vertX, _rectXMin, _rectXMax);
        float posY = Mathf.Clamp(vertY, _rectYMin, _rectYMax);

        _tmpV3.x = posX;
        _tmpV3.y = posY;
        _tmpVert.position = _tmpV3;
        /* ����ʵ�ֵ�Ч���ǣ�������Sprite��Rect��ס�����м��ڳ�һ��Բ�εĿɼ�����
         * ���ʵ�ָ��ӽ�Mask��Ч��
        */
        _tmpV2.x = _uvMinX + _uvScaleX * (posX - _rectXMin);
        _tmpV2.y = _uvMinY + _uvScaleY * (posY - _rectYMin);
        _tmpVert.uv0 = _tmpV2;

        vh.AddVert(_tmpVert);
    }
}
