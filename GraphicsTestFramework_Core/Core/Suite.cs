using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GraphicsTestFramework
{
    [CreateAssetMenu]
    public class Suite : ScriptableObject
    {
        public string SuiteName;
        public Object[] scenes;
    }
}
