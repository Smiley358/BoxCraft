using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredictionBoxScript : MonoBehaviour
{
    //設置可能時マテリアル
    [SerializeField] private Material canCreatableMaterial;
    //設置不可能時マテリアル
    [SerializeField] private Material canNotCreatableMaterial;

    //表示用BOX
    private GameObject renderBox;
    //表示用BOX
    private BoxBase renderBoxScript;
    //表示用BOXマテリアル
    private Material renderBoxMaterial;

    //一個前に入っていたBOX
    private GameObject previousBox;

    //生成可能フラグ
    public bool IsCreatable { get; private set; }

    void Start()
    {
        IsCreatable = true;
    }

    void Update()
    {
        if (!IsValid()) return;

        if (renderBoxMaterial == null)
        {
            renderBoxMaterial = renderBoxScript?.GetMeshRenderer()?.material;
            if (renderBoxMaterial == null) return;
        }

        //BOXの設置予測可否表示
        if (IsCreatable)
        {
            renderBoxMaterial.color = canCreatableMaterial.color;
        }
        else
        {
            renderBoxMaterial.color = canNotCreatableMaterial.color;
        }
    }

    public void OnTriggerEnter(UnityEngine.Collider other)
    {
        //Player以外は無視
        if (other.name != "Player") return;
        //生成不可
        IsCreatable = false;
    }

    public void OnTriggerExit(UnityEngine.Collider other)
    {
        //Player以外は無視
        if (other.name != "Player") return;
        //生成可能
        IsCreatable = true;
    }

    public void OnTriggerStay(UnityEngine.Collider other)
    {
        //Player以外は無視
        if (other.name != "Player") return;
        IsCreatable = false;
    }

    /// <summary>
    /// 使用可能かどうか
    /// </summary>
    /// <returns></returns>
    public bool IsValid()
    {
        return (renderBox != null) && (renderBoxScript != null);
    }


    /// <summary>
    /// BOXを90度回転させる
    /// </summary>
    public void Rotate()
    {
        if (renderBox == null) return;
        if (renderBoxScript == null) return;
        //回転させる（回転するかしないかはBoxBaseにゆだねる）
        renderBoxScript.Rotate();
    }

    /// <summary>
    /// 設置時の回転角を取得
    /// </summary>
    public Quaternion GetRotate()
    {
        return renderBox!=null ? renderBox.transform.rotation : Quaternion.identity;
    }

    /// <summary>
    /// 表示用のBOXのPrefabをアタッチする
    /// </summary>
    /// <param name="attach">表示用のBOXのPrefab</param>
    public void AttachPrefab(GameObject attach)
    {
        //同じものをアタッチしようとしていたら何もしない
        if (attach == previousBox)
        {
            return;
        }
        else
        {
            //attachされるBoxを保存
            previousBox = attach;
        }
        //現在の表示用BOXをデタッチ
        DetachPrefab();
        //nullなら作成しない
        if (attach == null) return;


        //表示用BOXの生成
        GameObject madeObject = Instantiate(attach, gameObject.transform);
        renderBox = madeObject;
        //レイヤーを自分と同じに（Ignore raycast）
        madeObject.layer = gameObject.layer;
        //ダイナミックオブジェクトに変更
        madeObject.isStatic = false;
        //スクリプトを取得
        renderBoxScript = renderBox.GetComponent<BoxBase>();
        //当たり判定を無効化
        renderBoxScript?.DisableCollition();
        //シェーダーをFadeに
        renderBoxMaterial = renderBoxScript?.GetMeshRenderer()?.material;
        if (renderBoxMaterial != null)
        {
            renderBoxMaterial.SetOverrideTag("RenderType", "Transparent");
            renderBoxMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            renderBoxMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            renderBoxMaterial.SetInt("_ZWrite", 0);
            renderBoxMaterial.DisableKeyword("_ALPHATEST_ON");
            renderBoxMaterial.EnableKeyword("_ALPHABLEND_ON");
            renderBoxMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            renderBoxMaterial.renderQueue = 3000;
        }
    }

    /// <summary>
    /// 表示用のBOXのPrefabをデタッチする
    /// </summary>
    public void DetachPrefab()
    {
        Destroy(renderBox);
        renderBox = null;
        renderBoxScript = null;
    }
}
