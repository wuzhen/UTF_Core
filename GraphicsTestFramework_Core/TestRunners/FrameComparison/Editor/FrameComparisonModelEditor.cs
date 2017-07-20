using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GraphicsTestFramework
{
	[CustomEditor(typeof(FrameComparisonModel))]
	public class FrameComparisonModelEditor : TestModelEditor{

		bool showAdvanced;

		[SerializeField]
		FrameComparisonModel m_Target;

		public override void OnInspectorGUI()
		{
			m_Target = (FrameComparisonModel)target;
			FrameComparisonSettings s_Target = (FrameComparisonSettings)m_Target.p_Settings;

			DrawCommon (m_Target.p_Settings);//Draw the SettingsBase settings
			s_Target.passFailThreshold = EditorGUILayout.Slider ("Pass/Fail Threshold (% Difference)", s_Target.passFailThreshold, 0f, 100f);//slider for pass/fail as it is a percentage of pixel difference
			EditorGUILayout.Space ();//some space before custom settings

			EditorGUILayout.LabelField ("Frame Comparison Settings", EditorStyles.boldLabel);//Custom settings
			s_Target.captureCamera = (Camera)EditorGUILayout.ObjectField ("Capture Camera ", s_Target.captureCamera, typeof(Camera), true);
			if (m_Target.p_Settings.captureCamera == null)
				EditorGUILayout.HelpBox ("Please select a camera for the Frame Comparison to use.", MessageType.Warning);
			s_Target.frameResolution = (FrameResolution)EditorGUILayout.EnumPopup ("Capture Resolution", s_Target.frameResolution);

			showAdvanced = EditorGUILayout.Foldout (showAdvanced, "Advanced");
			if (showAdvanced){
				EditorGUI.indentLevel++;
				s_Target.textureFormat = (TextureFormat)EditorGUILayout.EnumPopup ("Image Format", s_Target.textureFormat);
				s_Target.filterMode = (FilterMode)EditorGUILayout.EnumPopup ("Image Filtermode", s_Target.filterMode);
				EditorGUI.indentLevel--;
			}
		}

	}
}
