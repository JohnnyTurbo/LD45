using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Participant : MonoBehaviour
{
    public char[] heldLetters;
    public int numCardsHeld;
    public string participantName;

    protected bool hasDrawnToDeck;

    private void Start()
    {
        SetStartLetters();
    }

    protected virtual void SetStartLetters()
    {
        heldLetters = new char[14];
        numCardsHeld = 7;
        string letterStr = "";
        for(int i = 0; i < numCardsHeld; i++)
        {
            heldLetters[i] = LetterController.instance.GenerateRandomLetter();
            letterStr += heldLetters[i];
        }
        for(int i = numCardsHeld; i < 14; i++)
        {
            heldLetters[i] = '0';
            letterStr += '0';
        }
        //Debug.Log("Player: " + gameObject.name + " has: " + letterStr);

        DisplayLetters();
    }

    public virtual void BeginTurn()
    {
        hasDrawnToDeck = false;
    }

    public virtual PlayerActionResult TryAction()
    {
        if (CheckForLetterToPlay())
        {
            return PlayerActionResult.PlayedLetter;
        }
        else
        {
            if (hasDrawnToDeck)
            {
                if (!DrawNewCard())
                {
                    Debug.Log("Player drew a new card and busted.");
                    GameController.instance.DisplayText(participantName + " drew a new letter and busted.");
                    return PlayerActionResult.Busted;
                }
                else
                {
                    Debug.Log("Player drew a new card to their hand");
                    GameController.instance.DisplayText(participantName + " drew a new letter to their hand.");
                    return PlayerActionResult.DrawnToHand;
                }
            }
            else
            {
                GameController.instance.SetLineup(LetterController.instance.GenerateRandomLetter(), true);
                hasDrawnToDeck = true;
                Debug.Log("Player drew a new card to the deck");
                GameController.instance.DisplayText(participantName + " drew a new letter to the playing field.");
                return PlayerActionResult.DrawnToDeck;
            }
        }
    }

    /*
    public virtual void BeginTurn()
    {
        StartCoroutine(StepThroughTurn());
    }

    IEnumerator StepThroughTurn()
    {
        Debug.Log(participantName + " is up.");
        hasDrawnToDeck = false;

        yield return new WaitUntil(() => stepThrough == true);
        stepThrough = false;



        while (!CheckForLetterToPlay())
        {
            Debug.Log(participantName + " has no cards to play");
            if (!hasDrawnToDeck)
            {
                Debug.Log("drawing to deck");
                GameController.instance.SetLineup();
                hasDrawnToDeck = true;
            }
            else
            {
                Debug.Log("Drawing card");
                if (!DrawNewCard())
                {
                    //return;
                    StopCoroutine(StepThroughTurn());
                }
            }
        }
    }
    */

    public bool DrawNewCard()
    {
        if(numCardsHeld == 14)
        {
            //Debug.Log("Player " + participantName + " Busted!");
            //DisplayBustedLetters();
            //GameController.instance.EliminatePlayer(this);
            return false;
        }

        heldLetters[numCardsHeld] = LetterController.instance.GenerateRandomLetter();
        numCardsHeld++;

        //DebugCurLetters();
        DisplayLetters();
        return true;
    }

    private void DebugCurLetters()
    {
        string letterStr = "";
        for (int i = 0; i < heldLetters.Length; i++)
        {
            letterStr += heldLetters[i];
        }
        Debug.Log("Player: " + gameObject.name + " has: " + letterStr);
    }

    private bool CheckForLetterToPlay()
    {
        //Debug.Log("check 4 letter 2 play");
        char[] curLineup = GameController.instance.curLineup;
        
        for(int i = 0; i < heldLetters.Length; i++)
        {
            if(curLineup[2] == heldLetters[i])
            {
                //Debug.Log("Same as it ever was");
                PlayLetter(i);
                return true;
            }
        }

        for (int i = 0; i < heldLetters.Length; i++)
        {
            if (curLineup[1] == heldLetters[i] || curLineup[3] == heldLetters[i])
            {
                PlayLetter(i);
                return true;
            }
        }

        for (int i = 0; i < heldLetters.Length; i++)
        {
            if (curLineup[0] == heldLetters[i] || curLineup[4] == heldLetters[i])
            {
                PlayLetter(i);
                return true;
            }
        }

        return false;
    }

    protected void PlayLetter(int letterToPlayID)
    {
        //Debug.Log("Playletter");
        GameController.instance.PlayLetter(heldLetters[letterToPlayID]);

        for (int i = letterToPlayID; i < heldLetters.Length - 1; i++)
        {
            heldLetters[i] = heldLetters[i + 1];
        }
        heldLetters[heldLetters.Length - 1] = '0';
        numCardsHeld--;

        if(numCardsHeld <= 0)
        {
            string goString = participantName + " won the game because they played all their cards.";
            GameController.instance.ShowGameOverPanel(goString);
        }

        //DebugCurLetters();
        DisplayLetters();
    }

    protected virtual void DisplayLetters()
    {
        //Debug.Log("displaying letters");
        Text lettersText = transform.Find("HeldLetters").GetComponent<Text>();

        string displayStr = "";

        for(int i = 0; i < heldLetters.Length; i++)
        {
            if(i == 7)
            {
                displayStr += "\n";
            }
            if(heldLetters[i] == '0')
            {
                displayStr += "[ ]";
            }
            else
            {
                //displayStr += ("[" + heldLetters[i] + "]");
                displayStr += ("[.]");
            }
        }

        lettersText.text = displayStr;
    }

    public virtual void DisplayBustedLetters()
    {
        string bustedStr = "[B][U][S][T][E][D][!]\n[B][U][S][T][E][D][!]";
        Text lettersText = transform.Find("HeldLetters").GetComponent<Text>();
        lettersText.text = bustedStr;
    }

    public virtual void AnnouncePlayer()
    {
        string annStr = "It's " + participantName + "'s turn and they have " + numCardsHeld + " letters in hand.";
        Debug.Log(annStr, gameObject);
        GameController.instance.DisplayText(annStr);
    }
}
