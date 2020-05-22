using UnityEngine;
using System.Collections;
using KModkit;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;

public class KritFlipTheCoin : MonoBehaviour
{
    public KMSelectable CoinBtn;
    public KMSelectable SubmitBtn;

    public KMBombInfo BombInfo;

    public TextMesh DigitTxt;

    public Renderer CoinRender;
    public Texture FiftyCentHeadsTexture, FiftyCentTailsTexture;
    public Texture OneEuroHeadsTexture, OneEuroTailsTexture;
    public Texture TwoEuroHeadsTexture, TwoEuroTailsTexture;

    public KMAudio CoinToss;

    static int moduleIdCounter = 1;
    int moduleId;
    int StageNr;
    int CoinGen;
    int RandomSideGen;
    int Digit;

    public string CoinSide;
    public string Coin;
    string DesiredSide;
    string Value; //For logging purposes

    bool Flipped;
    bool Active;

    IEnumerator ProcessTwitchCommand(string Command)
    {
        if (Regex.IsMatch(Command, "^flip submit"))
        {
            yield return null;
            yield return new[] { CoinBtn };
            yield return new WaitForSecondsRealtime(0.01f);
            yield return null;
            yield return new[] { SubmitBtn };
        }
        else if (Regex.IsMatch(Command, "^doubleflip submit"))
        {
            yield return null;
            yield return new[] { CoinBtn };
            yield return new WaitForSecondsRealtime(0.01f);
            yield return null;
            yield return new[] { CoinBtn };
            yield return new WaitForSecondsRealtime(0.01f);
            yield return null;
            yield return new[] { SubmitBtn };
        }
        else
        {
            yield return "sendtochaterror Command \"" + Command + "\" is invalid";
        }
    }

    private readonly string TwitchHelpMessage = "To flip the coin once and submit that side, use \"!{0} flip submit\". To submit the current side, use \"!{0} doubleflip submit\" to flip twice, then submit.";

    void Awake()
    {
        moduleId = moduleIdCounter++;
        GetComponent<KMNeedyModule>().OnNeedyActivation += OnNeedyActivation;
        GetComponent<KMNeedyModule>().OnNeedyDeactivation += OnNeedyDeactivation;
        CoinBtn.OnInteract = CoinFlip;
        SubmitBtn.OnInteract = Deactivate;
        GetComponent<KMNeedyModule>().OnTimerExpired += OnTimerExpired;
        CoinGen = Random.Range(0, 3);
        switch (CoinGen)
        {
            case 1:
                {
                    Coin = "50C";
                    Value = "¢50";
                    CoinRender.material.mainTexture = FiftyCentHeadsTexture;
                    break;
                }
            case 2:
                {
                    Coin = "1E";
                    Value = "€1";
                    CoinRender.material.mainTexture = OneEuroHeadsTexture;
                    break;
                }
            default:
                {
                    CoinRender.material.mainTexture = TwoEuroHeadsTexture;
                    Coin = "2E";
                    Value = "€2";
                    break;
                }
        }
        CoinSide = "Heads";
        Debug.LogFormat("[Flip The Coin #{0}] The coin is {1}", moduleId, Value);
    }

    void Init()
    {
        RandomSideGen = Random.Range(0, 50);
        if (Coin == "50C")
        {
            if (RandomSideGen < 25)
            {
                //Tail side
                CoinSide = "Tails";
                CoinRender.material.mainTexture = FiftyCentTailsTexture;
            }
            else
            {
                //Heads side
                CoinSide = "Heads";
                CoinRender.material.mainTexture = FiftyCentHeadsTexture;
            }
        }
        else if (Coin == "1E")
        {
            if (RandomSideGen < 25)
            {
                //Tail side
                CoinSide = "Tails";
                CoinRender.material.mainTexture = OneEuroTailsTexture;
            }
            else
            {
                //Heads side
                CoinSide = "Heads";
                CoinRender.material.mainTexture = OneEuroHeadsTexture;
            }
        }
        else if (Coin == "2E")
        {
            if (RandomSideGen < 25)
            {
                //Tail side
                CoinSide = "Tails";
                CoinRender.material.mainTexture = TwoEuroTailsTexture;
            }
            else
            {
                //Heads side
                CoinSide = "Heads";
                CoinRender.material.mainTexture = TwoEuroHeadsTexture;
            }
        }
        
        Debug.LogFormat("[Flip The Coin #{0}] The current side for stage {1} is {2}", moduleId, StageNr, CoinSide);

        Digit = Random.Range(0, 10);
        DigitTxt.text = Digit.ToString();
        Debug.LogFormat("[Flip The Coin #{0}] The digit for stage {1} is {2}", moduleId, StageNr, Digit);
        GenerateDesiredSide();
    }

