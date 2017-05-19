using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    //[System.Serializable]
    public abstract class TestModel : MonoBehaviour
    {
        public Type logic;

        public abstract void SetLogic();
        public abstract TestLogicBase GetLogic();
    }
}
