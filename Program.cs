//This Program will assist you migrate the data 
//from a patient registry based database into OpenMRS using API

class Program{
	 
	 //This are the links to both the source db and the destination db
	 public static string connectionstring_pr = "server=localhost;database=patient_registry;uid=root;pwd=;default command timeout=0;convert zero datetime=True;SSL Mode =None";
     public static string connectionstring_kenya_emr = "server=localhost;database=openmrs;uid=idall;pwd=;default command timeout=0;convert zero datetime=True;SSL Mode =None";
	 
	 public static string credentials = "";//Holds the credentials in a base64 representation
	 public static string conn_kemr_api = "http://192.168.2.185:8080/openmrs/ws/rest/v1/"; //The link to access the openmrs APIs
	 public static string session_id = null; //Holds the session id that is used to authorise access to the API
	 public static string user_uuid = null; //Holds the authenticated user uuid
	 public static string loc_uuid = null; //Holds the location of the clinic useful when moving visit data 
	 public static string openmrs_id_uuid = null; 
	 public static string openmrs_pid_uuid = null;
	 public static string openmrs_swop_enrol_uuid = null;
	 public static string openmrs_encntr_swp_enrol_uuid = null;
	 public static string openmrs_encntr_reg_uuid = null; //Holds the specific encounter uuid used for registration
	 public static string openmrs_encntr_hts_uuid = null; //Holds the specific encounter uuid for HTS
	 public static string openmrs_id_conn = "http://192.168.1.185:8080/openmrs/module/idgen/generateIdentifier.form?source=1&username=admin&password=Admin123";
	
	 //This are the objects that are used 
	 public static string encounter_reg_temp = "{\r\n    \"name\": \"Discharge\",\r\n    \"description\": \"Attach encounters related to hospital dischargers\"\r\n}";
	 public static string patient_reg_temp = "{\r\n   \"identifiers\":[\r\n      {\r\n         \"identifier\":\"103VWY7\",\r\n         \"identifierType\":\"71075074-f02e-4270-89a3-f2dcda436f70\",\r\n         \"location\":\"9356400c-a5a2-4532-8f2b-2361b3446eb8\",\r\n         \"preferred\":false\r\n      },\r\n      {\r\n         \"identifier\":\"\",\r\n         \"identifierType\":\"\",\r\n         \"location\":\"\",\r\n         \"preferred\":false\r\n      }\r\n   ],\r\n   \"person\":{\r\n      \"gender\":\"M\",\r\n      \"age\":47,\r\n      \"birthdate\":\"1970-01-01T00:00:00.000+0100\",\r\n      \"birthdateEstimated\":false,\r\n      \"dead\":false,\r\n      \"deathDate\":null,\r\n      \"causeOfDeath\":null,\r\n      \"names\":[\r\n         {\r\n            \"givenName\":\"\",\r\n            \"familyName\":\"\"\r\n         }\r\n      ],\r\n      \"addresses\": [\r\n        {\r\n        \"address1\": \"\",\r\n        \"cityVillage\": \"\",\r\n        \"country\": \"\",\r\n        \"postalCode\": \"\"\r\n        }\r\n      ],\r\n      \r\n    }\r\n}";
	 public static string visit_reg_temp = "{\r\n    \"patient\": \"\",\r\n    \"visitType\": \"3371a4d4-f66f-4454-a86d-92c7b3da990c\",\r\n    \"startDatetime\": \"\",\r\n    \"location\": \"\",\r\n    \"indication\": null\r\n}";
	 public static string visit_encounters_temp = "{\r\n   \"encounters\":[\r\n      \"ed40c9f1-e548-4e4c-823b-193f8d6ba73f\"\r\n   ]\r\n}";
	 public static string person_attributes_temp = "{\"attributeType\": \"\",\"value\": \"\"}";
	 public static string encounter_reg_2 = "{\r\n  \"encounterDatetime\": \"2023-01-20T12:42:25.000Z\",\r\n  \"patient\": \"b5b7dbf8-88f8-427c-a2e5-55605e26dda7\",\r\n  \"encounterType\": \"ea68aad6-4655-4dc5-80f2-780e33055a9e\",\r\n  \"location\": \"e629d11c-cb0d-4f6d-a98c-99f8ca3e080a\",\r\n  \"encounterProviders\": [\r\n    {\r\n      \"provider\": \"48b55692-e061-4ffa-b1f2-fd4aaf506224\",\r\n      \"encounterRole\": \"a0b03050-c99b-11e0-9572-0800200c9a66\"\r\n    }\r\n  ],\r\n  \"visit\":\"2c5d6c83-1aef-4bd6-b128-368811dbceb3\"\r\n}";

