using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("BGM设置")]
    public AudioClip bgmClip;           // BGM音频文件
    public AudioSource bgmSource;        // BGM音频源
    public bool playOnStart = true;      // 是否在开始时播放
    public bool loop = true;             // 是否循环播放
    [Range(0f, 1f)]
    public float volume = 0.5f;         // 音量大小
    
    private void Start()
    {
        // 如果没有AudioSource，自动添加
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 设置AudioSource属性
        bgmSource.clip = bgmClip;
        bgmSource.loop = loop;
        bgmSource.volume = volume;
        bgmSource.playOnAwake = false;  // 不自动播放，由我们控制
        
        // 如果设置了自动播放，开始播放BGM
        if (playOnStart && bgmClip != null)
        {
            PlayBGM();
        }
    }
    
    /// <summary>
    /// 播放BGM
    /// </summary>
    public void PlayBGM()
    {
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.Play();
            Debug.Log("BGM开始播放");
        }
    }
    
    /// <summary>
    /// 停止BGM
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
            Debug.Log("BGM已停止");
        }
    }
    
    /// <summary>
    /// 暂停BGM
    /// </summary>
    public void PauseBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Pause();
            Debug.Log("BGM已暂停");
        }
    }
    
    /// <summary>
    /// 恢复BGM播放
    /// </summary>
    public void ResumeBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.UnPause();
            Debug.Log("BGM已恢复播放");
        }
    }
    
    /// <summary>
    /// 设置BGM音量
    /// </summary>
    public void SetBGMVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (bgmSource != null)
        {
            bgmSource.volume = volume;
        }
    }
    
    /// <summary>
    /// 切换BGM
    /// </summary>
    public void ChangeBGM(AudioClip newBGM)
    {
        if (newBGM != null)
        {
            bgmClip = newBGM;
            bgmSource.clip = newBGM;
            if (bgmSource.isPlaying)
            {
                bgmSource.Play(); // 重新开始播放
            }
            Debug.Log("BGM已切换");
        }
    }
}