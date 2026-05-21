using UnityEngine;

public class SceneMusicPlayer : MonoBehaviour
{
    [Header("Música")]
    [SerializeField] int musicIndex = 0;

    [Header("Opciones")]
    [SerializeField] bool playMusicOnStart = true;
    [SerializeField] bool stopMusicOnStart = false;

    private void Start()
    {
        if (AudioManager.Instance == null) return;

        if (stopMusicOnStart)
        {
            AudioManager.Instance.StopMusic();
            return;
        }

        if (playMusicOnStart)
        {
            AudioManager.Instance.PlayMusic(musicIndex);
        }
    }
}