using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;
using DG.Tweening;

namespace M2MqttUnity.IoTDashboard
{
    public class M2MqttUnityIoTDashboard : M2MqttUnityClient 
    {
        // Declare attributes
        // Screens
        public RectTransform login, error, mainScreen;
        // Inputs in screen 1
        public InputField brokerURIInputField;
        public InputField usernameInputField;
        public InputField passwordInputField;
        public Button connectButton;
        // Hide/show password images
        public Image hidePassImage;
        public Image showPassImage;
        // Toggle buttons
        public RectTransform buttonLED;
        public RectTransform buttonPUMP;
        public Image btled;
        public Image btpump;
        // Digital gauge
        public Scrollbar hunidityBar;
        public Text humidityBarText;
        // Error
        public Text typeMessage;
        public Text errorMessage;
        // Temperature and humidity text display
        public Text temperatureDisplay;
        public Text humidityDisplay;
        
        // Declare variable
        public bool isLogOut;
        public bool LEDON = false;
        public bool PUMPON = false;
        public bool isConnected;

        private List<string> eventMessages = new List<string>();

        public void ConnectToServer()
        {
            // Save new infomations
            PlayerPrefs.SetString("brokerURI", brokerURIInputField.text);
            PlayerPrefs.SetString("username", usernameInputField.text);
            PlayerPrefs.SetString("password", passwordInputField.text);
            // Update new information to connect to server
            this.brokerAddress = brokerURIInputField.text;
            this.mqttUserName = usernameInputField.text;
            this.mqttPassword = passwordInputField.text;
            // Call connect function
            this.Connect();
        }

        // If connecting successfully, then move to screen 2
        protected override void OnConnected()
        {
            base.OnConnected();
            isConnected = true;
            login.DOAnchorPos(new Vector2(-760, -74), 0.25f);
            mainScreen.DOAnchorPos(new Vector2(0, -74), 0.25f);
        }

        // Connect to server failure, annouce error
        protected override void OnConnectionFailed(string errorMessage)
        {
            base.OnConnectionFailed(errorMessage);
            typeMessage.text = "Connection failed!";
            this.errorMessage.text = errorMessage;
            login.DOAnchorPos(new Vector2(-760, -74), 0.1f);
            error.DOAnchorPos(new Vector2(0, -74), 0.1f);
        }

