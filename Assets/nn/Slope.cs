using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slope : MonoBehaviour {
    int numTimes = 500;
    int numPoints = 1000;
    int numTrain = 750;
//    GameObject lineGo;
    GameObject[] pointGos;
    Vector3[] points;
    float[] answers;
    float[] predictions;
    int maxX = 100;
    int maxZ = 100;
    int numInputs = 2;
    float[] weights;
    float[] deltaWeights;
    float[] inputs;
    float prediction;
    float learningRate = .01f;
    float[] sumWeights;
    float[] aveWeights;
    float[,] learnedWeights;
    GameObject[,] graphWeightGos;
    float[,] graphWeights;

    // Use this for initialization
	void Start () {
        initNN();
        initPoints();
        //return;
        //for (int p = 0; p < numPoints; p++)
        //{
        //    scorePointPrediction(p);
        //}
        //
        learnFromTrainingPoints();
        //
        //averageWeights();
        //
        updateGraphWeights();
        Invoke("predictLiveData", 3);
        //Invoke("updateGraphWeights", 3.1f);
//        predictLiveData();
    }

    void updateGraphWeights() {
        if (graphWeightGos == null)
        {
            graphWeightGos = new GameObject[maxX, maxZ];
        }
        for (int x = 0; x < maxX; x++) {
            for (int z = 0; z < maxZ; z++) {
                GameObject go = graphWeightGos[x, z]; 
                if (go == null) {
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.GetComponent<Renderer>().material.color = Random.ColorHSV();
                    graphWeightGos[x, z] = go;
                }
                float sy = 0;
                bool yn = answerPointXZ(x, z);
                if (yn == true)
                {
                    sy = 2;
                }
                else
                {
                    sy = 0;
                }
                go.transform.position = new Vector3(x, sy/2, -1.5f * maxZ + z);
                //go.GetComponent<Renderer>().material.color = Random.ColorHSV();
                go.transform.localScale = new Vector3(.75f, sy, .75f);
            }
        }
    }
	
    void learnFromTrainingPoints()
    {
        for (int p = 0; p < numTrain; p++)
        {
//            randomizeWeights();
            for (int t = 0; t < numTimes; t++)
            {
                //correctWeights(p);
                predictPoint(p);
                colorPrediction(p);
                correctWeights(p);
                for (int i = 0; i < numInputs; i++)
                {
                    learnedWeights[p, i] = weights[i];
                }
            }
        }
        Debug.Log("learned weights(" + weights[0].ToString("F2") + "," + weights[1].ToString("F2") + ")\n");
    }

    void predictPoint(int p)
    {
        loadPoint(p);
        prediction = 0;
        for (int i = 0; i < numInputs; i++)
        {
            prediction += inputs[i] * weights[i];
        }
        predictions[p] = prediction;
    }

    void colorPrediction(int p)
    {
        Color col = Color.black;
        if (predictions[p] >= 0)
        {
            col = Color.white;
        }
        else
        {
            col = Color.black;
        }
        //if (predictions[p] != answerPoint(p))
        //{
        //    col = Color.red;
        //}
        pointGos[p].GetComponent<Renderer>().material.color = col;
    }

    void colorAnswer(int p)
    {
        Color col = Color.black;
        if (answers[p] >= 0)
        {
            col = Color.white;
        }
        else
        {
            col = Color.black;
        }
        //if (predictions[p] != answerPoint(p))
        //{
        //    col = Color.red;
        //}
        pointGos[p].GetComponent<Renderer>().material.color = col;
    }

    void correctWeights(int p)
    {
        float sum = getSumWeights(p);
        if (sum < .001f) sum = .001f;
        float correction = answers[p] - predictions[p];
        for (int i = 0; i < numInputs; i++)
        {
            float fract = Mathf.Abs(weights[i]) / sum;
            float deltaWeight = fract * correction * learningRate;
            deltaWeights[i] = deltaWeight;
            weights[i] += deltaWeight;
        }
    }

    void predictLiveData() {
        float sumError = 0;
        for (int p = numTrain; p < numPoints; p++)
        {
            predictPoint(p);
            colorPrediction(p);
            sumError += Mathf.Abs(answers[p] - predictions[p]);
        }
        float aveError = sumError / (int)numPoints;
        Debug.Log("numPoints:" + numPoints + " numTimes:" + numTimes + " sumError:" + sumError + " aveError:" + aveError + "\n");
        Debug.Log("weights(" + weights[0].ToString("F2") + "," + weights[1].ToString("F2") + ")\n");
    }

    void averageWeights() {
        if (numTrain <= 0) return;
        sumLearnedWeights();
        setAveWeights();
        setWeightsToAve();
        Debug.Log("averaging weights(" + weights[0].ToString("F2") + "," + weights[1].ToString("F2") + ")\n");
    }

    void initSumWeights() {
        for (int w = 0; w < numInputs; w++)
        {
            sumWeights[w] = 0;
        }
    }

    void sumLearnedWeights() {
        initSumWeights();
        for (int p = 0; p < numTrain; p++)
        {
            for (int w = 0; w < numInputs; w++)
            {
                sumWeights[w] += learnedWeights[p, w];
            }
        }
    }

    void setAveWeights() {
        for (int w = 0; w < numInputs; w++)
        {
            aveWeights[w] = sumWeights[w] / (float)numTrain;
        }
    }

    void setWeightsToAve() {
        for (int w = 0; w < numInputs; w++)
        {
            weights[w] = aveWeights[w];
        }
    }

    float getSumWeights(int p) {
        float sum = 0;
        for (int i = 0; i < numInputs; i++)
        {
            sum += Mathf.Abs(weights[i]);
        }
        return sum;
    }

    void loadPoint(int p) {
        inputs[0] = points[p].x;
        inputs[1] = points[p].z;
    }

    void randomizeWeights() {
        for (int i = 0; i < numInputs; i++)
        {
            randomizeWeight(i);
        }
//        Debug.Log("init weights(" + weights[0].ToString("F2") + "," + weights[1].ToString("F2") + ")\n");
    }

    void randomizeWeight(int i) {
        weights[i] = Random.Range(-1f, 1f);
    }

    void initNN()
    {
        inputs = new float[numInputs];
        weights = new float[numInputs];
        learnedWeights = new float[numPoints, numInputs];
        deltaWeights = new float[numInputs];
        sumWeights = new float[numInputs];
        aveWeights = new float[numInputs];
        randomizeWeights();
    }

    void initPoints() {
        points = new Vector3[numPoints];
        pointGos = new GameObject[numPoints];
        answers = new float[numPoints];
        predictions = new float[numPoints];
        for (int p = 0; p < numPoints; p++)
        {
            points[p] = new Vector3(Random.Range(0f, maxX), 0, Random.Range(0f, maxZ));
            updatePoint(p);
            //predictPoint(p);
            answerPoint(p);
//            colorPrediction(p);
            colorAnswer(p);
            //pointGos[p].GetComponent<Renderer>().material.color = Color.cyan;
            //showPoint(p);
        }
    }

    void answerPoint(int p)
    {
        Vector3 pos = points[p];
        bool yn = answerPointXZ(pos.x, pos.z);
        if (yn == true)
        {
            answers[p] = 1;
        }
        else
        {
            answers[p] = -1;
        }
    }

    bool answerPointXZ(float x, float z) {
        bool yn = false;
        if (x > 25 && x < 75)
        {
            if (z >= x * .5f)
            {
                yn = true;
            }
            else
            {
                yn = false;
            }
        } else {
            if (z <= x * 1f)
            {
                yn = true;
            }
            else
            {
                yn = false;
            }
        }
        if (Vector3.Distance(new Vector3(x, 0, z), new Vector3(50, 0, 50)) < 15) {
            yn = false;
        }
        return yn;
    }

    void updatePoint(int p) {
        GameObject go = pointGos[p];
        if (go == null) {
            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "point " + p;
            pointGos[p] = go;
        }
        if (p < numTrain) {
            pointGos[p].transform.position = points[p];
        } else {
            pointGos[p].transform.position = points[p] + new Vector3(maxX * 1.5f, 0, 0);
        }
    }

    void showPointX(int p)
    {
        float error = Mathf.Abs(answers[p] - predictions[p]);
        Debug.Log(p + " " + points[p].x.ToString("F2") + "," + points[p].z.ToString("F2") + " prediction: " + predictions[p] + " answer:" + answers[p] + " weights(" + weights[0].ToString("F2") + "," + weights[1].ToString("F2") + ") deltas(" + deltaWeights[0].ToString("F2") + "," + deltaWeights[1].ToString("F2") + ") error: " + error.ToString("F2") + "\n");
    }
}
