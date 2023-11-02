using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using UnityEngine.UI;

public class Hackerman : MonoBehaviour
{
    [SerializeField] List<GameObject> passwordLetters = new();
    List<TextMeshProUGUI> passwordLettersTMP = new();
    public string password; 
    string currentLetters;
    [SerializeField] TMP_FontAsset myFont;
    [SerializeField] TMP_InputField myInputField;
    int myInputLength;
    bool isAwaitingClearing = false;
    TextMeshProUGUI hackermanText;

    private void Start()
    {
        StartCoroutine(LateAwake());
        StartCoroutine(LateStart());
    }

    IEnumerator LateAwake()
    {
        yield return null;
        hackermanText = GameObject.Find("HackermanText").GetComponent<TextMeshProUGUI>();
        

        hackermanText.text = hackermanText.text.Remove(hackermanText.text.IndexOf("{   }"), 5).Insert(hackermanText.text.IndexOf("{   }"),password.Count(x => Char.IsLetter(x)).ToString());
        hackermanText.text = hackermanText.text.Remove(hackermanText.text.IndexOf("{   }"), 5).Insert(hackermanText.text.IndexOf("{   }"), password.Count(x => Char.IsDigit(x)).ToString());

        foreach (char c in password)
        {
            
 
            passwordLetters.Add(new GameObject());
            GameObject background = new GameObject();
            background.AddComponent<Image>();
            background.transform.SetParent(transform, false);
            background.GetComponent<RectTransform>().sizeDelta = new Vector2(65, 75);
            background.GetComponent<Image>().color = new Color32(11, 24, 11, 255);
            passwordLetters[^1].AddComponent<TextMeshProUGUI>();
            passwordLetters[^1].transform.SetParent(background.transform, false);
            TextMeshProUGUI tmp = passwordLetters[^1].GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = myFont;
            passwordLettersTMP.Add(tmp);
        }
    }

    IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();
        myInputField.onDeselect.AddListener(delegate { myInputField.ActivateInputField(); });
        myInputField.onEndEdit.AddListener(delegate { myInputField.ActivateInputField(); });
        myInputField.onValueChanged.AddListener(delegate { EditPassValues(); });
        myInputField.onSubmit.AddListener(delegate { CheckPassword(); });
        myInputField.ActivateInputField();
    }

    void EditPassValues()
    {
        if (Input.GetKey(KeyCode.Return))
            return;
        if (isAwaitingClearing)
        {
            foreach (TextMeshProUGUI c in passwordLettersTMP)
                c.text = "";
            isAwaitingClearing = false;
            
            
        }
        currentLetters = myInputField.text;

        if (myInputField.text.Length > password.Length)
            myInputField.text = myInputField.text.Remove(myInputField.text.Length - 1);
        if (myInputLength > myInputField.text.Length)
            passwordLettersTMP[myInputField.text.Length].text = "";
        else if (myInputField.text.Length > 0)
            passwordLettersTMP[myInputField.text.Length - 1].text = myInputField.text[^1].ToString();

        myInputLength = myInputField.text.Length;

    }



    void CheckPassword()
    {
        currentLetters = "";
        foreach (TextMeshProUGUI c in passwordLettersTMP)
        {
            Regex r = new(@"(?>)" + Regex.Escape(c.text.ToString()));
            currentLetters += c.text;
            
            if (r.Matches(currentLetters).Count <= r.Matches(password).Count)
            {
                bool isInPlace = false;
                foreach (Match m in r.Matches(password))
                {
                    if (m.Index == passwordLettersTMP.IndexOf(c))
                        isInPlace = true;
                }

                if (isInPlace)
                    c.text = "<color=#00FF00FF>" + c.text;
                else
                    c.text = "<color=#FFFF00FF>" + c.text;

            }
            else
                c.text = "<color=#FF0000FF>" + c.text;
        }

        if (password == currentLetters)
        {
            StartCoroutine(Success());
            return;
        }

        currentLetters = "";
        myInputField.ActivateInputField();
        isAwaitingClearing = true;
        myInputField.text = "";
        myInputLength = 0;

    }

    IEnumerator Success()
    {
        myInputField.onSubmit.RemoveAllListeners();
        myInputField.onValueChanged.RemoveAllListeners();
        transform.parent.Find("Legend").gameObject.SetActive(false);
        hackermanText.text = "<color=#009F00FF>I'M IN";
        hackermanText.fontSize = 100;
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        yield return new WaitForSeconds(1);
        bool isKeyPressed = false;
        while(!isKeyPressed)
        {
            if(Input.anyKey)
            {
                isKeyPressed = true;
            }
            yield return null;
        }
        Destroy(transform.parent.gameObject);

    }


    // Update is called once per frame
    void Update()
    {
        myInputField.caretPosition = myInputField.text.Length;
    }
}
