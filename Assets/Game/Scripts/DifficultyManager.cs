using UnityEngine;

public static class DifficultyManager
{
    public static readonly string[] IDS_FASES = { "Fase1", "Fase2", "Fase3", "FaseExtra" };

    private static DifficultyConfig _config;
    public static DifficultyConfig Config
    {
        get
        {
            if (_config == null)
            {
                _config = Resources.Load<DifficultyConfig>("DifficultyConfig");
                if (_config == null)
                {
                    Debug.LogWarning("Arquivo 'DifficultyConfig' não encontrado em Assets/Resources! Usando valores padrão.");
                    _config = ScriptableObject.CreateInstance<DifficultyConfig>();
                }
            }
            return _config;
        }
    }

    public static int DificuldadeAtual
    {
        get => PlayerPrefs.GetInt("DificuldadeAtual", 0);
        set
        {
            PlayerPrefs.SetInt("DificuldadeAtual", Mathf.Clamp(value, 0, 2));
            PlayerPrefs.Save();
        }
    }

    public static int DificuldadeMaximaDesbloqueada
    {
        get => PlayerPrefs.GetInt("DificuldadeMaximaDesbloqueada", 0);
        set
        {
            PlayerPrefs.SetInt("DificuldadeMaximaDesbloqueada", Mathf.Clamp(value, 0, 2));
            PlayerPrefs.Save();
        }
    }

    public static ConfiguracaoModo ObterModo(int nivel = -1)
    {
        int dif = (nivel == -1) ? DificuldadeAtual : nivel;
        switch (dif)
        {
            case 0: return Config.modoBasico;
            case 1: return Config.modoRapido;
            case 2: return Config.modoMestre;
            default: return Config.modoBasico;
        }
    }

    public static string ObterNomeDificuldade(int nivel)
    {
        return ObterModo(nivel).nomeDoModo;
    }

    public static int ObterMaxVidas()
    {
        return ObterModo().maxVidas;
    }

    public static ConfigFasePadrao ObterConfigFasePadrao(string idFase)
    {
        ConfiguracaoModo modo = ObterModo();
        switch (idFase)
        {
            case "Fase1": return modo.fase1;
            case "Fase2": return modo.fase2;
            case "Fase3": return modo.fase3;
            default: return modo.fase1;
        }
    }

    public static ConfigFaseExtra ObterConfigFaseExtra()
    {
        return ObterModo().faseExtra;
    }

    public static ConfigFaseBase ObterConfigGeralFase(string idFase)
    {
        if (idFase == "FaseExtra") return ObterConfigFaseExtra();
        return ObterConfigFasePadrao(idFase);
    }

    // --- SALVAMENTO E VERIFICAÇÃO DE TROFÉUS ---

    public static string ObterChaveEstrelas(string idFase, int dificuldade = -1)
    {
        int dif = (dificuldade == -1) ? DificuldadeAtual : dificuldade;
        return $"Dif{dif}_{idFase}_Estrelas";
    }

    public static int ObterEstrelasFase(string idFase, int dificuldade = -1)
    {
        return PlayerPrefs.GetInt(ObterChaveEstrelas(idFase, dificuldade), 0);
    }

    public static int ObterTotalEstrelasNoModo(int dificuldade)
    {
        int total = 0;
        foreach (string id in IDS_FASES)
        {
            total += ObterEstrelasFase(id, dificuldade);
        }
        return total;
    }

    public static bool TrofeuDesbloqueado(int indexTrofeu)
    {
        return PlayerPrefs.GetInt($"Trofeu_{indexTrofeu}_Desbloqueado", 0) == 1;
    }

    public static bool VerificarDesbloqueioTrofeuAtual(out string mensagemDesbloqueio)
    {
        mensagemDesbloqueio = "";
        int dif = DificuldadeAtual;

        if (TrofeuDesbloqueado(dif)) return false;

        if (ObterTotalEstrelasNoModo(dif) >= 12)
        {
            PlayerPrefs.SetInt($"Trofeu_{dif}_Desbloqueado", 1);

            if (dif == 0)
            {
                DificuldadeMaximaDesbloqueada = Mathf.Max(DificuldadeMaximaDesbloqueada, 1);
                mensagemDesbloqueio = $"Troféu Desbloqueado!\nVocê completou o {ObterNomeDificuldade(0)} com 100% e liberou o {ObterNomeDificuldade(1)}!";
            }
            else if (dif == 1)
            {
                DificuldadeMaximaDesbloqueada = Mathf.Max(DificuldadeMaximaDesbloqueada, 2);
                mensagemDesbloqueio = $"Troféu Desbloqueado!\nVocê dominou o {ObterNomeDificuldade(1)} e liberou o {ObterNomeDificuldade(2)}!";
            }
            else if (dif == 2)
            {
                mensagemDesbloqueio = $"Troféu Final Desbloqueado!\nParabéns! Você conquistou 100% do jogo no {ObterNomeDificuldade(2)}!";
            }

            PlayerPrefs.Save();
            return true;
        }

        return false;
    }
}