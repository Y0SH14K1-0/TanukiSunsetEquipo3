using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DipTimeScaleFunction : MonoBehaviour
{
    // Start is called before the first frame update
    public AnimationCurve crv;
    float counter = 1f;
    float multiplier = 1f;
    bool setScale = false;

    private void Update()
    {
        if (!GameManager.Instance.isPaused)
        {
            if (counter <= 1f)
            {
                Time.timeScale = crv.Evaluate(counter);
                Time.fixedDeltaTime = Time.timeScale / 50f;
                counter += Time.unscaledDeltaTime / multiplier;
            }
            else
            {
                if (!setScale)
                {
                    setScale = true;
                    Time.timeScale = 1f;
                    Time.fixedDeltaTime = Time.timeScale / 50f;
                }
            }
        }

    }
    public void DipTimeScale(float timeMultiplier = 1f)
    {
        counter = 0f;
        multiplier = timeMultiplier;
        setScale = false;
    }
}
