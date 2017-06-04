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
        public int id { get; set; } // Unique ID
        
        public Type logic { get; set; } // Reference to the models logic type

        public abstract void SetID();

        public abstract void SetLogic();
    }

    // ------------------------------------------------------------------------------------
    // TestModel
    // - Next level TestModel class that all user facing logics derive from
    // - Adds an abstraction layer for defining logic type

    public abstract class TestModel<L> : TestModelBase where L : TestLogicBase
    {
        public abstract override void SetID();

        // Set test logic type
        public override void SetLogic()
        {
            logic = typeof(L); // Set type
        }
    }
}
