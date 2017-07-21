using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
	[RequireComponent(typeof(Camera))]
	public class CameraDebug : MonoBehaviour 
	{
		public enum CameraTexture 
		{
			Depth,
			Normals,
			MotionVectors
		}

		public CameraTexture debugView;
		public bool overwriteCameraTextureMode = false;
		public float minDistance = 0f;
		public float maxDistance = 1f;
		public float multiplier = 100f;
		Camera _camera;
		Shader _shader;
		Material _material;
 
    static class Uniforms
    {
        internal static readonly int _MotionMultiplier = Shader.PropertyToID("_MotionMultiplier");
        internal static readonly int _MinMax = Shader.PropertyToID("_MinMax");
    }

		void OnEnable()
		{
			if(_shader == null)        
                _shader = Shader.Find("Hidden/CameraDebug");
			_material = new Material(_shader);
			_material.hideFlags = HideFlags.DontSave;
			_camera = GetComponent<Camera>();
			if(overwriteCameraTextureMode)
				SetCameraTextures();
		}

		void OnDisable()
		{
			DestroyImmediate(_material);
		}

		void SetCameraTextures()
		{
			switch(debugView)
			{
				case CameraTexture.Depth:
					_camera.depthTextureMode = DepthTextureMode.Depth;
					break;
				case CameraTexture.Normals:
					_camera.depthTextureMode = DepthTextureMode.DepthNormals;
					break;
				case CameraTexture.MotionVectors:
					_camera.depthTextureMode = DepthTextureMode.MotionVectors;
					break;
			}
		}

		void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			_material.SetFloat (Uniforms._MotionMultiplier, multiplier);
			_material.SetVector(Uniforms._MinMax, new Vector4(minDistance, maxDistance, 0, 0));
			Graphics.Blit (source, destination, _material, (int)debugView);
		}
	}
}
