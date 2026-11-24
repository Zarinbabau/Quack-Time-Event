using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class FaseExtraQTEController : MonoBehaviour
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

    [Header("Configurações da Fase Extra")]
    [SerializeField] private float tempoSimples = 2.0f;
    [SerializeField] private float tempoRepeticao = 3.0f;
    [SerializeField] private float tempoSequencia = 4.0f;
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
    private AudioSource audioSource;
    private ScoreManager scoreManager;

    [Header("Contagem Regressiva Inicial")]
    [Tooltip("Painel/Fundo escuro opcional da contagem (desativa sozinho ao terminar)")]
    [SerializeField] private GameObject painelContagem;
    [Tooltip("Texto TextMeshPro que exibirá 3, 2, 1")]
    [SerializeField] private TMP_Text txtContagem;
    [Tooltip("Som opcional tocado a cada número (3, 2, 1)")]
    [SerializeField] private AudioClip somContagem;
    [Tooltip("Tempo de espera em segundos após a contagem antes de aparecer a primeira tecla")]
    [SerializeField] private float delayAposContagem = 0.6f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        scoreManager = GetComponent<ScoreManager>();

        var config = DifficultyManager.ObterConfigFaseExtra();
        tempoSimples = config.tempoSimples;
        tempoRepeticao = config.tempoRepeticao;
        tempoSequencia = config.tempoSequencia;
        totalAcertosObjetivo = config.acertosObjetivo;
        maxVidas = DifficultyManager.ObterMaxVidas();
    }

    private void Start()
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
        OcultarTodasTeclas();
        acertosAtuais = 0;
        errosAtuais = 0;
        vidasAtuais = maxVidas;
        AtualizarUIVidas();
        jogoAtivo = false;

        yield return StartCoroutine(ContagemRegressivaRotina());

        if (delayAposContagem > 0f)
            yield return new WaitForSeconds(delayAposContagem);

        jogoAtivo = true;
        StartCoroutine(RotinaQTE());
    }

    private IEnumerator RotinaQTE()
    {
        while (jogoAtivo && acertosAtuais < totalAcertosObjetivo)
        {
            characterSpriteRenderer.sprite = spriteIdle;
            OcultarTodasTeclas();

            MecanicaQTE mecanicaAtual = (MecanicaQTE)Random.Range(0, 3);
            bool eventoConcluidoComSucesso = false;
            float tempoGasto = 0f;
            QTEAction acaoFinal = QTEAction.Acao;

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
            float tempoLimiteAtual = ObterTempoLimiteMecanica(mecanicaAtual);

            if (eventoConcluidoComSucesso)
            {
                acertosAtuais++;
                AtualizarBarraProgresso();

                scoreManager.AdicionarPontos(tempoGasto, tempoLimiteAtual);
                characterSpriteRenderer.sprite = ObterSpriteAcao(acaoFinal);
                TocarSomAcertoAleatorio();
            }
            else
            {
                Debug.Log("Errou o evento!");
                characterSpriteRenderer.sprite = spriteTriste;
                TocarSom(soundErro);

                errosAtuais++;
                vidasAtuais--;
                AtualizarUIVidas();
                scoreManager.RemoverPontos(tempoGasto, tempoLimiteAtual);

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

    private float ObterTempoLimiteMecanica(MecanicaQTE mecanica)
    {
        switch (mecanica)
        {
            case MecanicaQTE.Simples: return tempoSimples;
            case MecanicaQTE.Repeticao: return tempoRepeticao;
            case MecanicaQTE.Sequencia: return tempoSequencia;
            default: return 2.0f;
        }
    }

    private IEnumerator RotinaSimples(System.Action<bool, float> resultadoEvento, System.Action<QTEAction> acaoFinal)
    {
        QTEAction acao = (QTEAction)Random.Range(0, 5);
        acaoFinal(acao);
        
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
        
        switch (acao)
        {
            case QTEAction.Esquerda: keySpriteRenderers[1].sprite = keySpritesEsquerda[index]; break;
            case QTEAction.Direita:  keySpriteRenderers[1].sprite = keySpritesDireita[index]; break;
            case QTEAction.Cima:     keySpriteRenderers[1].sprite = keySpritesCima[index]; break;
            case QTEAction.Baixo:    keySpriteRenderers[1].sprite = keySpritesBaixo[index]; break;
            case QTEAction.Acao:     keySpriteRenderers[1].sprite = keySpritesAcao[index]; break;
            default:                 keySpriteRenderers[1].sprite = null; break;
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

    private bool ChecarInputIncorretoGeral(QTEAction acaoEsperada)
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
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
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
        OcultarTodasTeclas();
        characterSpriteRenderer.sprite = spriteIdle;
        
        int estrelasGanhas = scoreManager.FinalizarFaseECalcularEstrelas();
        endGameUI.MostrarTela(estrelasGanhas);
    }

    public void Game_Over()
    {
        jogoAtivo = false;
        OcultarTodasTeclas();
        characterSpriteRenderer.sprite = spriteTriste;
        
        endGameUI.MostrarTela(0);
    }
}