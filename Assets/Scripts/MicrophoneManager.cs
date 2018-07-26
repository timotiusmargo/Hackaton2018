using UnityEngine;
using UnityEngine.Windows.Speech;

public class MicrophoneManager : MonoBehaviour {

    // Help to access instance of this object 
    public static MicrophoneManager instance;

    // AudioSource component, provides access to mic 
    private AudioSource audioSource;

    // Flag indicating mic detection 
    private bool microphoneDetected;

    // Component converting speech to text 
    private DictationRecognizer dictationRecognizer;

    private void Awake()
    {
        // Set this class to behave similar to singleton 
        instance = this;
    }

    void Start()
    {
        //Use Unity Microphone class to detect devices and setup AudioSource 
        if (Microphone.devices.Length > 0)
        {
            audioSource = GetComponent<AudioSource>();
            microphoneDetected = true;
        }
        else
        {
            Results.instance.SetSubtitleContent("No microphone detected :(");
        }
    }

    /// <summary> 
    /// Start microphone capture. Debugging message is delivered to the Results class. 
    /// </summary> 
    public void StartCapturingAudio()
    {
        if (microphoneDetected)
        {
            // Start dictation 
            dictationRecognizer = new DictationRecognizer();
            dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;
            dictationRecognizer.DictationComplete += DictationRecognizer_DictationComplete;
            dictationRecognizer.DictationError += DictationRecognizer_DictationError;
            dictationRecognizer.Start();

            // Update UI with mic status 
            Results.instance.SetSubtitleContent("Now say something ;)");
        }
    }

    /// <summary> 
    /// Stop microphone capture. Debugging message is delivered to the Results class. 
    /// </summary> 
    public void StopCapturingAudio()
    {
        Results.instance.SetSubtitleContent("Good bye for now!");
        Microphone.End(null);
        dictationRecognizer.DictationResult -= DictationRecognizer_DictationResult;
        dictationRecognizer.DictationComplete -= DictationRecognizer_DictationComplete;
        dictationRecognizer.DictationError -= DictationRecognizer_DictationError;
        dictationRecognizer.Dispose();
    }

    /// <summary>
    /// This handler is called every time the Dictation detects a pause in the speech. 
    /// Debugging message is delivered to the Results class.
    /// </summary>
    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        // Update UI with dictation captured
        Results.instance.SetSubtitleContent(text);

        // Start the coroutine that process the dictation through Azure 
        //StartCoroutine(Translator.instance.TranslateWithUnityNetworking(text));
    }

    /// <summary>
    /// This handler is called every time the Dictation completes
    /// </summary>
    private void DictationRecognizer_DictationComplete(DictationCompletionCause completionCause)
    {
        if (completionCause != DictationCompletionCause.Complete)
        {
            Results.instance.SetSubtitleContent(@"¯\_(ツ)_/¯");
        }
    }

    /// <summary>
    /// This handler is called every time the Dictation gets an error
    /// </summary>
    private void DictationRecognizer_DictationError(string error, int hresult)
    {
        Results.instance.SetSubtitleContent(@"¯\_(ツ)_/¯" + error);
    }
}
