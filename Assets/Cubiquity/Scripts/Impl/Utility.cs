using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Cubiquity.Impl
{
	public class Utility
	{
		// We use a static Random for making filenames, as Randoms are seeded by timestamp
		// and client code could potentially create a number of filenames in quick sucession.  
		private static System.Random randomIntGenerator = new System.Random();
			
		public static string GenerateRandomVoxelDatabaseName()
		{
			// Generate a random filename from an integer
			return randomIntGenerator.Next().ToString("X8") + ".vdb";
		}

        public static void DestroyImmediateWithChildren(GameObject gameObject)
        {
            // Nothing to do is the object is null
            if (gameObject == null)
                return;

            // Find all the child objects
            List<GameObject> childObjects = new List<GameObject>();
            foreach (Transform childTransform in gameObject.transform)
            {
                childObjects.Add(childTransform.gameObject);
            }

            // Destroy all children
            foreach(GameObject childObject in childObjects)
            {
                DestroyImmediateWithChildren(childObject);
            }

            // Destroy all components. Not sure if this is useful, or if it happens anyway?
            Component[] components = gameObject.GetComponents<Component>();
            foreach(Component component in components)
            {
                // We can't destroy the transform of a GameObject.
                if (component is Transform == false)
                {
                    Object.DestroyImmediate(component);
                }
            }

            // Destroy the object itself.
            Object.DestroyImmediate(gameObject);
        }
	}
}
