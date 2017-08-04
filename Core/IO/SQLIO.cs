using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace GraphicsTestFramework.SQL
{	
	public class SQLIO : MonoBehaviour {

		public static SQLIO _Instance = null;//Instance

		public static SQLIO Instance {
			get {
				if (_Instance == null)
					_Instance = (SQLIO)FindObjectOfType (typeof(SQLIO));
				return _Instance;
			}
		}

		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		//CONNECTION VARIABLES
		private string _conString = @"user id=UTF_admin;" +
		                            @"password=chicken22;data source=10.44.41.115;" +
		                            @"database=UTF_testbed;";
		private SqlConnection _connection = null;

		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		//LOCAL VARIABLES
		public bool liveConnection;
		private NetworkReachability netStat;


		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		//INFORMATION
		private SystemData sysData;//local version of systemData


		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		//Base Methods
		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		void Start(){
			InvokeRepeating ("CheckConnection", 0f, 5f); //Invoke network check
		}

		//Init gets called from ResultsIO
		public void Init (SystemData systemData)
		{
			sysData = systemData;
			_connection = new SqlConnection (_conString);//Create a new connnection with the _consString
		}

		void OnEnable(){
			//OpenConnection (_connection); //Try open a conneciton to DB
		}

		void OnDisable(){
			CloseConnection (_connection);//Close SQL conneciton on disable
		}

		//Opens a connection to the SQL DB
		void OpenConnection(SqlConnection connection){
			try
			{
				connection.Open();//open the connection
			}
			catch(Exception e)
			{
				Console.Instance.Write(DebugLevel.Critical, MessageLevel.LogWarning, e.ToString());
			}
			Console.Instance.Write (DebugLevel.File, MessageLevel.Log, "SQL connection is:" + connection.State.ToString ());//write the connection state to log
		}

		//Closes the connection to the SQL DB
		void CloseConnection(SqlConnection connection){
			try
			{
				connection.Close ();//close the connection
			}
			catch(Exception e)
			{
				Console.Instance.Write(DebugLevel.Critical, MessageLevel.LogWarning, e.ToString());
			}
			Console.Instance.Write (DebugLevel.File, MessageLevel.Log, "SQL connection is:" + connection.State.ToString ());//write the connection state to log
		}

		//simple test query, no return
		public string SQLQuery ( string _query ) {
			Debug.LogError (_query);
			try {
				SqlCommand cmd = new SqlCommand(_query, _connection);
				string _returnQuery = (string) cmd.ExecuteScalar ();
				return _returnQuery;
			}
			catch (SqlException _exception) {
				Debug.LogWarning(_exception.ToString());
				return null;
			}
		}

		public SqlDataReader SQLRequest(string _query){
			Debug.LogError (_query);
			try
			{
				SqlDataReader myReader = null;
				SqlCommand    myCommand = new SqlCommand(_query, _connection);
				myReader = myCommand.ExecuteReader();
				return myReader;
			}
			catch (Exception e)
			{
				Debug.Log(e.ToString());
				return null;
			}
		}

		//Testing method TODO - delete me
		[ContextMenu("DoThing")]
		public void DoThing(){
			//CreateTable ("Example", new string[]{"Hello", "Name", "Age"});
			GetbaselineTimestamp ("Debug");
		}

		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		// Query data - TODO wip
		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		public bool TableExists(string tableName){
			string s = SQLQuery ("IF object_id('dbo." + tableName + "') is not null PRINT 'HERE'");
			Debug.Log (s);
			return true;
		}

		public DateTime GetbaselineTimestamp(string suiteName){
			DateTime timestamp = DateTime.MinValue;//make date time min, we check this on the other end since it is not nullable
			SqlDataReader reader = SQLRequest ("SELECT * FROM SuiteBaselineTimestamps WHERE api='" + sysData.API + "' AND suiteName='" + suiteName + "' AND platform='" + sysData.Platform + "';");
			while(reader.Read ()){
				timestamp = reader.GetDateTime (3);
			}
			reader.Close ();
			return timestamp;
		}

		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		// Sending data - TODO wip
		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		//Creates a new table named 'tableName' and with the columns in 'columns'
		public void CreateTable(string tableName, string[] columns){
			string _columns = CreateColumns (columns);//gets a string formatted with data types
			string _stringRequest = SQLQuery("CREATE TABLE " + tableName + " (" + _columns + ");");
		}

		//Creates an entry of either result or baseline(replaces UploadData from old system)
		public IEnumerator AddEntry(ResultsIORow data, string[] fields, string tableName, int baseline){
			Console.Instance.Write (DebugLevel.File, MessageLevel.Log, "Starting SQL processing"); // Write to console

			StringBuilder outputString = new StringBuilder();
			outputString.Append (tableName);

			CreateTable (tableName, fields);

			if(baseline == 1){//baseline sorting
				outputString.Insert (0, "INSERT INTO ");//using the insert function
				outputString.Append (" VALUES (" + ConvertToValues (data.resultsColumn) + ");");//fill it out with straight values

				/*string comparisonString = " WHERE " +
					"Platform='" + data.resultsColumn[5] +
					"' AND API='" + data.resultsColumn[6] + 
					"' AND RenderPipe='" + data.resultsColumn[7] + 
					"' AND GroupName='" + data.resultsColumn[8] + 
					"' AND TestName='" + data.resultsColumn[9] + 
					"';";//the condition to match

				outputString.Insert (0, "UPDATE ");//using the update function
				outputString.Append (" SET " + ConvertToValues (data.resultsColumn, fields));//fill it out with the values and column names
				outputString.Append (comparisonString);//the condition to match
*/
			}else{//result sorting
				outputString.Insert (0, "INSERT INTO ");//using the insert function
				outputString.Append (" VALUES (" + ConvertToValues (data.resultsColumn) + ");");//fill it out with straight values
			}
			SQLQuery (outputString.ToString ());

			yield return new WaitForEndOfFrame ();
		}

		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		// Utilities - TODO wip
		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		//Method to check for valid connection, Invoked from start
		void CheckConnection(){
			netStat = Application.internetReachability; //Get network state
			if (netStat != NetworkReachability.ReachableViaLocalAreaNetwork && liveConnection == true){
				Console.Instance.Write (DebugLevel.Key, MessageLevel.LogError, "Internet Connection Lost");
				liveConnection = false; // connection is not available
			}
			else if (liveConnection == false){
				Console.Instance.Write (DebugLevel.Key, MessageLevel.LogWarning, "Internet Connection Live");
				liveConnection = true;// connection is available
				OpenConnection (_connection);
			}
		}

		//create column list for table creation, inclued data type
		string CreateColumns(string[] columns){
			StringBuilder sb = new StringBuilder ();
			for(int i = 0; i < columns.Length; i++){
				string dataType = " varchar(255)";
				if(columns[i].Length > 512)
					dataType = " nvarchar(MAX)";
				sb.Append (columns[i] + dataType);
				if (i != columns.Length - 1)
					sb.Append (',');
			}
			return sb.ToString ();
		}

		//create column list for insertion values
		string ConvertToValues(List<string> values){
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < values.Count; i++) {
				sb.Append ("'" + values[i] + "'");
				if (i != values.Count - 1)
					sb.Append (',');
			}
			return sb.ToString ();
		}

		//create column list for update values
		string ConvertToValues(List<string> values, string[] fields){
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < values.Count; i++) {
				sb.Append (fields[i] + "='" + values[i] + "' ");
				if (i != values.Count - 1)
					sb.Append (',');
			}
			return sb.ToString ();
		}

	}
}
