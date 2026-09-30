using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Dropdown))]
public class SpeedDropdownUI : MonoBehaviour
{
    private Dropdown speedDropdown;

    private void Awake() {
        speedDropdown = GetComponent<Dropdown>();
    }

    private void Start() {
        SetupDropdown();
    }

    private void OnDestroy() {
        if (speedDropdown != null) {
            speedDropdown.onValueChanged.RemoveListener(OnSpeedChanged);
        }
    }

    private void SetupDropdown() {
        if (speedDropdown == null) {
            return;
        }

        speedDropdown.ClearOptions();

        List<string> options = new List<string>() {
            "Slow (0.5x)",
            "Normal (1x)",
            "Fast (2x)",
            "Super Fast (4x)"
        };

        speedDropdown.AddOptions(options);

        int savedSpeedIndex = GameSpeedManager.GetSavedSpeedIndex();

        speedDropdown.onValueChanged.RemoveListener(OnSpeedChanged);
        speedDropdown.value = savedSpeedIndex;
        speedDropdown.RefreshShownValue();
        speedDropdown.onValueChanged.AddListener(OnSpeedChanged);

        GameSpeedManager.SetSpeedIndex(savedSpeedIndex);
    }

    private void OnSpeedChanged(int speedIndex) {
        GameSpeedManager.SetSpeedIndex(speedIndex);
    }
}