	 public static string cutoff_date = null;

	 public static DataTable tblVisits = new DataTable();

	 public static List<string> visits = new List<string>();
	 public static List<string> visits_uuids = new List<string>();
	 
	 static void Main(string[] args)
	 {
		    Console.WriteLine("Welcome to the Patient Registry KEMR Migration Tool");
			Console.WriteLine("");

			//Get the cut-off date
			Console.WriteLine("The the cut-off date for the migration process in the format of yyyy-MM-dd");
			cutoff_date = Console.ReadLine();
			
			 if (!string.IsNullOrWhiteSpace(cutoff_date))
			 {
				 if (DateTime.TryParse(cutoff_date, out _))
				 {
					 getVisits(cutoff_date);
				 }
			 }
			 else
			 {
				 Console.WriteLine("Invalid cutt-off date given\n\n");
				 Console.WriteLine("Resulting to defaults - All clients seen in the last 3 years");
				 DateTime dateTime = DateTime.Now.AddYears(-3);
				 getVisits(dateTime.ToString("yyyy-MM-dd"));
			 }
			 
			 
	 }
	 
	
	void getVisits(string cutoff_dte)
	{
		Console.WriteLine("Migration cutoff date set to " + Convert.ToDateTime(cutoff_dte).ToString("D") + "\n\n");

		if (!dictionary.ContainsKey("pr_ip"))
		{
			connServers();
		}
		else
		{
			connPR(dictionary["pr_ip"]);
			connKEMR();
		}




	}
	
	 void connServers()
	 {
		 Console.WriteLine("Enter the IP of the computer hosting the patient registry\n");
		 string? pr_ip = Console.ReadLine();

		 if (!string.IsNullOrWhiteSpace(pr_ip))
		 {
			 if (validIP(pr_ip))
			 {
				 if (pingIP(pr_ip))
				 {
					 Console.WriteLine("Patient Registry IP captured successfully\n\n");

					 connPR(pr_ip);

					 connKEMR();
				 }
			 }
			 else
			 {
				 Console.WriteLine("The Patient Registry IP " + pr_ip + " is not a valid IP4 address\n");
				 connServers();// Loop until a valid IP is given
			 }
		 }
		 else
		 {
			 Console.WriteLine("Invalid Patient Registry IP given, Process Aborted\n");
		 }
	 }
	 
	void connPR(string pr_ip)
	{
		connectionstring_pr = "server=" + pr_ip + ";database=patient_registry;uid=newuser;pwd=;default command timeout=0;convert zero datetime=True;SSL Mode=None";

		MySqlConnection mysql_conn = new MySqlConnection(connectionstring_pr);

		try
		{
			try
			{
				mysql_conn.Open();
			}
			catch
			{
				connectionstring_pr = "server=" + pr_ip + ";database=patient_registry;uid=newuser;pwd=;default command timeout=0;convert zero datetime=True;SSL Mode=None";

				mysql_conn = new MySqlConnection(connectionstring_pr);
			}

			if (mysql_conn.State == ConnectionState.Open)
			{
				string clinic_name = getClinicName(mysql_conn);

				Console.WriteLine("Connected to Patient Registry\n");
				Console.WriteLine("Working From " + clinic_name);

				dictionary["pr_ip"] = pr_ip;
			}
			else
			{
				Console.WriteLine("Unable to connect to Patient Registry using IP " + pr_ip + "\n\n");
			}
		}
		catch (MySqlException err)
		{
			Console.WriteLine("Error on Patient Registry Connect " + err.Message + "\n" + err.StackTrace);
		}
		finally
		{
			if (mysql_conn.State == ConnectionState.Open) mysql_conn.Close();
		}
	}
	
	string getClinicName(MySqlConnection mysql_conn)
	{
		try
		{
			string query = "SELECT * FROM clinic_info";

			MySqlCommand mysql_cmd = new MySqlCommand(query, mysql_conn);

			MySqlDataReader mysql_datareader = mysql_cmd.ExecuteReader();

			if (mysql_datareader.HasRows)
			{
				while (mysql_datareader.Read())
				{
					return mysql_datareader["Clinic_Name"].ToString();
				}
			}
		}
		catch (MySqlException err)
		{
			Console.WriteLine("Unable to locate Clinic Name from PR " + err.Message + "\n" + err.StackTrace);
		}

		return "";
	}
	
