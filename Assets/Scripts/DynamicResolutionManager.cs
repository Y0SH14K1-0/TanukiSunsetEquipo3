using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicResolutionManager : MonoBehaviour
{
    private int thresholdFPS = 54;

    private float maxResolutionWidthScale = 0.92f;
    private float maxResolutionHeightScale = 0.92f;
    private float minResolutionWidthScale = 0.66f;
    private float minResolutionHeightScale = 0.66f;

    float m_widthScale;
    float m_heightScale;

    void Start()
    {
        m_widthScale = maxResolutionWidthScale;
        m_heightScale = maxResolutionHeightScale;
    }

    void Update()
    {
        float oldWidthScale = m_widthScale;
        float oldHeightScale = m_heightScale;

        DetermineResolution();

        float averageFrameTime = ((float)m_gpuFrameTime+(float)m_cpuFrameTime)/2f;
        float availableFrameTime = (1f/(float)thresholdFPS);
        float currentScaleFactor = (availableFrameTime-averageFrameTime)/availableFrameTime;

        m_widthScale = Mathf.Lerp(minResolutionWidthScale, maxResolutionWidthScale, currentScaleFactor);
        m_heightScale = Mathf.Lerp(minResolutionHeightScale, maxResolutionHeightScale, currentScaleFactor);

        if (m_widthScale != oldWidthScale || m_heightScale != oldHeightScale)
        {
            ScalableBufferManager.ResizeBuffers(m_widthScale, m_heightScale);
        }
    }

    FrameTiming[] frameTimings = new FrameTiming[3];
    uint m_frameCount = 0;
    const uint kNumFrameTimings = 2;
    double m_gpuFrameTime;
    double m_cpuFrameTime;

    private void DetermineResolution()
    {
        ++m_frameCount;
        if (m_frameCount <= kNumFrameTimings)
        {
            return;
        }
        FrameTimingManager.CaptureFrameTimings();
        FrameTimingManager.GetLatestTimings(kNumFrameTimings, frameTimings);
        if (frameTimings.Length < kNumFrameTimings)
        {
            //Debug.LogFormat("Skipping frame {0}, didn't get enough frame timings.", m_frameCount);
            return;
        }

        m_gpuFrameTime = (double)frameTimings[0].gpuFrameTime;
        m_cpuFrameTime = (double)frameTimings[0].cpuFrameTime;
    }
}
