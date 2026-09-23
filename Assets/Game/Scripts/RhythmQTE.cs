using System.Collections;
using UnityEngine;

public class RhythmQTE : MonoBehaviour
{
    [Header("Configurações do QTE")]
    [Tooltip("Tecla que o jogador precisa pressionar.")]
    public KeyCode qteKey = KeyCode.Space;
    
    [Tooltip("Janela de tempo do QTE em segundos.")]
    public float timeLimit = 1.0f;

    [Header("Componentes e Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite defaultSprite;
    public Sprite hitSprite;
    public Sprite missSprite;

    private bool isQTEActive = false;
    private float timer = 0f;

    void Start()
    {
        // if (spriteRenderer == null)
        //     spriteRenderer = GetComponent<SpriteRenderer>();

        // Inicia o primeiro QTE (pode ser chamado via evento/gerenciador)
        StartQTE();
    }

    void Update()
    {
        if (!isQTEActive) return;

        // Atualiza o tempo do QTE
        timer += Time.deltaTime;

        // Verifica a entrada do jogador dentro do tempo
        if (Input.GetKeyDown(qteKey))
        {
            HandleHit();
        }
        // Se o tempo esgotar sem resposta
        else if (timer >= timeLimit)
        {
            HandleMiss();
        }
    }

    /// <summary>
    /// Inicia ou reinicia o Quick Time Event.
    /// </summary>
    public void StartQTE()
    {
        isQTEActive = true;
        timer = 0f;
        if (spriteRenderer != null && defaultSprite != null)
        {
            spriteRenderer.sprite = defaultSprite;
        }
    }

    private void HandleHit()
    {
        isQTEActive = false;
        if (spriteRenderer != null && hitSprite != null)
        {
            spriteRenderer.sprite = hitSprite;
        }
        Debug.Log("QTE: Acertou no tempo!");
    }

    private void HandleMiss()
    {
        isQTEActive = false;
        if (spriteRenderer != null && missSprite != null)
        {
            spriteRenderer.sprite = missSprite;
        }
        Debug.Log("QTE: Errou ou tempo esgotado!");
    }
}