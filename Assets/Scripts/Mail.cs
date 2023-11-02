using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Text.RegularExpressions;

public class Mail : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI display;
    TMP_InputField inputField;
    Machines machines;
    string path,toLog;
    bool isViewingMail;
    FileSystem.File mail;
    private void Awake()
    {

        
        machines = GameObject.Find("Machines").GetComponent<Machines>();
        display = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        inputField = transform.Find("Input").GetComponent<TMP_InputField>();



    }

    void Start()
    {

        inputField.ActivateInputField();

        inputField.onDeselect.AddListener(delegate { inputField.ActivateInputField(); } );
        inputField.onEndEdit.AddListener(delegate { inputField.ActivateInputField(); } );
        inputField.onSubmit.AddListener(delegate { CheckID(); } );
        StartCoroutine(Instructions());
    }

    IEnumerator Instructions()
    {

        display.text = "\n\nTo navigate, simply type in the ID number (without the '#') of a mail You wish to read, to fall back, type in 'Q', to write a mail, type in 'W', to log a mail to your machine, open it, then type in 'L'\n\n\t\tPress RETURN to continue";
        while(!Input.GetKeyDown(KeyCode.Return))
        {
            yield return null;
        }
        MainView();
    }

    void MainView()
    {
        inputField.text = "";
        isViewingMail = false;
        Regex ID = new Regex(@"(?<=ID: ).*?(?=(\\t))");
        Regex FROM = new Regex(@"(?<=From: ).*?(?=(\\n))");
        Regex SUBJECT = new Regex(@"(?<=Subject: ).*?(?=(\\n))");
        display.text = "To navigate, simply type in the ID number (without the '#') of a mail You wish to read, to fall back, type in 'Q', to write a mail, type in 'W', to log a mail to your machine, open it, then type in 'L'\n\n";

        foreach (FileSystem.File mail in machines.localMachine.fileSys.Directories.Find(x => x.path == "" && x.name == "Mail\\").files)
        {
            display.text += $"ID: {ID.Match(mail.content).Value}\t\tFrom: {FROM.Match(mail.content)}\n{(mail.content.Split("<UNREAD>").Length > 1 ? "<color=#AAAA00FF><UNREAD></color>" : "<color=#00AA00FF><READ></color>")}\nSubject: {SUBJECT.Match(mail.content)}\n<mark=#000020FF>s\t\t\t\t\t\t\t</mark>";
        }
    }

    void Log()
    {
        inputField.text = "";
        display.text = "\n\n\t\tThe mail has been logged to your machine";
        toLog = "";
        Regex ID = new Regex(@"(?<=ID: ).*?(?=(\\t))");
        if (machines.localMachine.fileSys.activeDirectory.files.Exists(x => x.name == "LOG" + ID.Match(mail.content).Value))
            return;
        Regex FROM = new Regex(@"(?<=From: ).*?(?=(\\n))");
        Regex SUBJECT = new Regex(@"(?<=Subject: ).*?(?=(\\n))");
        toLog += $"ID: {ID.Match(mail.content).Value}\nFrom: {FROM.Match(mail.content)}\nSubject: " + mail.content.Split("Subject: ")[1]; 
        machines.localMachine.fileSys.NewFile("LOG" + ID.Match(mail.content).Value, "txt", machines.localMachine.fileSys.activeDirectory.path + machines.localMachine.fileSys.activeDirectory.name, toLog, false);

    }

    void CheckID()
    {
        if (isViewingMail)
        {

            if (inputField.text.ToLower() == "q")
                MainView();
            else if (inputField.text.ToLower() == "l")
                Log();

            return;
        }
        else if (machines.localMachine.fileSys.Directories.Find(x => x.path == "" && x.name == "Mail\\").files.Exists(x => x.name.Trim('\\') == inputField.text.Trim('#')))
        {
            mail = machines.localMachine.fileSys.Directories.Find(x => x.path == "" && x.name == "Mail\\").files.Find(x => x.name.Trim('\\') == inputField.text.Trim('#'));
            mail.content = mail.content.Replace("<UNREAD>", "");
            display.text = mail.getRichContent();

            inputField.text = "";
            isViewingMail = true;
            return;
        }
        else if (inputField.text == "w")
        {
            display.text = "\n\n\t\tService down for maintenance\n\t\tSorry for the inconvinience";
            isViewingMail = true;
        }
        else if (!isViewingMail && inputField.text == "q")
        {
            GameObject.Find("MyInput").GetComponent<TMP_InputField>().ActivateInputField();
            GameObject.Find("InputOutput").GetComponent<InputOutput>().Cls();
            Destroy(gameObject);
        }
        inputField.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
