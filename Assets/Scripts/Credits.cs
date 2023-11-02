using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Credits : MonoBehaviour
{
    TextMeshProUGUI text;
    [SerializeField]
    SpriteRenderer background;
    float time = 0;
    // Start is called before the first frame update
    int firstVertexIndex, lastVertexIndex;
   

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        Invoke(nameof(DelayedStart), 0.3f);
        InvokeRepeating(nameof(ChangeColor), 1f, 0.001f);


    }

    void DelayedStart()
    {
        Debug.Log(text.textInfo.characterInfo.Length);
        Debug.Log(text.textInfo.meshInfo[0].vertexCount);
        firstVertexIndex = text.textInfo.characterInfo[0].vertexIndex;
        lastVertexIndex = text.textInfo.characterInfo[15].vertexIndex;
    }

    // Update is called once per frame
    void ChangeColor()
    {
        if(time < 255)
        background.color = new Color32(255, 255, 255, (byte)(time*10));
        Debug.Log(time);
        if (time >= 8)
            Application.Quit();

    /*        for (int vertexIndex =0; vertexIndex < text.textInfo.meshInfo[0].vertexCount-4; vertexIndex += 4)
            {

                text.textInfo.meshInfo[0].colors32[vertexIndex + 0] = new Color(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1));
                text.textInfo.meshInfo[0].colors32[vertexIndex + 1] = new Color(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1));
                text.textInfo.meshInfo[0].colors32[vertexIndex + 2] = new Color(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1));
                text.textInfo.meshInfo[0].colors32[vertexIndex + 3] = new Color(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1));
            }*/
    for (int i = firstVertexIndex; i <= lastVertexIndex + 3; i++)
        text.textInfo.meshInfo[0].colors32[i] = new Color32((byte)(i * Time.time - i), (byte)(i * 2 * Time.time - i*5), (byte)(i * 3 * Time.time - i*10), 255);
    text.UpdateVertexData();

    }

    private void Update()
    {
        time += Time.deltaTime;
    }

}
