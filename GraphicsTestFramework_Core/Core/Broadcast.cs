using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
	public abstract class Broadcast
	{
		public delegate void EndTestAction ();

		public delegate void EndBuildSetup (); // TODO - Set this up

		public delegate void EndResultsSave ();

		public delegate void LocalBaselineParsed ();
	}
}
