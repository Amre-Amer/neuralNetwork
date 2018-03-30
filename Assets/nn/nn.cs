using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nn : MonoBehaviour {
    int numInputs = 2;
    float[] inputs;
    float[] weights;

	// Use this for initialization
	void Start () {
        inputs = new float[numInputs];
        inputs[0] = .3f;
        inputs[1] = .7f;
        weights = new float[numInputs];
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //void initNN(int numInputs, int numHidden, int numOutputs) {
        
    //}

    //void createNode(int numWeights, float bias) {
        
    //}

    void forwardFeed() {
                        
    }
}
