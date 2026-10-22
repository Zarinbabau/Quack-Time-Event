using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class QTEControllerFaseExtra : MonoBehaviour
{
    public enum QTEAction { Esquerda, Direita, Cima, Baixo, Acao }
    public enum MecanicaQTE { Simples, Repeticao, Sequencia }

    [Header("Sprite Renderers")]
    [SerializeField] private SpriteRenderer characterSpriteRenderer;
    [Tooltip("Renderizadores das teclas. [1] será o central usado para mecânicas únicas.")]
    [SerializeField] private SpriteRenderer[] keySpriteRenderers = new SpriteRenderer[3];

    [Header("Sprites do Personagem")]
    [SerializeField] private Sprite spriteIdle;
    [SerializeField] private Sprite spriteEsquerda;
    [SerializeField] private Sprite spriteDireita;
    [SerializeField] private Sprite spriteCima;
    [SerializeField] private Sprite spriteBaixo;
    [SerializeField] private Sprite spriteAcao;
    [SerializeField] private Sprite spriteTriste;

    [Header("Sprites das Teclas Básicas")]
    [SerializeField] private Sprite spriteKeyEsquerda;
    [SerializeField] private Sprite spriteKeyDireita;
    [SerializeField] private Sprite spriteKeyCima;
    [SerializeField] private Sprite spriteKeyBaixo;
    [SerializeField] private Sprite spriteKeyAcao;

    [Header("Sprites de Repetição (3 Estágios)")]
    [SerializeField] private Sprite[] keySpritesEsquerda = new Sprite[3];
    [SerializeField] private Sprite[] keySpritesDireita = new Sprite[3];
    [SerializeField] private Sprite[] keySpritesCima = new Sprite[3];
    [SerializeField] private Sprite[] keySpritesBaixo = new Sprite[3];
    [SerializeField] private Sprite[] keySpritesAcao = new Sprite[3];

    [Header("Áudio")]
    [SerializeField] private AudioClip soundCliqueProgresso;
    [SerializeField] private AudioClip soundAcerto;
    [SerializeField] private AudioClip soundErro;

    [Header("Configurações da Fase Extra")]
    [SerializeField] private float tempoSimples = 2.0f;
    [SerializeField] private float tempoRepeticao = 3.0f;
    [SerializeField] private float tempoSequencia = 4.0f;
    
    [Header("Sistema de Pontuação")]
    [SerializeField] private int pontosBase = 100;
    private int pontuacaoTotal = 0;
    private const int TOTAL_ACERTOS_OBJETIVO = 15;

    private int acertosAtuais = 0;
    private bool jogoAtivo = false;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        StartCoroutine(RotinaInicio());
    }

    private IEnumerator RotinaInicio()
    {
        characterSpriteRenderer.sprite = spriteIdle;
        OcultarTodasTeclas();
        acertosAtuais = 0;
        pontuacaoTotal = 0;
        jogoAtivo = false;

        yield return new WaitForSeconds(3.0f);
        jogoAtivo = true;
        StartCoroutine(RotinaQTE());
    }

    private IEnumerator RotinaQTE()
    {
        while (jogoAtivo && acertosAtuais < TOTAL_ACERTOS_OBJETIVO)
        {
            characterSpriteRenderer.sprite = spriteIdle;
            OcultarTodasTeclas();

            MecanicaQTE mecanicaAtual = (MecanicaQTE)Random.Range(0, 3);
            bool eventoConcluidoComSucesso = false;
            float tempoGasto = 0f;
            QTEAction acaoFinal = QTEAction.Acao;

            // Delega o resultado de sucesso e o tempo gasto para as variáveis locais
            if (mecanicaAtual == MecanicaQTE.Simples)
            {
                yield return StartCoroutine(RotinaSimples((resultado, tempo) => { eventoConcluidoComSucesso = resultado; tempoGasto = tempo; }, acao => acaoFinal = acao));
            }
            else if (mecanicaAtual == MecanicaQTE.Repeticao)
            {
                yield return StartCoroutine(RotinaRepeticao((resultado, tempo) => { eventoConcluidoComSucesso = resultado; tempoGasto = tempo; }, acao => acaoFinal = acao));
            }
            else if (mecanicaAtual == MecanicaQTE.Sequencia)
            {
                yield return StartCoroutine(RotinaSequencia((resultado, tempo) => { eventoConcluidoComSucesso = resultado; tempoGasto = tempo; }, acao => acaoFinal = acao));
            }

            OcultarTodasTeclas();

            if (eventoConcluidoComSucesso)
            {
                acertosAtuais++;
                characterSpriteRenderer.sprite = ObterSpriteAcao(acaoFinal);
                TocarSom(soundAcerto);

                // Cálculo do multiplicador baseado no tempo restante
                float tempoLimite = mecanicaAtual == MecanicaQTE.Simples ? tempoSimples : (mecanicaAtual == MecanicaQTE.Repeticao ? tempoRepeticao : tempoSequencia);
                float tempoRestante = tempoLimite - tempoGasto;
                
                // Multiplicador vai de 1.0x (no limite do tempo) até 2.0x (instantâneo)
                float multiplicador = 1f + (tempoRestante / tempoLimite); 
                int pontosGanhos = Mathf.RoundToInt(pontosBase * multiplicador);
                pontuacaoTotal += pontosGanhos;

                Debug.Log($"Acertou! Tempo: {tempoGasto:F2}s | Mult: {multiplicador:F2}x | Pontos ganhos: {pontosGanhos} | Pontuação Total: {pontuacaoTotal} | Progresso: {acertosAtuais}/{TOTAL_ACERTOS_OBJETIVO}");
            }
            else
            {
                Debug.Log("Errou o evento!");
                characterSpriteRenderer.sprite = spriteTriste;
                TocarSom(soundErro);
            }

            yield return new WaitForSeconds(0.5f);

            if (acertosAtuais >= TOTAL_ACERTOS_OBJETIVO)
            {
                jogoAtivo = false;
                characterSpriteRenderer.sprite = spriteIdle;
                Debug.Log($"Fase Extra Concluída! Sua pontuação final foi de: {pontuacaoTotal}");
                yield break;
            }
        }
    }

    private IEnumerator RotinaSimples(System.Action<bool, float> resultadoEvento, System.Action<QTEAction> acaoFinal)
    {
        QTEAction acao = (QTEAction)Random.Range(0, 5);
        acaoFinal(acao);
        
        // Alterado para [1]
        keySpriteRenderers[1].gameObject.SetActive(true);
        keySpriteRenderers[1].sprite = ObterSpriteTecla(acao);
        keySpriteRenderers[1].color = Color.white;

        float tempo = 0f;
        while (tempo < tempoSimples)
        {
            tempo += Time.deltaTime;
            if (ChecarInputIncorretoGeral(acao)) { resultadoEvento(false, tempo); yield break; }
            if (ChecarInputCorreto(acao)) { resultadoEvento(true, tempo); yield break; }
            yield return null;
        }
        resultadoEvento(false, tempo);
    }

    private IEnumerator RotinaRepeticao(System.Action<bool, float> resultadoEvento, System.Action<QTEAction> acaoFinal)
    {
        QTEAction acao = (QTEAction)Random.Range(0, 5);
        acaoFinal(acao);
        int toques = 0;

        // Alterado para [1]
        keySpriteRenderers[1].gameObject.SetActive(true);
        AtualizarSpriteRepeticao(acao, toques);
        

        float tempo = 0f;
        while (tempo < tempoRepeticao && toques < 3)
        {
            tempo += Time.deltaTime;
            if (ChecarInputIncorretoGeral(acao)) { resultadoEvento(false, tempo); yield break; }
            
            if (ChecarInputCorreto(acao))
            {
                toques++;
                if (toques < 3)
                {
                    AtualizarSpriteRepeticao(acao, toques);
                    TocarSom(soundCliqueProgresso);
                }
            }
            yield return null;
        }
        resultadoEvento(toques >= 3, tempo);
    }

    private IEnumerator RotinaSequencia(System.Action<bool, float> resultadoEvento, System.Action<QTEAction> acaoFinal)
    {
        QTEAction[] seq = new QTEAction[3];
        for (int i = 0; i < 3; i++)
        {
            seq[i] = (QTEAction)Random.Range(0, 5);
            keySpriteRenderers[i].gameObject.SetActive(true);
            keySpriteRenderers[i].sprite = ObterSpriteTecla(seq[i]);
        }
        acaoFinal(seq[2]); 

        int passo = 0;
        AtualizarDestaqueSequencia(passo);

        float tempo = 0f;
        while (tempo < tempoSequencia && passo < 3)
        {
            tempo += Time.deltaTime;
            if (ChecarInputIncorretoGeral(seq[passo])) { resultadoEvento(false, tempo); yield break; }
            
            if (ChecarInputCorreto(seq[passo]))
            {
                passo++;
                if (passo < 3)
                {
                    AtualizarDestaqueSequencia(passo);
                    TocarSom(soundCliqueProgresso);
                }
            }
            yield return null;
        }
        resultadoEvento(passo >= 3, tempo);
    }

    private void OcultarTodasTeclas()
    {
        foreach (var sr in keySpriteRenderers)
        {
            if (sr != null) sr.gameObject.SetActive(false);
        }
    }

    private void AtualizarDestaqueSequencia(int indiceAtual)
    {
        for (int i = 0; i < 3; i++)
        {
            Color cor = Color.white;
            if (i < indiceAtual) cor.a = 0.15f;
            else if (i == indiceAtual) cor.a = 1.0f;
            else cor.a = 0.4f;
            keySpriteRenderers[i].color = cor;
        }
    }

    private void AtualizarSpriteRepeticao(QTEAction acao, int estagio)
    {
        int index = Mathf.Clamp(estagio, 0, 2);
        // Alterado para [1]
        keySpriteRenderers[1].sprite = acao switch
        {
            QTEAction.Esquerda => keySpritesEsquerda[index],
            QTEAction.Direita  => keySpritesDireita[index],
            QTEAction.Cima     => keySpritesCima[index],
            QTEAction.Baixo    => keySpritesBaixo[index],
            QTEAction.Acao     => keySpritesAcao[index],
            _ => null
        };
    }

    private Sprite ObterSpriteTecla(QTEAction acao)
    {
        return acao switch { QTEAction.Esquerda => spriteKeyEsquerda, QTEAction.Direita => spriteKeyDireita, QTEAction.Cima => spriteKeyCima, QTEAction.Baixo => spriteKeyBaixo, QTEAction.Acao => spriteKeyAcao, _ => null };
    }

    private Sprite ObterSpriteAcao(QTEAction acao)
    {
        return acao switch { QTEAction.Esquerda => spriteEsquerda, QTEAction.Direita => spriteDireita, QTEAction.Cima => spriteCima, QTEAction.Baixo => spriteBaixo, QTEAction.Acao => spriteAcao, _ => spriteIdle };
    }

    private bool ChecarInputCorreto(QTEAction acao)
    {
        return acao switch { QTEAction.Esquerda => Input.GetKeyDown(KeyCode.LeftArrow), QTEAction.Direita => Input.GetKeyDown(KeyCode.RightArrow), QTEAction.Cima => Input.GetKeyDown(KeyCode.UpArrow), QTEAction.Baixo => Input.GetKeyDown(KeyCode.DownArrow), QTEAction.Acao => Input.GetKeyDown(KeyCode.Space), _ => false };
    }

    private bool ChecarInputIncorretoGeral(QTEAction acaoEsperada)
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
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
    }
}