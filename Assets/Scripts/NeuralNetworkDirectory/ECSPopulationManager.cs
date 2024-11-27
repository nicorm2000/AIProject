using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeuralNetworkLib.Agents.Flocking;
using NeuralNetworkLib.Agents.SimAgents;
using NeuralNetworkLib.DataManagement;
using NeuralNetworkLib.NeuralNetDirectory;
using NeuralNetworkLib.NeuralNetDirectory.ECS;
using NeuralNetworkLib.NeuralNetDirectory.ECS.Patron;
using NeuralNetworkLib.NeuralNetDirectory.NeuralNet;
using NeuralNetworkLib.Utils;
using Pathfinder.Graph;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NeuralNetworkDirectory
{
    using SimAgentType = SimAgent<IVector, ITransform<IVector>>;
    using SimBoid = Boid<IVector, ITransform<IVector>>;

    public class EcsPopulationManager : MonoBehaviour
    {
        #region Variables

        [Header("Population Setup")]
        [SerializeField] private Mesh carnivoreMesh;
        [SerializeField] private Material carnivoreMat;
        [SerializeField] private Mesh herbivoreMesh;
        [SerializeField] private Material herbivoreMat;
        [SerializeField] private Mesh scavengerMesh;
        [SerializeField] private Material scavengerMat;

        [Header("Population Settings")]
        [SerializeField] private int carnivoreCount = 10;
        [SerializeField] private int herbivoreCount = 20;
        [SerializeField] private int scavengerCount = 10;
        [SerializeField] private float mutationRate = 0.01f;
        [SerializeField] private float mutationChance = 0.10f;
        [SerializeField] private int eliteCount = 4;
        
        [Header("Modifiable Settings")]
        [SerializeField] public int Generation;
        [SerializeField] private float Bias = 0.0f;
        [SerializeField] private int generationsPerSave = 25;
        [SerializeField] private float generationDuration = 20.0f;
        [SerializeField] private int generationToLoad = 0;

        public int gridWidth = 10;
        public int gridHeight = 10;
        public float speed = 1.0f;
        public static Sim2Graph graph;

        private bool isRunning = true;
        private int missingCarnivores;
        private int missingHerbivores;
        private int missingScavengers;
        private int plantCount;
        private int behaviourCount;
        private const int CellSize = 1;
        private const float SigmoidP = .5f;
        private float accumTime;
        private const string DirectoryPath = "NeuronData";
        private GeneticAlgorithm genAlg;
        private GraphManager<IVector, ITransform<IVector>> gridManager;
        private FitnessManager<IVector, ITransform<IVector>> fitnessManager;
        private static Dictionary<uint, Dictionary<BrainType, List<Genome>>> _population = new();
        private static readonly int BrainsAmount = Enum.GetValues(typeof(BrainType)).Length;

        /// <summary>
        /// Defines parallel options for managing the degree of parallelism in the graph's operations.
        /// </summary>
        private ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = 32
        };

        #endregion

        /// <summary>
        /// Initializes the simulation by setting up various data containers, input counts, brain types, and other components necessary for the simulation. 
        /// Also subscribes to the <see cref="Herbivore{IVector, ITransform{IVector}}.OnDeath"/> event and initializes the graph and other managers.
        /// </summary>
        private void Awake()
        {
            Herbivore<IVector, ITransform<IVector>>.OnDeath += RemoveEntity;
            DataContainer.herbBrainTypes = new Dictionary<int, BrainType>();
            DataContainer.scavBrainTypes = new Dictionary<int, BrainType>();
            DataContainer.carnBrainTypes = new Dictionary<int, BrainType>();
            DataContainer.herbBrainTypes[0] = BrainType.Eat;
            DataContainer.herbBrainTypes[1] = BrainType.Movement;
            DataContainer.herbBrainTypes[2] = BrainType.Escape;

            DataContainer.scavBrainTypes[0] = BrainType.Eat;
            DataContainer.scavBrainTypes[1] = BrainType.ScavengerMovement;
            DataContainer.scavBrainTypes[2] = BrainType.Flocking;

            DataContainer.carnBrainTypes[0] = BrainType.Eat;
            DataContainer.carnBrainTypes[1] = BrainType.Movement;
            DataContainer.carnBrainTypes[2] = BrainType.Attack;

            DataContainer.inputCounts = new[]
            {
                new NeuronInputCount
                {
                    agentType = SimAgentTypes.Carnivore, brainType = BrainType.Eat, inputCount = 4, outputCount = 1,
                    hiddenLayersInputs = new[] { 1 }
                },
                new NeuronInputCount
                {
                    agentType = SimAgentTypes.Carnivore, brainType = BrainType.Movement, inputCount = 7,
                    outputCount = 3, hiddenLayersInputs = new[] { 3 }
                },
                new NeuronInputCount
                {
                    agentType = SimAgentTypes.Carnivore, brainType = BrainType.Attack, inputCount = 4,
                    outputCount = 1, hiddenLayersInputs = new[] { 1 }
                },
                new NeuronInputCount
                {
                    agentType = SimAgentTypes.Herbivore, brainType = BrainType.Eat, inputCount = 4, outputCount = 1,
                    hiddenLayersInputs = new[] { 1 }
                },
                new NeuronInputCount
                {
                    agentType = SimAgentTypes.Herbivore, brainType = BrainType.Movement, inputCount = 8,
                    outputCount = 2, hiddenLayersInputs = new[] { 3 }
                },
                new NeuronInputCount
                {
                    agentType = SimAgentTypes.Herbivore, brainType = BrainType.Escape, inputCount = 4, outputCount = 1,
                    hiddenLayersInputs = new[] { 1 }
                },
                new NeuronInputCount
                {
                    agentType = SimAgentTypes.Scavenger, brainType = BrainType.Eat, inputCount = 4, outputCount = 1,
                    hiddenLayersInputs = new[] { 1 }
                },
                new NeuronInputCount
                {
                    agentType = SimAgentTypes.Scavenger, brainType = BrainType.ScavengerMovement, inputCount = 7,
                    outputCount = 2, hiddenLayersInputs = new[] { 3 }
                },
                new NeuronInputCount
                {
                    agentType = SimAgentTypes.Scavenger, brainType = BrainType.Flocking, inputCount = 16,
                    outputCount = 4,
                    hiddenLayersInputs = new[] { 12, 8, 6, 4 }
                },
            };

            DataContainer.InputCountCache =
                DataContainer.inputCounts.ToDictionary(input => (input.brainType, input.agentType));
            DataContainer.inputCounts = DataContainer.inputCounts;
            ECSManager.Init();
            gridManager = new GraphManager<IVector, ITransform<IVector>>(gridWidth, gridHeight);
            graph = new Sim2Graph(gridWidth, gridHeight, CellSize);
            DataContainer.graph = graph;
            StartSimulation();
            plantCount = DataContainer.Agents.Values.Count(agent => agent.agentType == SimAgentTypes.Herbivore) * 2;
            InitializePlants();
            fitnessManager = new FitnessManager<IVector, ITransform<IVector>>(DataContainer.Agents);
            behaviourCount = GetHighestBehaviourCount();
        }

        /// <summary>
        /// Updates the simulation by calculating the positions of the agents (carnivores, herbivores, and scavengers) in parallel, 
        /// and then drawing their mesh instances on the screen.
        /// </summary>
        private void Update()
        {
            Matrix4x4[] carnivoreMatrices = new Matrix4x4[carnivoreCount];
            Matrix4x4[] herbivoreMatrices = new Matrix4x4[herbivoreCount];
            Matrix4x4[] scavengerMatrices = new Matrix4x4[scavengerCount];

            int carnivoreIndex = 0;
            int herbivoreIndex = 0;
            int scavengerIndex = 0;

            Parallel.ForEach(DataContainer.Agents.Keys, id =>
            {
                IVector pos = DataContainer.Agents[id].Transform.position;
                Vector3 position = new Vector3(pos.X, pos.Y);
                Matrix4x4 matrix = Matrix4x4.Translate(position);

                switch (DataContainer.Agents[id].agentType)
                {
                    case SimAgentTypes.Carnivore:
                        int carnIndex = Interlocked.Increment(ref carnivoreIndex) - 1;
                        if (carnIndex < carnivoreMatrices.Length)
                        {
                            carnivoreMatrices[carnIndex] = matrix;
                        }

                        break;
                    case SimAgentTypes.Herbivore:
                        int herbIndex = Interlocked.Increment(ref herbivoreIndex) - 1;
                        if (herbIndex < herbivoreMatrices.Length)
                        {
                            herbivoreMatrices[herbIndex] = matrix;
                        }

                        break;
                    case SimAgentTypes.Scavenger:
                        int scavIndex = Interlocked.Increment(ref scavengerIndex) - 1;
                        if (scavIndex < scavengerMatrices.Length)
                        {
                            scavengerMatrices[scavIndex] = matrix;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            if (carnivoreMatrices.Length > 0)
            {
                Graphics.DrawMeshInstanced(carnivoreMesh, 0, carnivoreMat, carnivoreMatrices);
            }

            if (herbivoreMatrices.Length > 0)
            {
                Graphics.DrawMeshInstanced(herbivoreMesh, 0, herbivoreMat, herbivoreMatrices);
            }

            if (scavengerMatrices.Length > 0)
            {
                Graphics.DrawMeshInstanced(scavengerMesh, 0, scavengerMat, scavengerMatrices);
            }
        }

        /// <summary>
        /// Handles the fixed time step update for the simulation. It controls agent movement, performs actions for each entity turn, 
        /// and triggers a new epoch when the specified generation duration is reached.
        /// </summary>
        private void FixedUpdate()
        {
            if (!isRunning)
                return;

            float dt = Time.fixedDeltaTime;

            float clampSpeed = Mathf.Clamp(speed, 1, 1500);
            for (int i = 0; i < clampSpeed; i++)
            {
                EntitiesTurn(dt);
                accumTime += dt;
                if (!(accumTime >= generationDuration)) return;
                accumTime -= generationDuration;
                Epoch();
            }
        }

        /// <summary>
        /// Updates the state of all agents in the simulation during their turn. 
        /// This method processes agent inputs, updates their state in parallel, 
        /// and manages the behaviors of different agent types (e.g., scavenger) 
        /// by adjusting their boid offsets and updating their finite state machine (FSM).
        /// </summary>
        /// <param name="dt">The delta time since the last update, used for time-based calculations.</param>
        private void EntitiesTurn(float dt)
        {
            KeyValuePair<uint, SimAgentType>[] agentsCopy = DataContainer.Agents.ToArray();

            Parallel.ForEach(agentsCopy, parallelOptions, entity =>
            {
                entity.Value.UpdateInputs();
                InputComponent inputComponent = ECSManager.GetComponent<InputComponent>(entity.Key);
                if (inputComponent != null && DataContainer.Agents.TryGetValue(entity.Key, out SimAgentType agent))
                {
                    inputComponent.inputs = agent.input;
                }
            });

            ECSManager.Tick(dt);

            Parallel.ForEach(agentsCopy, parallelOptions, entity =>
            {
                OutputComponent outputComponent = ECSManager.GetComponent<OutputComponent>(entity.Key);
                if (outputComponent == null ||
                    !DataContainer.Agents.TryGetValue(entity.Key, out SimAgentType agent)) return;

                agent.output = outputComponent.Outputs;

                if (agent.agentType != SimAgentTypes.Scavenger) return;

                SimBoid boid = DataContainer.Scavengers[entity.Key]?.boid;

                if (boid != null)
                {
                    UpdateBoidOffsets(boid, outputComponent.Outputs
                        [GetBrainTypeKeyByValue(BrainType.Flocking, SimAgentTypes.Scavenger)]);
                }
            });

            int batchSize = 10;
            for (int i = 0; i < behaviourCount; i++)
            {
                int i1 = i;
                List<Task> tasks = new List<Task>();

                for (int j = 0; j < agentsCopy.Length; j += batchSize)
                {
                    KeyValuePair<uint, SimAgentType>[] batch = agentsCopy.Skip(j).Take(batchSize).ToArray();
                    tasks.Add(Task.Run(() =>
                    {
                        foreach (KeyValuePair<uint, SimAgentType> entity in batch)
                        {
                            entity.Value.Fsm.MultiThreadTick(i1);
                        }
                    }));
                }

                foreach (KeyValuePair<uint, SimAgentType> entity in agentsCopy)
                {
                    entity.Value.Fsm.MainThreadTick(i);
                }

                
                Task.WaitAll(tasks.ToArray());

                foreach (Task task in tasks)
                {
                    task.Dispose();
                }

                tasks.Clear();
            }

            fitnessManager.Tick();
        }

        /// <summary>
        /// Handles the simulation epoch by updating the population of agents, 
        /// managing the lifecycle of agents (e.g., loading new agents if required), 
        /// and generating new genomes for the next generation. 
        /// The epoch includes population cleanup, initialization of new plants, 
        /// and garbage collection after every 100 generations.
        /// </summary>
        private void Epoch()
        {
            Generation++;

            PurgingSpecials();

            missingCarnivores =
                carnivoreCount - DataContainer.Agents.Count(agent => agent.Value.agentType == SimAgentTypes.Carnivore);
            missingHerbivores =
                herbivoreCount - DataContainer.Agents.Count(agent => agent.Value.agentType == SimAgentTypes.Herbivore);
            missingScavengers =
                scavengerCount - DataContainer.Agents.Count(agent => agent.Value.agentType == SimAgentTypes.Scavenger);
            bool remainingPopulation = DataContainer.Agents.Count > 0;

            bool remainingCarn = carnivoreCount - missingCarnivores > 1;
            bool remainingHerb = herbivoreCount - missingHerbivores > 1;
            bool remainingScav = scavengerCount - missingScavengers > 1;

            ECSManager.GetSystem<NeuralNetSystem>().Deinitialize();
            if (Generation % generationsPerSave == 0)
            {
                Save(DirectoryPath, Generation);
            }

            if (remainingPopulation)
            {
                foreach (SimAgentType agent in DataContainer.Agents.Values)
                {
                    Debug.Log(agent.agentType + " survived.");
                }
            }

            CleanMap();
            InitializePlants();

            if (missingCarnivores == carnivoreCount)
            {
                Load(SimAgentTypes.Carnivore);
            }

            if (missingHerbivores == herbivoreCount)
            {
                Load(SimAgentTypes.Herbivore);
            }

            if (missingScavengers == scavengerCount)
            {
                Load(SimAgentTypes.Scavenger);
            }

            if (!remainingPopulation)
            {
                FillPopulation();
                _population.Clear();

                return;
            }

            Dictionary<SimAgentTypes, Dictionary<BrainType, List<Genome>>> genomes = new()
            {
                [SimAgentTypes.Scavenger] = new Dictionary<BrainType, List<Genome>>(),
                [SimAgentTypes.Herbivore] = new Dictionary<BrainType, List<Genome>>(),
                [SimAgentTypes.Carnivore] = new Dictionary<BrainType, List<Genome>>()
            };
            Dictionary<SimAgentTypes, Dictionary<BrainType, int>> indexes = new()
            {
                [SimAgentTypes.Scavenger] = new Dictionary<BrainType, int>(),
                [SimAgentTypes.Herbivore] = new Dictionary<BrainType, int>(),
                [SimAgentTypes.Carnivore] = new Dictionary<BrainType, int>()
            };


            foreach (SimAgentType agent in DataContainer.Agents.Values)
            {
                agent.Reset();
            }

            if (remainingCarn)
            {
                CreateNewGenomes(genomes, DataContainer.carnBrainTypes, SimAgentTypes.Carnivore, carnivoreCount);
            }

            if (remainingScav)
            {
                CreateNewGenomes(genomes, DataContainer.scavBrainTypes, SimAgentTypes.Scavenger, scavengerCount);
            }

            if (remainingHerb)
            {
                CreateNewGenomes(genomes, DataContainer.herbBrainTypes, SimAgentTypes.Herbivore, herbivoreCount);
            }

            FillPopulation();
            BrainsHandler(indexes, genomes, remainingCarn, remainingScav, remainingHerb);

            genomes.Clear();
            indexes.Clear();

            if (Generation % 100 != 0) return;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Updates the boid (flocking behavior) offsets for a scavenger agent, 
        /// including its cohesion, separation, direction, and alignment behavior offsets.
        /// This is based on the output data provided by the agent's neural network.
        /// </summary>
        /// <param name="boid">The boid object representing the scavenger agent's flocking behavior.</param>
        /// <param name="outputs">The output values from the agent's neural network, used to update the boid's behavior offsets.</param>
        private void UpdateBoidOffsets(SimBoid boid, float[] outputs)
        {
            boid.cohesionOffset = outputs[0];
            boid.separationOffset = outputs[1];
            boid.directionOffset = outputs[2];
            boid.alignmentOffset = outputs[3];
        }

        /// <summary>
        /// Initializes the population by destroying existing agents and creating new ones 
        /// for each agent type (herbivores, carnivores, and scavengers) based on 
        /// predefined counts for each type.
        /// </summary>
        private void GenerateInitialPopulation()
        {
            DestroyAgents();

            CreateAgents(herbivoreCount, SimAgentTypes.Herbivore);
            CreateAgents(carnivoreCount, SimAgentTypes.Carnivore);
            CreateAgents(scavengerCount, SimAgentTypes.Scavenger);

            accumTime = 0.0f;
        }

        /// <summary>
        /// Creates a specified number of agents of a given type (Carnivore, Herbivore, or Scavenger).
        /// Initializes the agents by setting up neural networks, components, and random positions. 
        /// This process includes adding brain components, assigning neural network inputs and outputs, 
        /// and generating genomes for the agents based on their type.
        /// </summary>
        /// <param name="count">The number of agents to create.</param>
        /// <param name="agentType">The type of agent to create (Carnivore, Herbivore, or Scavenger).</param>
        private void CreateAgents(int count, SimAgentTypes agentType)
        {
            Parallel.For(0, count, i =>
            {
                uint entityID = ECSManager.CreateEntity();
                NeuralNetComponent neuralNetComponent = new NeuralNetComponent();
                InputComponent inputComponent = new InputComponent();
                ECSManager.AddComponent(entityID, inputComponent);
                ECSManager.AddComponent(entityID, neuralNetComponent);

                Dictionary<int, BrainType> num = agentType switch
                {
                    SimAgentTypes.Carnivore => DataContainer.carnBrainTypes,
                    SimAgentTypes.Herbivore => DataContainer.herbBrainTypes,
                    SimAgentTypes.Scavenger => DataContainer.scavBrainTypes,
                    _ => throw new ArgumentException("Invalid agent type")
                };

                OutputComponent outputComponent = new OutputComponent();

                ECSManager.AddComponent(entityID, outputComponent);
                outputComponent.Outputs = new float[3][];

                foreach (BrainType brain in num.Values)
                {
                    NeuronInputCount inputsCount = DataContainer.InputCountCache[(brain, agentType)];
                    outputComponent.Outputs[GetBrainTypeKeyByValue(brain, agentType)] =
                        new float[inputsCount.outputCount];
                }

                List<NeuralNetComponent> brains = CreateBrain(agentType);
                Dictionary<BrainType, List<Genome>> genomes = new Dictionary<BrainType, List<Genome>>();

                foreach (NeuralNetComponent brain in brains)
                {
                    BrainType brainType = BrainType.Movement;
                    Genome genome =
                        new Genome(brain.Layers.Sum(layerList =>
                            layerList.Sum(layer => GetWeights(layer).Length)));
                    int j = 0;
                    foreach (List<NeuronLayer> layerList in brain.Layers)
                    {
                        brainType = layerList[j++].BrainType;
                        SetWeights(layerList, genome.genome);
                        layerList.ForEach(neuron => neuron.AgentType = agentType);
                    }

                    if (!genomes.ContainsKey(brainType))
                    {
                        genomes[brainType] = new List<Genome>();
                    }

                    genomes[brainType].Add(genome);
                }

                inputComponent.inputs = new float[brains.Count][];
                neuralNetComponent.Layers = brains.SelectMany(brain => brain.Layers).ToList();
                neuralNetComponent.Fitness = new float[BrainsAmount];
                neuralNetComponent.FitnessMod = new float[BrainsAmount];

                for (int j = 0; j < neuralNetComponent.FitnessMod.Length; j++)
                {
                    neuralNetComponent.FitnessMod[j] = 1.0f;
                }

                SimAgentType agent = CreateAgent(agentType);
                lock (DataContainer.Agents)
                {
                    DataContainer.Agents[entityID] = agent;
                }

                if (agentType == SimAgentTypes.Scavenger)
                {
                    lock (DataContainer.Scavengers)
                    {
                        DataContainer.Scavengers[entityID] = (Scavenger<IVector, ITransform<IVector>>)agent;
                    }
                }

                foreach (BrainType brain in agent.brainTypes.Values)
                {
                    lock (_population)
                    {
                        if (!_population.ContainsKey(entityID))
                        {
                            _population[entityID] = new Dictionary<BrainType, List<Genome>>();
                        }

                        _population[entityID][brain] = genomes[brain];
                    }
                }
            });
        }

        /// <summary>
        /// Creates an agent of the specified type and initializes its position and brain components. 
        /// The agent's brain type is assigned based on its type (Carnivore, Herbivore, or Scavenger), 
        /// and it is positioned randomly in the simulation grid. 
        /// This method also initializes the agent's specific behaviors, including flocking for scavengers.
        /// </summary>
        /// <param name="agentType">The type of agent to create (Carnivore, Herbivore, or Scavenger).</param>
        /// <returns>A fully initialized agent of the specified type.</returns>
        private SimAgentType CreateAgent(SimAgentTypes agentType)
        {
            INode<IVector> randomNode = agentType switch
            {
                SimAgentTypes.Carnivore => gridManager.GetRandomPositionInUpperQuarter(),
                SimAgentTypes.Herbivore => gridManager.GetRandomPositionInLowerQuarter(),
                SimAgentTypes.Scavenger => gridManager.GetRandomPosition(),
                _ => throw new ArgumentOutOfRangeException(nameof(agentType), agentType, null)
            };

            SimAgentType agent;

            switch (agentType)
            {
                case SimAgentTypes.Carnivore:
                    agent = new Carnivore<IVector, ITransform<IVector>>();
                    agent.brainTypes = DataContainer.carnBrainTypes;
                    agent.agentType = SimAgentTypes.Carnivore;
                    break;
                case SimAgentTypes.Herbivore:
                    agent = new Herbivore<IVector, ITransform<IVector>>();
                    agent.brainTypes = DataContainer.herbBrainTypes;
                    agent.agentType = SimAgentTypes.Herbivore;
                    break;
                case SimAgentTypes.Scavenger:
                    agent = new Scavenger<IVector, ITransform<IVector>>();
                    agent.brainTypes = DataContainer.scavBrainTypes;
                    agent.agentType = SimAgentTypes.Scavenger;
                    break;
                default:
                    throw new ArgumentException("Invalid agent type");
            }

            agent.SetPosition(randomNode.GetCoordinate());
            agent.Init();

            if (agentType == SimAgentTypes.Scavenger)
            {
                Scavenger<IVector, ITransform<IVector>> sca = (Scavenger<IVector, ITransform<IVector>>)agent;
                sca.boid.Init(DataContainer.flockingManager.Alignment, DataContainer.flockingManager.Cohesion,
                    DataContainer.flockingManager.Separation,
                    DataContainer.flockingManager.Direction);
            }

            return agent;
        }

        /// <summary>
        /// Creates the brain components for an agent based on its type. 
        /// Each agent type (Carnivore, Herbivore, or Scavenger) has a specific set of brain types and layers, 
        /// which are created and added to the agent's neural network.
        /// </summary>
        /// <param name="agentType">The type of agent to create brains for (Carnivore, Herbivore, or Scavenger).</param>
        /// <returns>A list of neural network components representing the agent's brain.</returns>
        private List<NeuralNetComponent> CreateBrain(SimAgentTypes agentType)
        {
            List<NeuralNetComponent> brains = new List<NeuralNetComponent>
                { CreateSingleBrain(BrainType.Eat, agentType) };


            switch (agentType)
            {
                case SimAgentTypes.Herbivore:
                    brains.Add(CreateSingleBrain(BrainType.Movement, SimAgentTypes.Herbivore));
                    brains.Add(CreateSingleBrain(BrainType.Escape, SimAgentTypes.Herbivore));
                    break;
                case SimAgentTypes.Carnivore:
                    brains.Add(CreateSingleBrain(BrainType.Movement, SimAgentTypes.Carnivore));
                    brains.Add(CreateSingleBrain(BrainType.Attack, SimAgentTypes.Carnivore));
                    break;
                case SimAgentTypes.Scavenger:
                    brains.Add(CreateSingleBrain(BrainType.ScavengerMovement, SimAgentTypes.Scavenger));
                    brains.Add(CreateSingleBrain(BrainType.Flocking, SimAgentTypes.Scavenger));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(agentType), agentType,
                        "Not prepared for this agent type");
            }

            return brains;
        }

        /// <summary>
        /// Creates a single brain component for the agent with a specified brain type and agent type. 
        /// This brain consists of a single layer with neurons and their corresponding properties, 
        /// such as activation function and bias. 
        /// </summary>
        /// <param name="brainType">The type of brain to create (e.g., Eat, Movement, Attack, etc.).</param>
        /// <param name="agentType">The type of agent this brain is associated with (Carnivore, Herbivore, or Scavenger).</param>
        /// <returns>A neural network component for the specified brain type and agent type.</returns>
        private NeuralNetComponent CreateSingleBrain(BrainType brainType, SimAgentTypes agentType)
        {
            NeuralNetComponent neuralNetComponent = new NeuralNetComponent();
            neuralNetComponent.Layers.Add(CreateNeuronLayerList(brainType, agentType));
            return neuralNetComponent;
        }

        /// <summary>
        /// Creates a list of neuron layers for the specified brain type and agent type. 
        /// This method sets up the structure of the neural network by adding input, hidden, and output layers, 
        /// with the number of neurons determined by the agent's input count and brain type.
        /// </summary>
        /// <param name="brainType">The type of brain (e.g., Eat, Movement, etc.).</param>
        /// <param name="agentType">The type of agent (Carnivore, Herbivore, or Scavenger).</param>
        /// <returns>A list of neuron layers representing the neural network for the specified brain type and agent type.</returns>
        private List<NeuronLayer> CreateNeuronLayerList(BrainType brainType, SimAgentTypes agentType)
        {
            if (!DataContainer.InputCountCache.TryGetValue((brainType, agentType), out NeuronInputCount inputCount))
            {
                throw new ArgumentException("Invalid brainType or agentType");
            }

            List<NeuronLayer> layers = new List<NeuronLayer>
            {
                new(inputCount.inputCount, inputCount.inputCount, Bias, SigmoidP)
                    { BrainType = brainType, AgentType = agentType }
            };

            foreach (int hiddenLayerInput in inputCount.hiddenLayersInputs)
            {
                layers.Add(new NeuronLayer(layers[^1].OutputsCount, hiddenLayerInput, Bias, SigmoidP)
                    { BrainType = brainType, AgentType = agentType });
            }

            layers.Add(new NeuronLayer(layers[^1].OutputsCount, inputCount.outputCount, Bias, SigmoidP)
                { BrainType = brainType, AgentType = agentType });

            return layers;
        }

        /// <summary>
        /// Clears the population of agents, effectively destroying all existing agents 
        /// in the current simulation state. This method is used to reset or clean up 
        /// the population data during specific simulation epochs or events.
        /// </summary>
        private void DestroyAgents()
        {
            _population.Clear();
        }

        /// <summary>
        /// Handles the processing of brains for different agent types (Carnivores, Herbivores, Scavengers).
        /// Updates the neural network components of agents by selecting a random genome and setting their weights.
        /// Resets the agent's position and state after updating their neural network.
        /// </summary>
        /// <param name="indexes">A dictionary containing the index information for each agent type and brain type.</param>
        /// <param name="genomes">A dictionary containing the genomes for each agent type and brain type.</param>
        /// <param name="remainingCarn">A boolean indicating if there are remaining carnivores to process.</param>
        /// <param name="remainingScav">A boolean indicating if there are remaining scavengers to process.</param>
        /// <param name="remainingHerb">A boolean indicating if there are remaining herbivores to process.</param>
        private void BrainsHandler(Dictionary<SimAgentTypes, Dictionary<BrainType, int>> indexes,
            Dictionary<SimAgentTypes, Dictionary<BrainType, List<Genome>>> genomes,
            bool remainingCarn, bool remainingScav, bool remainingHerb)
        {
            foreach (KeyValuePair<uint, SimAgentType> agent in DataContainer.Agents)
            {
                SimAgentTypes agentType = agent.Value.agentType;

                switch (agentType)
                {
                    case SimAgentTypes.Carnivore:
                        if (!remainingCarn) continue;
                        break;
                    case SimAgentTypes.Herbivore:
                        if (!remainingHerb) continue;
                        break;
                    case SimAgentTypes.Scavenger:
                        if (!remainingScav) continue;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                NeuralNetComponent neuralNetComponent = ECSManager.GetComponent<NeuralNetComponent>(agent.Key);

                foreach (BrainType brain in agent.Value.brainTypes.Values)
                {
                    agent.Value.GetBrainTypeKeyByValue(brain);
                    if (!indexes[agentType].ContainsKey(brain))
                    {
                        indexes[agentType][brain] = 0;
                    }

                    int index = Random.Range(0, genomes[agentType][brain].Count);
                    if (!_population.ContainsKey(agent.Key))
                    {
                        _population[agent.Key] = new Dictionary<BrainType, List<Genome>>();
                    }

                    if (!_population[agent.Key].ContainsKey(brain))
                    {
                        _population[agent.Key][brain] = new List<Genome>();
                    }

                    if (index >= genomes[agentType][brain].Count) continue;


                    SetWeights(neuralNetComponent.Layers[GetBrainTypeKeyByValue(brain, agent.Value.agentType)],
                        genomes[agentType][brain][index].genome);


                    _population[agent.Key][brain].Add(genomes[agentType][brain][index]);
                    genomes[agentType][brain].Remove(genomes[agentType][brain][index]);

                    agent.Value.Transform = new ITransform<IVector>(new MyVector(
                        gridManager.GetRandomPosition().GetCoordinate().X,
                        gridManager.GetRandomPosition().GetCoordinate().Y));
                    agent.Value.Reset();
                }
            }
        }

        /// <summary>
        /// Fills the population of agents by creating missing Herbivores, Carnivores, and Scavengers.
        /// Calls the CreateAgents method for each agent type to ensure the population is complete.
        /// </summary>
        private void FillPopulation()
        {
            CreateAgents(missingHerbivores, SimAgentTypes.Herbivore);
            CreateAgents(missingCarnivores, SimAgentTypes.Carnivore);
            CreateAgents(missingScavengers, SimAgentTypes.Scavenger);
        }

        /// <summary>
        /// Creates new genomes for each brain type and agent type using a genetic algorithm.
        /// The new genomes are added to the genomes dictionary for the specified agent type and brain type.
        /// </summary>
        /// <param name="genomes">A dictionary containing the genomes for each agent type and brain type.</param>
        /// <param name="brainTypes">A dictionary containing the brain types for each agent type.</param>
        /// <param name="agentType">The type of agent (Carnivore, Herbivore, or Scavenger).</param>
        /// <param name="count">The number of new genomes to create for each brain type and agent type.</param>
        private void CreateNewGenomes(Dictionary<SimAgentTypes, Dictionary<BrainType, List<Genome>>> genomes,
            Dictionary<int, BrainType> brainTypes, SimAgentTypes agentType, int count)
        {
            foreach (BrainType brain in brainTypes.Values)
            {
                genomes[agentType][brain] =
                    genAlg.Epoch(GetGenomesByBrainAndAgentType(agentType, brain).ToArray(), count);
            }
        }

        /// <summary>
        /// Retrieves a list of genomes for a specified agent type and brain type.
        /// The genomes are created by collecting the weights of neural network layers for each agent of the specified type and brain.
        /// </summary>
        /// <param name="agentType">The type of agent (Carnivore, Herbivore, or Scavenger).</param>
        /// <param name="brainType">The brain type for which to retrieve the genomes.</param>
        /// <returns>A list of genomes containing the weights of the neural network layers for the specified agent type and brain type.</returns>
        private List<Genome> GetGenomesByBrainAndAgentType(SimAgentTypes agentType, BrainType brainType)
        {
            List<Genome> genomes = new List<Genome>();

            foreach (KeyValuePair<uint, SimAgentType> agentEntry in DataContainer.Agents)
            {
                uint agentId = agentEntry.Key;
                SimAgentType agent = agentEntry.Value;

                if (agent.agentType != agentType)
                {
                    continue;
                }

                NeuralNetComponent neuralNetComponent = ECSManager.GetComponent<NeuralNetComponent>(agentId);

                List<float> weights = new List<float>();
                foreach (List<NeuronLayer> layerList in neuralNetComponent.Layers)
                {
                    foreach (NeuronLayer layer in layerList)
                    {
                        if (layer.BrainType != brainType) continue;


                        weights.AddRange(GetWeights(layer));
                    }
                }

                Genome genome = new Genome(weights.ToArray());
                genomes.Add(genome);
            }

            return genomes;
        }

        /// <summary>
        /// Initializes plant positions in the grid, setting them as bushes with a food value of 5.
        /// </summary>
        private void InitializePlants()
        {
            for (int i = 0; i < plantCount; i++)
            {
                INode<IVector> plantPosition = gridManager.GetRandomPosition();
                plantPosition.NodeType = SimNodeType.Bush;
                plantPosition.Food = 5;
            }
        }

        /// <summary>
        /// Cleans the map by resetting all nodes to empty and setting their food value to 0.
        /// </summary>
        private void CleanMap()
        {
            foreach (SimNode<IVector> node in DataContainer.graph.NodesType)
            {
                node.Food = 0;
                node.NodeType = SimNodeType.Empty;
            }
        }

        /// <summary>
        /// Saves the current agent neural network data to a specified directory for a given generation.
        /// </summary>
        /// <param name="directoryPath">The directory where the data will be saved.</param>
        /// <param name="generation">The current generation number for the data.</param>
        private void Save(string directoryPath, int generation)
        {
            List<AgentNeuronData> agentsData = new List<AgentNeuronData>();

            if (DataContainer.Agents.Count == 0) return;

            List<KeyValuePair<uint, SimAgentType>> entitiesCopy = DataContainer.Agents.ToList();

            agentsData.Capacity = entitiesCopy.Count * DataContainer.InputCountCache.Count;

            Parallel.ForEach(entitiesCopy, parallelOptions, entity =>
            {
                NeuralNetComponent netComponent = ECSManager.GetComponent<NeuralNetComponent>(entity.Key);
                foreach (List<NeuronLayer> neuronLayers in netComponent.Layers)
                {
                    List<float> weights = new List<float>();
                    AgentNeuronData neuronData = new AgentNeuronData();
                    foreach (NeuronLayer layer in neuronLayers)
                    {
                        neuronData.AgentType = layer.AgentType;
                        neuronData.BrainType = layer.BrainType;
                        weights.AddRange(GetWeights(layer));
                    }

                    neuronData.NeuronWeights = weights.ToArray();
                    lock (agentsData)
                    {
                        agentsData.Add(neuronData);
                    }
                }
            });

            NeuronDataSystem.SaveNeurons(agentsData, directoryPath, generation);
        }

        /// <summary>
        /// Saves the current agent neural network data to a specified directory for a given generation.
        /// </summary>
        /// <param name="directoryPath">The directory where the data will be saved.</param>
        /// <param name="generation">The current generation number for the data.</param>
        public void Load(SimAgentTypes agentType)
        {
            Dictionary<SimAgentTypes, Dictionary<BrainType, List<AgentNeuronData>>> loadedData =
                NeuronDataSystem.LoadLatestNeurons(DirectoryPath);

            if (loadedData.Count == 0 || !loadedData.ContainsKey(agentType)) return;
            System.Random random = new System.Random();

            foreach (var entity in DataContainer.Agents)
            {
                NeuralNetComponent netComponent = ECSManager.GetComponent<NeuralNetComponent>(entity.Key);
                if (netComponent == null || entity.Value.agentType != agentType) continue;

                if (!loadedData.TryGetValue(agentType, out Dictionary<BrainType, List<AgentNeuronData>> brainData))
                    return;

                foreach (KeyValuePair<int, BrainType> brainType in entity.Value.brainTypes)
                {
                    if (!brainData.TryGetValue(brainType.Value, out List<AgentNeuronData> neuronDataList)) continue;
                    if (neuronDataList.Count == 0) continue; // Ensure the list is not empty

                    int index = random.Next(0, neuronDataList.Count);
                    AgentNeuronData neuronData = neuronDataList[index];
                    foreach (List<NeuronLayer> neuronLayer in netComponent.Layers)
                    {
                        lock (neuronLayer)
                        {
                            SetWeights(neuronLayer, neuronData.NeuronWeights);
                            neuronLayer.ForEach(neuron => neuron.AgentType = neuronData.AgentType);
                            neuronLayer.ForEach(neuron => neuron.BrainType = neuronData.BrainType);
                        }
                    }

                    lock (loadedData)
                    {
                        loadedData[agentType][brainType.Value].Remove(neuronData);
                    }
                }
            }
        }

        /// <summary>
        /// Loads the neural network data for agents from a specified directory, optionally loading data for a specific generation.
        /// </summary>
        /// <param name="directoryPath">The path to the directory where the data is located.</param>
        public void Load(string directoryPath)
        {
            Dictionary<SimAgentTypes, Dictionary<BrainType, List<AgentNeuronData>>> loadedData = generationToLoad > 0 ?
                NeuronDataSystem.LoadSpecificNeurons(directoryPath, generationToLoad) :
                NeuronDataSystem.LoadLatestNeurons(directoryPath);

            if (loadedData.Count == 0) return;
            System.Random random = new System.Random();

            foreach (var entity in DataContainer.Agents)
            {
                NeuralNetComponent netComponent = ECSManager.GetComponent<NeuralNetComponent>(entity.Key);
                if (netComponent == null || !DataContainer.Agents.TryGetValue(entity.Key, out SimAgentType agent))
                {
                    return;
                }

                if (!loadedData.TryGetValue(agent.agentType,
                        out Dictionary<BrainType, List<AgentNeuronData>> brainData)) return;

                foreach (KeyValuePair<int, BrainType> brainType in agent.brainTypes)
                {
                    if (!brainData.TryGetValue(brainType.Value, out List<AgentNeuronData> neuronDataList)) return;
                    if (neuronDataList.Count == 0) continue; // Ensure the list is not empty

                    int index = random.Next(0, neuronDataList.Count);
                    AgentNeuronData neuronData = neuronDataList[index];
                    foreach (List<NeuronLayer> neuronLayer in netComponent.Layers)
                    {
                        lock (neuronLayer)
                        {
                            SetWeights(neuronLayer, neuronData.NeuronWeights);
                            neuronLayer.ForEach(neuron => neuron.AgentType = neuronData.AgentType);
                            neuronLayer.ForEach(neuron => neuron.BrainType = neuronData.BrainType);
                        }
                    }

                    lock (loadedData)
                    {
                        loadedData[agent.agentType][brainType.Value]
                            .Remove(loadedData[agent.agentType][brainType.Value][index]);
                    }
                }
            }
        }

        /// <summary>
        /// Starts the simulation, initializing agents, setting up the genetic algorithm, and loading initial data.
        /// </summary>
        private void StartSimulation()
        {
            DataContainer.Agents = new Dictionary<uint, SimAgentType>();
            _population = new Dictionary<uint, Dictionary<BrainType, List<Genome>>>();
            genAlg = new GeneticAlgorithm(eliteCount, mutationChance, mutationRate);
            GenerateInitialPopulation();
            Load(DirectoryPath);
            isRunning = true;
        }

        /// <summary>
        /// Stops the simulation by halting agent activity and resetting the generation count.
        /// </summary>
        public void StopSimulation()
        {
            isRunning = false;
            Generation = 0;
            DestroyAgents();
        }

        /// <summary>
        /// Pauses or resumes the simulation by toggling the running state.
        /// </summary>
        public void PauseSimulation()
        {
            isRunning = !isRunning;
        }

        /// <summary>
        /// Determines the highest count of behaviors across all agents, considering both multi-threaded and main-thread behaviors.
        /// </summary>
        /// <returns>The highest behavior count across all agents.</returns>
        private int GetHighestBehaviourCount()
        {
            int highestCount = 0;

            foreach (SimAgentType entity in DataContainer.Agents.Values)
            {
                int multiThreadCount = entity.Fsm.GetMultiThreadCount();
                int mainThreadCount = entity.Fsm.GetMainThreadCount();

                int maxCount = Math.Max(multiThreadCount, mainThreadCount);
                if (maxCount > highestCount)
                {
                    highestCount = maxCount;
                }
            }

            return highestCount;
        }

        /// <summary>
        /// Retrieves the key for a brain type by its associated value for a specific agent type.
        /// </summary>
        /// <param name="value">The brain type value to search for.</param>
        /// <param name="agentType">The agent type to search within.</param>
        /// <returns>The key corresponding to the specified brain type value.</returns>
        /// <exception cref="ArgumentException">Thrown if the agent type is invalid.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the brain type is not found.</exception>
        public static int GetBrainTypeKeyByValue(BrainType value, SimAgentTypes agentType)
        {
            Dictionary<int, BrainType> brainTypes = agentType switch
            {
                SimAgentTypes.Carnivore => DataContainer.carnBrainTypes,
                SimAgentTypes.Herbivore => DataContainer.herbBrainTypes,
                SimAgentTypes.Scavenger => DataContainer.scavBrainTypes,
                _ => throw new ArgumentException("Invalid agent type")
            };

            foreach (KeyValuePair<int, BrainType> kvp in brainTypes)
            {
                if (kvp.Value == value)
                {
                    return kvp.Key;
                }
            }

            throw new KeyNotFoundException(
                $"The value '{value}' is not present in the brainTypes dictionary for agent type '{agentType}'.");
        }

        /// <summary>
        /// Draws Gizmos for visualizing nodes in the simulation, with colors corresponding to node types.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            foreach (SimNode<IVector> node in DataContainer.graph.NodesType)
            {
                Gizmos.color = node.NodeType switch
                {
                    SimNodeType.Blocked => Color.black,
                    SimNodeType.Bush => Color.green,
                    SimNodeType.Corpse => Color.red,
                    SimNodeType.Carrion => Color.magenta,
                    SimNodeType.Empty => Color.white,
                    _ => Color.white
                };

                Gizmos.DrawSphere(new Vector3(node.GetCoordinate().X, node.GetCoordinate().Y), (float)CellSize / 5);
                Gizmos.DrawSphere(new Vector3(node.GetCoordinate().X, node.GetCoordinate().Y), (float)CellSize / 5);
            }
        }

        /// <summary>
        /// Purges agents based on specific conditions, such as health or the ability to reproduce, marking them for removal.
        /// </summary>
        private void PurgingSpecials()
        {
            List<uint> agentsToRemove = new List<uint>();

            foreach (KeyValuePair<uint, SimAgentType> agentEntry in DataContainer.Agents)
            {
                SimAgentType agent = agentEntry.Value;

                if (agent.agentType == SimAgentTypes.Herbivore)
                {
                    if (agent is Herbivore<IVector, ITransform<IVector>> { Hp: <= 0 })
                    {
                        agentsToRemove.Add(agentEntry.Key);
                    }
                }

                if (!agent.CanReproduce)
                {
                    agentsToRemove.Add(agentEntry.Key);
                }
            }

            foreach (uint agentId in agentsToRemove)
            {
                if (DataContainer.Agents.ContainsKey(agentId))
                {
                    RemoveEntity(DataContainer.Agents[agentId]);
                }
            }

            agentsToRemove.Clear();
        }

        /// <summary>
        /// Removes an agent from the simulation, cleaning up all associated data and references.
        /// </summary>
        /// <param name="simAgent">The agent to remove.</param>
        public static void RemoveEntity(SimAgentType simAgent)
        {
            simAgent.Uninit();
            uint agentId = DataContainer.Agents.FirstOrDefault(agent => agent.Value == simAgent).Key;
            DataContainer.Agents.Remove(agentId);
            _population.Remove(agentId);
            DataContainer.Scavengers.Remove(agentId);
            ECSManager.RemoveEntity(agentId);
        }

        /// <summary>
        /// Sets the weights for the neurons in a list of layers based on the provided weight array.
        /// </summary>
        /// <param name="layers">The list of neuron layers to update.</param>
        /// <param name="newWeights">The new weight values to assign to the neurons.</param>
        public static void SetWeights(List<NeuronLayer> layers, float[] newWeights)
        {
            if (newWeights == null || newWeights.Length == 0)
            {
                return;
            }

            int id = 0;
            foreach (var layer in layers)
            {
                for (int i = 0; i < layer.NeuronsCount; i++)
                {
                    float[] ws = layer.neurons[i].weights;
                    for (int j = 0; j < ws.Length; j++)
                    {
                        if (id >= newWeights.Length)
                        {
                            break;
                        }

                        ws[j] = newWeights[id++];
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the weights from the neurons of a given layer.
        /// </summary>
        /// <param name="layer">The neuron layer to extract weights from.</param>
        /// <returns>An array of the weights from the neurons in the layer.</returns>
        public static float[] GetWeights(NeuronLayer layer)
        {
            int totalWeights = (int)(layer.NeuronsCount * layer.InputsCount);
            float[] weights = new float[totalWeights];
            int id = 0;

            for (int i = 0; i < layer.NeuronsCount; i++)
            {
                float[] ws = layer.neurons[i].weights;

                for (int j = 0; j < ws.Length; j++)
                {
                    weights[id] = ws[j];
                    id++;
                }
            }

            return weights;
        }
    }
}