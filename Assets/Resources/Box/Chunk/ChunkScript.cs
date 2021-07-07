using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JsonConverter.ChunkJsonConverter;
using UnityEngine;

public partial class ChunkScript : MonoBehaviour
{
    //�����������t���O
    public bool IsTerrainGenerateCompleted { get; private set; }
    //�L���^�C�}�[�������Ă��邩�ǂ���
    public bool IsKillTimerSet { get; private set; }
    //�`�����N�̔z��C���f�b�N�X
    public Index3D worldIndex { get; private set; }

    //�אڃ`�����N
    private ChunkScript[,,] adjacentChunks;
    //�`�����N����Box�f�[�^
    private BoxData[,,] boxDatas;
    //Box�̐����f�[�^
    private string[,,] boxGenerateData;
    //���b�V�������t���O
    private bool isCombineMesh;

    //Box�ɑ΂���ύX�_
    [SerializeField] private List<BoxSaveData> changes;
    //�`�����N��X,Y,Z�T�C�Y
    [SerializeField] private Vector3 size;
    //�`�����N�̒��S���W
    [SerializeField] private Vector3 center;
    //��������I�u�W�F�N�g
    [SerializeField] private GameObject prefab;

    private void Awake()
    {
        //�`�����N����Box�f�[�^�z��
        boxDatas = new BoxData[chunkSize, chunkSize, chunkSize];

        //�`�����N���ɐ�������Box�̃f�[�^
        boxGenerateData = new string[chunkSize, chunkSize, chunkSize];

        //�אڃ`�����N
        adjacentChunks ??= new ChunkScript[3, 3, 3];

        //�`�����N�T�C�Y
        size = new Vector3(chunkSize, chunkSize, chunkSize);

        //���S���W
        center = transform.position;
    }

    private void Start()
    {
        //�`�����N�̕ύX�_�f�[�^�����[�h
        ChunkSaveData load = Converter.LoadSaveData(worldIndex);
        //�Z�[�u�f�[�^������Εێ�
        if ((load != null) && load.Changes != null)
        {
            changes.AddRange(load?.Changes);
        }

        //�R���C�_�[�̃T�C�Y���Z�b�g
        GetComponent<BoxCollider>().size = size;

        //�n�`����
        StartCoroutine(GenerateTerrain());

        //�߂��̃`�����N�𐶐�
        StartCoroutine(GenerateChunkIfNeeded());

        //�P�O�b������Player����̋��������ė��ꂷ���Ă�����폜����
        if (worldIndex.y != 0)
        {
            InvokeRepeating(nameof(KillTimerSetIfNeeded), 0, 10);
        }
        else
        {
            InvokeRepeating(nameof(KillTimerSetIfNeeded), 0, 10);
        }
    }

    private void LateUpdate()
    {
        //���b�V�������t���O�����Ă���
        if (isCombineMesh)
        {
            //���b�V���̍X�V�i�Č����j
            CombineMesh();
            //�t���O��������
            isCombineMesh = false;
        }
    }

