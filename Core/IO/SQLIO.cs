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
		                            @"database=UTF_testbed;" +
									@"timeout=600;" +
		                            @"pooling=true";
		private SqlConnection _connection = null;

		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		//LOCAL VARIABLES
		public connectionStatus liveConnection;
		private NetworkReachability netStat = NetworkReachability.NotReachable;

		//Query retry list - TODO - not hooked up or solved yet
		private List<QueryBackup> SQLNonQueryBackup = new List<QueryBackup>();

		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		//INFORMATION
		private SystemData sysData;//local version of systemData

		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		//Base Methods
		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		void Start(){
			System.Net.ServicePointManager.Expect100Continue = false;
			InvokeRepeating ("CheckConnection", 0f, 5f); //Invoke network check
		}

		//Init gets called from ResultsIO
		public void Init (SystemData systemData)
		{
			sysData = systemData;
			_connection = new SqlConnection (_conString);//Create a new connnection with the _consString
		}

		void OnDisable(){
			CloseConnection ();//Close SQL conneciton on disable
		}

		//Opens a connection to the SQL DB
		void OpenConnection(){
			try
			{
				_connection.Open();//open the connection
			}
			catch(SqlException _exception)
			{
				Console.Instance.Write(DebugLevel.Critical, MessageLevel.LogError, _exception.ToString());
			}
			Console.Instance.Write (DebugLevel.File, MessageLevel.Log, "SQL connection is:" + _connection.State.ToString ());//write the connection state to log
		}

		//Closes the connection to the SQL DB
		void CloseConnection(){
			try
			{
				_connection.Close ();//close the connection
			}
			catch(SqlException _exception)
			{
				Console.Instance.Write(DebugLevel.Critical, MessageLevel.LogError, _exception.ToString());
			}
			Console.Instance.Write (DebugLevel.File, MessageLevel.Log, "SQL connection is:" + _connection.State.ToString ());//write the connection state to log
		}

		//Resets the connection, opens it if its closed or cycles it if its already open
		void ResetConnection(){
			if (_connection.State == ConnectionState.Open) {
				_connection.Close ();
				OpenConnection ();
			} else
				OpenConnection ();
		}

		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		// Query methods - TODO wip
		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		//simple test query, no return
		public string SQLQuery ( string _query ) {
			Console.Instance.Write (DebugLevel.File, MessageLevel.Log, "Sending SQL Query of size:" + _query.Length);
			using (SqlCommand cmd = new SqlCommand(_query, _connection)) {
				ResetConnection ();
				try {
					cmd.CommandTimeout = 0;
					string _returnQuery = (string) cmd.ExecuteScalar ();
					cmd.Dispose ();
					return _returnQuery;
				}
				catch (SqlException _exception) {
					Console.Instance.Write (DebugLevel.Critical, MessageLevel.LogError, _exception.ToString ());//write error
					cmd.Dispose ();
					return null;
				}
			}
		}

		public int SQLNonQuery(string _input){
			//-----------------------------------------------------------------
			//For Migration only - TODO
			//LocalIO.Instance.LargeFileWrite (_input, "query");
			//-----------------------------------------------------------------
			Console.Instance.Write (DebugLevel.File, MessageLevel.Log, "Sending SQL NonQuery of size:" + _input.Length);
			using (SqlCommand cmd = new SqlCommand (_input, _connection)) {
				float t = Time.realtimeSinceStartup;
				ResetConnection ();
				try {
					cmd.CommandTimeout = 0;
					int _rowsChanged = cmd.ExecuteNonQuery ();
					cmd.Dispose ();
					return _rowsChanged;//return the amount of rows(entried) affected
				}
				catch (SqlException _exception) {
					Console.Instance.Write (DebugLevel.Critical, MessageLevel.LogError, _exception.ToString ());//write error
					cmd.Dispose ();
					return -1;//return -1 as this is generally row changed, -1 will represent a failure and then can be handled
				}
			}
		}

		public SqlDataReader SQLRequest(string _query){
			Console.Instance.Write (DebugLevel.File, MessageLevel.Log, "SQL Request Query Out:" + _query);
			SqlDataReader myReader = null;
			SqlCommand    myCommand = new SqlCommand(_query, _connection);
			ResetConnection ();
			try
			{
				myReader = myCommand.ExecuteReader();
				return myReader;
			}
			catch (SqlException _exception)
			{
				Console.Instance.Write (DebugLevel.Critical, MessageLevel.LogError, _exception.ToString ());//write error
				return null;
			}
		}

		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		// Query data
		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		//Gets the timestamp in DateTime format from the server
		public DateTime GetbaselineTimestamp(string suiteName){
			DateTime timestamp = DateTime.MinValue;//make date time min, we check this on the other end since it is not nullable
			SqlDataReader reader = SQLRequest ("SELECT * FROM SuiteBaselineTimestamps WHERE api='" + sysData.API + "' AND suiteName='" + suiteName + "' AND platform='" + sysData.Platform + "';");
			//string t = "";
			while(reader.Read ()){
				timestamp = System.DateTime.Parse (reader.GetDateTime (3).ToString());
				//timestamp = reader.GetDateTime (3);
			}
			//timestamp = System.DateTime.Parse (t);//, new System.Globalization.CultureInfo("en-US", false));
			reader.Close ();//close the reader after getting hte information
			return timestamp;
		}

		//fetch the server side baselines
		public ResultsIOData[] FetchBaselines(string[] suiteNames, string platform, string api){
			List<ResultsIOData> data = new List<ResultsIOData> ();
			List<string> tables = new List<string>();
			//Get the table names to pull baselines from
			foreach(string suite in suiteNames){
				SqlDataReader reader = SQLRequest (String.Format ("SELECT name FROM sys.tables WHERE name LIKE '{0}%Baseline'", suite));//select any tables with the suite name in it
				while(reader.Read ()){
					tables.Add (reader.GetString (0));//add the table name to the list to pull from
				}reader.Close ();
			}
			int n = 0;
			foreach(string table in tables){
				string suite = table.Substring (0, table.IndexOf ("_"));//grab the suite from the table name
				string testType = table.Substring (table.IndexOf ("_") + 1, table.LastIndexOf ("_") - (suite.Length + 1));//grab the test type from the table name
				data.Add (new ResultsIOData());
				data [n].suite = suite;
				data [n].testType = testType;
				//This line controls how baselines are selected, right now only Platform and API are unique
				SqlDataReader reader = SQLRequest (String.Format ("SELECT * FROM {0} WHERE platform='{1}' AND api='{2}'", table, platform, api));//Select the entries that match both platform and API
				while(reader.Read ()){
					ResultsIORow row = new ResultsIORow();
					for(int i = 0; i < reader.FieldCount; i++){
						if(data[n].fieldNames.Count != reader.FieldCount)//Only when processing the first row we collect column names until we have enough that match the count
							data[n].fieldNames.Add (reader.GetName (i));//gets the names of the column
						row.resultsColumn.Add (reader.GetValue (i).ToString ());//add the value
					}
					data[n].resultsRow.Add (row);
				}
				if (data [n].fieldNames.Count == 0)
					data.RemoveAt (n);
				reader.Close ();
				n++;
			}
			return data.ToArray ();
		}

		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		// Sending data
		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		//Set the suite baseline timestamp based of given SuiteBaselineData
		public string SetSuiteTimestamp(SuiteBaselineData SBD){
			string tableName = "SuiteBaselineTimestamps";
			List<string> values = new List<string> (){ SBD.suiteName, SBD.platform, SBD.api, SBD.suiteTimestamp};
			string[] fields = new string[]{ "suiteName", "platform", "api", "suiteTimestamp"};//TODO - currently hardcoded, may be a better way, but unimportant right now
			//condition string
			string comparisonString = "platform='" + SBD.platform +
				"' AND api='" + SBD.api + 
				"' AND suiteName='" + SBD.suiteName +
				"'";//the condition to match
			//this next line formats a SQL query, this is called at the end of uploading a new baseline
			return string.Format ("IF EXISTS (select 1 from {0} WHERE {2}) BEGIN UPDATE {0} SET {3} WHERE {2} END ELSE INSERT INTO {0} VALUES ({1});", new object[]{tableName, ConvertToValues (values), comparisonString, ConvertToValues (values, fields)});
		}

		//Creates an entry of either result or baseline(replaces UploadData from old system)
		public IEnumerator AddEntry(ResultsIOData inputData, string tableName, int baseline){
			Console.Instance.Write (DebugLevel.File, MessageLevel.Log, "Starting SQL query creation"); // Write to console
			StringBuilder outputString = new StringBuilder ();
			outputString.Append ("SET TRANSACTION ISOLATION LEVEL SERIALIZABLE; BEGIN TRANSACTION; ");//using transaction and isolation to avoid double write issues
			outputString.Append (TableCheck (tableName, inputData.fieldNames.ToArray ()));//adds a table check/creation

			int rowNum = 0;//Row counter
			if (baseline == 1) {//baseline sorting
				foreach (ResultsIORow row in inputData.resultsRow) {
					rowNum++;
					//condition string
					string comparisonString = "Platform='" + row.resultsColumn [5] +
					                          "' AND API='" + row.resultsColumn [6] +
					                          "' AND RenderPipe='" + row.resultsColumn [7] +
					                          "' AND GroupName='" + row.resultsColumn [8] +
					                          "' AND TestName='" + row.resultsColumn [9] +
					                          "'";//the condition to match
				
					outputString.AppendFormat (string.Format ("UPDATE {0} SET {1} WHERE {2};", tableName, ConvertToValues (row.resultsColumn, inputData.fieldNames.ToArray ()), comparisonString));//try update a row
					outputString.Append ("IF @@ROWCOUNT = 0 BEGIN ");//if no rows were changed then....
					outputString.AppendFormat (string.Format ("INSERT INTO {0} VALUES ({1});", tableName, ConvertToValues (row.resultsColumn)));//insert a new row
					outputString.Append ("END ");

					yield return null;
				}
				foreach (SuiteBaselineData SBD in ResultsIO.Instance._suiteBaselineData) {
					outputString.Append (SetSuiteTimestamp (SBD));
				}
			} else {//result sorting
				outputString.AppendFormat ("INSERT INTO {0} VALUES ", tableName);
				int count = inputData.resultsRow.Count;
				for (int i = 0; i < count; i++) {
					rowNum++;
					outputString.AppendFormat ("({0})", ConvertToValues (inputData.resultsRow [i].resultsColumn));
					if (i < count - 1)
						outputString.Append (",");
					yield return null;
				}
			}

			outputString.Append (" COMMIT TRANSACTION");//close transaction
			int num = 0;
			num = SQLNonQuery (outputString.ToString ());//send the query
			if (num == -1) {
				QueryBackup qb = new QueryBackup();//create a new backup
				qb.type = QueryType.NonQuery;//the type is nonquery
				qb.query = outputString.ToString();//store the query
				SQLNonQueryBackup.Add (qb);//store the backup
				Debug.Log(outputString.ToString());
				Console.Instance.Write (DebugLevel.File, MessageLevel.LogError, "Failed to upload, backing up"); // Write error to console
			} else {
				Console.Instance.Write (DebugLevel.File, MessageLevel.Log, "Uploaded " + num + " row(s) successfully"); // Write to console
			}
		}

		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		// Utilities - TODO wip
		// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		//Method to check for valid connection, Invoked from start
		void CheckConnection(){
			if(_connection == null)
				_connection = new SqlConnection (_conString);
			
			if(netStat != Application.internetReachability) {
				netStat = Application.internetReachability; //Get network state

				switch (netStat) {
				case NetworkReachability.NotReachable:
					Console.Instance.Write (DebugLevel.Key, MessageLevel.LogError, "Internet Connection Lost");
					liveConnection = connectionStatus.None; // connection is not available
					break;
				case NetworkReachability.ReachableViaCarrierDataNetwork:
					Console.Instance.Write (DebugLevel.Key, MessageLevel.LogError, "Internet Connection Not Reliable, Please connect to Wi-fi");
					liveConnection = connectionStatus.Mobile; // connection is not really available
					break;
				case NetworkReachability.ReachableViaLocalAreaNetwork:
					Console.Instance.Write (DebugLevel.Key, MessageLevel.Log, "Internet Connection Live");
					liveConnection = connectionStatus.Internet; // connection is available
					break;
				}
			}

			if (liveConnection == connectionStatus.Internet && _connection.State == ConnectionState.Open)
				liveConnection = connectionStatus.Server;
			else if (liveConnection == connectionStatus.Internet && _connection.State == ConnectionState.Closed)
				liveConnection = connectionStatus.Internet;
			
		}

		//Check to see if table exists
		public string TableCheck(string tableName, string[] columns){
			string _columns = CreateColumns (columns);//gets a string formatted with data types
			return string.Format ("IF object_id('dbo.{0}') is null CREATE TABLE {0} ({1});", tableName, _columns);//query to check for table, otherwise create one
		}

		//create column list for table creation, inclued data type
		string CreateColumns(string[] columns){
			StringBuilder sb = new StringBuilder ();
			for(int i = 0; i < columns.Length; i++){
				string dataType = "varchar(255)";
				if(i < 12)
					dataType = dataTypes[i];
				else
					dataType = "varchar(MAX)";
				
				sb.Append (columns[i] + " " + dataType);
				if (i != columns.Length - 1)
					sb.Append (',');
			}
			return sb.ToString ();
		}

		//create column list for un-named values
		string ConvertToValues(List<string> values){
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < values.Count; i++) {
				sb.Append ("'" + values[i] + "'");
				if (i != values.Count - 1)
					sb.Append (',');
			}
			return sb.ToString ();
		}

		//create column list for named values
		string ConvertToValues(List<string> values, string[] fields){
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < values.Count; i++) {
				sb.Append (fields[i] + "='" + values[i] + "' ");
				if (i != values.Count - 1)
					sb.Append (',');
			}
			return sb.ToString ();
		}

		class QueryBackup{
			public QueryType type;
			public string query;
		}

		enum QueryType{ Query, NonQuery, QueryRequest};

		//SQL data types for common
		private string[] dataTypes = new string[]{"DATETIME2",//datetime
												"varchar(255)",//UnityVersion
												"varchar(10)",//AppVersion
												"varchar(255)",//OS
												"varchar(255)",//Device
												"varchar(255)",//Platform
												"varchar(50)",//API
												"varchar(128)",//RenderPipe
												"varchar(128)",//GroupName
												"varchar(128)",//TestName
												"varchar(16)",//PassFail
												"varchar(MAX)",//Custom
		};
	}
}
