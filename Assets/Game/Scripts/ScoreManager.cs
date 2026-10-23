using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Identificação para o Save")]
    [Tooltip("Ex: Fase1, Fase2, Fase3. Deve ser único para cada cena.")]
    public string idFase = "Fase1"; 

    [Header("Configuração de Pontos")]
    public int pontosBase = 100;

    [Header("Limites para Estrelas")]
    [Tooltip("Pontos necessários para ganhar 1 estrela.")]
    public int pontosPara1Estrela = 1000;
    [Tooltip("Pontos necessários para ganhar 2 estrelas.")]
    public int pontosPara2Estrelas = 1500;
    [Tooltip("Pontos necessários para ganhar 3 estrelas.")]
    public int pontosPara3Estrelas = 1800; 

    private int pontuacaoTotal = 0;

    public void AdicionarPontos(float tempoGasto, float tempoLimite)
    {
        // Multiplicador vai de 1x (no limite) a 2x (instantâneo)
        float tempoRestante = tempoLimite - tempoGasto;
        float multiplicador = 1f + (tempoRestante / tempoLimite);
        
        int pontosGanhos = Mathf.RoundToInt(pontosBase * multiplicador);
        pontuacaoTotal += pontosGanhos;

        Debug.Log($"Acertou! Tempo: {tempoGasto:F2}s | Mult: {multiplicador:F2}x | Pontos Ganhos: {pontosGanhos} | Total: {pontuacaoTotal}");
    }

    public void FinalizarFaseECalcularEstrelas()
    {
        int estrelas = 0;

        if (pontuacaoTotal >= pontosPara3Estrelas) estrelas = 3;
        else if (pontuacaoTotal >= pontosPara2Estrelas) estrelas = 2;
        else if (pontuacaoTotal >= pontosPara1Estrela) estrelas = 1;

        // Salva apenas se o jogador conseguiu uma pontuação de estrelas maior que a anterior
        int estrelasSalvas = PlayerPrefs.GetInt(idFase + "_Estrelas", 0);
        if (estrelas > estrelasSalvas)
        {
            PlayerPrefs.SetInt(idFase + "_Estrelas", estrelas);
            PlayerPrefs.Save();
            Debug.Log($"Novo Recorde! Você salvou {estrelas} estrela(s) na {idFase}.");
        }
        else
        {
            Debug.Log($"Fase concluída com {estrelas} estrela(s). Recorde atual mantido em {estrelasSalvas}.");
        }
    }
}