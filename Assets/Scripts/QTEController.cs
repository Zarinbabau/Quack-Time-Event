using System.Collections;
using UnityEngine;

public class QTEController : MonoBehaviour
{
    public enum QTEAction { Esquerda, Direita, Cima, Baixo, Acao }

    [Header("Sprite Renderers")]
    [SerializeField] private SpriteRenderer characterSpriteRenderer;
    [SerializeField] private SpriteRenderer keySpriteRenderer;

    [Header("Sprites do Personagem")]
    [SerializeField] private Sprite spriteIdle;
    [SerializeField] private Sprite spriteEsquerda;
    [SerializeField] private Sprite spriteDireita;
    [SerializeField] private Sprite spriteCima;
    [SerializeField] private Sprite spriteBaixo;
    [SerializeField] private Sprite spriteAcao;
    [SerializeField] private Sprite spriteTriste;

    [Header("Sprites das Teclas")]
    [SerializeField] private Sprite spriteKeyEsquerda;
    [SerializeField] private Sprite spriteKeyDireita;
    [SerializeField] private Sprite spriteKeyCima;
    [SerializeField] private Sprite spriteKeyBaixo;
    [SerializeField] private Sprite spriteKeyAcao;

    [Header("Configurações do QTE")]
    [SerializeField] private float tempoParaResponder = 2.0f; // tempo Y para acertar
    private const int TOTAL_ACERTOS_OBJETIVO = 10;

    private int acertosAtuais = 0;
    private bool jogoAtivo = false;
    private bool inputRecebido = false;
    private QTEAction acaoAtual;

    private void Start()
    {
        IniciarJogo();
    }

    public void IniciarJogo()
    {
        StartCoroutine(RotinaInicio());
    }

    private IEnumerator RotinaInicio()
    {
        characterSpriteRenderer.sprite = spriteIdle;
        keySpriteRenderer.sprite = null;
        acertosAtuais = 0;
        jogoAtivo = false;

        Debug.Log("O jogo começará em 3 segundos...");
        yield return new WaitForSeconds(3.0f);

        jogoAtivo = true;
        StartCoroutine(RotinaQTE());
    }

    private IEnumerator RotinaQTE()
    {
        while (jogoAtivo && acertosAtuais < TOTAL_ACERTOS_OBJETIVO)
        {
            // Volta personagem para Idle
            characterSpriteRenderer.sprite = spriteIdle;

            // Sortear uma das 5 ações válidas
            acaoAtual = (QTEAction)Random.Range(0, 5);
            AtualizarSpriteTecla(acaoAtual);

            inputRecebido = false;
            bool acertou = false;
            float tempoDecorrido = 0f;

            // Aguarda entrada do jogador até o limite de tempo (Y)
            while (tempoDecorrido < tempoParaResponder && !inputRecebido)
            {
                tempoDecorrido += Time.deltaTime;

                if (ChecarInputIncorreto())
                {
                    inputRecebido = true;
                    acertou = false;
                    break;
                }

                if (ChecarInputCorreto(acaoAtual))
                {
                    inputRecebido = true;
                    acertou = true;
                    break;
                }

                yield return null;
            }

            // Esconde a tecla após a tentativa
            keySpriteRenderer.sprite = null;

            if (acertou && inputRecebido)
            {
                acertosAtuais++;
                Debug.Log($"Acertou! Progresso: {acertosAtuais}/{TOTAL_ACERTOS_OBJETIVO}");
                characterSpriteRenderer.sprite = ObterSpriteAcao(acaoAtual);
            }
            else
            {
                Debug.Log("Errou ou tempo esgotado!");
                characterSpriteRenderer.sprite = spriteTriste;
            }

            // Espera meio segundo antes de desativar a imagem e/ou avançar
            yield return new WaitForSeconds(0.5f);

            if (acertosAtuais >= TOTAL_ACERTOS_OBJETIVO)
            {
                FinalizarJogo();
                yield break;
            }

            // Retorna ao Idle antes de sortear o próximo
            characterSpriteRenderer.sprite = spriteIdle;
        }
    }

    private void AtualizarSpriteTecla(QTEAction acao)
    {
        switch (acao)
        {
            case QTEAction.Esquerda: keySpriteRenderer.sprite = spriteKeyEsquerda; break;
            case QTEAction.Direita:  keySpriteRenderer.sprite = spriteKeyDireita;  break;
            case QTEAction.Cima:     keySpriteRenderer.sprite = spriteKeyCima;     break;
            case QTEAction.Baixo:    keySpriteRenderer.sprite = spriteKeyBaixo;    break;
            case QTEAction.Acao:     keySpriteRenderer.sprite = spriteKeyAcao;     break;
        }
    }

    private Sprite ObterSpriteAcao(QTEAction acao)
    {
        return acao switch
        {
            QTEAction.Esquerda => spriteEsquerda,
            QTEAction.Direita  => spriteDireita,
            QTEAction.Cima     => spriteCima,
            QTEAction.Baixo    => spriteBaixo,
            QTEAction.Acao     => spriteAcao,
            _                  => spriteIdle,
        };
    }

    private bool ChecarInputCorreto(QTEAction acao)
    {
        return acao switch
        {
            QTEAction.Esquerda => Input.GetKeyDown(KeyCode.A),
            QTEAction.Direita  => Input.GetKeyDown(KeyCode.D),
            QTEAction.Cima     => Input.GetKeyDown(KeyCode.W),
            QTEAction.Baixo    => Input.GetKeyDown(KeyCode.S),
            QTEAction.Acao     => Input.GetKeyDown(KeyCode.Space),
            _                  => false,
        };
    }

    private bool ChecarInputIncorreto()
    {
        // Se pressionou qualquer tecla de QTE diferente da esperada
        if (Input.GetKeyDown(KeyCode.A) && acaoAtual != QTEAction.Esquerda) return true;
        if (Input.GetKeyDown(KeyCode.D) && acaoAtual != QTEAction.Direita)  return true;
        if (Input.GetKeyDown(KeyCode.W) && acaoAtual != QTEAction.Cima)     return true;
        if (Input.GetKeyDown(KeyCode.S) && acaoAtual != QTEAction.Baixo)    return true;
        if (Input.GetKeyDown(KeyCode.Space) && acaoAtual != QTEAction.Acao) return true;

        return false;
    }

    public void FinalizarJogo()
    {
        jogoAtivo = false;
        keySpriteRenderer.sprite = null;
        characterSpriteRenderer.sprite = spriteIdle;
        Debug.Log("Parabéns! Você completou os 10 acertos da fase!");
    }
}