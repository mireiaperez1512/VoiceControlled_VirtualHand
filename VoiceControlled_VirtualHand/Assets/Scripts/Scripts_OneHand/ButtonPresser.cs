using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Oculus.Interaction.Surfaces;
using System;
using UnityEngine.Serialization;
using Oculus.Interaction;
using System.Collections;

// NO SE HA CONSEGUIDO ESTA FUNCIONALIDAD TODAVÍA

// public class ButtonPresser : MonoBehaviour
// {
//     [Header("Obligatorio")]
//     [SerializeField] private PokeInteractable pokeButton;

//     [Header("Visual opcional – mesh que se hunde")]
//     [SerializeField] private Transform surfaceTransform;
//     [SerializeField] private float pressDepth = 0.008f;
//     [SerializeField] private float pressDuration = 0.15f;

//     [Header("Visual opcional – cambio de color")]
//     [SerializeField] private Renderer buttonRenderer;
//     [SerializeField] private Color pressedColor = Color.green;

//     // referencias a los componentes automáticos
//     private PokeInteractableVisual _pokeVis;
//     private InteractableColorVisual _colorVis;
//     private MaterialPropertyBlockEditor _mpbe;

//     private Vector3 _restLocalPos;
//     private Color _restColor;
//     private bool _isPressing;

//     private void Awake()
//     {
//         if (surfaceTransform) _restLocalPos = surfaceTransform.localPosition;
//         if (buttonRenderer) _restColor = buttonRenderer.material.color;

//         // cacheamos los “visuales” automáticos
//         _pokeVis = pokeButton.GetComponent<PokeInteractableVisual>();
//         _colorVis = pokeButton.GetComponent<InteractableColorVisual>();
//         _mpbe = buttonRenderer.GetComponent<MaterialPropertyBlockEditor>();
//     }

//     public void PressFromVoice()
//     {
//         if (_isPressing || pokeButton == null) return;
//         StartCoroutine(PressRoutine());
//     }

//     private IEnumerator PressRoutine()
//     {
//         _isPressing = true;

//         // 3) Hundimiento y color manual
//         if (surfaceTransform != null)
//             surfaceTransform.localPosition = _restLocalPos - new Vector3(0, 0, pressDepth);

//         if (buttonRenderer != null)
//             buttonRenderer.material.color = pressedColor;

//         yield return new WaitForSeconds(pressDuration);

//         // 4) Volver al estado original
//         if (surfaceTransform != null)
//             surfaceTransform.localPosition = _restLocalPos;

//         if (buttonRenderer != null)
//             buttonRenderer.material.color = _restColor;

//         _isPressing = false;
//         Debug.Log("[ButtonPresser] pulsación manual finalizada");
//     }
// }


public class ButtonPresser : MonoBehaviour
{
    [Header("Referencias del Botón")]
    [SerializeField] private Button uiButton;
    [SerializeField] private PokeInteractable pokeInteractable;

    [Header("Configuración Visual")]
    [SerializeField] private Transform buttonTransform;
    [SerializeField] private float pressDepth = 0.005f;
    [SerializeField] private float animationDuration = 0.1f;

