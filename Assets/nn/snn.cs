using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class snn : MonoBehaviour {
    int numNodes; // = 16; // equals res of images (number of pixels)
    int numPics = 16; //9;
    int resX = 25; //6; //256;
    float barSize = 150;
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
    int[] sortedOutputs;
    float output;
    GameObject[] pics;
    GameObject[] bars;
    Texture2D[] texs;
    Texture2D[] outputTexs;
    GameObject[] quads;
    string root = "fruits"; // "sketch";

	void Start () {
        float t = Time.realtimeSinceStartup;
        numNodes = resX * resX;
        Debug.Log("numPics:" + numPics + " res:" + resX + " numNodes:" + numNodes + " !:" + (numNodes * (numNodes - 1)) + "\n");
        outputs = new float[numPics];
        sortedOutputs = new int[numPics];
        nodes = new NodeType[numNodes];
        pics = new GameObject[numPics];
        bars = new GameObject[numPics];
        texs = new Texture2D[numPics];
        outputTexs = new Texture2D[numPics];
        quads = new GameObject[numPics];
        //
        for (int p = 0; p < numPics; p++) {
            loadPic2Node(p);
            calcNodes();
            createOutputTex(p);
            outputs[p] = output;
        }
        sortOutputs();
        //return;
        //showPics();
        //rangePics();
//        reorder();
        Debug.Log("secs:" + (Time.realtimeSinceStartup - t) + "\n");

//        showCools();
        cool(0);
        cool(1);
        cool(2);
        cool(3);
        //
        cooler(0);
        cooler(1);
        //cooler(2);
        //cooler(3);
        //cooler(4);
        //cooler(5);
        //cooler(6);
        //cooler(7);
        //cooler(8);
        //cooler(9);
        cooler(10);
        //cooler(11);
        //cooler(12);
        //cooler(13);
        //cooler(14);
        //cooler(15);
        //
        super(0);
        sortOutputs();
        showPics();
        rangePics();
	}

    void super(int p) {
        //GameObject[,] pixels = new GameObject[resX, resX];
        int pp = sortedOutputs[p];
        GameObject goP = new GameObject("pic" + pp);
        pics[p] = goP;
        string filePath = root + pp;
        Texture2D texFrom = Resources.Load(filePath) as Texture2D;
        //tex = new Texture2D(resX, resX, TextureFormat.RGBA32, false);
        float x0 = p * (resX * 1.1f);
        //
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "cool" + p;
        //quad.transform.parent = goP.transform;
        quad.transform.position = new Vector3(x0, 0, -6f * resX);
        quad.transform.localScale = new Vector3(resX, resX, resX);
        quad.transform.Rotate(90, 0, 0);
        //
        Texture2D tex = readTexture(texFrom);
        Debug.Log("tex:" + tex.width + " x " + tex.height + "\n");
        //
        //tex = texFrom;
        quad.GetComponent<Renderer>().material.mainTexture = tex;
        //
        for (int n = 0; n < 1; n++)
        {
            int rx = Random.Range(0, tex.width);
            int ry = Random.Range(0, tex.height);
            tex.SetPixel(rx, ry, Color.red);
            //Color colSum = new Color(0, 0, 0, 0);
            //Debug.Log("super:" + n);
            //for (int xn = 0; xn < tex.width; xn++)
            //{
            //    for (int yn = 0; yn < tex.height; yn++)
            //    {
            //        Color aveCol = distSquaredOfNeighbors(tex, xn, yn);
            //        //tex.SetPixel(xn, yn, aveCol);
            //        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //        float x = quad.transform.position.x + xn;
            //        float z = quad.transform.position.z - 1.5f * tex.height + yn;
            //        go.transform.position = new Vector3(x, s / 2, z);
            //        go.transform.localScale = new Vector3(1, 1, 1);
            //        go.GetComponent<Renderer>().material.color = aveCol;
            //        colSum += aveCol;
            //    }
            //}
            tex.Apply();
//            outputs[p] = colSum.r + colSum.g + colSum.b;
        }
    }

    void cooler(int p) {
        //GameObject[,] pixels = new GameObject[resX, resX];
        int pp = sortedOutputs[p];
        GameObject goP = new GameObject("pic" + pp);
        pics[p] = goP;
        string filePath = root + pp;
        Texture2D texFrom = Resources.Load(filePath) as Texture2D;
        //tex = new Texture2D(resX, resX, TextureFormat.RGBA32, false);
        float x0 = p * (resX * 1.1f);
        //
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "cool" + p;
        //quad.transform.parent = goP.transform;
        quad.transform.position = new Vector3(x0, 0, -4.5f * resX);
        quad.transform.localScale = new Vector3(resX, resX, resX);
        quad.transform.Rotate(90, 0, 0);
        //
        Texture2D tex = readTexture(texFrom);
        Debug.Log("tex:" + tex.width + " x " + tex.height + "\n");
        //
        //tex = texFrom;
        quad.GetComponent<Renderer>().material.mainTexture = tex;
        //
        for (int s = 2; s < tex.width/2; s *= 2)
        {
            Color colSum = new Color(0, 0, 0, 0);
            Debug.Log("s:" + s + " tree:" + tex.width / s);
            for (int xn = 0; xn < tex.width / s; xn++)
            {
                for (int yn = 0; yn < tex.height / s; yn++)
                {
                    Color aveCol = distSquaredOfNeighbors(tex, xn, yn);
                    if (s == 8)
                    {
                        //tex.SetPixel(xn, yn, aveCol);
                        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        float x = quad.transform.position.x + xn;
                        float z = quad.transform.position.z - .65f * tex.height + yn;
                        go.transform.position = new Vector3(x, s / 2, z);
                        go.transform.localScale = new Vector3(1, 1, 1);
                        go.GetComponent<Renderer>().material.color = aveCol;
                    }
                    colSum += aveCol;
                }
            }
            tex.Apply();
            outputs[p] = colSum.r + colSum.g + colSum.b;
        }
    }

    Color diffOfFour(Texture2D tex, int xn0, int yn0)
    {
        int s = 2;
        Color colSum = new Color(0, 0, 0, 0);
        for (int xn = 0; xn < s; xn += s)
        {
            for (int yn = 0; yn < s; yn += s)
            {
                colSum += tex.GetPixel(xn0 * s + xn, yn0 * s + yn);
            }
        }
        Color colAve = colSum / (s * s);
        colAve = colSum - tex.GetPixel(xn0 * s, yn0 * s);
        colAve.r = Mathf.Abs(colAve.r);
        colAve.g = Mathf.Abs(colAve.g);
        colAve.b = Mathf.Abs(colAve.b);
        //        Debug.Log(colAve);
        return colAve;
    }

    Color aveOfFour(Texture2D tex, int xn0, int yn0)
    {
        int s = 2;
        Color colSum = new Color(0, 0, 0, 0);
        for (int xn = 0; xn < s; xn += s) {
            for (int yn = 0; yn < s; yn += s) {
                colSum += tex.GetPixel(xn0 * s + xn, yn0 * s + yn);
            }
        }
        //Color colAve = colSum / (s * s);
        Color colAve = colSum;
//        Debug.Log(colAve);
        return colAve;
    }

    void showCools()
    {
        for (int p = 0; p < numPics; p++)
        {
            cool(p);
        }
    }

    void cool(int p) {
        int pp = sortedOutputs[p];
        GameObject goP = new GameObject("pic" + pp);
        pics[p] = goP;
        string filePath = root + pp;
        Texture2D texFrom = Resources.Load(filePath) as Texture2D;
        //tex = new Texture2D(resX, resX, TextureFormat.RGBA32, false);
        float x0 = p * (resX * 1.1f);
        //
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "cool" + p;
        //quad.transform.parent = goP.transform;
        quad.transform.position = new Vector3(x0, 0, -3f * resX);
        quad.transform.localScale = new Vector3(resX, resX, resX);
        quad.transform.Rotate(90, 0, 0);
        //
        Texture2D tex = readTexture(texFrom);
        //
        //tex = texFrom;
        quad.GetComponent<Renderer>().material.mainTexture = tex;
        //
        for (int xn = 0; xn < tex.width; xn++)
        {
            for (int yn = 0; yn < tex.height; yn++)
            {
//                float rgb = nodes[n].outputVal;
                Color col = tex.GetPixel(xn, yn);
//                Color aveCol = aveOfNeighbors(tex, xn, yn);
                Color aveCol = distSquaredOfNeighbors(tex, xn, yn);
                tex.SetPixel(xn, yn, aveCol);
                //            Debug.Log(xn + "," + yn + " = " + col + " aveNeighbors:" + aveCol + "\n");
            }
        }
        tex.Apply();
    }

    Color distSquaredOfNeighbors(Texture2D tex, int xn0, int yn0)
    {
        Color colSum = new Color(0, 0, 0);
        for (int xn = 0; xn <= tex.width; xn++)
        {
            for (int yn = 0; yn <= tex.height; yn++)
            {
                float dist = Mathf.Sqrt(Mathf.Pow(xn0 - xn, 2) + Mathf.Pow(yn0 - yn, 2));
                float f = 1 - dist / tex.width;
                Color colVal = tex.GetPixel(xn, yn) * f;
//                colSum += new Color(sigmoid(colVal.r), sigmoid(colVal.g), sigmoid(colVal.b));
                colSum += colVal;

            }
        }
        int s = tex.width * tex.height;
        Color colAve = colSum / s;
        return colAve;
    }

    Color aveOfNeighbors(Texture2D tex, int xn0, int yn0) {
        Color colSum = new Color(0, 0, 0);
        int r = 4;
        for (int xn = -r; xn <= r; xn++) {
            for (int yn = -r; yn <= r; yn++) {
                colSum += tex.GetPixel(xn0 + xn, yn0 + yn);                
            }        
        }
        colSum -= tex.GetPixel(xn0, yn0);
        int s = 2 * r + 1;
        s *= s;
        s -= 1;
        Color colAve = colSum / s;
        return colAve;
    }

    Texture2D readTexture(Texture2D texFrom) {
        Texture mainTexture = texFrom;
        Texture2D texture2D = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.RGBA32, false);

        RenderTexture currentRT = RenderTexture.active;

        RenderTexture renderTexture = new RenderTexture(mainTexture.width, mainTexture.height, 32);
        Graphics.Blit(mainTexture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

//        Color[] pixels = texture2D.GetPixels();

        RenderTexture.active = currentRT;

        return texture2D;
    }

    void showPics()
    {
        for (int p = 0; p < numPics; p++)
        {
            showPic(p);
        }
    }

    void showPic(int p)
    {
        int pp = sortedOutputs[p];
        GameObject goP = new GameObject("pic" + pp);
        pics[p] = goP;
        string filePath = root + pp;
        Texture2D tex = Resources.Load(filePath) as Texture2D;
        float x0 = p * (resX * 1.1f);
        //
        GameObject goBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //goBar.transform.parent = goP.transform;
        bars[p] = goBar;
        //
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "quad" + p;
        //quad.transform.parent = goP.transform;
        quad.transform.position = new Vector3(x0, 0, 0);
        quad.transform.localScale = new Vector3(resX, resX, resX);
        quad.transform.Rotate(90, 0, 0);
        quad.GetComponent<Renderer>().material.mainTexture = tex;
        quads[p] = quad;
    }

    void rangePics() {
        float min = 0;
        float max = 0;
        for (int p = 0; p < numPics; p++) {
//            Debug.Log(outputs[p]);
            if (p == 0 || outputs[p] < min) {
                min = outputs[p];                
            }
            if (p == 0 || outputs[p] > max)
            {
                max = outputs[p];
            }
        }
        Debug.Log("min:" + min + " max:" + max + "\n");
        float range = max - min;
        for (int p = 0; p < numPics; p++){
            int pp = sortedOutputs[p];
            float x = p * (resX * 1.1f);
            float sy = (outputs[pp] - min) / range * barSize;
            sy += 1;
            bars[p].transform.localScale = new Vector3(1, 1, sy);
            bars[p].transform.position = new Vector3(x, 0, resX + 1 + sy/2);
            //
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = new Vector3(- resX, 0, resX + 1 + sy);
            go.transform.localScale = new Vector3(10, 1, 1);
        }
    }

    void sortOutputs() {
        bool[] chosen = new bool[numPics];
        for (int p = 0; p < numPics; p++)
        {
            float maxOutput = -1;
            int ppMax = -1;
            bool ynFound = false;
            for (int pp = 0; pp < numPics; pp++)
            {
                if (chosen[pp] == false) {
                    if (outputs[pp] > maxOutput || ynFound == false)
                    {
                        ynFound = true;
                        maxOutput = outputs[pp];
                        ppMax = pp;
                    }
                }
            }
//            Debug.Log(ppMax);
            if (ppMax != -1)
            {
                chosen[ppMax] = true;
                sortedOutputs[p] = ppMax;
            }
        }
    }

    void loadPic2Node(int p) {
        string filePath = root + p;
        Texture2D tex = Resources.Load(filePath) as Texture2D;
        texs[p] = tex;
        for (int x = 0; x < resX; x++)
        {
            for (int y = 0; y < resX; y++)
            {
                Color col = tex.GetPixel(x, y);
                int n = y * resX + x;
                int v = (int)((col.r + col.g + col.b) * col.a) + 1;
                nodes[n] = new NodeType(v, 0);
            }
        }
    } 

    void createOutputTex(int p) {
        Texture2D tex = new Texture2D(resX, resX);
        for (int n = 0; n < numNodes; n++)
        {
            int xn = n / resX;
            int yn = n % resX;
            float rgb = nodes[n].outputVal;
            Color col = new Color(rgb, rgb, rgb);
            col = Random.ColorHSV();
            //col = Color.black;
            tex.SetPixel(xn, yn, col);
            //if (p == 0) {
            //    Debug.Log(xn + "," + yn + " = " + rgb);
            //}
        }
        tex.Apply();
        float x0 = p * (resX * 1.1f);
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "quad" + p;
        //quad.transform.parent = goP.transform;
        quad.transform.position = new Vector3(x0, 0, -1.5f * resX);
        quad.transform.localScale = new Vector3(resX, resX, resX);
        quad.transform.Rotate(90, 0, 0);
        quad.GetComponent<Renderer>().material.mainTexture = tex;

    }

    float sigmoid(float x) {
        return 1 / (1 + Mathf.Exp(-x));
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

    void calcNode(int nNode) {
        float sum = 0;
        for (int n = 0; n < numNodes; n++)
        {
            if (nNode != n)
            {
                // unit slope = (other / self) ratio of size
                float fract = nodes[nNode].inputVal / nodes[n].inputVal;
                // reduce by pixel distance squared (like the universe)
                float dist = getDistNodes(n, nNode);
//                fract *= 1 / (dist * dist);
                fract *= 1 / dist;
                sum += fract;
            }
        }
        float ave = sum / (numNodes - 1);
        int nCenter = resX * (resX/2) + resX/2;
        nCenter = 0;
        float distCenter = getDistNodes(nCenter, nNode);
        distCenter++;
//        ave *= 1 / (distCenter * distCenter);
        ave *= 1 / distCenter;
        nodes[nNode].outputVal = ave;
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