	static void connKEMR()
	{
		string? pr_ip = null;

		if (dictionary.ContainsKey("kemr_ip"))
		{
			pr_ip = dictionary["kemr_ip"];
		}

		if (pr_ip == null)
		{
			Console.WriteLine("Enter the IP of the computer hosting KEMR\n");
			pr_ip = Console.ReadLine();

			if (!string.IsNullOrWhiteSpace(pr_ip))
			{
				if (validIP(pr_ip))
				{
					if (pingIP(pr_ip))
					{
						connectionstring_kenya_emr = "server=" + pr_ip + ";database=openmrs;uid=idall;pwd=idall2021!;default command timeout=0;convert zero datetime=True;SSL Mode =None";

						openmrs_id_conn = $"http://{pr_ip}:8080/openmrs/module/idgen/generateIdentifier.form?source=1&username=admin&password=Admin123";

						Console.WriteLine("KEMR IP captured successfully\n\n");

						MySqlConnection mysql_conn = new MySqlConnection(connectionstring_kenya_emr);

						try
						{
							mysql_conn.Open();

							Console.WriteLine("Connected to KEMR successfully");

							dictionary["kemr_ip"] = pr_ip;

							configKEMR(pr_ip);

							getVisitsPR();

							regClientsPRtoKEMR();
						}
						catch (MySqlException err)
						{
							Console.WriteLine("Unable to connect to KEMR " + err.Message + "\n" + err.StackTrace);
						}
						finally
						{
							if (mysql_conn.State == ConnectionState.Open)
							{
								mysql_conn.Close();
							}
						}

					}
				}
				else
				{
					Console.WriteLine("The KEMR IP " + pr_ip + " is not a valid IP4 address\n");
					connKEMR();// Loop until a valid IP is given
				}
			}
			else
			{
				connectionstring_kenya_emr = "server=" + pr_ip + ";database=openmrs;uid=idall;pwd=idall2021!;default command timeout=0;convert zero datetime=True;SSL Mode =None";

				Console.WriteLine("KEMR IP captured successfully\n\n");

				MySqlConnection mysql_conn = new MySqlConnection(connectionstring_kenya_emr);

				try
				{
					mysql_conn.Open();

					Console.WriteLine("Connected to KEMR successfully");

					//dictionary["kemr_ip"] = pr_ip;
				}
				catch (MySqlException err)
				{
					Console.WriteLine("Unable to connect to KEMR " + err.Message + "\n" + err.StackTrace);
				}
				finally
				{
					if (mysql_conn.State == ConnectionState.Open)
					{
						mysql_conn.Close();
					}
				}
			}
		}
		else
		{
			Console.WriteLine("Invalid KEMR IP given, Process Aborted\n");
		}
	}
	
	private static void regClientsPRtoKEMR()
	{
	   Console.WriteLine("Registering Clients in PR into KEMR\n");

		int totalTicks = tblVisits.Rows.Count;

		var options = new ProgressBarOptions
		{
			ProgressCharacter = '─',
			ProgressBarOnBottom = true
		};

		int cnt = 0;

		using (var pbar = new ProgressBar(totalTicks, "Uploading Clients", options))
		{
			foreach (DataRow row in tblVisits.Rows)
			{
				//Get client enroll id
				string eid = getEnrollID(row["Clinic_ID"]);
				if(!existsinKEMR(eid))
				{
					//Get client biodata
					DataTable? tbl_biodata = getBiodata(row["Clinic_ID"]);
					if (tbl_biodata != null)
					{
						//Prep data for upload
						uploadClienttoKEMR(eid, row["Clinic_ID"].ToString(), tbl_biodata);
					}
					else
					{
						Console.WriteLine("Unable to extract client " + row["Clinic_ID"] + " biodata\n");
					}
					//Console.WriteLine("Client " + eid + " Does not Exist in KEMR and will be registered\n");
				}
				//pushvisitKEMR(row);
				pbar.Tick(cnt, " Done");
				cnt++;
			}
		}
	}
	
