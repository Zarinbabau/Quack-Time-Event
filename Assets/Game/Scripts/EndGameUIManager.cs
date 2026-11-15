using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class EndGameUIManager : MonoBehaviour
{
    [Header("Painel Principal")]
    public GameObject painelFimDeJogo;
    public Image fotoResultado;

    [Header("Botões (Ordem da Esquerda para Direita)")]
    public UnityEngine.UI.Button btnMenu;
    public UnityEngine.UI.Button btnRepetir;
    public UnityEngine.UI.Button btnProxima;

    [Header("Feedback Visual de Seleção")]
    public Color corNormal = Color.white;
    public Color corSelecionado = new Color(1f, 0.8f, 0.2f);
    public Vector3 escalaNormal = Vector3.one;
    public Vector3 escalaSelecionado = new Vector3(1.1f, 1.1f, 1.1f); 

    [Header("Fotos/Sprites de Resultado")]
    public Sprite fotoDerrota;
    public Sprite foto1Estrela;
    public Sprite foto2Estrelas;
    public Sprite foto3Estrelas;

    [Header("Aviso de Troféu e Novo Modo Desbloqueado")]
    [Tooltip("Painel filho dentro da tela de fim de jogo que aparece só quando libera um troféu")]
    public GameObject painelNovoTrofeu;
    public TMP_Text txtMensagemTrofeu;
    [Tooltip("Opcional: Imagem que mostra a arte do troféu conquistado")]
    public Image imgTrofeuConquistado;
    public Sprite[] spritesTrofeus = new Sprite[3];

    [Header("Configuração de Transição")]
    public string nomeCenaMenu = "LevelSelectionMenu"; 
    public int indiceProximaFaseNoMenu = 1;   

    private List<UnityEngine.UI.Button> botoesAtivos = new List<UnityEngine.UI.Button>();
    private int indexBotaoAtual = 0;

    private void Start()
    {
        if (painelFimDeJogo != null) painelFimDeJogo.SetActive(false);
        if (painelNovoTrofeu != null) painelNovoTrofeu.SetActive(false);
    }

    private void Update()
    {
        if (!painelFimDeJogo.activeSelf || botoesAtivos.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MudarSelecaoBotao(1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MudarSelecaoBotao(-1);
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            botoesAtivos[indexBotaoAtual].onClick.Invoke();
        }
    }

    public void MostrarTela(int estrelas)
    {
        painelFimDeJogo.SetActive(true);
        if (painelNovoTrofeu != null) painelNovoTrofeu.SetActive(false);

        botoesAtivos.Clear();
        botoesAtivos.Add(btnMenu);
        botoesAtivos.Add(btnRepetir);

        if (estrelas == 0) 
        {
            fotoResultado.sprite = fotoDerrota;
            btnProxima.gameObject.SetActive(false);
            indexBotaoAtual = 1; 
        }
        else 
        {
            btnProxima.gameObject.SetActive(true);
            botoesAtivos.Add(btnProxima);
            
            if (estrelas == 1) fotoResultado.sprite = foto1Estrela;
            else if (estrelas == 2) fotoResultado.sprite = foto2Estrelas;
            else if (estrelas == 3) fotoResultado.sprite = foto3Estrelas;

            indexBotaoAtual = 2; 

            // Verifica se conquistou todas as estrelas nesta dificuldade
            if (DifficultyManager.VerificarDesbloqueioTrofeuAtual(out string msgTrofeu))
            {
                if (painelNovoTrofeu != null)
                {
                    painelNovoTrofeu.SetActive(true);
                    if (txtMensagemTrofeu != null) txtMensagemTrofeu.text = msgTrofeu;

                    int dif = DifficultyManager.DificuldadeAtual;
                    if (imgTrofeuConquistado != null && spritesTrofeus != null && dif < spritesTrofeus.Length)
                    {
                        imgTrofeuConquistado.sprite = spritesTrofeus[dif];
                    }
                }
            }
        }

        ConfigurarEventosMouse();
        AtualizarFeedbackVisual();
    }

    private void ConfigurarEventosMouse()
    {
        for (int i = 0; i < botoesAtivos.Count; i++)
        {
            int indexGatilho = i;
            EventTrigger trigger = botoesAtivos[i].gameObject.GetComponent<EventTrigger>();
            
            if (trigger == null) 
                trigger = botoesAtivos[i].gameObject.AddComponent<EventTrigger>();
            
            trigger.triggers.Clear();

            EventTrigger.Entry hoverEntry = new EventTrigger.Entry();
            hoverEntry.eventID = EventTriggerType.PointerEnter;
            hoverEntry.callback.AddListener((data) => { 
                indexBotaoAtual = indexGatilho; 
                AtualizarFeedbackVisual(); 
            });
            
            trigger.triggers.Add(hoverEntry);
        }
    }

    private void MudarSelecaoBotao(int direcao)
    {
        indexBotaoAtual += direcao;

        if (indexBotaoAtual < 0) indexBotaoAtual = botoesAtivos.Count - 1;
        if (indexBotaoAtual >= botoesAtivos.Count) indexBotaoAtual = 0;

        AtualizarFeedbackVisual();
    }

    private void AtualizarFeedbackVisual()
    {
        for (int i = 0; i < botoesAtivos.Count; i++)
        {
            bool isSelecionado = (i == indexBotaoAtual);
            botoesAtivos[i].image.color = isSelecionado ? corSelecionado : corNormal;
            botoesAtivos[i].transform.localScale = isSelecionado ? escalaSelecionado : escalaNormal;
        }
    }

    public void VoltarAoMenu() { SceneManager.LoadScene(nomeCenaMenu); }
    public void RepetirFase() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void ProximaFase()
    {
        PlayerPrefs.SetInt("AutoProgression_ToIndex", indiceProximaFaseNoMenu);
        PlayerPrefs.Save();
        SceneManager.LoadScene(nomeCenaMenu);
    }
}