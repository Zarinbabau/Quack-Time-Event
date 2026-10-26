using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Fase2QTEController : MonoBehaviour
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
    [SerializeField] private AudioClip soundCliqueProgresso;
    [SerializeField] private AudioClip soundAcerto;
    [SerializeField] private AudioClip soundErro;

    [Header("Configurações da Fase 2")]
    [SerializeField] private float tempoParaResponder = 3.0f;
    private const int REPETICOES_NECESSARIAS = 3;
    private const int TOTAL_ACERTOS_OBJETIVO = 25;

    private int acertosAtuais = 0;
    public int errosAtuais = 0;
    private bool jogoAtivo = false;
    private QTEAction acaoAtual;
    private AudioSource audioSource;
    private int errosSeguidos = 0; 

    private ScoreManager scoreManager;

    public EndGameUIManager endGameUI;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        scoreManager = GetComponent<ScoreManager>();
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
        errosAtuais = 0;
        errosSeguidos = 0;
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
            acaoAtual = (QTEAction)Random.Range(0, 5);

            int toquesConcluidos = 0;
            AtualizarSpriteTecla(acaoAtual, toquesConcluidos);

            float tempoDecorrido = 0f;
            bool errou = false;

            while (tempoDecorrido < tempoParaResponder && toquesConcluidos < REPETICOES_NECESSARIAS)
            {
                tempoDecorrido += Time.deltaTime;

                if (ChecarInputIncorreto())
                {
                    errou = true;
                    break;
                }
                
                if (ChecarInputCorreto(acaoAtual))
                {
                    toquesConcluidos++;

                    if (toquesConcluidos < REPETICOES_NECESSARIAS)
                    {
                        AtualizarSpriteTecla(acaoAtual, toquesConcluidos);
                        TocarSom(soundCliqueProgresso != null ? soundCliqueProgresso : soundAcerto);
                    }
                }

                yield return null;
            }

            keySpriteRenderer.sprite = null;

            if (!errou && toquesConcluidos >= REPETICOES_NECESSARIAS)
            {
                acertosAtuais++;
                errosSeguidos = 0; 
                scoreManager.AdicionarPontos(tempoDecorrido, tempoParaResponder);
                characterSpriteRenderer.sprite = ObterSpriteAcao(acaoAtual);
                TocarSom(soundAcerto);
            }
            else
            {
                Debug.Log("Errou ou tempo esgotado!");
                characterSpriteRenderer.sprite = spriteTriste;
                TocarSom(soundErro);

                errosAtuais++;
                errosSeguidos++;
                scoreManager.RemoverPontos(tempoDecorrido, tempoParaResponder);

                if (errosSeguidos >= 3)
                {
                    yield return new WaitForSeconds(0.5f);
                    Game_Over();
                    yield break; 
                }
            }

            yield return new WaitForSeconds(0.7f);

            if (acertosAtuais >= TOTAL_ACERTOS_OBJETIVO)
            {
                FinalizarJogo();
                yield break;
            }

            characterSpriteRenderer.sprite = spriteIdle;
        }
    } // <- A CHAVE QUE ESTAVA FALTANDO/CAUSANDO O ERRO ESTÁ AQUI

    private void AtualizarSpriteTecla(QTEAction acao, int estagio)
    {
        int index = Mathf.Clamp(estagio, 0, 2);

        switch (acao)
        {
            case QTEAction.Esquerda: keySpriteRenderer.sprite = keySpritesEsquerda[index]; break;
            case QTEAction.Direita:  keySpriteRenderer.sprite = keySpritesDireita[index];  break;
            case QTEAction.Cima:     keySpriteRenderer.sprite = keySpritesCima[index];     break;
            case QTEAction.Baixo:    keySpriteRenderer.sprite = keySpritesBaixo[index];    break;
            case QTEAction.Acao:     keySpriteRenderer.sprite = keySpritesAcao[index];     break;
            default:                 keySpriteRenderer.sprite = null;                      break;
        }
    }

    private Sprite ObterSpriteAcao(QTEAction acao)
    {
        switch (acao)
        {
            case QTEAction.Esquerda: return spriteEsquerda;
            case QTEAction.Direita:  return spriteDireita;
            case QTEAction.Cima:     return spriteCima;
            case QTEAction.Baixo:    return spriteBaixo;
            case QTEAction.Acao:     return spriteAcao;
            default:                 return spriteIdle;
        }
    }

    private bool ChecarInputCorreto(QTEAction acao)
    {
        switch (acao)
        {
            case QTEAction.Esquerda: return Input.GetKeyDown(KeyCode.LeftArrow);
            case QTEAction.Direita:  return Input.GetKeyDown(KeyCode.RightArrow);
            case QTEAction.Cima:     return Input.GetKeyDown(KeyCode.UpArrow);
            case QTEAction.Baixo:    return Input.GetKeyDown(KeyCode.DownArrow);
            case QTEAction.Acao:     return Input.GetKeyDown(KeyCode.Space);
            default:                 return false;
        }
    }

    private bool ChecarInputIncorreto()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) && acaoAtual != QTEAction.Esquerda) return true;
        if (Input.GetKeyDown(KeyCode.RightArrow) && acaoAtual != QTEAction.Direita)  return true;
        if (Input.GetKeyDown(KeyCode.UpArrow) && acaoAtual != QTEAction.Cima)     return true;
        if (Input.GetKeyDown(KeyCode.DownArrow) && acaoAtual != QTEAction.Baixo)    return true;
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
        
        // Pega as estrelas e manda a UI aparecer
        int estrelasGanhas = scoreManager.FinalizarFaseECalcularEstrelas();
        endGameUI.MostrarTela(estrelasGanhas);
    }

    public void Game_Over()
    {
        jogoAtivo = false;
        keySpriteRenderer.sprite = null;
        characterSpriteRenderer.sprite = spriteTriste;
        
        // Manda 0 estrelas para a UI saber que é Game Over
        endGameUI.MostrarTela(0); 
    }
}