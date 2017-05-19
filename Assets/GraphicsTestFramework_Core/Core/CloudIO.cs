using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace GraphicsTestFramework
{
    public class CloudIO : MonoBehaviour
    {
        public static CloudIO cloudIO;

        //Called on Awake
        private void Awake()
        {
            CloudIO.cloudIO = this;
        }

        //Receive data from test runners
        public void SetData(string testType, CloudData inputData)
        {
            StartCoroutine(WriteDataToCloud(testType, inputData));
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Cloud IO received data");
        }

        //Write data to the cloud
        IEnumerator WriteDataToCloud(string tableName, CloudData inputData)
        {
            string[] output = new string[inputData.cloudRow.Count];
            string[] tableLabels = new string[inputData.cloudRow[0].cloudColumn.Count];
            for (int i = 0; i < tableLabels.Length; i++)
                tableLabels[i] = inputData.cloudRow[0].cloudColumn[i];
            CloudConnectorCore.CreateTable(tableLabels, tableName, !Application.isEditor);
            for (int rows = 1; rows < inputData.cloudRow.Count; rows++)
            {
                output[rows] = "{";
                for (int columns = 0; columns < inputData.cloudRow[rows].cloudColumn.Count; columns++)
                {
                    string s = "\"" + inputData.cloudRow[0].cloudColumn[columns] + "\":\"" + inputData.cloudRow[rows].cloudColumn[columns] + "\"";
                    if (columns < inputData.cloudRow[0].cloudColumn.Count - 1)
                        s += ",";
                    output[rows] += s;
                }
                output[rows] += "}";
                while (CloudConnectorCore.isWaiting == true)
                    yield return null;
                CloudConnectorCore.CreateObject(output[rows], tableName, !Application.isEditor);
            }
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Cloud IO wrote cloud data for "+tableName);
        }
    }

    /// <summary>
    /// DATA CLASSES
    /// </summary>

    [System.Serializable]
    public class CloudData
    {
        public List<CloudRow> cloudRow = new List<CloudRow>();
    }

    [System.Serializable]
    public class CloudRow
    {
        public List<string> cloudColumn = new List<string>();
    }
}
