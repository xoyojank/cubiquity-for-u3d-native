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

	[DllImport("CubiquityPlugin")]
	private static extern void SetTimeFromUnity(float t);
    [DllImport("CubiquityPlugin")]
    private static extern void UpdateVolume(uint volumeHandle, float[] viewMatrix, float[] projectionMatrix);

	[DllImport("CubiquityPlugin")]
	private static extern void SetUnityStreamingAssetsPath([MarshalAs(UnmanagedType.LPStr)] string path);
	[DllImport("CubiquityPlugin")]
	private static extern IntPtr GetRenderEventFunc();

    private static float[] MatrixToArray(Matrix4x4 _matrix)
    {
        float[] result = new float[16];

        for (int _row = 0; _row < 4; _row++)
        {
            for (int _col = 0; _col < 4; _col++)
            {
                result[_col + _row * 4] = _matrix[_row, _col];
            }
        }

        return result;
    }

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

            if (Camera.current != null)
            {
                var eyePosition = Camera.main.transform.position;
                var viewMatrix = Camera.main.worldToCameraMatrix;
                var projectionMatrix = GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, false);
                var cameraPosition = viewMatrix.inverse.GetColumn(3);

                UpdateVolume(0, MatrixToArray(viewMatrix), MatrixToArray(projectionMatrix));
            }

			// Issue a plugin event with arbitrary integer identifier.
			// The plugin can distinguish between different
			// things it needs to do based on this ID.
			// For our simple plugin, it does not matter which ID we pass here.
			GL.IssuePluginEvent(GetRenderEventFunc(), 1);
		}
	}
}
