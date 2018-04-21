using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Organic : MonoBehaviour {
    public int numTimes = 100;
    int numDataSets = 11;
    int numData = 4;
    float xBase;
    float zBase = 0; //8;
    float dX = 2; //3;
    float dZ = 2; //4;
    float delay = .25f; //.1f;
    float learningRate = .1f;
    float learningGoalPercent = 99f;
    int numNodes;
    int[] order;
    GameObject baseGo;
    LocationType[] nodeLocation;
    NodeType[] nodes;
    struct NodeType {
        public int[] inputsN;
        public float[] weights;
        public float target;
        public float prediction;
        public GameObject go;
        public Text text;
        public GameObject[] links;
        public Text[] weightTexts;
        public Text textId;
        public NodeType(int n, int[] inputsN0, float[] weights0, float prediction0, Vector3 posGo) {
            inputsN = inputsN0;
            weights = weights0;
            prediction = prediction0;
            text = null;
            links = null;
            weightTexts = null;
            textId = null;
            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "node " + n;
            go.transform.position = posGo;
            go.transform.localScale = new Vector3(1, .5f, 1);
            target = 0;
        }
    }
    struct LocationType {
        public float x;
        public float y;
        LocationType(float x0, float y0) {
            x = x0;
            y = y0;
        }
    }
    int cntFrames;
    float delayStart;
    float confidenceCurrent = 0;
    Text confidenceText;
    Text targetText;
    float predictionCurrent;
    float correctionCurrent;
    int dataSetCurrent = 0;
    //
    DataType[] data;
    struct DataType {
        public string role;
        public float value;
        public GameObject go;
        public Text text;
        public int s; // dataSet index
        public DataType(int s0, string role0, float value0, Vector3 posGo) {
            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "data " + role0;
            go.transform.position = posGo;
            go.transform.localScale = new Vector3(1, .5f, 1);
            go.GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f);
            role = role0;
            value = value0;
            text = null;
            s = s0;
        }
    }
    struct DataSetType {
        public bool ynTrainingData;
        public string status;
        public int matchingS;
        public DataType target;
        public DataType prediction;
        public DataType[] dataPoints;
        public Text text;
        public GameObject quadTex;
        public GameObject quadTexMatch;
        public int s; // dataSet index
        public DataSetType(int numData) {
            ynTrainingData = false;
            status = null;
            matchingS = -1;
            target = new DataType();
            prediction = new DataType();
            dataPoints = null;
            text = null;
            quadTex = null;
            quadTexMatch = null;
            s = -1;
        }
    }
    //DataType[] predictions;
    DataSetType[] dataSets;
    DataType targetData;
    GameObject pointer;
    public enum TrainingDataType
    {
        Increase = 0,
        Decrease = 1,
        Level = 2,
        Test = 3,
        Test2 = 4
    };
    public enum FeedForwardType
    {
        SumDelta = 0,
        WeightedSum = 1,
        Sum = 2
    };
    public FeedForwardType feedForwardTypeCurrent = FeedForwardType.WeightedSum;
    public enum ActivationType
    {
        ReLU = 0,
        Sigmoid = 1,
        OnOff = 2,
        Ave = 3,
        Sum = 4,
        SquareRoot = 5
    };
    public ActivationType activationTypeCurrent = ActivationType.SquareRoot;
    public enum FeedBackwardType
    {
        Weighted = 0,
        Scalar = 1,
        LearningRate = 2
    };
    public FeedBackwardType feedBackwardTypeCurrent = FeedBackwardType.Weighted;
    public bool ynUseWeights = true;
    public bool ynCorrect;
    public bool ynShortCircuit;
    bool ynTraining = true;
    bool ynDone = false;
    //float correctionAve = 0;
    public float weightsRandomRange = 1;

	// Use this for initialization
	void Start () {
        xBase = -dX * numDataSets;
        initBaseGo();
        initConfidenceMsg();
        initTargetMsg();
        //
        positionPointer();
        //
        //startFirst();
        //startPerceptron();
        //startSeven();
        //startSpider();
        startPyramid();
        //
        initTarget();
        initLinks();
        initLinkTexts();
        //
        initData();
        delayStart = Time.realtimeSinceStartup;
        //
        for (int t = 0; t < numTimes; t++)
        {
            float correctionSum = 0;
            float correctionAve = 0;
            int cnt = 0;
            dataSetCurrent = 0;
            for (int s = 0; s < numDataSets; s++)
            {
                if (dataSets[s].ynTrainingData == true)
                {
                    dataSetCurrent = s;
                    positionPointer();
                    predictTrainingData();
                    correctionCurrent = calcCorrectionOne(s);
                    //correctionSum += correctionCurrent;
                    //correctionCurrent = correctionAve;
                    correct();
//                    Debug.Log(s + " correctionCurrent:" + correctionCurrent + "\n");
                    cnt++;
                }
            }
            correctionAve = correctionSum / (float)cnt;
            Debug.Log("correctionSum:" + correctionSum + " correctionAve:" + correctionAve + "\n");
            if (ynCorrect == true)
            {
                //correctionCurrent = correctionAve;
                //correct();
                dataSetCurrent = 0;
                for (int s = 0; s < numDataSets; s++)
                {
                    dataSetCurrent = s;
                    positionPointer();
                    predictData();
                }
                //advanceDataSetCurrent();
            }
            //
            //advanceDataSetCurrent();
            dataSetCurrent = 0;
        }
	}

	void UpdateX()
	{
        if (Time.realtimeSinceStartup - delayStart > delay) {
            delayStart = Time.realtimeSinceStartup;
            if (ynDone == false) 
            {
                if (ynTraining == true)
                {
                    predictTrainingData();
                } 
                else 
                {
                    predictLiveData();
                }
                //
                if (dataSetCurrent == numDataSets - 1)
                {
                    if (ynTraining == true)
                    {
                        correctWeightedAll();
                        correct();
                        showNodes();
                        // sum error
                        // correct feedBackward
                        //confidenceCurrent += 34f;
                        Debug.Log("confidence:" + confidenceCurrent + "\n");
                        if (confidenceCurrent >= learningGoalPercent)
                        {
                            ynTraining = false;
                            Debug.Log("ynTraining:" + ynTraining + "\n");
                        }
                    }
                    else
                    {
                        ynDone = true;
                    }
                }
                advanceDataSetCurrent();
            }
            //Debug.Log("s:" + dataSetCurrent + " train:" + dataSets[dataSetCurrent].ynTrainingData + "\n");
        }
	}

    void randomizeWeights()
    {
        for (int n = 0; n < numNodes; n++)
        {
            if (nodes[n].inputsN != null)
            {
                for (int i = 0; i < nodes[n].inputsN.Length; i++)
                {
                    if (ynUseWeights == true) {
                        nodes[n].weights[i] = Random.Range(-weightsRandomRange, weightsRandomRange); // 1,10
                    } else {
                        nodes[n].weights[i] = 1;
                    }
                    //nodes[n].weights[i] = Random.Range(-1f, 1f); // 1,10
                    //nodes[n].weights[i] = 1;
                }
            }
        }
    }

    void correctWeightedAll()
    {
        float correctionSum = 0;
        int cnt = 0;
        for (int s = 0; s < numDataSets; s++)
        {
            if (dataSets[s].ynTrainingData == true)
            {
                correctionSum += calcCorrectionOne(s);
                cnt++;
            }
        }
        correctionCurrent = correctionSum;
        Debug.Log("correctionSum:" + correctionSum + "\n");
        //confidenceCurrent = 100f - Mathf.Abs(correctionAve) / targetAve;
        //Debug.Log("correctWeightedAll:confidenceCurrent:" + confidenceCurrent + " :ABS(correctionAve:" + correctionAve + ") / targetAve:" + targetAve + "\n");
    }

    float calcCorrectionOne(int s) {
        float target0 = dataSets[s].target.value;
        float prediction0 = dataSets[s].prediction.value;
        float diff = target0 - prediction0;
        return diff;
    }

    void correct()
    {
        if (feedBackwardTypeCurrent == FeedBackwardType.Weighted)
        {
            correctWeighted();
        }
        if (feedBackwardTypeCurrent == FeedBackwardType.Scalar)
        {
            correctScalar();
        }
        if (feedBackwardTypeCurrent == FeedBackwardType.LearningRate)
        {
            correctLearningRate();
        }
    }

    void correctWeighted()
    {
        //predictionCurrent = nodes[numNodes - 1].prediction;

        //        float correctionCurrent = targetData.value - predictionCurrent;
        //float correctionCurrent = correctionAve;
        //Debug.Log(" correctWeighted:target: " + targetData.value + " predictionCurrent:" + predictionCurrent + " correctionCurrent:" + correctionCurrent + "\n");
        correctionCurrent *= learningRate;

        int n = numNodes - 1;
        float c = correctionCurrent;

        correctWeightedNode(n, c);
    }

    void correctWeightedNode(int n, float c)
    {
        if (nodes[n].inputsN == null)
        {
            return;
        }
        float sumWeights = 0;
        for (int w = 0; w < nodes[n].weights.Length; w++)
        {
            sumWeights += nodes[n].weights[w];
        }
        for (int i = 0; i < nodes[n].inputsN.Length; i++)
        {
            float fract = nodes[n].weights[i] / sumWeights;
            float cNew = fract * c;
//            Debug.Log("cNew:" + cNew + "\n");
            nodes[n].weights[i] += cNew;
            int nNew = nodes[n].inputsN[i];
            correctWeightedNode(nNew, cNew);
        }
    }

    void advanceDataSetCurrent()
    {
        dataSetCurrent++;
        if (dataSetCurrent == numDataSets)
        {
            dataSetCurrent = 0;
        }
        positionPointer();
    }

   void predictTrainingData()
    {
        if (dataSets[dataSetCurrent].ynTrainingData == true) {
            loadDataSet2Nodes(dataSetCurrent);
            feedForward();
            showNodes();
            nodesPrediction2DataSet();
        }
    }

    void predictLiveData() {
        if (dataSets[dataSetCurrent].ynTrainingData == false)
        {
            loadDataSet2Nodes(dataSetCurrent);
            feedForward();
            showNodes();
            nodesPrediction2DataSet();
            matchPrediction(dataSetCurrent);
        }
    }

    void predictData() {
        loadDataSet2Nodes(dataSetCurrent);
        feedForward();
        showNodes();
        nodesPrediction2DataSet();
        matchPrediction(dataSetCurrent);
    }

    void UpdateNew()
    {
        if (dataSetCurrent >= numDataSets)
        {
            return;
        }
        if (Time.realtimeSinceStartup - delayStart > delay)
        {
            delayStart = Time.realtimeSinceStartup;
            DataSetType dataSet = dataSets[dataSetCurrent];
//            Debug.Log("Update:" + dataSetCurrent + " ynTrainingData:" + dataSet.ynTrainingData + "\n");
            if (dataSet.ynTrainingData == true) 
            {
                if (confidenceCurrent == 0)
                {
                    setDataSetStatus(dataSetCurrent, "busy");
                    loadDataSet2Nodes(dataSetCurrent);
                    feedForward();
                    showNodes();
                    calcConfidence();
                    Debug.Log("target:" + targetData.value + "\n");
                    Debug.Log(dataSetCurrent + " start..." + confidenceCurrent + "% predictionCurrent:" + predictionCurrent + "\n");
                }
                else
                {
                    if (confidenceCurrent < learningGoalPercent)
                    {
                        setDataSetStatus(dataSetCurrent, "busy");
                        correct();
                        feedForward();
                        showNodes();
                        calcConfidence();
                        Debug.Log(dataSetCurrent + " busy... " + confidenceCurrent + "%\n");
                        if (confidenceCurrent >= learningGoalPercent) {
                            cleanupNodes();
                            setDataSetStatus(dataSetCurrent, "trained");
                            Debug.Log(dataSetCurrent + " done...\n");
                        }
                    }
                }
            } else {
                loadDataSet2Nodes(dataSetCurrent);
                feedForward();
                showNodes();
                calcConfidence();
                cleanupNodes();
                matchPrediction(dataSetCurrent);
                setDataSetStatus(dataSetCurrent, "predicted");
                confidenceCurrent = learningGoalPercent;
                Debug.Log(dataSetCurrent + " trained... " + confidenceCurrent + "%\n");
            }
            if ((dataSet.ynTrainingData == true && confidenceCurrent >= learningGoalPercent) || dataSet.ynTrainingData == false)
            {
                //graphDataSet(dataSetCurrent);
                advanceDataSetCurrent();
                confidenceCurrent = 0;
                Debug.Log(dataSetCurrent + " advance...\n");
            }
        }
        cntFrames++;
    }

    // Update is called once per frame
    void UpdateOld()
    {
        if (dataSetCurrent >= numDataSets)
        {
            return;
        }
        if (Time.realtimeSinceStartup - delayStart > delay)
        {
            delayStart = Time.realtimeSinceStartup;
            DataSetType dataSet = dataSets[dataSetCurrent];
            //            Debug.Log("Update:" + dataSetCurrent + " ynTrainingData:" + dataSet.ynTrainingData + "\n");
            if (dataSet.ynTrainingData == true)
            {
                if (confidenceCurrent == 0)
                {
                    setDataSetStatus(dataSetCurrent, "busy");
                    loadDataSet2Nodes(dataSetCurrent);
                    feedForward();
                    showNodes();
                    calcConfidence();
                    Debug.Log("target:" + targetData.value + "\n");
                    Debug.Log(dataSetCurrent + " start..." + confidenceCurrent + "% predictionCurrent:" + predictionCurrent + "\n");
                }
                else
                {
                    if (confidenceCurrent < learningGoalPercent)
                    {
                        setDataSetStatus(dataSetCurrent, "busy");
                        correct();
                        feedForward();
                        showNodes();
                        calcConfidence();
                        Debug.Log(dataSetCurrent + " busy... " + confidenceCurrent + "%\n");
                        if (confidenceCurrent >= learningGoalPercent)
                        {
                            cleanupNodes();
                            setDataSetStatus(dataSetCurrent, "trained");
                            Debug.Log(dataSetCurrent + " done...\n");
                        }
                    }
                }
            }
            else
            {
                loadDataSet2Nodes(dataSetCurrent);
                feedForward();
                showNodes();
                calcConfidence();
                cleanupNodes();
                matchPrediction(dataSetCurrent);
                setDataSetStatus(dataSetCurrent, "predicted");
                confidenceCurrent = learningGoalPercent;
                Debug.Log(dataSetCurrent + " trained... " + confidenceCurrent + "%\n");
            }
            if ((dataSet.ynTrainingData == true && confidenceCurrent >= learningGoalPercent) || dataSet.ynTrainingData == false)
            {
                //graphDataSet(dataSetCurrent);
                advanceDataSetCurrent();
                confidenceCurrent = 0;
                Debug.Log(dataSetCurrent + " advance...\n");
            }
        }
        cntFrames++;
    }

    void initBaseGo()
    {
        baseGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseGo.transform.localScale = new Vector3(35, .1f, 20);
        baseGo.transform.position = new Vector3(xBase / 2 + 5, -.2f, -numData * dZ / 2);
        baseGo.GetComponent<Renderer>().material.color = new Color(.5f, .5f, .5f, .5f);
        makeMaterialTransparent(baseGo.GetComponent<Renderer>().material);
    }

    void initData()
    {
        dataSets = new DataSetType[numDataSets];
        for (int s = 0; s < numDataSets; s++)
        {
            dataSets[s].s = s;
            dataSets[s].dataPoints = new DataType[numData];
            float x = xBase + s * dX;
            float z = zBase;
            dataSets[s].target = createDataType(s, "target", 1, new Vector3(x, 0, z));
            z = zBase - dZ;
            dataSets[s].prediction = createDataType(s, "prediction", 1, new Vector3(x, 0, z));
            for (int d = 0; d < numData; d++)
            {
                float value = s + d;
                z = zBase + (d + 2) * -dZ;
                dataSets[s].dataPoints[d] = createDataType(s, "data", value, new Vector3(x, 0, z));
            }
            dataSets[s].text = createDataSetText(dataSets[s]);
            dataSets[s].text.text = s.ToString();
        }
        //return;
        loadTestData(0, TrainingDataType.Increase, 1f, true);
        loadTestData(1, TrainingDataType.Decrease, -1f, true);
        loadTestData(2, TrainingDataType.Level, 0.1f, true);
        loadTestData(3, TrainingDataType.Increase, 1f, true);
        loadTestData(4, TrainingDataType.Decrease, -1, true);
        loadTestData(5, TrainingDataType.Level, .1f, true);
        loadTestData(6, TrainingDataType.Decrease, -1, true);
        loadTestData(7, TrainingDataType.Level, .1f, true);
        loadTestData(8, TrainingDataType.Test, -1, false);
        loadTestData(9, TrainingDataType.Test2, -1, false);
        loadTestData(10, TrainingDataType.Level, -1, false);
        //showData();
        for (int s = 0; s < numDataSets; s++)
        {
            graphDataSet(s);
        }
    }

    void loadTestData(int s, TrainingDataType trainingType, float target0, bool ynTrainingData)
    {
        if (trainingType == TrainingDataType.Increase)
        {
            loadTestDataRamp(s, 1, 9, target0, ynTrainingData);
        }
        if (trainingType == TrainingDataType.Decrease)
        {
            loadTestDataRamp(s, 8, 2, target0, ynTrainingData);
        }
        if (trainingType == TrainingDataType.Level)
        {
            loadTestDataRamp(s, 1, 1, target0, ynTrainingData);
        }
        if (trainingType == TrainingDataType.Test)
        {
            loadTestDataRamp(s, 7, 2, target0, ynTrainingData);
        }
        if (trainingType == TrainingDataType.Test2)
        {
            loadTestDataRamp(s, 2, 7, target0, ynTrainingData);
        }
    }

    void matchPrediction(int sMatch)
    {
//        Debug.Log(sMatch + " matchPrediction:" + dataSets[sMatch].ynTrainingData + " " + "\n");
        float minDiff = 0;
        int minS = -1;
        float prediction = dataSets[sMatch].prediction.value;
        for (int s = 0; s < numDataSets; s++)
        {
            if (dataSets[s].ynTrainingData == true)
            {
                float diff = Mathf.Abs(prediction - dataSets[s].target.value);
                if (diff < minDiff || minS == -1)
                {
                    minDiff = diff;
                    minS = s;
                }
            }
        }
        //        Debug.Log("match:" + minS);
        dataSets[dataSetCurrent].matchingS = minS;
//        dataSets[dataSetCurrent].target.value = dataSets[minS].target.value;
//        sizeData(dataSets[dataSetCurrent].target);
        //
        graphDataSetMatch(dataSetCurrent);
    }

    void graphDataSetMatch(int s)
    {
        if (dataSets[s].quadTexMatch == null)
        {
            Vector3 pos = new Vector3(xBase + s * dX, .5f, zBase + 2f * dZ);
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "graph match " + s;
            go.transform.position = pos;
            dataSets[s].quadTexMatch = go;
        }
        int sMatch = dataSets[s].matchingS;
        dataSets[s].quadTexMatch.GetComponent<Renderer>().material.mainTexture = createTexForData(sMatch);
    }

    void graphDataSet(int s)
    {
        if (dataSets[s].quadTex == null)
        {
            Vector3 pos = new Vector3(xBase + s * dX, .5f, zBase + 1 * dZ);
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "graph " + s;
            go.transform.position = pos;
            dataSets[s].quadTex = go;
        }
        dataSets[s].quadTex.GetComponent<Renderer>().material.mainTexture = createTexForData(s);
    }

    Texture2D createTexForData(int s) {
        int w = numData;
        int h = numData;
        int spacing = 10;
        Texture2D tex = new Texture2D(w * spacing, h * spacing, TextureFormat.RGBA32, false);
        for (int d = 0; d < numData; d++) {
            float value = dataSets[s].dataPoints[d].value / 15 * h;
            for (int x = 0; x < spacing; x++) {
                for (int y = 0; y < spacing; y++) {
                    tex.SetPixel(d * spacing + x, (int)(value * spacing + y), Color.black);
                }
            }
        }
        tex.Apply();
        return tex;
    }

    void feedForward()
    {
        if (feedForwardTypeCurrent == FeedForwardType.SumDelta) {
            feedForwardSumDelta();
        }
        if (feedForwardTypeCurrent == FeedForwardType.WeightedSum)
        {
            feedForwardWeightedSum();
        }
        if (feedForwardTypeCurrent == FeedForwardType.Sum)
        {
            feedForwardSum();
        }
    }

    void feedForwardSumDelta()
    {
        for (int n = 0; n < numNodes; n++)
        {
            NodeType node = nodes[n];
            float sum = 0;
            if (nodes[n].inputsN != null)
            {
                for (int i = 0; i < node.inputsN.Length; i++)
                {
                    // delta
                    int iOther = n + 1;
                    if (iOther >= node.inputsN.Length - 1) {
                        iOther = 0;
                    }
                    float delta = nodes[node.inputsN[iOther]].prediction / nodes[node.inputsN[i]].prediction;
//                    float delta = nodes[node.inputsN[iOther]].prediction - nodes[node.inputsN[i]].prediction;
                    //sum += Mathf.Abs(delta) * nodes[n].weights[i];

//                    sum += delta * nodes[n].weights[i];
                    sum += delta;

                    //                    sum += nodes[nodes[n].inputsN[i]].prediction * nodes[n].weights[i];
//                    sum += nodes[node.inputsN[i]].prediction * node.weights[i];
                }
//                Debug.Log("feedForwardSumDelta:" + sum + "\n");
                nodes[n].prediction = sum;
//                sizeNode(n);
//                nodes[n].prediction = activateSquareRoot(sum);
            }
        }
    }

    void feedForwardWeightedSum()
    {
        for (int n = 0; n < numNodes; n++)
        {
            float sum = 0;
            if (nodes[n].inputsN != null)
            {
                for (int i = 0; i < nodes[n].inputsN.Length; i++)
                {
                    sum += nodes[nodes[n].inputsN[i]].prediction * nodes[n].weights[i];
                }
                nodes[n].prediction = activate(sum, n);
//                sizeNode(n);
            }
        }
    }

    void feedForwardSum()
    {
        for (int n = 0; n < numNodes; n++)
        {
            float sum = 0;
            if (nodes[n].inputsN != null)
            {
                for (int i = 0; i < nodes[n].inputsN.Length; i++)
                {
                    sum += nodes[nodes[n].inputsN[i]].prediction * nodes[n].weights[i];
                }
                nodes[n].prediction = activate(sum, n);
                //                sizeNode(n);
            }
        }
    }

    float activate(float sum, int n) {
        float result = 0;
        if (activationTypeCurrent == ActivationType.Ave) {
            result = activateAve(sum, n);            
        }        
        if (activationTypeCurrent == ActivationType.OnOff) {
            result = activateOnOff(sum);
        }        
        if (activationTypeCurrent == ActivationType.ReLU) {
            result = activateReLU(sum);
        }        
        if (activationTypeCurrent == ActivationType.Sigmoid) {
            result = activateSigmoid(sum);
        }        
        if (activationTypeCurrent == ActivationType.SquareRoot) {
            result = activateSquareRoot(sum);
        }        
        if (activationTypeCurrent == ActivationType.Sum) {
            result = sum;            
        }
        return result;
    }

    void correctScalar()
    {
        predictionCurrent = nodes[numNodes - 1].prediction;

        float correctionCurrent = targetData.value / predictionCurrent;
        //correctionCurrent *= learningRate;
        //correctionCurrent = Mathf.Sign(correctionCurrent) * Mathf.Sqrt(Mathf.Abs(correctionCurrent));

        Debug.Log("correctScalar --\n" + cntFrames + " target:" + targetData.value + " prediction:" + predictionCurrent + " correction:" + correctionCurrent + "\n");
        for (int n = 0; n < numNodes; n++)
        {
            if (nodes[n].inputsN != null)
            {
                for (int i = 0; i < nodes[n].inputsN.Length; i++)
                {
                    nodes[n].weights[i] *= correctionCurrent;
                }
            }
        }
    }

    void correctLearningRate()
    {
        predictionCurrent = nodes[numNodes - 1].prediction;

        float diff = targetData.value - predictionCurrent;
        float correctionCurrent = diff * learningRate;
//        float correctionCurrent = target.value / predictionCurrent;
//        correctionCurrent = Mathf.Sqrt(correctionCurrent);

        Debug.Log("correctLearningRate --\n" + cntFrames + " target:" + targetData.value + " prediction:" + predictionCurrent + " correction:" + correctionCurrent + "\n");
        for (int n = 0; n < numNodes; n++)
        {
            if (nodes[n].inputsN != null)
            {
                for (int i = 0; i < nodes[n].inputsN.Length; i++)
                {
                    nodes[n].weights[i] += correctionCurrent;
                }
            }
        }
    }

    void calcConfidence()
    {
        float correctionCurrent = targetData.value / predictionCurrent;
        correctionCurrent = Mathf.Sign(correctionCurrent) * Mathf.Sqrt(Mathf.Abs(correctionCurrent));
        //
        float error = Mathf.Abs(correctionCurrent - 1f); // 1 i is no error multiplier (stay the same, no change)
        confidenceCurrent = (1 - error) * 100;

        confidenceText.text = cntFrames + " :" + confidenceCurrent.ToString("F1") + "%";
        //        Debug.Log(confidenceText.text + "\n");
    }

    void setDataSetStatus(int s, string txt) {
        dataSets[s].status = txt;
        dataSets[s].text.text = txt;
    }

    void positionPointer()
    {
        if (pointer == null)
        {
            pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }
        pointer.transform.position = new Vector3(xBase + dX * dataSetCurrent, .5f, zBase + dZ * 2);
    }

    void nodesPrediction2DataSet() {
        cleanupNodes();        
    }

    void cleanupNodes() {
        float prediction = nodes[numNodes - 1].prediction;
        nodes[numNodes - 1].go.GetComponent<Renderer>().material.color = new Color(.5f, 1f, .5f);
        dataSets[dataSetCurrent].prediction.value = prediction;
        sizeData(dataSets[dataSetCurrent].prediction);
        dataSets[dataSetCurrent].prediction.go.GetComponent<Renderer>().material.color = new Color(.5f, 1f, .5f);
    }

    void showData() {
        for (int s = 0; s < numDataSets; s++)
        {
            Debug.Log(s + " showData:ynTraining:" + dataSets[s].ynTrainingData + "\n");
        }
    }

    Text createDataSetText(DataSetType dataSet0)
    {
        GameObject go = new GameObject("text dataSet");
        go.transform.SetParent(GameObject.Find("Canvas").transform);
        go.transform.Rotate(89, 0, 0);
//        Vector3 pos = dataSet0.dataPoints[numData - 1].go.transform.position + new Vector3(0, 0, -dZ);
        float x = xBase + dataSet0.s * dX;
        float z = zBase + (numData + 2) * -dZ;
        Vector3 pos = new Vector3(x, 0, z);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(.02f, .02f, .02f);
        Text text = go.AddComponent<Text>();
        RectTransform rect = go.GetComponent<RectTransform>();
        Font font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        text.font = font;
        text.name = go.name + ".";
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = dataSet0.status;
        return text;
    }

    void loadTestDataRamp(int s, float start, float finish, float target0, bool ynTrainingData0) {
        float delta = (finish - start) / (float)(numData - 1);
        for (int d = 0; d < numData; d++)
        {
            float value = start + delta * d;
            dataSets[s].dataPoints[d].value = value + Random.Range(-1 * value * .1f, value * .1f);
            sizeData(dataSets[s].dataPoints[d]);
        }
        dataSets[s].ynTrainingData = ynTrainingData0;
//        Debug.Log(s + " loadTestDataRamp: ynTrainingData:" + dataSets[s].ynTrainingData + "\n");
        dataSets[s].target.value = target0;
        //if (ynTrainingData0 == false) {
        //    dataSets[s].target.go.GetComponent<Renderer>().material.color = new Color(.5f, .5f, .5f);
        //}
        sizeData(dataSets[s].target);
        dataSets[s].status = "ready";
    }

    DataType createDataType(int s, string role, float value, Vector3 pos) {
        DataType dat = new DataType(s, role, value, pos);
        dat.text = createDataText(dat);
        sizeData(dat);
        return dat;
    }
	
    void sizeData(DataType dat)
    {
        float s = .1f;
        float sy = dat.value * s;
        GameObject dataGo = dat.go;
        dataGo.transform.localScale = new Vector3(1, sy, 1);
        dataGo.transform.position = new Vector3(dataGo.transform.position.x, sy, dataGo.transform.position.z);
        dat.text.text = dat.value.ToString("F2");
        dat.text.transform.position = dataGo.transform.position + new Vector3(0, Mathf.Abs(dataGo.transform.localScale.y) + .05f, 0);
        Color col = Color.black;
        if (dat.role == "data")
        {
            col = new Color(1f, .5f, .5f);
        }
        if (dat.role == "target")
        {
            col = new Color(.5f, 1f, 1f);
            if (dat.s >= 0)
            {
//                Debug.Log("dat.s:" + dat.s + " ynTrainingData:" + dataSets[dat.s].ynTrainingData + "\n");
                if (dataSets[dat.s].ynTrainingData == false)
                {
                    //Debug.Log("dat.s:" + dat.s + " " + dataSets[dat.s]);
                    col = new Color(1f, .5f, .5f);
                }
            }
        }
        if (dat.role == "prediction")
        {
            col = new Color(1f, 1f, .5f);
        }
        //if (dat.value < 0) {
        //    col *= .25f;            
        //}
        //if (dat.value == 0)
        //{
        //    col *= .5f;
        //}
        dataGo.GetComponent<Renderer>().material.color = col;
    }

    Text createDataText(DataType dat)
    {
        GameObject go = new GameObject("text data " + dat.role);
        go.transform.SetParent(GameObject.Find("Canvas").transform);
        go.transform.Rotate(89, 0, 0);
        go.transform.position = new Vector3(dat.go.transform.position.x, .55f, dat.go.transform.position.y);
        go.transform.localScale = new Vector3(.02f, .02f, .02f);
        Text text = go.AddComponent<Text>();
        RectTransform rect = go.GetComponent<RectTransform>();
        Font font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        text.font = font;
        text.name = go.name + ".";
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = dat.value.ToString("F2");
        return text;
    }

    void loadDataSet2Nodes(int s) {
        for (int d = 0; d < numData; d++) {
            nodes[d].prediction = dataSets[s].dataPoints[d].value;
//            nodes[d].prediction = d + 5;
            sizeNode(d);
        }
        targetData.value = dataSets[s].target.value;
        sizeData(targetData);
        targetText.text = targetData.value.ToString("F2");
    }

    void loadTraining2Nodes()
    {
        for (int n = 0; n < numData; n++)
        {
            nodes[n].prediction = n + 1;
        }
        nodes[0].prediction = 1;
        nodes[1].prediction = 2;
        nodes[2].prediction = 3;
        nodes[3].prediction = 4;
    }

    //float activate(string txt) {

    //}

    float activateSquareRoot(float sum)
    {
        return Mathf.Sqrt(sum);
    }

    float activateAve(float sum, int n)
    {
        return sum / nodes[n].inputsN.Length;
    }

    float activateOnOff(float sum)
    {
        if (sum >= 0)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    float activateReLU(float sum) {
        if (sum >= 0) {
            return sum;
        } else {
            return 0;
        }
    }

    float activateSigmoid(float sum)
    {
 //       Debug.Log("sigmoid:" + sum + " = " + sigmoid(sum) + "\n");
        return sigmoid(sum);
    }

    float sigmoid(float x)
    {
        return 1 / (1 + Mathf.Exp(-x));
    }

    void loadData2Nodes() {
        for (int d = 0; d < numData; d++) {
            nodes[d].prediction = data[d].value;
            nodes[d].text.text = nodes[d].prediction.ToString("F2");
        }       
    }

    void swapData2Nodes() {
        float[] tmp = new float[numData];
        for (int d = 0; d < numData; d++) {
            tmp[d] = nodes[d].prediction;
            //
            nodes[d].prediction = data[d].value;
            nodes[d].text.text = nodes[d].prediction.ToString("F2");
            sizeNode(d);
            //
            data[d].value = tmp[d];
            data[d].text.text = data[d].value.ToString("F2");
            sizeData(data[d]);
        }       
    }

    void showNodes()
    {
        for (int n = 0; n < numNodes; n++)
        {
            sizeNode(n);
            if (nodes[n].inputsN != null)
            {
                for (int i = 0; i < nodes[n].inputsN.Length; i++)
                {
                    nodes[n].weightTexts[i].text = nodes[n].weights[i].ToString("F2");
                    sizeLink(n, i);
                }
                nodes[n].text.text = nodes[n].prediction.ToString("F2");
            }
        }
    }

    void initTarget() {
        Vector3 posTarget = nodes[numNodes - 1].go.transform.position + new Vector3(3f, 0, 0);
        targetData = createDataType(-1, "target", 3.14f, posTarget);
    }

    void startPerceptron() {
        numNodes = 3;
        //initOrder("perceptron");
        initNodeLocation("perceptron");
        initNodesPerceptron();
    }

    void startFirst() {
        numNodes = 5;
        //initOrder("first");
        initNodeLocation("first");
        initNodesFirst();
    }

    void startSeven() {
        numNodes = 7;
        //initOrder("seven");
        initNodeLocation("seven");
        initNodesSeven();
    }

    void startSpider() {
        numNodes = 7;
        //initOrder("spider");
        initNodeLocation("spider");
        initNodesSpider();
    }

    void startPyramid() {
        numNodes = 9;
        //initOrder("spider");
        initNodeLocation("pyramid");
        initNodesPyramid();
    }

    void sizeNode(int n)
    {
        float s = .1f;
        float sy = Mathf.Abs(nodes[n].prediction) * s;
        GameObject nodeGo = nodes[n].go;
        nodeGo.transform.localScale = new Vector3(1, sy, 1);
        nodeGo.transform.position = new Vector3(nodeGo.transform.position.x, sy, nodeGo.transform.position.z);
        nodes[n].text.text = nodes[n].prediction.ToString("F2");
        float y = nodeGo.transform.localScale.y + .05f;
        nodes[n].text.transform.position = nodeGo.transform.position + new Vector3(0, y, 0);
    }

    void sizeLink(int n, int i) {
        float s = .125f;
        float w = nodes[n].weights[i];
        Color color = Color.magenta;
        if (w == 0) {
            color = Color.clear;            
        }
        if (w > 0) {
            color = Color.white;            
        }
        if (w < 0) {
            color = Color.black;            
        }
        nodes[n].links[i].GetComponent<Renderer>().material.color = color;
        float sx = s * w;
        nodes[n].links[i].transform.localScale = new Vector3(sx, .1f, nodes[n].links[i].transform.localScale.z);
    }

    void initTargetMsg()
    {
        GameObject targetGoParent = new GameObject("target");
        targetGoParent.transform.position = new Vector3(0, 0, dZ * 1.5f);
        //
        GameObject go = new GameObject("text target");
        go.transform.SetParent(GameObject.Find("Canvas").transform);
        go.transform.Rotate(89, 0, 0);
        go.transform.position = targetGoParent.transform.position;
        go.transform.localScale = new Vector3(.02f, .02f, .02f);
        targetText = go.AddComponent<Text>();
        RectTransform rect = go.GetComponent<RectTransform>();
        Font font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        targetText.font = font;
        targetText.name = go.name + ".";
        targetText.color = Color.black;
        targetText.alignment = TextAnchor.MiddleCenter;
        targetText.text = "target:" + targetData.value.ToString("F1");
    }

    void initConfidenceMsg() {
        GameObject confidenceGo = new GameObject("confidence");
        confidenceGo.transform.position = new Vector3(0, 0, dZ);
        //
        GameObject go = new GameObject("text confidence");
        go.transform.SetParent(GameObject.Find("Canvas").transform);
        go.transform.Rotate(89, 0, 0);
        go.transform.position = confidenceGo.transform.position;
        go.transform.localScale = new Vector3(.02f, .02f, .02f);
        confidenceText = go.AddComponent<Text>();
        RectTransform rect = go.GetComponent<RectTransform>();
        Font font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        confidenceText.font = font;
        confidenceText.name = go.name + ".";
        confidenceText.color = Color.black;
        confidenceText.alignment = TextAnchor.MiddleCenter;
        confidenceText.text = "confidence";
    }

    void initLinks() {
        for (int n = 0; n < numNodes; n++)
        {
            if (nodes[n].inputsN != null)
            {
                nodes[n].links = new GameObject[nodes[n].inputsN.Length];
                Vector3 posNode = nodes[n].go.transform.position;
                for (int i = 0; i < nodes[n].inputsN.Length; i++)
                {
                    Vector3 posInputN = nodes[nodes[n].inputsN[i]].go.transform.position;
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.position = (posNode + posInputN) / 2;
                    go.transform.LookAt(posInputN);
                    float dist = Vector3.Distance(posNode, posInputN);
                    //go.transform.position += go.transform.forward * -1 * dist / 2;
                    go.transform.localScale = new Vector3(.1f, .1f, dist);
                    go.transform.position += new Vector3(0, -.2f, 0);
                    nodes[n].links[i] = go;
                }
            }
        }
    }

    //void initNodeTexts() {
    //    for (int n = 0; n < numNodes; n++)
    //    {
    //        createNodeText(n, name.ToString());
    //    }
    //}

    void initLinkTexts()
    {
        for (int n = 0; n < numNodes; n++)
        {
            if (nodes[n].inputsN != null)
            {
                nodes[n].weightTexts = new Text[nodes[n].inputsN.Length];
                for (int i = 0; i < nodes[n].links.Length; i++)
                {
                    createLinkText(n, i, "w " + i);
                }
            }
        }
    }

    void initNodeLocation(string txt) {
        float s = 2;
        if (txt == "first")
        {
            nodeLocation = new LocationType[numNodes];
            nodeLocation[0].x = 0 * s;
            nodeLocation[0].y = 0 * -s;
            //
            nodeLocation[1].x = 2 * s;
            nodeLocation[1].y = 0 * -s;
            //
            nodeLocation[2].x = 1 * s;
            nodeLocation[2].y = 1 * -s;
            //
            nodeLocation[3].x = 3 * s;
            nodeLocation[3].y = 1 * -s;
            //
            nodeLocation[4].x = 2 * s;
            nodeLocation[4].y = 2 * -s;
        }
        if (txt == "perceptron")
        {
            nodeLocation = new LocationType[numNodes];
            nodeLocation[0].x = 0 * s;
            nodeLocation[0].y = 0 * -s;
            //
            nodeLocation[1].x = 0 * s;
            nodeLocation[1].y = 2 * -s;
            //
            nodeLocation[2].x = 1 * s;
            nodeLocation[2].y = 1 * -s;
        }
        if (txt == "seven")
        {
            nodeLocation = new LocationType[numNodes];
            nodeLocation[0].x = 0 * s;
            nodeLocation[0].y = 0 * -s;
            //
            nodeLocation[1].x = 0 * s;
            nodeLocation[1].y = 2 * -s;
            //
            nodeLocation[2].x = 0 * s;
            nodeLocation[2].y = 4 * -s;
            //
            nodeLocation[3].x = 0 * s;
            nodeLocation[3].y = 6 * -s;
            //
            nodeLocation[4].x = 1 * s;
            nodeLocation[4].y = 1 * -s;
            //
            nodeLocation[5].x = 1 * s;
            nodeLocation[5].y = 5 * -s;
            //
            nodeLocation[6].x = 2 * s;
            nodeLocation[6].y = 3 * -s;
        }
        if (txt == "spider")
        {
            nodeLocation = new LocationType[numNodes];
            nodeLocation[0].x = 0 * s;
            nodeLocation[0].y = 0 * -s;
            //
            nodeLocation[1].x = 0 * s;
            nodeLocation[1].y = 1 * -s;
            //
            nodeLocation[2].x = 0 * s;
            nodeLocation[2].y = 2 * -s;
            //
            nodeLocation[3].x = 0 * s;
            nodeLocation[3].y = 3 * -s;
            //
            nodeLocation[4].x = 0 * s;
            nodeLocation[4].y = 4 * -s;
            //
            nodeLocation[5].x = 0 * s;
            nodeLocation[5].y = 5 * -s;
            //
            nodeLocation[6].x = 1 * s;
            nodeLocation[6].y = 2 * -s;
        }
        if (txt == "pyramid")
        {
            nodeLocation = new LocationType[numNodes];
            nodeLocation[0].x = 0 * s;
            nodeLocation[0].y = 0 * -s;
            //
            nodeLocation[1].x = 0 * s;
            nodeLocation[1].y = 2 * -s;
            //
            nodeLocation[2].x = 0 * s;
            nodeLocation[2].y = 4 * -s;
            //
            nodeLocation[3].x = 0 * s;
            nodeLocation[3].y = 6 * -s;
            //
            nodeLocation[4].x = 2 * s;
            nodeLocation[4].y = 0 * -s;
            //
            nodeLocation[5].x = 2 * s;
            nodeLocation[5].y = 2 * -s;
            //
            nodeLocation[6].x = 2 * s;
            nodeLocation[6].y = 4 * -s;
            //
            nodeLocation[7].x = 2 * s;
            nodeLocation[7].y = 6 * -s;
            //
            nodeLocation[8].x = 4 * s;
            nodeLocation[8].y = 3 * -s;
        }
     }

    void initOrder(string txt) {
        if (txt == "first" || txt == "perceptron" || txt == "seven" || txt == "spider")
        {
            order = new int[numNodes];
            for (int n = 0; n < numNodes; n++)
            {
                order[n] = n;
            }
        }
    }

    void createNode(int n, int[]inputsN, float[]weights, float prediction) {
        Vector3 posNode = new Vector3(nodeLocation[n].x, 0, nodeLocation[n].y);
        nodes[n] = new NodeType(n, inputsN, weights, prediction, posNode);
        createNodeText(n, nodes[n].prediction.ToString());
        createNodeTextId(n);
        sizeNode(n);
    }

    void initNodesFirst()
    {
        int numInputs = 2;
        int[] inputsN;
        float[] weights;
        nodes = new NodeType[numNodes];
        //
        createNode(0, null, null, 1);
        //
        createNode(1, null, null, 2);
        //
        inputsN = new int[numInputs];
        inputsN[0] = 0;
        inputsN[1] = 1;
        weights = new float[numInputs];
        weights[0] = 5;
        weights[1] = 6;
        createNode(2, inputsN, weights, -1);
        //
        createNode(3, null, null, 4);
        //
        numInputs = 2;
        inputsN = new int[numInputs];
        inputsN[0] = 2;
        inputsN[1] = 3;
        weights = new float[numInputs];
        weights[0] = 7;
        weights[1] = 8;
        createNode(4, inputsN, weights, -1);
        //
        paintNodes();
        randomizeWeights();
    }

    void initNodesPerceptron()
    {
        int numInputs = 2;
        int[] inputsN;
        float[] weights;
        nodes = new NodeType[numNodes];
        //
        createNode(0, null, null, 1);
        //
        createNode(1, null, null, 2);
        //
        inputsN = new int[numInputs];
        inputsN[0] = 0;
        inputsN[1] = 1;
        weights = new float[numInputs];
        weights[0] = 5;
        weights[1] = 6;
        createNode(2, inputsN, weights, -1);
        //
        //
        paintNodes();
        randomizeWeights();
    }

    void initNodesSeven()
    {
        int numInputs = 2;
        int[] inputsN;
        float[] weights;
        nodes = new NodeType[numNodes];
        //
        createNode(0, null, null, 1);
        //
        createNode(1, null, null, 2);
        //
        createNode(2, null, null, 3);
        //
        createNode(3, null, null, 4);
        //
        numInputs = 2;
        inputsN = new int[numInputs];
        inputsN[0] = 0;
        inputsN[1] = 1;
        weights = new float[numInputs];
        weights[0] = 2;
        weights[1] = 3;
        createNode(4, inputsN, weights, -1);
        //
        numInputs = 2;
        inputsN = new int[numInputs];
        inputsN[0] = 2;
        inputsN[1] = 3;
        weights = new float[numInputs];
        weights[0] = 4;
        weights[1] = 5;
        createNode(5, inputsN, weights, -1);
        //
        numInputs = 4;
        inputsN = new int[numInputs];
        inputsN[0] = 4;
        inputsN[1] = 5;
        inputsN[2] = 1;
        inputsN[3] = 2;
        weights = new float[numInputs];
        weights[0] = 6;
        weights[1] = 7;
        weights[2] = 3;
        weights[3] = 2;
        createNode(6, inputsN, weights, -1);
        //
        paintNodes();
        randomizeWeights();
    }

    void initNodesSpider()
    {
        int numInputs;
        int[] inputsN;
        float[] weights;
        nodes = new NodeType[numNodes];
        //
        createNode(0, null, null, 1);
        //
        createNode(1, null, null, 2);
        //
        createNode(2, null, null, 3);
        //
        createNode(3, null, null, 4);
        //
        createNode(4, null, null, 5);
        //
        createNode(5, null, null, 6);
        //
        numInputs = 6;
        inputsN = new int[numInputs];
        inputsN[0] = 0;
        inputsN[1] = 1;
        inputsN[2] = 2;
        inputsN[3] = 3;
        inputsN[4] = 4;
        inputsN[5] = 5;
        weights = new float[numInputs];
        weights[0] = 2;
        weights[1] = 3;
        weights[2] = 4;
        weights[3] = 2;
        weights[4] = 5;
        weights[5] = 3;
        createNode(6, inputsN, weights, -1);
        //
        paintNodes();
        randomizeWeights();
    }

    void initNodesPyramid()
    {
        int numInputs;
        int[] inputsN;
        float[] weights;
        nodes = new NodeType[numNodes];
        //
        createNode(0, null, null, 3);
        //
        createNode(1, null, null, 1);
        //
        createNode(2, null, null, 2);
        //
        createNode(3, null, null, 4);
        //
        numInputs = 4; // 0
        inputsN = new int[numInputs];
        inputsN[0] = 0;
        inputsN[1] = 1;
        inputsN[2] = 2;
        inputsN[3] = 3;
        weights = new float[numInputs];
        weights[0] = 2;
        weights[1] = 3;
        weights[2] = 2;
        weights[3] = 3;
        createNode(4, inputsN, weights, -1);
        //
        numInputs = 4; // 0
        inputsN = new int[numInputs];
        inputsN[0] = 0;
        inputsN[1] = 1;
        inputsN[2] = 2;
        inputsN[3] = 3;
        weights = new float[numInputs];
        weights[0] = 2;
        weights[1] = 3;
        weights[2] = 2;
        weights[3] = 3;
        createNode(5, inputsN, weights, -1);
        //
        numInputs = 4; // 0
        inputsN = new int[numInputs];
        inputsN[0] = 0;
        inputsN[1] = 1;
        inputsN[2] = 2;
        inputsN[3] = 3;
        weights = new float[numInputs];
        weights[0] = 2;
        weights[1] = 3;
        weights[2] = 2;
        weights[3] = 3;
        createNode(6, inputsN, weights, -1);
        //
        numInputs = 4; // 0
        inputsN = new int[numInputs];
        inputsN[0] = 0;
        inputsN[1] = 1;
        inputsN[2] = 2;
        inputsN[3] = 3;
        weights = new float[numInputs];
        weights[0] = 2;
        weights[1] = 3;
        weights[2] = 2;
        weights[3] = 3;
        createNode(7, inputsN, weights, -1);
        //
        numInputs = 4;
        inputsN = new int[numInputs];
        if (ynShortCircuit == true) {
            inputsN[0] = 0;
            inputsN[1] = 1;
            inputsN[2] = 2;
            inputsN[3] = 3;
        } else {
            inputsN[0] = 4;
            inputsN[1] = 5;
            inputsN[2] = 6;
            inputsN[3] = 7;
        }
        weights = new float[numInputs];
        weights[0] = 2;
        weights[1] = 3;
        weights[2] = 2;
        weights[3] = 3;
        createNode(8, inputsN, weights, -1);
        //
        paintNodes();
        randomizeWeights();
    }

    void paintNodes() {
        for (int n = 0; n < numNodes; n++) {
            if (nodes[n].inputsN != null) {
                if (n == numNodes - 1)
                {
                    nodes[n].go.GetComponent<Renderer>().material.color = new Color(1f, 1f, .5f);
                } else {
                    nodes[n].go.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f);
                }
            } else {
                nodes[n].go.GetComponent<Renderer>().material.color = new Color(.5f, .5f, .5f);
            }
        }        
    }

    GameObject createNodeGo(int n) {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.transform.position = new Vector3(nodeLocation[n].x, 0, nodeLocation[n].y);
        go.transform.localScale = new Vector3(1, .5f, 1);
        return go;
    }

    void createNodeText(int n, string txt) {
        Vector3 pos = new Vector3(nodeLocation[n].x, .55f, nodeLocation[n].y);
        Text text = createText(pos, nodes[n].prediction.ToString("F2"));
        nodes[n].text = text;
    }

    void createNodeTextId(int n)
    {
        Vector3 pos = new Vector3(nodeLocation[n].x, 0, nodeLocation[n].y - 1f);
        Text text = createText(pos, n.ToString());
        nodes[n].textId = text;
    }

    void createLinkText(int n, int i, string txt)
    {
        GameObject goLink = nodes[n].links[i];
        Vector3 posNode = nodes[n].go.transform.position;
        Vector3 posLink = goLink.transform.position;
        float dist = dZ * .75f;
        GameObject goTmp = new GameObject();
        goTmp.transform.position = posNode;
        goTmp.transform.LookAt(posLink);
        goTmp.transform.position += goTmp.transform.forward * dist;
//        pos += new Vector3(0, 0, -dZ * .5f);
        Vector3 pos = goTmp.transform.position;
        pos.y += goLink.transform.localScale.y;
        Text text = createText(pos, nodes[n].weights[i].ToString("F2"));
        nodes[n].weightTexts[i] = text;
    }

    Text createText(Vector3 pos, string txt) {
        GameObject go = new GameObject("text");
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

    void makeMaterialTransparent(Material material)
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
