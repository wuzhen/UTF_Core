using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // TestModelBase
    // - Lowest level TestModel class that all models derive from
    // - Hides most logic away from end user

    public abstract class TestModelBase : MonoBehaviour
    {
        //public Type logic;
        public Type logic { get; set; } // Reference to the models logic type

        public abstract void SetLogic();
    }

    // ------------------------------------------------------------------------------------
    // TestModel
    // - Next level TestModel class that all user facing logics derive from
    // - Adds an abstraction layer for defining logic type

    public abstract class TestModel<L> : TestModelBase where L : TestLogicBase
    {
        // Set test logic type
        public override void SetLogic()
        {
            logic = typeof(L); // Set type
        }
    }
}
