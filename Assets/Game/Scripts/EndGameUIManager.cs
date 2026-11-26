using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

[System.Serializable]
public class FotosResultadoPorModo
{
    public Sprite foto1Estrela;
    public Sprite foto2Estrelas;
    public Sprite foto3Estrelas;
}

public class EndGameUIManager : MonoBehaviour
{
    [Header("Painel Principal")]
    public GameObject painelFimDeJogo;
    public Image fotoResultado;

    [Header("Efeito de Flash de Câmera")]
    [Tooltip("Imagem branca em tela cheia (acima do painelFimDeJogo) com Raycast Target desmarcado")]
    public Image imagemFlashCamera;
    public float duracaoFlash = 0.45f;

    [Header("Áudio da UI e Fim de Fase")]
    [Tooltip("AudioSource dedicado aos efeitos da UI (não use o mesmo da música de fundo)")]
    public AudioSource audioSourceUI;
    public AudioClip somFlashCamera;
    public AudioClip somVitoria;
    public AudioClip somDerrota;
    public AudioClip somCliqueBotao;
    [Tooltip("Tempo de espera após o flash para tocar o som de vitória ou derrota")]
    public float atrasoSomResultado = 0.35f;

    [Header("Música de Fundo da Fase")]
    [Tooltip("Arraste aqui o AudioSource que toca a música de fundo da fase")]
    public AudioSource musicaBackground;
    public float duracaoFadeMusica = 1.2f;

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
    [Tooltip("Configure as fotos de vitória para: [0] Modo Básico, [1] Modo Temporal, [2] Modo PATO PUNK")]
    public FotosResultadoPorModo[] fotosPorModo = new FotosResultadoPorModo[3];

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
    private bool painelNovoTrofeuAberto = false;
    private bool transicionandoCena = false;
    private int frameAberturaTrofeu = -1;
    private int frameFechamentoTrofeu = -1;

    private void Start()
    {
        if (audioSourceUI == null) audioSourceUI = GetComponent<AudioSource>();
        if (painelFimDeJogo != null) painelFimDeJogo.SetActive(false);
        if (painelNovoTrofeu != null) painelNovoTrofeu.SetActive(false);

        if (imagemFlashCamera != null)
        {
            Color c = imagemFlashCamera.color;
            c.a = 0f;
            imagemFlashCamera.color = c;
            imagemFlashCamera.raycastTarget = false;
            imagemFlashCamera.gameObject.SetActive(false);
        }

        painelNovoTrofeuAberto = false;
    }

    private void Update()
    {
        if (painelFimDeJogo == null || !painelFimDeJogo.activeSelf || transicionandoCena) return;

        // Se o aviso de novo troféu estiver na tela, bloqueia os botões até o jogador fechar
        if (painelNovoTrofeuAberto)
        {
            if (Time.frameCount > frameAberturaTrofeu &&
                (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ||
                 Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape) ||
                 Input.GetMouseButtonDown(0)))
            {
                FecharPainelNovoTrofeu();
            }
            return;
        }

