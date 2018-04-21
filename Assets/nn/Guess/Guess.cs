using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Guess : MonoBehaviour
{
    NNClass nn;
    //GameObject[,] graphGos;
    public int numDataSets = 10;
    public int numNodes = 5;
    public int numInputs = 2;
    public bool ynDefault;
    DataSetsClass dataSets;

	// Use this for initialization
	void Start () {
        startNN();
	}

    void startNN() {
        if (ynDefault == true) {
            initDataSetsDefault();
        } else {
            initDataSetsRandom();
        }
        //
        Debug.Log(numNodes);
        nn = new NNClass(numNodes, numInputs);
        nn.LoadInputDataSet(dataSets.dataSets[0]);
        if (ynDefault == true)
        {
            nn.DefaultWeights();
        } else {
            nn.RandomizeWeights();
        }
        nn.Train(1);
        nn.Graph();
        //Debug.Log(nn.nodeValues.Length + " " + nn.LoadInputs());
    }

    void initDataSetsDefault() {
        dataSets = new DataSetsClass(1, numInputs);
        DataSetType dataSet = new DataSetType(numInputs);
        dataSet.inputData[0] = 3;
        dataSet.inputData[1] = 2;
        dataSet.answer = 1;
        dataSets.LoadInputDataSet(dataSet, 0);
        dataSets.Graph();
//        dataSets.dataSets[0] = dataSet;
    }

    void initDataSetsRandom()
    {
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
        Debug.Log("dataSets:" + dataSets.dataSets[9].inputData[0]);
        dataSets.Graph();
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
