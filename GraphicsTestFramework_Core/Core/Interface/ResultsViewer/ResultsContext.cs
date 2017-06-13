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
        public Button viewTestButton;
        public GameObject[] objects;
    }
}
