using UnityEngine;
using UnityEngine.UI;

public class Results : MonoBehaviour {

    public static Results instance;

    [HideInInspector]
    public string subtitleContent;

    public Text subTitleContentText;

    private void Awake()
    {
        // Set this class to behave similar to singleton 
        instance = this;
    }

    /// <summary>
    /// Stores the translated result from Azure Service in the static instance of Result class. 
    /// </summary>
    public void SetTranslatedResult(string result)
    {
        //translationResult = result;
        //translationResultText.text = translationResult;
    }

    /// <summary>
    /// Stores the result from dictation in the static instance of Result class. 
    /// </summary>
    public void SetSubtitleContent(string result)
    {
        subtitleContent = result;
        subTitleContentText.text = subtitleContent;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
