using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Organic : MonoBehaviour {
    int numNodes = 5;
    float target = 100;
    float delay = 1;
    int[] order;
    LocationType[] nodeLocation;
    OrganicType[] nodes;
    struct OrganicType {
        public int[] inputsN;
        public int outputN;
        public float[] weights;
        public float prediction;
        public GameObject go;
        public Text text;
        public GameObject[] links;
        public Text[] texts;
        public OrganicType(int[] inputsN0, int outputN0, float[] weights0, float prediction0, GameObject go0) {
            inputsN = inputsN0;
            outputN = outputN0;
            weights = weights0;
            prediction = prediction0;
            go = go0;
            text = null;
            links = null;
            texts = null;
        }
    }
    struct LocationType {
        public float x;
        public float y;
        LocationType(int x0, int y0) {
            x = x0;
            y = y0;
        }
    }
    int cntFrames;
    float delayStart;
    float correction;
    float error;
    float confidence;
    Text confidenceText;
    Text targetText;
    //struct LinkType {
    //    public         
    //}

	// Use this for initialization
	void Start () {
        initConfidence();
        initTarget();
        initOrder();
        initNodeLocation();
        initNodes();
        initNodeTexts();
        initLinks();
        initLinkTexts();
        //
        showNodes();
        fastForward();
        showNodes();
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.realtimeSinceStartup - delayStart > delay)
        {
            error = Mathf.Abs(correction - 1f);
            confidence = (1 - error) * 100;
            if (error > .001f)
            {
                delayStart = Time.realtimeSinceStartup;
                correct();
                fastForward();
                showNodes();
                cntFrames++;
            }
        }
	}

    void correct() {
        correction = target / nodes[numNodes - 1].prediction;
        confidenceText.text = cntFrames + " :" + confidence.ToString("F1") +  "%";
        Debug.Log(confidenceText.text + "\n");
        for (int n = 0; n < numNodes; n++) {
            if (nodes[n].inputsN != null)
            {
                for (int i = 0; i < nodes[n].inputsN.Length; i++)
                {
                    nodes[n].weights[i] *= correction;
                }
            }
        }
    }

    void fastForward() {
        for (int n = 0; n < numNodes; n++) {
            float sum = 0;
            if (nodes[n].inputsN != null) {
                for (int i = 0; i < nodes[n].inputsN.Length; i++) {
                    sum += nodes[nodes[n].inputsN[i]].prediction * nodes[n].weights[i];
                }
                nodes[n].prediction = sum;
            }
        }       
    }

    void showNodes() {
        for (int n = 0; n < numNodes; n++) {
            if (nodes[n].inputsN != null)
            {
                for (int i = 0; i < nodes[n].inputsN.Length; i++)
                {
                    nodes[n].texts[i].text = nodes[n].weights[i].ToString("F2");
                    updateLink(n, i);
                }
                nodes[n].text.text = nodes[n].prediction.ToString("F2");
            }
        }
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

    void updateLink(int n, int i) {
        float s = .125f;
        float w = nodes[n].weights[i];
        nodes[n].links[i].transform.localScale = new Vector3(s * w, .1f, nodes[n].links[i].transform.localScale.z);
    }

    void initTarget()
    {
        GameObject confidenceGo = new GameObject("target");
        confidenceGo.transform.position = new Vector3(0, 0, -2.5f);
        //
        GameObject go = new GameObject("text confidence");
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
        targetText.text = "target:" + target.ToString("F1");
    }

    void initConfidence() {
        GameObject confidenceGo = new GameObject("confidence");
        confidenceGo.transform.position = new Vector3(0, 0, -3);
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

    void initNodeTexts() {
        for (int n = 0; n < numNodes; n++)
        {
            createNodeText(n, name.ToString());
        }
    }

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

    void initNodeLocation() {
        float s = 2;
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

    void initOrder() {
        order = new int[numNodes];
        order[0] = 0;
        order[1] = 1;
        order[2] = 2;
        order[3] = 3;
        order[4] = 4;
    }

    void initNodes()
    {
        int numInputs = 2;
        int[] inputsN;
        float[] weights;
        nodes = new OrganicType[numNodes];
        //
        nodes[0] = new OrganicType(null, 2, null, 1, createNodeGo(0));
        //
        nodes[1] = new OrganicType(null, 2, null, 2, createNodeGo(1));
        //
        inputsN = new int[numInputs];
        inputsN[0] = 0;
        inputsN[1] = 1;
        weights = new float[numInputs];
        weights[0] = 5;
        weights[1] = 6;
        nodes[2] = new OrganicType(inputsN, 4, weights, 0, createNodeGo(2));
        //
        nodes[3] = new OrganicType(null, 4, null, 4, createNodeGo(3));
        //
        numInputs = 2;
        inputsN = new int[numInputs];
        inputsN[0] = 2;
        inputsN[1] = 3;
        weights = new float[numInputs];
        weights[0] = 7;
        weights[1] = 8;
        nodes[4] = new OrganicType(inputsN, 4, weights, 0, createNodeGo(4));
        //
        paintNodes();
        randomizeWeights();
    }

    void paintNodes() {
        for (int n = 0; n < numNodes; n++) {
            if (nodes[n].inputsN != null) {
                if (n == numNodes - 1)
                {
                    nodes[n].go.GetComponent<Renderer>().material.color = new Color(.5f, 1f, .5f);
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
        text.text = nodes[n].prediction.ToString();
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
        text.text = nodes[n].weights[i].ToString();
        nodes[n].texts[i] = text;
    }
}
