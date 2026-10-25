using UnityEngine;
using UnityEditor; // Necessário para criar botões no editor

public class CheatSave
{
    // Adiciona o botão no menu superior da Unity
    [MenuItem("Ferramentas/Dar 3 Estrelas (Fases 1 a 3)")]
    public static void DarTresEstrelas()
    {
        // Define o valor 3 para as chaves exatas que configuramos no código
        PlayerPrefs.SetInt("Fase1_Estrelas", 3);
        PlayerPrefs.SetInt("Fase2_Estrelas", 3);
        PlayerPrefs.SetInt("Fase3_Estrelas", 3);
        
        // Salva as alterações forçadamente
        PlayerPrefs.Save(); 
        
        Debug.Log("Sucesso! Você agora tem 3 estrelas nas Fases 1, 2 e 3. A Fase Extra deve estar liberada!");
    }
}