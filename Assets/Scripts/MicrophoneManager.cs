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

    // dictationRecognizer instance status
    private SpeechSystemStatus dictationRecognizerStatus = SpeechSystemStatus.Stopped;

    private float frequencyCountdown;

    // limit the update frequency to be every 3 seconds
    private const float updateFrequency = 3.0f;

    private void Awake()
    {
        // Set this class to behave similar to singleton 
        instance = this;
    }

    void Start()
    {
        // Reset frequency countdown
        frequencyCountdown = updateFrequency;

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

    void Update()
    {
        frequencyCountdown -= Time.deltaTime;

        // Only update this thread every updateFrequency duration
        if (frequencyCountdown <= 0.0f)
        {
            if (dictationRecognizer != null)
            {
                SpeechSystemStatus currentStatus = dictationRecognizer.Status;

                if (currentStatus != dictationRecognizerStatus)
                {
                    if (currentStatus == SpeechSystemStatus.Running)
                    {
                        Results.instance.SetSubtitleContent(@"Say something ;)");
                    }
                    else if (currentStatus == SpeechSystemStatus.Failed)
                    {
                        Results.instance.SetSubtitleContent(@"Something is wrong! :( Fixing it up...");
                        StopCapturingAudio();
                        StartCapturingAudio();
                    }
                    else if (currentStatus == SpeechSystemStatus.Stopped)
                    {
                        Results.instance.SetSubtitleContent(@"Working hard (ง’̀-‘́)ง ...");
                        StopCapturingAudio();
                        StartCapturingAudio();
                    }

                    dictationRecognizerStatus = currentStatus;
                }
            }
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
            dictationRecognizer.AutoSilenceTimeoutSeconds = 0;
            dictationRecognizer.InitialSilenceTimeoutSeconds = 0;
            dictationRecognizer.Start();

            dictationRecognizerStatus = dictationRecognizer.Status;
        }
    }

    /// <summary> 
    /// Stop microphone capture. Debugging message is delivered to the Results class. 
    /// </summary> 
    public void StopCapturingAudio()
    {
        Results.instance.SetSubtitleContent("...");
        Microphone.End(null);
        dictationRecognizer.DictationResult -= DictationRecognizer_DictationResult;
        dictationRecognizer.DictationComplete -= DictationRecognizer_DictationComplete;
        dictationRecognizer.DictationError -= DictationRecognizer_DictationError;
        dictationRecognizer.Dispose();
        dictationRecognizer = null;
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