    [Header("Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    private Vector3 _originalPosition;
    private bool _isProcessing = false;

    private void Start()
    {
        // Cachear la posición original
        if (buttonTransform != null)
            _originalPosition = buttonTransform.localPosition;
        else if (transform != null)
        {
            buttonTransform = transform;
            _originalPosition = transform.localPosition;
        }

        // Auto-detectar componentes si no están asignados
        if (uiButton == null)
            uiButton = GetComponent<Button>();

        if (pokeInteractable == null)
            pokeInteractable = GetComponent<PokeInteractable>();
    }

    /// <summary>
    /// Método principal para presionar el botón por voz
    /// </summary>
    public void PressButton()
    {
        if (_isProcessing) return;

        StartCoroutine(PressButtonCoroutine());
    }

    private IEnumerator PressButtonCoroutine()
    {
        _isProcessing = true;

        // 1. Efectos visuales y de audio
        PlayPressEffects();

        // 2. Animación de hundimiento
        yield return StartCoroutine(AnimateButtonPress());

        // 3. Ejecutar la lógica del botón
        ExecuteButtonAction();

        // 4. Animación de regreso
        yield return StartCoroutine(AnimateButtonRelease());

        _isProcessing = false;
    }

    private void PlayPressEffects()
    {
        // Reproducir sonido si está configurado
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        Debug.Log("[SimpleVoiceButtonPresser] Botón presionado por voz");
    }

    private IEnumerator AnimateButtonPress()
    {
        if (buttonTransform == null) yield break;

        Vector3 startPosition = buttonTransform.localPosition;
        Vector3 endPosition = _originalPosition - new Vector3(0, 0, pressDepth);

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;

            // Usar una curva suave para la animación
            progress = Mathf.SmoothStep(0f, 1f, progress);

            buttonTransform.localPosition = Vector3.Lerp(startPosition, endPosition, progress);
            yield return null;
        }

        buttonTransform.localPosition = endPosition;
    }

    private IEnumerator AnimateButtonRelease()
    {
        if (buttonTransform == null) yield break;

        Vector3 startPosition = buttonTransform.localPosition;
        Vector3 endPosition = _originalPosition;

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;

            // Usar una curva suave para la animación
            progress = Mathf.SmoothStep(0f, 1f, progress);

            buttonTransform.localPosition = Vector3.Lerp(startPosition, endPosition, progress);
            yield return null;
        }

        buttonTransform.localPosition = endPosition;
    }

    private void ExecuteButtonAction()
    {
        // Método 1: Si es un botón UI, ejecutar directamente
        if (uiButton != null && uiButton.interactable)
        {
            uiButton.onClick.Invoke();
            Debug.Log("[SimpleVoiceButtonPresser] Evento UI Button ejecutado");
            return;
        }

        // Método 2: Si hay un InteractableUnityEventWrapper, usarlo
        var eventWrapper = GetComponent<InteractableUnityEventWrapper>();
        if (eventWrapper != null)
        {
            // Forzar la ejecución del evento cuando se selecciona
            try
            {
                var selectMethod = eventWrapper.GetType().GetMethod("Select");
                selectMethod?.Invoke(eventWrapper, null);
                Debug.Log("[SimpleVoiceButtonPresser] InteractableUnityEventWrapper ejecutado");
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SimpleVoiceButtonPresser] Error ejecutando InteractableUnityEventWrapper: {e.Message}");
            }
        }

        // Método 3: Buscar otros componentes de acción
        ExecuteAlternativeActions();
    }

    private void ExecuteAlternativeActions()
    {
        // Buscar cualquier MonoBehaviour que tenga métodos comunes de botón
        var components = GetComponents<MonoBehaviour>();

        foreach (var component in components)
        {
            var type = component.GetType();

            // Buscar métodos comunes de callback
            var methods = new string[] { "OnButtonClick", "OnPress", "OnSelect", "Execute", "Activate" };

            foreach (var methodName in methods)
            {
                var method = type.GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (method != null && method.GetParameters().Length == 0)
                {
                    try
                    {
                        method.Invoke(component, null);
                        Debug.Log($"[SimpleVoiceButtonPresser] Ejecutado {methodName} en {type.Name}");
                        return;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[SimpleVoiceButtonPresser] Error ejecutando {methodName}: {e.Message}");
                    }
                }
            }
        }

        Debug.LogWarning("[SimpleVoiceButtonPresser] No se encontró ningún método de acción para ejecutar");
    }

    /// <summary>
    /// Método de utilidad para configurar el botón desde el inspector o código
    /// </summary>
    public void SetupButton(Button button = null, PokeInteractable poke = null, Transform buttonTrans = null)
    {
        if (button != null) uiButton = button;
        if (poke != null) pokeInteractable = poke;
        if (buttonTrans != null)
        {
            buttonTransform = buttonTrans;
            _originalPosition = buttonTrans.localPosition;
        }
    }
}