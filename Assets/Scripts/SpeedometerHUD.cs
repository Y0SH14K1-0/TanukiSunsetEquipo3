using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedometerHUD : MonoBehaviour
{
    public TanukiBitsHUD tanukiBitsHUD;
    public Text speedIntegerText;
    public Text speedDecimalText;
    public Text speedUnitsText;
    public Text bitsIntegerText;
    public Image speedometerFillImage;
    public List<Text> speedometerMarkers;

    private SpeedUnit currentSpeedUnit;
    private float speedMultiplier = 1f;
    private float smoothSpeedInUnits;

    public void OnEnable()
    {
        OnTanukiBitsChanged(SaveSystem.CurrentSaveData().tanukiBitsCollected);
    }

    private void Start()
    {
        tanukiBitsHUD.OnTanukiBitsChanged += OnTanukiBitsChanged;

        // TODO: Grab this value from user preferences
        currentSpeedUnit = SpeedUnit.KMH;

        speedUnitsText.text = (currentSpeedUnit == SpeedUnit.KMH) ? "KM/H" : "MPH";
        speedMultiplier = (currentSpeedUnit == SpeedUnit.KMH) ? 3.6f : 2.237f;
        UpdateSpeedometerMarkers();
    }

    private void Update()
    {
        UpdateSpeedometer(GameManager.Instance.longBoardCTRL.speed0to1Probable);
    }

    private void UpdateSpeedometer(float speedNormalized)
    {
        float minSpeed = GameManager.Instance.longBoardCTRL.minSpeed * speedMultiplier;
        float maxSpeed = GameManager.Instance.longBoardCTRL.maxSpeed * speedMultiplier;
        float speedInUnits = Mathf.Lerp(minSpeed, maxSpeed, speedNormalized);
        
        smoothSpeedInUnits = Mathf.Lerp(smoothSpeedInUnits, speedInUnits, 5f * Time.unscaledDeltaTime);

        speedometerFillImage.fillAmount = smoothSpeedInUnits / maxSpeed;

        speedIntegerText.text = string.Format("{0:n0}", Mathf.FloorToInt(smoothSpeedInUnits).ToString());
        speedDecimalText.text = "."+((smoothSpeedInUnits - (float)Mathf.FloorToInt(smoothSpeedInUnits)) * 10f).ToString("0");
    }

    private void OnTanukiBitsChanged(int currentBits)
    {
        bitsIntegerText.text = string.Format("{0:n0}", currentBits);
    }

    private void UpdateSpeedometerMarkers()
    {
        float maxSpeed = GameManager.Instance.longBoardCTRL.maxSpeed * speedMultiplier;
        float speedIncrements = maxSpeed / (float)speedometerMarkers.Count;

        for (int i = 1; i < speedometerMarkers.Count+1; i++)
        {
            speedometerMarkers[i-1].text = string.Format("{0}", Mathf.FloorToInt(speedIncrements * (float)i));
        }
    }
}

public enum SpeedUnit
{
    KMH,
    MPH
}