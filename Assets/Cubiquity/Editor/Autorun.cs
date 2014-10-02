using UnityEngine;
using UnityEditor;

namespace Cubiquity
{
    [InitializeOnLoad]
    public class Autorun
    {
        static Autorun()
        {
            // We don't want to annoy the user with these messages every time they go in and out of
            // play mode. This is a crude way of only doing it the first time they launch the editor.
            bool editorJustLaunched = (EditorApplication.timeSinceStartup < 5);

            if (editorJustLaunched)
            {
#if CUBIQUITY_USE_UNSAFE
                Debug.Log("Using 'unsafe' code (no problem!).");
#else
                Debug.Log("Using 'safe' code.");
#endif
            }
        }
    }
}