    private void OnDestroy()
    {
        //�`�����N�f�[�^�̏�����
        for (int x = 0; x < boxDatas.GetLength(0); x++)
        {
            for (int y = 0; y < boxDatas.GetLength(1); y++)
            {
                for (int z = 0; z < boxDatas.GetLength(2); z++)
                {
                    boxDatas[x, y, z] = null;
                }
            }
        }

        //�`�����N�f�[�^�̕ۑ�
        Converter.UpdateSavedata(worldIndex, changes);

        //�אڃ`�����N�ɔj��ʒm
        for (int direction = (int)Direction.First; direction <= (int)Direction.Max; direction++)
        {
            int x = DirectionOffset[direction][X] + DirectionOffsetCenter[X];
            int y = DirectionOffset[direction][Y] + DirectionOffsetCenter[Y];
            int z = DirectionOffset[direction][Z] + DirectionOffsetCenter[Z];

            adjacentChunks[x, y, z]?.DestroyNotification(Direction.Max - direction);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Player�󂵂��󂯕t���Ȃ�
        if (other.name != "Player") return;
        //�������g��Player�̈ړ��ʒm
        PlayerMoveNotification();
        //Player�̃C���f�b�N�X��������
        PlayerIndex = worldIndex;

        //�S���ʕ����[�v
        for (int direction = (int)Direction.First; direction <= (int)Direction.Max; direction++)
        {
            //�אڃ`�����N�̍��W�����߂�
            int x = DirectionOffset[direction][X] + DirectionOffsetCenter[X];
            int y = DirectionOffset[direction][Y] + DirectionOffsetCenter[Y];
            int z = DirectionOffset[direction][Z] + DirectionOffsetCenter[Z];

            //�`�����N���Ȃ�������
            if (adjacentChunks[x, y, z] == null)
            {
                //�Q�Ƃ��c���Ă���X�N���v�g
                if (!(adjacentChunks[x, y, z] is null))
                {
                    //�Q�Ƃ�����
                    adjacentChunks[x, y, z] = null;
                }

                Vector3 offset = new Vector3(DirectionOffset[direction][X], DirectionOffset[direction][Y], DirectionOffset[direction][Z]);
                offset *= chunkSize;
                Index3D index3D = CalcWorldIndex(center + offset);
                //�S�`�����N�f�[�^����T��
                if (ChunkManagerScript.chunks.ContainsKey(index3D))
                {
                    adjacentChunks[x, y, z] = ChunkManagerScript.chunks[index3D];
                }
            }
            //�ړ��ʒm
            adjacentChunks[x, y, z]?.PlayerMoveNotification();
        }
    }

    private void OnDrawGizmos()
    {
        //���ڂ�������Ȃ��̂Ńf�o�b�O�\��
        if (worldIndex.y == 0)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.gray;
        }
        Gizmos.DrawWireCube(center, size);
    }

