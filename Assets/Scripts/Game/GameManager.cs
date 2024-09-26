using System.Collections.Generic;
using Pathfinder;
using Pathfinder.Graph;
using Pathfinder.Voronoi;
using StateMachine.Agents.RTS;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
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
        [SerializeField] private GameObject goldminePrefab;
        [SerializeField] private GameObject townCenterPrefab;
        [SerializeField] private GameObject grassPrefab;
        [SerializeField] private GameObject forestPrefab;
        [SerializeField] private GameObject dirtPrefab;
        [SerializeField] private GameObject treeCutDownPrefab;
        [SerializeField] private int minersQuantity;
        [SerializeField] private int caravansQuantity;

        [Header("Alarm Config")]
        [SerializeField] private Button alarmButton;

        public static Graph<Node<Vector2>, NodeVoronoi, Vector2> Graph;
        public static List<Node<Vector2>> MinesWithMiners = new List<Node<Vector2>>();
        public static AStarPathfinder<Node<Vector2>, Vector2, NodeVoronoi> Pathfinder;

        private Voronoi<NodeVoronoi, Vector2> voronoi;
        private Color color;

        private List<GameObject> visuals = new List<GameObject>();
        private bool updateVisuals = false;

        /// <summary>
        /// Initializes the game state, sets up the mines, units, and visual representation of the map.
        /// </summary>
        private void Start()
        {
            Miner.OnEmptyMine += RemakeVoronoi;
            Miner.OnReachMine += OnReachMine;
            Miner.OnLeaveMine += OnLeaveMine;

            alarmButton.onClick.AddListener(Retreat);
            color.a = 0.2f;
            Graph<Node<Vector2>, NodeVoronoi, Vector2>.OriginPosition = new NodeVoronoi(originPosition);

            Graph = new Vector2Graph(mapWidth, mapHeight, nodesSize);
            voronoi = new Voronoi<NodeVoronoi, Vector2>();

            AmountSafeChecks();
            SetupObstacles();

            int towncenterNode = CreateMines(out var townCenterPosition);
            Pathfinder = new AStarPathfinder<Node<Vector2>, Vector2, NodeVoronoi>(Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType);
            VoronoiSetup();

            for (int i = 0; i < minersQuantity; i++)
            {
                CreateMiner(townCenterPosition, towncenterNode);
            }

            for (int i = 0; i < caravansQuantity; i++)
            {
                CreateCaravan(townCenterPosition, towncenterNode);
            }

            AddVisuals();
        }

        /// <summary>
        /// Updates the visuals in the game each frame if necessary.
        /// </summary>
        private void Update()
        {
            if (updateVisuals)
            {
                AddVisuals();

                updateVisuals = false;
            }
        }

        /// <summary>
        /// Handles the event when a miner reaches a mine, updating the list of mines with miners.
        /// </summary>
        /// <param name="node">The node representing the mine that was reached.</param>
        private void OnReachMine(Node<Vector2> node)
        {
            RemoveEmptyNodes();
            MinesWithMiners.Add(node);
        }

        /// <summary>
        /// Handles the event when a miner leaves a mine, updating the list of mines with miners.
        /// </summary>
        /// <param name="node">The node representing the mine that was left.</param>
        private void OnLeaveMine(Node<Vector2> node)
        {
            MinesWithMiners.Remove(node);
            RemoveEmptyNodes();
        }

        /// <summary>
        /// Removes empty nodes from the list of mines with miners.
        /// </summary>
        public void RemoveEmptyNodes()
        {
            MinesWithMiners.RemoveAll(node => node.NodeType == NodeType.Empty);
        }

        /// <summary>
        /// Randomly sets up obstacles (forests, cut down trees, and dirt) in the graph.
        /// </summary>
        private void SetupObstacles()
        {
            for (int i = 0; i < Graph.CoordNodes.Count; i++)
            {
                if (Random.Range(0, 100) < 10)
                {
                    Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[i].NodeType = NodeType.Forest;
                }
            }
            for (int i = 0; i < Graph.CoordNodes.Count; i++)
            {
                if (Random.Range(0, 100) < 10)
                {
                    Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[i].NodeType = NodeType.TreeCutDown;
                }
            }
            for (int i = 0; i < Graph.CoordNodes.Count; i++)
            {
                if (Random.Range(0, 100) < 10)
                {
                    Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[i].NodeType = NodeType.Dirt;
                }
            }
        }

        /// <summary>
        /// Initializes the Voronoi diagram based on the current graph's mines.
        /// </summary>
        private void VoronoiSetup()
        {
            List<NodeVoronoi> voronoiNodes = new List<NodeVoronoi>();

            foreach (var t in Graph<Node<Vector2>, NodeVoronoi, Vector2>.mines)
            {
                voronoiNodes.Add(Graph.CoordNodes.Find((node => node.GetCoordinate() == t.GetCoordinate())));
            }

            voronoi.Init();
            voronoi.SetVoronoi(voronoiNodes);
        }

        /// <summary>
        /// Creates mines on the graph and determines the position of the town center.
        /// </summary>
        /// <param name="townCenterPosition">Outputs the position of the town center.</param>
        /// <returns>The index of the town center node.</returns>
        private int CreateMines(out Vector3 townCenterPosition)
        {
            for (int i = 0; i < minesQuantity; i++)
            {
                int rand = Random.Range(0, Graph.CoordNodes.Count);
                if (Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[rand].NodeType == NodeType.Mine) continue;
                Node<Vector2> node = Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[rand];
                node.NodeType = NodeType.Mine;
                node.gold = 100;
                Graph<Node<Vector2>, NodeVoronoi, Vector2>.mines.Add(node);
            }

            int towncenterNode = Random.Range(0, Graph.CoordNodes.Count);
            Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[towncenterNode].NodeType = NodeType.TownCenter;
            townCenterPosition = new Vector3(Graph.CoordNodes[towncenterNode].GetCoordinate().x, Graph.CoordNodes[towncenterNode].GetCoordinate().y);
            return towncenterNode;
        }

        /// <summary>
        /// Ensures that the quantities of mines, miners, and caravans are within valid ranges.
        /// </summary>
        private void AmountSafeChecks()
        {
            if (minesQuantity <= 0) minesQuantity = 1;
            if (minesQuantity > Graph.CoordNodes.Count) minesQuantity = Graph.CoordNodes.Count;
            if (minersQuantity <= 0) minersQuantity = 1;
            if (caravansQuantity <= 0) caravansQuantity = 1;
        }

        /// <summary>
        /// Creates a caravan unit at the specified town center position.
        /// </summary>
        /// <param name="townCenterPosition">The position where the caravan will be created.</param>
        /// <param name="towncenterNode">The index of the town center node.</param>
        private void CreateCaravan(Vector3 townCenterPosition, int towncenterNode)
        {
            GameObject caravan = Instantiate(caravanPrefab, townCenterPosition, Quaternion.identity);
            Caravan agent2 = caravan.GetComponent<Caravan>();
            agent2.CurrentNode = Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[towncenterNode];
            agent2.Voronoi = voronoi;
            agent2.Init();
        }

        /// <summary>
        /// Creates a miner unit at the specified town center position.
        /// </summary>
        /// <param name="townCenterPosition">The position where the miner will be created.</param>
        /// <param name="towncenterNode">The index of the town center node.</param>
        private void CreateMiner(Vector3 townCenterPosition, int towncenterNode)
        {
            GameObject miner = Instantiate(minerPrefab, townCenterPosition, Quaternion.identity);
            Miner agent = miner.GetComponent<Miner>();
            agent.CurrentNode = Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[towncenterNode];
            RTSAgent.TownCenter = Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[towncenterNode];
            agent.Voronoi = voronoi;
            agent.Init();
        }

        /// <summary>
        /// Makes the miners and caravans retreat to their base when triggered.
        /// </summary>
        private void Retreat()
        {
            RTSAgent.Retreat = !RTSAgent.Retreat;
        }

        /// <summary>
        /// Remakes the Voronoi diagram when necessary.
        /// </summary>
        private void RemakeVoronoi()
        {
            List<NodeVoronoi> voronoiNodes = new List<NodeVoronoi>();
            Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType.ForEach(node =>
            {
                if (node.NodeType == NodeType.Mine && node.gold <= 0) node.NodeType = NodeType.Empty;
            });
            foreach (var mine in Graph<Node<Vector2>, NodeVoronoi, Vector2>.mines)
            {
                if (mine.gold > 0) voronoiNodes.Add(Graph.CoordNodes.Find(node => node.GetCoordinate() == mine.GetCoordinate()));
            }

            voronoi.SetVoronoi(voronoiNodes);
            updateVisuals = true;
        }

        /// <summary>
        /// Instantiates visual representations of the mines, units, and the environment.
        /// </summary>
        private void AddVisuals()
        {
            for (int i = 0; i < visuals.Count; i++)
            {
                Destroy(visuals[i]);
            }

            visuals.Clear();

            foreach (var node in Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType)
            {
                GameObject gameObject = null;

                switch (node.NodeType)
                {
                    case NodeType.Empty:
                        gameObject = grassPrefab;
                        break;
                    case NodeType.Forest:
                        gameObject = forestPrefab;
                        break;
                    case NodeType.Mine:
                        gameObject = goldminePrefab;
                        break;
                    case NodeType.TownCenter:
                        gameObject = townCenterPrefab;
                        break;
                    case NodeType.TreeCutDown:
                        gameObject = treeCutDownPrefab;
                        break;
                    case NodeType.Dirt:
                        gameObject = dirtPrefab;
                        break;
                }

                visuals.Add(Instantiate(gameObject, new Vector3(node.GetCoordinate().x, node.GetCoordinate().y, 0), Quaternion.identity));
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            foreach (var sector in voronoi.SectorsToDraw())
            {
                Handles.color = color;
                List<Vector3> list = new List<Vector3>();
                foreach (var nodeVoronoi in sector.PointsToDraw())
                    list.Add(new Vector3(nodeVoronoi.GetX(), nodeVoronoi.GetY()));
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
                    NodeType.TreeCutDown => Color.green,
                    NodeType.Dirt => Color.gray,
                    NodeType.Forest => Color.red,
                    _ => Color.white
                };
   
                Gizmos.DrawSphere(new Vector3(node.GetCoordinate().x, node.GetCoordinate().y), nodesSize / 5);
            }
        }
    }
}