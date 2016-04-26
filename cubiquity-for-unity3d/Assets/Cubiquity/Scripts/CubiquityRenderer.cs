using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.VR;
using UnityEngine.Rendering;

#if CUBIQUITY_NATIVE_RENDERER

namespace Cubiquity
{
static class MatrixExtension
{

    public static float[] toFloatArray(this Matrix4x4 _matrix)
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
}

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
    private static extern void UpdateCamera(float[] viewMatrix, float[] projectionMatrix, bool isVR);

    [DllImport("CubiquityPlugin")]
    private static extern void SetUnityStreamingAssetsPath([MarshalAs(UnmanagedType.LPStr)] string path);
    [DllImport("CubiquityPlugin")]
    private static extern IntPtr GetRenderEventFunc();

    private CommandBuffer commandBuffer;
    public Camera camera;

    void Start()
    {
        SetUnityStreamingAssetsPath(Application.streamingAssetsPath);

        commandBuffer = new CommandBuffer();
        commandBuffer.IssuePluginEvent(GetRenderEventFunc(), 1);
        camera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
        GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
    }

    void OnWillRenderObject()
    {
        // Set time for the plugin
        SetTimeFromUnity(Time.timeSinceLevelLoad);
        if (Camera.current != null)
        {
            var viewMatrix = Camera.current.worldToCameraMatrix;
            var projectionMatrix = GL.GetGPUProjectionMatrix(Camera.current.projectionMatrix, VRSettings.enabled);

            UpdateCamera(viewMatrix.toFloatArray(), projectionMatrix.toFloatArray(), VRSettings.enabled);
        }
    }
}

}

#endif