namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // Broadcast
    // - Delegates for broadcasting messages between scripts

    public abstract class Broadcast
	{
        // TestLogic > TesList
        // Current test complete. Continue.
        public delegate void EndTestAction ();

        // TODO - Set this up
        public delegate void EndBuildSetup ();

        // CloudIO > TestLogic
        // Current results saved by CloudIO. End test and continue.
        // --------
        // CloudIO > ViewerToolbar
        // Current results saved by CloudIO. View next test. Used on Baseline resolve path.
        public delegate void EndResultsSave ();

        // ResultsIO > TestStructure
        // Baselines have been parsed. Start structure generation.
		public delegate void LocalBaselineParsed ();
	}
}
