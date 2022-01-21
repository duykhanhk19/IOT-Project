print("IOT gateway")

import paho.mqtt.client as mqttclient
import time
import json
import random

BROKER_ADDRESS = "demo.thingsboard.io"
PORT = 1883
THINGS_BOARD_ACCESS_TOKEN = "V8Wlen76eZVHqNu9wfgy"


def subscribed(client, userdata, mid, granted_qos):
    print("Subscribed...")


def recv_message(client, userdata, message):
    print("Received: ", message.payload.decode("utf-8"))
    temp_data = {'value': True}
    try:
        jsonobj = json.loads(message.payload)
        if jsonobj['method'] == "setValue":
            temp_data['value'] = jsonobj['params']
            client.publish('v1/devices/me/attributes', json.dumps(temp_data), 1)
    except:
        pass


def connected(client, usedata, flags, rc):
    if rc == 0:
        print("Thingsboard connected successfully!!")
        client.subscribe("v1/devices/me/rpc/request/+")
    else:
        print("Connection is failed")


client = mqttclient.Client("Gateway_Thingsboard")
client.username_pw_set(THINGS_BOARD_ACCESS_TOKEN)

client.on_connect = connected
client.connect(BROKER_ADDRESS, PORT)
client.loop_start()

client.on_subscribe = subscribed
client.on_message = recv_message

temp = 30
humi = 50
latitude = 10.8231
longitude = 106.6297
while True:
    collect_data = {'temperature': temp, 'humidity': humi, 'latitude': latitude, 'longitude': longitude}
    temp += random.randrange(-5, 6) 
    humi += random.randrange(-5, 6)
    client.publish('v1/devices/me/telemetry', json.dumps(collect_data), 1)
    time.sleep(10)