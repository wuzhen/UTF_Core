using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GraphicsTestFramework
{
	[CustomEditor(typeof(FlipBookModel))]
	public class FlipBookModelEditor : TestModelEditor{

		bool showAdvanced;
		FrameResolution[] allowedResolutions = new FrameResolution[]{FrameResolution.nHD, FrameResolution.qHD};
		SettingsBase.WaitType[] allowedWaitTypes = new SettingsBase.WaitType[]{SettingsBase.WaitType.Frames, SettingsBase.WaitType.Seconds};

		public override void OnInspectorGUI()
		{
			FlipBookModel m_Target = (FlipBookModel)target;
			FlipBookSettings s_Target = (FlipBookSettings)m_Target.p_Settings;

			DrawCommon (s_Target);//Draw the SettingsBase settings
			s_Target.passFailThreshold = EditorGUILayout.Slider ("Pass/Fail Threshold (% Difference)", s_Target.passFailThreshold, 0f, 100f);//slider for pass/fail as it is a percentage of pixel difference
			EditorGUILayout.Space ();//some space before custom settings

			EditorGUILayout.LabelField ("Flipbook Settings", EditorStyles.boldLabel);//Custom settings
			s_Target.captureCamera = (Camera)EditorGUILayout.ObjectField ("Capture Camera ", s_Target.captureCamera, typeof(Camera), true);
			if (s_Target.captureCamera == null)
				EditorGUILayout.HelpBox ("Please select a camera for the Frame Comparison to use.", MessageType.Warning);

			s_Target.framesToCapture = EditorGUILayout.IntPopup ("Capture Frame Count", s_Target.framesToCapture, new string[] { // Drop down for frames to capture
				"2x2 (4 frames)",
				"3x3 (9 frames)",
				"4x4 (16 frames)"
			}, new int[] {
				4,
				9,
				16
			});

			s_Target.frameResolution = (FrameResolution)EditorGUILayout.IntPopup ("Capture Resolution", (int)s_Target.frameResolution, new string[] {
				allowedResolutions [0].ToString (),
				allowedResolutions [1].ToString ()
			}, new int[] {
				(int)allowedResolutions [0],
				(int)allowedResolutions [1]
			});

			//s_Target.frameResolution = (FrameResolution)EditorGUILayout.EnumPopup ("Capture Resolution", s_Target.frameResolution);
			EditorGUILayout.LabelField ("Flipbook Timing Settings", EditorStyles.boldLabel);//Custom settings
			//s_Target.captureWaitType = (SettingsBase.WaitType)EditorGUILayout.EnumPopup ("Wait type between captures", s_Target.captureWaitType);
			s_Target.captureWaitType = (SettingsBase.WaitType)EditorGUILayout.IntPopup ("Wait type between captures", (int)s_Target.captureWaitType, new string[] {
				allowedWaitTypes [0].ToString (),
				allowedWaitTypes [1].ToString ()
			}, new int[] {
				(int)allowedWaitTypes [0],
				(int)allowedWaitTypes [1]
			});
			if (s_Target.captureWaitType == SettingsBase.WaitType.Frames)
				s_Target.captureWaitFrames = EditorGUILayout.IntField ("Frames to wait", s_Target.captureWaitFrames);
			else if (s_Target.captureWaitType == SettingsBase.WaitType.Seconds)
				s_Target.captureWaitSeconds = EditorGUILayout.FloatField ("Seconds to wait", s_Target.captureWaitSeconds);

			showAdvanced = EditorGUILayout.Foldout (showAdvanced, "Advanced Options");
			if (showAdvanced){
				EditorGUI.indentLevel++;
				s_Target.textureFormat = (TextureFormat)EditorGUILayout.EnumPopup ("Image Format", s_Target.textureFormat);
				s_Target.filterMode = (FilterMode)EditorGUILayout.EnumPopup ("Image Filtermode", s_Target.filterMode);
				EditorGUI.indentLevel--;
			}
		}
	}
}
