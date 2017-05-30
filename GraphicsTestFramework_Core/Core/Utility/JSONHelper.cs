using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
	/// <summary>
	/// JSON helper.
	/// </summary>
	public class JSONHelper : MonoBehaviour {
		
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// Converting to JSON
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Converts a ResultsIO class 
		/// </summary>
		/// <returns>The JSO.</returns>
		/// <param name="inputData">Input data.</param>
		public static string[] ToJSON (ResultsIOData inputData)
		{
			if (inputData.resultsRow.Count <= 1) {
				Debug.LogWarning ("Invalid ResultsIOData sent to Serializer");
				return null;
			} else {
				string[] output = new string[inputData.resultsRow.Count];
				string[] tableLabels = new string[inputData.resultsRow [0].resultsColumn.Count];
				for (int i = 0; i < tableLabels.Length; i++)// column labels in string array
					tableLabels [i] = inputData.resultsRow [0].resultsColumn [i];
				for (int rows = 0; rows < inputData.resultsRow.Count; rows++) {
					output [rows] = "{";
					for (int columns = 0; columns < inputData.resultsRow [rows].resultsColumn.Count; columns++) {
						/*if (columns == 9)//rip out the commonResults class - TODO remove if works
							continue;*/
						string s = "\"" + inputData.resultsRow [0].resultsColumn [columns] + "\":\"" + inputData.resultsRow [rows].resultsColumn [columns] + "\"";
						if (columns < inputData.resultsRow [0].resultsColumn.Count - 1)
							s += ",";
						output [rows] += s;
					}
					output [rows] += "}";
				}
				if (Master.Instance.debugMode == Master.DebugMode.Messages)
					Debug.Log ("Serialized ResultsIOData");
				return output;
			}
		}

		/// <summary>
		/// Takes a dictionary of key value pairs and returns a JSON formatted string.
		/// </summary>
		/// <returns>A string formatted as JSON</returns>
		/// <param name="inputData">Dictionary of key value pairs to be converted.</param>
		public static string ToJSON (Dictionary<string, string> inputData)
		{
			string output = "{";
			int i = 0;
			foreach (string key in inputData.Keys) {
				string s = "\"" + key + "\":\"" + inputData[key] + "\"";
				if (i < inputData.Count - 1)
					s += ",";
				output += s;
				i++;
			}
			output += "}";
			return output;
		}

		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// Converting from JSON
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------


		/// <summary>
		/// Converts a JSON string to a dictionary list.
		/// </summary>
		/// <returns>A dictionary of kay value pairs.</returns>
		/// <param name="inputData">Input unformatted JSON string.</param>
		public static Dictionary<string, string> JSON_Dictionary(string inputData){
			string[] separators = new string[]{ "\",\"", "\":\"" };//split by the two JSON separators
			string[] splitData = inputData.Substring (2, inputData.Length - 4).Split (separators, System.StringSplitOptions.None);//remove curly brackets
			Dictionary<string, string> convertedJSON = new Dictionary<string, string> ();

			for (int i = 0; i < splitData.Length/2; i ++) {
				convertedJSON.Add (splitData [i * 2], splitData [(i * 2) + 1]);
			}
			return convertedJSON;
		}

		/// <summary>
		/// Converts JSON to the ResultsIO class.
		/// </summary>
		/// <returns>A ResultsIO is returned.</returns>
		/// <param name="inputData">Input unformatted JSON string.</param>
		public static ResultsIOData FromJSON (string inputData)
		{
			if (inputData != null) {
				string[] separators = new string[]{ "\",\"", "\":\"" };//split by the two JSON separators
				string[] splitData = inputData.Substring (2, inputData.Length - 5).Split (separators, System.StringSplitOptions.None);//remove curly brackets
				ResultsIOData data = new ResultsIOData ();//new ResultsIOData
				data.resultsRow.Add (new ResultsIORow ());

				for (int i = 0; i < splitData.Length; i++) {
					int cur = i;
					data.resultsRow [0].resultsColumn.Add (splitData [cur]);
				}
				return data;
			} else
				return null;
		}

		/// <summary>
		/// Converts multiple JSON strings to the ResultsIO class, populating multiple results rows.
		/// </summary>
		/// <returns>A ResultsIO is returned.</returns>
		/// <param name="inputData">Array of unformatted JSON strings.</param>
		public static ResultsIOData FromJSON (string[] inputData)
		{
			if (inputData.Length != 0) {
				string[] separators = new string[]{ "\",\"", "\":\"" };//split by the two JSON separators
				ResultsIOData data = new ResultsIOData ();//new ResultsIOData

				for (int a = 0; a < inputData.Length; a++) {
					string[] splitData = inputData[a].Substring (2, inputData[a].Length - 5).Split (separators, System.StringSplitOptions.None);//remove curly brackets
					ResultsDataCommon RDC = new ResultsDataCommon ();
					for (int i = 0; i < splitData.Length; i++) {
						data.resultsRow.Add(new ResultsIORow());
						int cur = i;
						data.resultsRow [a].resultsColumn.Add (splitData [cur]);

						switch (i) {
						case 1:
							RDC.DateTime = splitData [cur];
							break;
						case 3:
							RDC.UnityVersion = splitData [cur];
							break;
						case 5:
							RDC.AppVersion = splitData [cur];
							break;
						case 7:
							RDC.Platform = splitData [cur];
							break;
						case 9:
							RDC.API = splitData [cur];
							break;
						case 11:
							RDC.RenderPipe = splitData [cur];
							break;
						case 13:
							RDC.SceneName = splitData [cur];
							break;
						case 15:
							RDC.TestName = splitData [cur];
							break;
						default:
							break;
						}
					}
					data.resultsRow [a].commonResultsIOData = RDC;
				}
				return data;
			} else
				return null;
		}
	}
}
