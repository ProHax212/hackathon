using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class IoTController : MonoBehaviour {

	public string websocketConnectionString;
	[HideInInspector]
	public Dictionary<string, IoTData> iotData = new Dictionary<string, IoTData>{ };
	[HideInInspector]
	public bool isRedLightOn = false;
	[HideInInspector]
	public bool isYellowLightOn = false;
	[HideInInspector]
	public bool isGreenLightOn = false;

	public VoiceController voiceController;

	private WebSocket websocket;

	public struct IoTData{
		public string deviceName;
		public string formattedString;

		public IoTData(string deviceName, string formattedString){
			this.deviceName = deviceName;
			this.formattedString = formattedString;
		}


	}

	public struct IoTCommand {

		private IoTData data;
		private float value;
		private string type;

		public IoTCommand(string deviceName, string formattedString, float value) {
			this.data = new IoTData(deviceName, formattedString);
			this.type = "command";
			this.value = value;
		}

		public override string ToString () {
			JSONObject json = new JSONObject ();
			json.AddField ("name", this.data.deviceName);
			json.AddField (this.data.formattedString, this.value);
			json.AddField ("type", this.type);
			return json.ToString ();
		}

	}

	// Use this for initialization
	void Start () {
		if (websocketConnectionString != "") {
			websocket = new WebSocket (websocketConnectionString);
			StartCoroutine (CheckConnection ());
		}

		iotData = new Dictionary<string, IoTData> ();
		iotData ["motor"] = new IoTData ("motor", "RPM : 1000\nOutputVoltage : 50");
		iotData ["patlite"] = new IoTData ("patlite", "Light: Green\nLight: Red");
	}

	// Update is called once per frame
	void Update () {

	}

	// Continuously check if the connection is alive
	public IEnumerator CheckConnection(){
		while (true) {
			if (!websocket.IsAlive) {
				Debug.Log ("Trying to connect to IoT websocket");
				DisconnectWebsocket ();
				ConnectWebsocket ();
			}

			yield return new WaitForSeconds (5);
		}
	}

	/*
	 	"type" : "reading"
		"motor" : {"RPM" : 1000, "OutputVoltage": 50}
		"patlite" : {}
	*/

	// Connect asynchronously to the websocket
	private void ConnectWebsocket(){
		websocket = new WebSocket (websocketConnectionString);

		websocket.OnOpen += (sender, e) => {
			Debug.Log("IoT connection opened");
		};

		// Handle json data from the server
		websocket.OnMessage += (sender, e) => {
			Dictionary<string, IoTData> newIotData = new Dictionary<string, IoTData>();

			Debug.Log(e.Data);

			JSONObject json = new JSONObject(e.Data);

			// Check if readings
			if(!json.HasField("type") || json.GetField("type").str != "reading"){
				return;
			}

			foreach(string device in json.keys){
				if(device == "type"){
					continue;
				}

				JSONObject deviceJSON = json.GetField(device);

				// Update patlite info if necessary
				/*if(device == "patlite"){
					isRedLightOn = (int)deviceJSON.GetField("RedLightControlState").f > 1;
					isYellowLightOn = (int)deviceJSON.GetField("AmberLightControlState").f > 1;
					isGreenLightOn = (int)deviceJSON.GetField("GreenLightControlState").f > 1;
				}*/

				string formattedString = "";
				for(int i = 0; i < deviceJSON.keys.Count; i++){
					formattedString += deviceJSON.keys[i].ToString() + " : " + deviceJSON.GetField(deviceJSON.keys[i]).ToString();

					if(i < deviceJSON.keys.Count - 1){
						formattedString += "\n";
					}
				}

				newIotData[device] = new IoTData(device, formattedString);
			}

			iotData = newIotData;
		};

		websocket.OnError += (sender, e) => {
			Debug.Log("IoT websocket error received");
			Debug.Log(e.Exception.ToString());
		};

		websocket.ConnectAsync ();
	}

	public void SendWebsocketMessage(string MessageData) {
		if (!websocket.IsAlive) {
			Debug.Log ("IoT websocket is not open, message was not sent");
			return;
		}

		websocket.Send (MessageData);
	}

	// Disconnect the websocket connection
	private void DisconnectWebsocket(){
		websocket.Close ();
		websocket = null;
	}

}
