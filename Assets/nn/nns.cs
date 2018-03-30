using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nns : MonoBehaviour {
    public int numNodes = 100; //9;
    int numLinks = 0;
    float[] nodes;
    GameObject[] nodeGos;
    int cntLink;
    float sumLinks;
    struct LinkType {
        public int n1;
        public int n2;
        public LinkType(int n10, int n20) {
            n1 = n10;
            n2 = n20;
        }
    }
    LinkType[] links;

	// Use this for initialization
	void Start () {
        LinkType lt = new LinkType(1, 2);
        Debug.Log(lt.n1 + " " + lt.n2);
        //init();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnGUI()
    {
        string txt = "numNode: " + numNodes + " links:" + cntLink + " sum:" + sumLinks + "\n";
        GUI.Label(new Rect(10, Screen.height - 50, 200, 50), txt);
    }

    void init() {
        numLinks = numNodes * (numNodes - 1) / 2;
        nodes = new float[numNodes];
        links = new LinkType[numLinks];
        nodeGos = new GameObject[numNodes];
        //
        createNodes();
        loadNodes();
        createLinks();
        loadLinks();
        //
        cntLink = (numNodes * (numNodes - 1) / 2);
        sumLinks = getSum();
    }

    void createNodes() {
        for (int n = 0; n < numNodes; n++)
        {
            createNode(n);
        }
    }

    void loadNodes() {
        for (int n = 0; n < numNodes; n++)
        {
            loadNodeRandom(n);
            //loadNodeCos(n);
            //loadNodeFlat(n);
        }
    }

    void createLinks() {
        for (int n = 0; n < numNodes; n++)
        {
            for (int nn = n + 1; nn < numNodes; nn++)
            {
                createLink(n, nn);
            }
        }
    }

    void loadLinks() {
        for (int n = 0; n < numNodes; n++)
        {
            for (int nn = n + 1; nn < numNodes; nn++)
            {
                float ratio = getLinkValue(n, nn);
                loadLink(n, nn, ratio);
            }
        }
    }

    float getSum() {
        float sum = 0;
        for (int n = 0; n < numNodes; n++)
        {
            for (int nn = n + 1; nn < numNodes; nn++)
            {
                float ratio = getLinkValue(n, nn);
//                sum += ratio;
                float ratioSigmoid = sigmoid(ratio);
                sum += ratioSigmoid;
            }
        }
        return sum;
    }

    float sigmoid(float x) {
        float sig = 1 / (1 + Mathf.Exp(-x));
//        Debug.Log(x + " sigmoid " + sig);
        return sig;
    }

    void loadNodeCos(int n) {
        float ang = (float)n / numNodes * 90;
        nodes[n] = numNodes * Mathf.Cos(ang * Mathf.Deg2Rad);
    }

    void loadNodeRandom(int n)
    {
        nodes[n] = Random.Range(0, numNodes);
    }

    void loadNodeFlat(int n)
    {
        nodes[n] = 6;
    }

	void createNode(int n) {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.position = Random.insideUnitSphere * numNodes;
        nodeGos[n] = go;
        //
        GameObject goBarNode = GameObject.CreatePrimitive(PrimitiveType.Cube);
        goBarNode.transform.position = new Vector3(n * 2, nodes[n] / 2, numNodes);
        goBarNode.transform.localScale = new Vector3(1, nodes[n], 1);
        //nodeGos[n] = go;
    }

    void createLink(int n1, int n2) {
        //links[cntLink]
        //
        createLinkGo(n1, n2);
//        links[]
        //
        //GameObject goBarLink = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //float w = .5f;
        //goBarLink.transform.position = new Vector3(cntLink * .5f, ratio / 2, numNodes * 2);
        //goBarLink.transform.localScale = new Vector3(w, ratio, 1);
        cntLink++;
    }

    void createLinkGo(int n1, int n2) {
        Vector3 posN1 = nodeGos[n1].transform.position;
        Vector3 posN2 = nodeGos[n2].transform.position;
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.transform.position = posN1;
        go.transform.LookAt(posN2);
        float dist = Vector3.Distance(posN1, posN2);
        go.transform.position += go.transform.forward * dist / 2;
        go.transform.Rotate(-90, 0, 0);
        go.transform.localScale = new Vector3(.2f, dist / 2, .2f);
        go.GetComponent<Renderer>().material.color = Random.ColorHSV();
    }

    void loadLink(int n1, int n2, float ratio) {
//        links[]         
    }

    float getLinkValue(int n1, int n2) {
        return nodes[n2] / nodes[n1];
    }

    string getAlphaForInt(int n) {
        string str = "?";
        if (n == 0) str = "A";
        if (n == 1) str = "B";
        if (n == 2) str = "C";
        if (n == 3) str = "D";
        if (n == 4) str = "E";
        if (n == 5) str = "F";
        if (n == 6) str = "G";
        if (n == 7) str = "H";
        if (n == 8) str = "I";
        return str;
    }
}
