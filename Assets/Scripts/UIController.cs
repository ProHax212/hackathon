using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

	public Canvas canvas;
	public BoundingBoxController boundingBoxController;
	public IoTController iotController;

	[Range(0f, 5f)]
	public float boxThickness = 2f;
	[Range(0f, 20f)]
	public float categoryTextSize = 2f;
	[Range(0, 20)]
	public int categoryFontSize = 2;
	public RectTransform boxImage_p;
	public RectTransform categoryText_p;

	public RectTransform iotText_p;
	[Range(0f, 20f)]
	public float iotTextSize;
	[Range(0f, 20f)]
	public int iotTextFontSize;

	public RectTransform motorSlider_p;

	public RectTransform redButton_p;
	public RectTransform yellowButton_p;
	public RectTransform greenButton_p;
	[Range(0f, 100f)]
	public float buttonPadding = 40f;

	public Dictionary<string, bool> showClasses = new Dictionary<string, bool> ();

	private RectTransform motorSlider;
	private RectTransform redButton;
	private RectTransform yellowButton;
	private RectTransform greenButton;

	private float canvasWidth, canvasHeight;
	private List<RectTransform> drawnBoxes = new List<RectTransform>();

	[HideInInspector]
	public bool showIotData = true;

	[HideInInspector]
	public bool showBoxes = true;

	// Use this for initialization
	void Start () {
		// Create and hide UI Inputs
		motorSlider = Instantiate(motorSlider_p, canvas.transform);
		motorSlider.gameObject.SetActive (false);
		motorSlider.GetComponent<Slider> ().interactable = true;

		redButton = Instantiate (redButton_p, canvas.transform);
		redButton.gameObject.SetActive (false);
		yellowButton = Instantiate (yellowButton_p, canvas.transform);
		yellowButton.gameObject.SetActive (false);
		greenButton = Instantiate (greenButton_p, canvas.transform);
		greenButton.gameObject.SetActive (false);

		// Add button handlers
		redButton.GetComponent<Button>().onClick.AddListener (ToggleRed);
		yellowButton.GetComponent<Button>().onClick.AddListener (ToggleYellow);
		greenButton.GetComponent<Button>().onClick.AddListener (ToggleGreen);
	}
	
	// Update is called once per frame
	void Update() {
		clearCanvas();
		drawBoxes();
	}

	public void AddClass(string className){
		if (!showClasses.ContainsKey (className)) {
			showClasses.Add (className, true);
		}
	}

	// Clear the canvas of drawn boxes
	private void clearCanvas(){
		foreach (RectTransform drawnBox in drawnBoxes) {
			Destroy (drawnBox.gameObject);
		}

		drawnBoxes.Clear ();
	}

	// Draw all of the currently seen bounding boxes
	private void drawBoxes(){
		BoundingBoxController.BoundingBox[] boundingBoxes = boundingBoxController.boundingBoxes;	// Get a copy of the current bounding Boxes

		// Loop through known bounding boxes and draw
		bool sliderFound = false;
		bool patliteFound = false;
		foreach (BoundingBoxController.BoundingBox boundingBox in boundingBoxes) {
			// See if slider is found
			if (boundingBox.name == "motor") {
				sliderFound = true;
			}
			if (boundingBox.name == "patlite") {
				patliteFound = true;
			}

			// Check if the box should be shown
			//if (!showClasses [boundingBox.name]) {
			//	continue;
			//}

			Color boxColor = Color.white;
			ColorUtility.TryParseHtmlString (boundingBox.color, out boxColor);
			Material boxMaterial = new Material (Shader.Find ("UI/Default"));
			boxMaterial.color = boxColor;

			// Create the parent object
			RectTransform boxObj = new GameObject("Box Object", new System.Type[]{typeof(RectTransform)}).GetComponent<RectTransform>();
			boxObj.SetParent (canvas.transform, false);
			boxObj.localPosition = new Vector3 (boundingBox.x, boundingBox.y, 0f);
			boxObj.sizeDelta = new Vector2 (boundingBox.width, boundingBox.height);

			// Top Border
			RectTransform topBorder = Instantiate(boxImage_p);
			topBorder.SetParent (boxObj, false);
			topBorder.localPosition = new Vector3 (0, (boundingBox.height / 2) - (boxThickness / 2), 0f);
			topBorder.sizeDelta = new Vector2 (boundingBox.width, boxThickness);
			topBorder.GetComponent<Image> ().material = boxMaterial;

			// Bot Border
			RectTransform botBorder = Instantiate(boxImage_p);
			botBorder.SetParent (boxObj, false);
			botBorder.localPosition = new Vector3 (0, -(boundingBox.height / 2) + (boxThickness / 2), 0f);
			botBorder.sizeDelta = new Vector2 (boundingBox.width, boxThickness);
			botBorder.GetComponent<Image> ().material = boxMaterial;

			// Left Border
			RectTransform leftBorder = Instantiate(boxImage_p);
			leftBorder.SetParent (boxObj, false);
			leftBorder.localPosition = new Vector3 (-(boundingBox.width / 2) + (boxThickness / 2), 0f, 0f);
			leftBorder.sizeDelta = new Vector2 (boxThickness, boundingBox.height);
			leftBorder.GetComponent<Image> ().material = boxMaterial;

			// Right Border
			RectTransform rightBorder = Instantiate(boxImage_p);
			rightBorder.SetParent (boxObj, false);
			rightBorder.localPosition = new Vector3 ((boundingBox.width / 2) - (boxThickness / 2), 0f, 0f);
			rightBorder.sizeDelta = new Vector2 (boxThickness, boundingBox.height);
			rightBorder.GetComponent<Image> ().material = boxMaterial;

			// Classification
			RectTransform categoryText = Instantiate(categoryText_p);
			categoryText.SetParent (boxObj, false);
			categoryText.localPosition = new Vector3 (0, (boundingBox.height / 2) - (boxThickness / 2) + categoryTextSize);
			categoryText.sizeDelta = new Vector2 (boundingBox.width, categoryTextSize);
			Text textComponent = categoryText.GetComponent<Text> ();
			textComponent.text = boundingBox.name;
			textComponent.color = Color.white;
			textComponent.fontSize = categoryFontSize;

			// Check if you need to show the boxes
			if (!showBoxes) {
				topBorder.gameObject.SetActive (false);
				botBorder.gameObject.SetActive (false);
				rightBorder.gameObject.SetActive (false);
				leftBorder.gameObject.SetActive (false);
			}

			// ********************* IoT DEVICE ***************************
			// Check if IoT Data should be shown
			if (showIotData) {

				// Check if IoT Device
				bool isIotDevice = false;
				string iotDeviceName = "";
				foreach (string deviceName in iotController.iotData.Keys) {
					if (deviceName.StartsWith (boundingBox.name)) {
						iotDeviceName = deviceName;
						isIotDevice = true;
					}
				}

				// IoT Device Found
				if (isIotDevice) {
					Dictionary<string, IoTController.IoTData> iotData = iotController.iotData;
					RectTransform iotText = Instantiate (iotText_p);
					iotText.SetParent (boxObj, false);
					iotText.sizeDelta = new Vector2 (boundingBox.width, boundingBox.height);
//					iotText.localPosition = new Vector3 (-boundingBox.width/2 + boxThickness/2 + iotText.sizeDelta.x/2, boundingBox.height / 2 - boxThickness / 2 - iotText.sizeDelta.y/2, 0f);
					iotText.localPosition = new Vector3 (-boundingBox.width/2 + boxThickness/2 + iotText.sizeDelta.x/2, 0f, 0f); // centerd text

					// Format the text for the IoT Data
					Text iotTextComponent = iotText.GetComponent<Text> ();
					iotTextComponent.text = iotData [iotDeviceName].formattedString;
					iotTextComponent.fontSize = iotTextFontSize;
					iotTextComponent.color = Color.white;

					// ******************** IoT Commands **************************
					switch (boundingBox.name) {
					case "motor":
						if (!motorSlider.gameObject.activeInHierarchy) {
							motorSlider.gameObject.SetActive (true);
						}

						motorSlider.SetParent (boxObj, false);
						motorSlider.localRotation = Quaternion.Euler (new Vector3 (0f, 0f, 90f));
						motorSlider.localPosition = new Vector3 (boundingBox.width / 2 + boxThickness + motorSlider.sizeDelta.y / 2, 0f, 0f);
						motorSlider.SetParent (canvas.transform);
						break;
					case "patlite":
						if (!redButton.gameObject.activeInHierarchy) {
							redButton.gameObject.SetActive (true);
						}
						if (!yellowButton.gameObject.activeInHierarchy) {
							yellowButton.gameObject.SetActive (true);
						}
						if (!greenButton.gameObject.activeInHierarchy) {
							greenButton.gameObject.SetActive (true);
						}

						RectTransform buttonsObj = new GameObject ("Buttons Object", new System.Type[]{ typeof(RectTransform)}).GetComponent<RectTransform> ();
						buttonsObj.SetParent (boxObj.transform, false);
						buttonsObj.sizeDelta = new Vector2 (boundingBox.width, boundingBox.height);
						buttonsObj.localPosition = new Vector3 (boundingBox.width / 2 + boxThickness + buttonsObj.sizeDelta.x/2, 0f, 0f);

						// Place all of the buttons
						redButton.SetParent (buttonsObj, false);
						yellowButton.SetParent (buttonsObj, false);
						greenButton.SetParent (buttonsObj, false);

						int paddingCount = 0;
						redButton.localPosition = new Vector3 (-buttonsObj.sizeDelta.x / 2 + redButton.sizeDelta.x / 2, buttonsObj.sizeDelta.y / 2 - redButton.sizeDelta.y / 2 - paddingCount * buttonPadding, 0f);
						paddingCount += 1;

						yellowButton.localPosition = new Vector3 (-buttonsObj.sizeDelta.x / 2 + yellowButton.sizeDelta.x / 2, buttonsObj.sizeDelta.y / 2 - yellowButton.sizeDelta.y / 2 - paddingCount * buttonPadding, 0f);
						paddingCount += 1;

						greenButton.localPosition = new Vector3 (-buttonsObj.sizeDelta.x / 2 + greenButton.sizeDelta.x / 2, buttonsObj.sizeDelta.y / 2 - greenButton.sizeDelta.y / 2 - paddingCount * buttonPadding, 0f);

						redButton.SetParent (canvas.transform);
						yellowButton.SetParent (canvas.transform);
						greenButton.SetParent (canvas.transform);
						break;
					}
				}
			}

			drawnBoxes.Add (boxObj);
		}

		if (!sliderFound) {
			motorSlider.gameObject.SetActive (false);
		}
		if (!patliteFound) {
			redButton.gameObject.SetActive (false);
			yellowButton.gameObject.SetActive (false);
			greenButton.gameObject.SetActive (false);
		}
	}

	void ToggleRed(){
		IoTController.IoTCommand command = new IoTController.IoTCommand ("patlite", "RedLightControlState", iotController.isRedLightOn ? 1 : 2);
		iotController.SendWebsocketMessage (command.ToString());
		iotController.isRedLightOn = !iotController.isRedLightOn;
	}

	void ToggleYellow(){
		IoTController.IoTCommand command = new IoTController.IoTCommand ("patlite", "AmberLightControlState", iotController.isYellowLightOn ? 1 : 2);
		iotController.SendWebsocketMessage (command.ToString());
		iotController.isYellowLightOn = !iotController.isYellowLightOn;
	}

	void ToggleGreen(){
		IoTController.IoTCommand command = new IoTController.IoTCommand ("patlite", "GreenLightControlState", iotController.isGreenLightOn ? 1 : 2);
		iotController.SendWebsocketMessage (command.ToString());
		iotController.isGreenLightOn = !iotController.isGreenLightOn;
	}

}
