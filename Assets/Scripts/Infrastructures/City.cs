using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class City : MonoBehaviour {
    [Header("City Infos")] 
    [SerializeField] SO_City soCity_;

    [Header("UI")] 
    [SerializeField] TextMeshProUGUI uiCityName_;
    // Start is called before the first frame update
    void Start() {
        uiCityName_.text = soCity_.cityName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
