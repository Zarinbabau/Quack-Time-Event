using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Identificação para o Save")]
    public string idFase = "Fase1"; 

    [Header("Configuração de Pontos")]
    public int pontosBase = 100;

    [Header("Limites para Estrelas")]
    public int pontosPara1Estrela = 1000;
    public int pontosPara2Estrelas = 1500;
    public int pontosPara3Estrelas = 1800; 

    private int pontuacaoTotal = 0;

    public void AdicionarPontos(float tempoGasto, float tempoLimite)
    {
        float tempoRestante = tempoLimite - tempoGasto;
        float multiplicador = 1f + (tempoRestante / tempoLimite);
        
        int pontosGanhos = Mathf.RoundToInt(pontosBase * multiplicador);
        pontuacaoTotal += pontosGanhos;

        Debug.Log($"[ACERTO] Tempo: {tempoGasto:F2}s | Mult: {multiplicador:F2}x | Ganhou: {pontosGanhos} | Total: {pontuacaoTotal}");
    }

    public void RemoverPontos(float tempoGasto, float tempoLimite)
    {
        float tempoRestante = tempoLimite - tempoGasto;
        // Se o tempo esgotou, tempoRestante será menor ou igual a 0, então travamos em 0 para não dar erro
        if (tempoRestante < 0) tempoRestante = 0; 
        
        float multiplicador = 1f + (tempoRestante / tempoLimite);
        
        int pontosPerdidos = Mathf.RoundToInt(pontosBase * multiplicador);
        pontuacaoTotal -= pontosPerdidos;

        // Impede que a pontuação fique negativa
        if (pontuacaoTotal < 0) pontuacaoTotal = 0;

        Debug.Log($"[ERRO] Tempo: {tempoGasto:F2}s | Mult: {multiplicador:F2}x | Perdeu: {pontosPerdidos} | Total: {pontuacaoTotal}");
    }

    public void FinalizarFaseECalcularEstrelas()
    {
        int estrelas = 0;

        if (pontuacaoTotal >= pontosPara3Estrelas) estrelas = 3;
        else if (pontuacaoTotal >= pontosPara2Estrelas) estrelas = 2;
        else if (pontuacaoTotal >= pontosPara1Estrela) estrelas = 1;

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