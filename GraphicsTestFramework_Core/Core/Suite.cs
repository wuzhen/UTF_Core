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
        public string SuiteName;
        public Object[] scenes;
    }
}
