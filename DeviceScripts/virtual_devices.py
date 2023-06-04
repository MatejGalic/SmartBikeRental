import random
import threading
import time
import json
from paho.mqtt import client as mqtt_client

broker = '161.53.19.19'
port = 56883
topic = {
    1: "json/BR_Virtual_1/IoTGrupa10",
    2: "json/BR_Virtual_2/IoTGrupa10",
    3: "json/BR_Virtual_3/IoTGrupa10",
    4: "json/BR_Virtual_4/IoTGrupa10",
    5: "json/BR_Virtual_5/IoTGrupa10"
}

subscribe = {
    1: "all/BR_Virtual_1",
    2: "all/BR_Virtual_2",
    3: "all/BR_Virtual_3",
    4: "all/BR_Virtual_4",
    5: "all/BR_Virtual_5"
}

longitude = {
    1: 15.971724,
    2: 15.978129,
    3: 15.977711,
    4: 15.967212,
    5: 15.962231
}

latitude = {
    1: 45.800705,
    2: 45.804902,
    3: 45.811831,
    4: 45.808898,
    5: 45.842641
}

client_id = f'publish-{random.randint(0, 1000)}'
led = 0


def connect_mqtt():
    client = mqtt_client.Client()
    client.on_connect = on_connect
    client.on_message = on_message
    client.connect(broker, port, 60)
    return client


def on_connect(client, userdata, flags, rc):
    thread_number = int(threading.current_thread().name)
    if rc == 0:
        print("Device-" + str(thread_number) + " is connected to MQTT Broker!")
    else:
        print("Failed to connect, return code %d\n", rc)
    client.subscribe(subscribe.get(thread_number), 1)


def on_message(client, userdata, msg):
    thread_number = int(threading.current_thread().name)
    print("Device-" + str(thread_number) + " received a message. LED is " + str(
        led) + " topic is " + msg.topic + " payload is " + str(msg.payload))
    data = json.loads(msg.payload)
    bike_rental_led = None
    for content_node in data['contentNodes']:
        if 'BikeRentalLed' in content_node['source']['resourceSpec']:
            bike_rental_led = content_node['value']
            break
    if bike_rental_led == 1:
        time.sleep(random.randint(2, 10))
        print("Device-" + str(thread_number) + " is unlocking bike LED is " + str(led))
        unlock_bike()
        print("Device-" + str(thread_number) + " bike unlocked. LED is " + str(led))
        time.sleep(10)
        print("Device-" + str(thread_number) + " is locking bike LED is " + str(led))
        lock_bike()
        print("Device-" + str(thread_number) + " bike locked. LED is " + str(led))
        publish(client, read_distance(led), led, thread_number)


def publish(client, distance, led, thread_number):
    timestamp = int(time.time()) * 1000 + 7200000
    msg = {
        "header": {
            "timeStamp": timestamp
        },
        "body": {
            "BikeRentalActuator": {
                "BikeRentalLed": led
            },
            "BikeRentalHC-SR04": {
                "BikeRentalDistance": distance
            },
            "BikeRentalGPS": {
                "BikeRentalLatitude": latitude.get(thread_number),
                "BikeRentalLongitude": longitude.get(thread_number)
            }
        }
    }
    msg_mqtt = json.dumps(msg)
    result = client.publish(topic.get(thread_number), msg_mqtt)
    status = result[0]
    if status == 0:
        print(f"Device-'{thread_number}' is sending `{msg}` to topic `{topic.get(thread_number)}`")
    else:
        print(f"Device-'{thread_number}' failed to send message to topic {topic.get(thread_number)}")


def lock_bike():
    global led
    led = 0


def unlock_bike():
    global led
    led = 1


def read_distance(led):
    if led == 0:
        return 5
    elif led == 1:
        return random.randint(6, 100000)


def run_device_thread(thread_number):
    client = connect_mqtt()
    distance = read_distance(1)
    publish(client, distance, 0, thread_number)
    client.loop_forever()


if __name__ == '__main__':
    device_threads = []
    for i in range(1, 6):
        thread = threading.Thread(target=run_device_thread, args=(i,), name=f"{i}")
        device_threads.append(thread)

    for thread in device_threads:
        thread.start()

    for thread in device_threads:
        thread.join()
