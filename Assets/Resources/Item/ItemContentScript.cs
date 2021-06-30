using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class ItemContentScript : MonoBehaviour
{
    //���V�ړ��̃X�g���[�N
    [SerializeField] private static float FloatStroke = 0.1f;
    //�������ꂽ����
    [SerializeField] private float startTime;
    //����������
    [SerializeField] private bool OnJoin;
    //�����̐e�I�u�W�F�N�g
    [NonSerialized] public ItemScript ParentScript;

    void Start()
    {
        //�������ꂽ����
        startTime = Time.time;
        //�e�I�u�W�F�N�g�̃X�N���v�g��ێ�
        ParentScript ??= transform.parent.GetComponent<ItemScript>();
    }
 
    void Update()
    {
        //��]
        Quaternion rotation = Quaternion.Euler(new Vector3(0, ((Time.time + startTime) % 30) / 30.0f * 360, 0));
        transform.localRotation = rotation;
        transform.localPosition = new Vector3(0, 0.5f, 0);

        //���V�A�j���[�V����
        Vector3 offset = new Vector3(0, Mathf.Sin(Time.time + startTime) * FloatStroke, 0);
        transform.localPosition = offset;
    }

    private void OnTriggerEnter(Collider other)
    {
        //�A�C�e������Ȃ���Ή������Ȃ�
        if (other.gameObject.tag != "Item") return;
        //�����A�C�e������Ȃ���Ή������Ȃ�
        if (other.name != name) return;
        //���肪��������̏ꍇ�������Ȃ�
        if (other.gameObject.GetComponent<ItemContentScript>().OnJoin == true) return;

        //�e�Ɍ����ʒm
        ExecuteEvents.Execute<IItemJoiner>(
            target: other.transform.parent.gameObject,
            eventData: null,
            functor: (IItemJoiner itemJoiner, BaseEventData eventData) =>
            {
                itemJoiner.JoinItem(ParentScript.contentCount);
            });
        OnJoin = true;
        Destroy(transform.parent.gameObject);
    }
}
