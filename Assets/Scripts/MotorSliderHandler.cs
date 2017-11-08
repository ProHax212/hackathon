using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class MotorSliderHandler : MonoBehaviour, IEndDragHandler {

	public int maxMotorRPM;
	public string connectionString;

	private Slider slider;

	// Use this for initialization
	void Start () {
		slider = GetComponent<Slider> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// Called when the slider is finished moving
	public void OnEndDrag(PointerEventData eventData){
		Debug.Log ("Slider value: " + slider.value);
	}

}
