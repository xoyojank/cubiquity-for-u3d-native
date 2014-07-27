using UnityEngine;
using System.Collections;

namespace Cubiquity
{
	/// Specifies the differnt permissions with which a file can be opened.
	/*
	 * This is primarily used when opening voxel databases.
	 * \sa VolumeData
	 */
	public enum WritePermissions
	{
		/// Allow only reading.
		ReadOnly,
		/// Allow both reading an writing.
		ReadWrite
	};
}
