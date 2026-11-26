using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CenaAnimacaoController : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Arraste o objeto da historinha que tem o componente Animator")]
    public Animator animatorHistoria;

    [Tooltip("Arraste aqui os Animators do Canvas (MolduraFilme e ManchasFilme) para travarem junto no final")]
    public Animator[] animatorsCanvas;

    [Tooltip("Opcional: Texto ou botão (ex: 'Pressione qualquer tecla para continuar') que aparece quando a animação termina")]
    public GameObject indicadorContinuar;

    [Header("Efeito de Fade-Out")]
    [Tooltip("Arraste uma UI Image preta (esticada na tela inteira) para fazer o escurecimento")]
    public Image imagemFade;

    [Tooltip("Duração em segundos do escurecimento antes de trocar de cena")]
    public float duracaoFadeOut = 0.8f;

    [Header("Transição de Cena")]
    public string nomeCenaDestino = "LevelSelectionMenu";

    [Tooltip("Use 1 na Animação Inicial para dar zoom direto na Fase 1 ao terminar. Deixe -1 na Animação Final.")]
    public int indiceAutoProgressoNoMenu = -1;

    [Header("Controles de Avanço")]
    [Tooltip("Se marcado, o jogador também pode pular a animação antes de ela acabar")]
    public bool permitirPularDuranteAnimacao = true;

    [Tooltip("Evita que um clique acidental logo ao abrir a cena pule a animação")]
    public float tempoMinimoAntesDePular = 0.8f;

    private float tempoDecorrido = 0f;
    private bool animacaoConcluida = false;
    private int frameConclusao = -1;
    private bool saindo = false;

    private void Start()
    {
        if (indicadorContinuar != null)
            indicadorContinuar.SetActive(false);

        if (imagemFade != null)
        {
            Color cor = imagemFade.color;
            cor.a = 0f;
            imagemFade.color = cor;
            imagemFade.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (saindo) return;

        tempoDecorrido += Time.deltaTime;

        // 1. Detecta o fim da animação e trava a história e o Canvas
        if (!animacaoConcluida && animatorHistoria != null)
        {
            AnimatorStateInfo estado = animatorHistoria.GetCurrentAnimatorStateInfo(0);
            if (!animatorHistoria.IsInTransition(0) && estado.normalizedTime >= 1f)
            {
                TravarNoUltimoQuadro(estado);
            }
        }

        // 2. Se a animação já acabou, espera o jogador apertar qualquer botão/tecla para sair
        if (animacaoConcluida)
        {
            if (Time.frameCount > frameConclusao && Input.anyKeyDown)
            {
                FinalizarCena();
            }
            return;
        }

        // 3. Opcional: permite pular enquanto a animação ainda está rodando
        if (permitirPularDuranteAnimacao && tempoDecorrido >= tempoMinimoAntesDePular)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetMouseButtonDown(0))
            {
                FinalizarCena();
            }
        }
    }

    private void TravarNoUltimoQuadro(AnimatorStateInfo estado)
    {
        animacaoConcluida = true;
        frameConclusao = Time.frameCount;

        // Trava a animação da historinha exatamente no último frame
        animatorHistoria.Play(estado.fullPathHash, 0, 0.999f);
        animatorHistoria.Update(0f);
        animatorHistoria.speed = 0f;

        // Trava também as animações do Canvas (moldura e manchas)
        if (animatorsCanvas != null)
        {
            foreach (Animator anim in animatorsCanvas)
            {
                if (anim != null)
                    anim.speed = 0f;
            }
        }

        if (indicadorContinuar != null)
            indicadorContinuar.SetActive(true);
    }

    // Também pode ser chamado diretamente pelo evento OnClick() de um botão de UI
    public void FinalizarCena()
    {
        if (saindo) return;
        saindo = true;
        StartCoroutine(RotinaFadeOutESair());
    }

    private IEnumerator RotinaFadeOutESair()
    {
        if (imagemFade != null && duracaoFadeOut > 0f)
        {
            imagemFade.gameObject.SetActive(true);
            imagemFade.raycastTarget = true;

            Color cor = imagemFade.color;
            float tempoFade = 0f;

            while (tempoFade < duracaoFadeOut)
            {
                tempoFade += Time.deltaTime;
                cor.a = Mathf.Clamp01(tempoFade / duracaoFadeOut);
                imagemFade.color = cor;
                yield return null;
            }

            cor.a = 1f;
            imagemFade.color = cor;
        }

        if (indiceAutoProgressoNoMenu >= 0)
        {
            PlayerPrefs.SetInt("AutoProgression_ToIndex", indiceAutoProgressoNoMenu);
            PlayerPrefs.Save();
        }

        SceneManager.LoadScene(nomeCenaDestino);
    }
}