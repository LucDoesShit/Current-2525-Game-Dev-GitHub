using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class DayAndNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    [Range(0f, 24f)]
    public float currentTime;
    public float timeSpeed = 1f;

    [Header("CurrentTime")]
    public string currentTimeString;

    [Header("Sun Settings")]
    public Light sunLight;
    [Range(0f, 90f)] public float sunLatitude = 20f;
    [Range(-180f, 180f)] public float sunLongitude = -90f;
    public float sunIntensity = 1f;
    public AnimationCurve sunIntensityMultiplier;
    public AnimationCurve sunTemperatureCurve;

    public bool isDay = true;
    public bool sunActive = true;
    public bool moonActive = true;

    [Header("Moon Settings")]
    public Light moonLight;
    [Range(0f, 90f)] public float moonLatitude = 40f;
    [Range(-180f, 180f)] public float moonLongitude = 90f;
    public float moonIntensity = 1f;
    public AnimationCurve moonIntensityMultiplier;
    public AnimationCurve moonTemperatureCurve;

    [Header("Stars")]
    public VolumeProfile volumeProfile;
    private PhysicallyBasedSky skySettings;
    public float starsIntensity = 1f;
    public AnimationCurve starsCurve;
    [Range(0f, 90f)] public float polarStarLatitude = 40f;
    [Range(-180f, 180f)] public float polarStarLongitude = 90f;

    void Start()
    {
        UpdateTimeText();
        CheckShadowStatus();
        SkyStar();
    }

    void Update()
    {
        currentTime += Time.deltaTime * timeSpeed;
        UpdateTimeText();
        UpdateLight();
        CheckShadowStatus();
        SkyStar();
    }

    private void OnValidate()
    {
        UpdateLight();
        CheckShadowStatus();
        SkyStar();
    }

    void UpdateTimeText()
    {
        float displayTime = currentTime % 24f;
        currentTimeString = Mathf.Floor(displayTime).ToString("00") + ":" + ((displayTime % 1) * 60).ToString("00");
    }

    void UpdateLight()
    {
        float normalizedTime = (currentTime % 24f) / 24f;
        float sunRotation = normalizedTime * 360f;

        sunLight.transform.localRotation =
            Quaternion.Euler(sunLatitude - 90, sunLongitude, 0) *
            Quaternion.Euler(0, sunRotation, 0);

        moonLight.transform.localRotation =
            Quaternion.Euler(90 - moonLatitude, moonLongitude, 0) *
            Quaternion.Euler(0, sunRotation, 0);

        float sunIntensityCurve = sunIntensityMultiplier.Evaluate(normalizedTime);
        float moonIntensityCurve = moonIntensityMultiplier.Evaluate(normalizedTime);

        if (sunLight != null)
        {
            sunLight.intensity = sunIntensityCurve * sunIntensity;
            sunLight.colorTemperature = sunTemperatureCurve.Evaluate(normalizedTime) * 10000f;
        }

        if (moonLight != null)
        {
            moonLight.intensity = moonIntensityCurve * moonIntensity;
            moonLight.colorTemperature = moonTemperatureCurve.Evaluate(normalizedTime) * 10000f;
        }
    }

    void CheckShadowStatus()
    {
        HDAdditionalLightData sunLightData = sunLight.GetComponent<HDAdditionalLightData>();
        HDAdditionalLightData moonLightData = moonLight.GetComponent<HDAdditionalLightData>();

        float sunHeight = Vector3.Dot(sunLight.transform.forward, Vector3.down);

        if (sunHeight > 0)
        {
            if (sunLightData != null) sunLightData.EnableShadows(true);
            if (moonLightData != null) moonLightData.EnableShadows(false);

            sunLight.gameObject.SetActive(true);
            moonLight.gameObject.SetActive(false);

            isDay = true;
            sunActive = true;
            moonActive = false;
        }
        else
        {
            if (sunLightData != null) sunLightData.EnableShadows(false);
            if (moonLightData != null) moonLightData.EnableShadows(true);

            sunLight.gameObject.SetActive(false);
            moonLight.gameObject.SetActive(true);

            isDay = false;
            sunActive = false;
            moonActive = true;
        }
    }

    void SkyStar()
    {
        if (volumeProfile != null && volumeProfile.TryGet<PhysicallyBasedSky>(out skySettings))
        {
            float normalizedTime = (currentTime % 24f) / 24f;
            skySettings.spaceEmissionMultiplier.value = starsCurve.Evaluate(normalizedTime) * starsIntensity;
            skySettings.spaceRotation.value =
                (Quaternion.Euler(90 - polarStarLatitude, polarStarLongitude, 0) *
                 Quaternion.Euler(0, normalizedTime * 360f, 0)).eulerAngles;
        }
    }
}