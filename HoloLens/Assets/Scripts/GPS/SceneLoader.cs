using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class SceneLoader : MonoBehaviour {
    public Button SceneBtn;

    // Use this for initialization
    void Start () {
        Button loadSceneBtn = SceneBtn.GetComponent<Button>();

        loadSceneBtn.onClick.AddListener(LoadThisScene);

    }

    // Update is called once per frame
    void Update () {
		
	}

    void LoadThisScene()
    {
        Debug.Log("Pressed button " + SceneBtn.tag.ToString());
        SceneManager.LoadScene(SceneBtn.tag.ToString());
        Debug.Log("loadØ: "+ SceneBtn.tag.ToString());
   
    }
}
