using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para trocar de cena

[System.Serializable]
public class DadosFaseMenu
{
    [Tooltip("O mesmo ID usado no ScoreManager para salvar (Ex: Fase1)")]
    public string idFase;
    
    [Tooltip("O nome exato da Cena desta fase na Unity (Ex: Cena_Fase1)")]
    public string nomeDaCena;
    
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

        // CHECA SE VEIO DO BOTÃO "PRÓXIMA FASE"
        if (PlayerPrefs.HasKey("AutoProgression_ToIndex"))
        {
            int indexAlvo = PlayerPrefs.GetInt("AutoProgression_ToIndex");
            PlayerPrefs.DeleteKey("AutoProgression_ToIndex"); // Apaga para não repetir
            
            StartCoroutine(RotinaAutoProgresso(indexAlvo));
        }
    }

// NOVA CORROTINA DE ANIMAÇÃO
    private IEnumerator RotinaAutoProgresso(int indexAlvo)
    {
        inputsBloqueados = true;
        
        // Espera meio segundo pro menu carregar visualmente
        yield return new WaitForSeconds(0.5f);

        // Move a seleção uma por uma até chegar na fase desejada
        while (currentIndex < indexAlvo)
        {
            MudarSelecao(1);
            yield return new WaitForSeconds(0.5f); // Ritmo da animação (pode alterar)
        }

        // Aguarda um instante para o jogador ver que desbloqueou/chegou na foto
        yield return new WaitForSeconds(0.5f);

        // Dá o Enter automaticamente
        ConfirmarFase();
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
        // Regras de Desbloqueio Baseadas no Índice Atual
        
        if (currentIndex == 1) // Fase 2
        {
            // Verifica se a Fase 1 (índice 0) tem pelo menos 1 estrela
            if (PlayerPrefs.GetInt(fases[0].idFase + "_Estrelas", 0) < 1)
            {
                Debug.Log("Fase 2 Bloqueada! Complete a Fase 1 com pelo menos 1 estrela.");
                return; // Impede o carregamento
            }
        }
        else if (currentIndex == 2) // Fase 3
        {
            // Verifica se a Fase 2 (índice 1) tem pelo menos 1 estrela
            if (PlayerPrefs.GetInt(fases[1].idFase + "_Estrelas", 0) < 1)
            {
                Debug.Log("Fase 3 Bloqueada! Complete a Fase 2 com pelo menos 1 estrela.");
                return;
            }
        }
        else if (currentIndex == 3) // Fase Extra (Fase 4)
        {
            // Fase extra exige 3 estrelas nas fases 1, 2 e 3
            int estrelasFase1 = PlayerPrefs.GetInt(fases[0].idFase + "_Estrelas", 0);
            int estrelasFase2 = PlayerPrefs.GetInt(fases[1].idFase + "_Estrelas", 0);
            int estrelasFase3 = PlayerPrefs.GetInt(fases[2].idFase + "_Estrelas", 0);

            if (estrelasFase1 < 3 || estrelasFase2 < 3 || estrelasFase3 < 3)
            {
                Debug.Log("Fase Extra Bloqueada! Você precisa de 3 estrelas nas 3 primeiras fases.");
                return; 
            }
        }

        // Se passou pelos bloqueios, inicia a transição
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

        // Carrega a cena real usando o novo campo 'nomeDaCena'
        Debug.Log("Carregando cena: " + fases[currentIndex].nomeDaCena);
        SceneManager.LoadScene(fases[currentIndex].nomeDaCena);
    }
}