using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class IoTController : MonoBehaviour {

	public string websocketConnectionString;
	[HideInInspector]
	public Dictionary<string, IoTData> iotData = new Dictionary<string, IoTData>{ };

	private WebSocket websocket;

	public struct IoTData{
		public string deviceName;
		public string formattedString;

		public IoTData(string deviceName, string formattedString){
			this.deviceName = deviceName;
			this.formattedString = formattedString;
		}
	}

	// Use this for initialization
	void Start () {
		if (websocketConnectionString != "") {
			websocket = new WebSocket (websocketConnectionString);
			//StartCoroutine (CheckConnection ());
		}

		iotData = new Dictionary<string, IoTData> ();
		iotData ["motor"] = new IoTData ("motor", "RPM : 1000\nOutputVoltage : 50");
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

			JSONObject json = new JSONObject(e.Data);
			foreach(string device in json.keys){
				string formattedString = "";
				JSONObject deviceJSON = json.GetField(device);
				for(int i = 0; i < json.keys.Count; i++){
					formattedString += json.keys[i].ToString() + " : " + json.GetField(json.keys[i]).ToString();

					if(i < json.keys.Count - 1){
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

	// Disconnect the websocket connection
	private void DisconnectWebsocket(){
		websocket.Close ();
		websocket = null;
	}

}