    void GenerateDesiredSide()
    {
        if (Coin == "50C")
        {
            //Rules for the 50 cents coin: The Serial Number.
            if (Digit > BombInfo.GetSerialNumberNumbers().Last())
            {
                DesiredSide = "Heads";
            }
            else
            {
                DesiredSide = "Tails";
            }
        }
        else if (Coin == "1E")
        {
            //Rules for the 1 euro coin: The batteries.
            if (Digit > BombInfo.GetBatteryCount())
            {
                DesiredSide = "Heads";
            }
            else
            {
                DesiredSide = "Tails";
            }
        }
        else if (Coin == "2E")
        {
            //Rules for the 2 euro coin: The indicators.
            if (Digit > BombInfo.GetOnIndicators().Count() - BombInfo.GetOffIndicators().Count())
            {
                
                DesiredSide = "Heads";
            }
            else
            {
                DesiredSide = "Tails";
            }
        }
        Debug.LogFormat("[Flip The Coin #{0}] The desired side for stage {1} is {2}", moduleId, StageNr, DesiredSide);
    }

    protected bool CoinFlip()
    {
        int FlipSound;
        GetComponent<KMSelectable>().AddInteractionPunch();
        if (Active)
        {
            Flipped = true;
        }
        if (Coin == "50C")
        {
            if (CoinSide == "Heads")
            {
                //Changing to Tail side
                CoinSide = "Tails";
                CoinRender.material.mainTexture = FiftyCentTailsTexture;
            }
            else if (CoinSide == "Tails")
            {
                //Changing to Heads side
                CoinSide = "Heads";
                CoinRender.material.mainTexture = FiftyCentHeadsTexture;
            }
        }
        else if (Coin == "1E")
        {
            {
                if (CoinSide == "Heads")
                {
                    //Changing to Tail side
                    CoinSide = "Tails";
                    CoinRender.material.mainTexture = OneEuroTailsTexture;
                }
                else if (CoinSide == "Tails")
                {
                    //Changing to Heads side
                    CoinSide = "Heads";
                    CoinRender.material.mainTexture = OneEuroHeadsTexture;
                }
            }
        }
        else if (Coin == "2E")
        {
            if (CoinSide == "Heads")
            {
                //Changing to Tail side
                CoinSide = "Tails";
                CoinRender.material.mainTexture = TwoEuroTailsTexture;
            }
            else if (CoinSide == "Tails")
            {
                //Changing to Heads side
                CoinSide = "Heads";
                CoinRender.material.mainTexture = TwoEuroHeadsTexture;
            }
        }

        FlipSound = Random.Range(0, 3);
        switch (FlipSound)
        {
            case 1:
                {
                    CoinToss.PlaySoundAtTransform("CoinToss2", transform);
                    break;
                }
            case 2:
                {
                    CoinToss.PlaySoundAtTransform("CoinToss3", transform);
                    break;
                }
            default:
                {
                    CoinToss.PlaySoundAtTransform("CoinToss", transform);
                    break;
                }
        }
        return false;
    }

    protected bool Submit()
    {
        SubmitBtn.OnInteract = Deactivate;
        Debug.LogFormat("[Flip The Coin #{0}] Submitted at stage {1} is {2}, desired is {3}", moduleId, StageNr, CoinSide, DesiredSide);
        GetComponent<KMSelectable>().AddInteractionPunch();
        if (Flipped)
        {
            if (CoinSide == DesiredSide)
            {
                Active = false;
                GetComponent<KMNeedyModule>().OnPass();
                Debug.LogFormat("[Flip The Coin #{0}] Stage {1} was correct.", moduleId, StageNr);
            }
            else
            {
                GetComponent<KMNeedyModule>().OnStrike();
                Debug.LogFormat("[Flip The Coin #{0}] Stage {1} was incorrect.", moduleId, StageNr);
                GetComponent<KMNeedyModule>().OnPass();
            }
        }
        else
        {
            GetComponent<KMNeedyModule>().OnStrike();
            Debug.LogFormat("[Flip The Coin #{0}] Didn't flip the coin at stage {1}!", moduleId, StageNr);
            GetComponent<KMNeedyModule>().OnPass();
        }
        Active = false;
        return false;
    }

    protected bool Deactivate()
    {
        return false;
    }

    protected void OnNeedyActivation()
    {
        Active = true;
        Flipped = false;
        SubmitBtn.OnInteract += Submit;
        StageNr++;
        Debug.LogFormat("[Flip The Coin #{0}] Module Activated", moduleId);
        Init();
    }

    protected void OnNeedyDeactivation()
    {
    
    }

    protected void OnTimerExpired()
    {
        GetComponent<KMNeedyModule>().OnStrike();
        SubmitBtn.OnInteract = Deactivate;
    }
}