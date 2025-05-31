using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject rebindMenu;
    [SerializeField] private GameObject profilesMenu;
    [SerializeField] private GameObject rebindObjectPrefab;
    [SerializeField] private Transform rebindButtonParent;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform profilesButtonParent;

    private List<GameObject> menuStack = new();
    private PlayerInput input;
    private string currentProfile;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainMenu.SetActive(true);
        rebindMenu.SetActive(false);
        profilesMenu.SetActive(false);

        menuStack.Add(mainMenu);
    }

    private void ToMenu(GameObject menu)
    {
        menuStack[menuStack.Count - 1].SetActive(false);
        menu.SetActive(true);
        menuStack.Add(menu);
    }

    public void OnProfiles()
    {
        LoadProfiles();

        ToMenu(profilesMenu);
    }

    private void LoadProfiles()
    {
        for (int i = profilesButtonParent.childCount - 1; i >= 0; i--)
        {
            Destroy(profilesButtonParent.GetChild(i).gameObject);
        }

        var profiles = SaveDataManager.GetProfiles();
        foreach (var profile in profiles)
        {
            var profileButton = Instantiate(buttonPrefab, profilesButtonParent);
            profileButton.GetComponent<Button>().onClick.AddListener(() => SetCurrentProfile(profile.name));
            profileButton.GetComponent<Button>().onClick.AddListener(OnRebind);
            profileButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = profile.name;
        }


        var defaultButton = Instantiate(buttonPrefab, profilesButtonParent);
        defaultButton.GetComponent<Button>().onClick.AddListener(CreateProfile);
        defaultButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "New Profile";
    }

    private void SetCurrentProfile(string profile)
    {
        currentProfile = profile;
    }

    public void CreateProfile()
    {
        var profiles = SaveDataManager.GetProfiles();
        SaveDataManager.AddProfile($"profile {profiles.Count}", null);

        LoadProfiles();
    }
    public void OnBack()
    {
        menuStack[menuStack.Count - 1].SetActive(false);
        menuStack.RemoveAt(menuStack.Count - 1);
        menuStack[menuStack.Count - 1].SetActive(true);
    }

    public void OnSaveRebind()
    {
        SaveDataManager.UpdateProfile(currentProfile, input.actions.SaveBindingOverridesAsJson());
    }

    public void OnRebind()
    {
        for (int i = rebindButtonParent.childCount - 1; i >= 0; i--)
        {
            Destroy(rebindButtonParent.GetChild(i).gameObject);
        }

        input = FindAnyObjectByType<PlayerInput>();

        foreach (var profile in SaveDataManager.GetProfiles())
        {
            if (profile.name == currentProfile)
            {
                if (!string.IsNullOrEmpty(profile.settings))
                {
                    input.actions.LoadBindingOverridesFromJson(profile.settings);
                }
                else
                {
                    input.actions.RemoveAllBindingOverrides();
                }
                break;
            }
        }


        foreach (var action in input.actions)
        {
            if (action.name == "Any")
                continue;
            if (action.bindings.Count > 2)
            {
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    if (action.bindings[i].isPartOfComposite)
                    {
                        var rebindObject = Instantiate(rebindObjectPrefab, rebindButtonParent).GetComponent<RebindObjectController>();
                        rebindObject.Init(action, i);
                    }
                }
            }
            else
            {
                var rebindObject = Instantiate(rebindObjectPrefab, rebindButtonParent).GetComponent<RebindObjectController>();
                rebindObject.Init(action);
            }

        }

        ToMenu(rebindMenu);
    }

    public void OnResetAll()
    {
        var rebinds = FindObjectsByType<RebindObjectController>(FindObjectsSortMode.None);
        foreach (var rebind in rebinds)
            rebind.OnReset();
    }
}
