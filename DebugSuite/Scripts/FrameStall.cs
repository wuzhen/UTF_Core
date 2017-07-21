using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework.DebugSuite
{
    // ------------------------------------------------------------------------------------
    // FrameStall
    // - Stalls a frame for a random time

    public class FrameStall : MonoBehaviour
    {
        public int multiplier = 100; // Multiplier of the frame stall time
        List<Vector3> list = new List<Vector3>(); // List for vectors

        // Every frame
        void Update()
        {
            list.Clear(); // Clear the list
            float ran = Random.Range(0f, 1); // Get a random number
            for(int i = 0; i < ran * multiplier; i++) // Iterate random * multiplier
            {
                Vector3 newVector = new Vector3(ran, ran, ran); //Create a new vector
                list.Add(newVector); // Add it to the list
            }
        }
    }
}
