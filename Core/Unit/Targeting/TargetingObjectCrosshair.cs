using System.Collections;
using Cinemachine;
using Core.Helpers;
using Core.Unit.Targeting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Container for the Targeting Crosshair while targeting body parts (TargetingObjects)
/// </summary>
public class TargetingObjectCrosshair : MonoBehaviour
{
    public Color crosshairColor;
    public Canvas crosshairCanvas;
    public Image crosshairImage;
    public Vector3 lookAtVector;
    public bool isVisible;
    [Range(100, 1000)] public float rotationSpeed = 200;
    [Range(0, 10)] public float moveSpeed = 2;
    [Range(100, 1000)] public float colorSpeed = 200;
    public TargetingObject target;
    private CinemachineVirtualCameraBase cam;

    private Color crosshairRed;
    private Color crosshairYellow;
    private bool isColoringRed;
    private float z;

    public void Start()
    {
        crosshairRed = ColorHelper.GetColor(ColorPallete.Red, ColorShade.Darker);
        crosshairYellow = ColorHelper.GetColor(ColorPallete.Yellow, ColorShade.Lighter);
    }

    public void EnableCrosshair(TargetingObject targetingObject)
    {
        isVisible = true;
        gameObject.SetActive(true);
        crosshairColor = crosshairRed;
        crosshairCanvas.gameObject.SetActive(false);
        cam = CameraController.instance.currentCinemachine;
        target = targetingObject;
        StartCoroutine(MoveCrosshair());
    }

    public void DisableCrosshair()
    {
        isVisible = false;
        gameObject.SetActive(false);
        StopAllCoroutines();
    }

    private IEnumerator MoveCrosshair()
    {
        var isInPosition = false;
        while (!isInPosition)
        {
            yield return new WaitForSeconds(1);
            transform.position = target.transform.position;
            transform.LookAt(cam.transform);
            isInPosition = true;
        }

        while (isVisible)
        {
            crosshairCanvas.gameObject.SetActive(true);
            transform.LookAt(cam.transform);
            var targetingCrosshairPosition = target.transform.position;
            var offset = cam.transform.position - targetingCrosshairPosition;
            transform.position = Vector3.Slerp(transform.position, targetingCrosshairPosition + offset.normalized * .5f,
                Time.deltaTime * moveSpeed);
            crosshairCanvas.transform.Rotate(0, 0, .2f, Space.Self);

            if (crosshairImage.color == crosshairYellow) isColoringRed = true;

            if (crosshairImage.color == crosshairRed) isColoringRed = false;
            crosshairImage.color = Color.Lerp(crosshairImage.color, isColoringRed ? crosshairRed : crosshairYellow,
                Time.deltaTime * colorSpeed);
            yield return new WaitForEndOfFrame();
        }
    }

    public void ChangeTargetingObject(TargetingObject targetingObject)
    {
        target = targetingObject;
        //StartCoroutine(ChangeMoveSpeed(3));
    }

    private IEnumerator ChangeMoveSpeed(float speedMultiplier)
    {
        var countDown = 1f;
        var originalSpeed = moveSpeed;
        var changeTargetSpeed = moveSpeed * speedMultiplier;
        moveSpeed = changeTargetSpeed;
        while (countDown >= 0)
        {
            countDown -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        moveSpeed = originalSpeed;
    }
}