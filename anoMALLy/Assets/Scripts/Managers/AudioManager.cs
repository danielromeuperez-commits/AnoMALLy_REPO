using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //Declaración del Singleton
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null) Debug.Log("No hay GameManager!");
            return instance;
        }

    }
    //Fin del Singleton

    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioClip[] musicLibrary;
    public AudioClip[] sfxLibrary;

    private void Awake()
    {
        if (instance == null)
        {
            //Si no hay GameManager lo referenciamos y hacemos que perdure entre escenas
            instance = this;
        }
        else
        {
            //Si ya hay GameManager, el duplicado se destruye
            Destroy(gameObject);
        }
    }

    public void PlayMusic(int musicToPlay)
    {
        musicSource.clip = musicLibrary[musicToPlay];
        musicSource.Play(); //Reproducir la música desde el principio
    }

    public void PlaySFX(int sfxToPlay)
    {
        sfxSource.PlayOneShot(sfxLibrary[sfxToPlay]);
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void UnPauseMusic()
    {
        musicSource.UnPause();
    }
}
