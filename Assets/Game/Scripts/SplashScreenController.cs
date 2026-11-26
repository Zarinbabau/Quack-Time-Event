using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SplashScreenController : MonoBehaviour
{
    public static bool BloqueandoInput { get; private set; }

    [Header("Painéis e Scripts da Cena")]
    [Tooltip("Painel pai que contém a Splash Art e o texto 'Aperte Espaço'")]
    public GameObject painelSplash;

    [Tooltip("CanvasGroup do painel onde ficam o Logo e os Botões do Menu")]
    public CanvasGroup canvasGroupMenuPrincipal;

    [Tooltip("Arraste aqui o componente de script do seu Menu Principal para ele ficar travado durante a Splash")]
    public MonoBehaviour scriptMenuPrincipal;

    [Header("Texto Pulsante")]
    [Tooltip("Texto 'Aperte Espaço para continuar'")]
    public TMP_Text txtAperteEspaco;
    public float velocidadePulso = 3f;
    [Range(0f, 1f)] public float alphaMinimoTexto = 0.25f;

    [Header("Transição e Áudio")]
    public float duracaoFadeOut = 0.4f;
    public AudioClip somAoContinuar;

    [Header("Comportamento")]
    [Tooltip("Se marcado, aparece ao abrir o jogo e não reaparece ao clicar em 'Voltar' nas outras telas")]
    public bool mostrarApenasAoAbrirOJogo = true;
    public bool permitirCliqueMouseTambem = false;

    private static bool splashJaExibidaNaSessao = false;
    private CanvasGroup canvasGroupSplash;
    private AudioSource audioSource;
    private bool fechando = false;

    private void Awake()
    {
        if (painelSplash != null)
        {
            canvasGroupSplash = painelSplash.GetComponent<CanvasGroup>();
            if (canvasGroupSplash == null)
                canvasGroupSplash = painelSplash.AddComponent<CanvasGroup>();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && somAoContinuar != null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        if (mostrarApenasAoAbrirOJogo && splashJaExibidaNaSessao)
        {
            DesativarSplashImediatamente();
            return;
        }

        if (painelSplash != null)
        {
            painelSplash.SetActive(true);
            canvasGroupSplash.alpha = 1f;
            canvasGroupSplash.blocksRaycasts = true;
            BloqueandoInput = true;

            if (scriptMenuPrincipal != null)
                scriptMenuPrincipal.enabled = false;

            if (canvasGroupMenuPrincipal != null)
            {
                canvasGroupMenuPrincipal.interactable = false;
                canvasGroupMenuPrincipal.blocksRaycasts = false;
            }

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void Update()
    {
        if (!BloqueandoInput || fechando) return;

        // Mantém nenhum botão do menu selecionado pelo EventSystem enquanto a Splash estiver aberta
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            EventSystem.current.SetSelectedGameObject(null);

        // Faz o texto "Aperte Espaço para continuar" piscar suavemente
        if (txtAperteEspaco != null)
        {
            float t = (Mathf.Sin(Time.unscaledTime * velocidadePulso) + 1f) * 0.5f;
            Color cor = txtAperteEspaco.color;
            cor.a = Mathf.Lerp(alphaMinimoTexto, 1f, t);
            txtAperteEspaco.color = cor;
        }

        bool pressionouAvancar = Input.GetKeyDown(KeyCode.Space) ||
                                 (permitirCliqueMouseTambem && Input.GetMouseButtonDown(0));

        if (pressionouAvancar)
        {
            StartCoroutine(RotinaFecharSplash());
        }
    }

    private IEnumerator RotinaFecharSplash()
    {
        fechando = true;
        splashJaExibidaNaSessao = true;

        if (somAoContinuar != null && audioSource != null)
            audioSource.PlayOneShot(somAoContinuar);

        if (canvasGroupSplash != null && duracaoFadeOut > 0f)
        {
            float tempo = 0f;
            while (tempo < duracaoFadeOut)
            {
                tempo += Time.unscaledDeltaTime;
                canvasGroupSplash.alpha = Mathf.Lerp(1f, 0f, tempo / duracaoFadeOut);
                yield return null;
            }
        }

        // Garante que o aperto de Espaço deste frame já tenha passado antes de reativar o menu
        yield return null;

        DesativarSplashImediatamente();
    }

    private void DesativarSplashImediatamente()
    {
        if (painelSplash != null)
            painelSplash.SetActive(false);

        if (canvasGroupMenuPrincipal != null)
        {
            canvasGroupMenuPrincipal.interactable = true;
            canvasGroupMenuPrincipal.blocksRaycasts = true;
        }

        if (scriptMenuPrincipal != null)
            scriptMenuPrincipal.enabled = true;

        BloqueandoInput = false;
        fechando = false;
    }
}