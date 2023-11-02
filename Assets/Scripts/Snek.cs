using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Snek : MonoBehaviour
{
    TMP_InputField tmpInput;
    GameObject systemText;
    TextMeshProUGUI display;
    InputOutput inputOutput;
    TextMeshProUGUI scoreCounter;
    Machines machine;
    string map;
    int score, hiscore;
    Vector2 direction = new Vector2(0,0);
    Vector2 input = new Vector2(0,0);
    char gameOverOption;
    int foodPos;

    List<int> positions = new();

    void Awake()
    {
        machine = GameObject.Find("Machines").GetComponent<Machines>();
        tmpInput = GameObject.Find("MyInput").GetComponent<TMP_InputField>();
        systemText = GameObject.Find("TextBlock");
        scoreCounter = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        display = GetComponent<TextMeshProUGUI>();
        inputOutput = GameObject.Find("InputOutput").GetComponent<InputOutput>();
        inputOutput.gameObject.SetActive(false);
        map = display.text;
        tmpInput.enabled = false;
        systemText.SetActive(false);
        score = 0;
        hiscore = int.Parse(machine.activeMachine.fileSys.Directories.Find(x => x.name == "SYS\\").files.Find(x => x.name == "snake").content);
    }
    private void Start()
    {

        foodPos = Random.Range(3,60) + 62 * Random.Range(1,24);
        positions.Add(62 * 12 + 31);
        display.text = display.text.Remove(foodPos, 1);
        display.text = display.text.Insert(foodPos, "*");
        scoreCounter.text = $"  Hi\nScore\n{hiscore.ToString("D5")}\n\n\nScore\n00000";
        InvokeRepeating(nameof(UpdatePosition), 0, 0.1f);
        InvokeRepeating(nameof(Movement), 0, 0.025f);

    }

    void Movement()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        if (inputX > 0)
        {
            if (positions.Count > 1)
            {
                if (positions[0] + 1 != positions[1])
                    direction = Vector2.right;
            }
            else
                direction = Vector2.right;
        }
        else if (inputX < 0)
        {
            if (positions.Count > 1)
            {
                if (positions[0] - 1 != positions[1])
                    direction = Vector2.left;
            }
            else
                direction = Vector2.left;
        }
        else if (inputY > 0)
        {
            if (positions.Count > 1)
            {
                if (positions[0] - 62 != positions[1])
                    direction = Vector2.up;
            }
            else
                direction = Vector2.up;
        }
        else if (inputY < 0)
        {
            if (positions.Count > 1)
            {
                if (positions[0] + 62 != positions[1])
                    direction = Vector2.down;
            }
            else
                direction = Vector2.down;
        }
    }


    void UpdatePosition()
    {
        char i = (char)((int)Random.Range(56, 91));

        if (direction != Vector2.zero && display.text[positions[0] + (int)(direction.x - direction.y * 62)] != ' ' && display.text[positions[0] + (int)(direction.x - direction.y * 62)] != '*')
        {
            StartCoroutine(GameOver());
            return;
        }

        display.text = display.text.Remove(positions[0],1);
        display.text = display.text.Insert(positions[0]," ");

        if(positions.Count > 1)
        {
            display.text = display.text.Remove(positions[^1], 1);
            display.text = display.text.Insert(positions[^1], " ");
            positions[^1] = positions[0];
            display.text = display.text.Remove(positions[^1], 1);
            display.text = display.text.Insert(positions[^1], i.ToString());
            positions.Insert(1,positions[0]);
            positions.RemoveAt(positions.Count-1);
        }

        positions[0] += (int)(direction.x - direction.y * 62);
        display.text = display.text.Remove(positions[0], 1);
        display.text = display.text.Insert(positions[0], i.ToString());

        Food();
    }

    void Food()
    {
        if (positions[0] == foodPos)
        {
            foodPos = Random.Range(3, 60) + 62 * Random.Range(1, 24);
            while (positions.Contains(foodPos))
                foodPos = Random.Range(3, 60) + 62 * Random.Range(1, 24);
            display.text = display.text.Remove(foodPos, 1);
            display.text = display.text.Insert(foodPos, "*");
            Grow();
        }
    }

    void Grow()
    {
        positions.Add(positions[^1] + (int)(-direction.x + direction.y * 62));
        scoreCounter.text = scoreCounter.text.Substring(0, scoreCounter.text.Length - 6) + scoreCounter.text.Substring(scoreCounter.text.Length - 6,6).Replace(score.ToString("D5"),(score+=10).ToString("D5"));
    }

    IEnumerator GameOver()
    {
        CancelInvoke(nameof(UpdatePosition));
        display.text = display.text.Remove(62 * 12 + 27, 9);
        display.text = display.text.Insert(62 * 12 + 27, "<color=\"red\">Game Over</color>");

        display.text = display.text.Remove(62 * 14 + 39, 25);
        display.text = display.text.Insert(62 * 14 + 39, "'Q' to exit, 'R' to retry");

        if (score > hiscore)
            hiscore = score;

        scoreCounter.text = $"  Hi\nScore\n{hiscore.ToString("D5")}\n\n\nScore\n00000";

        yield return AwaitKeyPress();
        if (gameOverOption == 'r')
            Setup();
        else if (gameOverOption == 'q')
        {
            machine.activeMachine.fileSys.Directories.Find(x => x.name == "SYS\\").files.Find(x => x.name == "snake").content = hiscore.ToString();
            display.gameObject.SetActive(false);
            tmpInput.enabled = true;
            systemText.SetActive(true);
            inputOutput.gameObject.SetActive(true);
            inputOutput.myInput.ActivateInputField();
            this.enabled = false;
        }

    }

    IEnumerator AwaitKeyPress()
    {
        bool wasKeyPressed = false;
        while (!wasKeyPressed)
        {
            Debug.Log("wtf");
            if(Input.GetKeyDown(KeyCode.Q))
            {
                gameOverOption = 'q';
                wasKeyPressed = true;
            }
            else if (Input.GetKeyUp(KeyCode.R))
            {
                gameOverOption = 'r';
                wasKeyPressed = true;
            }
            yield return null;

        }

    }

    void Setup()
    {
        score = 0;
        foodPos = Random.Range(3, 60) + 62 * Random.Range(1, 24);
        positions.Clear();
        positions.Add(62 * 12 + 31);
        direction = Vector2.zero;
        display.text = map;
        display.text = display.text.Remove(foodPos, 1);
        display.text = display.text.Insert(foodPos, "*");
        InvokeRepeating(nameof(UpdatePosition), 0, 0.1f);
        InvokeRepeating(nameof(Movement), 0, 0.025f);

    }

    // Update is called once per frame
    void Update()
    {

    }
}