        if (botoesAtivos.Count == 0) return;

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
        // Inicia a redução gradual da música de fundo via MusicManager
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PararMusicaComFade(duracaoFadeMusica);
        }
        else if (musicaBackground != null && musicaBackground.isPlaying)
        {
            StartCoroutine(DiminuirMusicaFundoRotina());
        }

        TocarSomUI(somFlashCamera);
    if (imagemFlashCamera != null)
    {
        StartCoroutine(EfeitoFlashCameraRotina());
    }
    
        // Toca o som de vitória ou derrota logo após o flash
        StartCoroutine(TocarSomResultadoComAtraso(estrelas > 0));

        painelFimDeJogo.SetActive(true);
        painelNovoTrofeuAberto = false;
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
            
            Sprite spriteVitoria = ObterSpriteVitoria(estrelas);
            if (spriteVitoria != null)
            {
                fotoResultado.sprite = spriteVitoria;
            }

            indexBotaoAtual = 2; 

            // Verifica se conquistou todas as estrelas nesta dificuldade
            if (DifficultyManager.VerificarDesbloqueioTrofeuAtual(out string msgTrofeu))
            {
                if (painelNovoTrofeu != null)
                {
                    painelNovoTrofeu.SetActive(true);
                    painelNovoTrofeuAberto = true;
                    frameAberturaTrofeu = Time.frameCount;

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

    private IEnumerator EfeitoFlashCameraRotina()
    {
        imagemFlashCamera.gameObject.SetActive(true);
        Color cor = imagemFlashCamera.color;
        cor.a = 1f;
        imagemFlashCamera.color = cor;

        float tempo = 0f;
        while (tempo < duracaoFlash)
        {
            tempo += Time.unscaledDeltaTime;
            cor.a = Mathf.Lerp(1f, 0f, tempo / duracaoFlash);
            imagemFlashCamera.color = cor;
            yield return null;
        }

        cor.a = 0f;
        imagemFlashCamera.color = cor;
        imagemFlashCamera.gameObject.SetActive(false);
    }

    private IEnumerator TocarSomResultadoComAtraso(bool venceu)
    {
        if (atrasoSomResultado > 0f)
        {
            yield return new WaitForSecondsRealtime(atrasoSomResultado);
        }

        TocarSomUI(venceu ? somVitoria : somDerrota);
    }

    private IEnumerator DiminuirMusicaFundoRotina()
    {
        float volumeInicial = musicaBackground.volume;
        float tempo = 0f;

        while (tempo < duracaoFadeMusica)
        {
            tempo += Time.unscaledDeltaTime;
            musicaBackground.volume = Mathf.Lerp(volumeInicial, 0f, tempo / duracaoFadeMusica);
            yield return null;
        }

        musicaBackground.volume = 0f;
        musicaBackground.Stop();
    }

    private void TocarSomUI(AudioClip clip)
    {
        if (audioSourceUI != null && clip != null)
        {
            audioSourceUI.PlayOneShot(clip);
        }
    }

    private Sprite ObterSpriteVitoria(int estrelas)
    {
        if (fotosPorModo == null || fotosPorModo.Length == 0) return null;

        int dif = Mathf.Clamp(DifficultyManager.DificuldadeAtual, 0, fotosPorModo.Length - 1);
        Sprite spriteEscolhido = ExtrairSpritePorEstrelas(fotosPorModo[dif], estrelas);

        if (spriteEscolhido == null && dif != 0)
        {
            spriteEscolhido = ExtrairSpritePorEstrelas(fotosPorModo[0], estrelas);
        }

        return spriteEscolhido;
    }

    private Sprite ExtrairSpritePorEstrelas(FotosResultadoPorModo grupo, int estrelas)
    {
        if (grupo == null) return null;
        if (estrelas == 1) return grupo.foto1Estrela;
        if (estrelas == 2) return grupo.foto2Estrelas;
        if (estrelas == 3) return grupo.foto3Estrelas;
        return null;
    }

    public void FecharPainelNovoTrofeu()
    {
        TocarSomUI(somCliqueBotao);
        if (painelNovoTrofeu != null) painelNovoTrofeu.SetActive(false);
        painelNovoTrofeuAberto = false;
        frameFechamentoTrofeu = Time.frameCount;
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
                if (painelNovoTrofeuAberto || transicionandoCena) return;
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
            bool isSelecionado = !painelNovoTrofeuAberto && (i == indexBotaoAtual);
            botoesAtivos[i].image.color = isSelecionado ? corSelecionado : corNormal;
            botoesAtivos[i].transform.localScale = isSelecionado ? escalaSelecionado : escalaNormal;
        }
    }

    public void VoltarAoMenu()
    {
        if (painelNovoTrofeuAberto || transicionandoCena || Time.frameCount == frameFechamentoTrofeu) return;
        StartCoroutine(TocarSomECarregarCena(nomeCenaMenu));
    }

    public void RepetirFase()
    {
        if (painelNovoTrofeuAberto || transicionandoCena || Time.frameCount == frameFechamentoTrofeu) return;
        StartCoroutine(TocarSomECarregarCena(SceneManager.GetActiveScene().name));
    }

    public void ProximaFase()
    {
        if (painelNovoTrofeuAberto || transicionandoCena || Time.frameCount == frameFechamentoTrofeu) return;
        PlayerPrefs.SetInt("AutoProgression_ToIndex", indiceProximaFaseNoMenu);
        PlayerPrefs.Save();
        StartCoroutine(TocarSomECarregarCena(nomeCenaMenu));
    }

    private IEnumerator TocarSomECarregarCena(string nomeCena)
    {
        transicionandoCena = true;
        TocarSomUI(somCliqueBotao);

        // Pequena pausa para o som do clique tocar antes de a cena ser destruída
        yield return new WaitForSecondsRealtime(0.18f);

        SceneManager.LoadScene(nomeCena);
    }
}