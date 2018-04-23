using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct DataSetType
{
    public int numInputs;
    public float[] inputData;
    public float answer;
    public float guess;
    public bool isAnswered;
    public DataSetType(int numInputs0)
    {
        numInputs = numInputs0;
        inputData = new float[numInputs0];
        answer = 0;
        guess = 0;
        isAnswered = false;
    }
}

public class DataSetsClass
{
    public float dXData = 3;
    public float dZData = 2;
    public int numDataSets;
    public int numInputs;
    public DataSetType[] dataSets;
    public GameObject[,] dataGos;
    public GameObject[] guessGos;
    public GameObject[] answerGos;
    public Text[] guessTexts;
    public Text[] answerTexts;
    public Text[,] dataTexts;
    public DataSetsClass(int numDataSets0, int numInputs0)
    {
        numDataSets = numDataSets0;
        numInputs = numInputs0;
        dataSets = new DataSetType[numDataSets];
    }

    public bool LoadInputDataSet(DataSetType dataSet0, int d)
    {
        dataSets[d] = dataSet0;
        return true;
    }

    public void Graph(float scaleData)
    {
        GraphDataSets(scaleData);
    }

    //float CheckDataValue(float x) {
    //    if (float.IsNaN(x))
    //    {
    //        Debug.Log("isNaN:" + x + "\n");
    //        x = 0;
    //    }
    //    if (float.IsInfinity(x))
    //    {
    //        Debug.Log("isInfinity:" + x + "\n");
    //        x = 100;
    //    }
    //    return x;
    //}

    void GraphDataSets(float scaleData)
    {
        if (dataGos == null)
        {
            dataGos = new GameObject[numDataSets, numInputs];
            dataTexts = new Text[numDataSets, numInputs];
            guessGos = new GameObject[numDataSets];
            guessTexts = new Text[numDataSets];
            answerGos = new GameObject[numDataSets];
            answerTexts = new Text[numDataSets];
        }
        for (int d = 0; d < numDataSets; d++)
        {
            DataSetType dataSet = dataSets[d];
            if (dataSet.inputData != null)
            {
                GameObject go;
                for (int i = 0; i < numInputs; i++)
                {
                    if (dataGos[d, i] == null)
                    {
                        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.name = "dataSets dataGo " + d + " " + i + " data";
                        dataGos[d, i] = go;
                        Text text = CreateTextData(go.transform.position, "data");
                        dataTexts[d, i] = text;
                    }
//                    float sy = CheckDataValue(dataSet.inputData[i] * scaleData);
                    float sy = dataSet.inputData[i] * scaleData;
                    float x = (d - numDataSets) * dXData;
                    float z = -1 * i * dZData;
                    Vector3 pos = new Vector3(x, sy / 2, z);
                    dataGos[d, i].transform.position = pos;
                    dataGos[d, i].transform.localScale = new Vector3(1, sy, 1);
                    Vector3 posText = pos + new Vector3(0, sy / 2 + .1f, 0);
                    dataTexts[d, i].transform.position = posText;
                    dataTexts[d, i].text = dataSet.inputData[i].ToString("F2");
                    if (dataSet.isAnswered == true) {
                        dataGos[d, i].GetComponent<Renderer>().material.color = new Color(.5f, .5f, .5f);
                    } else {
                        dataGos[d, i].GetComponent<Renderer>().material.color = new Color(.25f, .25f, .25f);
                    }
                }
                if (1 == 1)
                {
                    if (guessGos[d] == null)
                    {
                        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.name = "dataSets dataGo " + d + " guess";
                        go.GetComponent<Renderer>().material.color = new Color(.5f, .5f, .25f);
                        guessGos[d] = go;
                        Text text = CreateTextData(go.transform.position, "guess");
                        guessTexts[d] = text;
                    }
                    //float sy = CheckDataValue(dataSet.guess * scaleData);
                    float sy = dataSet.guess * scaleData;
                    float x = (d - numDataSets) * dXData;
                    float z = -1 * numInputs * dZData;
                    Vector3 pos = new Vector3(x, sy/2, z);
                    guessGos[d].transform.position = pos;
                    guessGos[d].transform.localScale = new Vector3(1, sy, 1);
                    guessGos[d].transform.eulerAngles = new Vector3(0, 45, 0);
                    Vector3 posText = pos + new Vector3(0, guessGos[d].transform.localScale.y / 2 + .1f, 0);
                    guessTexts[d].transform.position = posText;
                    guessTexts[d].text = dataSet.guess.ToString("F2");
                }
                if (1 == 1)
                {
                    if (answerGos[d] == null)
                    {
                        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.name = "dataSets dataGo " + d + " answer";
                        answerGos[d] = go;
                        Text text = CreateTextData(go.transform.position, "answer");
                        answerTexts[d] = text;
                    }
//                    float sy = CheckDataValue(dataSet.answer * scaleData);
                    float sy = dataSet.answer * scaleData;
                    float x = (d - numDataSets) * dXData;
                    float z = -1 * (numInputs + 1) * dZData;
                    Vector3 pos = new Vector3(x, sy/2, z);
                    answerGos[d].transform.position = pos;
                    answerGos[d].transform.localScale = new Vector3(1, sy, 1);
                    answerGos[d].transform.eulerAngles = new Vector3(0, 45, 0);
                    Vector3 posText = pos + new Vector3(0, answerGos[d].transform.localScale.y / 2 + .1f, 0);
                    answerTexts[d].transform.position = posText;
                    answerTexts[d].text = dataSet.answer.ToString("F2");
                    if (dataSet.isAnswered == true) {
                        answerGos[d].GetComponent<Renderer>().material.color = new Color(.25f, .5f, .25f);
                    } else {
                        answerGos[d].GetComponent<Renderer>().material.color = new Color(.25f, .25f, .25f);
                    }
                }
            }
        }
    }

    Text CreateTextData(Vector3 pos, string txt)
    {
        GameObject go = new GameObject("text " + txt);
        go.transform.SetParent(GameObject.Find("Canvas").transform);
        go.transform.Rotate(89, 0, 0);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(.02f, .02f, .02f);
        Text text = go.AddComponent<Text>();
        RectTransform rect = go.GetComponent<RectTransform>();
        Font font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        text.font = font;
        text.name = go.name + ".";
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = txt;
        return text;
    }
}
