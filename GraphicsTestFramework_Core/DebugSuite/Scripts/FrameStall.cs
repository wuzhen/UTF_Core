using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework.DebugSuite
{
    public class FrameStall : MonoBehaviour
    {
        public int multiplier = 100;
        List<Vector3> list = new List<Vector3>();

        // Update is called once per frame
        void Update()
        {
            list.Clear();
            float ran = Random.Range(0f, 1);
            for(int i = 0; i < ran * multiplier; i++)
            {
                Vector3 newVector = new Vector3(ran, ran, ran);
                list.Add(newVector);
            }
        }
    }
}
