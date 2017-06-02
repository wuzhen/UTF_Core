using System;
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
        int activeTestLogic; // Track the active logic

        // ------------------------------------------------------------------------------------
        // Instance Management

        // Generate script instances for a type (Called by TestStructure)
        public void GenerateTestTypeInstances(TestModel model)
        {
            string typeName = model.logic.ToString().Replace("GraphicsTestFramework.", "").Replace("Logic", ""); // Get type name from logic name
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Generating test type instances for " + typeName); // Write to console
            Transform instanceParent = Master.Instance.transform.Find("TestRunners"); // Find instance parent
            if (instanceParent) // If it exists
            {
                if (!instanceParent.Find(typeName)) // If instance doesnt already exist
                {
                    GameObject newChild = new GameObject(); // Create a gameobject to hold the instance
                    newChild.transform.SetParent(instanceParent); // Set parent
                    newChild.name = typeName; // Set gameobject name
                    TestLogicBase logic = (TestLogicBase)newChild.AddComponent(model.logic); // Add logic component
                    logic.SetName(); // Set name on logic
                    logic.SetDisplay(); // Set display on logic
                    logic.SetResults(); // Set results type on logic
                    TestDisplayBase display = (TestDisplayBase)newChild.AddComponent(logic.display); // Add display component
                    display.SetLogic(logic); // Set logic on display
                    display.GetResultsContextObject(); // Get context object for results entry
                    AddType(logic); // Add to type list
                }
            }
            else
                Console.Instance.Write(DebugLevel.Critical, MessageLevel.Log, "Test Runner parent not found! Aborting"); // Write to console
        }

        // ------------------------------------------------------------------------------------
        // Set Data

        // Add a new test type
        void AddType(TestLogicBase inputLogic)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Adding new TestType of " + inputLogic.name); // Write to console
            TestType newType = new TestType(); // Create new class instance
            newType.typeName = inputLogic.name; // Set name from input
            newType.logicInstance = inputLogic; // Set instance from input
            typeList.Add(newType); // Add to list
        }

        // Set the active test logic
        public void SetActiveLogic(TestLogicBase inputLogic)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting active test logic to " + inputLogic.name); // Write to console
            for (int i = 0; i < typeList.Count; i++) // Iterate types
            {
                if (typeList[i].logicInstance == inputLogic) // If logic instance is equal to input
                    activeTestLogic = i; // Set as active
            }
        }

        // ------------------------------------------------------------------------------------
        // Get Data

        // Get a logic instance from a type name
        public TestLogicBase GetLogicInstanceFromName(string nameInput)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Getting logic instance from name " + nameInput); // Write to console
            for (int i = 0; i < typeList.Count; i++) // Iterate types
            {
                if(typeList[i].typeName == nameInput) // If type name is equal to input
                    return typeList[i].logicInstance; // Return it
            }
            return null; // Otherwise return null
        }

        // Get a type name from its type index
        // TODO - Move these methods to a unique ID
        public string GetTestTypeNameFromIndex(int index)
        {
            List<Type> types = Common.GetSubTypes<TestModel>(); // Get a type list
            return types[index].ToString().Replace("GraphicsTestFramework.", "").Replace("Model", ""); // Return the type name of the requested index
        }

        // Get the active test logic
        public TestLogicBase GetActiveTestLogic()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Getting active test logic"); // Write to console
            return typeList[activeTestLogic].logicInstance; // Return active
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

