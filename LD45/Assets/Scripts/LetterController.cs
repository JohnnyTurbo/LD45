using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterController : MonoBehaviour
{
    private static LetterController _instance;
    public static LetterController instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        GenerateRandomLetter();
    }

    public char GenerateRandomLetter()
    {
        char randLetter = (char)('A' + Random.Range(0, 26));
        //Debug.Log("rand letter: " + randLetter);
        return randLetter;
    }

    public char GetNextLetter(char curLetter)
    {
        if(curLetter == 'Z')
        {
            return 'A';
        }
        else
        {
            return (char)(curLetter + 1);
        }
    }

    public char GetPrevLetter(char curLetter)
    {
        if(curLetter == 'A')
        {
            return 'Z';
        }
        else
        {
            return (char)(curLetter - 1);
        }
    }

    public char[] GetLineup(char baseLetter)
    {
        char[] lineup = new char[5];

        lineup[2] = baseLetter;
        lineup[1] = GetPrevLetter(lineup[2]);
        lineup[0] = GetPrevLetter(lineup[1]);
        lineup[3] = GetNextLetter(lineup[2]);
        lineup[4] = GetNextLetter(lineup[3]);

        return lineup;
    }
}
