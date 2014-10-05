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
            if (gameObject == null)
                return;

            List<GameObject> childObjects = new List<GameObject>();
            foreach (Transform childTransform in gameObject.transform)
            {
                childObjects.Add(childTransform.gameObject);
            }

            foreach(GameObject childObject in childObjects)
            {
                DestroyImmediateWithChildren(childObject);
            }

            Debug.Log("Detroying " + gameObject.name);
            Object.DestroyImmediate(gameObject);
        }
	}
}
