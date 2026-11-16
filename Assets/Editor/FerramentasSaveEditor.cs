#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FerramentasSaveEditor
{
    [MenuItem("Tools/Estrelas e Modos/1. Dar 3 Estrelas - Modo Básico")]
    public static void CompletarModoBasico()
    {
        AplicarEstrelasNoModo(0, 1, "Modo Básico");
    }

    [MenuItem("Tools/Estrelas e Modos/2. Dar 3 Estrelas - Modo Temporal")]
    public static void CompletarModoTemporal()
    {
        AplicarEstrelasNoModo(1, 2, "Modo Temporal");
    }

    [MenuItem("Tools/Estrelas e Modos/3. Dar 3 Estrelas - Modo PATO PUNK")]
    public static void CompletarModoPatoPunk()
    {
        AplicarEstrelasNoModo(2, 2, "Modo PATO PUNK");
    }

    [MenuItem("Tools/Estrelas e Modos/4. Dar 3 Estrelas - TODOS os Modos")]
    public static void CompletarTodosOsModos()
    {
        for (int dif = 0; dif <= 2; dif++)
        {
            foreach (string idFase in DifficultyManager.IDS_FASES)
            {
                string chave = DifficultyManager.ObterChaveEstrelas(idFase, dif);
                PlayerPrefs.SetInt(chave, 3);
            }
            PlayerPrefs.SetInt($"Trofeu_{dif}_Desbloqueado", 1);
        }

        DifficultyManager.DificuldadeMaximaDesbloqueada = 2;
        PlayerPrefs.Save();

        FinalizarAcao("3 estrelas aplicadas em TODAS as fases dos 3 modos e troféus desbloqueados!");
    }

    [MenuItem("Tools/Estrelas e Modos/5. Zerar Todo o Save (Resetar)")]
    public static void ZerarSaveCompleto()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        FinalizarAcao("Todo o progresso (estrelas, modos e troféus) foi zerado!");
    }

    private static void AplicarEstrelasNoModo(int dificuldade, int proximaDificuldadeLiberada, string nomeModo)
    {
        foreach (string idFase in DifficultyManager.IDS_FASES)
        {
            string chave = DifficultyManager.ObterChaveEstrelas(idFase, dificuldade);
            PlayerPrefs.SetInt(chave, 3);
        }

        PlayerPrefs.SetInt($"Trofeu_{dificuldade}_Desbloqueado", 1);
        DifficultyManager.DificuldadeMaximaDesbloqueada = Mathf.Max(
            DifficultyManager.DificuldadeMaximaDesbloqueada,
            proximaDificuldadeLiberada
        );
        PlayerPrefs.Save();

        FinalizarAcao($"3 estrelas aplicadas em todas as fases do {nomeModo}!");
    }

    private static void FinalizarAcao(string mensagem)
    {
        Debug.Log($"[Ferramentas] {mensagem}");

        // Se clicar no botão com o jogo rodando, recarrega a cena para atualizar as estrelas/troféus na hora
        if (Application.isPlaying)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
#endif