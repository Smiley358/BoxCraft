using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    /// <summary>
    /// �N���b�v�ۊǗp�N���X
    /// </summary>
    [Serializable]
    public class ClipData{
        //�I�[�f�B�I�N���b�v
        [SerializeField] private AudioClip Clip;

        /// <summary>
        /// �Đ�
        /// </summary>
        public void Play()
        {
            audioSource?.PlayOneShot(Clip);
        }

        /// <summary>
        /// ���O�̔�r
        /// </summary>
        /// <param name="name">��r���������O</param>
        /// <returns>�����Ȃ�True</returns>
        public bool Compare(string name)
        {
            return name == Clip?.name;
        }
    }

    //�C���X�^���X
    private static SoundEmitter instance;
    //�I�[�f�B�I�\�[�X�R���|�[�l���g
    private static AudioSource audioSource;

    //�N���b�v���X�g
    [SerializeField] private List<ClipData> clips;

    private void Awake()
    {
        instance = instance != null ? instance : this;
        audioSource = audioSource != null ? audioSource : GetComponent<AudioSource>();
    }

    /// <summary>
    /// �I�[�f�B�I�N���b�v�̎擾
    /// </summary>
    /// <param name="name">�N���b�v��</param>
    /// <returns>���݂���I�[�f�B�I�N���b�v</returns>
    public static ClipData FindClip(string name)
    {
        return instance?.clips?.Find(clip => clip.Compare(name));
    }
}
