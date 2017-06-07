using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // TestViewerStatistics
    // - Controls statistics window within the viewer

    public class TestViewerStatistics : MonoBehaviour
    {
        // ------------------------------------------------------------------------------------
        // Variables

        // Singleton
        private static TestViewerStatistics _Instance = null;
        public static TestViewerStatistics Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = (TestViewerStatistics)FindObjectOfType(typeof(TestViewerStatistics));
                return _Instance;
            }
        }

        // Data
        public GameObject statisticsEntryPrefab;
        bool isHidden = false;
        bool isPopulated = false;
        public List<GameObject> entries = new List<GameObject>();

        // ------------------------------------------------------------------------------------
        // Context & State

        // Set context
        public void SetContext(TestViewerTabData.TestViewerTabStatistic[] input)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting context"); // Write to console
            isPopulated = false; // Reset
            if (input == null) // If no statistics
                gameObject.SetActive(false); // Disable statistics window
            else
                Generate(input); // Generate statistics window
        }

        // Generate window context
        void Generate(TestViewerTabData.TestViewerTabStatistic[] input)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Generating statistics"); // Write to console
            foreach (GameObject go in entries) // Iterate current entries
                Destroy(go); // Delete them
            entries.Clear(); // Reset
            float entryHeight = 0; // Collect entry height to scale window
            for(int i = 0; i < input.Length; i++) // Iterate statistics
            {
                GameObject newStat = Instantiate(statisticsEntryPrefab, transform, false); // Create stat instance
                entries.Add(newStat); // Add to list to track
                RectTransform newStatRect = newStat.GetComponent<RectTransform>(); // Get rect
                newStatRect.anchoredPosition = new Vector2(0, (i + 1) * -newStatRect.sizeDelta.y); // Set position
                entryHeight = newStatRect.sizeDelta.y; // Get height of an entry
                TestViewerStatisticsEntry newStatEntry = newStat.GetComponent<TestViewerStatisticsEntry>(); // Get script reference
                newStatEntry.Setup(input[i]); // Setup the entry
            }
            RectTransform rect = GetComponent<RectTransform>(); // Get rect
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, entryHeight * (input.Length + 1)); // Set window size
            isPopulated = true; // Mark toggle active
            if (!isHidden) // If not hidden
                gameObject.SetActive(true); // Enable statistics window
        }

        // Toggle visibility of the window
        public void ToggleVisible()
        {
            if (isPopulated) // Check if viable
            {
                Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Toggle statistics window"); // Write to console
                isHidden = !isHidden; // Switch hidden state
                gameObject.SetActive(!isHidden); // Set active
            }
        }

        // Set window position
        public void SetPosition(float indent)
        {
            RectTransform rect = GetComponent<RectTransform>(); // Get rect
            rect.anchoredPosition = new Vector2(-10, -(indent + 10)); // Set position
        }
    }
}
