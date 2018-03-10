/***************************************************
 Largely based off the Adafruit library code
 written by Limor Fried/Ladyada for Adafruit Industries.
 BSD license, all text above must be included in any redistribution
****************************************************/

#include <stdint.h>
#include <math.h>
#include <Wire.h>
#include "I2C_16.h"
#include "TMP006.h"

uint8_t sensor1 = 0x40;                 // I2C address of TMP006, can be 0x40-0x47
uint16_t samples = TMP006_CFG_16SAMPLE; // # of samples per reading, can be 1/2/4/8/16

void setup()
{
  Serial.begin(9600);
  config_TMP006(sensor1, samples);
}

void loop()
{
  float object_temp = readObjTempC(sensor1);
  Serial.println(object_temp);
  delay(4000); // delay 1 second for every 4 samples per reading
}
