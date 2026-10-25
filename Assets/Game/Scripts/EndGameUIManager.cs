using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndGameUIManager : MonoBehaviour
{
    [Header("Painel Principal")]
    public GameObject painelFimDeJogo;
    public Image fotoResultado;

    [Header("Botões")]
    public Button btnMenu;
    public Button btnRepetir;
    public Button btnProxima;

    [Header("Fotos/Sprites de Resultado")]
    public Sprite fotoDerrota;
    public Sprite foto1Estrela;
    public Sprite foto2Estrelas;
    public Sprite foto3Estrelas;

    [Header("Configuração de Transição")]
    public string nomeCenaMenu = "MenuScene"; // Coloque o nome real da cena do seu menu
    public int indiceProximaFaseNoMenu = 1;   // 0 = Fase1, 1 = Fase2, 2 = Fase3

    private void Start()
    {
        // Garante que a tela comece desativada
        painelFimDeJogo.SetActive(false);
    }

    public void MostrarTela(int estrelas)
    {
        painelFimDeJogo.SetActive(true);

        if (estrelas == 0) // Game Over
        {
            fotoResultado.sprite = fotoDerrota;
            btnProxima.gameObject.SetActive(false); // Remove o botão de próxima fase
        }
        else // Vitória
        {
            btnProxima.gameObject.SetActive(true);
            
            if (estrelas == 1) fotoResultado.sprite = foto1Estrela;
            else if (estrelas == 2) fotoResultado.sprite = foto2Estrelas;
            else if (estrelas == 3) fotoResultado.sprite = foto3Estrelas;
        }
    }

    // --- FUNÇÕES DOS BOTÕES ---

    public void VoltarAoMenu()
    {
        SceneManager.LoadScene(nomeCenaMenu);
    }

    public void RepetirFase()
    {
        // Recarrega a cena atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }

    public void ProximaFase()
    {
        // Salva uma "carta" para o Menu avisando para animar a ida até a próxima fase
        PlayerPrefs.SetInt("AutoProgression_ToIndex", indiceProximaFaseNoMenu);
        PlayerPrefs.Save();
        
        SceneManager.LoadScene(nomeCenaMenu);
    }
}