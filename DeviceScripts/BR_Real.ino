#include <TimeLib.h>
#include <WiFi.h>
#include <PubSubClient.h>
#include <ArduinoJson.h>

// WiFi credentials
const char* ssid = "ssid";
const char* password = "pass";

// MQTT broker details
const char* mqttServer = "161.53.19.19";
const int mqttPort = 56883;
const char* mqttTopic = "json/BR_Real/IoTGrupa10";

// HC-SR04 Ultrasonic Sensor
const int trigPin = 4;
const int echoPin = 5;

// External LED
const int ledPin = 3;

WiFiClient wifiClient;
PubSubClient mqttClient(wifiClient);

float longitude = 16.440193;
float latitude =  43.508133;

float readDistance();
int extractBikeRentalLED(char* payload);
void connectToWiFi();
void connectToMQTT();
void publish(float distance, int led);
void callback(char* topic, byte* payload, unsigned int length);

void callback(char* topic, byte* payload, unsigned int length) {
  Serial.println("Starting callback");
  char* payloadString = (char*)payload;
  payloadString[length] = '\0';

  int led = extractBikeRentalLED(payloadString);

  Serial.printf("Got bike rental led: %d", led);
  if (led == 1) {
    digitalWrite(ledPin, HIGH);

    unsigned long startTime = millis();
    while (millis() - startTime < 10000) {
      int newDistance = readDistance(); 
      if (newDistance > 5.0 ) {
        publish(newDistance, 0);
      }
      delay(100); 
    }
    publish(readDistance(), 0);
  }
}

float readDistance() {
  // Trigger the ultrasonic sensor
  digitalWrite(trigPin, LOW);
  delayMicroseconds(2);
  digitalWrite(trigPin, HIGH);
  delayMicroseconds(10);
  digitalWrite(trigPin, LOW);

  // Read the echo duration
  long duration = pulseIn(echoPin, HIGH);
  // Calculate the distance based on the speed of sound
  float distance = duration * 0.034 / 2;

  return distance;
}

int extractBikeRentalLED(char* payload) {
  DynamicJsonDocument doc(512);
  DeserializationError error = deserializeJson(doc, payload);
  int value = -1;

  if (error) {
    Serial.print("Error: ");
    Serial.println(error.c_str());
  } else {
    if (doc["contentNodes"]["source"]["resourceSpec"] == "BikeRentalLed") {
      value = (int)doc["contentNodes"]["value"];
    }
  }
  return value;
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
  mqttClient.setCallback(callback);
  mqttClient.setServer(mqttServer, mqttPort);
  while (!mqttClient.connected()) {
    Serial.println("Connecting to MQTT...");
    if (mqttClient.connect("clientID")) {
      Serial.println("Connected to MQTT");
      bool result = mqttClient.subscribe("all/BR_Real");
      Serial.printf("Subscribed: %d\n", result);
    } else {
      Serial.print("Failed to connect to MQTT, rc=");
      Serial.println(mqttClient.state());
      delay(2000);
    }
  }
}

void publish(float distance, int led) {
    time_t timestamp = now();
    char msg[300];
    snprintf(msg, sizeof(msg),
        "\"header\":{\"timeStamp\":%ld},"
        "\"body\":{"
        "\"BikeRentalActuator\":{\"BikeRentalLed\":%d},"
        "\"BikeRentalHC-SR04\":{\"BikeRentalDistance\":%.2f},"
        "\"BikeRentalGPS\":{\"BikeRentalLatitude\":%f,\"BikeRentalLongitude\":%f}}}", 
          timestamp, led, distance, latitude, longitude);

    Serial.printf(msg);
    Serial.print("\n");

    // Publish the message
    bool status = mqttClient.publish(mqttTopic, msg);

    // Check the publish status
    if (status) {
      Serial.printf("Sending `%s` to topic `%s`\n", msg, mqttTopic);
    } else {
      Serial.printf("Failed to send message to topic %s.", mqttTopic);
    }
}

void setup() {
  Serial.begin(115200);
  pinMode(trigPin, OUTPUT);
  pinMode(echoPin, INPUT);
  pinMode(ledPin, OUTPUT);
  digitalWrite(ledPin, LOW); 

  connectToWiFi();
  connectToMQTT();
  delay(2000);
}

void reconnect() {
  while (!mqttClient.connected()) {
    Serial.println("Connecting to MQTT...");
    if (mqttClient.connect("clientID")) {
      Serial.println("Connected to MQTT");
      bool result = mqttClient.subscribe("all/BR_Real");
      Serial.printf("Subscribed: %d\n", result);
    } else {
      Serial.print("Failed to connect to MQTT, rc=");
      Serial.println(mqttClient.state());
      delay(2000);
    }
  }
}

bool send = true; // for testing
void loop() {
  if (!mqttClient.connected()) {
    reconnect();
  }
  mqttClient.loop(); // Maintain  MQTT communication
  float distance = readDistance();

  if(send) {  // testing publish
    send = false;
    publish(distance, 0);
  }

  if (distance < 0.5) {
    digitalWrite(ledPin, LOW); 
  } else {
    digitalWrite(ledPin, HIGH); 
  }

  // Serial.printf("Distance: %.2f cm\n", distance);

  delay(1000);
}