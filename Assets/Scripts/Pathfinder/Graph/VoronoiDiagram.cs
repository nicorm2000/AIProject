using System.Collections.Generic;
using Utils;

namespace Pathfinder
{
    /// <summary>
    /// Represents a Voronoi diagram, which is a partition of a plane into regions based on distance to points in a specific subset.
    /// </summary>
    public class VoronoiDiagram
    {
        /// <summary>
        /// Gets the list of Voronoi polygons generated for the diagram.
        /// </summary>
        public List<VoronoiPolygon> Polygons { get; private set; }

        /// <summary>
        /// Constructs a VoronoiDiagram by generating Voronoi polygons based on a set of nodes (sites).
        /// </summary>
        /// <param name="nodes">The list of nodes (site points) for which to generate Voronoi polygons.</param>
        public VoronoiDiagram(List<Node<Vec2Int>> nodes)
        {
            Polygons = GenerateVoronoiPolygons(nodes);
        }

        /// <summary>
        /// Generates Voronoi polygons based on the input list of nodes.
        /// </summary>
        /// <param name="nodes">List of nodes to be used as sites for generating Voronoi polygons.</param>
        /// <returns>A list of Voronoi polygons corresponding to the given nodes.</returns>
        private List<VoronoiPolygon> GenerateVoronoiPolygons(List<Node<Vec2Int>> nodes)
        {
            List<VoronoiPolygon> polygons = new List<VoronoiPolygon>();

            foreach (var node in nodes)
            {
                var polygon = new VoronoiPolygon(node);
                polygons.Add(polygon);
            }

            return polygons;
        }
    }

    /// <summary>
    /// Represents a polygon in the Voronoi diagram, with a site and a list of vertices forming its boundaries.
    /// </summary>
    public class VoronoiPolygon
    {
        /// <summary>
        /// Gets the site (central node) of the Voronoi polygon.
        /// </summary>
        public Node<Vec2Int> Site { get; private set; }

        /// <summary>
        /// Gets the list of vertices that form the boundary of the Voronoi polygon.
        /// </summary>
        public List<Vec2Int> Vertices { get; private set; }

        /// <summary>
        /// Constructs a VoronoiPolygon for a given site, initializing an empty list of vertices.
        /// </summary>
        /// <param name="site">The node representing the site of the Voronoi polygon.</param>
        public VoronoiPolygon(Node<Vec2Int> site)
        {
            Site = site;
            Vertices = new List<Vec2Int>();
        }
    }
}