	private static void uploadClienttoKEMR(string eid, string? cid, DataTable tbl_biodata)
	{
		foreach (DataRow item in tbl_biodata.Rows)
		{
			dynamic dynObj = JsonConvert.DeserializeObject(patient_reg_temp);

			try
			{
				dynObj.identifiers[0].identifier = getOpenMRSID();
				dynObj.identifiers[0].identifierType = openmrs_id_uuid;
				dynObj.identifiers[0].location = loc_uuid;
				dynObj.identifiers[1].identifier = eid;
				dynObj.identifiers[1].identifierType = openmrs_pid_uuid;
				dynObj.identifiers[1].location = loc_uuid;

				dynObj.person.gender = item["Patient_Gender"];
				dynObj.person.age = getAge(item["dob"]);

				try
				{
					dynObj.person.birthdate = Convert.ToDateTime(item["dob"]).ToString("s") + ".000" + Convert.ToDateTime(item["dob"]).ToString("zzz").Replace(":", "");
				}
				catch { }

				dynObj.person.names[0].givenName = item["Patient_FName"] + " " + item["Patient_MName"];
				dynObj.person.names[0].familyName = item["Patient_LName"];

				using (WebClient client = new WebClient())
				{
					client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

					client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

					client.Headers.Add(HttpRequestHeader.Authorization, string.Format("Basic {0}", credentials));

					client.Headers.Add(HttpRequestHeader.Cookie, string.Format("JSESSIONID={0}", session_id));

					string pay_load = Convert.ToString(dynObj);

					string rslt = client.UploadString(conn_kemr_api + "patient", "POST", pay_load);

					Debug.WriteLine(rslt, "OpenMRS Patient Reg Result");

					dynamic dynObj2 = JsonConvert.DeserializeObject(rslt);

					string uuid = dynObj2.uuid;

					try
					{
						//Register Client Tel
						if (!string.IsNullOrEmpty(item["Patient_Phone"].ToString()))
						{
							regAttributes("b2c38640-2603-4629-aebd-3b54f33f1e3a", uuid, item["Patient_Phone"].ToString());
						}
					}
					catch { }

					try
					{
						//Register App resposible for Reg
						regAttributes("ac9a19f2-88af-4f3b-b4c2-f6e57c0d89af", uuid, "IDALL_Migrator");
					}
					catch { }

					Console.WriteLine($"Client {eid} registered in KEMR Succefully\n");

					//Register client corresponding visits
					regClientVisitsinKEMR(uuid, cid);

				}
			}
			catch(Exception err) 
			{
				Console.WriteLine("Error on Pushing Client to KEMR "+err.Message+"\n"+err.StackTrace+"\n");
			}
			
		}
	}
	
	 private static void regClientVisitsinKEMR(string uuid, string? cid)
	 {
		 //Get client visits since cutoff date
		 DataTable tblClientVisits = getClientSpecVisits(cid);
		 DataTable tblVisitDates = getClientVisitDates(cid); 

		 if (tblClientVisits != null )
		 {
			 foreach (DataRow dr in tblVisitDates.Rows)
			 {
				 
				 dynamic enctrObj = JsonConvert.DeserializeObject(Resource1.encounter_temp_2);

				 using (WebClient client = new WebClient())
				 {
					 dynamic dynObj = JsonConvert.DeserializeObject(visit_reg_temp);

					 dynObj.patient = uuid;

					 dynObj.startDatetime = Convert.ToDateTime(dr["Visit_Date"]).ToString("s", System.Globalization.CultureInfo.InvariantCulture) + ".000Z";

					 dynObj.location = loc_uuid;
											
					 string pl = JsonConvert.SerializeObject(dynObj);

					 Console.WriteLine(pl);

					 client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

					 client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

					 client.Headers.Add(HttpRequestHeader.Authorization, string.Format("Basic {0}", credentials));

					 client.Headers.Add(HttpRequestHeader.Cookie, string.Format("JSESSIONID={0}", session_id));

					 string rslt = client.UploadString(conn_kemr_api + "visit", "POST", pl);

					 dynamic dynObj2 = JsonConvert.DeserializeObject(rslt);

					 //Console.WriteLine("Result of Visit Generation - "+Convert.ToString(dynObj2));
					 for (int i = 0; i < tblClientVisits.Rows.Count; i++)
					 {
						 if (dr["Visit_Date"].Equals(tblClientVisits.Rows[i]["Visit_Date"]))
						 {
							 regVisitEncounters(uuid, (string)dynObj2.uuid, enctrObj, tblClientVisits.Rows[i]["Visit_Date"], tblClientVisits.Rows[i]["Visit_Type"].ToString());
						 }
					 }

				 }
			 }
		 }
	 }
	 
