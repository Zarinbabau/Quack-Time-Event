using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
{
    [Header("Elementos da UI (Ordem de Cima para Baixo)")]
    [Tooltip("Objeto pai que contém o Logo e os Botões (será ocultado durante a animação)")]
    public GameObject containerMenuUI;
    public Button btnJogar;
    public Button btnTrofeus;
    public Button btnComoJogar;

    [Header("Feedback Visual de Seleção")]
    public Color corNormal = Color.white;
    public Color corSelecionado = new Color(1f, 0.8f, 0.2f);
    public Vector3 escalaNormal = Vector3.one;
    public Vector3 escalaSelecionado = new Vector3(1.1f, 1.1f, 1.1f);

    [Header("Configuração do Animator")]
    public Animator casaAnimator;
    public string triggerJogar = "AbrirPortaJogar";
    public string triggerTrofeus = "EntrarJanelaTrofeus";
    public string triggerComoJogar = "AbrirPortinhaComoJogar";

    [Header("Nomes Exatos das Cenas de Destino")]
    public string cenaSelecaoFases = "LevelSelectionMenu";
    public string cenaTrofeus = "TrofeusScene";
    public string cenaComoJogar = "ComoJogarScene";

    [Header("Áudio (Opcional)")]
    public AudioSource audioSource;
    public AudioClip somNavegacao;
    public AudioClip somConfirmar;

    private List<Button> botoes = new List<Button>();
    private int indexAtual = 0;
    private bool inputsBloqueados = false;

    private void Start()
    {
        if (casaAnimator == null)
            casaAnimator = GetComponent<Animator>();

        if (containerMenuUI != null)
            containerMenuUI.SetActive(true);

        // Organiza os botões na ordem vertical
        botoes.Add(btnJogar);
        botoes.Add(btnTrofeus);
        botoes.Add(btnComoJogar);

        // Conecta os cliques automaticamente via código
        btnJogar.onClick.AddListener(() => IniciarTransicao(triggerJogar));
        btnTrofeus.onClick.AddListener(() => IniciarTransicao(triggerTrofeus));
        btnComoJogar.onClick.AddListener(() => IniciarTransicao(triggerComoJogar));

        ConfigurarEventosMouse();
        AtualizarFeedbackVisual();
    }

    private void Update()
    {
        if (inputsBloqueados) return;

        // Navegação vertical pelo teclado (Setas Cima/Baixo ou W/S)
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            MudarSelecao(1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            MudarSelecao(-1);
        }

        // Confirmação com Enter ou Espaço
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
        {
            botoes[indexAtual].onClick.Invoke();
        }
    }

    private void ConfigurarEventosMouse()
    {
        for (int i = 0; i < botoes.Count; i++)
        {
            int indexGatilho = i;
            EventTrigger trigger = botoes[i].gameObject.GetComponent<EventTrigger>();

            if (trigger == null)
                trigger = botoes[i].gameObject.AddComponent<EventTrigger>();

            trigger.triggers.Clear();

            EventTrigger.Entry hoverEntry = new EventTrigger.Entry();
            hoverEntry.eventID = EventTriggerType.PointerEnter;
            hoverEntry.callback.AddListener((data) =>
            {
                if (!inputsBloqueados && indexAtual != indexGatilho)
                {
                    indexAtual = indexGatilho;
                    TocarSom(somNavegacao);
                    AtualizarFeedbackVisual();
                }
            });

            trigger.triggers.Add(hoverEntry);
        }
    }

    private void MudarSelecao(int direcao)
    {
        indexAtual += direcao;

        if (indexAtual < 0) indexAtual = botoes.Count - 1;
        if (indexAtual >= botoes.Count) indexAtual = 0;

        TocarSom(somNavegacao);
        AtualizarFeedbackVisual();
    }

    private void AtualizarFeedbackVisual()
    {
        for (int i = 0; i < botoes.Count; i++)
        {
            bool isSelecionado = (i == indexAtual);
            botoes[i].image.color = isSelecionado ? corSelecionado : corNormal;
            botoes[i].transform.localScale = isSelecionado ? escalaSelecionado : escalaNormal;
        }
    }

    private void IniciarTransicao(string nomeTrigger)
    {
        if (inputsBloqueados) return;

        inputsBloqueados = true;
        TocarSom(somConfirmar);

        // Esconde o logo e os botões para deixar a visão limpa durante a animação
        if (containerMenuUI != null)
            containerMenuUI.SetActive(false);

        if (casaAnimator != null)
            casaAnimator.SetTrigger(nomeTrigger);
    }

    private void TocarSom(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    // =========================================================================
    // MÉTODOS CHAMADOS PELOS ANIMATION EVENTS NO FINAL DE CADA ANIMAÇÃO
    // =========================================================================

    public void CarregarCenaJogar()
    {
        SceneManager.LoadScene(cenaSelecaoFases);
    }

    public void CarregarCenaTrofeus()
    {
        SceneManager.LoadScene(cenaTrofeus);
    }

    public void CarregarCenaComoJogar()
    {
        SceneManager.LoadScene(cenaComoJogar);
    }
}