using UnityEngine;
using TMPro;

public class UIExibirErros : MonoBehaviour
{
    public TextMeshProUGUI textoDeErros;

    // Referência para o outro script
    public QTEController QTEController;

    void Update()
    {
        // Lê diretamente a variável pública 'errosAtuais' do outro script a cada frame
        if (QTEController != null)
        {
            textoDeErros.text = "Erros: " + QTEController.errosAtuais;
        }
    }
}