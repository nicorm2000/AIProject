﻿        using System;
using System.Collections.Generic;

namespace Pathfinder.Graph
{
    public abstract class SimGraph<TNodeType, TCoordinateNode, TCoordinateType>
        where TNodeType : INode<TCoordinateType>
        where TCoordinateNode : ICoordinate<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>
    {
        public static TCoordinateNode MapDimensions;
        public static TCoordinateNode OriginPosition;
        public static float CellSize;
        public TCoordinateNode[,] CoordNodes;
        public readonly TNodeType[,] NodesType;

        public SimGraph(int x, int y, float cellSize)
        {
            MapDimensions = new TCoordinateNode();
            MapDimensions.SetCoordinate(x, y);
            CellSize = cellSize;

            CoordNodes = new TCoordinateNode[x, y];
            NodesType = new TNodeType[x, y];

            CreateGraph(x, y, cellSize);

            AddNeighbors(cellSize);
        }

        public abstract void CreateGraph(int x, int y, float cellSize);

        private void AddNeighbors(float cellSize)
        {
            var neighbors = new List<INode<TCoordinateType>>();

            for (var i = 0; i < CoordNodes.GetLength(0); i++)
            {
                for (var j = 0; j < CoordNodes.GetLength(1); j++)
                {
                    neighbors.Clear();
                    for (var k = 0; k < CoordNodes.GetLength(0); k++)
                    {
                        for (var l = 0; l < CoordNodes.GetLength(1); l++)
                        {
                            if (i == k && j == l) continue;

                            var isNeighbor =
                                (Approximately(CoordNodes[i, j].GetX(), CoordNodes[k, l].GetX()) &&
                                 Approximately(Math.Abs(CoordNodes[i, j].GetY() - CoordNodes[k, l].GetY()), cellSize)) ||
                                (Approximately(CoordNodes[i, j].GetY(), CoordNodes[k, l].GetY()) &&
                                 Approximately(Math.Abs(CoordNodes[i, j].GetX() - CoordNodes[k, l].GetX()), cellSize));

                            if (isNeighbor) neighbors.Add(NodesType[k, l]);
                        }
                    }

                    NodesType[i, j].SetNeighbors(new List<INode<TCoordinateType>>(neighbors));
                }
            }
        }

        public bool Approximately(float a, float b)
        {
            return Math.Abs(a - b) < 1e-6f;
        }
    }
}