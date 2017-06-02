using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // TestTypeManager
    // - Tracks test types
    // - Tracks active test logic instances
    // - Get test logic references

    public class TestTypeManager : MonoBehaviour
    {
        // ------------------------------------------------------------------------------------
        // Variables

        // Singleton
        private static TestTypeManager _Instance = null;
        public static TestTypeManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = (TestTypeManager)FindObjectOfType(typeof(TestTypeManager));
                return _Instance;
            }
        }

        // Data
        public List<TestType> typeList = new List<TestType>(); // Local list of test types

        // ------------------------------------------------------------------------------------
        // Set Data

        // Add a new test type
        public void AddType(TestLogicBase inputLogic)
        {
            TestType newType = new TestType(); // Create new class instance
            newType.typeName = inputLogic.name; // Set name from input
            newType.logicInstance = inputLogic; // Set instance from input
            typeList.Add(newType); // Add to list
        }

        // ------------------------------------------------------------------------------------
        // Get Data

        // Get a logic instance from a type name
        public TestLogicBase GetLogicInstanceFromName(string nameInput)
        {
            for(int i = 0; i < typeList.Count; i++) // Iterate types
            {
                if(typeList[i].typeName == nameInput) // If type name is equal to input
                    return typeList[i].logicInstance; // Return it
            }
            return null; // Otherwise return null
        }

        // ------------------------------------------------------------------------------------
        // Local Data Structures

        [System.Serializable]
        public class TestType
        {
            public string typeName;
            public TestLogicBase logicInstance;
        }
    }
}

