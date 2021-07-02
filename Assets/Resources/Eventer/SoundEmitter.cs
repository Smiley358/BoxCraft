using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    /// <summary>
    /// クリップ保管用クラス
    /// </summary>
    [Serializable]
    public class ClipData{
        //オーディオクリップ
        [SerializeField] private AudioClip Clip;

        /// <summary>
        /// 再生
        /// </summary>
        public void Play()
        {
            audioSource?.PlayOneShot(Clip);
        }

        /// <summary>
        /// 名前の比較
        /// </summary>
        /// <param name="name">比較したい名前</param>
        /// <returns>同じならTrue</returns>
        public bool Compare(string name)
        {
            return name == Clip?.name;
        }
    }

    //インスタンス
    private static SoundEmitter instance;
    //オーディオソースコンポーネント
    private static AudioSource audioSource;

    //クリップリスト
    [SerializeField] private List<ClipData> clips;

    private void Awake()
    {
        instance = instance != null ? instance : this;
        audioSource = audioSource != null ? audioSource : GetComponent<AudioSource>();
    }

    /// <summary>
    /// オーディオクリップの取得
    /// </summary>
    /// <param name="name">クリップ名</param>
    /// <returns>存在するオーディオクリップ</returns>
    public static ClipData FindClip(string name)
    {
        return instance?.clips?.Find(clip => clip.Compare(name));
    }
}
