using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;

public class VoiceController : MonoBehaviour {

	public UIController uiController;

	private KeywordRecognizer keywordRecognizer;
	Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action> ();

	// Use this for initialization
	void Start () {
		// Create keywords for the recognizer
		keywords.Add ("Show Data", () => {
			Debug.Log("Show Data");
			uiController.showBoxesOnly = false;
		});

		keywords.Add ("Hide Data", () => {
			Debug.Log("Hide Data");
			uiController.showBoxesOnly = true;
		});

		keywordRecognizer = new KeywordRecognizer (keywords.Keys.ToArray());
		keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;

		keywordRecognizer.Start ();
	}

	private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args){
		System.Action keywordAction;
		if(keywords.TryGetValue(args.text, out keywordAction)){
			keywordAction.Invoke();
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
