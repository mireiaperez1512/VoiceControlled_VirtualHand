using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Oculus.Voice;
using UnityEngine.Events;
using TMPro;
using System.Reflection;
using Meta.WitAi.CallbackHandlers;
using System.Linq; 


public class ActivationWord : MonoBehaviour
{
    [Header("App Voice Experience")]
    [SerializeField] private AppVoiceExperience voice;

    //ACTIVAR ÓRDENES CON BOTÓN DEL CONTROLADOR
   [Header("Controller Wake")]
   [Tooltip("Si está activo, pulsar Botón A equivale a la wake-word")]
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

        /* 4) Deshabilita todos los handlers de órdenes al iniciar */
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

       // Botón A: OVRInput.Button.One
       if (OVRInput.GetDown(OVRInput.Button.One))
       {
           // Evitamos duplicar si ya estamos en modo órdenes
           if (!waitingCommand)
           {
               Debug.Log("[Wake] Botón A pulsado → handlers ON");
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
        SetCommandHandlersEnabled(true);      // se habilitan los handlers, ya hay asociación intent <-> acción
        if (statusLabel) statusLabel.text = listeningText;
        onWakeWord?.Invoke();
        Debug.Log("[Wake] escucha detectado → handlers ON");
    }

    //Transcripciones
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





// A VECES VA Y OTRAS NO, CON PROBLEMAS
// public class ActivationWord : MonoBehaviour
// {
//     public AppVoiceExperience voiceKeyword;
//     public AppVoiceExperience voiceMain;

//     void Start()
//     {
//         // Arrancar micrófono al iniciar
//         voiceKeyword.Activate();

//         // Cada vez que Wit termina una petición, arrancar otra
//         voiceKeyword.VoiceEvents.OnStoppedListening.AddListener(() =>
//         {
//             // (un breve retardo evita superposiciones)
//             Invoke(nameof(ReActivate), 0.1f);
//         });
//     }
//     void ReActivate()
//     {
//         if (!voiceKeyword.Active)
//             voiceKeyword.Activate();
//     }

//     public void OnKeywordDetected()
//     {

//     if (!voiceMain.Active)
//     {
//         voiceMain.Activate();
//     }
//     }
// }

// public class ActivationWord : MonoBehaviour
// {
//     [Header("A) Escucha 'escucha'")]
//     public AppVoiceExperience voiceKeyword;
//     [Header("B) Escucha órdenes")]
//     public AppVoiceExperience voiceMain;

//     private enum State { WaitingKeyword, WaitingCommand }
//     private State state = State.WaitingKeyword;

//     void Start()
//     {
//         if (voiceKeyword == null || voiceMain == null)
//         {
//             Debug.LogError("[ActivationWord] Asigna ambas AppVoiceExperience en el Inspector");
//             enabled = false;
//             return;
//         }
//         // Arrancamos siempre en modo palabra-clave
//         voiceKeyword.Activate();
//         HookLogs(voiceKeyword, "KW");
//         HookLogs(voiceMain, "MAIN");

//         // Puente: si el SDK corta por silencio o frase ignorada,
//         // reactiva la keyword SOLO si seguimos en modo keyword
//         voiceKeyword.VoiceEvents.OnStoppedListening.AddListener(KeywordKeepAlive);
//     }

//     // Llamado por tu Intent “activar_escucha”
//     public void OnKeywordDetected()
//     {
//         if (state != State.WaitingKeyword) return;
//         state = State.WaitingCommand;
//         Debug.Log("[ActivationWord] → MODO ÓRDENES");
//         voiceKeyword.Deactivate();
//         voiceMain.Activate();
//     }

//     // Llamado desde VirtualGrabber tras cada acción
//     public void OnActionExecuted()
//     {
//         if (state != State.WaitingCommand) return;
//         state = State.WaitingKeyword;
//         Debug.Log("[ActivationWord] → MODO PALABRA CLAVE");
//         voiceMain.Deactivate();
//         voiceKeyword.Activate();
//     }

// /* ---------- PUENTE KEEP-ALIVE ---------- */

//     private void KeywordKeepAlive()
//     {
//         // Solo reactivar si seguimos esperando la palabra-clave
//         if (state == State.WaitingKeyword && !voiceKeyword.Active)
//         {
//             Debug.Log("[KW] re-armado tras silencio o frase ignorada");
//             voiceKeyword.Activate();
//         }
//     }

//     void HookLogs(AppVoiceExperience ave, string tag)
//     {
//         ave.VoiceEvents.OnStartListening.AddListener(() => Debug.Log($"[{tag}] start"));
//         ave.VoiceEvents.OnStoppedListening.AddListener(() => Debug.Log($"[{tag}] stop"));
//         ave.VoiceEvents.OnError.AddListener((e, m) => Debug.LogWarning($"[{tag}] ERROR {e}: {m}"));
//     }
// }


//TRANSCRIBE Y OYE TODO, SIN PALABRA CLAVE, PERO AL MENOS PARECE FUNCIONAR MUY BIEN
// public class ActivationWord : MonoBehaviour
// {
//     [Header("Wit Voice")]
//     [SerializeField] private AppVoiceExperience voice;   // arrastra tu AppVoiceExperience

//     [Header("Wake‑word intent (SimpleIntentHandler)")]
//     [SerializeField] private SimpleIntentHandler wakeIntent;   // intent = activar_escucha
//     [Tooltip("Confianza mínima para el intent wake‑word")]
//     [Range(0, 1)] public float wakeConfidence = 0.6f;

//     [Header("UI opcional (debug)")]
//     [SerializeField] private TextMeshProUGUI partialText;
//     [SerializeField] private TextMeshProUGUI fullText;

//     [Header("Events")]                   // los expones en el Inspector
//     public UnityEvent onWakeWord;         // feedback (sonido, UI…)
//     public UnityEvent<string> onCommand;  // frase completa tras wake

//     private bool waitingCommand = false;

//     private void Awake()
//     {
//         if (!voice)
//             Debug.LogError("[VoiceWakeCommandManager] Arrastra el AppVoiceExperience en el Inspector");
//         if (!wakeIntent)
//             Debug.LogError("[VoiceWakeCommandManager] Arrastra el SimpleIntentHandler del wake‑word");

//         // 1) Wake‑word listener (intent activar_escucha)
//         wakeIntent.OnIntentTriggered.AddListener(OnWakeWord);

//         // 2) Transcripción en vivo
//         voice.VoiceEvents.OnPartialTranscription.AddListener(HandlePartial);
//         voice.VoiceEvents.OnFullTranscription.AddListener(HandleFull);

//         // 3) Cada vez que Wit envía respuesta HTTP, re‑activar mic
//         voice.VoiceEvents.OnRequestCompleted.AddListener(() => voice.Activate());

//         // 4) Comenzar escuchando desde el inicio
//         voice.Activate();
//     }

//     private void OnDestroy()
//     {
//         // limpieza
//         wakeIntent.OnIntentTriggered.RemoveListener(OnWakeWord);
//         voice.VoiceEvents.OnPartialTranscription.RemoveListener(HandlePartial);
//         voice.VoiceEvents.OnFullTranscription.RemoveListener(HandleFull);
//     }

//     /* ───────────────── Wake‑word detectado ───────────────── */
//     private void OnWakeWord()
//     {
//         waitingCommand = true;             // próxima frase será el comando
//         onWakeWord?.Invoke();              // feedback opcional
//         Debug.Log("[Wake] ‘escucha’ detectado. Esperando comando…");
//     }

//     /* ───────────────── Transcripción parcial ───────────────── */
//     private void HandlePartial(string text)
//     {
//         if (!waitingCommand) return;
//         if (partialText) partialText.text = text;   // mostrar live‑caption opcional
//     }

//     /* ───────────────── Transcripción completa ───────────────── */
//     private void HandleFull(string text)
//     {
//         if (!waitingCommand) return;
//         waitingCommand = false;            // reseteamos para la siguiente ronda

//         if (fullText) fullText.text = text;        // debug
//         onCommand?.Invoke(text);                   // lanza tu lógica de orden
//         Debug.Log($"[Cmd] \"{text}\"");
//     }
// }
