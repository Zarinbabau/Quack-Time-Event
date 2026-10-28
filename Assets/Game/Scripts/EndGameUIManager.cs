using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public Color corSelecionado = new Color(1f, 0.8f, 0.2f); // Amarelo/Dourado para destacar
    public Vector3 escalaNormal = Vector3.one;
    public Vector3 escalaSelecionado = new Vector3(1.1f, 1.1f, 1.1f); // Fica 10% maior quando selecionado

    [Header("Fotos/Sprites de Resultado")]
    public Sprite fotoDerrota;
    public Sprite foto1Estrela;
    public Sprite foto2Estrelas;
    public Sprite foto3Estrelas;

    [Header("Configuração de Transição")]
    public string nomeCenaMenu = "MenuScene"; // Nome exato da cena do menu
    public int indiceProximaFaseNoMenu = 1;   // 0 = Fase1, 1 = Fase2, 2 = Fase3

    private List<Button> botoesAtivos = new List<Button>();
    private int indexBotaoAtual = 0;

    private void Start()
    {
        painelFimDeJogo.SetActive(false);
    }

    private void Update()
    {
        // Se o painel não estiver ativo, ignora os comandos
        if (!painelFimDeJogo.activeSelf || botoesAtivos.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            MudarSelecaoBotao(1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            MudarSelecaoBotao(-1);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // Aciona o evento "On Click" do botão que está selecionado no momento
            botoesAtivos[indexBotaoAtual].onClick.Invoke();
        }
    }

    public void MostrarTela(int estrelas)
    {
        painelFimDeJogo.SetActive(true);
        botoesAtivos.Clear();

        // Adiciona os botões na ordem em que devem ser navegados (Esquerda -> Direita)
        botoesAtivos.Add(btnMenu);
        botoesAtivos.Add(btnRepetir);

        if (estrelas == 0) // Game Over
        {
            fotoResultado.sprite = fotoDerrota;
            btnProxima.gameObject.SetActive(false);
            indexBotaoAtual = 1; // Foco no botão de Repetir
        }
        else // Vitória
        {
            btnProxima.gameObject.SetActive(true);
            botoesAtivos.Add(btnProxima);
            
            if (estrelas == 1) fotoResultado.sprite = foto1Estrela;
            else if (estrelas == 2) fotoResultado.sprite = foto2Estrelas;
            else if (estrelas == 3) fotoResultado.sprite = foto3Estrelas;

            indexBotaoAtual = 2; // Foco no botão de Próxima Fase
        }

        AtualizarFeedbackVisual();
    }

    private void MudarSelecaoBotao(int direcao)
    {
        indexBotaoAtual += direcao;

        // Efeito de loop (vai do último para o primeiro e vice-versa)
        if (indexBotaoAtual < 0) indexBotaoAtual = botoesAtivos.Count - 1;
        if (indexBotaoAtual >= botoesAtivos.Count) indexBotaoAtual = 0;

        AtualizarFeedbackVisual();
    }

    private void AtualizarFeedbackVisual()
    {
        for (int i = 0; i < botoesAtivos.Count; i++)
        {
            bool isSelecionado = (i == indexBotaoAtual);
            
            // Pinta e altera o tamanho do botão para o jogador saber onde está
            botoesAtivos[i].image.color = isSelecionado ? corSelecionado : corNormal;
            botoesAtivos[i].transform.localScale = isSelecionado ? escalaSelecionado : escalaNormal;
        }
    }

    // --- FUNÇÕES EXECUTADAS PELOS BOTÕES ---

    public void VoltarAoMenu()
    {
        SceneManager.LoadScene(nomeCenaMenu);
    }

    public void RepetirFase()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }

    public void ProximaFase()
    {
        // Envia o aviso para a cena do Menu fazer a animação até a próxima fase
        PlayerPrefs.SetInt("AutoProgression_ToIndex", indiceProximaFaseNoMenu);
        PlayerPrefs.Save();
        
        SceneManager.LoadScene(nomeCenaMenu);
    }
}