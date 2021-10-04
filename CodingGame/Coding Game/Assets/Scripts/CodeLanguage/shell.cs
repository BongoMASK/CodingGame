using UnityEngine;
using TMPro;

public class shell : MonoBehaviour
{
    [SerializeField] TMP_InputField userCode;
    [SerializeField] TMP_Text output;

    pseudo pseudo = new pseudo();

    public void ExecuteCode() {
        output.text = "pseudo > ";
        RTResult result = pseudo.Run("<stdin>", userCode.text);

        if (result.error != null)
            output.text = result.error.Display();

        else
            output.text = result.value.Display();
    }
}
