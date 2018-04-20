// Michael Camara, 2018

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowSliderValue : MonoBehaviour {

    public Slider slider;
    private Text valueText;

    
    void Awake() {
        if(slider == null) {
            Debug.LogError("Could not find UI.Slider ref on " + transform);
        }

        valueText = GetComponent<Text>();
        if(valueText == null) {
            Debug.LogError("Could not find UI.Text on " + transform);
        }

        UpdateSliderText();
    }
	
	public void UpdateSliderText() {
        valueText.text = slider.value.ToString();
    }
}
