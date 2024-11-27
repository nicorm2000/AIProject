using System;
using NeuralNetworkDirectory;
using NeuralNetworkLib.Utils;
using Random = System.Random;

namespace Pathfinder.Graph
{
    public class GraphManager<TVector, TTransform>
        where TTransform : ITransform<TVector> 
        where TVector : IVector, IEquatable<TVector>
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        private Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphManager"/> class with the specified width and height.
        /// </summary>
        /// <param name="width">The width of the graph.</param>
        /// <param name="height">The height of the graph.</param>
        public GraphManager(int width, int height)
        {
            Width = width;
            Height = height;
            random = new Random();
        }

        /// <summary>
        /// Gets a random position within the lower quarter of the graph (y-coordinate between 0 and Height/4).
        /// </summary>
        /// <returns>A random node located within the lower quarter of the graph.</returns>
        public INode<IVector> GetRandomPositionInLowerQuarter()
        {
            int x = random.Next(0, Width);
            int y = random.Next(1, Height / 4);
            return EcsPopulationManager.graph.NodesType[x, y];
        }

        /// <summary>
        /// Gets a random position within the upper quarter of the graph (y-coordinate between 3*Height/4 and Height-1).
        /// </summary>
        /// <returns>A random node located within the upper quarter of the graph.</returns>
        public INode<IVector> GetRandomPositionInUpperQuarter()
        {
            int x = random.Next(0, Width);
            int y = random.Next(3 * Height / 4, Height-1);
            return EcsPopulationManager.graph.NodesType[x, y];
        }

        /// <summary>
        /// Gets a random position within the entire graph (y-coordinate between 0 and Height).
        /// </summary>
        /// <returns>A random node located anywhere in the graph.</returns>
        public INode<IVector> GetRandomPosition()
        {
            int x = random.Next(0, Width);
            int y = random.Next(0, Height);
            return EcsPopulationManager.graph.NodesType[x, y];
        }
    }
}