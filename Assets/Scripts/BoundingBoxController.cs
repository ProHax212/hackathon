using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

using System.Threading;
using System.Net;
using System.Net.Sockets;

public class BoundingBoxController : MonoBehaviour {

	[HideInInspector]
	public BoundingBox[] boundingBoxes = new BoundingBox[]{ };
	public int udpPort;

	public float canvasXRange;
	public float canvasYRange;
	public float canvasXOffset;
	public float canvasYOffset;

	private WebSocket websocket;
	private Thread receiveThread;

	public struct BoundingBox{
		public float x, y;
		public float width, height;
		public string name;
		public string color;
		public float xRange;
		public float yRange;

		public BoundingBox(float x, float y, float width, float height, string name, string color, float xRange, float yRange){
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
			this.name = name;
			this.color = color;
			this.xRange = xRange;
			this.yRange = yRange;
		}
	}

	// Use this for initialization
	void Start () {
		boundingBoxes = new BoundingBox[]{ 
			//new BoundingBox(0, 0, 100, 100, "Ryan", "#FF0000FF", 100f, 100f),
			//new BoundingBox(-100, -100, 50, 50, "motor", "#00FF00FF", 100f, 100f)
		};

		receiveThread = new Thread (receiveUDPData);
		receiveThread.IsBackground = true;
		receiveThread.Start ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// Separate thread to read UDP data
	private void receiveUDPData(){
		Debug.Log ("Creating Client");
		UdpClient client = new UdpClient (udpPort);
		client.Client.Blocking = false;
		Debug.Log ("Client created");

		while (true) {
			try{
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
				byte[] data = client.Receive(ref anyIP);

				string text = System.Text.Encoding.UTF8.GetString(data);
				Debug.Log(text);
				if(text != null){
					// Parse the JSON and populate the bounding boxes array
					List<BoundingBox> newBoundingBoxes = new List<BoundingBox>();

					List<JSONObject> boundingBoxesJSON = new JSONObject(text).list;
					foreach(JSONObject boundingBoxJSON in boundingBoxesJSON){
						int left = (int)boundingBoxJSON.GetField("left").f;
						int top = (int)boundingBoxJSON.GetField("top").f;
						int bot = (int)boundingBoxJSON.GetField("bot").f;
						int right = (int)boundingBoxJSON.GetField("right").f;

						float x = (left + right) / 2;
						float y = (top + bot) / 2;
						float width = right - left;
						float height = bot - top;
						string name = boundingBoxJSON.GetField("name").str;
						string color = boundingBoxJSON.GetField("color").str;
						float maxX = boundingBoxJSON.GetField("ix").f;
						float maxY = boundingBoxJSON.GetField("iy").f;
						newBoundingBoxes.Add(new BoundingBox(x, y, width, height, name, color, maxX, maxY));
					}

					// Update the bounding boxes array
					newBoundingBoxes = convertData(newBoundingBoxes);
					boundingBoxes = newBoundingBoxes.ToArray();
					Debug.Log("Length: " + boundingBoxes.Length);
				}
			}
			catch(System.Exception err){
				//Debug.Log (err.ToString ());
			}
		}
	}

	private List<BoundingBox> convertData(List<BoundingBox> boundingBoxes){
		float yoloXOffset = 0;
		float yoloYOffset = 0;

		// Convert the x, y, width, height of all of the boxes
		for (int i = 0; i < boundingBoxes.Count; i++) {
			float newX = ((boundingBoxes[i].x + yoloXOffset) / boundingBoxes[i].yRange) * canvasXRange + canvasXOffset;
			float newY = ((boundingBoxes[i].y + yoloYOffset) / boundingBoxes[i].xRange) * canvasYRange + canvasYOffset;
			float newWidth = boundingBoxes[i].width / boundingBoxes[i].yRange * canvasXRange;
			float newHeight = boundingBoxes[i].height / boundingBoxes[i].xRange * canvasYRange;

			BoundingBox newBoundingBox = new BoundingBox (newX, -newY, newWidth, newHeight, boundingBoxes [i].name, boundingBoxes [i].color, boundingBoxes [i].xRange, boundingBoxes [i].yRange);

			boundingBoxes [i] = newBoundingBox;
		}

		return boundingBoxes;
	}

	void OnApplicationQuit(){
		receiveThread.Abort ();
		receiveThread.Join ();
	}

	// Continuously check if the connection is alive
	/*public IEnumerator CheckConnection(){
		while (true) {
			if (!websocket.IsAlive) {
				Debug.Log ("Trying to connect to bounding box websocket");
				DisconnectWebsocket ();
				ConnectWebsocket ();
			}

			yield return new WaitForSeconds (5);
		}
	}

	// Connect asynchronously to the websocket
	private void ConnectWebsocket(){
		websocket = new WebSocket (websocketConnectionString);

		websocket.OnOpen += (sender, e) => {
			Debug.Log("BoundingBox connection opened");
		};

		// Handle json data from the server
		websocket.OnMessage += (sender, e) => {
			Debug.Log("BoundingBox message received: " + e.Data);
			websocket.Send("Hi Shreya");
		};

		websocket.OnError += (sender, e) => {
			Debug.Log("Bounding box websocket error received");
			Debug.Log(e.Exception.ToString());
		};

		websocket.ConnectAsync ();
	}

	// Disconnect the websocket connection
	private void DisconnectWebsocket(){
		websocket.Close ();
		websocket = null;
	}*/

}
