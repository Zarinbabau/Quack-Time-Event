using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class QTEControllerFase2 : MonoBehaviour
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

    [Header("Sprites de Progresso da Tecla (3 Estágios)")]
    [Tooltip("Sprite inicial (0 cliques), após 1 clique e após 2 cliques para Esquerda (A)")]
    [SerializeField] private Sprite[] keySpritesEsquerda = new Sprite[3];
    
    [Tooltip("Sprite inicial, estágio 1 e estágio 2 para Direita (D)")]
    [SerializeField] private Sprite[] keySpritesDireita = new Sprite[3];
    
    [Tooltip("Sprite inicial, estágio 1 e estágio 2 para Cima (W)")]
    [SerializeField] private Sprite[] keySpritesCima = new Sprite[3];
    
    [Tooltip("Sprite inicial, estágio 1 e estágio 2 para Baixo (S)")]
    [SerializeField] private Sprite[] keySpritesBaixo = new Sprite[3];
    
    [Tooltip("Sprite inicial, estágio 1 e estágio 2 para Ação (Espaço)")]
    [SerializeField] private Sprite[] keySpritesAcao = new Sprite[3];

    [Header("Áudio")]
    [SerializeField] private AudioClip soundCliqueProgresso; // Opcional: som leve ao acertar 1 toque
    [SerializeField] private AudioClip soundAcerto;          // Som ao completar os 3 toques
    [SerializeField] private AudioClip soundErro;            // Som de erro/tempo esgotado

    [Header("Configurações da Fase 2")]
    [SerializeField] private float tempoParaResponder = 3.0f; // Tempo total para dar os 3 toques
    private const int REPETICOES_NECESSARIAS = 3;
    private const int TOTAL_ACERTOS_OBJETIVO = 10;

    private int acertosAtuais = 0;
    private bool jogoAtivo = false;
    private QTEAction acaoAtual;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

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

        Debug.Log("Iniciando Fase 2 em 3 segundos...");
        yield return new WaitForSeconds(3.0f);

        jogoAtivo = true;
        StartCoroutine(RotinaQTE());
    }

    private IEnumerator RotinaQTE()
    {
        while (jogoAtivo && acertosAtuais < TOTAL_ACERTOS_OBJETIVO)
        {
            characterSpriteRenderer.sprite = spriteIdle;

            // Sortear a ação da rodada
            acaoAtual = (QTEAction)Random.Range(0, 5);

            int toquesConcluidos = 0;
            AtualizarSpriteTecla(acaoAtual, toquesConcluidos);

            float tempoDecorrido = 0f;
            bool errou = false;

            // Loop de tempo limite para realizar os 3 toques
            while (tempoDecorrido < tempoParaResponder && toquesConcluidos < REPETICOES_NECESSARIAS)
            {
                tempoDecorrido += Time.deltaTime;

                // Checa se apertou a tecla incorreta
                if (ChecarInputIncorreto())
                {
                    errou = true;
                    break;
                }

                // Checa se apertou a tecla correta
                if (ChecarInputCorreto(acaoAtual))
                {
                    toquesConcluidos++;

                    if (toquesConcluidos < REPETICOES_NECESSARIAS)
                    {
                        // Atualiza o sprite para o próximo estágio
                        AtualizarSpriteTecla(acaoAtual, toquesConcluidos);
                        TocarSom(soundCliqueProgresso != null ? soundCliqueProgresso : soundAcerto);
                    }
                }

                yield return null;
            }

            // Oculta a indicação da tecla
            keySpriteRenderer.sprite = null;

            // Verificação de sucesso ou falha no evento
            if (!errou && toquesConcluidos >= REPETICOES_NECESSARIAS)
            {
                acertosAtuais++;
                Debug.Log($"Evento Concluído! Progresso da Fase: {acertosAtuais}/{TOTAL_ACERTOS_OBJETIVO}");
                
                characterSpriteRenderer.sprite = ObterSpriteAcao(acaoAtual);
                TocarSom(soundAcerto);
            }
            else
            {
                Debug.Log("Errou ou tempo esgotado!");
                characterSpriteRenderer.sprite = spriteTriste;
                TocarSom(soundErro);
            }

            // Espera meio segundo com o feedback visual na tela
            yield return new WaitForSeconds(0.7f);

            if (acertosAtuais >= TOTAL_ACERTOS_OBJETIVO)
            {
                FinalizarJogo();
                yield break;
            }

            // Retorna para Idle antes do próximo sorteio
            characterSpriteRenderer.sprite = spriteIdle;
        }
    }

    private void AtualizarSpriteTecla(QTEAction acao, int estagio)
    {
        // Limita o índice entre 0 e 2
        int index = Mathf.Clamp(estagio, 0, 2);

        keySpriteRenderer.sprite = acao switch
        {
            QTEAction.Esquerda => keySpritesEsquerda[index],
            QTEAction.Direita  => keySpritesDireita[index],
            QTEAction.Cima     => keySpritesCima[index],
            QTEAction.Baixo    => keySpritesBaixo[index],
            QTEAction.Acao     => keySpritesAcao[index],
            _                  => null,
        };
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
        if (Input.GetKeyDown(KeyCode.A) && acaoAtual != QTEAction.Esquerda) return true;
        if (Input.GetKeyDown(KeyCode.D) && acaoAtual != QTEAction.Direita)  return true;
        if (Input.GetKeyDown(KeyCode.W) && acaoAtual != QTEAction.Cima)     return true;
        if (Input.GetKeyDown(KeyCode.S) && acaoAtual != QTEAction.Baixo)    return true;
        if (Input.GetKeyDown(KeyCode.Space) && acaoAtual != QTEAction.Acao) return true;

        return false;
    }

    private void TocarSom(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void FinalizarJogo()
    {
        jogoAtivo = false;
        keySpriteRenderer.sprite = null;
        characterSpriteRenderer.sprite = spriteIdle;
        Debug.Log("Parabéns! Você concluiu a Fase 2!");
    }
}