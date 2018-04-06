using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perceptron : MonoBehaviour {
    float learningRate = .1f;
    int numInputs = 2;
    int numData = 400;
    int numTrain = 400;
    int numWeights;
    DataType[] data;
    int resX = 100;
    int resZ = 100;
    GameObject lin;
    float[] weights;
    int cntFrames;
    float delay = 1f;
    float delayTime;
    float sumDiff = 1;
    struct DataType {
        public float[] inputs;
        public float target;
        public float prediction;
        public float diff;
        public GameObject go;
        public DataType(float[]inputs0, float target0, float prediction0, float diff0, GameObject go0) {
            inputs = inputs0;
            target = target0;
            prediction = prediction0;
            diff = diff0;
            go = go0;
        }
    }

	// Use this for initialization
	void Start () {
        numWeights = numInputs;
        initData();
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.realtimeSinceStartup - delayTime > delay) {
            delayTime = Time.realtimeSinceStartup;
            if (sumDiff > 0)
            {
                forward();
                graph();
            }
            cntFrames++;
        }
	}

    void forward() {
        //Debug.Log("forward.........................................\n");
        sumDiff = 0;
        for (int d = 0; d < numData; d++) {
            data[d].prediction = predict(d);
            if (d < numTrain) {
                data[d].diff = data[d].target - data[d].prediction;
                sumDiff += Mathf.Abs(data[d].diff);
                float deltaWeight = learningRate * data[d].diff;
                //Debug.Log("d:" + d + " deltaWeight:" + deltaWeight + " diff:" + data[d].diff + "\n");
                for (int w = 0; w < numWeights; w++) {
                    weights[w] += deltaWeight * data[d].inputs[w];
                }
            }
        }
        Debug.Log(cntFrames + " sumDiff:" + sumDiff+ " w(" + weights[0] + "," + weights[1] + ") " + weights[1]/weights[0] + "\n");
    }

    float predict(int d) {
        float sum = 0;
        for (int i = 0; i < numInputs; i++) {
            sum += weights[i] * data[d].inputs[i];
        }
        return activate(sum);        
    }

    float activate(float sum) {
        if (sum >= 0) {
            return 1;
        } else {
            return -1;
        }       
    }

    void initData() {
        weights = new float[numWeights];
        for (int w = 0; w < numWeights; w++) {
            weights[w] = Random.Range(-1, 1);
        }
        createLine();
        data = new DataType[numData];
        for (int d = 0; d < numData; d++) {
            float[] inputs = new float[numInputs];
            inputs[0] = Random.Range(0, resX);
            inputs[1] = Random.Range(0, resZ);
            float diff = 0;
            float prediction = 0;
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.transform.position = new Vector3(inputs[0], 0, inputs[1]);
            DataType dat = new DataType(inputs, 0, prediction, diff, go);
            data[d] = dat;
            data[d].target = rateInputs(d);
        }
    }

    void createLine() {
        lin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lin.transform.position = new Vector3(resX / 2, 0, resZ / 2);
        lin.transform.localScale = new Vector3(1, 1, resX + resZ);
        lin.transform.Rotate(0, 45, 0);
    }

    float rateInputs(int d) {
        float target = -1;
        if (data[d].inputs[1] > data[d].inputs[0])
        {
            target = 1;
        }
        return target;
    }

    void graph() {
        //Debug.Log("graph.............................\n");
        for (int d = 0; d < numData; d++)
        {
            DataType dat = data[d];
            GameObject go = dat.go;
            Color col = Color.white;
            if (dat.prediction == rateInputs(d))
            {
                col = Color.green;
            } else {
                col = Color.red;
            }
//            Debug.Log(d + " (" + data[d].inputs[0] + "," + data[d].inputs[1] + ") w(" + weights[0] + "," + weights[1] + ") p " + data[d].prediction + " t " + data[d].target);
            go.GetComponent<Renderer>().material.color = col;
//            Debug.Log("d:" + d + " x:" + dat.inputs[0] + " z:" + dat.inputs[1] + " target:" + dat.target + " prediction:" + dat.prediction + "\n");
        }
    }

    void showWeights() {
        Debug.Log("------------");
        for (int w = 0; w < numWeights; w++)
        {
            Debug.Log("w:" + w + " weight:" + weights[w] + "\n");
        }
    }

    void showProjectedLine() {
        for (int x = 0; x < resX; x++) {
            
        }
    }
}
