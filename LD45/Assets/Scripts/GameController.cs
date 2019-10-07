//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public enum PlayerActionResult { PlayedLetter, DrawnToDeck, DrawnToHand, Busted, PickOne }

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public List<Participant> gameParticipatns;

    public Text curLetters;
    public Text textPrintout;
    public Text gameOverMessage;

    public char[] curLineup;
    public GameObject choiceButtons;
    public GameObject advanceButton;
    public GameObject drawButton;
    public GameObject startPanel, swPanel, withText, gameOverPanel, jammer, otherPlayers, printoutPanel, rulesPanel, rulesButton;
    public bool playerStepThrough;

    private PlayerActionResult curPAR;
    private List<Participant> curPlayersToElim;
    private Participant curParticipant;
    private bool stepThrough;
    public bool isPrintingText { get; private set; }
    private int curParticipantID;

    public void OnButtonStartButton()
    {
        startPanel.SetActive(false);
        swPanel.SetActive(true);
        StartCoroutine(BeginIntroSequence());
    }

    IEnumerator BeginIntroSequence()
    {
        yield return new WaitForSeconds(1f);
        withText.SetActive(true);
        yield return new WaitForSeconds(1f);
        jammer.SetActive(true);
        yield return new WaitForSeconds(3f);
        swPanel.SetActive(false);
        otherPlayers.SetActive(true);
        printoutPanel.SetActive(true);
        curLetters.gameObject.SetActive(true);
        rulesButton.SetActive(true);
        yield return null;
        BeginGame();
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        curLineup = new char[5] { '0', '0', '0', '0', '0' };
        stepThrough = false;
        curPlayersToElim = new List<Participant>();
        choiceButtons.SetActive(false);
        drawButton.SetActive(false);
        //BeginGame();
    }

    /*
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            BeginGame();
        }

        if (Input.GetKeyDown(KeyCode.F12))
        {
            stepThrough = true;
        }
    }
    */

    private void BeginGame()
    {
        //curLineup = LetterController.instance.GetLineup(LetterController.instance.GenerateRandomLetter());
        //curLetters.text = (curLineup[0] + " - " + curLineup[1] + " - [" + curLineup[2] + "] - " + curLineup[3] + " - " + curLineup[4]);

        SetLineup(LetterController.instance.GenerateRandomLetter(), true);

        curParticipantID = 0;
        curParticipant = gameParticipatns[curParticipantID];
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        

        while (gameParticipatns.Count > 1)
        {
            //Debug.Log("It's " + curParticipant.participantName + "'s turn.");
            curParticipant.AnnouncePlayer();
            curParticipant.BeginTurn();

            

            if (curParticipant == PlayerController.instance)
            {
                bool playerTurn = true;

                while (playerTurn)
                {
                    //PlayerController.instance.PlayersTurn();
                    yield return new WaitUntil(() => playerStepThrough == true);
                    playerStepThrough = false;

                    switch (PlayerController.instance.playerPAR)
                    {
                        case PlayerActionResult.Busted:
                            curPlayersToElim.Add(curParticipant);
                            playerTurn = false;
                            break;

                        case PlayerActionResult.DrawnToDeck:

                            break;

                        case PlayerActionResult.DrawnToHand:

                            break;

                        case PlayerActionResult.PlayedLetter:
                            playerTurn = false;
                            break;

                        case PlayerActionResult.PickOne:
                            PlayerController.instance.ChoosePlayer();
                            yield return new WaitUntil(() => playerStepThrough == true);
                            playerStepThrough = false;
                            playerTurn = false;
                            break;
                    }
                }
                advanceButton.SetActive(true);
                drawButton.SetActive(false);
            }
            else
            {
                yield return new WaitUntil(() => stepThrough == true);
                stepThrough = false;

                curPAR = curParticipant.TryAction();

                while (curPAR != PlayerActionResult.PlayedLetter)
                {
                    //Debug.Log("action failed.");

                    if (curPAR == PlayerActionResult.Busted)
                    {
                        curPlayersToElim.Add(curParticipant);
                        break;
                        //EliminatePlayer(curParticipant);
                        //yield break;
                    }

                    yield return new WaitUntil(() => stepThrough == true);
                    stepThrough = false;

                    curPAR = curParticipant.TryAction();
                }
            }

            yield return new WaitUntil(() => stepThrough == true);
            stepThrough = false;

            for (int i = 0; i < curPlayersToElim.Count; i++)
            {
                EliminatePlayer(curPlayersToElim[i]);
                yield return new WaitUntil(() => stepThrough == true);
                stepThrough = false;
            }

            curPlayersToElim.Clear();

            curParticipantID = (curParticipantID + 1) % gameParticipatns.Count;
            curParticipant = gameParticipatns[curParticipantID];          
        }

        ShowGameOverPanel(gameParticipatns[0].participantName + " won the game because all other players were eliminated.");

    }

    public void EliminatePlayer(Participant playerToElim)
    {
        //StopCoroutine(GameLoop());
        gameParticipatns.Remove(playerToElim);
        playerToElim.DisplayBustedLetters();
        if(curParticipantID == gameParticipatns.Count) { curParticipantID = 0; }
        curParticipant = gameParticipatns[curParticipantID];
        Debug.Log(playerToElim.participantName + " was eliminated. There are now " + gameParticipatns.Count + " in game." +
                  " And it is now " + curParticipant.participantName + "'s turn.");
        DisplayText(playerToElim.participantName + " was eliminated from the game.");
        //StartCoroutine(GameLoop());
    }

    public void SetLineup(char baseLetter, bool checkForDupe)
    {
        char[] newLineup = LetterController.instance.GetLineup(baseLetter);
        while(checkForDupe && newLineup[0] == curLineup[0])
        {
            Debug.Log("dupe");
            newLineup = LetterController.instance.GetLineup(baseLetter);
        }
        curLineup = newLineup;
        curLetters.text = (curLineup[0] + " - " + curLineup[1] + " - [" + curLineup[2] + "] - " + curLineup[3] + " - " + curLineup[4]);
    }

    public void PlayLetter(char letterToPlay)
    {
        string resultStr = curParticipant.participantName + " played letter: " + letterToPlay;
        if (letterToPlay == curLineup[2])
        {
            if(curParticipant == PlayerController.instance)
            {
                PlayerController.instance.playerPAR = PlayerActionResult.PlayedLetter;
            }

            Debug.Log("Player played: " + letterToPlay + " and " + GiveCardToAllPlayers());
            resultStr += " and everyone else draws a new letter.";
            //Debug.Log("Everyone else gets a new letter");
            //GiveCardToAllPlayers();
        }
        else if (letterToPlay == curLineup[1] || letterToPlay == curLineup[3])
        {
            if (curParticipant == PlayerController.instance)
            {
                PlayerController.instance.playerPAR = PlayerActionResult.PickOne;
                resultStr += ". Now choose a player to draw a card.";
            }
            else
            {
                resultStr += (". " + ChoosePlayerToGetCard());
                //Debug.Log("Player played: " + letterToPlay + " and " + ChoosePlayerToGetCard());
                //resultStr += " and they choose someone to get a card";               
                //Debug.Log("Pick one player to get a letter!");
                //ChoosePlayerToGetCard();
            }
        }
        else if (letterToPlay == curLineup[0] || letterToPlay == curLineup[4])
        {
            if (curParticipant == PlayerController.instance)
            {
                PlayerController.instance.playerPAR = PlayerActionResult.PlayedLetter;
            }

            Debug.Log("Player played: " + letterToPlay + " and no one gets a letter");
            resultStr += " but no one draws a new card.";
            //Debug.Log("No one gets a letter");
        }
        DisplayText(resultStr);
        SetLineup(letterToPlay, false);
    }

    private string GiveCardToAllPlayers()
    {
        //List<Participant> playersToElim = new List<Participant>();

        foreach(Participant gamer in gameParticipatns)
        {
            if(gamer == curParticipant)
            {
                continue;
            }
            if (!gamer.DrawNewCard())
            {
                //playersToElim.Add(gamer);
                curPlayersToElim.Add(gamer);
            }
        }

        string resultStr = ("EVERY BODY GETS A CARD! except " + curParticipant.participantName);

        /*
        foreach(Participant eliminatedPlayer in playersToElim)
        {
            EliminatePlayer(eliminatedPlayer);
        }
        */

        return resultStr;
    }

    private string ChoosePlayerToGetCard()
    {
        string resultStr;

        int lowestCards = 15;
        List<Participant> lowestPlayers = new List<Participant>();

        foreach (Participant gamer in gameParticipatns)
        {
            if (gamer == curParticipant)
            {
                continue;
            }
            if (gamer.numCardsHeld == lowestCards)
            {
                lowestPlayers.Add(gamer);
            }
            else if (gamer.numCardsHeld < lowestCards)
            {
                lowestPlayers.Clear();
                lowestPlayers.Add(gamer);
                lowestCards = gamer.numCardsHeld;
            }
        }

        int randomIndex = Random.Range(0, lowestPlayers.Count);


        resultStr = (curParticipant.name + " chose " + lowestPlayers[randomIndex].participantName + " to draw a card.");

        if (!lowestPlayers[randomIndex].DrawNewCard())
        {
            //EliminatePlayer(lowestPlayers[randomIndex]);
            curPlayersToElim.Add(lowestPlayers[randomIndex]);
        }

        return resultStr;

        /*
        string resultStr;

        if(curParticipant == PlayerController.instance)
        {
            //choose player to get card
            resultStr = ("LDJammer chose " + gameParticipatns[0].participantName + " to draw yung card.");
            if (!gameParticipatns[0].DrawNewCard())
            {
                //EliminatePlayer(gameParticipatns[0]);
                curPlayersToElim.Add(gameParticipatns[0]);
            }
        }
        else
        {
            int lowestCards = 15;
            List<Participant> lowestPlayers = new List<Participant>();

            foreach(Participant gamer in gameParticipatns)
            {
                if(gamer == curParticipant)
                {
                    continue;
                }
                if(gamer.numCardsHeld == lowestCards)
                {
                    lowestPlayers.Add(gamer);
                }
                else if(gamer.numCardsHeld < lowestCards)
                {
                    lowestPlayers.Clear();
                    lowestPlayers.Add(gamer);
                }
            }

            int randomIndex = Random.Range(0, lowestPlayers.Count);
            

            resultStr = (curParticipant.name + " chose " + lowestPlayers[randomIndex].participantName + " to draw a card.");

            if (!lowestPlayers[randomIndex].DrawNewCard())
            {
                //EliminatePlayer(lowestPlayers[randomIndex]);
                curPlayersToElim.Add(lowestPlayers[randomIndex]);
            }
        }

        return resultStr;
        */
    }

    public void OnButtonAdvance()
    {
        if (isPrintingText) { return; }
        stepThrough = true;
    }

    public void DisplayText(string textToDisplay)
    {
        isPrintingText = true;
        StartCoroutine(DisplayTextCoro(textToDisplay));
    }

    private IEnumerator DisplayTextCoro(string textToDisplay)
    {
        textPrintout.text = "";

        for(int i = 0; i < textToDisplay.Length; i++)
        {
            textPrintout.text += textToDisplay[i];
            yield return null;
        }
        isPrintingText = false;
    }

    public void ShowGameOverPanel(string textToDisplay)
    {
        gameOverPanel.SetActive(true);
        gameOverMessage.text = textToDisplay;
    }

    public void OnButtonTryAgain()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OnButtonShowRules()
    {
        rulesPanel.SetActive(true);
    }

    public void OnButtonReturn()
    {
        rulesPanel.SetActive(false);
    }
}
