using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // MenuResolveListEntry
    // - Instance of an entry in the Resolve list

    public class MenuResolveListEntry : MonoBehaviour 
	{
        // ------------------------------------------------------------------------------------
        // Variables

        public Text line1; // Top line
		public Text line2; // Bottom line

        // ------------------------------------------------------------------------------------
        // Initialization

        // Setup the instance
        public void Setup(string input1, string input2)
		{
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting up resolve list entry"); // Write to console
            line1.text = input1; // Set top line
			line2.text = input2; // Set bottom line
		}
	}
}
