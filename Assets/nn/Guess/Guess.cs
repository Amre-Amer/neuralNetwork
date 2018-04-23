using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Guess : MonoBehaviour
{
    NNClass nn;
    public int numDataSets;
    public int numNodes = 5;
    public int numInputs;
    DataSetsClass dataSets;
    public ConfigType config;
    [Range(.01f, 1f)]
    public float scaleData = 1;
    float scaleDataLast;
    public enum ConfigType {
        Default = 0,
        Random = 1,
        Slopes = 2
    } 

	void Start () {
        startNN();
	}

	void Update()
	{
        if (scaleData != scaleDataLast) {
            dataSets.Graph(scaleData);
            //Debug.Log("graph\n");
        }
        scaleDataLast = scaleData;
	}

	void startNN() 
    {
        InitDataSets();
        InitNN();
        InitWeights();
        Train();
        dataSets.Graph(scaleData);
    }

    void InitNN() {
        nn = new NNClass(numNodes, numInputs);
    }

    void Train() {
        int t = 2;
        for (int d = 0; d < numDataSets; d++)
        {
            nn.LoadInputDataSet(dataSets.dataSets[d]);
            nn.Guess();
//            nn.Train(t);
            Debug.Log(d + " train guess:" + nn.nodes[numNodes].value + "\n");
            dataSets.dataSets[d].guess = nn.nodes[numNodes].value;
        }
        nn.Graph();
    }

    void InitWeights() {
        if (config == ConfigType.Default)
        {
            nn.DefaultWeights();
        }
        else
        {
            nn.RandomizeWeights();
        }
    }

    void InitDataSets() {
        if (config == ConfigType.Default)
        {
            InitDataSetsDefault();
        }
        if (config == ConfigType.Random)
        {
            InitDataSetsRandom();
        }
        if (config == ConfigType.Slopes)
        {
            InitDataSetsSlope();
        }
    }

    void InitDataSetsDefault() 
    {
        numDataSets = 1;
        numInputs = 2;
        dataSets = new DataSetsClass(numDataSets, numInputs);
        DataSetType dataSet = new DataSetType(numInputs);
        dataSet.inputData[0] = 3;
        dataSet.inputData[1] = 2;
        dataSet.answer = 1;
        dataSets.LoadInputDataSet(dataSet, 0);
//        dataSets.dataSets[0] = dataSet;
        dataSets.Graph(scaleData);
        Debug.Log("initDataSetsDefault\n");
    }

    void InitDataSetsSlope()
    {
        numDataSets = 10;
        numInputs = 10;
        dataSets = new DataSetsClass(numDataSets, numInputs);
        for (int d = 0; d < numDataSets; d++)
        {
            DataSetType dataSet = new DataSetType(numInputs);
            float slope = Random.Range(.1f, 9f);
            for (int i = 0; i < dataSet.inputData.Length; i++)
            {
                dataSet.inputData[i] = i * slope;
            }
            dataSet.isAnswered = false;
            if (d < numDataSets * .75f) {
                dataSet.isAnswered = true;
                dataSet.answer = slope;
            }
            dataSets.dataSets[d] = dataSet;
        }
        dataSets.Graph(scaleData);
        Debug.Log("initDataSetsRandom\n");
    }

    void InitDataSetsRandom()
    {
        numDataSets = 10;
        numInputs = 10;
        dataSets = new DataSetsClass(numDataSets, numInputs);
        for (int d = 0; d < numDataSets; d++)
        {
            DataSetType dataSet = new DataSetType(numInputs);
            for (int i = 0; i < dataSet.inputData.Length; i++)
            {
                dataSet.inputData[i] = Random.Range(1, 10);
            }
            dataSet.answer = Random.Range(1, 3);
            dataSets.dataSets[d] = dataSet;
        }
        dataSets.Graph(scaleData);
        Debug.Log("initDataSetsRandom\n");
    }

    void MakeMaterialTransparent(Material material)
    {
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
    }
}
