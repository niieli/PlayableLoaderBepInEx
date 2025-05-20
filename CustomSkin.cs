using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BepInEx;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PlayableLoaderBepInEx;

public class CustomSkin : MonoBehaviour
{
    private Button customSkinButton, refreshButton, folderButton;
    public static GameObject customSkinPanel, tempButton;
    private static TextMeshProUGUI currentPlayable, currentPlayableShadow;
    private static TextMeshProUGUI currentSelectedText, currentSelectedTextShadow;
    private static TextMeshProUGUI currentPlayableTitle, currentPlayableTitleShadow;
    private static Toggle enableToggle;
    private static readonly List<GameObject> Buttons = new();
    private GridLayoutGroup layoutGroup;
    private int previousButtonCount = -1;
    void Start()
    {
        tempButton = transform.Find("TempButton").gameObject;
        customSkinButton = transform.Find("Button").GetComponent<Button>();
        customSkinPanel = transform.Find("Panel").gameObject;
        refreshButton = customSkinPanel.transform.Find("RefreshButton").GetComponent<Button>();
        folderButton = customSkinPanel.transform.Find("OpenFilesButton").GetComponent<Button>();
        customSkinPanel.SetActive(false);
        currentPlayable = customSkinPanel.transform.Find("Current").GetComponent<TextMeshProUGUI>();
        currentPlayableShadow = customSkinPanel.transform.Find("CurrentShadow").GetComponent<TextMeshProUGUI>();
        enableToggle = customSkinPanel.transform.Find("EnableToggle").GetComponent<Toggle>();
        enableToggle.isOn = PlayableLoaderPlugin.IsEnabled;
        enableToggle.onValueChanged.AddListener(EnableToggle());
        customSkinButton.onClick.AddListener(TogglePanel);
        refreshButton.onClick.AddListener(OnRefresh);
        folderButton.onClick.AddListener(OpenFolder);
        currentPlayableTitle = customSkinPanel.transform.Find("CurrentPlayable").GetComponent<TextMeshProUGUI>();
        currentPlayableTitleShadow = customSkinPanel.transform.Find("CurrentPlayableShadow").GetComponent<TextMeshProUGUI>();
        layoutGroup = customSkinPanel.GetComponent<GridLayoutGroup>();
        UpdateLayout();
    }

    void Update()
    {
        if (!enableToggle.isOn)
        {
            var textShadow = Buttons[0].transform.Find("PlayableNameShadow").GetComponent<TextMeshProUGUI>();
            var textNormal = Buttons[0].transform.Find("PlayableName").GetComponent<TextMeshProUGUI>();
            currentSelectedTextShadow.color = new Color(0.42f, 0.09f,0.584f);
            currentSelectedText.color = Color.white;
            currentSelectedTextShadow = textShadow;
            currentSelectedText = textNormal;
            currentSelectedTextShadow.color = new Color(0f, 0.28f, 0.28f);
            currentSelectedText.color = new Color(0.41f, 0.915f, 0.41f);
            if (currentPlayableTitle.text == "Current playable:")
            {
                currentPlayableTitleShadow.text = "Current playable (Disabled):";
                currentPlayableTitle.text = "Current playable (Disabled):";
            }
        } 
        else if (currentPlayableTitle.text == "Current playable (Disabled):")
        {
            currentPlayableTitleShadow.text = "Current playable:";
            currentPlayableTitle.text = "Current playable:";
        }
        UpdateLayout();
    }

    void UpdateLayout()
    {
        int count = GetButtonCount();
        if (count == previousButtonCount) return;
        previousButtonCount = count;

        if (count < 6)
        {
            layoutGroup.cellSize = new Vector2(500f, 500f);
        }
        else if (count < 10)
        {
            layoutGroup.cellSize = new Vector2(350f, 350f);
        }
        else if (count < 18)
        {
            layoutGroup.cellSize = new Vector2(275f, 275f);
        }

        layoutGroup.spacing = count is > 18 and < 27 ? new Vector2(-80f, -80f) : Vector2.zero;
        layoutGroup.padding.left = count > 27 ? 86 : 0;
    }

    private int GetButtonCount()
    {
        return FindObjectsOfType<Button>().Count(t => t.name.StartsWith("Temp") && t.name.EndsWith("(Clone)"));
    }
    
    public static Button AddButton(Texture icon,string name, string author, string folderName)
    {
        var addedButton = Instantiate(tempButton.gameObject, customSkinPanel.transform);
        Buttons.Add(addedButton);
        addedButton.GetComponent<RawImage>().texture = icon;
        var textShadow = addedButton.transform.Find("PlayableNameShadow").GetComponent<TextMeshProUGUI>();
        textShadow.text = $"{name}\n(by {author})";
        var textNormal = addedButton.transform.Find("PlayableName").GetComponent<TextMeshProUGUI>();
        textNormal.text = $"{name}\n(by {author})";
        if (folderName == PlayableLoaderPlugin.CurrentResource.FolderName)
        {
            currentSelectedTextShadow = textShadow;
            currentSelectedText = textNormal;
            currentSelectedTextShadow.color = new Color(0f, 0.28f,0.28f);
            currentSelectedText.color = new Color(0.41f, 0.915f,0.41f);
        }
        addedButton.GetComponent<Button>().onClick.AddListener(ActiveText(addedButton, folderName));
        return addedButton.GetComponent<Button>();
    }

    public static void DestroyButtons()
    {
        Buttons.ForEach(Destroy);
        Buttons.Clear();
    }

    private static UnityAction ActiveText(GameObject gameObject, string folderName)
    {
        return () => {
            currentSelectedTextShadow.color = new Color(0.42f, 0.09f,0.584f);
            currentSelectedText.color = Color.white;

            currentSelectedTextShadow = gameObject.transform.Find("PlayableNameShadow").GetComponent<TextMeshProUGUI>();
            currentSelectedText = gameObject.transform.Find("PlayableName").GetComponent<TextMeshProUGUI>();
            currentSelectedTextShadow.color = new Color(0f, 0.28f,0.28f);
            currentSelectedText.color = new Color(0.41f, 0.915f,0.41f);
        };
    }

    private static UnityAction<bool> EnableToggle()
    {
        return arg0 =>
        {
            PlayableLoaderPlugin.OnToggle(enableToggle.isOn);
        };
    }

    private void TogglePanel()
    {
        var panelState = customSkinPanel.activeSelf;
        customSkinPanel.SetActive(!panelState);
        transform.parent.parent.Find("Control bar").gameObject.SetActive(panelState);
    }

    private void OnRefresh()
    {
        PlayableLoaderPlugin.Refresh();
    }

    private void OpenFolder()
    {
        Process.Start(Path.Combine(Paths.PluginPath, "playables"));
    }

    public static void SetCurrentPlayableName(string name)
    {
        currentPlayableShadow.text = name;
        currentPlayable.text = name;
    }
}
