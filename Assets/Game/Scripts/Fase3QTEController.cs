using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("Sistema de Vidas (HUD)")]
    [Tooltip("Arraste as 3 imagens (UI Image) das vidas da esquerda para a direita")]
    [SerializeField] private Image[] iconesVidas = new Image[3];
    [SerializeField] private Sprite spriteVidaCheia;
    [SerializeField] private Sprite spriteVidaPerdida;

    [Header("Áudio")]
    [SerializeField] private AudioClip soundCliqueProgresso;
    [Tooltip("Adicione os 3 áudios diferentes de acerto")]
    [SerializeField] private AudioClip[] soundsAcerto = new AudioClip[3];
    [SerializeField] private AudioClip soundErro;

    [Header("Configurações da Fase 3")]
    [SerializeField] private float tempoParaResponder = 4.0f;
    private const int TAMANHO_SEQUENCIA = 3;
    private int totalAcertosObjetivo = 25;
    private int maxVidas = 3;

    [Header("Barra de Progresso")]
    [Tooltip("Arraste o Transform do retângulo da barra (Objeto Pai) aqui")]
    public Transform barraProgresso;
    private Vector3 escalaOriginalBarra;

    [Header("UI de Fim de Jogo")]
    public EndGameUIManager endGameUI;

    private int acertosAtuais = 0;
    public int errosAtuais = 0;
    private int vidasAtuais = 3;
    private bool jogoAtivo = false;
    private QTEAction[] sequenciaAtual = new QTEAction[TAMANHO_SEQUENCIA];
    private AudioSource audioSource;
    private ScoreManager scoreManager;

    [Header("Contagem Regressiva Inicial")]
    [Tooltip("Painel/Fundo escuro opcional da contagem (desativa sozinho ao terminar)")]
    [SerializeField] private GameObject painelContagem;
    [Tooltip("Texto TextMeshPro que exibirá 3, 2, 1")]
    [SerializeField] private TMP_Text txtContagem;
    [Tooltip("Som opcional tocado a cada número (3, 2, 1)")]
    [SerializeField] private AudioClip somContagem;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        scoreManager = GetComponent<ScoreManager>(); 

        var config = DifficultyManager.ObterConfigFasePadrao("Fase3");
        tempoParaResponder = config.tempoParaResponder;
        totalAcertosObjetivo = config.acertosObjetivo;
        maxVidas = DifficultyManager.ObterMaxVidas();
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
        vidasAtuais = maxVidas;
        AtualizarUIVidas();
        jogoAtivo = false;

        yield return StartCoroutine(ContagemRegressivaRotina());

        jogoAtivo = true;
        StartCoroutine(RotinaQTE());
    }

    private IEnumerator RotinaQTE()
    {
        while (jogoAtivo && acertosAtuais < totalAcertosObjetivo)
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
                        if (soundCliqueProgresso != null) TocarSom(soundCliqueProgresso);
                        else TocarSomAcertoAleatorio();
                    }
                }

                yield return null;
            }

            LimparSpritesTeclas();

            if (!errou && indicePassoAtual >= TAMANHO_SEQUENCIA)
            {
                acertosAtuais++;
                AtualizarBarraProgresso();

                scoreManager.AdicionarPontos(tempoDecorrido, tempoParaResponder);
                characterSpriteRenderer.sprite = ObterSpriteAcao(sequenciaAtual[TAMANHO_SEQUENCIA - 1]);
                TocarSomAcertoAleatorio();
            }
            else
            {
                Debug.Log("Errou a sequência ou o tempo esgotou!");
                characterSpriteRenderer.sprite = spriteTriste;
                TocarSom(soundErro);

                errosAtuais++;
                vidasAtuais--;
                AtualizarUIVidas();
                scoreManager.RemoverPontos(tempoDecorrido, tempoParaResponder);

                if (vidasAtuais <= 0)
                {
                    yield return new WaitForSeconds(0.5f); 
                    Game_Over();
                    yield break; 
                }
            }

            yield return new WaitForSeconds(0.5f);

            if (acertosAtuais >= totalAcertosObjetivo)
            {
                FinalizarJogo();
                yield break;
            }

            characterSpriteRenderer.sprite = spriteIdle;
        }
    }

    private void AtualizarUIVidas()
    {
        if (iconesVidas == null) return;

        for (int i = 0; i < iconesVidas.Length; i++)
        {
            if (iconesVidas[i] != null)
            {
                iconesVidas[i].gameObject.SetActive(i < maxVidas);
                iconesVidas[i].sprite = (i < vidasAtuais) ? spriteVidaCheia : spriteVidaPerdida;
            }
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

    private void TocarSomAcertoAleatorio()
    {
        if (soundsAcerto == null || soundsAcerto.Length == 0) return;

        AudioClip clipSorteado = soundsAcerto[Random.Range(0, soundsAcerto.Length)];
        TocarSom(clipSorteado);
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
            float porcentagem = (float)acertosAtuais / totalAcertosObjetivo;
            barraProgresso.localScale = new Vector3(escalaOriginalBarra.x * porcentagem, escalaOriginalBarra.y, escalaOriginalBarra.z);
        }
    }

    private IEnumerator ContagemRegressivaRotina()
    {
        if (painelContagem != null) painelContagem.SetActive(true);
        if (txtContagem != null) txtContagem.gameObject.SetActive(true);

        for (int i = 3; i >= 1; i--)
        {
            if (txtContagem != null)
            {
                txtContagem.text = i.ToString();
                txtContagem.transform.localScale = Vector3.one * 1.4f;
            }

            TocarSom(somContagem);

            float tempoSegundo = 0f;
            while (tempoSegundo < 1f)
            {
                tempoSegundo += Time.deltaTime;
                if (txtContagem != null)
                {
                    txtContagem.transform.localScale = Vector3.Lerp(Vector3.one * 1.4f, Vector3.one, tempoSegundo * 2f);
                }
                yield return null;
            }
        }

        if (txtContagem != null) txtContagem.gameObject.SetActive(false);
        if (painelContagem != null) painelContagem.SetActive(false);
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