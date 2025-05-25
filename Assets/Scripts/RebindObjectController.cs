using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindObjectController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actionName;
    [SerializeField] private TextMeshProUGUI inputName;

    private InputAction targetAction = null;
    private int bindingIndex;


    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    public void OnRebind()
    {
        targetAction.Disable();

        inputName.color = Color.green;

        rebindingOperation = targetAction.PerformInteractiveRebinding(bindingIndex)
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => CompleteRebind())
            .OnCancel(operation => CompleteRebind())
            .Start();
    }

    public void OnReset()
    {
        targetAction.Disable();
        if (bindingIndex != -1)
            targetAction.RemoveBindingOverride(bindingIndex);
        else
            targetAction.RemoveAllBindingOverrides();
        CompleteRebind();
    }

    private void CompleteRebind()
    {
        if (rebindingOperation != null)
            rebindingOperation.Dispose();
        targetAction.Enable();

        UpdateUI();
    }

    private void UpdateUI()
    {
        inputName.color = Color.white;
        if (bindingIndex == -1)
        {
            actionName.text = targetAction.name;
            inputName.text = targetAction.GetBindingDisplayString();
        }
        else
        {
            string name = targetAction.name + targetAction.bindings[bindingIndex].name.ToUpper();
            switch (name)
            {
                case "MovePOSITIVE":
                    name = "Right";
                    break;
                case "MoveNEGATIVE":
                    name = "Left";
                    break;
                case "AimPOSITIVE":
                    name = "Up";
                    break;
                case "AimNEGATIVE":
                    name = "Down";
                    break;
            }
            actionName.text = name;
            inputName.text = targetAction.GetBindingDisplayString(bindingIndex);
        }
    }

    public void Init(InputAction action, int bindingIndex = -1)
    {
        this.targetAction = action;
        this.bindingIndex = bindingIndex;
        UpdateUI();
    }
}