	private static void regVisitEncounters(string p_uuid,string uuid, dynamic enctrObj, object v_dte, string v_typ)
	{

		try
		{

			//Resolve each visit with corresponding uuid from uploaded visits earlier
			int indx = visits.IndexOf(v_typ);

			if (indx != -1)
			{
				enctrObj.encounterDatetime = Convert.ToDateTime(v_dte).ToString("s", System.Globalization.CultureInfo.InvariantCulture) + ".000Z";

				enctrObj.patient = p_uuid;

				enctrObj.location = loc_uuid;

				enctrObj.visit = uuid;

				enctrObj.encounterType = visits_uuids[indx];

				using (WebClient client = new WebClient())
				{
					string pl = JsonConvert.SerializeObject(enctrObj);

					client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

					client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

					client.Headers.Add(HttpRequestHeader.Authorization, string.Format("Basic {0}", credentials));

					client.Headers.Add(HttpRequestHeader.Cookie, string.Format("JSESSIONID={0}", session_id));

					try
					{
						string rslt = client.UploadString(conn_kemr_api + "encounter", "POST", pl);

						dynamic dynObj2 = JsonConvert.DeserializeObject(rslt);

						//Console.WriteLine("Result of Visit Enrollment Attachment - " + Convert.ToString(dynObj2));
					}
					catch { }
				}
			}
		}
		catch { }

		
	}
	
	 private static DataTable getClientSpecVisits(string? cid)
	 {
		 MySqlConnection mysql_conn = new MySqlConnection(connectionstring_pr);

		 try
		 {
			 mysql_conn.Open();

			 string query = "SELECT * FROM patient_visits WHERE Clinic_ID = @cid ";

			 MySqlCommand mysql_cmd = new MySqlCommand(query, mysql_conn);

			 mysql_cmd.Parameters.AddWithValue("@cid", cid);
			//mysql_cmd.Parameters.AddWithValue("@cutoff", cutoff_date);

			 MySqlDataReader dataReader = mysql_cmd.ExecuteReader();

			 DataTable tblRslt = new DataTable();

			 tblRslt.Load(dataReader);

			 return tblRslt;

		 }
		 catch(MySqlException err)
		 {
			 Console.WriteLine($"Unable to get client {cid} visits "+err.Message+"\n"+err.StackTrace+"\n");
		 }
		 return null;
	 }
	 
	private static DataTable getClientVisitDates(string? cid)
	{
		MySqlConnection mysql_conn = new MySqlConnection(connectionstring_pr);

		try
		{
			mysql_conn.Open();

			string query = "SELECT Visit_Date FROM patient_visits WHERE Clinic_ID = @cid GROUP BY Visit_Date";

			MySqlCommand mysql_cmd = new MySqlCommand(query, mysql_conn);

			mysql_cmd.Parameters.AddWithValue("@cid", cid);
			//mysql_cmd.Parameters.AddWithValue("@cutoff", cutoff_date);

			MySqlDataReader dataReader = mysql_cmd.ExecuteReader();

			DataTable tblRslt = new DataTable();

			tblRslt.Load(dataReader);

			return tblRslt;

		}
		catch (MySqlException err)
		{
			Console.WriteLine($"Unable to get client {cid} visits " + err.Message + "\n" + err.StackTrace + "\n");
		}
		return null;
	}
	
	private static dynamic getAge(object v)
	{
		DateTime dob = DateTime.Now;

		DateTime.TryParse(v.ToString(), out dob);

		var age = DateTime.Now.Year - dob.Year;

		if (dob.Date > DateTime.Now.AddYears(-age)) age--;

		return age >= 100 ? 70 : age;
	}
	
	private static void regAttributes(string key, string uuid, string val)
	{
		using (WebClient client = new WebClient())
		{
		   
			dynamic dynObj = JsonConvert.DeserializeObject(person_attributes_temp);

			dynObj.attributeType = key;

			dynObj.value = val;

			string pay_load = JsonConvert.SerializeObject(dynObj);

			client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

			client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

			client.Headers.Add(HttpRequestHeader.Authorization, string.Format("Basic {0}", credentials));

			client.Headers.Add(HttpRequestHeader.Cookie, string.Format("JSESSIONID={0}", session_id));

			string rslt = client.UploadString(conn_kemr_api + "person/" + uuid + "/attribute", "POST", pay_load);

			Debug.WriteLine("OpenMRS Attribute Result", rslt);

			dynamic dynObj2 = JsonConvert.DeserializeObject(rslt);

			if (Convert.ToString(dynObj2.display).Contains(val))
			{
				Console.WriteLine($"{val} attribute sync with Kenya EMR Successful\n");
									
			}

		}
	}
	
