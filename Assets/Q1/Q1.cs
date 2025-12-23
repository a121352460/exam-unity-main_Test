using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

/**
界面上有三个输入框，分别对应 X,Y,Z 的值，请实现 {@link Q1.onGenerateBtnClick} 函数，生成一个 10 × 10 的可控随机矩阵，并显示到界面上，矩阵要求如下：
1. {@link COLORS} 中预定义了 5 种颜色
2. 每个点可选 5 种颜色中的 1 种
3. 按照从左到右，从上到下的顺序，依次为每个点生成颜色，(0, 0)为左上⻆点，(9, 9)为右下⻆点，(0, 9)为右上⻆点
4. 点(0, 0)随机在 5 种颜色中选取
5. 其他各点的颜色计算规则如下，设目标点坐标为(m, n）：
    a. (m, n - 1)所属颜色的概率为基准概率加 X%
    b. (m - 1, n)所属颜色的概率为基准概率加 Y%
    c. 如果(m, n - 1)和(m - 1, n)同色，则该颜色的概率为基准概率加 Z%
    d. 其他颜色平分剩下的概率
*/

public class Q1 : MonoBehaviour
{
    private static readonly Color[] COLORS = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        new Color(1f, 0.5f, 0f) // Orange
    };

    private const int Count = 10;
    // 每个格子的大小
    private const float GRID_ITEM_SIZE = 75f;

    [SerializeField]
    private InputField xInputField = null;

    [SerializeField]
    private InputField yInputField = null;

    [SerializeField]
    private InputField zInputField = null;

    [SerializeField]
    private Transform gridRootNode = null;

    [SerializeField]
    private GameObject gridItemPrefab = null;
    
    GameObject[,] gridItem = new GameObject[Count,Count];
    
    //颜色概率
    List<float> colorRataList = new List<float>();
    //是否已经设置颜色
    List<bool> colorRateState = new List<bool>();
    
    //每个格子的颜色信息
    private int[,] gridItemColorValue = new int[Count,Count];
    //基础概率
    float BaseRate = 1f/COLORS.Length;
    //是否已经初始化格子
    bool initialized = false;

    private int inputXValue, inputYValue, inputZValue;
    public void OnGenerateBtnClick()
    {
        
        // TODO: 请在此处开始作答
        inputXValue = int.TryParse(xInputField.text,NumberStyles.Integer,CultureInfo.InvariantCulture,out var resultx) ? resultx : 0;
        inputYValue = int.TryParse(yInputField.text,NumberStyles.Integer,CultureInfo.InvariantCulture,out var resulty) ? resulty : 0;
        inputZValue = int.TryParse(zInputField.text,NumberStyles.Integer,CultureInfo.InvariantCulture,out var resultz) ? resultz : 0;
        if(!initialized)
        {
            float offsetx = - Count*GRID_ITEM_SIZE/2+ GRID_ITEM_SIZE/2;
            float offsety = - Count*GRID_ITEM_SIZE/2- GRID_ITEM_SIZE/2;
            for (int y = 0; y < Count; y++)
            {
                for (int x = 0; x < Count; x++)
                {
                    gridItem[y, x] = GameObject.Instantiate(gridItemPrefab);
                    gridItem[y, x].transform.SetParent(gridRootNode);
                    gridItem[y, x].name = $"{y}{x}";
                    gridItem[y, x].transform.localScale = Vector3.one;
                    gridItem[y, x].transform.localPosition = new Vector3(x * GRID_ITEM_SIZE +offsetx,(Count-y) * GRID_ITEM_SIZE +offsety,0);
                    
                }
            }
            initialized = true;
            for (int i = 0; i < COLORS.Length; i++)
            {
                colorRataList.Add(BaseRate);
                colorRateState.Add(false);
            }
        }
       
        RefreshColor();
    }
    //重置颜色
    void ResetColorRate()
    {
        for (int i = 0; i < colorRataList.Count; i++)
        {
            colorRataList[i] = 0;
            colorRateState[i] = false;
        }
    }
    int GetColor(int x, int y)
    {
        float lastRate = 0f;
        if (x == 0 && y == 0)
        {
            return Random.Range(0, COLORS.Length);
        }
        else if (x > 0 && y > 0)
        {
            ResetColorRate();
            var yx_1  = gridItemColorValue[y, x - 1];
            var y_1x = gridItemColorValue[y-1, x];
           
            if (yx_1 == y_1x)
            {
                colorRataList[yx_1] = BaseRate + inputZValue / 100f;
                colorRateState[yx_1] = true;
                lastRate = (1 - colorRataList[yx_1]) / (colorRataList.Count-1);
            }
            else
            {
                colorRataList[yx_1] = BaseRate + inputXValue/ 100f;
                colorRateState[yx_1] = true;
                colorRataList[y_1x] = BaseRate + inputYValue/ 100f;
                colorRateState[y_1x] = true;
                lastRate = (1 - colorRataList[yx_1] -  colorRataList[y_1x]) / (colorRataList.Count-2);
            }

            
        }
        else if (x > 0)
        {
            ResetColorRate();
            var yx_1  = gridItemColorValue[y, x - 1];
            colorRataList[yx_1] = BaseRate + inputXValue / 100f;
            colorRateState[yx_1] = true;
            lastRate = (1 - colorRataList[yx_1]) / (colorRataList.Count-1);
        }
        else if (y > 0)
        {
            ResetColorRate();
            var y_1x  = gridItemColorValue[y-1, x];
            colorRataList[y_1x] = BaseRate + inputYValue/ 100f;
            colorRateState[y_1x] = true;
            lastRate = (1 - colorRataList[y_1x]) / (colorRataList.Count-1);
        }
        for (int i = 0; i < colorRataList.Count; i++)
        {
            if (!colorRateState[i])
            {
                colorRataList[i] = lastRate;
            }
                
        }
        return getRandomColor();
    }

    int getRandomColor()
    {
        var randomResult = Random.Range(0,1f);
        for (int i = 0; i < colorRataList.Count; i++)
        {
            if (randomResult <= colorRataList[i])
            {
                return i;
            }
            randomResult -= colorRataList[i];
        }
        return 0;
    }
    void RefreshColor()
    {
        for (int y = 0; y < Count; y++)
        {
            for (int x = 0; x < Count; x++)
            {
                gridItemColorValue[y,x] = GetColor(x, y);
                gridItem[y, x].GetComponent<Image>().color = COLORS[gridItemColorValue[y,x]];
            }
        }
    }
    
}
