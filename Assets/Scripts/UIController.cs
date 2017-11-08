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

	public List<string> iotDeviceNames;

	public RectTransform iotText_p;
	[Range(0f, 20f)]
	public float iotTextSize;
	[Range(0f, 20f)]
	public int iotTextFontSize;

	public RectTransform motorSlider_p;
	public RectTransform motorSlider;

	private float canvasWidth, canvasHeight;
	private List<RectTransform> drawnBoxes = new List<RectTransform>();

	[HideInInspector]
	public bool showBoxesOnly = false;

	// Use this for initialization
	void Start () {
		showBoxesOnly = false;

		// Create and hide UI Inputs
		motorSlider = Instantiate(motorSlider_p);
		motorSlider.gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update() {
		clearCanvas();
		drawBoxes();
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
		foreach (BoundingBoxController.BoundingBox boundingBox in boundingBoxes) {
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

			// ********************* IoT DEVICE ***************************
			// Check if IoT Data should be shown
			/*if (!showBoxesOnly) {
				Dictionary<string, IoTController.IoTData> iotData = iotController.iotData;
				if (iotDeviceNames.Contains (boundingBox.name)) {
					RectTransform iotText = Instantiate (iotText_p);
					iotText.SetParent (boxObj, false);
					iotText.localPosition = new Vector3 (0f, -(boundingBox.height / 2) - (boxThickness / 2), 0f);
					iotText.sizeDelta = new Vector2 (iotTextSize, iotTextSize);
					Text iotTextComponent = iotText.GetComponent<Text> ();
					iotTextComponent.text = iotData [boundingBox.name].formattedString;
					iotTextComponent.fontSize = iotTextFontSize;
					iotTextComponent.color = Color.white;
				}

				// ******************** IoT Commands **************************
				/*switch (boundingBox.name) {
				case "motor":
					//motorSlider.gameObject.SetActive (true);
					//motorSlider.SetParent (boxObj, false);
					//motorSlider.localPosition = new Vector3 (0f, (boundingBox.height / 2) + (boxThickness) + categoryTextSize, 0f);
					//break;
				}
			}*/

			drawnBoxes.Add (boxObj);
		}
	}

}
