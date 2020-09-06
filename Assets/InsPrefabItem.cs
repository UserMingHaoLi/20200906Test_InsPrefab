using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum EM_InsPrefabItem4
{
    Up = 1,
    Down = 2,
    Right = 4,
    Left = 8,
}
public class InsPrefabItem : MonoBehaviour
{
    public int ml_CenterIndex;
    public List<Vector2> ml_Cells;

    private List<int> ml_vDistance;
    [HideInInspector]
    public float maxUp = 0;
    [HideInInspector]
    public float maxDown = 0;
    [HideInInspector]
    public float maxRight = 0;
    [HideInInspector]
    public float maxLeft = 0;

    public static float static_maxX = 0;
    public static float static_maxY = 0;
    public void Init()
    {
        //取最大(最小)XY即可
        foreach (var item in ml_Cells)
        {
            if(item.x == 0 && item.y > maxUp)
            {
                maxUp = item.y;
            }
            if (item.x == 0 && item.y < maxDown)
            {
                maxDown = item.y;
            }
            if (item.y == 0 && item.x > maxRight)
            {
                maxRight = item.x;
            }
            if (item.y == 0 && item.x < maxLeft)
            {
                maxLeft = item.x;
            }
        }
        Debug.Log($"{gameObject.name} {maxUp} {maxDown} {maxRight} {maxLeft}");

        if (maxUp + Mathf.Abs(maxDown) + 1 > static_maxY)
        {
            static_maxY = maxUp + Mathf.Abs(maxDown) + 1;
        }
        if (maxRight + Mathf.Abs(maxLeft) + 1 > static_maxX)
        {
            static_maxX = maxRight + Mathf.Abs(maxLeft) + 1;
        }
    }
}
