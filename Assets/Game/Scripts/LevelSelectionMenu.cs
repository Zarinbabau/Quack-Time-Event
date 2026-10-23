using System.Collections;
using UnityEngine;

// Essa classe agrupa as informações de cada fase no Inspector
[System.Serializable]
public class DadosFaseMenu
{
    [Tooltip("O mesmo ID usado no ScoreManager (Ex: Fase1, Fase2)")]
    public string idFase;
    
    [Tooltip("O GameObject da foto no quadro")]
    public Transform fotoTransform;
    
    [Tooltip("Arraste os 3 GameObjects das estrelas desta fase")]
    public GameObject[] iconesEstrelas = new GameObject[3];
}

public class LevelSelectionMenu : MonoBehaviour
{
    [Header("Configuração das Fases")]
    [Tooltip("Configure as fases na ordem em que o jogador navega (Esquerda p/ Direita)")]
    public DadosFaseMenu[] fases;
    
    [Header("Configuração de Câmera")]
    public Camera mainCamera;
    public float zoomOffsetZ = -1.5f;
    public float velocidadeZoom = 2f;

    private int currentIndex = 0;
    private bool inputsBloqueados = false;

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        
        CarregarEstrelasVisuais();
        AtualizarSelecaoVisual();
    }

    private void CarregarEstrelasVisuais()
    {
        foreach (var fase in fases)
        {
            // Busca o valor salvo, retornando 0 caso a fase ainda não tenha sido jogada
            int estrelasGanhas = PlayerPrefs.GetInt(fase.idFase + "_Estrelas", 0);

            for (int i = 0; i < 3; i++)
            {
                if (fase.iconesEstrelas[i] != null)
                {
                    // Ativa a estrela apenas se o índice dela for menor que o número de estrelas ganhas
                    fase.iconesEstrelas[i].SetActive(i < estrelasGanhas);
                }
            }
        }
    }

    private void Update()
    {
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
            ConfirmarFase();
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

    private void ConfirmarFase()
    {
        // Bloqueio da Fase Extra (Assumindo que seja a 4ª fase no array, índice 3)
        if (currentIndex == 3) 
        {
            int estrelasFase1 = PlayerPrefs.GetInt("Fase1_Estrelas", 0);
            int estrelasFase2 = PlayerPrefs.GetInt("Fase2_Estrelas", 0);
            int estrelasFase3 = PlayerPrefs.GetInt("Fase3_Estrelas", 0);

            if (estrelasFase1 < 3 || estrelasFase2 < 3 || estrelasFase3 < 3)
            {
                Debug.Log("Você precisa de 3 estrelas nas 3 primeiras fases para liberar a Fase Extra!");
                // Opcional: Ative um painel de aviso na UI aqui
                return; 
            }
        }

        inputsBloqueados = true;
        StartCoroutine(ZoomCameraRotina());
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

        Debug.Log("Carregando cena da fase: " + fases[currentIndex].idFase);
        // UnityEngine.SceneManagement.SceneManager.LoadScene(fases[currentIndex].idFase);
    }
}