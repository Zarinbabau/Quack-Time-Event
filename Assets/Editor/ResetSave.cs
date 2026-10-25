using UnityEngine;
using UnityEditor; // Necessário para criar botões no editor

public class ResetSave
{
    // Cria um item na barra superior da Unity
    [MenuItem("Ferramentas/Resetar Save (PlayerPrefs)")]
    public static void ResetarPlayerPrefs()
    {
        PlayerPrefs.DeleteAll(); // Apaga todas as chaves salvas
        Debug.Log("Todos os saves e estrelas foram resetados com sucesso!");
    }
}