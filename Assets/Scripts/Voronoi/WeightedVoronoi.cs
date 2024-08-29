using System.Collections.Generic;
using UnityEngine;

public class WeightedVoronoi
{
    private readonly List<VoronoiPoint> _points;
    private readonly Dictionary<(int, int), BisectorInfo> _bisectorCache;

    public WeightedVoronoi()
    {
        _points = new List<VoronoiPoint>();
        _bisectorCache = new Dictionary<(int, int), BisectorInfo>();
    }

    public List<VoronoiPoint> Points => _points;

    public void AddPoint(float x, float y, float weight = 1.0f)
    {
        _points.Add(new VoronoiPoint(x, y, weight));
    }

    private float WeightedDistance(VoronoiPoint p1, VoronoiPoint p2)
    {
        return Vector2.Distance(new Vector2(p1.X, p1.Y), new Vector2(p2.X, p2.Y)) - p1.Weight + p2.Weight;
    }

    private BisectorInfo GetBisectorInfo(int i, int j)
    {
        if (_bisectorCache.TryGetValue((i, j), out var cachedInfo))
        {
            return cachedInfo;
        }

        var point = _points[i];
        var otherPoint = _points[j];

        Vector2 midPoint = new Vector2(
            (point.X + otherPoint.X) / 2,
            (point.Y + otherPoint.Y) / 2
        );

        float dx = otherPoint.X - point.X;
        float dy = otherPoint.Y - point.Y;

        Vector2 normal = new Vector2(-dy, dx).normalized;
        float distance = WeightedDistance(point, otherPoint) / 2;
        Vector2 weightedMidPoint = midPoint + normal * distance;

        var bisectorInfo = new BisectorInfo(weightedMidPoint, normal);
        _bisectorCache[(i, j)] = bisectorInfo;
        _bisectorCache[(j, i)] = bisectorInfo;

        return bisectorInfo;
    }

    public List<VoronoiCell> ComputeDiagram(Rect boundingBox)
    {
        List<VoronoiCell> cells = new List<VoronoiCell>();

        for (int i = 0; i < _points.Count; i++)
        {
            VoronoiCell cell = new VoronoiCell(_points[i]);

            for (int j = 0; j < _points.Count; j++)
            {
                if (i == j) continue;

                var bisectorInfo = GetBisectorInfo(i, j);

                Vector2 edgeStart = bisectorInfo.MidPoint + bisectorInfo.Normal * 1000f;
                Vector2 edgeEnd = bisectorInfo.MidPoint - bisectorInfo.Normal * 1000f;

                if (ClipEdge(ref edgeStart, ref edgeEnd, boundingBox))
                {
                    VoronoiEdge edge = new VoronoiEdge(edgeStart, edgeEnd);
                    cell.Edges.Add(edge);
                }
            }

            CloseCell(cell, boundingBox);
            cells.Add(cell);
        }

        return cells;
    }

    private bool ClipEdge(ref Vector2 start, ref Vector2 end, Rect bounds)
    {
        float t0 = 0.0f;
        float t1 = 1.0f;

        Vector2 direction = end - start;

        for (int edge = 0; edge < 4; edge++)
        {
            float p = 0, q = 0, r;

            if (edge == 0) { p = -direction.x; q = start.x - bounds.xMin; }
            if (edge == 1) { p = direction.x; q = bounds.xMax - start.x; }
            if (edge == 2) { p = -direction.y; q = start.y - bounds.yMin; }
            if (edge == 3) { p = direction.y; q = bounds.yMax - start.y; }

            r = q / p;

            if (p == 0 && q < 0) return false;

            if (p < 0)
            {
                if (r > t1) return false;
                else if (r > t0) t0 = r;
            }
            else if (p > 0)
            {
                if (r < t0) return false;
                else if (r < t1) t1 = r;
            }
        }

        end = start + t1 * direction;
        start = start + t0 * direction;
        return true;
    }

    private void CloseCell(VoronoiCell cell, Rect boundingBox)
    {
        List<VoronoiEdge> closedEdges = new List<VoronoiEdge>();

        foreach (VoronoiEdge edge in cell.Edges)
        {
            Vector2 newStart = edge.Start;
            Vector2 newEnd = edge.End;

            if (!boundingBox.Contains(newStart))
            {
                ExtendEdgeToBounds(ref newStart, ref newEnd, boundingBox);
            }
            if (!boundingBox.Contains(newEnd))
            {
                ExtendEdgeToBounds(ref newEnd, ref newStart, boundingBox);
            }

            closedEdges.Add(new VoronoiEdge(newStart, newEnd));
        }

        cell.Edges = closedEdges;
    }

    private void ExtendEdgeToBounds(ref Vector2 pointToExtend, ref Vector2 referencePoint, Rect boundingBox)
    {
        Vector2 direction = pointToExtend - referencePoint;

        if (pointToExtend.x < boundingBox.xMin)
            pointToExtend = new Vector2(boundingBox.xMin, referencePoint.y + (boundingBox.xMin - referencePoint.x) * direction.y / direction.x);
        else if (pointToExtend.x > boundingBox.xMax)
            pointToExtend = new Vector2(boundingBox.xMax, referencePoint.y + (boundingBox.xMax - referencePoint.x) * direction.y / direction.x);

        if (pointToExtend.y < boundingBox.yMin)
            pointToExtend = new Vector2(referencePoint.x + (boundingBox.yMin - referencePoint.y) * direction.x / direction.y, boundingBox.yMin);
        else if (pointToExtend.y > boundingBox.yMax)
            pointToExtend = new Vector2(referencePoint.x + (boundingBox.yMax - referencePoint.y) * direction.x / direction.y, boundingBox.yMax);
    }

    private struct BisectorInfo
    {
        public Vector2 MidPoint;
        public Vector2 Normal;

        public BisectorInfo(Vector2 midPoint, Vector2 normal)
        {
            MidPoint = midPoint;
            Normal = normal;
        }
    }
}