	private static dynamic getOpenMRSID()
	{
		using (WebClient client = new WebClient())
		{
			try
			{
				client.Headers[HttpRequestHeader.Authorization] = string.Format(
					"Basic {0}", credentials);

				client.Headers.Add(HttpRequestHeader.Cookie, string.Format("JSESSIONID={0}", session_id));

				string rslt = client.DownloadString(openmrs_id_conn);

				Debug.WriteLine("OpenMRS ID Result", rslt);

				dynamic dynObj = JsonConvert.DeserializeObject(rslt);

				return dynObj.identifiers[0];
			}
			catch (Exception err) 
			{
				Console.WriteLine("Error on Generating OpenMRS " + err.Message + "\n" + err.StackTrace);
			}

			return null;

		}


	}
	
	private static DataTable? getBiodata(object v)
	{
	   MySqlConnection mysql_conn = new MySqlConnection(connectionstring_pr);
		try
		{
			mysql_conn.Open();

			string query = "SELECT Patient_FName,Patient_MName,Patient_LName,Patient_Gender,CONCAT(Patient_YOB,'-',Patient_MOB,'-',Patient_DOB) AS dob,\r\nPatient_Phone FROM patient_entry WHERE Clinic_ID = @cid";

			MySqlCommand mysql_cmd = new MySqlCommand(query, mysql_conn);
			mysql_cmd.Parameters.AddWithValue("@cid", v.ToString());

			MySqlDataReader dataReader =  mysql_cmd.ExecuteReader();

			if (dataReader.HasRows)
			{
				DataTable tblRslt = new DataTable();
				tblRslt.Load(dataReader);
				return tblRslt;
			}
		}
		catch (MySqlException err) 
		{
			Console.WriteLine("Unable to get client "+v+" biodata "+err.Message+"\n"+err.StackTrace);
		}
		finally { if(mysql_conn.State == ConnectionState.Open) mysql_conn.Close(); }

		return null;
	}
	
	private static bool existsinKEMR(object v)
	{
		MySqlConnection mysql_conn = new MySqlConnection(connectionstring_kenya_emr);

		try
		{
			mysql_conn.Open();

			string query = "SELECT identifier FROM patient_identifier WHERE identifier = @id";

			MySqlCommand mysql_cmd = new MySqlCommand(query, mysql_conn);
			mysql_cmd.Parameters.AddWithValue("@id", v);

			MySqlDataReader dataReader = mysql_cmd.ExecuteReader();

			if (dataReader.HasRows)
			{
				return true;
			}
		}
		catch (MySqlException err)
		{
			Console.WriteLine("Unable to evaluate client enroll id " + err.Message + "\n" + err.StackTrace);
		}
		finally { mysql_conn.Close(); }

		return false;
	}
	
	private static string getEnrollID(object v)
	{
		MySqlConnection mysql_conn = new MySqlConnection(connectionstring_pr);

		try
		{
			mysql_conn.Open();

			string query = "SELECT Enrol_No FROM patient_info WHERE Clinic_ID = @cid";

			MySqlCommand mysql_cmd = new MySqlCommand(query, mysql_conn);
			mysql_cmd.Parameters.AddWithValue("@cid", v);

			MySqlDataReader dataReader = mysql_cmd.ExecuteReader();

			if (dataReader.HasRows)
			{
				while (dataReader.Read())
				{
					return dataReader["Enrol_No"].ToString();
				}
			}
		}
		catch (MySqlException err)
		{
			Console.WriteLine("Unable to evaluate client enroll id " + err.Message + "\n" + err.StackTrace);
		}
		finally { mysql_conn.Close(); }

		return null;
	}
	
	public class CookieAwareWebClient : WebClient
	{
		public CookieAwareWebClient()
		{
			CookieContainer = new CookieContainer();
			this.ResponseCookies = new CookieCollection();
		}

		public CookieContainer CookieContainer { get; private set; }
		public CookieCollection ResponseCookies { get; set; }

		protected override WebRequest GetWebRequest(Uri address)
		{
			var request = (HttpWebRequest)base.GetWebRequest(address);
			request.CookieContainer = CookieContainer;
			return request;
		}

