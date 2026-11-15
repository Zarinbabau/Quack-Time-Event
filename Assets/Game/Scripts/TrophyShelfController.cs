using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

[System.Serializable]
public class DadosTrofeu
{
    public string nomeTrofeu;
    [TextArea] public string descricaoDesbloqueado;
    public SpriteRenderer rendererTrofeu;
    public Sprite spriteSilhueta;
    public Sprite spriteRevelado;
    [Tooltip("Opcional: objeto de brilho/partícula que ativa quando revelado")]
    public GameObject efeitoBrilho;
}

public class TrophyShelfController : MonoBehaviour
{
    [Header("Configuração dos 3 Troféus")]
    public DadosTrofeu[] trofeus = new DadosTrofeu[3];

    [Header("UI de Informação do Troféu")]
    public GameObject painelInfo;
    public TMP_Text txtTituloTrofeu;
    public TMP_Text txtDescricaoTrofeu;
    public string tituloPadrao = "Prateleira de Troféus";
    [TextArea] public string descricaoPadrao = "";

    [Header("Navegação e Retorno")]
    public Camera mainCamera;
    public UnityEngine.UI.Button btnVoltar;
    public string nomeCenaMenuPrincipal = "MainMenu";

    [Header("Feedback Visual do Botão Voltar")]
    public Color corNormal = Color.white;
    public Color corSelecionado = new Color(1f, 0.8f, 0.2f);
    public Vector3 escalaNormal = Vector3.one;
    public Vector3 escalaSelecionado = new Vector3(1.1f, 1.1f, 1.1f);

    private bool saindo = false;

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        if (btnVoltar != null)
        {
            btnVoltar.onClick.AddListener(VoltarAoMenu);
            ConfigurarHoverBotaoVoltar();
        }

        AtualizarVisualTrofeus();
        ExibirTextoPadrao();
    }

    private void AtualizarVisualTrofeus()
    {
        for (int i = 0; i < trofeus.Length; i++)
        {
            bool desbloqueado = DifficultyManager.TrofeuDesbloqueado(i);

            if (trofeus[i].rendererTrofeu != null)
            {
                trofeus[i].rendererTrofeu.sprite = desbloqueado 
                    ? trofeus[i].spriteRevelado 
                    : trofeus[i].spriteSilhueta;
            }

            if (trofeus[i].efeitoBrilho != null)
            {
                trofeus[i].efeitoBrilho.SetActive(desbloqueado);
            }
        }
    }

    private void Update()
    {
        if (saindo) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
        {
            VoltarAoMenu();
            return;
        }

        DetectarMouseSobreTrofeu();
    }

    private void DetectarMouseSobreTrofeu()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector2 posicaoMundo = mainCamera.ScreenToWorldPoint(mousePos);

        Collider2D hit = Physics2D.OverlapPoint(posicaoMundo);

        if (hit != null)
        {
            for (int i = 0; i < trofeus.Length; i++)
            {
                if (trofeus[i].rendererTrofeu != null && hit.transform == trofeus[i].rendererTrofeu.transform)
                {
                    ExibirDetalhesTrofeu(i);
                    return;
                }
            }
        }

        ExibirTextoPadrao();
    }

    private void ExibirDetalhesTrofeu(int index)
    {
        if (painelInfo != null && !painelInfo.activeSelf)
            painelInfo.SetActive(true);

        bool desbloqueado = DifficultyManager.TrofeuDesbloqueado(index);

        if (desbloqueado)
        {
            if (txtTituloTrofeu != null) txtTituloTrofeu.text = trofeus[index].nomeTrofeu;
            if (txtDescricaoTrofeu != null) txtDescricaoTrofeu.text = trofeus[index].descricaoDesbloqueado;
        }
        else
        {
            int estrelasAtuais = DifficultyManager.ObterTotalEstrelasNoModo(index);
            string nomeModo = DifficultyManager.ObterNomeDificuldade(index);

            if (txtTituloTrofeu != null) txtTituloTrofeu.text = "Troféu Bloqueado";
            if (txtDescricaoTrofeu != null)
                txtDescricaoTrofeu.text = $"Consiga 3 estrelas em todas as 4 fases no {nomeModo}.\nProgresso: {estrelasAtuais} / 12 Estrelas";
        }
    }

    private void ExibirTextoPadrao()
    {
        if (painelInfo != null && !painelInfo.activeSelf)
            painelInfo.SetActive(true);

        if (txtTituloTrofeu != null && txtTituloTrofeu.text != tituloPadrao)
            txtTituloTrofeu.text = tituloPadrao;

        if (txtDescricaoTrofeu != null && txtDescricaoTrofeu.text != descricaoPadrao)
            txtDescricaoTrofeu.text = descricaoPadrao;
    }

    private void ConfigurarHoverBotaoVoltar()
    {
        EventTrigger trigger = btnVoltar.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = btnVoltar.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener((data) =>
        {
            btnVoltar.image.color = corSelecionado;
            btnVoltar.transform.localScale = escalaSelecionado;
        });

        EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener((data) =>
        {
            btnVoltar.image.color = corNormal;
            btnVoltar.transform.localScale = escalaNormal;
        });

        trigger.triggers.Add(enter);
        trigger.triggers.Add(exit);
    }

    public void VoltarAoMenu()
    {
        if (saindo) return;
        saindo = true;
        SceneManager.LoadScene(nomeCenaMenuPrincipal);
    }
}