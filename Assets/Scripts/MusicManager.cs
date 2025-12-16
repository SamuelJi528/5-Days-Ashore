using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip dayMusic;
    public AudioClip nightMusic;
    public AudioClip deathMusic;
    public AudioClip winMusic;

    bool isDay;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }
    //Morning/Night music based on time
    public void SetDayState(bool day)
    {
        if (day == isDay && audioSource.clip != null) return;

        isDay = day;
        AudioClip target = isDay ? dayMusic : nightMusic;
        if (target == null) return;

        audioSource.clip = target;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void PlayDeathMusic()
    {
        audioSource.clip = deathMusic;
        audioSource.loop = false;
        audioSource.Play();
    }

    public void PlayWinMusic()
    {
        audioSource.clip = winMusic;
        audioSource.loop = false;
        audioSource.Play();
    }
}
