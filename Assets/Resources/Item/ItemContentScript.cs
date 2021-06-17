using UnityEngine;

public class ItemContentScript : MonoBehaviour
{
    //���V�ړ��̃X�g���[�N
    private static float FloatStroke = 0.1f;
    //�������ꂽ����
    private float startTime;
    //����������
    private bool OnJoin;

    void Start()
    {
        //�������ꂽ����
        startTime = Time.time;
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
        other.transform.parent.gameObject.SendMessage("JoinItem", transform.parent.gameObject.GetComponent<ItemScript>().contentCount);
        OnJoin = true;
        Destroy(transform.parent.gameObject);
    }
}
