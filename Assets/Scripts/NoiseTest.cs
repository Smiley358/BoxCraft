using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NoiseTest : MonoBehaviour
{
    [System.Serializable]
    public class TestParm
    {
        public int x;
        public int y;
        public int z;
        public int offset;

        public float scale;
        public float height;
        public float power;
    }

    [System.Serializable]
    public class NoiseDatas
    {
        public List<TestParm> List;
    }


    List<int[]> noise;

    public List<NoiseDatas> noiseParm;

    // Start is called before the first frame update
    void Start()
    {
        noise = new List<int[]>();
        //noiseParm = new List<TestParm>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        noise.Clear();
        foreach (var parms in noiseParm)
        {
            var noises = new int[128];
            for (int i = 0; i < parms.List.Count; i++)
            {
                for(int j = 0; j < 128; j++)
                {
                    noises[j] += ChunkScript.Noise.GetNoiseInt(
                    parms.List[i].x + j,
                    parms.List[i].y,
                    parms.List[i].z,
                    parms.List[i].scale,
                    parms.List[i].height,
                    parms.List[i].power)+
                    parms.List[i].offset;
                }
            }
            noise.Add(noises);
        }
    }

    Color[] color = new Color[]{
        Color.red,
        Color.green,
        Color.yellow,
        Color.blue,
        Color.cyan,
        Color.magenta
        };

    private void OnDrawGizmos()
    {
        int c = 0;
        foreach (var n in noise)
        {
            Gizmos.color = color[c % color.Length];
            c++;
            for (int i = 0; i < n.Length - 1; i++)
            {
                Gizmos.DrawLine(new Vector3(i * 3, n[i], 0), new Vector3((i + 1) * 3, n[i + 1], 0));
            }
        }
    }
}
