using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // ResultsContext
    // - Abstract class for referencing gameobjects for a results menu context object
    // - Display scripts handle how to initialise this

    public class ResultsContext : MonoBehaviour
    {
        // UI References
        public Button viewTestButton;
        public GameObject[] objects;
        // Data
        ResultsEntry activeEntry;

        // Setup the context object
        public void Setup(ResultsEntry inputEntry)
        {
            activeEntry = inputEntry; // Track active results entry
            viewTestButton.onClick.AddListener(delegate { OnClickViewTest(); }); // Add listener for view test
        }

        // On button click: View Test
        public void OnClickViewTest()
        {
            TestDisplayBase display = activeEntry.logic.GetComponent<TestDisplayBase>(); // Get display reference from logic
            ResultsBase results = (ResultsBase)activeEntry.logic.DeserializeResults(activeEntry.resultsData); // Deserialize results for this results entry
            ResultsViewer.Instance.SetState(0); // Disable results viewer
            display.EnableTestViewer(results, new TestViewerToolbar.State(false, false, false, false, false)); // Enable test viewer with active results data
        }
    }
}
