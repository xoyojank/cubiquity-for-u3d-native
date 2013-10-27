﻿using UnityEngine;
using System.Collections;

[System.Serializable]
public sealed class TerrainVolumeBrush
{
	public bool isVisible = true; // Visible by default so the user doesn't wonder where their custom brush is.
	public Vector3 center = new Vector3(0.0f, 0.0f, 0.0f);
	public float innerRadius = 8.0f;
	public float outerRadius = 10.0f;
	public float opacity = 1.0f;
	public Color color = Color.white;
}