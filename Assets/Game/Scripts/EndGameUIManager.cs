using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // Necessário para detectar o mouse na UI

public class EndGameUIManager : MonoBehaviour
{
    [Header("Painel Principal")]
    public GameObject painelFimDeJogo;
    public Image fotoResultado;

    [Header("Botões (Ordem da Esquerda para Direita)")]
    public Button btnMenu;
    public Button btnRepetir;
    public Button btnProxima;

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

    [Header("Configuração de Transição")]
    public string nomeCenaMenu = "MenuScene"; 
    public int indiceProximaFaseNoMenu = 1;   

    private List<Button> botoesAtivos = new List<Button>();
    private int indexBotaoAtual = 0;

    private void Start()
    {
        painelFimDeJogo.SetActive(false);
    }

    private void Update()
    {
        if (!painelFimDeJogo.activeSelf || botoesAtivos.Count == 0) return;

        // Navegação pelo Teclado
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MudarSelecaoBotao(1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MudarSelecaoBotao(-1);
        }

        // Confirmação pelo Teclado
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            botoesAtivos[indexBotaoAtual].onClick.Invoke();
        }
    }

    public void MostrarTela(int estrelas)
    {
        painelFimDeJogo.SetActive(true);
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
        }

        ConfigurarEventosMouse();
        AtualizarFeedbackVisual();
    }

    private void ConfigurarEventosMouse()
    {
        // Adiciona um leitor de mouse dinamicamente a cada botão visível
        for (int i = 0; i < botoesAtivos.Count; i++)
        {
            int indexGatilho = i; // Necessário para salvar a posição correta na memória
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

    // --- FUNÇÕES DOS BOTÕES ---
    public void VoltarAoMenu() { SceneManager.LoadScene(nomeCenaMenu); }
    public void RepetirFase() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void ProximaFase()
    {
        PlayerPrefs.SetInt("AutoProgression_ToIndex", indiceProximaFaseNoMenu);
        PlayerPrefs.Save();
        SceneManager.LoadScene(nomeCenaMenu);
    }
}