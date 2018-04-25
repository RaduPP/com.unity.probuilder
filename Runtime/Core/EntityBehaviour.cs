﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
	abstract class EntityBehaviour : MonoBehaviour
	{
		[Tooltip("Allow ProBuilder to automatically hide and show this object when entering or exiting play mode.")]
		public bool manageVisibility = true;

		public abstract void Initialize();

		public abstract void OnEnterPlayMode();

		protected void SetMaterial(Material material)
		{
			var pb = GetComponent<ProBuilderMesh>();

			if (pb != null)
			{
                foreach(var face in pb.faces)
    				face.material = material;
				pb.ToMesh();
				pb.Refresh();
			}
			else if (GetComponent<Renderer>())
				GetComponent<Renderer>().sharedMaterial = material;
			else
				gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
		}

		// Not necessary because OnEnterPlayMode is operating on an instance of the scene, not the actual scene
//		public abstract void OnExitPlayMode();
	}
}
