using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // Adicionado para suportar o TextMeshPro

[System.Serializable]
public class DadosFaseMenu
{
    public string idFase;
    public string nomeDaCena;
    public Transform fotoTransform;
    public GameObject[] iconesEstrelas = new GameObject[3];
    
    [Tooltip("Arraste o GameObject da imagem do cadeado desta fase (se houver)")]
    public GameObject iconeCadeado;
}

public class LevelSelectionMenu : MonoBehaviour
{
    [Header("Configuração das Fases")]
    public DadosFaseMenu[] fases;
    
    [Header("Configuração de Câmera")]
    public Camera mainCamera;
    public float zoomOffsetZ = -1.5f;
    public float velocidadeZoom = 2f;

    [Header("UI de Fase Bloqueada")]
    public GameObject painelBloqueado;
    
    [Tooltip("Arraste o componente de Texto (TextMeshPro) que exibirá a mensagem de bloqueio")]
    public TMP_Text txtMensagemBloqueio; // Alterado para aceitar TextMeshPro

    private int currentIndex = 0;
    private bool inputsBloqueados = false;
    private bool painelBloqueadoAberto = false;

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        
        if (painelBloqueado != null) 
            painelBloqueado.SetActive(false);

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

    private void CarregarEstrelasVisuais()
    {
        foreach (var fase in fases)
        {
            int estrelasGanhas = PlayerPrefs.GetInt(fase.idFase + "_Estrelas", 0);

            for (int i = 0; i < 3; i++)
            {
                if (fase.iconesEstrelas[i] != null)
                {
                    fase.iconesEstrelas[i].SetActive(i < estrelasGanhas);
                }
            }
        }
    }

    private void VerificarCadeados()
    {
        // Fase 1 (índice 0) nunca tem cadeado, pois é a inicial.
        if (fases.Length > 0 && fases[0].iconeCadeado != null)
            fases[0].iconeCadeado.SetActive(false);

        // Verifica a Fase 2
        if (fases.Length > 1 && fases[1].iconeCadeado != null)
        {
            bool bloqueada = PlayerPrefs.GetInt(fases[0].idFase + "_Estrelas", 0) < 1;
            fases[1].iconeCadeado.SetActive(bloqueada);
        }

        // Verifica a Fase 3
        if (fases.Length > 2 && fases[2].iconeCadeado != null)
        {
            bool bloqueada = PlayerPrefs.GetInt(fases[1].idFase + "_Estrelas", 0) < 1;
            fases[2].iconeCadeado.SetActive(bloqueada);
        }

        // Verifica a Fase Extra (Fase 4)
        if (fases.Length > 3 && fases[3].iconeCadeado != null)
        {
            int e1 = PlayerPrefs.GetInt(fases[0].idFase + "_Estrelas", 0);
            int e2 = PlayerPrefs.GetInt(fases[1].idFase + "_Estrelas", 0);
            int e3 = PlayerPrefs.GetInt(fases[2].idFase + "_Estrelas", 0);

            bool bloqueada = (e1 < 3 || e2 < 3 || e3 < 3);
            fases[3].iconeCadeado.SetActive(bloqueada);
        }
    }

    private void Update()
    {
        if (painelBloqueadoAberto)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || 
                Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
            {
                FecharPainelBloqueado();
            }
            return;
        }

        if (inputsBloqueados) return;

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
        if (currentIndex == 1) 
        {
            if (PlayerPrefs.GetInt(fases[0].idFase + "_Estrelas", 0) < 1)
            {
                ExibirAvisoBloqueio("Fase 2 Bloqueada!\nConquiste pelo menos 1 estrela na Fase 1 para liberar.");
                return false;
            }
        }
        else if (currentIndex == 2) 
        {
            if (PlayerPrefs.GetInt(fases[1].idFase + "_Estrelas", 0) < 1)
            {
                ExibirAvisoBloqueio("Fase 3 Bloqueada!\nConquiste pelo menos 1 estrela na Fase 2 para liberar.");
                return false;
            }
        }
        else if (currentIndex == 3) 
        {
            int e1 = PlayerPrefs.GetInt(fases[0].idFase + "_Estrelas", 0);
            int e2 = PlayerPrefs.GetInt(fases[1].idFase + "_Estrelas", 0);
            int e3 = PlayerPrefs.GetInt(fases[2].idFase + "_Estrelas", 0);

            if (e1 < 3 || e2 < 3 || e3 < 3)
            {
                ExibirAvisoBloqueio("Fase Extra Bloqueada!\nÉ necessário conseguir 3 estrelas nas Fases 1, 2 e 3 para liberar esta fase.");
                return false;
            }
        }

        inputsBloqueados = true;
        StartCoroutine(ZoomCameraRotina());
        return true;
    }

    private void ExibirAvisoBloqueio(string mensagem)
    {
        if (painelBloqueado != null)
        {
            if (txtMensagemBloqueio != null)
                txtMensagemBloqueio.text = mensagem;

            painelBloqueado.SetActive(true);
            painelBloqueadoAberto = true;
        }

        inputsBloqueados = false;
    }

    public void FecharPainelBloqueado()
    {
        if (painelBloqueado != null)
            painelBloqueado.SetActive(false);

        painelBloqueadoAberto = false;
        inputsBloqueados = false;
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