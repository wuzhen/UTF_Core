using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    public class TestTypeManager : MonoBehaviour
    {
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

        public List<TestType> typeList = new List<TestType>();

        public void AddType(TestLogicBase inputLogic)
        {
            TestType newType = new TestType();
            newType.typeName = inputLogic.name;
            newType.typeInstance = inputLogic;
            typeList.Add(newType);
        }

        public TestLogicBase GetLogicInstanceFromName(string nameInput)
        {
            for(int i = 0; i < typeList.Count; i++)
            {
                if(typeList[i].typeName == nameInput)
                {
                    return typeList[i].typeInstance;
                }
            }
            return null;
        }

        [System.Serializable]
        public class TestType
        {
            public string typeName;
            public TestLogicBase typeInstance;
        }
    }
}

