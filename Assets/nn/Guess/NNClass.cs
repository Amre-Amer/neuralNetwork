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
    public DataSetType(int numInputs0)
    {
        numInputs = numInputs0;
        inputData = new float[numInputs0];
        answer = 0;
        guess = 0;
    }
}

public class NodeClass
{
    public float value;
    public float[] weights;
    public bool isOutput;
    public GameObject[] weightGos;
    public Text[] weightTexts;
    public NodeClass(bool isOutput0, int numWeights0)
    {
        isOutput = isOutput0;
        weights = new float[numWeights0];
        weightGos = new GameObject[numWeights0];
        weightTexts = new Text[numWeights0];
    }
}

public class NNClass
{
    float learningRate = .1f;
    public int numNodes;
    public int numInputs;
    public int numWeights;
    NodeClass[] nodes;
    public DataSetType dataSet;
    public DataSetType[] dataSets;
    public GameObject[] inputGos;
    public Text[] inputTexts;
    public GameObject[] nodeGos;
    public Text[] nodeTexts;
    float dXNodes = 10;
    float dXOutput = 3;
    float dZNodes = 3;
    float dZInputs = 2;

    public NNClass(int numNodes0, int numInputs0)
    {
        numNodes = numNodes0;
        numInputs = numInputs0;
        nodes = new NodeClass[numNodes + 1];
        for (int n = 0; n < numNodes; n++)
        {
            nodes[n] = new NodeClass(false, numInputs);
        }
        nodes[numNodes] = new NodeClass(true, numNodes);
    }

    public bool LoadInputDataSet(DataSetType dataSet0)
    {
        dataSet = dataSet0;
        return true;
    }

    public bool Train(int numTimes)
    {
        for (int t = 0; t < numTimes; t++)
        {
            Guess();
            //                ShowNodes();
            correctWeights();
        }
        return true;
    }

    public bool Guess()
    {
        for (int n = 0; n < numNodes; n++)
        {
            float sum0 = 0;
            for (int i = 0; i < numInputs; i++)
            {
                float sumThis = dataSet.inputData[i] * nodes[n].weights[i];
                sum0 += sumThis;
            }
            nodes[n].value = sum0;
        }
        float sum = 0;
        for (int n = 0; n < numNodes; n++)
        {
            float sumThis = nodes[n].value * nodes[numNodes].weights[n];
            sum += sumThis;
        }
        nodes[numNodes].value = sum;
        dataSet.guess = nodes[numNodes].value;
        return true;
    }

    public bool DefaultWeights()
    {
        nodes[0].weights[0] = .5f;
        nodes[0].weights[1] = .2f;
        //
        nodes[1].weights[0] = .1f;
        nodes[1].weights[1] = .2f;
        //
        nodes[2].weights[0] = .1f;
        nodes[2].weights[1] = .3f;
        //
        nodes[3].weights[0] = .1f;
        nodes[3].weights[1] = .3f;
        nodes[3].weights[2] = .2f;
        //
        return true;
    }

    public bool RandomizeWeights()
    {
        for (int n = 0; n < numNodes + 1; n++)
        {
            for (int w = 0; w < nodes[n].weights.Length; w++)
            {
                nodes[n].weights[w] = Random.Range(-1f, 1f);
            }
        }
        return true;
    }

    public float getSumWeights()
    {
        float sum = 0;
        for (int n = 0; n < numNodes; n++)
        {
            for (int w = 0; w < nodes[n].weights.Length; w++)
            {
                sum += nodes[n].weights[w];
            }
        }
        return sum;
    }

    public float getGuess()
    {
        return dataSet.guess;
    }

    public float getAnswer()
    {
        return dataSet.answer;
    }

    public void correctWeights()
    {
        float correction = getAnswer() - getGuess();
        //Debug.Log("correction(answer - guess)(" +  getAnswer() + "," + getGuess() + "):" + correction + "\n");
        correction *= learningRate;
        Debug.Log("correction:" + correction + "\n");
        float sumWeights = getSumWeights();
        for (int n = 0; n < numNodes + 1; n++)
        {
            for (int w = 0; w < nodes[n].weights.Length; w++)
            {
                float fraction = nodes[n].weights[w] / sumWeights;
                float deltaWeight = fraction * correction;
                nodes[n].weights[w] += deltaWeight;
            }
        }
    }

    public void ShowNodes()
    {
        for (int n = 0; n < numNodes + 1; n++)
        {
            Debug.Log("node:" + n + " " + nodes[n].value + "\n");
        }
    }

    public void Graph()
    {
        GraphInputs();
        GraphNodes();
        graphWeights();
    }

