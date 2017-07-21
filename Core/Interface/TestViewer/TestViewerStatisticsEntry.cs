using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // StatisticsEntry
    // - Instance for an entry in the Statistics window

    public class TestViewerStatisticsEntry : MonoBehaviour
    {
        // ------------------------------------------------------------------------------------
        // Variables

        public Text label;
        public Text value;

        // ------------------------------------------------------------------------------------
        // Initialization

        // Setup the instance
        public void Setup(TestViewerTabData.TestViewerTabStatistic input)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setup"); // Write to console
            label.text = input.statLabel; // Set label
            value.text = input.statValue; // Set value
        }
    }
}
