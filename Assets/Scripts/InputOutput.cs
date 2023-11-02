using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputOutput : MonoBehaviour
{
    public TextMeshProUGUI inputText;
    public TMP_InputField myInput;
    public RectTransform gridObject;
    public TextMeshProUGUI textBoxPrefab;
    public float gridHeight, textHeight;
    public int offsetY = 20;
    public string label;
    int myCaretPos;
    int myInputLength = 0;
    List<string> previousCommands = new();
    int previousCommandIndex = 0;
    bool cycle;
    int scrollValue = 0;
    [SerializeField] Machines machines;
    void Start()
    {
        
        myInput.onValueChanged.AddListener(delegate { UpdateText(); });
        myInput.onSubmit.AddListener(delegate { AddText(myInput.text, true); if(myInput.text.Length > 0) previousCommands.Add(myInput.text); });
        myInput.onDeselect.AddListener(delegate { myInput.ActivateInputField(); });
        myInput.onEndEdit.AddListener(delegate { myInput.ActivateInputField(); });
        label = (string.IsNullOrEmpty(machines.activeMachine.label) ? machines.activeMachine.IP : machines.activeMachine.label);
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();
        gridHeight = -inputText.rectTransform.anchoredPosition.y;
        inputText.text = $"({label}) {(machines.localMachine.activeDirectory.Length > 30 ? machines.localMachine.activeDirectory.Substring(0,10) + "..." + machines.localMachine.activeDirectory.Substring(machines.localMachine.activeDirectory.Length-16, 15) : machines.localMachine.activeDirectory)}><color=#4543B0>\u2588</color>" + myInput.text + "\u2588";
        myCaretPos = inputText.text.Length-1;

        myInput.ActivateInputField();
        InvokeRepeating(nameof(CaretBlink), 0.001f, 0.7f);
        /*Debug.Log("Caret :" + myCaretPos);*/


    }




    IEnumerator AdjustHeight()
    {

        label = (machines.activeMachine.label == null || machines.activeMachine.label == "" ? machines.activeMachine.IP : machines.activeMachine.label);
        yield return null;
        textHeight = inputText.preferredHeight;
        gridHeight = -inputText.rectTransform.anchoredPosition.y + textHeight;
        inputText = Instantiate(textBoxPrefab, new Vector2(textBoxPrefab.transform.position.x, -gridHeight - offsetY), Quaternion.identity);
        inputText.transform.SetParent(gridObject.transform, false);
        inputText.text = $"({label}) {(machines.localMachine.activeDirectory.Length > 30 ? machines.localMachine.activeDirectory.Substring(0,10) + "..." + machines.localMachine.activeDirectory.Substring(machines.localMachine.activeDirectory.Length-16, 15) : machines.localMachine.activeDirectory)}><color=#4543B0>\u2588</color>";

        if (inputText.rectTransform.anchoredPosition.y + gridObject.anchoredPosition.y < -500)
            gridObject.anchoredPosition += new Vector2(0, -500 - (inputText.rectTransform.anchoredPosition.y + gridObject.anchoredPosition.y));

        /*Debug.Log("CaretAdjust: " + (inputText.text.Length));*/

        myCaretPos = inputText.text.Length;
        inputText.text = inputText.text.Insert(myCaretPos, "\u2588");

        myInput.text = "";
       
    }

    public void UpdateText()
    {
        if (Input.GetKey(KeyCode.UpArrow) )
        {
            myInput.caretPosition = inputText.text.Length;
            return;
        }
        if ( Input.GetKey(KeyCode.DownArrow))
        {
            myInput.caretPosition = 0;
            return;
        }
        inputText.text = $"({label}) {(machines.localMachine.activeDirectory.Length > 30 ? machines.localMachine.activeDirectory.Substring(0,10) + "..." + machines.localMachine.activeDirectory.Substring(machines.localMachine.activeDirectory.Length-16, 15) : machines.localMachine.activeDirectory)}><color=#4543B0>\u2588</color>" + myInput.text;

        gridObject.anchoredPosition -= new Vector2(0, scrollValue);
        scrollValue = 0;
        if (!Input.GetKey(KeyCode.Return) && myInputLength > myInput.text.Length)
        {
            myCaretPos--;
            
        }
        else if (!Input.GetKey(KeyCode.Return) && myInputLength < myInput.text.Length)
        {
            myCaretPos++;
        }
        myInputLength = myInput.text.Length;
  
        inputText.text = inputText.text.Insert(myCaretPos, "\u2588");
        if (inputText.rectTransform.anchoredPosition.y - inputText.preferredHeight + gridObject.anchoredPosition.y < -500)
            gridObject.anchoredPosition += new Vector2(0, -500 + inputText.preferredHeight - (inputText.rectTransform.anchoredPosition.y + gridObject.anchoredPosition.y));


    }

    public void Cd()
    {
        inputText.text = $"({label}) {(machines.localMachine.activeDirectory.Length > 30 ? machines.localMachine.activeDirectory.Substring(0,10) + "..." + machines.localMachine.activeDirectory.Substring(machines.localMachine.activeDirectory.Length-16, 15) : machines.localMachine.activeDirectory)}><color=#4543B0>\u2588</color>" + myInput.text;
        gridObject.anchoredPosition -= new Vector2(0, scrollValue);
        scrollValue = 0;
        myCaretPos = inputText.text.Length;
        myInputLength = myInput.text.Length;
        inputText.text = inputText.text.Insert(myCaretPos, "\u2588");
        if (inputText.rectTransform.anchoredPosition.y - inputText.preferredHeight + gridObject.anchoredPosition.y < -500)
            gridObject.anchoredPosition += new Vector2(0, -500 + inputText.preferredHeight - (inputText.rectTransform.anchoredPosition.y + gridObject.anchoredPosition.y));
    }

    public void ColorCharacter(Color32 color, int characterPos)
    {
        


        int vertexIndex = inputText.textInfo.characterInfo[inputText.textInfo.characterCount -characterPos -1].vertexIndex;
        inputText.textInfo.meshInfo[0].colors32[vertexIndex + 0] = color;
        inputText.textInfo.meshInfo[0].colors32[vertexIndex + 1] = color;
        inputText.textInfo.meshInfo[0].colors32[vertexIndex + 2] = color;
        inputText.textInfo.meshInfo[0].colors32[vertexIndex + 3] = color;
        inputText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

    }

    public void AddText(string textPassed, bool ShouldAddPathToText)
    {

        if (ShouldAddPathToText)
            inputText.text = $"({label}) {(machines.localMachine.activeDirectory.Length > 30 ? machines.localMachine.activeDirectory.Substring(0,10) + "..." + machines.localMachine.activeDirectory.Substring(machines.localMachine.activeDirectory.Length-16, 15) : machines.localMachine.activeDirectory)}><color=#4543B0>\u2588</color>" + textPassed;
        else
            inputText.text = textPassed;
        StartCoroutine(nameof(AdjustHeight));

        previousCommandIndex = 0;
        gridObject.anchoredPosition -= new Vector2(0, scrollValue);
        scrollValue = 0;
        myInput.ActivateInputField();
        
    }

    void CaretBlink()
    {

        Color32 color = new(255, 255, 255, 255);
        if (cycle)
            color = new Color32(0, 0, 0, 0);

        
        ColorCharacter(color, myInputLength - myInput.caretPosition);
        
        /*inputText.text = $"({label}) {(machines.localMachine.activeDirectory.Length > 30 ? machines.localMachine.activeDirectory.Substring(0,10) + "..." + machines.localMachine.activeDirectory.Substring(machines.localMachine.activeDirectory.Length-16, 15) : machines.localMachine.activeDirectory)}><color=\"black\">\u2588<color=\"white\">" + myInput.text + "<color=\"white\">\u2588";
        if(cycle)
            inputText.text = $"({label}) {(machines.localMachine.activeDirectory.Length > 30 ? machines.localMachine.activeDirectory.Substring(0,10) + "..." + machines.localMachine.activeDirectory.Substring(machines.localMachine.activeDirectory.Length-16, 15) : machines.localMachine.activeDirectory)}><color=\"black\">\u2588<color=\"white\">" + myInput.text + "<color=\"black\">\u2588";*/
        
        cycle = !cycle;

        

    }


    public void Cls()
    {
        while (gridObject.childCount > 1)
            DestroyImmediate(gridObject.GetChild(0).gameObject);
        inputText.rectTransform.anchoredPosition = new Vector2(textBoxPrefab.transform.position.x, - offsetY);
        gridObject.anchoredPosition = new Vector2(-600, 505);
        scrollValue = 0;




    }


    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (previousCommands.Count <= previousCommandIndex)
            {
                myInput.caretPosition = 99999;
                return;

            }

            
            previousCommandIndex++;
            myInput.text = previousCommands[^previousCommandIndex];
            myCaretPos = inputText.text.Length;
            myInput.caretPosition = 99999;
            Cd();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && previousCommandIndex > 1)
        {
            
            previousCommandIndex--;
            myInput.text = previousCommands[^previousCommandIndex];
            myCaretPos = 99999;
            myInput.caretPosition = inputText.text.Length;
            Cd();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && previousCommandIndex == 1)
        {
            
            previousCommandIndex--;
            myInput.text = "";
            myCaretPos = 99999;
            myInput.caretPosition = inputText.text.Length;
            Cd();
        }
        
        if(Input.GetKey(KeyCode.LeftArrow) && myInput.caretPosition >= 0)
        {
            inputText.text = inputText.text.Remove(myCaretPos, 1);
            myCaretPos = inputText.text.Length - myInput.text.Length + myInput.caretPosition;
            inputText.text = inputText.text.Insert(myCaretPos, "\u2588");
        }


        if (Input.GetKey(KeyCode.RightArrow) && myInput.caretPosition <= myInput.text.Length+1)
        {
            inputText.text = inputText.text.Remove(myCaretPos, 1);
            myCaretPos = inputText.text.Length - myInput.text.Length + myInput.caretPosition;
            inputText.text = inputText.text.Insert(myCaretPos, "\u2588");
        }

        

            
        

        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
        {
            gridObject.anchoredPosition -= new Vector2(0, 18);
            scrollValue -= 18;
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {

            gridObject.anchoredPosition += new Vector2(0, 18);
            scrollValue += 18;
        }

        if (Input.GetKeyDown(KeyCode.Return))
            CancelInvoke(nameof(CaretBlink));
        else if (Input.GetKeyUp(KeyCode.Return))
            InvokeRepeating(nameof(CaretBlink), 0, 0.7f);
            

    }
}
