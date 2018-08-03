using System;
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

    // Private lock to prevent race condition
    private System.Object stateLock = new System.Object();

    // limit the update frequency to be every 0.3 second
    private const float speechStatusUpdateFrequency = 0.3f;

    // Allow 1 second to lapsed to reset the speech engine
    private const float speechEngineResetTurnover = 1f;

    // Clear rendered speech 10 seconds after it renders
    private const float speechTextExpiry = 10.0f;

    // Clear rendered speech 30 seconds after it reset
    private const float speechTextExpirySinceReset = 30.0f;

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
        lock (stateLock)
        {
            speechStatusUpdateTimer -= Time.deltaTime;
            speechTextExpiryTimer -= Time.deltaTime;

            // Only update this thread every updateFrequency duration
            if (speechStatusUpdateTimer <= 0.0f)
            {
                if (dictationRecognizer != null)
                {
                    SpeechSystemStatus currentStatus = dictationRecognizer.Status;

                    if (currentStatus != SpeechSystemStatus.Running || currentStatus != dictationRecognizerStatus)
                    {
                        if (currentStatus == SpeechSystemStatus.Running)
                        {
                            Results.instance.SetSubtitleContent(@"Say something ;)");
                            Debug.Log("### " + Time.realtimeSinceStartup + " MicrophoneManager - Update speech status : Now running");
                            speechStatusUpdateTimer = speechStatusUpdateFrequency;
                        }
                        else if (currentStatus == SpeechSystemStatus.Failed || currentStatus == SpeechSystemStatus.Stopped)
                        {
                            Results.instance.SetSubtitleContent(@"Resetting...");
                            Debug.Log("### " + Time.realtimeSinceStartup + " MicrophoneManager - Update speech status : " + currentStatus);
                            ResetAudioCapture();
                            speechStatusUpdateTimer = speechEngineResetTurnover;
                        }

                        dictationRecognizerStatus = currentStatus;
                    }
                }
            }

            // Only update this thread every updateFrequency duration
            if (speechTextExpiryTimer <= 0.0f)
            {
                if (dictationRecognizer != null && dictationRecognizer.Status == SpeechSystemStatus.Running)
                {
                    Results.instance.SetSubtitleContent("...");
                    Debug.Log("### " + Time.realtimeSinceStartup + " MicrophoneManager - Update text expiry : Blanked out");
                }

                // Reset countdown timer to maintain the same frequency
                speechTextExpiryTimer = speechTextExpiry;
            }
        }
    }

    /// <summary> 
    /// Reset the speech system DictationRecognizer 
    /// </summary> 
    public void ResetAudioCapture()
    {
        Debug.Log("### " + Time.realtimeSinceStartup + " MicrophoneManager - ResetAudioCapture");

        lock (stateLock)
        {
            StopCapturingAudio();
            StartCapturingAudio();
            speechTextExpiryTimer = speechTextExpirySinceReset;
        }
    }

    /// <summary> 
    /// Start microphone capture. Debugging message is delivered to the Results class. 
    /// </summary> 
    public void StartCapturingAudio()
    {
        Debug.Log("### " + Time.realtimeSinceStartup + " MicrophoneManager - StartCapturingAudio");
        if (microphoneDetected)
        {
            try
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
            catch (Exception e)
            {
                Debug.Log("### " + Time.realtimeSinceStartup + " MicrophoneManager - StartCapturingAudio Exception!! : " + e.Message);
            }
        }
    }

    /// <summary> 
    /// Stop microphone capture. Debugging message is delivered to the Results class. 
    /// </summary> 
    public void StopCapturingAudio()
    {
        Debug.Log("### " + Time.realtimeSinceStartup + " MicrophoneManager - StopCapturingAudio");
        lock (stateLock)
        {
            if (dictationRecognizer != null)
            {
                try
                {
                    Microphone.End(null);
                    dictationRecognizer.DictationResult -= DictationRecognizer_DictationResult;
                    dictationRecognizer.DictationComplete -= DictationRecognizer_DictationComplete;
                    dictationRecognizer.DictationError -= DictationRecognizer_DictationError;
                    dictationRecognizer.Dispose();
                    dictationRecognizer = null;
                }
                catch (Exception e)
                {
                    Debug.Log("### " + Time.realtimeSinceStartup + " MicrophoneManager - StartCapturingAudio Exception!! : " + e.Message);
                }
            }
        }
    }

    /// <summary>
    /// This handler is called every time the Dictation detects a pause in the speech. 
    /// Debugging message is delivered to the Results class.
    /// </summary>
    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        Debug.Log("### " + Time.realtimeSinceStartup + " MicrophoneManager - DictationRecognizer_DictationResult : " + text);

        lock (stateLock)
        {
            // Update UI with dictation captured
            Results.instance.SetSubtitleContent(text);
            speechTextExpiryTimer = speechTextExpiry;

            // Start the coroutine that process the dictation through Azure 
            //StartCoroutine(Translator.instance.TranslateWithUnityNetworking(text));
        }
    }

    /// <summary>
    /// This handler is called every time the Dictation completes
    /// </summary>
    private void DictationRecognizer_DictationComplete(DictationCompletionCause completionCause)
    {
        Debug.Log("### " + Time.realtimeSinceStartup + " MicrophoneManager - DictationRecognizer_DictationComplete : " + completionCause);

        if (completionCause != DictationCompletionCause.Complete)
        {
            lock (stateLock)
            {
                Results.instance.SetSubtitleContent(@"¯\_(ツ)_/¯ --- " + completionCause);
            }
        }
    }

    /// <summary>
    /// This handler is called every time the Dictation gets an error
    /// </summary>
    private void DictationRecognizer_DictationError(string error, int hresult)
    {
        Debug.Log("### " + Time.realtimeSinceStartup + " MicrophoneManager - DictationRecognizer_DictationError : " + error + " " + hresult);

        lock (stateLock)
        {
            Results.instance.SetSubtitleContent(@"¯\_(ツ)_/¯" + error);
        }
    }
}
