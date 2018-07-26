using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Results : MonoBehaviour {

    public static Results instance;

    [HideInInspector]
    public string azureResponseCode;

    [HideInInspector]
    public string translationResult;

    [HideInInspector]
    public string dictationResult;

    [HideInInspector]
    public string micStatus;

    [HideInInspector]
    public string subtitleContent;

    [HideInInspector]
    public string textConfidenceLevel;

    [HideInInspector]
    public string alertMessage;

    public Text microphoneStatusText;

    public Text azureResponseText;

    public Text dictationText;

    public Text translationResultText;

    public Text subTitleContentText;

    public Text textConfidenceLevelText;

    public Text alertMessageText;

    public GameObject confidenceCube;

    private void Awake()
    {
        // Set this class to behave similar to singleton 
        instance = this;
    }

    /// <summary>
    /// Stores the Azure response value in the static instance of Result class.
    /// </summary>
    public void SetAzureResponse(string result)
    {
        azureResponseCode = result;
        azureResponseText.text = azureResponseCode;
    }

    /// <summary>
    /// Stores the translated result from dictation in the static instance of Result class. 
    /// </summary>
    public void SetDictationResult(string result)
    {
        dictationResult = result;
        dictationText.text = dictationResult;
    }

    /// <summary>
    /// Stores the translated result from Azure Service in the static instance of Result class. 
    /// </summary>
    public void SetTranslatedResult(string result)
    {
        translationResult = result;
        translationResultText.text = translationResult;
    }

    /// <summary>
    /// Stores the status of the Microphone in the static instance of Result class. 
    /// </summary>
    public void SetMicrophoneStatus(string result)
    {
        micStatus = result;
        microphoneStatusText.text = micStatus;
    }

    /// <summary>
    /// Stores the result from dictation in the static instance of Result class. 
    /// </summary>
    public void SetSubtitleContent(string result)
    {
        subtitleContent = result;
        subTitleContentText.text = subtitleContent;
    }

    /// <summary>
    /// Stores the confidence level from dictation in the static instance of Result class. 
    /// </summary>
    public void SetTextConfidence(string result)
    {
        textConfidenceLevel = result;
        textConfidenceLevelText.text = textConfidenceLevel;
    }

    public void SetAlertMessage(string result)
    {
        alertMessage = result;
        alertMessageText.text = alertMessage;
    }

    public void ChangeCubeColor(Color color)
    {
        MeshRenderer gameObjectRenderer = confidenceCube.GetComponent<MeshRenderer>();
        gameObjectRenderer.material.color = color;     
    }

}
