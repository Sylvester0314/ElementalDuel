using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public enum LayoutType
{
    Horizontal,
    Vertical
}

public enum AlignmentType
{
    Left,
    Center,
    Right
}

[ExecuteAlways]
public class AutomaticLayout : MonoBehaviour
{
    public bool reverse;
    public bool previewInEditMode;
    public LayoutType layoutType = LayoutType.Horizontal;
    public AlignmentType alignmentType = AlignmentType.Center;
    public float spacing;
    public float scale = 1;
    public float xOffset;
    public float yOffset;
    public float zOffset;

    public List<RectTransform> childrenRT = new ();
    public List<Transform> children = new ();
    public List<Vector3> originPositions = new ();
    public int count;

    private Vector3 _offset;
    private float _totalLength;
    private List<float> _lengthList;

    private void Loop(int min, int max, Action<int> action, bool descending = false)
    {
        var indexes = Enumerable.Range(min, max).ToList();
        if (descending)
            indexes.Reverse();
        if (reverse)
            indexes.Reverse();
        
        indexes.ForEach(action);
    }
    
    public void RefreshOriginPosition()
    {
        originPositions.Clear();
        Loop(0, count, i =>
        {
            var position = transform.GetChild(i).localPosition;
            if (position != Vector3.zero)
                position -= _offset;
            originPositions.Add(position);
        });
    }
    
    public void InitLayoutData()
    {
        count = transform.childCount;
        children.Clear();
        childrenRT.Clear();
        Loop(0, count, i =>
        {
            var child = transform.GetChild(i);
            var canvas = child.Find("CanvasBody");
            var rect = canvas.GetComponent<RectTransform>();
            children.Add(child);
            childrenRT.Add(rect);
        });
    }

    public void Awake()
    {
        InitLayoutData();
        RefreshOriginPosition();
    }

    public void SetOffset(float x, float y, float z)
    {
        xOffset = x;
        yOffset = y;
        zOffset = z;
    }

    public void SetOffset(Vector3 offset)
    {
        xOffset = offset.x;
        yOffset = offset.y;
        zOffset = offset.z;
    }

    private float GetLength(RectTransform rectTransform)
    {
        var result = layoutType switch
        {
            LayoutType.Horizontal => rectTransform.rect.width * scale,
            LayoutType.Vertical => rectTransform.rect.height * scale,
            _ => 0
        };
        return result;
    }

    private Vector3 CalculatePosition(float current, Vector3 position)
    {
        var result = layoutType switch
        {
            LayoutType.Horizontal => new Vector3(current, position.y, position.z),
            LayoutType.Vertical => new Vector3(position.x, current, position.z),
            _ => Vector3.zero
        };
        return result;
    }

    private void LayoutPrepare()
    {
        _offset = new Vector3(xOffset, yOffset, zOffset);
        _totalLength = spacing * (count - 1);
        _lengthList = new List<float>();

        var most = children
            .GroupBy(trans => 
                layoutType == LayoutType.Horizontal 
                    ? trans.localScale.x 
                    : trans.localScale.y
                    )
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();
        
        if (most == null)
            return;
        
        Loop(0, count, i =>
        {
            var length = GetLength(childrenRT[i]) * most.Key;
            _totalLength += length;
            _lengthList.Add(length);
        });
    }
    
    private List<Vector3> CalculateCenterAlignmentLayout()
    {
        LayoutPrepare();
        var results = new List<Vector3>();
        if (count == 0)
            return results;
        
        var current = (_lengthList[0] - _totalLength) / 2;
        Loop(0, count, i =>
        {
            var relative = CalculatePosition(current, originPositions[i]);
            var position = transform.TransformPoint(_offset + relative);
            results.Add(position);
            current += spacing + _lengthList[i];
        });

        return results;
    }

    public void MovementLayout(float duration, Ease ease = Ease.OutQuad, List<Transform> excepted = null)
    {
        var positions = CalculateLayout();
        Loop(0, count, i =>
        {
            if (excepted != null && excepted.Contains(children[i]))
                return;
            children[i]
                .DOMove(positions[i], duration)
                .SetEase(ease);
        }, true);
    }

    public void StaticLayout()
    {
        var positions = CalculateLayout();
        Loop(0, count, i =>
        {
            children[i].position = positions[i];
        }, true);
    }

    public List<Vector3> CalculateLayout()
    {
        return alignmentType switch
        {
            AlignmentType.Left => new List<Vector3>(),
            AlignmentType.Center => CalculateCenterAlignmentLayout(),
            AlignmentType.Right => new List<Vector3>(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public void Update()
    {
        if (count == 0)
            return;
        if (!previewInEditMode)
            return;
        
        if (count != transform.childCount)
            InitLayoutData();
        
        RefreshOriginPosition();
        StaticLayout();
    }
}