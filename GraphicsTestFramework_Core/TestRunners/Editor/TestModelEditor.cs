using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GraphicsTestFramework
{
	[CustomEditor(typeof(TestModelBase), true)]
	public class TestModelEditor : Editor {
		
		public virtual void DrawCommon(SettingsBase s_Target){
			EditorGUILayout.LabelField ("Common Settings", EditorStyles.boldLabel);


			s_Target.waitType = (SettingsBase.WaitType)EditorGUILayout.EnumPopup (new GUIContent ("Wait Type", "Choose the type of start delay \nFrames=Wait 'X' rendered frames \nSeconds= Wait 'X' seconds \nStable Framerate=Wait for a steady framerate \nCallback=Wait for a custom callback from a script "), s_Target.waitType);

			switch(s_Target.waitType){
			case SettingsBase.WaitType.Frames:
				s_Target.waitFrames = EditorGUILayout.IntField ("Frames to wait", s_Target.waitFrames);
				break;
			case SettingsBase.WaitType.Seconds:
				s_Target.waitSeconds = EditorGUILayout.FloatField ("Seconds to wait", s_Target.waitSeconds);
				break;
			default:
				break;
			}
		}
	}
}