    /// <summary>
    /// �n�`�𐶐�����
    /// </summary>
    private IEnumerator GenerateTerrain()
    {
        //�n�\�łȂ����
        if (worldIndex.y != 0)
        {
            //�n�`���������t���O�𗧂Ă�
            IsTerrainGenerateCompleted = true;
            yield break;
        }

        //�o�E���f�B���O�{�b�N�X��X,Y,Z�N�_�ʒu�܂ł̃I�t�Z�b�g
        Vector3 offset = size / 2 - center;

        //�n�\��Box�ƒǉ����ꂽBox�𐶐�
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                //�m�C�Y����
                float noise = Mathf.PerlinNoise(
                    (transform.position.x + x) / mapResolutionHorizontal,
                    (transform.position.z + z) / mapResolutionHorizontal);
                //�n�`�̈�ԏ�̈ʒu
                int top = (int)Mathf.Round(mapResolutionVertical * noise);
                for (int y = 0; y < chunkSize; y++)
                {
                    //�����\��̏ꏊ�ɕύX�_�����邩�擾
                    BoxSaveData change = changes.Find(data => data.Index == new Index3D(x, y, z));
                    GameObject createPrefab = null;
                    //y��top��菬������
                    if (y <= top)
                    {
                        //�n�\��Box�̂�
                        if (y == top)
                        {
                            //�ʏ퐶����prefab�����Ă���
                            createPrefab = prefab;
                        }
                        //�n�`���������̃f�[�^�̂ݕێ�
                        boxGenerateData[x, y, z] = prefab.name;
                    }

                    //�ύX�_����������
                    if (change != null)
                    {
                        //Name������Ή����u����Ă���
                        if (change.Name.Length > 0)
                        {
                            //�ύX���ꂽBox��prefab�����[�h
                            createPrefab = PrefabManager.Instance.GetPrefab(change.Name);
                        }
                        else
                        {
                            //Box�̍폜�f�[�^�������Ƃ��Ȃ̂ŉ����������Ȃ�
                            createPrefab = null;
                        }
                    }

                    //createPrefab��null�łȂ���Ύ����������Z�[�u�f�[�^����̐���
                    if (createPrefab != null)
                    {
                        //����
                        GameObject box = CreateBoxAndBelongToChunk(
                            createPrefab,
                            CalcWorldPositionFromBoxLocalIndex(new Index3D(x, y, z)),
                            Quaternion.identity);
                    }
                }
            }
        }

        //�n�`���������t���O�𗧂Ă�
        IsTerrainGenerateCompleted = true;
        Debug.Log("Generate Terrain : " + worldIndex.ToString());

        //�S�`�����N�̐����҂�
        while (true)
        {
            if (ChunkManagerScript.IsCompleted) break;
            yield return null;
        }

        //�폜���ꂽBox���ӂ�Box�̐���
        var destroys = changes.FindAll(data => data.Name == "");
        foreach (var destroyBox in destroys)
        {
            //���W
            Vector3 position = CalcWorldPositionFromBoxLocalIndex(destroyBox.Index);

            //���ӂɕK�v�ł����Box�𐶐�
            CreateAdjacentBoxIfNeeded(position);
        }

        //�`�����N���̑SBox
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    //�K�v�ɉ����Ė�����������
                    var script = boxDatas[x, y, z]?.Script;
                    if (script != null)
                    {
                        script.DisableIfNeeded();
                    }
                }
            }
            yield return null;
        }

        //���b�V��������
        CombineMesh();
    }

    /// <summary>
    /// �K�v�ɉ����Ď��ӂɃ`�����N�𐶐����ĕێ�����
    /// ������Player�̈ړ��ʒm���o��
    /// </summary>
    private IEnumerator GenerateChunkIfNeeded()
    {
        //�����\��ɏo����Index���X�g
        List<Index3D> createList = new List<Index3D>();

        //direction�����ւ̃I�t�Z�b�g�v�Z
        Func<int, Index3D> calcDirectionOffsetDelegate = (direction) =>
        {
            return new Index3D(
                DirectionOffset[direction][X] + DirectionOffsetCenter[X],
                DirectionOffset[direction][Y] + DirectionOffsetCenter[Y],
                DirectionOffset[direction][Z] + DirectionOffsetCenter[Z]
                );
        };
        //�������W���v�Z����
        Func<int, Vector3> calcCreatePositionDelegate = (direction) =>
        {
            return new Vector3(
                    DirectionOffset[direction][X],
                    DirectionOffset[direction][Y],
                    DirectionOffset[direction][Z]) * chunkSize + center;
        };

        //�S���ʑ���
        for (int direction = (int)Direction.First; direction <= (int)Direction.Max; direction++)
        {
            Index3D arrayIndex = calcDirectionOffsetDelegate(direction);

            //�`�����N���Ȃ�������
            if (adjacentChunks[arrayIndex.x, arrayIndex.y, arrayIndex.z] == null)
            {
                //�Q�Ƃ��c���Ă���X�N���v�g
                if (!(adjacentChunks[arrayIndex.x, arrayIndex.y, arrayIndex.z] is null))
                {
                    //�Q�Ƃ�����
                    adjacentChunks[arrayIndex.x, arrayIndex.y, arrayIndex.z] = null;
                }

                //����������W
                Vector3 position = calcCreatePositionDelegate(direction);
                //��������C���f�b�N�X
                Index3D index3D = CalcWorldIndex(position);
                //�S�`�����N�f�[�^����T��
                if (ChunkManagerScript.chunks.ContainsKey(index3D))
                {
                    //����������ێ�
                    adjacentChunks[arrayIndex.x, arrayIndex.y, arrayIndex.z] = ChunkManagerScript.chunks[index3D];
                }
                else
                {
                    //Player���痣�ꂷ���Ă��Ȃ����
                    if (IsFitIntoFar(index3D))
                    {
                        //�����L���[�֒ǉ�
                        ChunkManagerScript.CreateOrder(position);
                        //�����L���[�֒ǉ������̂ŕۑ�
                        createList.Add(index3D);
                    }
                }
            }
        }

        //�S�`�����N�̐����҂�
        while (true)
        {
            if (ChunkManagerScript.IsCompleted) break;
            yield return null;
        }
        Debug.Log("Create Chunk : " + worldIndex.ToString());

        for (int direction = (int)Direction.First; direction <= (int)Direction.Max; direction++)
        {
            Index3D arrayIndex = calcDirectionOffsetDelegate(direction);

            //�`�����N���Ȃ�������
            if (adjacentChunks[arrayIndex.x, arrayIndex.y, arrayIndex.z] == null)
            {
                //�Q�Ƃ��c���Ă���X�N���v�g
                if (!(adjacentChunks[arrayIndex.x, arrayIndex.y, arrayIndex.z] is null))
                {
                    //�Q�Ƃ�����
                    adjacentChunks[arrayIndex.x, arrayIndex.y, arrayIndex.z] = null;
                }

                //��������C���f�b�N�X
                Index3D index3D = CalcWorldIndex(calcCreatePositionDelegate(direction));
                //�����\�񂵂Ă�����
                if (createList.Contains(index3D))
                {
                    //�S�`�����N�f�[�^����T��
                    while (true)
                    {
                        //��������Ă����Ƃ�
                        if (ChunkManagerScript.chunks.ContainsKey(index3D))
                        {
                            adjacentChunks[arrayIndex.x, arrayIndex.y, arrayIndex.z] = ChunkManagerScript.chunks[index3D];
                            break;
                        }
                        //�������s���Ă����Ƃ�
                        else if (ChunkManagerScript.IsCreateFailed(index3D))
                        {
                            break;
                        }
                        yield return null;
                    }
                }
            }
        }
    }

    /// <summary>
    /// ���b�V��������
    /// </summary>
    private void CombineMesh()
    {
        //Box���烁�b�V���ւ̃A�N�Z�T�[���擾
        IMeshAccessor[] meshAccessors = transform.GetComponentsInChildren<IMeshAccessor>();

        //�}�e���A��
        Dictionary<string, Material> materials = new Dictionary<string, Material>();
        //���}�e���A����MeshFilter���X�g
        Dictionary<string, List<MeshFilter>> meshFilters = new Dictionary<string, List<MeshFilter>>();

        //�SIMeshAccessor�ɑ΂��čs��
        for (int i = 0; i < meshAccessors.Length; i++)
        {
            //�}�e���A��
            Material material = meshAccessors[i].GetMaterial();
            //�}�e���A�����A�}�e���A�����Ȃ�������}�e���A�����O���[�v"NonMaterial"
            string materialName = material == null ? "NonMaterial" : material.name;

            //�}�e���A�����o�^����Ă��Ȃ��ꍇ
            if (!meshFilters.ContainsKey(materialName))
            {
                //�}�e���A���o�^
                List<MeshFilter> filterList = new List<MeshFilter>();
                meshFilters.Add(materialName, filterList);
                materials.Add(materialName, material);
            }

            //���b�V���t�B���^�[�����X�g�ɒǉ�
            meshFilters[materialName].Add(meshAccessors[i].GetMeshFilter());
            //�����_�����O�𖳌���
            var renderer = meshAccessors[i].GetMeshRenderer();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        //�}�e���A�����ƂɃ��b�V��������
        foreach (var meshFilter in meshFilters)
        {
            //�����������b�V����\������Q�[���I�u�W�F�N�g
            GameObject combineObject = null;
            //�Q�[���I�u�W�F�N�g��
            string gameObjectName = "Combined:" + meshFilter.Key;
            //���ɂ���Ύ擾
            combineObject = transform.Find(gameObjectName)?.gameObject;

            //������������
            if (combineObject == null)
            {
                combineObject = new GameObject();
                combineObject.name = gameObjectName;
                combineObject.transform.SetParent(transform);
                combineObject.transform.SetAsFirstSibling();
            }

            //MeshFilter���擾�A�Ȃ���΃A�^�b�`
            MeshFilter combinedMeshFilter = combineObject.GetComponent<MeshFilter>();
            if (combinedMeshFilter == null)
            {
                combinedMeshFilter = combineObject.AddComponent<MeshFilter>();
            }
            //MeshRenderer���擾�A�Ȃ���΃A�^�b�`
            MeshRenderer combinedMeshRenderer = combineObject.GetComponent<MeshRenderer>();
            if (combinedMeshRenderer == null)
            {
                combinedMeshRenderer = combineObject.AddComponent<MeshRenderer>();
            }

            //�������郁�b�V���̔z��
            List<MeshFilter> combineMeshFilters = meshFilter.Value;
            //�����p�\����
            CombineInstance[] combine = new CombineInstance[combineMeshFilters.Count];

            //�������郁�b�V���̏���CombineInstance�ɒǉ�
            for (int i = 0; i < combineMeshFilters.Count; i++)
            {
                combine[i].mesh = combineMeshFilters[i].sharedMesh;
                combine[i].transform = combineMeshFilters[i].transform.localToWorldMatrix;
            }

            //�����������b�V�����쐬�����Q�[���I�u�W�F�N�g�A�^�b�`
            combinedMeshFilter.mesh = new Mesh();
            combinedMeshFilter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            combinedMeshFilter.mesh.CombineMeshes(combine);

            //�����������b�V���Ƀ}�e���A�����A�^�b�`
            combinedMeshRenderer.material = materials[meshFilter.Key];
        }
    }

    /// <summary>
    /// Player���`�����N���ړ������Ƃ��ɌĂ΂��
    /// </summary>
    private void PlayerMoveNotification()
    {
        //�L���^�C�}�[�������Ă�����
        if (IsKillTimerSet)
        {
            //�폜�^�C�}�[���L�����Z��
            Debug.Log("Kill Timer Cancel: " + worldIndex.ToString());
            CancelInvoke(nameof(DestroyThis));
            IsKillTimerSet = false;
        }
        //�K�v�ɉ����ă`�����N�̐����Ƌߗ׃`�����N�ֈړ��ʒm
        StartCoroutine(GenerateChunkIfNeeded());
    }

    /// <summary>
    /// ���Ӄ`�����N���폜���ꂽ�ۂɌĂ΂��
    /// </summary>
    /// <param name="direction"></param>
    private void DestroyNotification(Direction direction)
    {
        int x = DirectionOffset[(int)direction][X] + DirectionOffsetCenter[X];
        int y = DirectionOffset[(int)direction][Y] + DirectionOffsetCenter[Y];
        int z = DirectionOffset[(int)direction][Z] + DirectionOffsetCenter[Z];

        adjacentChunks[x, y, z] = null;
    }

    /// <summary>
    /// �`�����N�̒��S���W��Player��
    /// chunkSize * far����Ă����ꍇ
    /// �L���^�C�}�[������
    /// </summary>
    private void KillTimerSetIfNeeded()
    {
        //�L���^�C�}�[�������Ă���Ƃ��͏������Ȃ�
        if (IsKillTimerSet) return;

        //Player�Ɨ��ꂷ���Ă����
        if (!IsFitIntoFar(worldIndex))
        {
            //�폜�\��
            Debug.Log("Kill Timer Begin: " + worldIndex.ToString());
            Invoke(nameof(DestroyThis), 10);
            IsKillTimerSet = true;
        }
    }

    /// <summary>
    /// �������폜
    /// �폜�ƈꏏ�Ƀf�[�^�x�[�X���������
    /// �Q�ƃG���[�΍��p
    /// </summary>
    private void DestroyThis()
    {
        ChunkManagerScript.ForceDestroyChunk(gameObject);
    }

    /// <summary>
    /// �K�v�ɉ����Ď��ӂ�Box�𐶐�����
    /// </summary>
    /// <param name="position">���S���W</param>
    /// <returns>�͈͊O�`�����N</returns>
    private List<ChunkScript> CreateAdjacentBoxIfNeeded(Vector3 position)
    {
        //�͂ݏo��Box�̏�������`�����N(�����̂̓�����R�ȏ�ɂ͂Ȃ�Ȃ�)
        List<ChunkScript> chunks = new List<ChunkScript>();
        //�אڂ���U������Box
        BoxData[] createDatas = new BoxData[6];
        //�אڂ���6������Box��K�v�ɉ����Ď�������
        for (int direction = (int)Direction.Top, i = 0; direction <= (int)Direction.Bottom; direction++, i++)
        {
            //�אڂ���Box�̎擾(��������True)
            BoxData affiliationBox = GetAdjacentBox(position, (Direction)direction, true, true);
            //�ꎞ�ۑ�
            createDatas[i] = affiliationBox;
            //��������������Ȃ��Ƃ�
            if ((affiliationBox != null) && (affiliationBox.Script.parentChunk != this))
            {
                chunks.Add(affiliationBox.Script.parentChunk);
            }
        }
        //��������Box�ɑ΂��Ď����������v�Z
        foreach (var boxData in createDatas)
        {
            boxData?.Script?.DisableIfNeeded();
        }

        return chunks;
    }

    /// <summary>
    /// Box�𐶐����ăf�[�^��ۑ�����
    /// </summary>
    /// <param name="prefab">��������Box��prefab</param>
    /// <param name="position">�����ʒu</param>
    /// <param name="rotation">��]</param>
    public GameObject CreateBoxAndBelongToChunk(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        //�����C���f�b�N�X�̌v�Z
        Index3D index = CalcLocalIndexFromBoxWorldPosition(position);
        //�����\�łȂ���Ή������Ȃ�
        if (!CanSpawnBox(index)) return null;

        //����
        GameObject box = BoxBase.Create(this, prefab, position, rotation);
        //�e��ݒ�
        box.transform.SetParent(transform);
        //�f�[�^��ۑ�
        boxDatas[index.x, index.y, index.z] = new BoxData(box, box.GetComponent<BoxBase>());
        //���b�V���Č����t���O�𗧂Ă�
        isCombineMesh = true;

        //�`�����N�ł��łɕύX���s���Ă��邩�擾
        BoxSaveData change = changes.Find(data => data.Index == index);
        //�ύX������΍폜
        changes.Remove(change);
        //��������Box�����������Ɠ������̂łȂ����
        if (boxGenerateData[index.x, index.y, index.z] != prefab.name)
        {
            //�ύX����ǉ�
            changes.Add(new BoxSaveData(prefab.name, index, box.transform.rotation));
        }

        return box;
    }

    /// <summary>
    /// �ߗׂ�Box���擾����
    /// </summary>
    /// <param name="baseBox">�Box</param>
    /// <param name="direction">�Box����ǂ̕�����Box��</param>
    /// <param name="autoCreate">�����������邩�ǂ���</param>
    /// <param name="acrossChunk">�`�����N���܂����ŏ������邩�ǂ���</param>
    /// <returns>direction������Box</returns>
    public BoxData GetAdjacentBox(Vector3 position, Direction direction, bool autoCreate = false, bool acrossChunk = false)
    {
        //���[�J�����W
        Vector3 localPosition = new Vector3(
            position.x + (chunkSize / 2),
            position.y + (chunkSize / 2),
            position.z + (chunkSize / 2)) - center;

        //direction�����ւ̃I�t�Z�b�g
        Vector3 offset = new Vector3(
            DirectionOffset[(int)direction][X] * boxSize,
            DirectionOffset[(int)direction][Y] * boxSize,
            DirectionOffset[(int)direction][Z] * boxSize);

        //�I�t�Z�b�g�����Z
        localPosition += offset;

        //�C���f�b�N�X
        Index3D index = new Index3D(
            (int)Mathf.Floor(localPosition.x), 
            (int)Mathf.Floor(localPosition.y), 
            (int)Mathf.Floor(localPosition.z));

        //���`�����N�O�Ȃ�
        if (!index.IsFitIntoRange(0, chunkSize - 1))
        {
            //�`�����N�Ԃ��܂����ŏ�������Ƃ�
            if (acrossChunk)
            {
                //���Y�`�����N���̃C���f�b�N�X�ɂȂ���
                Index3D convertIndex = new Index3D(
                (index.x + chunkSize) % chunkSize,
                (index.y + chunkSize) % chunkSize,
                (index.z + chunkSize) % chunkSize);
                //����������o��
                Index3D directionOffset = new Index3D(
                    //1,0,-1�ɒ���
                    Math.Sign((int)(index.x / (float)chunkSize)) + DirectionOffsetCenter[X],
                    Math.Sign((int)(index.y / (float)chunkSize)) + DirectionOffsetCenter[Y],
                    Math.Sign((int)(index.z / (float)chunkSize)) + DirectionOffsetCenter[Z]);

                ChunkScript acrossChunkScript = adjacentChunks[directionOffset.x, directionOffset.y, directionOffset.z];
                //�ߗׂ̃`�����N�f�[�^���Ȃ��ꍇ
                if (acrossChunkScript == null)
                {
                    //�ۑ�����Ă��Ȃ�������������Ȃ��̂Ń}�l�[�W���[����T��
                    ChunkManagerScript.chunks.TryGetValue(worldIndex + directionOffset, out acrossChunkScript);
                    //�ۑ�
                    adjacentChunks[directionOffset.x, directionOffset.y, directionOffset.z] = acrossChunkScript;
                }

                //���Y�`�����N��Box�f�[�^���擾
                return acrossChunkScript?.GetBox(convertIndex, autoCreate);
            }
            else
            {
                //�`�����N���܂����ŏ������Ȃ��̂Ŗ�������
                return null;
            }
        }

        //Box��Ԃ�
        return GetBox(index, autoCreate);
    }

    /// <summary>
    /// Box���擾����
    /// </summary>
    /// <param name="index">�擾������Box�̃C���f�b�N�X</param>
    /// <param name="autoCreate">�����ҋ@�̏ꍇ��������̂��ǂ���</param>
    /// <returns>Box�f�[�^</returns>
    public BoxData GetBox(Index3D index, bool autoCreate = false)
    {
        //�C���f�b�N�X�O�ł���΃`�����N�O�Ȃ̂�Box�����݂��Ȃ�����
        if (!index.IsFitIntoRange(0, chunkSize - 1)) return null;

        //���Y�C���f�b�N�X�̃{�b�N�X�f�[�^���擾
        BoxData boxData = boxDatas?[index.x, index.y, index.z];
        //Box�����݂����A���������iautoCreate�j��True�Ȃ�
        if ((boxData == null) && autoCreate)
        {
            //�ύX�f�[�^
            BoxSaveData change = changes.Find(data => data.Index == index);
            //prefab��
            string prefabName = null;
            //��]
            Quaternion rotation = Quaternion.identity;

            //�ύX�_������
            if (change != null)
            {
                //�폜����Ȃ��Ƃ�
                prefabName = change.Name;
                rotation = change.Rotation;
            }
            //�ύX�_���Ȃ�
            else
            {
                //���������󋵂��擾
                prefabName = boxGenerateData?[index.x, index.y, index.z];
            }

            //prefabName���󂶂�Ȃ�������
            if (!string.IsNullOrEmpty(prefabName))
            {
                //���[���h���W
                Vector3 worldPosition = CalcWorldPositionFromBoxLocalIndex(index);
                //��������Ă��Ȃ������Ȃ̂ō��
                var box = CreateBoxAndBelongToChunk(PrefabManager.Instance.GetPrefab(prefabName), worldPosition, rotation);
                boxData = new BoxData(box, box.GetComponent<BoxBase>());
            }
        }

        //Box��Ԃ�
        return boxData;
    }


    /// <summary>
    /// �אڂ���Box�����邩�m�F����
    /// �`�����N�O�Ȃ瓖�Y�`�����N�����m�F
    /// </summary>
    /// <param name="baseBox">�Box</param>
    /// <param name="direction">�Box����ǂ̕�����Box��</param>
    /// <returns>direction������Box�����݂��邩</returns>
    public bool IsAdjacetBoxExist(GameObject baseBox, Direction direction)
    {
        //baseBox���i�[����Ă���R�����z��̃C���f�b�N�X���v�Z
        int x = (int)Mathf.Floor(baseBox.transform.localPosition.x + (chunkSize / 2 - boxSize / 2));
        int y = (int)Mathf.Floor(baseBox.transform.localPosition.y + (chunkSize / 2 - boxSize / 2));
        int z = (int)Mathf.Floor(baseBox.transform.localPosition.z + (chunkSize / 2 - boxSize / 2));

        //baseBox����direction������Box�̃C���f�b�N�X���v�Z
        x += DirectionOffset[(int)direction][X];
        y += DirectionOffset[(int)direction][Y];
        z += DirectionOffset[(int)direction][Z];

        //�C���f�b�N�X��
        Index3D index = new Index3D(x, y, z);

        //���`�����N�O�Ȃ�
        if (!index.IsFitIntoRange(0, chunkSize - 1))
        {
            //���Y�`�����N���̃C���f�b�N�X�ɂȂ���
            Index3D convertIndex = new Index3D(
            (index.x + chunkSize) % chunkSize,
            (index.y + chunkSize) % chunkSize,
            (index.z + chunkSize) % chunkSize);
            //����������o��
            Index3D directionOffset = new Index3D(
                //1,0,-1�ɒ���
                Math.Sign((int)(index.x / (float)chunkSize)) + DirectionOffsetCenter[X],
                Math.Sign((int)(index.y / (float)chunkSize)) + DirectionOffsetCenter[Y],
                Math.Sign((int)(index.z / (float)chunkSize)) + DirectionOffsetCenter[Z]);

            //���Y�`�����N��Box�f�[�^���擾
            return adjacentChunks[directionOffset.x, directionOffset.y, directionOffset.z]?.IsBoxExist(convertIndex) ?? false;
        }

        //Box�����݂��Ă��邩��Ԃ�
        return IsBoxExist(index);
    }

    /// <summary>
    /// Box���폜
    /// </summary>
    /// <param name="box"></param>
    public void DestroyBox(GameObject box)
    {
        //�C���f�b�N�X�̌v�Z
        Index3D index = new Index3D(
                                (int)Mathf.Floor(box.transform.localPosition.x + (chunkSize / 2 - boxSize / 2)),
                                (int)Mathf.Floor(box.transform.localPosition.y + (chunkSize / 2 - boxSize / 2)),
                                (int)Mathf.Floor(box.transform.localPosition.z + (chunkSize / 2 - boxSize / 2))
                            );

        //�͈͊O�͉������Ȃ�
        if (!index.IsFitIntoRange(0, chunkSize - 1)) return;

        //���ӂɎ�������
        CreateAdjacentBoxIfNeeded(box.transform.position);


        //�ێ��f�[�^������
        boxDatas[index.x, index.y, index.z] = null;
        //�폜
        Destroy(box);
        //���b�V���Č����t���O�𗧂Ă�
        isCombineMesh = true;

        //�`�����N�ł��łɕύX���s���Ă��邩�擾
        BoxSaveData change = changes.Find(data => data.Index == index);
        //�ύX������΍폜
        changes.Remove(change);
        //�������ɉ����Ȃ������Ƃ���ł���ΕύX��ێ����Ȃ�
        if (!string.IsNullOrEmpty(boxGenerateData[index.x, index.y, index.z]))
        {
            //�ǉ�
            changes.Add(new BoxSaveData("", index, box.transform.rotation));
        }
    }
}
