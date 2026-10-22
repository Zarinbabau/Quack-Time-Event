using System.Collections;
using UnityEngine;

public class LevelSelectionMenu : MonoBehaviour
{
    [Tooltip("Arraste os GameObjects das Fotos na ordem desejada.")]
    public Transform[] fotos;
    
    [Tooltip("Referência da câmera que fará o movimento.")]
    public Camera mainCamera;
    
    [Tooltip("Distância final da câmera em relação à foto ao dar o zoom.")]
    public float zoomOffsetZ = -1.5f;
    public float velocidadeZoom = 2f;

    private int currentIndex = 0;
    private bool inputsBloqueados = false;

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        AtualizarSelecaoVisual();
    }

    private void Update()
    {
        // Se a animação de entrar na fase começou, ignora qualquer input
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
        
        // Mantém a seleção dentro dos limites do array
        if (currentIndex < 0) currentIndex = 0;
        if (currentIndex >= fotos.Length) currentIndex = fotos.Length - 1;

        AtualizarSelecaoVisual();
    }

    private void AtualizarSelecaoVisual()
    {
        for (int i = 0; i < fotos.Length; i++)
        {
            // O índice 1 no GetChild representa o segundo filho na hierarquia do Unity
            if (fotos[i].childCount > 1)
            {
                bool isSelecionada = (i == currentIndex);
                fotos[i].GetChild(1).gameObject.SetActive(isSelecionada);
            }
            else
            {
                Debug.LogWarning($"A foto {fotos[i].name} não possui um segundo filho para ativar.");
            }
        }
    }

    private void ConfirmarFase()
    {
        inputsBloqueados = true;
        StartCoroutine(ZoomCameraRotina());
    }

    private IEnumerator ZoomCameraRotina()
    {
        Transform fotoAlvo = fotos[currentIndex];
        Vector3 posicaoInicial = mainCamera.transform.position;
        
        // Define o alvo mantendo o X e Y da foto, mas recuando no Z para não atravessar a imagem
        Vector3 posicaoAlvo = new Vector3(fotoAlvo.position.x, fotoAlvo.position.y, fotoAlvo.position.z + zoomOffsetZ);

        float progresso = 0f;

        while (progresso < 1f)
        {
            progresso += Time.deltaTime * velocidadeZoom;
            
            // Interpolação suave (Lerp) com SmoothStep para um movimento mais fluido
            mainCamera.transform.position = Vector3.Lerp(posicaoInicial, posicaoAlvo, Mathf.SmoothStep(0, 1, progresso));
            yield return null;
        }

        Debug.Log("Zoom concluído. Carregando cena da fase: " + fotoAlvo.name);
        // Coloque aqui o código para carregar a cena real, por exemplo:
        // UnityEngine.SceneManagement.SceneManager.LoadScene(fotoAlvo.name);
    }
}