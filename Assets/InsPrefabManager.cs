using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class InsPrefabManager : MonoBehaviour
{
    public Texture2D ml_TextureInsTest;//图源
    public GameObject ml_Cube_White;//白色方块
    public GameObject ml_Cube_Black;//黑色方块
    public GameObject ml_Cube_Empt;//空物体
    public GameObject ml_Cube_Ins;//组合Prefab预制件的方块
    Color32[] ml_colors;//图片上的像素
    int ml_width;//图片的宽
    int ml_height;//图片的高

    Color32 ml_Color_Black = new Color32(0, 0, 0, 255);
    Color32 ml_Color_White = new Color32(255, 255, 255, 255);
    List<InsPrefabItem> ml_vInsPrefabItem = new List<InsPrefabItem>();//预制体库
    private void Awake()
    {
        //InitIns();
        InitMap();
        LugsTest();
        //开始随机(远端递归) -起始点 0,0
        StartCoroutine(StartIns());
    }
    #region InitIns
    public List<List<Vector2>> prefabIns = new List<List<Vector2>>()
    {
        new List<Vector2>()
        {
            new Vector2(0,0),
        },
        new List<Vector2>()
        {
            new Vector2(0,0),
            new Vector2(0,1),
        },
        new List<Vector2>()
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(0,-1),
        },
        new List<Vector2>()
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,0),
        },
        new List<Vector2>()
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(0,2),
            new Vector2(1,0),
        },
        new List<Vector2>()
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(0,-1),
            new Vector2(1,0),
        },
        new List<Vector2>()
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(0,-1),
            new Vector2(1,0),
            new Vector2(-1,0),
        },
        new List<Vector2>()
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(-1,0),
            new Vector2(1,0),
            new Vector2(2,0),
        },
    };
    private void InitIns()
    {
        int offSetx = 0;
        foreach (var item in prefabIns)
        {
            GameObject empt = Instantiate(ml_Cube_Empt);
            empt.transform.parent = transform;
            empt.transform.position = new Vector3(offSetx, 0, 0);
            empt.transform.rotation = Quaternion.identity;
            foreach (var v3 in item)
            {
                GameObject cube = Instantiate(ml_Cube_Ins);
                cube.transform.parent = empt.transform;
                cube.transform.localPosition = new Vector3(v3.x, v3.y, 0);
                cube.transform.localRotation = Quaternion.identity;
            }
            offSetx += 10;
        }
    }
    #endregion
    #region InitMap
    private void InitMap()
    {
        ml_colors = ml_TextureInsTest.GetPixels32();
        ml_width = ml_TextureInsTest.width;
        ml_height = ml_TextureInsTest.height;
        ml_vCellType = new List<EM_CellType>(ml_width * ml_height);
        for (int i = 0; i < ml_height; i++)
        {
            for (int j = 0; j < ml_width; j++)
            {
                Color32 color32 = ml_colors[ml_height * i + j];//所以是从坐下开始,每个高度读满
                GameObject cube = null;
                if (IsCorss(color32))
                {
                    cube = Instantiate(ml_Cube_Black);
                    ml_vCellType.Add(EM_CellType.Null);
                }
                else
                {
                    cube = Instantiate(ml_Cube_White);
                    ml_vCellType.Add(EM_CellType.NoTaget);
                }
                cube.name = $"ml_height * i + j | {ml_height * i} + {j}";
                cube.transform.parent = transform;
                cube.transform.position = new Vector3(j, i, 0);
                cube.transform.rotation = Quaternion.identity;
            }
        }
    }

    bool IsCorss(Color32 color32)
    {
        bool result = false;
        if (ml_Color_White.Equals(color32))
        {
            result = false;
            Debug.Log(color32);
        }
        else if (ml_Color_Black.Equals(color32))
        {
            result = true;
            Debug.Log(color32);
        }
        else
        {
            Debug.LogError(color32);
        }
        return result;
    }
    #endregion
    #region 加载山脉库
    private void LugsTest()
    {
        string path = "Assets\\Resource\\Folder1";
        GetAllPrefabs(path);
    }

    private void GetAllPrefabs(string directory)
    {
        if (string.IsNullOrEmpty(directory) || !directory.StartsWith("Assets"))
            throw new Exception("folderPath");

        string[] files = Directory.GetFiles(directory);
        foreach (var file in files)
        {
            if (!file.EndsWith(".prefab")) continue;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(file);
            if (!prefab.activeSelf) continue;//不进入随机库
            InsPrefabItem insPrefab = prefab.GetComponent<InsPrefabItem>();
            insPrefab.Init();
            ml_vInsPrefabItem.Add(insPrefab);
        }
        ml_vInsPrefabItem.Reverse();
    }
    #endregion
    Vector2 ml_CurRun = new Vector2(-1, 0);
    List<Vector2> ml_StackV2 = new List<Vector2>();
    List<EM_InsPrefabItem4> ml_StackItem4 = new List<EM_InsPrefabItem4>();

    private IEnumerator StartIns()
    {
        Vector2 v2Cur = ml_CurRun;
        Vector2 v2Pre = ml_CurRun;
        bool bIns = false;
        while (!(ml_StackV2.Count == 0 
            && (int)ml_CurRun.x == ml_width - 1
            && (int)ml_CurRun.y == ml_height - 1))
        {
            if(bIns)
            {
                yield return null;
            }
            v2Pre = v2Cur;
            if(v2Pre == new Vector2(-1,0))
            {
                v2Pre = Vector2.zero;
            }

            if (ml_StackV2.Count > 0)
            {
                v2Cur = ml_StackV2.Last();
                item4 = ml_StackItem4.Last();
                ml_StackV2.RemoveAt(ml_StackV2.Count - 1);
                ml_StackItem4.RemoveAt(ml_StackItem4.Count - 1);
            }
            else
            {
                if(ml_CurRun.x == ml_width - 1)
                {
                    ml_CurRun.x = 0;
                    ml_CurRun.y++;
                }
                else
                {
                    ml_CurRun.x++;
                }
                v2Cur = ml_CurRun;
                RefItem4(v2Cur, v2Pre);
            }
            bIns = TryInsV2(v2Cur);
        }


        yield break;
    }
    //你是上一个格子的什么方向
    EM_InsPrefabItem4 item4 = 0;
    private void RefItem4(Vector2 cur, Vector2 pre)
    {
        item4 = 0;
        if (cur == pre)
        {//同位置就是四向
            item4 |= EM_InsPrefabItem4.Up;
            item4 |= EM_InsPrefabItem4.Down;
            item4 |= EM_InsPrefabItem4.Right;
            item4 |= EM_InsPrefabItem4.Left;
        }

        if(cur.y > pre.y)
        {
            item4 |= EM_InsPrefabItem4.Up;
        }
        if (cur.y < pre.y)
        {
            item4 |= EM_InsPrefabItem4.Down;
        }
        if (cur.x > pre.x)
        {
            item4 |= EM_InsPrefabItem4.Right;
        }
        if (cur.x < pre.x)
        {
            item4 |= EM_InsPrefabItem4.Left;
        }
        if(item4 == 0)
        {
            throw new Exception($"RefItem4 0 {cur} {pre}");
        }
    }
    enum EM_CellType
    {
        Null,//未处理
        Inst,//已生成
        NoTaget,//不是目标
    }
    List<EM_CellType> ml_vCellType;//地上到底有什么 , 
    private bool TryInsV2(Vector2 v2)
    {
        //从最大的开始匹配周围格子
        //到这里其实已经被筛选过了, 所以直接根据方向尝试生成就可以了
        bool result = false;
        if((int)(v2.y * ml_height + v2.x) >= ml_vCellType.Count)
        {
            Debug.Log(1);
        }
        EM_CellType type = ml_vCellType[(int)(v2.y * ml_height + v2.x)];
        if (type == EM_CellType.Null)
        {
            bool bHas = false;
            bool bIns = false;
            InsPrefabItem insItem = null;
            for (int i = 0; i < 4; i++)
            {
                if (!bHas && (item4 & EM_InsPrefabItem4.Up) != 0)
                {
                    item4 ^= EM_InsPrefabItem4.Up;
                    ml_vInsPrefabItem.Sort((l, r) =>
                    {

                        if (l.maxDown > r.maxDown)
                        {
                            return -1;
                        }
                        if (l.maxDown < r.maxDown)
                        {
                            return 1;
                        }

                        if(l.ml_Cells.Count > r.ml_Cells.Count)
                        {
                            return -1;
                        }
                        if (l.ml_Cells.Count < r.ml_Cells.Count)
                        {
                            return 1;
                        }
                        return 0;
                    });
                    bHas = true;
                }
                if (!bHas && (item4 & EM_InsPrefabItem4.Down) != 0)
                {
                    item4 ^= EM_InsPrefabItem4.Down;
                    ml_vInsPrefabItem.Sort((l, r) =>
                    {

                        if (l.maxUp > r.maxUp)
                        {
                            return -1;
                        }
                        if (l.maxUp < r.maxUp)
                        {
                            return 1;
                        }
                        return 0;
                    });
                    bHas = true;
                }
                if (!bHas && (item4 & EM_InsPrefabItem4.Right) != 0)
                {
                    item4 ^= EM_InsPrefabItem4.Right;
                    ml_vInsPrefabItem.Sort((l, r) =>
                    {

                        if (l.maxLeft > r.maxLeft)
                        {
                            return -1;
                        }
                        if (l.maxLeft < r.maxLeft)
                        {
                            return 1;
                        }
                        return 0;
                    });
                    bHas = true;
                }
                if (!bHas && (item4 & EM_InsPrefabItem4.Left) != 0)
                {
                    item4 ^= EM_InsPrefabItem4.Left;
                    ml_vInsPrefabItem.Sort((l, r) =>
                    {

                        if (l.maxRight > r.maxRight)
                        {
                            return -1;
                        }
                        if (l.maxRight < r.maxRight)
                        {
                            return 1;
                        }
                        return 0;
                    });
                    bHas = true;
                }

                if (bHas)
                {
                    foreach (var item in ml_vInsPrefabItem)
                    {
                        foreach (var cell in item.ml_Cells)
                        {
                            bIns = true;
                            if (v2.x + cell.x >= ml_width) continue;
                            if (v2.y + cell.y >= ml_height) continue;
                            Vector2 offSetCell = new Vector2(v2.x + cell.x, v2.y + cell.y);
                            if(ml_vCellType[(int)(offSetCell.y * ml_height + offSetCell.x)] != EM_CellType.Null)
                            {
                                bIns = false;
                                break;
                            }
                        }
                        if (bIns)
                        {
                            insItem = item;
                            break;
                        }
                    }
                    if(bIns)
                    {
                        List<InsPrefabItem> RandomList = RandomSort(ml_vInsPrefabItem);
                        //RandomList.Remove(insItem);
                        foreach (var item in RandomList)
                        {
                            bIns = true;
                            foreach (var cell in item.ml_Cells)
                            {
                                if (v2.x + cell.x >= ml_width) continue;
                                if (v2.y + cell.y >= ml_height) continue;
                                Vector2 offSetCell = new Vector2(v2.x + cell.x, v2.y + cell.y);
                                if (ml_vCellType[(int)(offSetCell.y * ml_height + offSetCell.x)] != EM_CellType.Null)
                                {
                                    bIns = false;
                                    break;
                                }
                            }
                            if (bIns)
                            {
                                insItem = item;
                                break;
                            }
                        }
                        bIns = true;
                    }
                }
                if (bIns)
                {
                    break;
                }
            }

            //Create
            if(bIns && insItem != null)
            {
                GameObject goIns = Instantiate<GameObject>(insItem.gameObject);
                goIns.name = $"v2 = {v2}";
                goIns.transform.parent = transform;
                goIns.transform.position = new Vector3(v2.x,v2.y,-1);
                goIns.transform.rotation = Quaternion.identity;
                //更改Type
                foreach (var cell in insItem.ml_Cells)
                {
                    if (v2.x + cell.x >= ml_width) continue;
                    if (v2.y + cell.y >= ml_height) continue;
                    Vector2 offSetCell = new Vector2(v2.x + cell.x, v2.y + cell.y);
                    ml_vCellType[(int)(offSetCell.y * ml_height + offSetCell.x)] = EM_CellType.Inst;
                    //周边入栈
                    List<Vector2> tempCell = new List<Vector2>();
                    for (int i = (int)(-InsPrefabItem.static_maxY); i <= InsPrefabItem.static_maxY; i++)
                    {
                        for (int j = (int)(-InsPrefabItem.static_maxX); j <= InsPrefabItem.static_maxX; j++)
                        {
                            if (offSetCell.y + i >= ml_height || offSetCell.y + i < 0) continue;
                            if (offSetCell.x + j >= ml_width || offSetCell.x + j < 0) continue;
                            tempCell.Add(new Vector2(offSetCell.x + j, offSetCell.y + i));
                        }
                    }
                    tempCell.Sort((l, r) =>
                    {
                        float absX = Mathf.Abs(l.x - offSetCell.x);
                        float absY = Mathf.Abs(l.y - offSetCell.y);
                        float absSubL = absX + absY;
                        absX = Mathf.Abs(r.x - offSetCell.x);
                        absY = Mathf.Abs(r.y - offSetCell.y);
                        float absSubR = absX + absY;
                        if (absSubL > absSubR)
                        {
                            return -1;
                        }
                        if (absSubL < absSubR)
                        {
                            return 1;
                        }
                        return 0;
                    });
                    List<EM_InsPrefabItem4> tempItem4 = new List<EM_InsPrefabItem4>();
                    foreach (var item in tempCell)
                    {
                        RefItem4(item,offSetCell);
                        tempItem4.Add(item4);
                    }
                    ml_StackV2.AddRange(tempCell);
                    ml_StackItem4.AddRange(tempItem4);
                }
            }

        }
        return result;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    System.Random random = new System.Random();
    private List<T> RandomSort<T>(List<T> list)
    {
        var newList = new List<T>();
        foreach (var item in list)
        {
            int nRandom = random.Next(0,newList.Count + 1);
            newList.Insert(nRandom, item);
        }
        return newList;
    }
}
