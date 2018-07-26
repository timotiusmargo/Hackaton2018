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

    // DictationRecognizer instance status
    private SpeechSystemStatus dictationRecognizerStatus = SpeechSystemStatus.Stopped;

    // Countdown timers for different regular operations
    private float speechStatusUpdateTimer;
    private float speechTextExpiryTimer;

    // limit the update frequency to be every 0.3 second
    private const float speechStatusUpdateFrequency = 0.3f;

    // Clear rendered speech 8 seconds after it renders
    private const float speechTextExpiry = 8.0f;

    // Clear rendered speech 120 seconds after it reset
    private const float speechTextExpirySinceReset = 120.0f;

    private void Awake()
    {
        // Set this class to behave similar to singleton 
        instance = this;
    }

    void Start()
    {
        // Reset frequency countdown
        speechStatusUpdateTimer = speechStatusUpdateFrequency;
        speechTextExpiryTimer = speechTextExpirySinceReset;

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
        speechStatusUpdateTimer -= Time.deltaTime;
        speechTextExpiryTimer -= Time.deltaTime;

        // Only update this thread every updateFrequency duration
        if (speechStatusUpdateTimer <= 0.0f)
        {
            if (dictationRecognizer != null)
            {
                SpeechSystemStatus currentStatus = dictationRecognizer.Status;

                if (currentStatus != dictationRecognizerStatus)
                {
                    if (currentStatus == SpeechSystemStatus.Running)
                    {
                        Results.instance.SetSubtitleContent(@"Say something ;)");
                        Debug.Log($"MicrophoneManager - Update speech status : Now running");
                    }
                    else if (currentStatus == SpeechSystemStatus.Failed)
                    {
                        Results.instance.SetSubtitleContent(@"Something's wrong! (ง’̀-‘́)ง Fixing...");
                        Debug.Log($"MicrophoneManager - Update speech status : Failed");
                        ResetAudioCapture();
                    }
                    else if (currentStatus == SpeechSystemStatus.Stopped)
                    {
                        Debug.Log($"MicrophoneManager - Update speech status : Stopped");
                        ResetAudioCapture();
                    }

                    dictationRecognizerStatus = currentStatus;
                }
            }

            speechStatusUpdateTimer = speechStatusUpdateFrequency;
        }

        // Only update this thread every updateFrequency duration
        if (speechTextExpiryTimer <= 0.0f)
        {
            if (dictationRecognizer != null && dictationRecognizer.Status == SpeechSystemStatus.Running)
            {
                Results.instance.SetSubtitleContent("...");
                Debug.Log($"MicrophoneManager - Update text expiry : Blanked out");
            }

            // Reset countdown timer to maintain the same frequency
            speechTextExpiryTimer = speechTextExpiry;
        }
    }

    /// <summary> 
    /// Reset the speech system DictationRecognizer 
    /// </summary> 
    public void ResetAudioCapture()
    {
        Debug.Log($"MicrophoneManager - ResetAudioCapture");
        StopCapturingAudio();
        StartCapturingAudio();
        speechTextExpiryTimer = speechTextExpirySinceReset;
    }

    /// <summary> 
    /// Start microphone capture. Debugging message is delivered to the Results class. 
    /// </summary> 
    public void StartCapturingAudio()
    {
        Debug.Log($"MicrophoneManager - StartCapturingAudio");
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
        Debug.Log($"MicrophoneManager - StopCapturingAudio");
        if (dictationRecognizer != null)
        {
            Microphone.End(null);
            dictationRecognizer.DictationResult -= DictationRecognizer_DictationResult;
            dictationRecognizer.DictationComplete -= DictationRecognizer_DictationComplete;
            dictationRecognizer.DictationError -= DictationRecognizer_DictationError;
            dictationRecognizer.Dispose();
            dictationRecognizer = null;
        }
    }

    /// <summary>
    /// This handler is called every time the Dictation detects a pause in the speech. 
    /// Debugging message is delivered to the Results class.
    /// </summary>
    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        Debug.Log($"MicrophoneManager - DictationRecognizer_DictationResult");

        // Update UI with dictation captured
        Results.instance.SetSubtitleContent(text);
        speechTextExpiryTimer = speechTextExpiry;

        // Start the coroutine that process the dictation through Azure 
        //StartCoroutine(Translator.instance.TranslateWithUnityNetworking(text));
    }

    /// <summary>
    /// This handler is called every time the Dictation completes
    /// </summary>
    private void DictationRecognizer_DictationComplete(DictationCompletionCause completionCause)
    {
        Debug.Log($"MicrophoneManager - DictationRecognizer_DictationComplete");

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
        Debug.Log($"MicrophoneManager - DictationRecognizer_DictationError");

        Results.instance.SetSubtitleContent(@"¯\_(ツ)_/¯" + error);
    }
}
