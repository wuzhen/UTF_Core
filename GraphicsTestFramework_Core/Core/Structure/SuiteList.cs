using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // Suite List Scriptable Object
    // - Created automatically by Suite Manager during pre build

    [CreateAssetMenu]
    public class SuiteList : ScriptableObject
    {
        [SerializeField] public List<Suite> suites = new List<Suite>();
    }
}
