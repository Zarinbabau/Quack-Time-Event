using UnityEngine;
using TMPro;

public class UIExibirErros : MonoBehaviour
{
    public TextMeshProUGUI textoDeErros;

    // Referência para o outro script
    public Fase1QTEController Fase1QTEController;

    void Update()
    {
        // Lê diretamente a variável pública 'errosAtuais' do outro script a cada frame
        if (Fase1QTEController != null)
        {
            textoDeErros.text = "Erros: " + Fase1QTEController.errosAtuais;
        }
    }
}