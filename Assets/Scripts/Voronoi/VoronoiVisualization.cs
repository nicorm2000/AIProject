using UnityEngine;
using System.Collections.Generic;

public class VoronoiVisualization : MonoBehaviour
{
    public Rect boundingBox = new Rect(-5, -5, 10, 10);
    public int numPoints = 10;
    public float maxWeight = 3f;
    public GameObject linePrefab;
    public GameObject pointPrefab;
    private WeightedVoronoi voronoi;

    void Start()
    {
        voronoi = new WeightedVoronoi();

        for (int i = 0; i < numPoints; i++)
        {
            float x = Random.Range(boundingBox.xMin, boundingBox.xMax);
            float y = Random.Range(boundingBox.yMin, boundingBox.yMax);
            float weight = Random.Range(0.5f, maxWeight);
            voronoi.AddPoint(x, y, weight);
        }

        List<VoronoiCell> cells = voronoi.ComputeDiagram(boundingBox);
        DrawVoronoi(cells);
        DrawPoints();
    }

    void DrawVoronoi(List<VoronoiCell> cells)
    {
        foreach (var cell in cells)
        {
            List<Vector2> polygon = new List<Vector2>();

            foreach (var edge in cell.Edges)
            {
                // Clip and store each edge's start and end points
                Vector2 clippedStart = edge.Start;
                Vector2 clippedEnd = edge.End;

                if (ClipEdgeToBounds(ref clippedStart, ref clippedEnd, boundingBox))
                {
                    polygon.Add(clippedStart);
                    polygon.Add(clippedEnd);

                    // Draw the clipped line
                    Color startColor = GetColorBasedOnWeight(cell.Site.Weight);
                    DrawEdge(clippedStart, clippedEnd, startColor, startColor);
                }
            }

            // If polygon points were collected, draw the polygon (Voronoi cell)
            if (polygon.Count > 2)
            {
                DrawPolygon(polygon, GetColorBasedOnWeight(cell.Site.Weight));
            }
        }
    }

    void DrawPoints()
    {
        foreach (var point in voronoi.Points)
        {
            GameObject pointMarker = Instantiate(pointPrefab, new Vector3(point.X, point.Y, 0), Quaternion.identity);
            Renderer renderer = pointMarker.GetComponent<Renderer>();
            renderer.material.color = GetColorBasedOnWeight(point.Weight);
        }
    }

    bool ClipEdgeToBounds(ref Vector2 start, ref Vector2 end, Rect bounds)
    {
        float t0 = 0.0f;
        float t1 = 1.0f;
        Vector2 direction = end - start;

        // Liang-Barsky algorithm for clipping
        for (int edge = 0; edge < 4; edge++)
        {
            float p = 0, q = 0;
            if (edge == 0) { p = -direction.x; q = -(bounds.xMin - start.x); }
            if (edge == 1) { p = direction.x; q = bounds.xMax - start.x; }
            if (edge == 2) { p = -direction.y; q = -(bounds.yMin - start.y); }
            if (edge == 3) { p = direction.y; q = bounds.yMax - start.y; }

            if (p == 0 && q < 0) return false; // Parallel and outside

            float r = q / p;
            if (p < 0) { if (r > t1) return false; else if (r > t0) t0 = r; }
            else if (p > 0) { if (r < t0) return false; else if (r < t1) t1 = r; }
        }

        Vector2 newStart = start + t0 * direction;
        Vector2 newEnd = start + t1 * direction;

        start = newStart;
        end = newEnd;

        return true;
    }

    void DrawEdge(Vector2 start, Vector2 end, Color startColor, Color endColor)
    {
        GameObject line = Instantiate(linePrefab);
        LineRenderer lr = line.GetComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.SetPosition(0, new Vector3(start.x, start.y, 0));
        lr.SetPosition(1, new Vector3(end.x, end.y, 0));
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;

        lr.startColor = startColor;
        lr.endColor = endColor;
    }

    void DrawPolygon(List<Vector2> polygon, Color color)
    {
        for (int i = 0; i < polygon.Count; i += 2)
        {
            Vector2 start = polygon[i];
            Vector2 end = polygon[(i + 1) % polygon.Count];
            DrawEdge(start, end, color, color);
        }
    }

    Color GetColorBasedOnWeight(float weight)
    {
        float normalizedWeight = Mathf.InverseLerp(0.5f, maxWeight, weight);
        return Color.Lerp(Color.blue, Color.red, normalizedWeight);
    }
}
