print("IOT gateway")

import paho.mqtt.client as mqttclient
import time
import json
import serial.tools.list_ports

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
        if jsonobj['method'] == "setLEDValue":
            temp_data['LEDValue'] = jsonobj['params']
            if jsonobj['params'] == True:
                if len(bbc_port) > 0:
                    ser.write(str("#LED_ON$").encode('ascii'))
                    print("#LED_ON$")
            else:
                if len(bbc_port) > 0:
                    ser.write(str("#LED_OFF$").encode('ascii'))
                    print("#LED_OFF$")
        if jsonobj['method'] == "setFANValue":
            temp_data['FANValue'] = jsonobj['params']
            if jsonobj['params'] == True:
                if len(bbc_port) > 0:
                    ser.write(str("#FAN_ON$").encode('ascii'))
                    print("#FAN_ON$")
            else:
                if len(bbc_port) > 0:
                    ser.write(str("#FAN_OFF$").encode('ascii'))
                    print("#FAN_OFF$")
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

def getPort():
    ports = serial.tools.list_ports.comports()
    N = len(ports)
    commPort = "None"
    for i in range(0, N):
        port = ports[i]
        strPort = str(port)
        if "USB Serial Device" in strPort:
            splitPort = strPort.split(" ")
            commPort = (splitPort[0])
    return commPort

def processData(data):
    data = data.replace("!", "")
    data = data.replace("#", "")
    splitData = data.split(":")
    print(splitData)
    if splitData[1] == "TEMP":
        collect_data = {'temperatureMicrobit': splitData[2]}
        client.publish('v1/devices/me/telemetry', json.dumps(collect_data), 1)
    if splitData[1] == "LIGHT":
        collect_data = {'lightMicrobit': splitData[2]}
        client.publish('v1/devices/me/telemetry', json.dumps(collect_data), 1)
    if splitData[1] == "LED":
        if splitData[2] == '1':
            temp_data = {'LEDValue': True}
        else:
            temp_data = {'LEDValue': False}
        client.publish('v1/devices/me/attributes', json.dumps(temp_data), 1)
    if splitData[1] == "FAN":
        if splitData[2] == '1':
            temp_data = {'FANValue': True}
        else:
            temp_data = {'FANValue': False}
        client.publish('v1/devices/me/attributes', json.dumps(temp_data), 1)
    
def readSerial():
    bytesToRead = ser.inWaiting()
    if (bytesToRead > 0):
        mess = ser.read(bytesToRead).decode("UTF-8")
        print("Serial receive", mess)
        while ("#" in mess) and ("!" in mess):
            start = mess.find("!")
            end = mess.find("#")
            processData(mess[start:end + 1])
            if (end == len(mess)):
                mess = ""
            else:
                mess = mess[end+1:]

bbc_port = ""
if len(bbc_port) <= 0:
    bbc_port = getPort()
ser = serial.Serial(port=bbc_port, baudrate=115200)

while True:
    if(len(bbc_port) > 0):
        readSerial()
    time.sleep(1)