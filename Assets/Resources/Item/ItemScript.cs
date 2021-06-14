using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : MonoBehaviour
{
    //�A�C�e���̃v���n�u
    static public GameObject ItemPrefab;
    static public GameObject ItemContentPrefab;
    public enum ChildIndex
    {
        Physics,
        Content
    }

    public static bool Create(GameObject itemObject)
    {
        if (ItemPrefab == null)
        {
            ItemPrefab = Resources.Load("Item/Item") as GameObject;
        }
        if (ItemContentPrefab == null)
        {
            ItemContentPrefab = Resources.Load("Item/ItemContent") as GameObject;
        }
        //�A�C�e��������BOX�̊Ǘ��N���X���擾
        BoxBase box = itemObject.GetComponent<BoxBase>();
        //Box�Ǘ��N���X�������Ă��Ȃ���΃A�C�e���f�[�^���Ȃ��̂Ő������Ȃ�
        if (box == null)
        {
            return false;
        }
        //�A�C�e���f�[�^�𐶐����ăZ�b�g
        InventoryItem itemData = new InventoryItem(box.ItemData);
        //�A�C�e���f�[�^���Ȃ�������A�C�e���������s��Ȃ�
        if (itemData== null)
        {
            return false;
        }

        //�A�C�e���̃C���X�^���X��
        GameObject madeObject = Instantiate(ItemPrefab);
        //madeObject.transform.SetPositionAndRotation(itemObject.transform.position, Quaternion.Euler(Vector3.zero));
        madeObject.transform.position = itemObject.transform.position;
        //madeObject.transform.localPosition = Vector3.zero;
        madeObject.name = "Item_" + itemData.ItemName;

        //�A�C�e���Ǘ��N���X�̎擾
        ItemScript itemScript = madeObject.GetComponent<ItemScript>();
        //�A�C�e���f�[�^�̊i�[
        itemScript.ItemData = itemData;

        //�A�C�e���̒��g���C���X�^���X��
        GameObject madeContent = Instantiate(ItemContentPrefab, madeObject.transform);
        //�{���̖��O�ɕύX�i"(Clone)"�Ƃ䂤�ڔ����������j
        madeContent.name = ItemContentPrefab.name;
        //���b�V���̐ݒ�
        madeContent.GetComponent<MeshFilter>().mesh = itemObject.GetComponent<MeshFilter>().mesh;
        //�}�e���A���̐ݒ�
        madeContent.GetComponent<MeshRenderer>().material = itemObject.GetComponent<MeshRenderer>().material;
        return true;
    }



    public GameObject content { get; private set; }
    private float Speed = 2.5f;
    private Rigidbody ownRigidbody;
    public int contentCount { get; private set; }
    private Vector3 moveVelocity;

    //�A�C�e���f�[�^
    public InventoryItem ItemData { get; private set; }

    private void Start()
    {
        //�q�I�u�W�F�N�g��ێ����Ă���
        if (content == null)
        {
            content = transform.GetChild((int)ChildIndex.Content)?.gameObject;
            //�i�[����Ă�����
            if (content != null)
            {
                //�R���e���c���𑝂₷
                contentCount = 1;
            }
            //�Ȃ�����A�C�e���Ȃ̂Ŏ���
            else
            {
                Destroy(gameObject);
            }
        }

        //���W�b�h�{�f�B��ێ�
        ownRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //�{�̂���]�����Ȃ�
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    private void FixedUpdate()
    {
        //�߂��̃A�C�e���֋߂Â�
        //y���ړ���RigidBody�̏d�͂��g�p
        moveVelocity.Scale(new Vector3(1, 0, 1));
        moveVelocity.y = ownRigidbody.velocity.y;
        ownRigidbody.velocity = moveVelocity;
        moveVelocity = Vector3.zero;
    }

    private void OnTriggerStay(Collider other)
    {
        //�A�C�e������Ȃ���Ή������Ȃ�
        if (other.gameObject.tag != "Item") return;
        //�����A�C�e������Ȃ���Ή������Ȃ�
        if (other.name != name) return;

        //�ʂ̃A�C�e���ւ̕���
        Vector3 targetVector = (other.transform.position - transform.position).normalized;
        //Y���͉������Ȃ�
        //targetVector.Scale(new Vector3(1, 0, 1));
        //targetVector.y = ownRigidbody.velocity.y;

        //�^�[�Q�b�g�����݂̐i�s�����Ƒ傫���O���ꍇ�������Ȃ�
        if (Vector3.Angle(moveVelocity, targetVector) >= 90.0f) return;
        //�ړ��x�N�g���̎Z�o
        moveVelocity = (moveVelocity + (targetVector * Speed)).normalized * Speed;
    }

    /// <summary>
    /// �A�C�e���������i�J�E���g�𑝂₷�j
    /// </summary>
    /// <param name="count">������</param>
    public void JoinItem(int count)
    {
        contentCount += count;
    }

    /// <summary>
    /// �A�C�e�����C���x���g���[�ɂ��܂��i�J�E���g�����炷�j
    /// </summary>
    /// <param name="count">���܂���</param>
    /// <returns>���܂�����</returns>
    public int StorageItem(int count)
    {
        if (count > contentCount)
        {
            count = contentCount;
        }
        contentCount -= count;

        //��ɂȂ�����j��
        if (contentCount <= 0)
        {
            Destroy(gameObject);
        }

        return count;
    }
}
