using UnityEngine;
using System.Collections;

using Cubiquity.Impl;

namespace Cubiquity
{
	/// Controls some visual aspects of the volume and allows it to be rendered.
	/**
	 * The role of the VolumeRenderer component for volumes is conceptually similar to the role of Unity's MeshRenderer class for meshes.
	 * Specifically, it can be attached to a GameObject which also has a Volume component to cause that Volume component to be drawn. It 
	 * also exposes a number of properties such as whether a volume should cast and receive shadows.
	 * 
	 * Remember that Cubiquity acctually draws the volume by creating standard Mesh objects. Internally Cubiquity will copy the properties
	 * of the VolumeRenderer to the MeshRenderers which are generated.
	 * 
	 * \sa VolumeCollider
	 */
    [ExecuteInEditMode]
	public abstract class VolumeRenderer : MonoBehaviour
	{
        void OnEnable()
        {
            hasChanged = true;
        }

        void OnDisable()
        {
            hasChanged = true;
        }

        /*void OnDestroy()
        {
            Utility.DestroyOrDestroyImmediate(mMaterial);
            Utility.DestroyOrDestroyImmediate(mMaterialLod1);
            Utility.DestroyOrDestroyImmediate(mMaterialLod2);
        }*/

        /// Material for this volume.
        public Material material
        {
            get
            {
                return mMaterial;
            }
            set
            {
                if (mMaterial != value)
                {
                    mMaterial = value;

                    // All Cubiquity materials have some standard parameters, and we should
                    // probably refactor these into some kind of base material if Unity supports that?
                    mMaterial.SetFloat("_height", 0.0f);
                    computeNormalMultiplier(mMaterial);

                    // The material has been changed, so the LODed version will be regenerated on demand.
                    mMaterialLod1 = null;
                    mMaterialLod2 = null;

                    hasChanged = true;
                }
            }
        }
        [SerializeField]
		private Material mMaterial;

        public Material materialLod1
        {
            get
            {
                if (mMaterialLod1 == null)
                {
                    mMaterialLod1 = new Material(mMaterial);
                    
                    mMaterialLod1.SetFloat("_height", 1.0f);
                    computeNormalMultiplier(mMaterialLod1);
                }
                return mMaterialLod1;
            }
        }
        private Material mMaterialLod1;

        public Material materialLod2
        {
            get
            {
                if (mMaterialLod2 == null)
                {
                    mMaterialLod2 = new Material(mMaterial);

                    mMaterialLod2.SetFloat("_height", 2.0f);
                    computeNormalMultiplier(mMaterialLod2);
                }
                return mMaterialLod2;
            }
        }
        private Material mMaterialLod2;
		
		/// Controls whether this volume casts shadows.
		public bool castShadows
		{
			get
			{
				return mCastShadows;
			}
			set
			{
				if(mCastShadows != value)
				{
					mCastShadows = value;
                    hasChanged = true;
				}
			}
		}
		[SerializeField]
		private bool mCastShadows = true;
		
		/// Controls whether this volume receives shadows.
		public bool receiveShadows
		{
			get
			{
				return mReceiveShadows;
			}
			set
			{
				if(mReceiveShadows != value)
				{
					mReceiveShadows = value;
                    hasChanged = true;
				}
			}
		}
		[SerializeField]
		private bool mReceiveShadows = true;

		/// Controls whether the wireframe overlay is displayed when this volume is selected in the editor.
		public bool showWireframe
		{
			get
			{
				return mShowWireframe;
			}
			set
			{
				if(mShowWireframe != value)
				{
					mShowWireframe = value;
                    hasChanged = true;
				}
			}
		}
		[SerializeField]
		private bool mShowWireframe = false;

        /// Controls the point at which Cubiquity switches to a different level of detail.
        public float lodThreshold
        {
            get
            {
                return mLodThreshold;
            }
            set
            {
                float difference = Mathf.Abs(mLodThreshold - value);
                if(difference > 0.000001)
                {
                    hasChanged = true;
                }

                // We set this even if the movement is tiny, otherwise
                // I'm concerned the slider in the editor could get stuck.
                mLodThreshold = value;                
            }
        }
        [SerializeField]
        private float mLodThreshold = 1.0f;

        /// Specifies the lowest (least detailed) LOD which Cubiquity will render for this volume.
        public int minimumLOD
        {
            get
            {
                return mMinimumLOD;
            }
            set
            {
                if (mMinimumLOD != value)
				{
                    mMinimumLOD = value;
                    hasChanged = true;
				}
            }
        }
        [SerializeField]
        private int mMinimumLOD = 0;
		
		/// \cond
        [System.NonSerialized]
        public bool hasChanged = true;
		/// \endcond
		
		// Dummy start method rqured for the 'enabled' checkbox to show up in the inspector.
		void Start() { }

        private void computeNormalMultiplier(Material mat)
        {
            // We compute surface normals using derivative operations in the fragment shader, but for some reason
            // these are backwards on Linux. We can correct for this in the shader by setting the multiplier below.
#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR
            float normalMultiplier = -1.0f;
#else
            float normalMultiplier = 1.0f;
#endif
            mat.SetFloat("normalMultiplier", normalMultiplier);
        }
	}
}
