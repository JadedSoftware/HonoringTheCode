using System;
using System.Collections;
using System.Collections.Generic;
using Core.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CrossFadeCanvas : MonoBehaviour
{
    private bool isFading;
    [SerializeField] private Image crossFadeImage;
    [SerializeField] private Material crossFadeMaterial;
    [SerializeField] private string propertyName = "_Progress";
    public UnityEvent OnTranistionFinished;
    
    private void Start()
    {
        crossFadeImage = GetComponentInChildren<Image>();
        crossFadeMaterial = crossFadeImage.material;
        crossFadeMaterial.SetFloat(propertyName, 0);
    }

    public void FadeIn()
    {
        StartCoroutine(CrossFadeIn());
    }

    private IEnumerator CrossFadeIn()
    {
        var currentTime = 0f;
        isFading = true;
        while (isFading)
        {
            crossFadeMaterial.SetFloat(propertyName, Mathf.Clamp01(currentTime/GameUiController.instance.crossFadeTime));
            currentTime += Time.deltaTime;
            isFading = currentTime < GameUiController.instance.crossFadeTime;
            yield return null;
        }
        GameUiController.instance.FadeInComplete();
        gameObject.SetActive(false);
    }
}
