﻿using UnityEngine;
using UnityEditor;

using System.Collections;

namespace Cubiquity
{	
	[CustomEditor (typeof(ColoredCubesVolumeRenderer))]
	public class ColoredCubesVolumeRendererInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			ColoredCubesVolumeRenderer renderer = target as ColoredCubesVolumeRenderer;
			
			float labelWidth = 120.0f;
			
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Receive Shadows:", GUILayout.Width(labelWidth));
				renderer.receiveShadows = EditorGUILayout.Toggle(renderer.receiveShadows);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Cast Shadows:", GUILayout.Width(labelWidth));
				renderer.castShadows = EditorGUILayout.Toggle(renderer.castShadows);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Show Wireframe:", GUILayout.Width(labelWidth));
			renderer.showWireframe = EditorGUILayout.Toggle(renderer.showWireframe);
			EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("LOD Threshold:", GUILayout.Width(labelWidth));
            renderer.lodThreshold = EditorGUILayout.Slider(renderer.lodThreshold, 0.5f, 2.0f);
            EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
				renderer.material = EditorGUILayout.ObjectField("Material: ", renderer.material, typeof(Material), true) as Material;
			EditorGUILayout.EndHorizontal();
		}
	}
}