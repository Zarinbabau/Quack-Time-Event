using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[System.Serializable]
public class ArteFasePorModo
{
    [Tooltip("Imagem da foto ainda não revelada (silhueta / apagada)")]
    public Sprite spriteSilhueta;
    [Tooltip("Imagem colorida da foto revelada")]
    public Sprite spriteRevelada;
}

[System.Serializable]
public class DadosFaseMenu
{
    public string idFase;
    public string nomeDaCena;
    public Transform fotoTransform;

    [Header("Visual da Foto por Dificuldade")]
    [Tooltip("Arraste o SpriteRenderer da foto na parede")]
    public SpriteRenderer rendererFoto;
    [Tooltip("Configure as imagens para: [0] Modo Básico, [1] Modo Rápido, [2] Modo Mestre")]
    public ArteFasePorModo[] artesPorModo = new ArteFasePorModo[3];

    [Header("Estrelas e Cadeado")]
    public GameObject[] iconesEstrelas = new GameObject[3];
    [Tooltip("Arraste o GameObject da imagem do cadeado desta fase (se houver)")]
    public GameObject iconeCadeado;
}

public class LevelSelectionMenu : MonoBehaviour
{
    public enum RegraRevelacaoFoto
    {
        AoDesbloquearAFase,
        AoConcluirAFaseComEstrela
    }

    [Header("Configuração das Fases")]
    public DadosFaseMenu[] fases;

    [Tooltip("Define se a foto fica colorida assim que o cadeado abre ou só depois de ganhar 1 estrela na fase")]
    public RegraRevelacaoFoto quandoRevelarFoto = RegraRevelacaoFoto.AoDesbloquearAFase;
    
    [Header("Configuração de Câmera")]
    public Camera mainCamera;
    public float zoomOffsetZ = -1.5f;
    public float velocidadeZoom = 2f;

    [Header("Seletor de Dificuldade")]
    public TMP_Text txtDificuldadeAtual;
    public UnityEngine.UI.Button btnDificuldadeAnterior;
    public UnityEngine.UI.Button btnProximaDificuldade;

    [Header("UI de Fase / Modo Bloqueado")]
    public GameObject painelBloqueado;
    public TMP_Text txtMensagemBloqueio; 

    [Header("Voltar ao Menu Principal")]
    public string cenaMenuPrincipal = "MainMenu";
    public UnityEngine.UI.Button btnVoltar;
    public Color corBotaoNormal = Color.white;
    public Color corBotaoSelecionado = new Color(1f, 0.8f, 0.2f);
    public Vector3 escalaBotaoNormal = Vector3.one;
    public Vector3 escalaBotaoSelecionado = new Vector3(1.1f, 1.1f, 1.1f);

    private int currentIndex = 0;
    private bool inputsBloqueados = false;
    private bool painelBloqueadoAberto = false;
    private Vector3 ultimaPosicaoMouse;

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        
        if (painelBloqueado != null) 
            painelBloqueado.SetActive(false);

        if (btnVoltar != null)
        {
            btnVoltar.onClick.AddListener(VoltarAoMenuPrincipal);
            ConfigurarHoverBotao(btnVoltar);
        }

        if (btnDificuldadeAnterior != null)
        {
            btnDificuldadeAnterior.onClick.AddListener(() => AlterarDificuldade(-1));
            ConfigurarHoverBotao(btnDificuldadeAnterior);
        }

        if (btnProximaDificuldade != null)
        {
            btnProximaDificuldade.onClick.AddListener(() => AlterarDificuldade(1));
            ConfigurarHoverBotao(btnProximaDificuldade);
        }

        ultimaPosicaoMouse = Input.mousePosition;

        AtualizarUIDificuldade();
        CarregarEstrelasVisuais();
        VerificarCadeados();
        AtualizarFotosVisuais();
        AtualizarSelecaoVisual();

