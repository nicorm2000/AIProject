using System.Collections.Generic;
using UnityEngine;
using MathDebbuger.Internals;

namespace MathDebbuger
{
    /// <summary>
    /// A static class for debugging vector operations in 3D space.
    /// </summary>
    public static class Vector3Debugger
    {
        private static Camera renderCamera;
        private static Dictionary<string, CameraInternals.CameraDebugger> debuggers;

        /// <summary>
        /// Initializes the debugger by finding the main camera in the scene.
        /// </summary>
        /// <returns>True if initialization was successful; otherwise, false.</returns>
        private static bool InitDebugger()
        {
            renderCamera = Object.FindObjectOfType<Camera>();
            if (renderCamera)
            {
                debuggers = new Dictionary<string, CameraInternals.CameraDebugger>();
                renderCamera.gameObject.AddComponent<CameraInternals.VectorHandles>();
                return true;
            }
            Debug.LogError("Init Failed: The Vector3Debugger needs a Camera in scene to work.");
            return false;
        }

        /// <summary>
        /// Checks if the debugger has been initialized.
        /// </summary>
        /// <returns>True if initialized; otherwise, false.</returns>
        private static bool CheckInited()
        {
            if (renderCamera)
                return true;
            return InitDebugger();
        }

        /// <summary>
        /// Checks if a key already exists in the debugger dictionary.
        /// </summary>
        /// <param name="key">The identifier key to check.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        private static bool KeyAlreadyExist(string key)
        {
            if (debuggers.ContainsKey(key))
            {
                Debug.LogError("Init Failed: The identifier \"" + key + "\" is already in use.");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a key exists in the debugger dictionary.
        /// </summary>
        /// <param name="key">The identifier key to check.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        private static bool ExistKey(string key)
        {
            if (!debuggers.ContainsKey(key))
            {
                Debug.LogError("Find Identifier Failed: The identifier \"" + key + "\" doesn't exist.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Adds a vector from the origin (zero) to the specified destination position with a unique identifier.
        /// </summary>
        /// <param name="destinationPosition">The destination position of the vector.</param>
        /// <param name="identifier">The unique identifier for the vector.</param>
        public static void AddVector(Vector3 destinationPosition, string identifier)
        {
            if (!CheckInited() || KeyAlreadyExist(identifier))
                return;

            CameraInternals.CameraDebugger cameraDebugger = renderCamera.gameObject.AddComponent<CameraInternals.CameraDebugger>();
            cameraDebugger.hideFlags = HideFlags.HideInInspector;
            List<Vector3> positions = new List<Vector3> { Vector3.zero, destinationPosition };
            cameraDebugger.Init(positions);
            debuggers.Add(identifier, cameraDebugger);
        }

        /// <summary>
        /// Adds a vector from the specified origin position to the destination position with a unique identifier.
        /// </summary>
        /// <param name="originPosition">The origin position of the vector.</param>
        /// <param name="destinationPosition">The destination position of the vector.</param>
        /// <param name="identifier">The unique identifier for the vector.</param>
        public static void AddVector(Vector3 originPosition, Vector3 destinationPosition, string identifier)
        {
            if (!CheckInited() || KeyAlreadyExist(identifier))
                return;

            CameraInternals.CameraDebugger cameraDebugger = renderCamera.gameObject.AddComponent<CameraInternals.CameraDebugger>();
            cameraDebugger.hideFlags = HideFlags.HideInInspector;
            List<Vector3> positions = new List<Vector3> { originPosition, destinationPosition };
            cameraDebugger.Init(positions);
            debuggers.Add(identifier, cameraDebugger);
        }

        /// <summary>
        /// Adds a vector from the origin (zero) to the specified destination position with a unique identifier and color.
        /// </summary>
        /// <param name="destinationPosition">The destination position of the vector.</param>
        /// <param name="vectorColor">The color of the vector.</param>
        /// <param name="identifier">The unique identifier for the vector.</param>
        public static void AddVector(Vector3 destinationPosition, Color vectorColor, string identifier)
        {
            if (!CheckInited() || KeyAlreadyExist(identifier))
                return;

            CameraInternals.CameraDebugger cameraDebugger = renderCamera.gameObject.AddComponent<CameraInternals.CameraDebugger>();
            cameraDebugger.hideFlags = HideFlags.HideInInspector;
            List<Vector3> positions = new List<Vector3> { Vector3.zero, destinationPosition };
            cameraDebugger.Init(positions, vectorColor);
            debuggers.Add(identifier, cameraDebugger);
        }

        /// <summary>
        /// Adds a vector from the specified origin position to the destination position with a unique identifier and color.
        /// </summary>
        /// <param name="originPosition">The origin position of the vector.</param>
        /// <param name="destinationPosition">The destination position of the vector.</param>
        /// <param name="vectorColor">The color of the vector.</param>
        /// <param name="identifier">The unique identifier for the vector.</param>
        public static void AddVector(Vector3 originPosition, Vector3 destinationPosition, Color vectorColor, string identifier)
        {
            if (!CheckInited() || KeyAlreadyExist(identifier))
                return;

            CameraInternals.CameraDebugger cameraDebugger = renderCamera.gameObject.AddComponent<CameraInternals.CameraDebugger>();
            cameraDebugger.hideFlags = HideFlags.HideInInspector;
            List<Vector3> positions = new List<Vector3> { originPosition, destinationPosition };
            cameraDebugger.Init(positions, vectorColor);
            debuggers.Add(identifier, cameraDebugger);
        }

        /// <summary>
        /// Adds a sequence of vectors with an option to use the first vertex as the origin (zero) with a unique identifier.
        /// </summary>
        /// <param name="positions">The list of positions representing the sequence of vectors.</param>
        /// <param name="useTheFirstVertexAsZero">If true, the first vertex will be considered as the origin.</param>
        /// <param name="identifier">The unique identifier for the vector sequence.</param>
        public static void AddVectorsSecuence(List<Vector3> positions, bool useTheFirstVertexAsZero, string identifier)
        {
            if (!CheckInited() || KeyAlreadyExist(identifier))
                return;

            CameraInternals.CameraDebugger cameraDebugger = renderCamera.gameObject.AddComponent<CameraInternals.CameraDebugger>();
            cameraDebugger.hideFlags = HideFlags.HideInInspector;

            // If the first vertex is not to be used as the origin, insert zero at the beginning
            if (!useTheFirstVertexAsZero)
                positions.Insert(0, Vector3.zero);

            cameraDebugger.Init(positions);
            debuggers.Add(identifier, cameraDebugger);
        }

        /// <summary>
        /// Adds a sequence of vectors with an option to use the first vertex as the origin (zero) and a color with a unique identifier.
        /// </summary>
        /// <param name="positions">The list of positions representing the sequence of vectors.</param>
        /// <param name="useTheFirstVertexAsZero">If true, the first vertex will be considered as the origin.</param>
        /// <param name="vectorColor">The color of the vector sequence.</param>
        /// <param name="identifier">The unique identifier for the vector sequence.</param>
        public static void AddVectorsSecuence(List<Vector3> positions, bool useTheFirstVertexAsZero, Color vectorColor, string identifier)
        {
            if (!CheckInited() || KeyAlreadyExist(identifier))
                return;

            CameraInternals.CameraDebugger cameraDebugger = renderCamera.gameObject.AddComponent<CameraInternals.CameraDebugger>();
            cameraDebugger.hideFlags = HideFlags.HideInInspector;

            // If the first vertex is not to be used as the origin, insert zero at the beginning
            if (!useTheFirstVertexAsZero)
                positions.Insert(0, Vector3.zero);

            cameraDebugger.Init(positions, vectorColor);
            debuggers.Add(identifier, cameraDebugger);
        }

        /// <summary>
        /// Updates the destination position of an existing vector identified by the unique identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier for the vector.</param>
        /// <param name="newDestinationPosition">The new destination position for the vector.</param>
        public static void UpdatePosition(string identifier, Vector3 newDestinationPosition)
        {
            if (!ExistKey(identifier))
                return;

            List<Vector3> newPositions = new List<Vector3> { Vector3.zero, newDestinationPosition };
            debuggers[identifier].UpdateVectors(newPositions);
        }

        /// <summary>
        /// Updates the origin and destination positions of an existing vector identified by the unique identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier for the vector.</param>
        /// <param name="newOriginPosition">The new origin position for the vector.</param>
        /// <param name="newDestinationPosition">The new destination position for the vector.</param>
        public static void UpdatePosition(string identifier, Vector3 newOriginPosition, Vector3 newDestinationPosition)
        {
            if (!ExistKey(identifier))
                return;

            List<Vector3> newPositions = new List<Vector3> { newOriginPosition, newDestinationPosition };
            debuggers[identifier].UpdateVectors(newPositions);
        }

        /// <summary>
        /// Updates the positions of a sequence of vectors identified by the unique identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier for the vector sequence.</param>
        /// <param name="newPositions">The new positions for the vector sequence.</param>
        public static void UpdatePositionsSecuence(string identifier, List<Vector3> newPositions)
        {
            if (ExistKey(identifier))
                debuggers[identifier].UpdateVectors(newPositions);
        }

        /// <summary>
        /// Updates the color of an existing vector identified by the unique identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier for the vector.</param>
        /// <param name="newColor">The new color for the vector.</param>
        public static void UpdateColor(string identifier, Color newColor)
        {
            if (ExistKey(identifier))
                debuggers[identifier].UpdateColor(newColor);
        }

        /// <summary>
        /// Enables the visualization of all vectors in the editor.
        /// </summary>
        public static void EnableEditorView()
        {
            foreach (KeyValuePair<string, CameraInternals.CameraDebugger> debugger in debuggers)
            {
                debugger.Value.EnableShowInEditor();
            }
        }

        /// <summary>
        /// Disables the visualization of all vectors in the editor.
        /// </summary>
        public static void DisableEditorView()
        {
            foreach (KeyValuePair<string, CameraInternals.CameraDebugger> debugger in debuggers)
            {
                debugger.Value.DisableShowInEditor();
            }
        }

        /// <summary>
        /// Sets the vector arrow style for all vectors.
        /// </summary>
        /// <param name="arrow">The vector arrow style to set.</param>
        public static void SetVectorArrow(VectorArrow arrow)
        {
            foreach (KeyValuePair<string, CameraInternals.CameraDebugger> debugger in debuggers)
            {
                debugger.Value.SetVectorArrow(arrow);
            }
        }

        /// <summary>
        /// Sets the font size for the vector labels.
        /// </summary>
        /// <param name="size">The font size to set.</param>
        public static void SetFontSize(int size)
        {
            foreach (KeyValuePair<string, CameraInternals.CameraDebugger> debugger in debuggers)
            {
                debugger.Value.SetFontSize(size);
            }
        }

        /// <summary>
        /// Enables the visualization of a specific vector identified by the unique identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier for the vector.</param>
        public static void EnableEditorView(string identifier)
        {
            if (ExistKey(identifier))
                debuggers[identifier].EnableShowInEditor();
        }

        /// <summary>
        /// Disables the visualization of a specific vector identified by the unique identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier for the vector.</param>
        public static void DisableEditorView(string identifier)
        {
            if (ExistKey(identifier))
                debuggers[identifier].DisableShowInEditor();
        }

        /// <summary>
        /// Sets the vector arrow style for a specific vector identified by the unique identifier.
        /// </summary>
        /// <param name="arrow">The vector arrow style to set.</param>
        /// <param name="identifier">The unique identifier for the vector.</param>
        public static void SetVectorArrow(VectorArrow arrow, string identifier)
        {
            if (ExistKey(identifier))
                debuggers[identifier].SetVectorArrow(arrow);
        }

        /// <summary>
        /// Sets the font size for the vector labels of a specific vector identified by the unique identifier.
        /// </summary>
        /// <param name="size">The font size to set.</param>
        /// <param name="identifier">The unique identifier for the vector.</param>
        public static void SetFontSize(int size, string identifier)
        {
            if (ExistKey(identifier))
                debuggers[identifier].SetFontSize(size);
        }

        /// <summary>
        /// Deletes a vector identified by the unique identifier from the debugger.
        /// </summary>
        /// <param name="identifier">The unique identifier for the vector.</param>
        public static void DeleteVector(string identifier)
        {
            if (ExistKey(identifier))
            {
                debuggers[identifier].Delete();
                debuggers.Remove(identifier);
            }
        }

        /// <summary>
        /// Disables the specific vector identified by the unique identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier for the vector.</param>
        public static void TurnOffVector(string identifier)
        {
            if (ExistKey(identifier))
                debuggers[identifier].enabled = false;
        }

        /// <summary>
        /// Enables the specific vector identified by the unique identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier for the vector.</param>
        public static void TurnOnVector(string identifier)
        {
            if (ExistKey(identifier))
                debuggers[identifier].enabled = true;
        }

        /// <summary>
        /// Clears all vectors from the debugger.
        /// </summary>
        public static void Clear()
        {
            if (debuggers == null || debuggers.Count == 0)
                return;

            foreach (KeyValuePair<string, CameraInternals.CameraDebugger> debugger in debuggers)
            {
                debugger.Value.Delete();
            }
            debuggers.Clear();
        }

        /// <summary>
        /// Checks if a vector identified by the unique identifier exists in the debugger.
        /// </summary>
        /// <param name="vectorId">The unique identifier for the vector.</param>
        /// <returns>True if the vector exists; otherwise, false.</returns>
        public static bool ContainsVector(string vectorId)
        {
            if (debuggers == null) return false;
            return debuggers.ContainsKey(vectorId);
        }

        /// <summary>
        /// Updates the position and color of an existing vector identified by the unique identifier.
        /// </summary>
        /// <param name="node1">The new origin position of the vector.</param>
        /// <param name="node2">The new destination position of the vector.</param>
        /// <param name="color">The new color for the vector.</param>
        /// <param name="vectorId">The unique identifier for the vector.</param>
        public static void UpdateVector(Vector3 node1, Vector3 node2, Color color, string vectorId)
        {
            if (!ContainsVector(vectorId)) return;

            UpdatePosition(vectorId, node1, node2);
            UpdateColor(vectorId, color);
        }
    }
}