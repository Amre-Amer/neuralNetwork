using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class snn : MonoBehaviour {
    int numNodes; // = 16; // equals res of images (number of pixels)
    int numPics = 9;
    NodeType[] nodes;
    struct NodeType {
        public float inputVal;
        public float outputVal;
        public NodeType(int inputVal0, int outputVal0) {
            inputVal = inputVal0;
            outputVal = outputVal0;
        }
    }
    float[] outputs;
    float output;
    GameObject[] pics;
    GameObject[] bars;
    int resX = 4;

	void Start () {
        numNodes = resX * resX;
        Debug.Log("numNodes:" + numNodes + " !:" + (numNodes * (numNodes - 1)) + "\n");
        outputs = new float[numPics];
        nodes = new NodeType[numNodes];
        pics = new GameObject[numPics];
        bars = new GameObject[numPics];
        //
        showPics();
        for (int p = 0; p < numPics; p++) {
            loadPic(p);
            calcNodes();
            outputs[p] = output;
        }
        sortPics();
	}

    void sortPics() {
        float min = 0;
        float max = 0;
        for (int p = 0; p < numPics; p++) {
            Debug.Log(outputs[p]);
            if (p == 0 || outputs[p] < min) {
                min = outputs[p];                
            }
            if (p == 0 || outputs[p] > max)
            {
                max = outputs[p];
            }
        }
        float range = max - min;
        for (int p = 0; p < numPics; p++){
            float x = p * (resX + 1) + resX/2 - .5f;
            float sy = (outputs[p] - min) / range * 10;
            sy += 1;
            bars[p].transform.localScale = new Vector3(1, 1, sy);
            bars[p].transform.position += new Vector3(x, 0, resX + 1 + sy/2);
        }
        Debug.Log("min:" + min + " max:" + max + "\n");
    }

    void loadPic(int p) {
        string filePath = "sketch" + p;
        Texture2D tex = Resources.Load(filePath) as Texture2D;
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                Color col = tex.GetPixel(x, y);
                int n = y * tex.width + x;
                int v = (int)col.r + 1;
                nodes[n] = new NodeType(v, 0);
            }
        }
    } 

    void showPics() {
        for (int p = 0; p < numPics; p++)
        {
            showPic(p);
        }
    }

    void showPic(int p) {
        GameObject goP = new GameObject("pic" + p);
        pics[p] = goP;
        string filePath = "sketch" + p;
        Texture2D tex = Resources.Load(filePath) as Texture2D;
        float x0 = p * (resX + 1);
        for (int x = 0; x < tex.width; x++) {
            for (int y = 0; y < tex.height; y++) {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = new Vector3(x0 + x, 0, y);
                Color col = tex.GetPixel(x, y);
                go.GetComponent<Renderer>().material.color = col;
                go.transform.parent = goP.transform;
            }
        }
        GameObject goBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        goBar.transform.parent = goP.transform;
        bars[p] = goBar;
    }
	
    void calcNodes()
    {
        output = 0;
        for (int n = 0; n < numNodes; n++)
        {
            calcNode(n);
            output += nodes[n].outputVal;
        }
    }

    float sigmoid(float x) {
        return 1 / (1 + Mathf.Exp(-x));
    }

    void calcNode(int n) {
        float sum = 0;
        for (int nn = 0; nn < numNodes; nn++)
        {
            if (nn != n)
            {
                // unit slope = (other / self) ratio of size
                float fract = nodes[nn].inputVal / nodes[n].inputVal;
                // reduce by pixel distance squared (like the universe)
                float dist = getDistNodes(n, nn);
                fract *= 1 / (dist * dist);
                sum += fract;
            }
        }
        float ave = sum / (numNodes - 1);
        int nCenter = 12;
        float distCenter = getDistNodes(nCenter, n);
//        ave *= 1 / (distCenter * distCenter);
//        ave *= 1 / distCenter;
        nodes[n].outputVal = ave;
    }

    float getDistNodes(int n, int nn) {
        int xn = n / resX;
        int yn = n % resX;
        //
        int xnn = nn / resX;
        int ynn = nn % resX;
        //
        float dist = Vector2.Distance(new Vector2(xn, yn), new Vector2(xnn, ynn));
        return dist;
    }
}
