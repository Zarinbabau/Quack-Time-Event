using UnityEngine;

[System.Serializable]
public class ConfigFaseBase
{
    [Tooltip("Quantidade de QTEs corretos para encher a barra e vencer a fase")]
    public int acertosObjetivo = 25;

    [Header("Meta de Pontos (Estrelas)")]
    public int pontosPara1Estrela = 500;
    public int pontosPara2Estrelas = 1200;
    public int pontosPara3Estrelas = 2000;
}

[System.Serializable]
public class ConfigFasePadrao : ConfigFaseBase
{
    [Tooltip("Tempo em segundos para o jogador apertar a tecla")]
    public float tempoParaResponder = 2.0f;
}

[System.Serializable]
public class ConfigFaseExtra : ConfigFaseBase
{
    [Tooltip("Tempo em segundos quando aparecer 1 tecla simples")]
    public float tempoSimples = 2.0f;
    [Tooltip("Tempo em segundos quando aparecer a tecla de 3 toques")]
    public float tempoRepeticao = 3.0f;
    [Tooltip("Tempo em segundos quando aparecer a sequência de 3 teclas")]
    public float tempoSequencia = 4.0f;
}

[System.Serializable]
public class ConfiguracaoModo
{
    public string nomeDoModo = "Modo Básico";
    [Range(1, 3)] public int maxVidas = 3;

    [Header("Configurações por Fase")]
    public ConfigFasePadrao fase1 = new ConfigFasePadrao { tempoParaResponder = 2.5f, acertosObjetivo = 15, pontosPara1Estrela = 300, pontosPara2Estrelas = 700, pontosPara3Estrelas = 1200 };
    public ConfigFasePadrao fase2 = new ConfigFasePadrao { tempoParaResponder = 3.5f, acertosObjetivo = 15, pontosPara1Estrela = 300, pontosPara2Estrelas = 700, pontosPara3Estrelas = 1200 };
    public ConfigFasePadrao fase3 = new ConfigFasePadrao { tempoParaResponder = 4.5f, acertosObjetivo = 15, pontosPara1Estrela = 300, pontosPara2Estrelas = 700, pontosPara3Estrelas = 1200 };
    public ConfigFaseExtra faseExtra = new ConfigFaseExtra { tempoSimples = 2.5f, tempoRepeticao = 3.5f, tempoSequencia = 4.5f, acertosObjetivo = 15, pontosPara1Estrela = 300, pontosPara2Estrelas = 700, pontosPara3Estrelas = 1200 };
}

[CreateAssetMenu(fileName = "DifficultyConfig", menuName = "Configuracoes/Dificuldade do Jogo")]
public class DifficultyConfig : ScriptableObject
{
    [Header("1. Modo Básico")]
    public ConfiguracaoModo modoBasico = new ConfiguracaoModo
    {
        nomeDoModo = "Modo Básico",
        maxVidas = 3
    };

    [Header("2. Modo Rápido")]
    public ConfiguracaoModo modoRapido = new ConfiguracaoModo
    {
        nomeDoModo = "Modo Rápido",
        maxVidas = 3,
        fase1 = new ConfigFasePadrao { tempoParaResponder = 1.8f, acertosObjetivo = 25, pontosPara1Estrela = 600, pontosPara2Estrelas = 1400, pontosPara3Estrelas = 2200 },
        fase2 = new ConfigFasePadrao { tempoParaResponder = 2.7f, acertosObjetivo = 25, pontosPara1Estrela = 600, pontosPara2Estrelas = 1400, pontosPara3Estrelas = 2200 },
        fase3 = new ConfigFasePadrao { tempoParaResponder = 3.6f, acertosObjetivo = 25, pontosPara1Estrela = 600, pontosPara2Estrelas = 1400, pontosPara3Estrelas = 2200 },
        faseExtra = new ConfigFaseExtra { tempoSimples = 1.8f, tempoRepeticao = 2.7f, tempoSequencia = 3.6f, acertosObjetivo = 25, pontosPara1Estrela = 600, pontosPara2Estrelas = 1400, pontosPara3Estrelas = 2200 }
    };

    [Header("3. Modo Mestre")]
    public ConfiguracaoModo modoMestre = new ConfiguracaoModo
    {
        nomeDoModo = "Modo Mestre",
        maxVidas = 2,
        fase1 = new ConfigFasePadrao { tempoParaResponder = 1.3f, acertosObjetivo = 35, pontosPara1Estrela = 1000, pontosPara2Estrelas = 2200, pontosPara3Estrelas = 3500 },
        fase2 = new ConfigFasePadrao { tempoParaResponder = 2.0f, acertosObjetivo = 35, pontosPara1Estrela = 1000, pontosPara2Estrelas = 2200, pontosPara3Estrelas = 3500 },
        fase3 = new ConfigFasePadrao { tempoParaResponder = 2.6f, acertosObjetivo = 35, pontosPara1Estrela = 1000, pontosPara2Estrelas = 2200, pontosPara3Estrelas = 3500 },
        faseExtra = new ConfigFaseExtra { tempoSimples = 1.3f, tempoRepeticao = 2.0f, tempoSequencia = 2.6f, acertosObjetivo = 35, pontosPara1Estrela = 1000, pontosPara2Estrelas = 2200, pontosPara3Estrelas = 3500 }
    };
}