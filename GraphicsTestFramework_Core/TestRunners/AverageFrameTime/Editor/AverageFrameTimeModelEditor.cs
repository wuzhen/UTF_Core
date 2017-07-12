using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GraphicsTestFramework
{
	[CustomEditor(typeof(AverageFrameTimeModel))]
	public class AverageFrameTimeModelEditor : TestModelEditor{

		public override void OnInspectorGUI()
		{
			AverageFrameTimeModel m_Target = (AverageFrameTimeModel)target;
			AverageFrameTimeSettings s_Target = (AverageFrameTimeSettings)m_Target.p_Settings;

			DrawCommon (s_Target);//Draw the SettingsBase settings
			s_Target.passFailThreshold = EditorGUILayout.FloatField ("Pass/Fail Threshold (ms Difference)", s_Target.passFailThreshold);//slider for pass/fail as it is a percentage of pixel difference
			EditorGUILayout.Space ();//some space before custom settings

			EditorGUILayout.LabelField ("Average Frame Time Settings", EditorStyles.boldLabel);//Custom settings
			s_Target.timingType = (AverageFrameTimeSettings.TimingType)EditorGUILayout.EnumPopup ("Timing Type", s_Target.timingType);
			if (s_Target.timingType == AverageFrameTimeSettings.TimingType.Custom)
				s_Target.customTimingMultiplier = EditorGUILayout.FloatField (new GUIContent ("Custom Timing Multiplier", "This number is used to multiply the ticks output by the sampling"), s_Target.customTimingMultiplier);
			s_Target.sampleFrames = EditorGUILayout.IntField (new GUIContent ("Sample Frames", "The amount of rendered frames to capture performance over"), s_Target.sampleFrames);

		}

	}
}
