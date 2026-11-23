using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum RegraDesbloqueioItem
{
    SempreLiberado,
    Requer1EstrelaFase1,
    Requer1EstrelaFase2,
    Requer3EstrelasFases1a3,
    Requer1EstrelaFaseExtra,
    Requer3EstrelasFaseExtra
}

[System.Serializable]
public class SpritesEstrelasPorModo
{
    [Tooltip("Sprites das 3 partes coloridas da foto (1ª, 2ª e 3ª estrela) neste modo")]
    public Sprite[] sprites = new Sprite[3];
}

[System.Serializable]
public class DadosFaseMenu
{
    public string idFase;
    public string nomeDaCena;
    public Transform fotoTransform;
    public RegraDesbloqueioItem regraDesbloqueio = RegraDesbloqueioItem.SempreLiberado;

    [Header("Estrelas / Partes Coloridas (Deixe tamanho 0 para animações)")]
    [Tooltip("Arraste os 3 GameObjects filhos que colorem a foto a cada estrela")]
    public GameObject[] iconesEstrelas = new GameObject[3];

    [Tooltip("Defina os 3 sprites coloridos para: [0] Básico, [1] Temporal, [2] PATO PUNK")]
    public SpritesEstrelasPorModo[] spritesEstrelasPorModo = new SpritesEstrelasPorModo[3];

    [Header("Bloqueio")]
    [Tooltip("Arraste o GameObject da imagem do cadeado/silhueta desta foto (se houver)")]
    public GameObject iconeCadeado;
}

public class LevelSelectionMenu : MonoBehaviour
{
    [Header("Configuração das Fases e Animações")]
    public DadosFaseMenu[] fases;
    
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
        int dif = DifficultyManager.DificuldadeAtual;

        foreach (var fase in fases)
        {
            // Se for uma animação (sem ícones de estrelas), pula esta etapa
            if (fase.iconesEstrelas == null || fase.iconesEstrelas.Length == 0) continue;

            int estrelasGanhas = DifficultyManager.ObterEstrelasFase(fase.idFase);

            for (int i = 0; i < fase.iconesEstrelas.Length; i++)
            {
                if (fase.iconesEstrelas[i] == null) continue;

                Sprite spriteAlvo = ObterSpriteEstrela(fase, dif, i);
                if (spriteAlvo != null)
                {
                    SpriteRenderer sr = fase.iconesEstrelas[i].GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = spriteAlvo;
                    }
                    else
                    {
                        UnityEngine.UI.Image img = fase.iconesEstrelas[i].GetComponent<UnityEngine.UI.Image>();
                        if (img != null) img.sprite = spriteAlvo;
                    }
                }

                fase.iconesEstrelas[i].SetActive(i < estrelasGanhas);
            }
        }
    }

    private Sprite ObterSpriteEstrela(DadosFaseMenu fase, int modo, int indiceEstrela)
    {
        if (fase.spritesEstrelasPorModo == null || fase.spritesEstrelasPorModo.Length == 0)
            return null;

        int indexModo = Mathf.Clamp(modo, 0, fase.spritesEstrelasPorModo.Length - 1);

        if (fase.spritesEstrelasPorModo[indexModo] != null &&
            fase.spritesEstrelasPorModo[indexModo].sprites != null &&
            indiceEstrela < fase.spritesEstrelasPorModo[indexModo].sprites.Length &&
            fase.spritesEstrelasPorModo[indexModo].sprites[indiceEstrela] != null)
        {
            return fase.spritesEstrelasPorModo[indexModo].sprites[indiceEstrela];
        }

        if (fase.spritesEstrelasPorModo[0] != null &&
            fase.spritesEstrelasPorModo[0].sprites != null &&
            indiceEstrela < fase.spritesEstrelasPorModo[0].sprites.Length)
        {
            return fase.spritesEstrelasPorModo[0].sprites[indiceEstrela];
        }

        return null;
    }

    private bool ItemEstaDesbloqueado(int index, out string mensagemBloqueio)
    {
        mensagemBloqueio = "";
        RegraDesbloqueioItem regra = fases[index].regraDesbloqueio;

        switch (regra)
        {
            case RegraDesbloqueioItem.SempreLiberado:
                return true;

            case RegraDesbloqueioItem.Requer1EstrelaFase1:
                if (DifficultyManager.ObterEstrelasFase("Fase1") < 1)
                {
                    mensagemBloqueio = "Fase 2 Bloqueada!\nConquiste pelo menos 1 estrela na Fase 1 para liberar.";
                    return false;
                }
                return true;

            case RegraDesbloqueioItem.Requer1EstrelaFase2:
                if (DifficultyManager.ObterEstrelasFase("Fase2") < 1)
                {
                    mensagemBloqueio = "Fase 3 Bloqueada!\nConquiste pelo menos 1 estrela na Fase 2 para liberar.";
                    return false;
                }
                return true;

            case RegraDesbloqueioItem.Requer3EstrelasFases1a3:
                int e1 = DifficultyManager.ObterEstrelasFase("Fase1");
                int e2 = DifficultyManager.ObterEstrelasFase("Fase2");
                int e3 = DifficultyManager.ObterEstrelasFase("Fase3");
                if (e1 < 3 || e2 < 3 || e3 < 3)
                {
                    mensagemBloqueio = "Fase Extra Bloqueada!\nÉ necessário conseguir 3 estrelas nas Fases 1, 2 e 3 para liberar.";
                    return false;
                }
                return true;

            case RegraDesbloqueioItem.Requer1EstrelaFaseExtra:
                if (DifficultyManager.ObterEstrelasFase("FaseExtra") < 1)
                {
                    mensagemBloqueio = "Cena Final Bloqueada!\nConclua a Fase Extra com pelo menos 1 estrela para liberar.";
                    return false;
                }
                return true;

            case RegraDesbloqueioItem.Requer3EstrelasFaseExtra:
                if (DifficultyManager.ObterEstrelasFase("FaseExtra") < 3)
                {
                    mensagemBloqueio = "Cena Final Bloqueada!\nConquiste 3 estrelas na Fase Extra para liberar.";
                    return false;
                }
                return true;

            default:
                return true;
        }
    }

    private void VerificarCadeados()
    {
        for (int i = 0; i < fases.Length; i++)
        {
            if (fases[i].iconeCadeado != null)
            {
                bool desbloqueado = ItemEstaDesbloqueado(i, out _);
                fases[i].iconeCadeado.SetActive(!desbloqueado);
            }
        }
    }

    private void Update()
    {
        if (painelBloqueadoAberto)
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)  || 
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

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
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
            if (foto != null && foto.childCount > 1)
            {
                bool isSelecionada = (i == currentIndex);
                foto.GetChild(1).gameObject.SetActive(isSelecionada);
            }
        }
    }

    private bool TentarEntrarNaFase()
    {
        if (!ItemEstaDesbloqueado(currentIndex, out string msgBloqueio))
        {
            ExibirAvisoBloqueio(msgBloqueio);
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

        while (currentIndex < indexAlvo && currentIndex < fases.Length - 1)
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