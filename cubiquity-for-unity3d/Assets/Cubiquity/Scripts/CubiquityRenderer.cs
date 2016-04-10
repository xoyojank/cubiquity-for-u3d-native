using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;


public class CubiquityRenderer : MonoBehaviour
{
	// Native plugin rendering events are only called if a plugin is used
	// by some script. This means we have to DllImport at least
	// one function in some active script.
	// For this example, we'll call into plugin's SetTimeFromUnity
	// function and pass the current time so the plugin can animate.

	[DllImport ("CubiquityPlugin")]
	private static extern void SetTimeFromUnity(float t);

	[DllImport("CubiquityPlugin")]
	private static extern void SetUnityStreamingAssetsPath([MarshalAs(UnmanagedType.LPStr)] string path);
	[DllImport("CubiquityPlugin")]
	private static extern IntPtr GetRenderEventFunc();

	IEnumerator Start ()
    {
		SetUnityStreamingAssetsPath(Application.streamingAssetsPath);
		yield return StartCoroutine("CallPluginAtEndOfFrames");
	}

	private IEnumerator CallPluginAtEndOfFrames()
	{
		while (true)
        {
			// Wait until all frame rendering is done
			yield return new WaitForEndOfFrame();

			// Set time for the plugin
			SetTimeFromUnity(Time.timeSinceLevelLoad);

			// Issue a plugin event with arbitrary integer identifier.
			// The plugin can distinguish between different
			// things it needs to do based on this ID.
			// For our simple plugin, it does not matter which ID we pass here.
			GL.IssuePluginEvent(GetRenderEventFunc(), 1);
		}
	}
}
