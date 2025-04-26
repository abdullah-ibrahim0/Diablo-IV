using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemonHealthbar : MonoBehaviour
{
    public Image healthBarForeground; // Assign this in the Inspector
    private Transform cameraTransform; // To face the camera
    private DemonBehavior demonBehavior;

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = Camera.main.transform;
        demonBehavior = GetComponentInParent<DemonBehavior>();

    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position + cameraTransform.forward);

        if (demonBehavior != null && healthBarForeground != null)
        {
            float healthPercent = Mathf.Clamp01((float)demonBehavior.health / 40f); // Assuming max health is 100
            healthBarForeground.fillAmount = healthPercent;
            healthBarForeground.transform.localScale = new Vector3(healthPercent, 1,1);
            Debug.Log("Demon Health Percent: " + healthPercent);
        }
    }
}
