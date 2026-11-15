using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class VoltarMenuController : MonoBehaviour
{
    [Header("Configuração de Retorno")]
    public string nomeCenaMenuPrincipal = "MainMenu";
    public Button btnVoltar;

    [Header("Feedback Visual do Botão")]
    public Color corNormal = Color.white;
    public Color corSelecionado = new Color(1f, 0.8f, 0.2f);
    public Vector3 escalaNormal = Vector3.one;
    public Vector3 escalaSelecionado = new Vector3(1.1f, 1.1f, 1.1f);

    private bool saindo = false;

    private void Start()
    {
        if (btnVoltar != null)
        {
            btnVoltar.onClick.AddListener(VoltarParaMenu);
            ConfigurarHoverMouse();
        }
    }

    private void Update()
    {
        if (saindo) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
        {
            VoltarParaMenu();
        }
    }

    private void ConfigurarHoverMouse()
    {
        EventTrigger trigger = btnVoltar.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = btnVoltar.gameObject.AddComponent<EventTrigger>();

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

    public void VoltarParaMenu()
    {
        if (saindo) return;
        saindo = true;
        SceneManager.LoadScene(nomeCenaMenuPrincipal);
    }
}