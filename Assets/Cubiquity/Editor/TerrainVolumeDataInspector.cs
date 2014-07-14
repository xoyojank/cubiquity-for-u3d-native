using UnityEngine;
using UnityEditor;

using System.Collections;

namespace Cubiquity
{	
	[CustomEditor (typeof(TerrainVolumeData))]
	public class TerrainVolumeDataInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			TerrainVolumeData data = target as TerrainVolumeData;
			
			EditorGUILayout.LabelField("Full path to voxel database:", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox(data.fullPathToVoxelDatabase, MessageType.None);
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Open as read-only:", EditorStyles.boldLabel, GUILayout.Width(150));
				data.writePermissions = GUILayout.Toggle(data.writePermissions == WritePermissions.ReadOnly, "")
					? WritePermissions.ReadOnly : WritePermissions.ReadWrite;
			EditorGUILayout.EndHorizontal();
			if(data.writePermissions == WritePermissions.ReadOnly)
			{
				EditorGUILayout.HelpBox("Opening a voxel database in read-only mode allows multiple VolumeData instances " +
					"to make use of it at the same time. You will still be able to modify the volume data in the editor or " +
					"in play mode, but you will not be able to save the changes back into the voxel database.", MessageType.Info);
			}
			else
			{
				EditorGUILayout.HelpBox("Opening a voxel database in read-write mode (not read-only)" +
				    "allows you to save any changes back to disk. However, in this case only a single " +
					"VolumeData instance can make use of the voxel database.", MessageType.Info);
			}

			EditorUtility.SetDirty(data);
		}
	}
}