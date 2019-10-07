using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : Participant
{
    public static PlayerController instance;

    public PlayerActionResult playerPAR;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SetButtonInteract(false);
        SetStartLetters();
    }

    protected override void SetStartLetters()
    {
        heldLetters = new char[14];
        numCardsHeld = 7;

        heldLetters[0] = 'N';
        heldLetters[1] = 'O';
        heldLetters[2] = 'T';
        heldLetters[3] = 'H';
        heldLetters[4] = 'I';
        heldLetters[5] = 'N';
        heldLetters[6] = 'G';

        string letterStr = "NOTHING";

        for (int i = 7; i < 14; i++)
        {
            heldLetters[i] = '0';
            letterStr += '0';
        }
        //Debug.Log("Player: " + gameObject.name + " has: " + letterStr);
        DisplayLetters();
    }

    public override void BeginTurn()
    {
        base.BeginTurn();
        SetButtonInteract(true);
    }

    public void PlayersTurn()
    {
        GameController.instance.DisplayText("Choose letter to play or draw a card");
        //disable next button, enable draw button and letter buttons.
    }

    public bool GetLetterAtIndex(int index, out char curLetter)
    {
        curLetter = '0';
        if(index >= heldLetters.Length)
        {
            Debug.LogError("Accessing index out of range", gameObject);
            return false;
        }
        else if(heldLetters[index] == '0')
        {
            Debug.LogWarning("No letter at that position", gameObject);
            return false;
        }
        else {
            curLetter = heldLetters[index];
            return true;
        }
    }

    public void OnButtonDrawCard()
    {
        if (GameController.instance.isPrintingText)
        {
            return;
        }

        if (hasDrawnToDeck)
        {
            if (!DrawNewCard())
            {
                Debug.Log("Player drew a new card and busted.");
                GameController.instance.DisplayText(participantName + " drew a new letter and busted.");
                playerPAR = PlayerActionResult.Busted;
            }
            else
            {
                Debug.Log("Player drew a new card to their hand");
                GameController.instance.DisplayText(participantName + " drew a new letter to their hand.");
                playerPAR = PlayerActionResult.DrawnToHand;
            }
        }
        else
        {
            GameController.instance.SetLineup(LetterController.instance.GenerateRandomLetter(), true);
            hasDrawnToDeck = true;
            Debug.Log("Player drew a new card to the deck");
            GameController.instance.DisplayText(participantName + " drew a new letter to the playing field.");
            playerPAR = PlayerActionResult.DrawnToDeck;
        }
    }

    public void OnButtonLetter(int buttonID)
    {
        if (GameController.instance.isPrintingText)
        {
            return;
        }
        if (!GetLetterAtIndex(buttonID, out char curLetter))
        {
            return;
        }

        //GameController.instance.PlayLetter(curLetter);

        if (CanPlayLetter(curLetter))
        {
            PlayLetter(buttonID);
            //playerPAR = PlayerActionResult.PlayedLetter;
            SetButtonInteract(false);
            GameController.instance.playerStepThrough = true;
        }
        else
        {
            GameController.instance.DisplayText("Cannot play " + curLetter + " now. Try another letter or draw a new one.");
        }
    }

    private bool CanPlayLetter(char letterToPlay)
    {
        char[] curLineup = GameController.instance.curLineup;

        for(int i = 0; i < curLineup.Length; i++)
        {
            if(curLineup[i] == letterToPlay)
            {
                //PlayLetter(i);
                return true;
            }
        }

        return false;
    }

    protected override void DisplayLetters()
    {
        for(int i = 0; i < heldLetters.Length; i++)
        {
            if (heldLetters[i] == '0')
            {
                transform.GetChild(i).Find("Text").GetComponent<Text>().text = "[  ]";
            }
            else
            {
                transform.GetChild(i).Find("Text").GetComponent<Text>().text = "[" + heldLetters[i] + "]";
            }
        }
    }

    public void SetButtonInteract(bool interact)
    {
        foreach(Transform but in transform)
        {
            but.GetComponent<Button>().interactable = interact;
        }
    }

    public override void DisplayBustedLetters()
    {
        Debug.Log("PlayerBUsted!");
    }

    public override void AnnouncePlayer()
    {
        GameController.instance.advanceButton.SetActive(false);
        GameController.instance.drawButton.SetActive(true);
        string annStr = participantName + ", it's your turn. Choose letter to play or draw a card.";
        Debug.Log(annStr, gameObject);
        GameController.instance.DisplayText(annStr);
    }

    public void ChoosePlayer()
    {
        //disable advance button, enable player buttons
        GameController.instance.drawButton.SetActive(false);
        GameController.instance.choiceButtons.SetActive(true);

        foreach (Transform child in GameController.instance.choiceButtons.transform)
        {
            child.gameObject.SetActive(false);
        }

        for (int j = 0; j < GameController.instance.gameParticipatns.Count - 1; j++)
        {
            Button curButton = GameController.instance.choiceButtons.transform.GetChild(j).GetComponent<Button>();
            curButton.gameObject.SetActive(true);
            curButton.onClick.RemoveAllListeners();
            int tempInput = j;
            curButton.onClick.AddListener(() => { OnButtonChoice(tempInput); });
            curButton.transform.Find("Text").GetComponent<Text>().text = GameController.instance.gameParticipatns[j].participantName;
        }

        /*
        int i = 0;
        foreach (Transform child in GameController.instance.choiceButtons.transform)
        {
            int tempInput = i;
            child.GetComponent<Button>().onClick.AddListener(() => { OnButtonChoice(tempInput); });
            i++;
        }
        */
    }

    void OnButtonChoice(int id)
    {
        Debug.Log("clicking on id: " + id);
        //playerPAR = PlayerActionResult.PlayedLetter;
        GameController.instance.choiceButtons.SetActive(false);
        
        Participant playerToDraw = GameController.instance.gameParticipatns[id];
        playerToDraw.DrawNewCard();
        string resultStr = "You chose " + playerToDraw.participantName + " to draw a card.";
        GameController.instance.DisplayText(resultStr);
        GameController.instance.advanceButton.SetActive(true);
        GameController.instance.playerStepThrough = true;
        
    }
}
