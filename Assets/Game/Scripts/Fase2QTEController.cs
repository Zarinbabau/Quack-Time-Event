using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("Configurações da Fase 2")]
    [SerializeField] private float tempoParaResponder = 3.0f;
    private const int REPETICOES_NECESSARIAS = 3;
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
    private QTEAction acaoAtual;
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

       var config = DifficultyManager.ObterConfigFasePadrao("Fase2");
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
        keySpriteRenderer.sprite = null;
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
                        if (soundCliqueProgresso != null) TocarSom(soundCliqueProgresso);
                        else TocarSomAcertoAleatorio();
                    }
                }

                yield return null;
            }

            keySpriteRenderer.sprite = null;

            if (!errou && toquesConcluidos >= REPETICOES_NECESSARIAS)
            {
                acertosAtuais++;
                AtualizarBarraProgresso();
                
                scoreManager.AdicionarPontos(tempoDecorrido, tempoParaResponder);
                characterSpriteRenderer.sprite = ObterSpriteAcao(acaoAtual);
                TocarSomAcertoAleatorio();
            }
            else
            {
                Debug.Log("Errou ou tempo esgotado!");
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

            yield return new WaitForSeconds(0.7f);

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
        keySpriteRenderer.sprite = null;
        characterSpriteRenderer.sprite = spriteIdle;
        
        int estrelasGanhas = scoreManager.FinalizarFaseECalcularEstrelas();
        endGameUI.MostrarTela(estrelasGanhas);
    }

    public void Game_Over()
    {
        jogoAtivo = false;
        keySpriteRenderer.sprite = null;
        characterSpriteRenderer.sprite = spriteTriste;
        
        endGameUI.MostrarTela(0);
    }
}