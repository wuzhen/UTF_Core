using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // Suite Scriptable Object
    // - One instance for each Suite entry
    // - Managed by SuiteController

    [CreateAssetMenu]
    public class Suite : ScriptableObject
    {
        [SerializeField] public string suiteName;
        [SerializeField] public bool isDebugSuite;
        [SerializeField] public List<Group> groups = new List<Group>();
    }

    [Serializable]
    public class Group
    {
        [SerializeField] public string groupName;
        [SerializeField] public List<Test> tests = new List<Test>();
    }

    [Serializable]
    public class Test
    {
        [SerializeField] public UnityEngine.Object scene;
        [SerializeField] public int testTypes;
    }
}