		protected override WebResponse GetWebResponse(WebRequest request)
		{
			var response = (HttpWebResponse)base.GetWebResponse(request);
			this.ResponseCookies = response.Cookies;
			return response;
		}
	}
	
	 static void configKEMR(string pr_ip)
	 {

		 conn_kemr_api = "http://" + pr_ip + ":8080/openmrs/ws/rest/v1/";

		 credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("admin:Admin123"));

		 connKEMRAPI();

	 }

	 static void connKEMRAPI()
	 {
		 //Check if IDALL user credentials are available
		 if (getConnKEMRCreds())
		 {
			 getOpenMRSDefaultLocation();

			 getOpenMRSIdentifierUUIDs();

			 //getOpenMRSEncounterUUIDS();

			 Console.WriteLine("Connected to OpenMRS API\n");


		 }
		 else
		 {
			 Console.WriteLine("Unable to Connect to OpenMRS API\n");
		 }
	 }
	 
	private static void getOpenMRSIdentifierUUIDs()
	{
		MySqlConnection mysql_conn = new MySqlConnection(connectionstring_kenya_emr);

		try
		{

			string query = "SELECT `uuid` FROM patient_identifier_type " +
				"WHERE `name` = 'OpenMRS ID' OR `name` = 'Patient Clinic Number' " +
				"OR `name` = 'SWOP Enrollment Number' ";

			mysql_conn.Open();

			MySqlCommand mysql_cmd = new MySqlCommand(query, mysql_conn);

			MySqlDataReader mysql_datareader = mysql_cmd.ExecuteReader();

			if (mysql_datareader.HasRows)
			{
				while (mysql_datareader.Read())
				{
					if (openmrs_id_uuid == null)
					{
						openmrs_id_uuid = mysql_datareader["uuid"].ToString();

					}
					else if (openmrs_pid_uuid == null)
					{
						openmrs_pid_uuid = mysql_datareader["uuid"].ToString();
					}

				}
			}
		}
		catch (MySqlException err)
		{
			Debug.WriteLine(err.Message, "Identifer Load Error");
		}
		finally
		{
			mysql_conn.Close();
		}
	}
	
	private static void getOpenMRSDefaultLocation()
	{
		loc_uuid = getOpenMRSFacLoc(""); //Name of the clinic you need to attach to the data being migrated
	}

	private static string getOpenMRSFacLoc(string fac)
	{
		MySqlConnection mysql_conn = new MySqlConnection(connectionstring_kenya_emr);

		try
		{
			mysql_conn.Open();

			string query = "SELECT `uuid` FROM location WHERE `name` = @fac";

			MySqlCommand mysql_cmd = new MySqlCommand(query, mysql_conn);

			mysql_cmd.Parameters.AddWithValue("@fac", fac);

			MySqlDataReader mysql_datareader = mysql_cmd.ExecuteReader();

			if (mysql_datareader.HasRows)
			{
				while (mysql_datareader.Read())
				{
					return mysql_datareader["uuid"].ToString();
				}
			}
		}
		catch (Exception err)
		{
			Debug.WriteLine(err.Message, "Error on Facility Location Pickup");
			Debug.WriteLine(err.StackTrace, "Error on Facility Location Pickup");
		}
		finally { mysql_conn.Close(); }

		return null;
	}

	private static bool getConnKEMRCreds()
	{
		try
		{
			using (CookieAwareWebClient client = new CookieAwareWebClient())
			{
				client.Headers[HttpRequestHeader.Authorization] = string.Format(
				 "Basic {0}", credentials);

				string rslt = client.DownloadString(conn_kemr_api + "session");

				Debug.WriteLine("Synch Client Data KEMR Result - upload data", rslt);

				dynamic dynObj = JsonConvert.DeserializeObject(rslt);

				if (dynObj.authenticated == "true")
				{
					string cookie_rslt = client.ResponseCookies["JSESSIONID"].ToString().Replace("JSESSIONID=", "");

					session_id = dynObj.sessionId == null ? cookie_rslt : dynObj.sessionId;

					user_uuid = dynObj.user.uuid;

					return true;



				}
				else if (dynObj.authenticated == "false")
				{
					Console.WriteLine("Unable to Connect to KEMR - Authentication Failure\n");

				}
			}
		}
		catch (Exception err)
		{
			Console.WriteLine("The following error has occured " + err.Message + "\n" + err.StackTrace);

		}

		return false;
	}
	
	static void getVisitsPR()
	{
		MySqlConnection mysql_conn = new MySqlConnection(connectionstring_pr);
		try
		{
			string query = "SELECT * FROM patient_visits WHERE Visit_Date >= @v_dte GROUP BY Visit_Type";

			mysql_conn.Open();

			MySqlCommand mysql_cmd = new MySqlCommand(query, mysql_conn);

			mysql_cmd.Parameters.AddWithValue("@v_dte", cutoff_date);

			tblVisits.Load(mysql_cmd.ExecuteReader());

			if (tblVisits.Rows.Count > 0)
			{
				//Process Visits into KEMR
				visits.Clear();
				visits_uuids.Clear();

				int totalTicks = tblVisits.Rows.Count;

				var options = new ProgressBarOptions
				{
					ProgressCharacter = '─',
					ProgressBarOnBottom = true
				};

				int cnt = 0;

				using (var pbar = new ProgressBar(totalTicks, "Uploading Data", options))
				{
					foreach (DataRow row in tblVisits.Rows)
					{
						pushvisitKEMR(row);
						pbar.Tick(cnt, " Done");
						cnt++;
					}
				}

				Console.WriteLine("Patient Registry Visit Types Registered with KEMR - "+cnt+"\n");
			}

		}
		catch (MySqlException err)
		{
			Console.WriteLine("Unable to extract visits from Patient Registry " + err.Message + "\n" + err.StackTrace);
		}
		finally { if (mysql_conn.State == ConnectionState.Open) { mysql_conn.Close(); } }
	}
	
	        private static void pushvisitKEMR(DataRow row)
        {
            if (!kemrhasEncounters(row["Visit_Type"].ToString()))
            {
                dynamic? dynObj = JsonConvert.DeserializeObject(encounter_reg_temp);

                try
                {

                    dynObj.name = row["Visit_Type"].ToString();
                    dynObj.description = "Patient Registry Visit " + row["Visit_Type"].ToString();
					
                    string pay_load = Convert.ToString(dynObj);

                    using (WebClient client = new WebClient())
                    {
                        client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

                        client.Headers.Add(HttpRequestHeader.Authorization, string.Format("Basic {0}", credentials));

                        client.Headers.Add(HttpRequestHeader.Cookie, string.Format("JSESSIONID={0}", session_id));


                        string rslt = client.UploadString(conn_kemr_api + "encountertype", "POST", pay_load);

                        Debug.WriteLine(rslt, "OpenMRS Patient Reg Result");

                        dynamic dynObj2 = JsonConvert.DeserializeObject(rslt);

                        visits.Add(row["Visit_Type"].ToString());

                        visits_uuids.Add((string) dynObj2.uuid);

                    }
                }
                catch (WebException ex)
                {
                    Console.WriteLine("Error registering PR Encounter " + ex.Message + "\n" + ex.StackTrace + "\n");
                }
            }
            else
            {
                Console.WriteLine("Visit Type " + row["Visit_Type"].ToString() + " already in KEMR Encounter List");
            }
        }

        private static bool kemrhasEncounters(string? v)
        {
            MySqlConnection mysql_conn = new MySqlConnection(connectionstring_kenya_emr);

            try
            {
                mysql_conn.Open();

                string query = "SELECT uuid,name FROM encounter_type WHERE name = @nme";

                MySqlCommand mysql_cmd = new MySqlCommand(query, mysql_conn);

                mysql_cmd.Parameters.AddWithValue("@nme", v);

                MySqlDataReader dataReader = mysql_cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        visits.Add(dataReader["name"].ToString());
                        visits_uuids.Add(dataReader["uuid"].ToString());
                    }
                    return true;
                }
            }
            catch (MySqlException err)
            {
                Console.WriteLine("Error evaluating PR Visit for Encounter Insertion " + err.Message + "\n" + err.StackTrace + "\n");
            }
            finally { mysql_conn.Close(); }

            return false;
        }

        public static bool pingIP(string pr_ip)
        {
            try
            {
                Ping x = new Ping();
                PingReply reply = x.Send(IPAddress.Parse(pr_ip), 5000);
                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
            }

            catch { }


            return false;
        }

        public static bool validIP(string pr_ip)
        {
            if (pr_ip.Count(c => c == '.') != 3) return false;
            IPAddress address;
            return IPAddress.TryParse(pr_ip, out address);
        }
	 
  }
	
}
