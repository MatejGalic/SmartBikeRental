#include <WiFi.h>
#include <PubSubClient.h>
#include <ctime>
#include "time.h"
#include "sntp.h"
#include "cJSON.h"

const char* ntpServer1 = "pool.ntp.org";
const char* ntpServer2 = "time.nist.gov";
const long  gmtOffset_sec = 3600;
const int   daylightOffset_sec = 3600;
const char* time_zone = "CET-1CEST,M3.5.0,M10.5.0/3";

// WiFi credentials
const char* ssid = "ssid";
const char* password = "pass";

// MQTT broker details
const char* mqttServer = "161.53.19.19";
const int mqttPort = 56883;
const char* mqttTopic = "json/BR_Real/IoTGrupa10";
const char* mqttSubscribeTopic = "all/BR_Real";

// HC-SR04 Ultrasonic Sensor
const int trigPin = 4;
const int echoPin = 5;
const int ledPin = 3; // External LED

int led = 0;
long duration;
int distance;
float longitude = 15.976398618818015;
float latitude =  45.811136644362456;

WiFiClient wifiClient;
PubSubClient mqttClient(wifiClient);

int isTimeAvailable = 0;

int readDistance();
void timeavailable(struct timeval *t);
void parseJSON(const char* jsonString);
void connectToWiFi();
void connectToMQTT();
void publish();
void callback(char* topic, byte* payload, unsigned int length);

int readDistance() {
  // Trigger the ultrasonic sensor
  digitalWrite(trigPin, LOW);
  delayMicroseconds(2);
  digitalWrite(trigPin, HIGH);
  delayMicroseconds(10);
  digitalWrite(trigPin, LOW);

  // Read the echo duration
  duration = pulseIn(echoPin, HIGH);
  // Calculate the distance based on the speed of sound
  distance = duration * 0.034 / 2;

  return distance;
}

void timeavailable(struct timeval *t)
{
  Serial.println("Got time adjustment from NTP!");
  isTimeAvailable = 1;
}

void parseJSON(const char* jsonString) {
    cJSON* json = cJSON_Parse(jsonString);
    if (json == NULL) {
        printf("Failed to parse JSON string.\n");
        return;
    }

    cJSON* header = cJSON_GetObjectItem(json, "header");
    if (header != NULL) {
        cJSON* timeStamp = cJSON_GetObjectItem(header, "timeStamp");
    }

    cJSON* body = cJSON_GetObjectItem(json, "body");
    if (body != NULL) {
        cJSON* actuator = cJSON_GetObjectItem(body, "BikeRentalActuator");
        if (actuator != NULL) {
            cJSON* led = cJSON_GetObjectItem(actuator, "BikeRentalLed");
        }

        cJSON* hcSr04 = cJSON_GetObjectItem(body, "BikeRentalHC-SR04");
        if (hcSr04 != NULL) {
            cJSON* distance = cJSON_GetObjectItem(hcSr04, "BikeRentalDistance");
        }

        cJSON* gps = cJSON_GetObjectItem(body, "BikeRentalGPS");
        if (gps != NULL) {
            cJSON* latitude = cJSON_GetObjectItem(gps, "BikeRentalLatitude");
            cJSON* longitude = cJSON_GetObjectItem(gps, "BikeRentalLongitude");
        }
    }
    cJSON_Delete(json);
}

void connectToWiFi() {
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(1000);
    Serial.println("Connecting to WiFi...");
  }
  Serial.println("Connected to WiFi");
}

void connectToMQTT() {
  while (!mqttClient.connected()) {
    Serial.println("Connecting to MQTT...");
    if (mqttClient.connect("clientID")) {
      mqttClient.setBufferSize(2048);
      mqttClient.subscribe(mqttSubscribeTopic);
      Serial.println("Connected to MQTT");
    } else {
      Serial.print("Failed to connect to MQTT, rc=");
      Serial.println(mqttClient.state());
      delay(2000);
    }
  }
}

void publish() {
    while(!isTimeAvailable) {
      mqttClient.loop();
    }
    time_t timestamp = time(nullptr);
    long long int timestampINT = ((long long int)timestamp + 7200) * 1000;

    distance = readDistance();
    Serial.printf("HC-SR04 distance: %d\n", distance);

    char msg[300];
    snprintf(msg, sizeof(msg),
        "{\"header\":{\"timeStamp\":%lld},"
        "\"body\":{"
        "\"BikeRentalActuator\":{\"BikeRentalLed\":%d},"
        "\"BikeRentalHC-SR04\":{\"BikeRentalDistance\":%d},"
        "\"BikeRentalGPS\":{\"BikeRentalLatitude\":%f,\"BikeRentalLongitude\":%f}}}", 
          timestampINT, led, distance, latitude, longitude);
    parseJSON(msg);

    bool status = mqttClient.publish(mqttTopic, msg);

    if (status) {
      Serial.printf("Sending this message to the topic %s:\n", mqttTopic);
      Serial.printf("%s\n", msg);
    } else {
      Serial.printf("Failed to send message to topic %s. MQTTClient connected: %d\n", mqttTopic, mqttClient.connected());
    }
}

void callback(char* topic, byte* payload, unsigned int length) {
  int cnt = 0;
  for(int i=0; i<length;i++) {
    if(payload[i] == 'L') {
      ++cnt;
    } else if(cnt == 1 && payload[i] == 'e') {
      ++cnt;
    }else if(cnt == 2 && payload[i] == 'd') {
      if(payload[i+12] == 49) {
        led = 1;
      }
      break;
    } else {
      cnt = 0;
    }
  }

  Serial.printf("Received bike LED: %d\n", led);

  if(led == 1) { 
    digitalWrite(ledPin, HIGH); 
    Serial.println("Bike unlocked");
    delay(10000);
    digitalWrite(ledPin, LOW);
    led = 0;
    Serial.println("Bike locked"); 
    publish();
  }
}

void setup() {
  Serial.begin(115200);
  pinMode(trigPin, OUTPUT);
  pinMode(echoPin, INPUT);
  pinMode(ledPin, OUTPUT);
  digitalWrite(ledPin, LOW); 

  sntp_set_time_sync_notification_cb( timeavailable );
  sntp_servermode_dhcp(1);  
  configTime(gmtOffset_sec, daylightOffset_sec, ntpServer1, ntpServer2);

  connectToWiFi();
  mqttClient.setServer(mqttServer, mqttPort);
  mqttClient.setCallback(callback);
  connectToMQTT();
  delay(5000);

  publish();
}

void loop() {
  if (!mqttClient.connected()) {
    connectToMQTT();
  }
  mqttClient.loop(); // Maintain  MQTT communication
}