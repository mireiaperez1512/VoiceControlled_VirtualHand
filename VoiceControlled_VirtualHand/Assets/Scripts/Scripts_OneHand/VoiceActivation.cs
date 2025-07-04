using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Oculus.Voice;
using UnityEngine.Events;
using TMPro;
using System.Reflection;
using Meta.WitAi.CallbackHandlers;
using System.Linq; 


public class VoiceActivation : MonoBehaviour
{
    [Header("App Voice Experience (único mic)")]
    [SerializeField] private AppVoiceExperience voice;

    //ACTIVAR ÓRDENES CON BOTÓN DEL CONTRALADOR IZQUIERDO
   [Header("Controller Wake")]
   [Tooltip("Si está activo, pulsar Botón Y equivale a la wake-word")]
    public bool enableControllerWake = true;
    [SerializeField] private TextMeshProUGUI statusLabel;
    [SerializeField] private string listeningText = "Listening";
    [SerializeField] private string idleText      = "Inactive";

    //ACTIVAR ÓRDENES CON PALABRA CLAVE
    [Header("Wake-word intent")]
    [SerializeField] private SimpleIntentHandler wakeIntent;          // intent = activar_escucha
    [Range(0,1)]  public float wakeConfidence = 0.6f;

    [Header("Intent handlers de ÓRDENES")]
    [SerializeField] private SimpleIntentHandler[] commandIntents;

    [Header("UI opcional")]
    [SerializeField] private TextMeshProUGUI partialText;
    [SerializeField] private TextMeshProUGUI fullText;

    [Header("Eventos opcionales")]
    public UnityEvent onWakeWord;          // feedback visual/sonoro
    public UnityEvent<string> onCommand;   // frase completa

    private bool waitingCommand = false;

    /* ───────────────────────── SET-UP ───────────────────────── */

    private void Awake()
    {
        if (!voice || !wakeIntent)
        {
            Debug.LogError("Asigna voice + wakeIntent en el Inspector");
            enabled = false; return;
        }

        /* 1) Wake-word listener */
        wakeIntent.OnIntentTriggered.AddListener(OnWakeWord);

        /* 2) Transcripción en tiempo real */
        voice.VoiceEvents.OnPartialTranscription.AddListener(HandlePartial);
        voice.VoiceEvents.OnFullTranscription.AddListener(HandleFull);

        /* 3) Reactivar mic al terminar la petición */
        voice.VoiceEvents.OnRequestCompleted.AddListener(() => voice.Activate());

        /* 4) Deshabilita todos los handlers de órdenes al arrancar */
        SetCommandHandlersEnabled(false);

        /* 4.bis) Suscribe un callback para apagar los handlers justo DESPUÉS de que se dispare cualquier orden   */
        foreach (var h in commandIntents)
            if (h) h.OnIntentTriggered.AddListener(OnCommandIntentFired);

        /* 5) Arranca el micrófono */
        voice.Activate();

        if (commandIntents == null || commandIntents.Length == 0)
        {
            commandIntents = GetComponents<SimpleIntentHandler>()
                             .Where(h => h != wakeIntent)
                             .ToArray();
            Debug.Log($"[Init] Se han detectado {commandIntents.Length} handlers de órdenes.");
        }
        // Arranca con el Text (TMP) del Button en Inactive
        if (statusLabel) statusLabel.text = idleText;
    }

    private void Update()
    {
        if (!enableControllerWake) return;

        // Botón Y
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            if (!waitingCommand)
            {
                Debug.Log("[Wake] Botón X pulsado → handlers ON");
                waitingCommand = true;
                SetCommandHandlersEnabled(true);
                if (statusLabel) statusLabel.text = listeningText;
                onWakeWord?.Invoke();
            }
        }
    }

    private void OnCommandIntentFired()
    {
        SetCommandHandlersEnabled(false);   // apagamos todos los handlers de órdenes
        waitingCommand = false;             // volvemos al estado inicial
        if (statusLabel) statusLabel.text = idleText;
    }

    private void OnWakeWord()
    {
        waitingCommand = true;
        SetCommandHandlersEnabled(true);      // se habilitan los simple intent handlers
        if (statusLabel) statusLabel.text = listeningText;
        onWakeWord?.Invoke();
        Debug.Log("[Wake] escucha detectado → handlers ON");
    }



    //TRANSCRIPCIONES
    private void HandlePartial(string text)
    {
        if (!waitingCommand) return;
        if (partialText) partialText.text = text;
    }

    private void HandleFull(string text)
    {
        if (!waitingCommand) return;

        waitingCommand = false;
        SetCommandHandlersEnabled(false);
        if (fullText) fullText.text = text;
        onCommand?.Invoke(text);
        Debug.Log($"[Cmd] \"{text}\"  → handlers OFF");
    }

    private void SetCommandHandlersEnabled(bool value)
    {
        foreach (var h in commandIntents)
            if (h) h.enabled = value;
    }
}



