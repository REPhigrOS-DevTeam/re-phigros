using System.Text.RegularExpressions;
using Network.Verify.API;
using UnityEngine;
using UnityEngine.UI;

public class TempRepAccountInfoSetter : MonoBehaviour
{
    public InputField ifUsername, ifVerifyToken;

    private void Awake()
    {
        ifUsername.onEndEdit.AddListener(CheckUsername);
        ResetRep();
    }

    public void ResetRep()
    {
        ifUsername.text = RepAPI.Username;
        ifVerifyToken.text = RepAPI.VerifyToken;
    }

    public void SetRep()
    {
        RepAPI.Username = ifUsername.text;
        RepAPI.VerifyToken = ifVerifyToken.text;
        RepAPI.SaveUsernameAndToken();
    }
    private static readonly Regex usernameRegex = new Regex("[^a-zA-Z0-9]");
    private void CheckUsername(string input)
    {
        if (usernameRegex.IsMatch(input) || string.IsNullOrEmpty(input))
        {
            ifUsername.text = RepAPI.Username;
        }
        else
        {
            RepAPI.Username = input;
            RepAPI.SaveUsernameAndToken();
        }
    }
}