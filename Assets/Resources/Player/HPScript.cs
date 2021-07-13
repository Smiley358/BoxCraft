using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPScript : MonoBehaviour
{
    //HP��prefab
    private static GameObject prefab;

    /// <summary>
    /// HP�̃n�[�g������
    /// </summary>
    /// <param name="index">���Ԗڂ̃n�[�g��</param>
    /// <param name="basePosition">��_</param>
    /// <returns>HP�̐���X�N���v�g</returns>
    public static HPScript Create(int index, Vector3 basePosition)
    {
        //prefab�̎擾
        prefab ??= PrefabManager.Instance.GetPrefab("HP");
        if(prefab == null)
        {
            Debug.Log("HP prefab is Not Exist");
        }

        //�n�[�g�̃g�����X�t�H�[��
        var rectTransform = prefab.GetComponent<RectTransform>();
        //�n�[�g�̕�
        float heartWidth = rectTransform.rect.width;
        //�n�[�g�̈ʒu
        Vector3 position = new Vector3(
            basePosition.x + (heartWidth + HPControler.HPmargin) * index,
            basePosition.y,
            basePosition.z);

        var hp = Instantiate(prefab, position, Quaternion.identity);
        if (hp != null)
        {
            var script = hp.GetComponent<HPScript>();
            script.index = index;
            return script;
        }
        return null;
    }


    //���Ԗڂ̃n�[�g��
    public int index { get; private set; }

    //�n�[�g�̃X�v���C�g��\������R���|�[�l���g
   [SerializeField] private Image HPImage;

    void Start()
    {
        //�C���X�y�N�^�[����Z�b�g�ł��Ă��Ȃ������ꍇ�ɃZ�b�g����
        HPImage ??= gameObject.transform.Find("Heart")?.GetComponent<Image>();
    }

    /// <summary>
    /// �n�[�g���X�V����
    /// </summary>
    public void UpdateHeart()
    {
        //�[�����v�Z
        float edge = (PlayerScript.Player.HP - HPControler.HeartWeight * index);
        //�[����HPControler.HeartWeight�����̏ꍇ�A�n�[�g�����Ă���͎̂���
        if (edge <= HPControler.HeartWeight)
        {
            //0�����Ȃ�0���
            edge = edge < 0 ? 0 : edge;
            //�n�[�g�̓h��Ԃ��͈͂̐ݒ�
            HPImage.fillAmount = edge / HPControler.HeartWeight;
        }
        else
        {
            //����Ă�͎̂�������Ȃ��̂�Max�ݒ�
            HPImage.fillAmount = 1f;
        }
    }
}
