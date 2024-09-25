﻿using System.Collections.Generic;
using Pathfinder;
using Pathfinder.Graph;
using Pathfinder.Voronoi;
using StateMachine.Agents.RTS;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [Header("Map Config")] 
        [SerializeField] private int mapWidth;
        [SerializeField] private int mapHeight;
        [SerializeField] private int minesQuantity;
        [SerializeField] private float nodesSize;
        [SerializeField] private Vector2 originPosition;

        [Header("Units Config")] 
        [SerializeField] private GameObject minerPrefab;
        [SerializeField] private GameObject caravanPrefab;
        [SerializeField] private GameObject minePrefab;
        [SerializeField] private GameObject townCenterPrefab;
        [SerializeField] private int minersQuantity;
        [SerializeField] private int cartsQuantity;

        public static Graph<Node<Vector2>, NodeVoronoi, Vector2> Graph;

        private Voronoi<NodeVoronoi, Vector2> voronoi;
        private Color color;
        private void Start()
        {
            Miner.OnEmptyMine += RemakeVoronoi;
            if (!Application.isPlaying)
                return;
            
            color.a = 0.2f;

            Graph<Node<Vector2>, NodeVoronoi, Vector2>.OriginPosition = new NodeVoronoi(originPosition);

            Graph = new Vector2Graph(mapWidth, mapHeight, nodesSize);

            voronoi = new Voronoi<NodeVoronoi, Vector2>();

            for (int i = 0; i < minesQuantity; i++)
            {
                int rand = Random.Range(0, Graph.CoordNodes.Count);
                Node<Vector2> node = Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[rand];
                node.NodeType = NodeType.Mine;
                node.gold = 100;
                Graph<Node<Vector2>, NodeVoronoi, Vector2>.mines.Add(node);
            }

            int towncenterNode = Random.Range(0, Graph.CoordNodes.Count);
            Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[towncenterNode].NodeType = NodeType.TownCenter;

            Vector3 townCenterPosition = new Vector3(Graph.CoordNodes[towncenterNode].GetCoordinate().x,
                Graph.CoordNodes[towncenterNode].GetCoordinate().y);

            List<NodeVoronoi> voronoiNodes = new List<NodeVoronoi>();
            foreach (var t in Graph<Node<Vector2>, NodeVoronoi, Vector2>.mines)
            {
                voronoiNodes.Add(Graph.CoordNodes.Find((node => node.GetCoordinate() == t.GetCoordinate())));
            }
            voronoi.Init();

            GameObject miner = Instantiate(minerPrefab, townCenterPosition, Quaternion.identity);
            Miner agent = miner.GetComponent<Miner>();
            agent.CurrentNode = Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[towncenterNode];
            RTSAgent.TownCenter = Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[towncenterNode];
            agent.Voronoi = voronoi;
            agent.Init();
            GameObject caravan = Instantiate(caravanPrefab, townCenterPosition, Quaternion.identity);
            Caravan agent2 = caravan.GetComponent<Caravan>();
            agent2.CurrentNode = Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[towncenterNode];
            agent2.Voronoi = voronoi;
            agent2.Init();
            voronoi.SetVoronoi(voronoiNodes);
        }

        private void RemakeVoronoi()
        {
            List<NodeVoronoi> voronoiNodes = new List<NodeVoronoi>();
            foreach (var mine in Graph<Node<Vector2>, NodeVoronoi, Vector2>.mines)
            {
                if (mine.gold > 0)
                    voronoiNodes.Add(Graph.CoordNodes.Find(node => node.GetCoordinate() == mine.GetCoordinate()));
            }
            voronoi.SetVoronoi(voronoiNodes);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            foreach (var sector in voronoi.SectorsToDraw())
            {
                Handles.color = color;
                List<Vector3> list = new List<Vector3>();
                foreach (var nodeVoronoi in sector.PointsToDraw()) list.Add(new Vector3(nodeVoronoi.GetX(), nodeVoronoi.GetY()));
                Handles.DrawAAConvexPolygon(list.ToArray());

                Handles.color = Color.black;
                Handles.DrawPolyLine(list.ToArray());
            }

            foreach (var node in Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType)
            {
                Gizmos.color = node.NodeType switch
                {
                    NodeType.Mine => Color.yellow,
                    NodeType.Empty => Color.white,
                    NodeType.TownCenter => Color.blue,
                    NodeType.Blocked => Color.red,
                    _ => Color.white
                };

                Gizmos.DrawSphere(new Vector3(node.GetCoordinate().x, node.GetCoordinate().y), nodesSize);
            }
        }
    }
}