using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Oculus.Voice;
using UnityEngine.Events;
using Meta.WitAi.Events;


//se ha modificado el script inicial de ActivationButton para que ahora siempre esté escuchando
//para la escena MainMenu, que siempre escuche y espere a los comandos "Mano izquierda", "Mano derecha"
//o "Ambas manos" para así llamar al MenuLoader.cs
public class ActivationButton : MonoBehaviour
{
    [SerializeField] private AppVoiceExperience voice;

    void Awake()
    {
        if (voice == null) voice = FindAnyObjectByType<AppVoiceExperience>();

        // Suscríbete a eventos por si el SDK cortara la grabación
        VoiceEvents ve = voice.VoiceEvents;
        ve.OnStoppedListening.AddListener(OnStopped);
        ve.OnAborted.AddListener(OnStopped);
        ve.OnError.AddListener((s, m) => OnStopped());

        // Activa la escucha
        voice.Activate();
    }

    // Vuelve a activar si se detuviera -> así logras micro siempre activo
    private void OnStopped() => voice.Activate();
}


//Script original de ActivationButton, experiencia de voz desactivada hasta que se pulse botón del controlador
// {
//     public AppVoiceExperience voiceExperience;
//     void Start()
//     {
//         voiceExperience.Deactivate();   // inicia desactivado
//     }

// void Update()
//     {
//         if (OVRInput.GetDown(OVRInput.Button.One))   // Botón A
//         {
//             if (!voiceExperience.Active)                       // Solo si no está escuchando
//             {
//                 voiceExperience.Activate();                    // Empieza a escuchar
//             }
//         }
//     }
// }