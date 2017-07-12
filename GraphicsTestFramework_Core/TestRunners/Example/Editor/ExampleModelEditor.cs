using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GraphicsTestFramework
{
	[CustomEditor(typeof(ExampleModel))]
	public class ExampleModelEditor : TestModelEditor{

		public override void OnInspectorGUI()
		{
			ExampleModel m_Target = (ExampleModel)target;
			ExampleSettings s_Target = (ExampleSettings)m_Target.p_Settings;

			DrawCommon (s_Target);//Draw the SettingsBase settings
			s_Target.passFailThreshold = EditorGUILayout.FloatField ("Pass/Fail Threshold (ms Difference)", s_Target.passFailThreshold);//slider for pass/fail as it is a percentage of pixel difference

		}

	}
}
