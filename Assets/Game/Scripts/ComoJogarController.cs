using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class ComoJogarController : MonoBehaviour
{
    [Header("Páginas (Filhos do Canvas)")]
    [Tooltip("Defina a quantidade de páginas no Inspector e arraste cada painel/objeto filho para cá")]
    public GameObject[] paginas;

    [Tooltip("Se marcado, ao avançar na última página ele volta para a primeira (e vice-versa)")]
    public bool permitirLoop = false;

    [Tooltip("Opcional: Texto para mostrar o número da página (Ex: 1 / 4)")]
    public TMP_Text txtIndicadorPagina;

    [Header("Botões de Navegação")]
    public UnityEngine.UI.Button btnAnterior;
    public UnityEngine.UI.Button btnProximo;
    public UnityEngine.UI.Button btnVoltar;
    public string nomeCenaMenuPrincipal = "MainMenu";

    [Header("Feedback Visual dos Botões")]
    public Color corNormal = Color.white;
    public Color corSelecionado = new Color(1f, 0.8f, 0.2f);
    public Vector3 escalaNormal = Vector3.one;
    public Vector3 escalaSelecionado = new Vector3(1.1f, 1.1f, 1.1f);

    [Header("Áudio (Opcional)")]
    public AudioSource audioSource;
    public AudioClip somVirarPagina;
    public AudioClip somVoltar;

    private int paginaAtual = 0;
    private bool saindo = false;

    private void Start()
    {
        if (btnAnterior != null)
        {
            btnAnterior.onClick.AddListener(PaginaAnterior);
            ConfigurarHoverBotao(btnAnterior);
        }

        if (btnProximo != null)
        {
            btnProximo.onClick.AddListener(ProximaPagina);
            ConfigurarHoverBotao(btnProximo);
        }

        if (btnVoltar != null)
        {
            btnVoltar.onClick.AddListener(VoltarAoMenu);
            ConfigurarHoverBotao(btnVoltar);
        }

        paginaAtual = 0;
        AtualizarPagina(false);
    }

    private void Update()
    {
        if (saindo) return;

        // Voltar ao menu principal
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
        {
            VoltarAoMenu();
            return;
        }

        // Navegação entre páginas pelo teclado
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            ProximaPagina();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            PaginaAnterior();
        }
    }

    public void ProximaPagina()
    {
        if (saindo || paginas == null || paginas.Length == 0) return;

        if (paginaAtual < paginas.Length - 1)
        {
            paginaAtual++;
            AtualizarPagina(true);
        }
        else if (permitirLoop)
        {
            paginaAtual = 0;
            AtualizarPagina(true);
        }
    }

    public void PaginaAnterior()
    {
        if (saindo || paginas == null || paginas.Length == 0) return;

        if (paginaAtual > 0)
        {
            paginaAtual--;
            AtualizarPagina(true);
        }
        else if (permitirLoop)
        {
            paginaAtual = paginas.Length - 1;
            AtualizarPagina(true);
        }
    }

    private void AtualizarPagina(bool tocarSom)
    {
        if (paginas == null || paginas.Length == 0) return;

        // Ativa apenas a página atual e desativa todas as outras
        for (int i = 0; i < paginas.Length; i++)
        {
            if (paginas[i] != null)
            {
                paginas[i].SetActive(i == paginaAtual);
            }
        }

        // Esconde a seta esquerda na primeira página e a direita na última (se o loop estiver desligado)
        if (!permitirLoop)
        {
            if (btnAnterior != null)
            {
                bool mostrarAnterior = paginaAtual > 0;
                if (!mostrarAnterior) ResetarVisualBotao(btnAnterior);
                btnAnterior.gameObject.SetActive(mostrarAnterior);
            }

            if (btnProximo != null)
            {
                bool mostrarProximo = paginaAtual < paginas.Length - 1;
                if (!mostrarProximo) ResetarVisualBotao(btnProximo);
                btnProximo.gameObject.SetActive(mostrarProximo);
            }
        }

        if (txtIndicadorPagina != null)
        {
            txtIndicadorPagina.text = $"{paginaAtual + 1} / {paginas.Length}";
        }

        if (tocarSom)
        {
            TocarSom(somVirarPagina);
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
            if (!saindo)
            {
                botao.image.color = corSelecionado;
                botao.transform.localScale = escalaSelecionado;
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
        botao.image.color = corNormal;
        botao.transform.localScale = escalaNormal;
    }

    public void VoltarAoMenu()
    {
        if (saindo) return;
        saindo = true;
        TocarSom(somVoltar);
        SceneManager.LoadScene(nomeCenaMenuPrincipal);
    }

    private void TocarSom(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}