    void GraphInputs()
    {
        if (inputGos == null)
        {
            inputGos = new GameObject[numInputs];
            inputTexts = new Text[numInputs];
        }
        for (int i = 0; i < numInputs; i++)
        {
            GameObject go = inputGos[i];
            if (go == null)
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.GetComponent<Renderer>().material.color = new Color(.75f, .25f, .25f);
                //float a = (numNodes - numInputs) / 2f;
                //float offset = dZInputs * a;
                float z = -1 * i * dZInputs;
                //z -= offset;
                Vector3 pos = new Vector3(0, 0, z);
                go.transform.position = pos;
                inputGos[i] = go;
                Vector3 posText = pos + new Vector3(0, go.transform.localScale.y / 2 + .1f, 0);
                Text text = CreateText(posText, "");
                inputTexts[i] = text;
            }
            inputTexts[i].text = dataSet.inputData[i].ToString("F2");
        }
    }

    void GraphNodes()
    {
        if (nodeGos == null)
        {
            nodeGos = new GameObject[numNodes + 1];
            nodeTexts = new Text[numNodes + 1];
        }
        for (int n = 0; n < numNodes + 1; n++)
        {
            GameObject go = nodeGos[n];
            if (go == null)
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                go.GetComponent<Renderer>().material.color = new Color(.75f, .75f, .75f);
                go.transform.localScale = new Vector3(1, .5f, 1);
                float x = dXNodes + dXOutput;
                float zMid = -1 * (numInputs - 1) / 2f * dZInputs;
                float z = zMid;
                if (n < numNodes)
                {
                    x = dXNodes;
                    z = zMid + (numNodes - 1) / 2f * dZNodes;
                    z -= n * dZNodes;
                }
                Vector3 pos = new Vector3(x, 0, z);
                go.transform.position = pos;
                nodeGos[n] = go;
                Vector3 posText = pos + new Vector3(0, go.transform.localScale.y + .1f, 0);
                Text text = CreateText(posText, "");
                nodeTexts[n] = text;
            }
            nodeTexts[n].text = nodes[n].value.ToString("F2");
        }
    }

    void graphWeights()
    {
        for (int n = 0; n < numNodes + 1; n++)
        {
            if (n < numNodes)
            {
                for (int i = 0; i < numInputs; i++)
                {
                    updateNodeWeightGo(n, i);
                    updateWeightText(n, i);
                }
            }
            else
            {
                for (int nn = 0; nn < numNodes; nn++)
                {
                    updateNodeWeightGo(n, nn);
                    updateWeightText(n, nn);
                }
            }
        }
    }

    void updateNodeWeightGo(int n, int w)
    {
        GameObject go = nodes[n].weightGos[w];
        if (go == null)
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "weight n:" + n + " w:" + w;
            go.GetComponent<Renderer>().material.color = new Color(.25f, .25f, .75f);
            Vector3 posA;
            if (n < numNodes)
            {
                posA = inputGos[w].transform.position;
            }
            else
            {
                posA = nodeGos[w].transform.position;
            }
            Vector3 posNode = nodeGos[n].transform.position;
            Vector3 pos = (posA + posNode) / 2;
            float dist = Vector3.Distance(posA, posNode);
            go.transform.localScale = new Vector3(.1f, .1f, dist);
            go.transform.position = pos;
            go.transform.LookAt(posA);
            nodes[n].weightGos[w] = go;
            //
            //pos = posNode;
            //pos += go.transform.forward * 2;
            Vector3 posText = pos + new Vector3(0, go.transform.localScale.y + .1f, 0);
            float offset = -dist / 2 + 1.75f;
            posText += go.transform.forward * offset;
            Text text = CreateText(posText, "");
            nodes[n].weightTexts[w] = text;
        }
        updateWeightText(n, w);
    }

    void updateWeightText(int n, int w)
    {
        nodes[n].weightTexts[w].text = nodes[n].weights[w].ToString("F2");
    }

    Text CreateText(Vector3 pos, string txt)
    {
        GameObject go = new GameObject("link text");
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

public class DataSetsClass
{
    public float dXData = 3;
    public float dZData = 2;
    public int numDataSets;
    public int numInputs;
    public DataSetType[] dataSets;
    public GameObject[,] dataGos;
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

    public void Graph()
    {
        GraphDataSets();
    }

    void GraphDataSets()
    {
        if (dataGos == null)
        {
            //Debug.Log("numInputs:" + numInputs);
            dataGos = new GameObject[numDataSets, numInputs];
            dataTexts = new Text[numDataSets, numInputs];
        }
        for (int d = 0; d < numDataSets; d++)
        {
            DataSetType dataSet = dataSets[d];
            if (dataSet.inputData != null)
            {
                //                    Debug.Log(":" + dataSet.inputData + " " + dataSet.inputData.Length + "\n");
                for (int i = 0; i < numInputs; i++)
                {
                    //                        Debug.Log(":" + numInputs);
                    GameObject go = dataGos[d, i];
                    if (go == null)
                    {
                        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.name = "dataSets dataGo " + d + " " + i;
                        go.GetComponent<Renderer>().material.color = new Color(.25f, .25f, .25f);
                        float x = (d - numDataSets) * dXData;
                        float z = -1 * i * dZData;
                        Vector3 pos = new Vector3(x, 0, z);
                        go.transform.position = pos;
                        dataGos[d, i] = go;
                        Vector3 posText = pos + new Vector3(0, go.transform.localScale.y / 2 + .1f, 0);
                        Text text = CreateText(posText, "");
                        dataTexts[d, i] = text;
                    }
                    dataTexts[d, i].text = dataSet.inputData[i].ToString("F2");
                }
            }
        }
    }

    Text CreateText(Vector3 pos, string txt)
    {
        GameObject go = new GameObject("text data");
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