        public void LEDPublish()
        {
            if(LEDON)
            {
                client.Publish("/bkiot/1913743/led", System.Text.Encoding.UTF8.GetBytes("'device': 'LED', 'status': 'ON'"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                Debug.Log("LED published ON");
            }
            else
            {
                client.Publish("/bkiot/1913743/led", System.Text.Encoding.UTF8.GetBytes("'device': 'LED', 'status': 'OFF'"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                Debug.Log("LED published OFF");
            }
        }

        public void PUMPPublish()
        {
            if (PUMPON)
            {
                client.Publish("/bkiot/1913743/pump", System.Text.Encoding.UTF8.GetBytes("'device': 'PUMP', 'status': 'ON'"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                Debug.Log("PUMP published ON");
            }
            else
            {
                client.Publish("/bkiot/1913743/pump", System.Text.Encoding.UTF8.GetBytes("'device': 'PUMP', 'status': 'OFF'"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                Debug.Log("PUMP published OFF");
            }
        }

        protected override void SubscribeTopics()
        {
            client.Subscribe(new string[] { "/bkiot/1913743/status" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        protected override void UnsubscribeTopics()
        {
            client.Unsubscribe(new string[] { "/bkiot/1913743/status" });
        }

        protected override void OnDisconnected()
        {
            if (!isLogOut)
            {
                typeMessage.text = "Disconnected!";
                this.errorMessage.text = "";
                error.DOAnchorPos(new Vector2(0, -74), 0.25f);
            }
        }

        protected override void OnConnectionLost()
        {
            if (!isLogOut)
            {
                typeMessage.text = "Disconnected lost!";
                this.errorMessage.text = "";
                error.DOAnchorPos(new Vector2(0, -74), 0.25f);
            }
        }

        protected override void Start()
        {
            // Move screens
            login.DOAnchorPos(new Vector2(0, -74), 0.1f);
            error.DOAnchorPos(new Vector2(0, 1100), 0.1f);
            mainScreen.DOAnchorPos(new Vector2(770, -74), 0.1f);
            // Initial type of password display
            isLogOut = false;
            passwordInputField.contentType = InputField.ContentType.Password;
            hidePassImage.enabled = true;
            showPassImage.enabled = false;
            // Load the last information to inputfield
            brokerURIInputField.text = PlayerPrefs.GetString("brokerURI");
            usernameInputField.text = PlayerPrefs.GetString("username");
            passwordInputField.text = PlayerPrefs.GetString("password");
            // Update temperature and humidity for display
            UpdateTemperature();
            UpdateHumidity();
            // Initial for toggle button
            isConnected = false;
            LEDON = true; PUMPON = true;
            ButtonLEDClick();
            ButtonPUMPClick();
            ButtonLEDClick();
            ButtonPUMPClick();

            base.Start();
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            string msg = System.Text.Encoding.ASCII.GetString(message);

            string field, value;
            int i = 0;
            while(i < msg.Length)
            {
                field = ""; value = "";
                while (i < msg.Length && msg[i] != '\"') i++;
                i++;
                while (i < msg.Length && msg[i] != '\"') 
                {
                    field += msg[i];
                    i++;
                }
                i += 2;
                while( i < msg.Length && msg[i] != ',' && msg[i] != '}')
                {
                    value += msg[i];
                    i++;
                }
                if(field == "temp")
                {
                    PlayerPrefs.SetString("temperature", value);
                    UpdateTemperature();
                }
                else if(field == "humi")
                {
                    PlayerPrefs.SetString("humidity", value);
                    UpdateHumidity();
                }
            }

            Debug.Log("Received: " + msg);
            StoreMessage(msg);
        }

        private void StoreMessage(string eventMsg)
        {
            eventMessages.Add(eventMsg);
        }

        private void ProcessMessage(string msg)
        {
            Debug.Log("Received: " + msg);
        }

        protected override void Update()
        {
            base.Update(); // call ProcessMqttEvents()

            if (eventMessages.Count > 0)
            {
                foreach (string msg in eventMessages)
                {
                    ProcessMessage(msg);
                }
                eventMessages.Clear();
            }
        }

        public void ShowUserPassword()
        {
            if (passwordInputField.contentType == InputField.ContentType.Password)
            {
                passwordInputField.contentType = InputField.ContentType.Standard;
                hidePassImage.enabled = false;
                showPassImage.enabled = true;
            }
            else
            {
                passwordInputField.contentType = InputField.ContentType.Password;
                hidePassImage.enabled = true;
                showPassImage.enabled = false;
            }
            passwordInputField.ForceLabelUpdate();
        }

        public void BackToLogin()
        {
            login.DOAnchorPos(new Vector2(0, -74), 0.1f);
            error.DOAnchorPos(new Vector2(0, 1100), 0.1f);
            mainScreen.DOAnchorPos(new Vector2(770, -74), 0.1f);
        }

        public void LogOut()
        {
            isLogOut = true;
            isConnected = false;
            Disconnect();
            login.DOAnchorPos(new Vector2(0, -74), 0.1f);
            mainScreen.DOAnchorPos(new Vector2(770, -74), 0.1f);
        }

        public void ResetIsLogOut()
        {
            isLogOut = false;
        }

        public void UpdateTemperature()
        {
            if(PlayerPrefs.HasKey("temperature"))
            {
                temperatureDisplay.text = (PlayerPrefs.GetString("temperature").ToString() + " °C");
            }
            else
            {
                temperatureDisplay.text = "__ °C";
            }
            int t = 0;
            if(Int32.TryParse(PlayerPrefs.GetString("temperature"), out t))
            {
                UpdateNeedle(t);
            }
        }

        public void UpdateHumidity()
        {
            if (PlayerPrefs.HasKey("humidity"))
            {
                humidityDisplay.text = (PlayerPrefs.GetString("humidity").ToString() + " %");
                humidityBarText.text = (PlayerPrefs.GetString("humidity").ToString() + " %");
            }
            else
            {
                humidityDisplay.text = "__ %";
                humidityBarText.text = "__ %";
            }
            int h = 0;
            if (Int32.TryParse(PlayerPrefs.GetString("humidity"), out h))
            {
                hunidityBar.size = (float)(h % 100)/100;
            }
        }

        public void ButtonLEDClick()
        {
            if (LEDON)
            {
                buttonLED.DOAnchorPos(new Vector2(39, -33), 0.25f);
                btled.color = new Color(14f / 255f, 213f / 255f, 210f / 255f);
            }
            else
            {
                buttonLED.DOAnchorPos(new Vector2(-39, -33), 0.25f);
                btled.color = Color.gray;
            }
            if(isConnected) LEDPublish();
            LEDON = !LEDON;
        }
        public void ButtonPUMPClick()
        {
            if (PUMPON)
            {
                buttonPUMP.DOAnchorPos(new Vector2(39, -33), 0.25f);
                btpump.color = new Color(14f / 255f, 213f / 255f, 210f / 255f);
            }
            else
            {
                buttonPUMP.DOAnchorPos(new Vector2(-39, -33), 0.25f);
                btpump.color = Color.gray;
            }
            if(isConnected) PUMPPublish();
            PUMPON = !PUMPON;
        }


        // Needle of analog gauge
        public Transform needleTransform;

        private const float MIN_NEEDLE = 210;
        private const float MAX_NEEDLE = -30;

        private int minValue = -10, maxValue = 100;

        public void UpdateNeedle(float value)
        {
            float angle = (float)(value - minValue) / (float)(maxValue - minValue) * (MIN_NEEDLE - MAX_NEEDLE);
            needleTransform.eulerAngles = new Vector3(0, 0, (float)MIN_NEEDLE - angle - (float)1);
        }

    }
}
