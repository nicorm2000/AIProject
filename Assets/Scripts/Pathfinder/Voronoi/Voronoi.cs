using System;
using System.Collections.Generic;
using System.Linq;
using Game;

namespace Pathfinder.Voronoi
{
    /// <summary>
    /// Represents a Voronoi diagram generator for coordinate systems. 
    /// Manages map boundaries (limits) and sectors for spatial partitioning.
    /// </summary>
    /// <typeparam name="TCoordinate">The type of coordinate used.</typeparam>
    /// <typeparam name="TCoordinateType">The type of the coordinate's value type (e.g., float, int).</typeparam>
    public class Voronoi<TCoordinate, TCoordinateType>
        where TCoordinate : IEquatable<TCoordinate>, ICoordinate<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>
    {
        private readonly List<Limit<TCoordinate, TCoordinateType>> limits = new();
        private readonly List<Sector<TCoordinate, TCoordinateType>> sectors = new();

        /// <summary>
        /// Initializes the Voronoi diagram by setting up the map limits.
        /// </summary>
        public void Init()
        {
            InitLimits();
        }

        /// <summary>
        /// Initializes the map boundaries using the map's size and origin.
        /// Sets the limits (Up, Down, Left, Right) based on the map dimensions.
        /// </summary>
        private void InitLimits()
        {
            // Calculate the map boundaries using its dimensions, node spacing, and origin point
            TCoordinate mapSize = new TCoordinate();
            mapSize.SetCoordinate(MapGenerator<TCoordinate, TCoordinateType>.MapDimensions.GetCoordinate());
            mapSize.Multiply(MapGenerator<TCoordinate, TCoordinateType>.CellSize); // Scaling map by cell size
            TCoordinate offset = new TCoordinate();
            offset.SetCoordinate(MapGenerator<TCoordinate, TCoordinateType>.OriginPosition.GetCoordinate());

            // Set the boundary in the Up direction
            TCoordinate coordinateUp = new TCoordinate();
            coordinateUp.SetCoordinate(0, mapSize.GetY());
            coordinateUp.Add(offset.GetCoordinate());
            limits.Add(new Limit<TCoordinate, TCoordinateType>(coordinateUp, Direction.Up));

            // Set the boundary in the Down direction
            TCoordinate coordinateDown = new TCoordinate();
            coordinateDown.SetCoordinate(mapSize.GetX(), 0f);
            coordinateDown.Add(offset.GetCoordinate());
            limits.Add(new Limit<TCoordinate, TCoordinateType>(coordinateDown, Direction.Down));

            // Set the boundary in the Right direction
            TCoordinate coordinateRight = new TCoordinate();
            coordinateRight.SetCoordinate(mapSize.GetX(), mapSize.GetY());
            coordinateRight.Add(offset.GetCoordinate());
            limits.Add(new Limit<TCoordinate, TCoordinateType>(coordinateRight, Direction.Right));

            // Set the boundary in the Left direction
            TCoordinate coordinateLeft = new TCoordinate();
            coordinateLeft.SetCoordinate(0, 0);
            coordinateLeft.Add(offset.GetCoordinate());
            limits.Add(new Limit<TCoordinate, TCoordinateType>(coordinateLeft, Direction.Left));
        }

        /// <summary>
        /// Initializes the Voronoi sectors using the provided gold mines coordinates.
        /// Each gold mine becomes a sector, and segments between sectors are calculated.
        /// </summary>
        /// <param name="goldMines">List of coordinates representing the gold mines.</param>
        public void SetVoronoi(List<TCoordinate> goldMines)
        {
            sectors.Clear();
            if (goldMines.Count <= 0) return;

            // Add each gold mine as a sector
            foreach (var mine in goldMines)
            {
                Node<TCoordinateType> node = new Node<TCoordinateType>();
                node.SetCoordinate(mine.GetCoordinate());
                sectors.Add(new Sector<TCoordinate, TCoordinateType>(node));
            }

            // Add the map boundaries (limits) to each sector
            foreach (var sector in sectors)
            {
                sector.AddSegmentLimits(limits);
            }

            // Add segments between each pair of sectors (excluding itself)
            for (int i = 0; i < goldMines.Count; i++)
            {
                for (int j = 0; j < goldMines.Count; j++)
                {
                    if (i == j) continue; // Skip segment to itself
                    sectors[i].AddSegment(goldMines[i], goldMines[j]);
                }
            }

            // Calculate intersections between the sector boundaries
            foreach (var sector in sectors)
            {
                sector.SetIntersections();
            }
        }

        /// <summary>
        /// Finds the closest gold mine to the given agent position.
        /// </summary>
        /// <param name="agentPosition">The agent's current position.</param>
        /// <returns>The closest gold mine node, or null if no sectors are found.</returns>
        public Node<TCoordinateType> GetMineCloser(TCoordinate agentPosition)
        {
            // Calculate which mine is closest to the given position
            return sectors != null ?
                (from sector in sectors
                 where sector.CheckPointInSector(agentPosition)
                 select sector.Mine).FirstOrDefault() : null;
        }

        /// <summary>
        /// Retrieves the list of sectors for drawing purposes.
        /// </summary>
        /// <returns>A list of sectors.</returns>
        public List<Sector<TCoordinate, TCoordinateType>> SectorsToDraw()
        {
            return sectors;
        }
    }
}