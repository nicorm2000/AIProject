﻿using System;
using System.Collections.Generic;

namespace Pathfinder.Graph
{

    public abstract class Graph<TNodeType, TCoordinateNode, TCoordinateType>
        where TNodeType : INode<TCoordinateType>, new()
        where TCoordinateNode : ICoordinate<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>, new()
    {
        public readonly List<TCoordinateNode> CoordNodes = new List<TCoordinateNode>();
        public readonly List<TNodeType> NodesType = new List<TNodeType>();

        public Graph(int x, int y, float cellSize)
        {
            CreateGraph(x, y, cellSize);

            AddNeighbors(cellSize);
        }

        public abstract void CreateGraph(int x, int y, float cellSize);

        private void AddNeighbors(float cellSize)
        {
            List<INode<TCoordinateType>> neighbors = new List<INode<TCoordinateType>>();

            for (int i = 0; i < CoordNodes.Count; i++)
            {
                neighbors.Clear();
                for (int j = 0; j < CoordNodes.Count; j++)
                {
                    if (i == j) continue;
                    if ((Approximately(CoordNodes[i].GetX(), CoordNodes[j].GetX()) &&
                         Approximately(Math.Abs(CoordNodes[i].GetY() - CoordNodes[j].GetY()), cellSize)) ||
                        (Approximately(CoordNodes[i].GetY(), CoordNodes[j].GetY()) &&
                         Approximately(Math.Abs(CoordNodes[i].GetX() - CoordNodes[j].GetY()), cellSize)) ||
                        (Approximately(Math.Abs(CoordNodes[i].GetY() - CoordNodes[j].GetY()), cellSize) &&
                         Approximately(Math.Abs(CoordNodes[i].GetX() - CoordNodes[j].GetX()), cellSize)))
                    {
                        neighbors.Add(NodesType[j]);
                    }
                }
                NodesType[i].SetNeighbors(neighbors);
            }
        }

        public bool Approximately(float a, float b)
        {
            return Math.Abs(a - b) < 1e-6f;
        }
    }
}