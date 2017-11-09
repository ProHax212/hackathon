using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;

public class VoiceController : MonoBehaviour {

	public UIController uiController;

	private KeywordRecognizer keywordRecognizer;
	private List<string> classesToAdd = new List<string>();
	private Dictionary<string, Object> classesInRecognizer = new Dictionary<string, Object>();
	private bool voiceRecognizerReady = false;
	Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action> ();


	// Use this for initialization
	void Start () {
		// Create keywords for the recognizer
		keywords.Add ("Show Data", () => {
			Debug.Log("Show Data");
			uiController.showIotData = true;
		});

		keywords.Add ("Hide Data", () => {
			Debug.Log("Hide Data");
			uiController.showIotData = false;
		});

		keywords.Add ("Show Boxes", () => {
			Debug.Log ("Show Boxes");
			uiController.showBoxes = true;
		});

		keywords.Add ("Hide Boxes", () => {
			Debug.Log ("Hide Boxes");
			uiController.showBoxes = false;
		});

		keywordRecognizer = new KeywordRecognizer (keywords.Keys.ToArray());
		keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;

		keywordRecognizer.Start ();
		voiceRecognizerReady = true;
	}

	void Update(){
		if (!voiceRecognizerReady) {
			return;
		}

		// Check if you need to update the classes
		bool updated = false;
		foreach(string className in classesToAdd){
			updated = true;
			if (classesInRecognizer.ContainsKey (className)) {
				continue;
			}
			classesInRecognizer.Add (className, new Object());
			Debug.Log ("Voice class added: " + className);
			if (!keywords.ContainsKey ("Show " + className)) {
				keywords.Add ("Show " + className, () => {
					uiController.showClasses [className] = true;
					Debug.Log ("Show " + className);
				});
				keywords.Add ("Hide " + className, () => {
					uiController.showClasses [className] = false;
					Debug.Log ("Hide " + className);
				});
				keywords.Add ("Show Only " + className, () => {
					foreach (string uiClassName in uiController.showClasses.Keys) {
						if (uiClassName != className) {
							uiController.showClasses [uiClassName] = false;
						}
					}

					uiController.showClasses [className] = true;
					Debug.Log ("Show Only " + className);
				});
			}
		}

		if (updated) {
			keywordRecognizer.Stop ();
			keywordRecognizer = new KeywordRecognizer (keywords.Keys.ToArray ());
			keywordRecognizer.Start ();
			classesToAdd.Clear ();
		}
	}

	private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args){
		System.Action keywordAction;
		if(keywords.TryGetValue(args.text, out keywordAction)){
			keywordAction.Invoke();
		}
	}

	public void AddClass(string className){
		if (!classesInRecognizer.ContainsKey (className)) {
			classesToAdd.Add (className);
		}
	}
}
