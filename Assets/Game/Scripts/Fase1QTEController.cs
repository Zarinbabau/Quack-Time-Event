using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class Fase1QTEController : MonoBehaviour
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

    [Header("Sistema de Vidas (HUD)")]
    [Tooltip("Arraste as 3 imagens (UI Image) das vidas da esquerda para a direita")]
    [SerializeField] private Image[] iconesVidas = new Image[3];
    [SerializeField] private Sprite spriteVidaCheia;
    [SerializeField] private Sprite spriteVidaPerdida;

    [Header("Áudio")]
    [Tooltip("Adicione os 3 áudios diferentes de acerto")]
    [SerializeField] private AudioClip[] soundsAcerto = new AudioClip[3];
    [SerializeField] private AudioClip soundErro;

    [Header("Configurações do QTE")]
    [SerializeField] private float tempoParaResponder = 2.0f;
    private int totalAcertosObjetivo = 25;
    private int maxVidas = 3;

    private int acertosAtuais = 0;
    public int errosAtuais = 0;
    private int vidasAtuais = 3;
    private bool jogoAtivo = false;
    private bool inputRecebido = false;
    private QTEAction acaoAtual;
    private AudioSource audioSource;
    
    private ScoreManager scoreManager;
    public EndGameUIManager endGameUI;

    [Header("Barra de Progresso")]
    [Tooltip("Arraste o Transform do retângulo da barra aqui")]
    public Transform barraProgresso;
    private Vector3 escalaOriginalBarra;

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

        var config = DifficultyManager.ObterConfigFasePadrao("Fase1");
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

            acaoAtual = (QTEAction)Random.Range(0, 5);
            AtualizarSpriteTecla(acaoAtual);

            inputRecebido = false;
            bool acertou = false;
            float tempoDecorrido = 0f;

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

            keySpriteRenderer.sprite = null;

            if (acertou && inputRecebido)
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
                errosAtuais++;
                vidasAtuais--;
                AtualizarUIVidas();

                scoreManager.RemoverPontos(tempoDecorrido, tempoParaResponder);
                TocarSom(soundErro);

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
            QTEAction.Esquerda => Input.GetKeyDown(KeyCode.LeftArrow),
            QTEAction.Direita  => Input.GetKeyDown(KeyCode.RightArrow),
            QTEAction.Cima     => Input.GetKeyDown(KeyCode.UpArrow),
            QTEAction.Baixo    => Input.GetKeyDown(KeyCode.DownArrow),
            QTEAction.Acao     => Input.GetKeyDown(KeyCode.Space),
            _                  => false,
        };
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