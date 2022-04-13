using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace gaugeChart
{
    public class analogGauge : MonoBehaviour
    {

        private Transform labelTemplate;
        private const float MIN_ANGLE = 30;
        private const float MAX_ANGLE = -210;

        private const float MIN_NEEDLE = 210;
        private const float MAX_NEEDLE = -30;

        private int minValue = -10, maxValue = 100;

        private void Awake()
        {
            labelTemplate = transform.Find("Template");
            labelTemplate.gameObject.SetActive(false);
            CreateLabel();
        }

        private void CreateLabel()
        {
            int noLabels = 10;

            float betweenAngle = (float)(MIN_ANGLE - MAX_ANGLE) / (noLabels);

            for (int i = 0; i <= noLabels; i++)
            {
                Transform newLable = Instantiate(labelTemplate, transform);
                newLable.eulerAngles = new Vector3(0, 0, MIN_ANGLE - betweenAngle * i);
                newLable.Find("TextLabel").GetComponent<Text>().text = Mathf.RoundToInt(minValue + i * ((maxValue - minValue) / noLabels)).ToString();
                newLable.Find("TextLabel").eulerAngles = Vector3.zero;
                newLable.gameObject.SetActive(true);
            }
        }
    }
}