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
    [SerializeField] private float tempoParaResponder = 4.0f;
    private const int TAMANHO_SEQUENCIA = 3;
    private const int TOTAL_ACERTOS_OBJETIVO = 25;

    [Header("Barra de Progresso")]
    [Tooltip("Arraste o Transform do retângulo da barra (Objeto Pai) aqui")]
    public Transform barraProgresso;
    private Vector3 escalaOriginalBarra;

    [Header("UI de Fim de Jogo")]
    public EndGameUIManager endGameUI;

    private int acertosAtuais = 0;
    public int errosAtuais = 0;
    private bool jogoAtivo = false;
    private QTEAction[] sequenciaAtual = new QTEAction[TAMANHO_SEQUENCIA];
    private AudioSource audioSource;
    private int errosSeguidos = 0;
    private ScoreManager scoreManager;

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
        // === CÓDIGO DA BARRA ===
        if (barraProgresso != null)
        {
            if (escalaOriginalBarra == Vector3.zero) 
                escalaOriginalBarra = barraProgresso.localScale;
                
            barraProgresso.localScale = new Vector3(0f, escalaOriginalBarra.y, escalaOriginalBarra.z);
        }

        characterSpriteRenderer.sprite = spriteIdle;
        LimparSpritesTeclas();
        acertosAtuais = 0;
        errosAtuais = 0;
        errosSeguidos = 0; 
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

            GerarSequenciaAleatoria();
            ExibirTodasAsTeclas();

            int indicePassoAtual = 0;
            AtualizarDestaqueTeclas(indicePassoAtual);

            float tempoDecorrido = 0f;
            bool errou = false;

            while (tempoDecorrido < tempoParaResponder && indicePassoAtual < TAMANHO_SEQUENCIA)
            {
                tempoDecorrido += Time.deltaTime;
                QTEAction acaoEsperada = sequenciaAtual[indicePassoAtual];

                if (ChecarInputIncorreto(acaoEsperada))
                {
                    errou = true;
                    break;
                }

                if (ChecarInputCorreto(acaoEsperada))
                {
                    indicePassoAtual++;

                    if (indicePassoAtual < TAMANHO_SEQUENCIA)
                    {
                        AtualizarDestaqueTeclas(indicePassoAtual);
                        TocarSom(soundCliqueProgresso != null ? soundCliqueProgresso : soundAcerto);
                    }
                }

                yield return null;
            }

            LimparSpritesTeclas();

            if (!errou && indicePassoAtual >= TAMANHO_SEQUENCIA)
            {
                acertosAtuais++;
                AtualizarBarraProgresso();

                errosSeguidos = 0;
                scoreManager.AdicionarPontos(tempoDecorrido, tempoParaResponder);
                characterSpriteRenderer.sprite = ObterSpriteAcao(sequenciaAtual[TAMANHO_SEQUENCIA - 1]);
                TocarSom(soundAcerto);
            }
            else
            {
                Debug.Log("Errou a sequência ou o tempo esgotou!");
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

            if (i < indiceAtual) cor.a = 0.15f;
            else if (i == indiceAtual) cor.a = 1.0f;
            else cor.a = 0.4f;

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
        switch (acao)
        {
            case QTEAction.Esquerda: return spriteKeyEsquerda;
            case QTEAction.Direita:  return spriteKeyDireita;
            case QTEAction.Cima:     return spriteKeyCima;
            case QTEAction.Baixo:    return spriteKeyBaixo;
            case QTEAction.Acao:     return spriteKeyAcao;
            default:                 return null;
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

    private void AtualizarBarraProgresso()
    {
        if (barraProgresso != null)
        {
            float porcentagem = (float)acertosAtuais / TOTAL_ACERTOS_OBJETIVO;
            barraProgresso.localScale = new Vector3(escalaOriginalBarra.x * porcentagem, escalaOriginalBarra.y, escalaOriginalBarra.z);
        }
    }

    public void FinalizarJogo()
    {
        jogoAtivo = false;
        LimparSpritesTeclas(); 
        characterSpriteRenderer.sprite = spriteIdle;
        
        int estrelasGanhas = scoreManager.FinalizarFaseECalcularEstrelas();
        endGameUI.MostrarTela(estrelasGanhas);
    }

    public void Game_Over()
    {
        jogoAtivo = false;
        LimparSpritesTeclas();
        characterSpriteRenderer.sprite = spriteTriste;
        
        endGameUI.MostrarTela(0);
    }
}