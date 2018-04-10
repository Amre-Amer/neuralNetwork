using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Organic : MonoBehaviour {
    int numNodes;
    float delay = .1f;
    int[] order;
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
        public Text[] texts;
        public NodeType(int n, int[] inputsN0, float[] weights0, float prediction0, Vector3 posGo) {
            inputsN = inputsN0;
            weights = weights0;
            prediction = prediction0;
            text = null;
            links = null;
            texts = null;
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
    int dataSetCurrent = 0;
    //
    int numData = 4;
    DataType[] data;
    struct DataType {
        public string role;
        public float value;
        public GameObject go;
        public Text text;
        public DataType(string role0, float value0, Vector3 posGo) {
            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "data " + role0;
            go.transform.position = posGo;
            go.transform.localScale = new Vector3(1, .5f, 1);
            go.GetComponent<Renderer>().material.color = new Color(.5f, .5f, .5f);
            if (role0 == "data") {
                go.GetComponent<Renderer>().material.color = new Color(1f, .5f, .5f);
            }
            if (role0 == "target") {
                go.GetComponent<Renderer>().material.color = new Color(.5f, 1f, 1f);
            }
            if (role0 == "prediction")
            {
                go.GetComponent<Renderer>().material.color = new Color(1f, 1f, .5f);
            }
            role = role0;
            value = value0;
            text = null;
        }
    }
    struct DataSetType {
        public bool ynTrainingData;
        public string status;
        public int matchingS;
        public DataType target;
        public DataType prediction;
        public DataType[] dataPoints;
        public Texture2D texData;
        public Texture2D texPredictionData;
        public Text text;
        public DataSetType(int numData) {
            ynTrainingData = false;
            status = null;
            matchingS = -1;
            target = new DataType();
            prediction = new DataType();
            dataPoints = null;
            texData = null;
            texPredictionData = null;
            text = null;
        }
    }
    DataType[] predictions;
    DataSetType[] dataSets;
    float xBase = -13;
    float zBase = 8;
    float dX = 3;
    float dZ = 4;
    DataType target;
    int numDataSets = 4;
    GameObject pointer;
 
	// Use this for initialization
	void Start () {
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
	}

    // Update is called once per frame
    void Update()
    {
        if (dataSetCurrent >= numDataSets)
        {
            return;
        }
        if (Time.realtimeSinceStartup - delayStart > delay)
        {
            delayStart = Time.realtimeSinceStartup;
            DataSetType dataSet = dataSets[dataSetCurrent];
            if (dataSet.ynTrainingData == true) 
            {
                if (confidenceCurrent == 0)
                {
                    setDataSetStatus(dataSetCurrent, "busy");
                    loadDataSet2Nodes(dataSetCurrent);
                    fastForward();
                    showNodes();
                    calcConfidence();
                    Debug.Log("target:" + target );
                    Debug.Log(dataSetCurrent + " start..." + confidenceCurrent + "% predictionCurrent:" + predictionCurrent + "\n");
                }
                else
                {
                    if (confidenceCurrent < 99.9f)
                    {
                        setDataSetStatus(dataSetCurrent, "busy");
                        correct();
                        fastForward();
                        showNodes();
                        calcConfidence();
                        Debug.Log(dataSetCurrent + " busy... " + confidenceCurrent + "\n");
                        if (confidenceCurrent >= 99.9f) {
                            cleanupNodes();
                            setDataSetStatus(dataSetCurrent, "trained");
                            Debug.Log(dataSetCurrent + " done...\n");
                        }
                    }
                }
            } else {
                loadDataSet2Nodes(dataSetCurrent);
                fastForward();
                showNodes();
                calcConfidence();
                cleanupNodes();
                matchPrediction(dataSetCurrent);
                setDataSetStatus(dataSetCurrent, "predicted");
                Debug.Log(dataSetCurrent + " trained... " + confidenceCurrent + "%\n");
            }
            if ((dataSet.ynTrainingData == true && confidenceCurrent >= 99.9f) || dataSet.ynTrainingData == false)
            {
                advanceDataSetCurrent();
                confidenceCurrent = 0;
                Debug.Log(dataSetCurrent + " advance...\n");
            }
        }
        cntFrames++;
    }

    void correct()
    {
        predictionCurrent = nodes[numNodes - 1].prediction;

        float correctionCurrent = target.value / predictionCurrent;
        correctionCurrent = Mathf.Sign(correctionCurrent) * Mathf.Sqrt(Mathf.Abs(correctionCurrent));

        Debug.Log("--\n" + cntFrames + " target:" + target + " prediction:" + predictionCurrent + " correction:" + correctionCurrent + "\n");
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

    void calcConfidence()
    {
        float correctionCurrent = target.value / predictionCurrent;
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

    void graphDataSet(int s) {
        
    }

    void advanceDataSetCurrent() {
        if (dataSetCurrent < numDataSets) {
            dataSetCurrent++;
        }
        positionPointer();
    }

    void positionPointer()
    {
        if (pointer == null)
        {
            pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }
        pointer.transform.position = new Vector3(xBase + dX * dataSetCurrent, 0, zBase + dZ * 1);
    }

    void cleanupNodes() {
        float prediction = nodes[numNodes - 1].prediction;
        nodes[numNodes - 1].go.GetComponent<Renderer>().material.color = new Color(.5f, 1f, .5f);
        dataSets[dataSetCurrent].prediction.value = prediction;
        sizeData(dataSets[dataSetCurrent].prediction);
        dataSets[dataSetCurrent].prediction.go.GetComponent<Renderer>().material.color = new Color(.5f, 1f, .5f);
    }

    void matchPrediction(int sMatch) {
        float minDiff = 0;
        int minS = -1;
        float prediction = dataSets[sMatch].prediction.value;
        for (int s = 0; s < numDataSets; s++) {
            if (dataSets[s].ynTrainingData == true)
            {
                float diff = Mathf.Abs(prediction - dataSets[s].target.value);
                if (diff < minDiff || minS == -1) {
                    minDiff = diff;
                    minS = s;
                }
            }
        }
//        Debug.Log("match:" + minS);
        dataSets[dataSetCurrent].matchingS = minS;
        dataSets[dataSetCurrent].target.value = dataSets[minS].target.value;
        sizeData(dataSets[dataSetCurrent].target);
    }

    void initData() {
        dataSets = new DataSetType[numDataSets];
        for (int s = 0; s < numDataSets; s++) {
            dataSets[s].dataPoints = new DataType[numData];
            float x = xBase + s * dX;
            float z = zBase;
            dataSets[s].target = createDataType("target", 1, new Vector3(x, 0, z));
            z = zBase - dZ;
            dataSets[s].prediction = createDataType("prediction", 1, new Vector3(x, 0, z));
            for (int d = 0; d < numData; d++) {
                float value = s + d;
                z = zBase + (d + 2) * -dZ;
                dataSets[s].dataPoints[d] = createDataType("data", value, new Vector3(x, 0, z));
            }
            dataSets[s].text = createDataSetText(dataSets[s]);
            dataSets[s].text.text = s.ToString();
        }
        loadTestData(0, "decrease");
        loadTestData(1, "level");
        loadTestData(2, "increase");
        loadTestData(3, "test");
    }

    Text createDataSetText(DataSetType dataSet0)
    {
        GameObject go = new GameObject("text dataSet");
        go.transform.SetParent(GameObject.Find("Canvas").transform);
        go.transform.Rotate(89, 0, 0);
        Vector3 pos = dataSet0.dataPoints[numData - 1].go.transform.position + new Vector3(0, 0, -dZ);
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

    void loadTestData(int s, string testName) {
        if (testName == "increase") {
            loadTestDataRamp(s, 1, 10, 3, true);
        }
        if (testName == "decrease")
        {
            loadTestDataRamp(s, 10, 1, 1, true);
        }
        if (testName == "level")
        {
            loadTestDataRamp(s, 5, 5, 2, true);
        }
        if (testName == "test")
        {
            loadTestDataRamp(s, 7, 5, -1, false);
        }
    }

    void loadTestDataRamp(int s, float start, float finish, float target0, bool ynTrainingData) {
        float delta = (finish - start) / (float)(numData - 1);
        for (int d = 0; d < numData; d++)
        {
            float value = start + delta * d;
            dataSets[s].dataPoints[d].value = value;
            sizeData(dataSets[s].dataPoints[d]);
        }
        dataSets[s].ynTrainingData = ynTrainingData;
        dataSets[s].target.value = target0;
        if (ynTrainingData == false) {
            dataSets[s].target.go.GetComponent<Renderer>().material.color = new Color(.5f, .5f, .5f);
        }
        sizeData(dataSets[s].target);
        dataSets[s].status = "ready";
    }

    DataType createDataType(string role, float value, Vector3 pos) {
        DataType dat = new DataType(role, value, pos);
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
        dataGo.transform.position = new Vector3(dataGo.transform.position.x, sy / 2, dataGo.transform.position.z);
        dat.text.text = dat.value.ToString("F2");
        dat.text.transform.position = dataGo.transform.position + new Vector3(0, dataGo.transform.localScale.y + .05f, 0);
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
        target.value = dataSets[s].target.value;
        sizeData(target);
        targetText.text = target.value.ToString("F2");
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

    void fastForward()
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
                //              nodes[n].prediction = activateSigmoid(sum);
                //                nodes[n].prediction = activateOnOff(sum);
                //              nodes[n].prediction = activateAve(sum, n);
                //              nodes[n].prediction = sum;
                nodes[n].prediction = activateSquareRoot(sum);
                //              Debug.Log(n + " prediction:" + nodes[n].prediction);
            }
        }
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

    float activateSigmoid(float sum)
    {
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

    void initDataX()
    {
        data = new DataType[numData];
        for (int d = 0; d < numData; d++)
        {
            float s = 2f;
            float value = d  + 1 + Random.Range(-s, s);
            value = d * 5;
            value = d + 2;
            if (d == 0) value = 1;
            if (d == 1) value = 2;
            if (d == 2) value = 4;
            if (d == 3) value = 4;
            data[d] = new DataType("data", value, new Vector3(-4, 0, d * -4));
            data[d].text = createDataText(data[d]);
            sizeData(data[d]);
        }
        int numPredictions = 1;
        int c = 0;
        float val = 10;
        predictions = new DataType[numPredictions];
        predictions[c] = new DataType("prediction", val, new Vector3(-4 - c, 0, 3));
        predictions[c].text = createDataText(predictions[c]);
        sizeData(predictions[c]);
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
                    nodes[n].texts[i].text = nodes[n].weights[i].ToString("F2");
                    sizeLink(n, i);
                }
                nodes[n].text.text = nodes[n].prediction.ToString("F2");
            }
        }
    }

    void initTarget() {
        Vector3 posTarget = nodes[numNodes - 1].go.transform.position + new Vector3(3f, 0, 0);
        target = createDataType("target", 3.14f, posTarget);
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
        numNodes = 10;
        //initOrder("spider");
        initNodeLocation("pyramid");
        initNodesPyramid();
    }

    void sizeDataOld(int n) {
        float s = .1f;
        float sy = data[n].value * s;
        GameObject dataGo = data[n].go;
        dataGo.transform.localScale = new Vector3(1, sy, 1);
        dataGo.transform.position = new Vector3(dataGo.transform.position.x, sy / 2, dataGo.transform.position.z);
        data[n].text.transform.position = dataGo.transform.position + new Vector3(0, dataGo.transform.localScale.y + .05f, 0);
    }

    void sizeNode(int n)
    {
        float s = .1f;
        float sy = nodes[n].prediction * s;
        if (float.IsNaN(sy)) sy = 0;
        GameObject nodeGo = nodes[n].go;
        nodeGo.transform.localScale = new Vector3(1, sy, 1);
        nodeGo.transform.position = new Vector3(nodeGo.transform.position.x, sy/2, nodeGo.transform.position.z);
        nodes[n].text.text = nodes[n].prediction.ToString("F2");
        nodes[n].text.transform.position = nodeGo.transform.position + new Vector3(0, nodeGo.transform.localScale.y + .05f, 0);
    }

    void randomizeWeights() {
        for (int n = 0; n < numNodes; n++) {
            if (nodes[n].inputsN != null)
            {
                for (int i = 0; i < nodes[n].inputsN.Length; i++)
                {
                    nodes[n].weights[i] = Random.Range(1, 10);
                }
            }
        }
    }

    void sizeLink(int n, int i) {
        float s = .125f;
        float w = nodes[n].weights[i];
        //        if (w <= 0) w = 1;
        //w = -1;
        //w = sigmoid(w);
        //Debug.Log(w);
        //if (w < .001) w = .001f;
        //if (w > 1000) w = 1000;
        float sx = s * w;
        if (float.IsNaN(sx)) sx = 0;
        nodes[n].links[i].transform.localScale = new Vector3(sx, .1f, nodes[n].links[i].transform.localScale.z);
    }

    void initTargetMsg()
    {
        GameObject confidenceGo = new GameObject("target");
        confidenceGo.transform.position = new Vector3(-2, 0, -2.5f);
        //
        GameObject go = new GameObject("text target");
        go.transform.SetParent(GameObject.Find("Canvas").transform);
        go.transform.Rotate(89, 0, 0);
        go.transform.position = confidenceGo.transform.position;
        go.transform.localScale = new Vector3(.02f, .02f, .02f);
        targetText = go.AddComponent<Text>();
        RectTransform rect = go.GetComponent<RectTransform>();
        Font font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        targetText.font = font;
        targetText.name = go.name + ".";
        targetText.color = Color.black;
        targetText.alignment = TextAnchor.MiddleCenter;
        targetText.text = "target:" + target.value.ToString("F1");
    }

    void initConfidenceMsg() {
        GameObject confidenceGo = new GameObject("confidence");
        confidenceGo.transform.position = new Vector3(-2, 0, -3);
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
                nodes[n].texts = new Text[nodes[n].inputsN.Length];
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
            nodeLocation[4].y = 1 * -s;
            //
            nodeLocation[5].x = 2 * s;
            nodeLocation[5].y = 3 * -s;
            //
            nodeLocation[6].x = 2 * s;
            nodeLocation[6].y = 5 * -s;
            //
            nodeLocation[7].x = 4 * s;
            nodeLocation[7].y = 2 * -s;
            //
            nodeLocation[8].x = 4 * s;
            nodeLocation[8].y = 4 * -s;
            //
            nodeLocation[9].x = 6 * s;
            nodeLocation[9].y = 3 * -s;
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
        numInputs = 4;
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
        numInputs = 4;
        inputsN = new int[numInputs];
        inputsN[0] = 0;
        inputsN[1] = 1;
        inputsN[2] = 2;
        inputsN[3] = 3;
        weights = new float[numInputs];
        weights[0] = 4;
        weights[1] = 5;
        weights[2] = 2;
        weights[3] = 3;
        createNode(5, inputsN, weights, -1);
        //
        numInputs = 4;
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
        numInputs = 3;
        inputsN = new int[numInputs];
        inputsN[0] = 4;
        inputsN[1] = 5;
        inputsN[2] = 6;
        weights = new float[numInputs];
        weights[0] = 2;
        weights[1] = 3;
        weights[2] = 2;
        createNode(7, inputsN, weights, -1);
        //
        numInputs = 3;
        inputsN = new int[numInputs];
        inputsN[0] = 4;
        inputsN[1] = 5;
        inputsN[2] = 6;
        weights = new float[numInputs];
        weights[0] = 2;
        weights[1] = 3;
        weights[1] = 2;
        createNode(8, inputsN, weights, -1);
        //
        numInputs = 2;
        inputsN = new int[numInputs];
        inputsN[0] = 7;
        inputsN[1] = 8;
        weights = new float[numInputs];
        weights[0] = 2;
        weights[1] = 3;
        createNode(9, inputsN, weights, -1);
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
        GameObject go = new GameObject("text node " + n);
        go.transform.SetParent(GameObject.Find("Canvas").transform);
        go.transform.Rotate(89, 0, 0);
        go.transform.position = new Vector3(nodeLocation[n].x, .55f, nodeLocation[n].y);
        go.transform.localScale = new Vector3(.02f, .02f, .02f);
        Text text = go.AddComponent<Text>();
        RectTransform rect = go.GetComponent<RectTransform>();
        Font font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        text.font = font;
        text.name = go.name + ".";
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = nodes[n].prediction.ToString("F2");
        nodes[n].text = text;
    }

    void createLinkText(int n, int i, string txt)
    {
        GameObject go = new GameObject("text link " + n);
        go.transform.SetParent(GameObject.Find("Canvas").transform);
        go.transform.Rotate(89, 0, 0);
        GameObject goLink = nodes[n].links[i];
        go.transform.position = new Vector3(goLink.transform.position.x, .25f, goLink.transform.position.z);
        go.transform.localScale = new Vector3(.02f, .02f, .02f);
        Text text = go.AddComponent<Text>();
        RectTransform rect = go.GetComponent<RectTransform>();
        Font font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        text.font = font;
        text.name = go.name + ".";
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = nodes[n].weights[i].ToString("F2");
        nodes[n].texts[i] = text;
    }
}
