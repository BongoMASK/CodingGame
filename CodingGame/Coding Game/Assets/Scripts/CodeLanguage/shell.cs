using UnityEngine;
using TMPro;

public class shell : MonoBehaviour
{
    [SerializeField] TMP_InputField userCode;
    [SerializeField] TMP_Text output;

    pseudo pseudo = new pseudo();

    public void ExecuteCode() {
        output.text = "pseudo > ";
        TokenError tokenError = pseudo.Run(userCode.text);

        if (tokenError.error != null)
            output.text = tokenError.error;

        else
            output.text = tokenError.result;

        Debug.Log(tokenError.result);
    }
}
