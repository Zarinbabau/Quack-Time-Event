using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct MusicaPorCena
{
    public string nomeDaCena;
    public AudioClip musica;
}

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Configuração de Áudio")]
    public AudioSource audioSource;
    public List<MusicaPorCena> faixasPorCena;

    private float volumePadrao = 1f;
    private Coroutine rotinaFadeOut;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource != null)
        {
            volumePadrao = audioSource.volume;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AoCarregarCena;
    }

    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        foreach (var item in faixasPorCena)
        {
            if (item.nomeDaCena == cena.name)
            {
                TrocarMusica(item.musica);
                return;
            }
        }
    }

    public void TrocarMusica(AudioClip novaMusica)
    {
        if (novaMusica == null || audioSource == null) return;

        // Cancela qualquer fade-out anterior caso o jogador reinicie a fase rápido
        if (rotinaFadeOut != null)
        {
            StopCoroutine(rotinaFadeOut);
            rotinaFadeOut = null;
        }

        // Se a mesma música já estiver tocando no volume normal (ex: entre dois menus), não reinicia
        if (audioSource.clip == novaMusica && audioSource.isPlaying && Mathf.Approximately(audioSource.volume, volumePadrao))
            return;

        audioSource.volume = volumePadrao;
        audioSource.clip = novaMusica;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void PararMusicaComFade(float duracao)
    {
        if (audioSource == null || !audioSource.isPlaying) return;

        if (rotinaFadeOut != null)
        {
            StopCoroutine(rotinaFadeOut);
        }

        rotinaFadeOut = StartCoroutine(RotinaFadeOut(duracao));
    }

    private IEnumerator RotinaFadeOut(float duracao)
    {
        float volumeInicial = audioSource.volume;
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(volumeInicial, 0f, tempo / duracao);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
        rotinaFadeOut = null;
    }
}