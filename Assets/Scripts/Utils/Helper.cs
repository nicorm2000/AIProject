using UnityEngine;

namespace Utils
{
    public class Helper : MonoBehaviour
    {
        /// <summary>
        /// Instantiates a prefab at the specified position and rotation.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <param name="position">The position to instantiate the prefab at.</param>
        /// <param name="rotation">The rotation to apply to the instantiated prefab.</param>
        /// <returns>The instantiated GameObject, or null if the prefab was not provided.</returns>
        public static GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            // Check if the prefab is null before attempting to instantiate
            if (prefab == null)
            {
                Debug.LogError("Prefab is null. Unable to instantiate.");
                return null;
            }

            // Instantiate the prefab at the specified position and rotation
            return Object.Instantiate(prefab, position, rotation);
        }
    }
}