using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Fase3QTEController : MonoBehaviour
{
    public enum QTEAction { Esquerda, Direita, Cima, Baixo, Acao }

    [Header("Sprite Renderers do Personagem")]
    [SerializeField] private SpriteRenderer characterSpriteRenderer;

    [Header("Sprite Renderers das 3 Teclas")]
    [Tooltip("Arraste aqui 3 Sprite Renderers da sua cena (Ex: Tecla1, Tecla2, Tecla3)")]
    [SerializeField] private SpriteRenderer[] keySpriteRenderers = new SpriteRenderer[3];

    [Header("Sprites do Personagem")]
    [SerializeField] private Sprite spriteIdle;
    [SerializeField] private Sprite spriteEsquerda;
    [SerializeField] private Sprite spriteDireita;
    [SerializeField] private Sprite spriteCima;
    [SerializeField] private Sprite spriteBaixo;
    [SerializeField] private Sprite spriteAcao;
    [SerializeField] private Sprite spriteTriste;

    [Header("Sprites das Teclas Indicadoras")]
    [SerializeField] private Sprite spriteKeyEsquerda;
    [SerializeField] private Sprite spriteKeyDireita;
    [SerializeField] private Sprite spriteKeyCima;
    [SerializeField] private Sprite spriteKeyBaixo;
    [SerializeField] private Sprite spriteKeyAcao;

    [Header("Áudio")]
    [SerializeField] private AudioClip soundCliqueProgresso;
    [SerializeField] private AudioClip soundAcerto;
    [SerializeField] private AudioClip soundErro;

    [Header("Configurações da Fase 3")]
    [SerializeField] private float tempoParaResponder = 4.0f; // Tempo total para toda a sequência
    private const int TAMANHO_SEQUENCIA = 3;
    private const int TOTAL_ACERTOS_OBJETIVO = 10;

    private int acertosAtuais = 0;
    private bool jogoAtivo = false;
    private QTEAction[] sequenciaAtual = new QTEAction[TAMANHO_SEQUENCIA];
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
        LimparSpritesTeclas();
        acertosAtuais = 0;
        jogoAtivo = false;

        Debug.Log("Iniciando Fase 3 em 3 segundos...");
        yield return new WaitForSeconds(3.0f);

        jogoAtivo = true;
        StartCoroutine(RotinaQTE());
    }

    private IEnumerator RotinaQTE()
    {
        while (jogoAtivo && acertosAtuais < TOTAL_ACERTOS_OBJETIVO)
        {
            characterSpriteRenderer.sprite = spriteIdle;

            // 1. Sorteia e exibe as 3 teclas simultaneamente
            GerarSequenciaAleatoria();
            ExibirTodasAsTeclas();

            int indicePassoAtual = 0;
            AtualizarDestaqueTeclas(indicePassoAtual);

            float tempoDecorrido = 0f;
            bool errou = false;

            // 2. Loop de tempo para aguardar o jogador apertar na ordem correta
            while (tempoDecorrido < tempoParaResponder && indicePassoAtual < TAMANHO_SEQUENCIA)
            {
                tempoDecorrido += Time.deltaTime;
                QTEAction acaoEsperada = sequenciaAtual[indicePassoAtual];

                // Checa se o jogador apertou a tecla incorreta da rodada
                if (ChecarInputIncorreto(acaoEsperada))
                {
                    errou = true;
                    break;
                }

                // Checa se o jogador acertou a tecla esperada do passo atual
                if (ChecarInputCorreto(acaoEsperada))
                {
                    indicePassoAtual++;

                    if (indicePassoAtual < TAMANHO_SEQUENCIA)
                    {
                        // Atualiza qual tecla está ativa no momento (as já acertadas ficam apagadas)
                        AtualizarDestaqueTeclas(indicePassoAtual);
                        TocarSom(soundCliqueProgresso != null ? soundCliqueProgresso : soundAcerto);
                    }
                }

                yield return null;
            }

            LimparSpritesTeclas();

            // 3. Resultado do evento
            if (!errou && indicePassoAtual >= TAMANHO_SEQUENCIA)
            {
                acertosAtuais++;
                Debug.Log($"Sequência concluída! Progresso: {acertosAtuais}/{TOTAL_ACERTOS_OBJETIVO}");

                characterSpriteRenderer.sprite = ObterSpriteAcao(sequenciaAtual[TAMANHO_SEQUENCIA - 1]);
                TocarSom(soundAcerto);
            }
            else
            {
                Debug.Log("Errou a sequência ou o tempo esgotou!");
                characterSpriteRenderer.sprite = spriteTriste;
                TocarSom(soundErro);
            }

            // Espera meio segundo antes de voltar ao Idle
            yield return new WaitForSeconds(0.5f);

            if (acertosAtuais >= TOTAL_ACERTOS_OBJETIVO)
            {
                FinalizarJogo();
                yield break;
            }

            characterSpriteRenderer.sprite = spriteIdle;
        }
    }

    private void GerarSequenciaAleatoria()
    {
        for (int i = 0; i < TAMANHO_SEQUENCIA; i++)
        {
            sequenciaAtual[i] = (QTEAction)Random.Range(0, 5);
        }
    }

    private void ExibirTodasAsTeclas()
    {
        for (int i = 0; i < TAMANHO_SEQUENCIA; i++)
        {
            if (keySpriteRenderers[i] != null)
            {
                keySpriteRenderers[i].sprite = ObterSpriteTecla(sequenciaAtual[i]);
                keySpriteRenderers[i].gameObject.SetActive(true);
            }
        }
    }

    private void AtualizarDestaqueTeclas(int indiceAtual)
    {
        for (int i = 0; i < TAMANHO_SEQUENCIA; i++)
        {
            if (keySpriteRenderers[i] == null) continue;

            Color cor = keySpriteRenderers[i].color;

            if (i < indiceAtual)
            {
                // Teclas já acertadas: ficam com opacidade bem baixa
                cor.a = 0.15f;
            }
            else if (i == indiceAtual)
            {
                // Tecla que DEVE ser apertada agora: opacidade e brilho total
                cor.a = 1.0f;
            }
            else
            {
                // Teclas futuras na sequência: semitransparentes
                cor.a = 0.4f;
            }

            keySpriteRenderers[i].color = cor;
        }
    }

    private void LimparSpritesTeclas()
    {
        for (int i = 0; i < TAMANHO_SEQUENCIA; i++)
        {
            if (keySpriteRenderers[i] != null)
            {
                keySpriteRenderers[i].sprite = null;
            }
        }
    }

    private Sprite ObterSpriteTecla(QTEAction acao)
    {
        return acao switch
        {
            QTEAction.Esquerda => spriteKeyEsquerda,
            QTEAction.Direita  => spriteKeyDireita,
            QTEAction.Cima     => spriteKeyCima,
            QTEAction.Baixo    => spriteKeyBaixo,
            QTEAction.Acao     => spriteKeyAcao,
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
            QTEAction.Esquerda => Input.GetKeyDown(KeyCode.LeftArrow),
            QTEAction.Direita  => Input.GetKeyDown(KeyCode.RightArrow),
            QTEAction.Cima     => Input.GetKeyDown(KeyCode.UpArrow),
            QTEAction.Baixo    => Input.GetKeyDown(KeyCode.DownArrow),
            QTEAction.Acao     => Input.GetKeyDown(KeyCode.Space),
            _                  => false,
        };
    }

    private bool ChecarInputIncorreto(QTEAction acaoEsperada)
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) && acaoEsperada != QTEAction.Esquerda) return true;
        if (Input.GetKeyDown(KeyCode.RightArrow) && acaoEsperada != QTEAction.Direita)  return true;
        if (Input.GetKeyDown(KeyCode.UpArrow) && acaoEsperada != QTEAction.Cima)     return true;
        if (Input.GetKeyDown(KeyCode.DownArrow) && acaoEsperada != QTEAction.Baixo)    return true;
        if (Input.GetKeyDown(KeyCode.Space) && acaoEsperada != QTEAction.Acao) return true;

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
        LimparSpritesTeclas();
        characterSpriteRenderer.sprite = spriteIdle;
        Debug.Log("Parabéns! Você concluiu a Fase 3!");
    }
}