        if (PlayerPrefs.HasKey("AutoProgression_ToIndex"))
        {
            int indexAlvo = PlayerPrefs.GetInt("AutoProgression_ToIndex");
            PlayerPrefs.DeleteKey("AutoProgression_ToIndex"); 
            StartCoroutine(RotinaAutoProgresso(indexAlvo));
        }
    }

    public void AlterarDificuldade(int direcao)
    {
        if (inputsBloqueados || painelBloqueadoAberto) return;

        int novaDif = Mathf.Clamp(DifficultyManager.DificuldadeAtual + direcao, 0, 2);
        if (novaDif == DifficultyManager.DificuldadeAtual) return;

        if (novaDif > DifficultyManager.DificuldadeMaximaDesbloqueada)
        {
            string modoAnterior = DifficultyManager.ObterNomeDificuldade(novaDif - 1);
            string modoAlvo = DifficultyManager.ObterNomeDificuldade(novaDif);
            ExibirAvisoBloqueio($"{modoAlvo} Bloqueado!\nConquiste 3 estrelas em todas as 4 fases do {modoAnterior} para liberar.");
            return;
        }

        DifficultyManager.DificuldadeAtual = novaDif;
        AtualizarUIDificuldade();
        CarregarEstrelasVisuais();
        VerificarCadeados();
        AtualizarFotosVisuais();
    }

    private void AtualizarUIDificuldade()
    {
        int dif = DifficultyManager.DificuldadeAtual;

        if (txtDificuldadeAtual != null)
            txtDificuldadeAtual.text = DifficultyManager.ObterNomeDificuldade(dif);

        if (btnDificuldadeAnterior != null)
        {
            bool podeVoltar = dif > 0;
            if (!podeVoltar) ResetarVisualBotao(btnDificuldadeAnterior);
            btnDificuldadeAnterior.gameObject.SetActive(podeVoltar);
        }

        if (btnProximaDificuldade != null)
        {
            bool temProximo = dif < 2;
            if (!temProximo) ResetarVisualBotao(btnProximaDificuldade);
            btnProximaDificuldade.gameObject.SetActive(temProximo);
        }
    }

    private void ConfigurarHoverBotao(UnityEngine.UI.Button botao)
    {
        EventTrigger trigger = botao.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = botao.gameObject.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener((data) =>
        {
            if (!inputsBloqueados && !painelBloqueadoAberto)
            {
                botao.image.color = corBotaoSelecionado;
                botao.transform.localScale = escalaBotaoSelecionado;
            }
        });

        EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener((data) =>
        {
            ResetarVisualBotao(botao);
        });

        trigger.triggers.Add(enter);
        trigger.triggers.Add(exit);
    }

    private void ResetarVisualBotao(UnityEngine.UI.Button botao)
    {
        if (botao == null) return;
        botao.image.color = corBotaoNormal;
        botao.transform.localScale = escalaBotaoNormal;
    }

    private void CarregarEstrelasVisuais()
    {
        foreach (var fase in fases)
        {
            int estrelasGanhas = DifficultyManager.ObterEstrelasFase(fase.idFase);
            for (int i = 0; i < 3; i++)
            {
                if (fase.iconesEstrelas[i] != null)
                    fase.iconesEstrelas[i].SetActive(i < estrelasGanhas);
            }
        }
    }

    private bool FaseEstaDesbloqueada(int index)
    {
        if (index == 0) return true;
        if (index == 1) return DifficultyManager.ObterEstrelasFase(fases[0].idFase) >= 1;
        if (index == 2) return DifficultyManager.ObterEstrelasFase(fases[1].idFase) >= 1;
        if (index == 3)
        {
            int e1 = DifficultyManager.ObterEstrelasFase(fases[0].idFase);
            int e2 = DifficultyManager.ObterEstrelasFase(fases[1].idFase);
            int e3 = DifficultyManager.ObterEstrelasFase(fases[2].idFase);
            return e1 >= 3 && e2 >= 3 && e3 >= 3;
        }
        return false;
    }

    private void VerificarCadeados()
    {
        for (int i = 0; i < fases.Length; i++)
        {
            if (fases[i].iconeCadeado != null)
            {
                fases[i].iconeCadeado.SetActive(!FaseEstaDesbloqueada(i));
            }
        }
    }

    private void AtualizarFotosVisuais()
    {
        int dif = DifficultyManager.DificuldadeAtual;

        for (int i = 0; i < fases.Length; i++)
        {
            if (fases[i].rendererFoto == null) continue;
            if (fases[i].artesPorModo == null || fases[i].artesPorModo.Length == 0) continue;

            int indexModo = Mathf.Clamp(dif, 0, fases[i].artesPorModo.Length - 1);
            ArteFasePorModo arteAtual = fases[i].artesPorModo[indexModo];

            // Caso ainda não tenha colocado artes para os modos 1 ou 2, usa as do Modo 0 para não sumir
            if (arteAtual.spriteRevelada == null && arteAtual.spriteSilhueta == null)
                arteAtual = fases[i].artesPorModo[0];

            bool revelada = (quandoRevelarFoto == RegraRevelacaoFoto.AoDesbloquearAFase)
                ? FaseEstaDesbloqueada(i)
                : DifficultyManager.ObterEstrelasFase(fases[i].idFase) >= 1;

            Sprite spriteEscolhido = revelada ? arteAtual.spriteRevelada : arteAtual.spriteSilhueta;
            if (spriteEscolhido != null)
            {
                fases[i].rendererFoto.sprite = spriteEscolhido;
            }
        }
    }

    private void Update()
    {
        if (painelBloqueadoAberto)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || 
                Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape) || 
                Input.GetMouseButtonDown(0)) 
            {
                FecharPainelBloqueado();
            }
            return;
        }

        if (inputsBloqueados) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
        {
            VoltarAoMenuPrincipal();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q)) AlterarDificuldade(-1);
        else if (Input.GetKeyDown(KeyCode.E)) AlterarDificuldade(1);

        // --- SISTEMA DO MOUSE ---
        bool moveuMouse = Input.mousePosition != ultimaPosicaoMouse;
        ultimaPosicaoMouse = Input.mousePosition;

        if (moveuMouse)
        {
            VerificarMouseSobreFase();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
            Vector2 posicaoMouseMundo = mainCamera.ScreenToWorldPoint(mousePos);
            
            Collider2D hit = Physics2D.OverlapPoint(posicaoMouseMundo);

            if (hit != null)
            {
                for (int i = 0; i < fases.Length; i++)
                {
                    if (hit.transform == fases[i].fotoTransform || hit.transform.IsChildOf(fases[i].fotoTransform))
                    {
                        if (currentIndex == i) TentarEntrarNaFase();
                        break;
                    }
                }
            }
        }

        // --- SISTEMA DO TECLADO ---
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            MudarSelecao(1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            MudarSelecao(-1);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            TentarEntrarNaFase();
        }
    }

    private void VerificarMouseSobreFase()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector2 posicaoMouseMundo = mainCamera.ScreenToWorldPoint(mousePos);
        
        Collider2D hit = Physics2D.OverlapPoint(posicaoMouseMundo);

        if (hit != null)
        {
            for (int i = 0; i < fases.Length; i++)
            {
                if (hit.transform == fases[i].fotoTransform || hit.transform.IsChildOf(fases[i].fotoTransform))
                {
                    if (currentIndex != i)
                    {
                        currentIndex = i;
                        AtualizarSelecaoVisual();
                    }
                    break;
                }
            }
        }
    }

    private void MudarSelecao(int direcao)
    {
        currentIndex += direcao;
        if (currentIndex < 0) currentIndex = 0;
        if (currentIndex >= fases.Length) currentIndex = fases.Length - 1;
        AtualizarSelecaoVisual();
    }

    private void AtualizarSelecaoVisual()
    {
        for (int i = 0; i < fases.Length; i++)
        {
            Transform foto = fases[i].fotoTransform;
            if (foto.childCount > 1)
            {
                bool isSelecionada = (i == currentIndex);
                foto.GetChild(1).gameObject.SetActive(isSelecionada);
            }
        }
    }

    private bool TentarEntrarNaFase()
    {
        if (!FaseEstaDesbloqueada(currentIndex))
        {
            if (currentIndex == 1)
                ExibirAvisoBloqueio("Fase 2 Bloqueada!\nConquiste pelo menos 1 estrela na Fase 1 para liberar.");
            else if (currentIndex == 2)
                ExibirAvisoBloqueio("Fase 3 Bloqueada!\nConquiste pelo menos 1 estrela na Fase 2 para liberar.");
            else if (currentIndex == 3)
                ExibirAvisoBloqueio("Fase Extra Bloqueada!\nÉ necessário conseguir 3 estrelas nas Fases 1, 2 e 3 para liberar esta fase.");

            return false;
        }

        inputsBloqueados = true;
        StartCoroutine(ZoomCameraRotina());
        return true;
    }

    private void ExibirAvisoBloqueio(string mensagem)
    {
        if (painelBloqueado != null)
        {
            if (txtMensagemBloqueio != null) txtMensagemBloqueio.text = mensagem;
            painelBloqueado.SetActive(true);
            painelBloqueadoAberto = true;
        }
        inputsBloqueados = false;
    }

    public void FecharPainelBloqueado()
    {
        if (painelBloqueado != null) painelBloqueado.SetActive(false);
        painelBloqueadoAberto = false;
        inputsBloqueados = false;
    }

    public void VoltarAoMenuPrincipal()
    {
        if (inputsBloqueados) return;
        inputsBloqueados = true;
        SceneManager.LoadScene(cenaMenuPrincipal);
    }

    private IEnumerator RotinaAutoProgresso(int indexAlvo)
    {
        inputsBloqueados = true;
        yield return new WaitForSeconds(0.5f);

        while (currentIndex < indexAlvo)
        {
            MudarSelecao(1);
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(0.5f);
        TentarEntrarNaFase();
    }

    private IEnumerator ZoomCameraRotina()
    {
        Transform fotoAlvo = fases[currentIndex].fotoTransform;
        Vector3 posicaoInicial = mainCamera.transform.position;
        Vector3 posicaoAlvo = new Vector3(fotoAlvo.position.x, fotoAlvo.position.y, fotoAlvo.position.z + zoomOffsetZ);

        float progresso = 0f;
        while (progresso < 1f)
        {
            progresso += Time.deltaTime * velocidadeZoom;
            mainCamera.transform.position = Vector3.Lerp(posicaoInicial, posicaoAlvo, Mathf.SmoothStep(0, 1, progresso));
            yield return null;
        }

        SceneManager.LoadScene(fases[currentIndex].nomeDaCena);
    }
}