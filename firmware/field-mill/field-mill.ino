/**
 * NOA ReACT - Field Mill
 */

// Uncomment SERIAL_BINARY to use binary format when sending data through the UART.
// Uncomment SERIAL_TEXT to send data in text format through the UART.
// Use SERIAL_BINARY when chaining to a space mill sensor. Do not uncomment both!
#define SERIAL_BINARY
// #define SERIAL_TEXT

#include <Servo.h>
#include <Wire.h>

// MPU6050 libraries
#include "I2Cdev.h"
#include "MPU6050_6Axis_MotionApps20.h"


/** Servo motor **/
Servo myservo;

/** MPU: Device, buffers, angles */
MPU6050 mpu;
uint16_t mpuPacketSize;
uint16_t mpuFifoCount;
uint8_t mpuFifoBuffer[64];

Quaternion q;
VectorFloat gravity;
float ypr[3];

/** Interrupt handler for MPU */
volatile bool mpuInterrupt = false;
void dmpDataReady() {
    mpuInterrupt = true;
}

/**
 * Initializes the MPU6050 gyro through I2C
 */
void setupMPU6050()
{
  Wire.begin();
  mpu.initialize();
  pinMode(2, INPUT);

  int status = mpu.dmpInitialize();
  mpu.setXGyroOffset(220);
  mpu.setYGyroOffset(76);
  mpu.setZGyroOffset(-85);
  mpu.setZAccelOffset(1688);

  mpu.CalibrateAccel(6);
  mpu.CalibrateGyro(6);

  mpu.setDMPEnabled(true);

  attachInterrupt(digitalPinToInterrupt(2), dmpDataReady, RISING);
  mpuPacketSize = mpu.dmpGetFIFOPacketSize();
}

int getAnalogPeakToPeak()
{
  int tmp = analogRead(0);
  int min = tmp;
  int max = tmp;

  for (int i = 0; i < 9500; i++)
  {
    tmp = analogRead(0);
    if (tmp > max)
    {
      max = tmp;
    }
    if (tmp < min)
    {
      min = tmp;
    }
  }

  return max - min;
}

void setup()
{
  // XData UART interface: 9600baud rate, 8N1
  Serial.begin(9600);

  // Initialize LED
  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, HIGH);

  // Gyro initialization
  setupMPU6050();

  // Servo initialization
  myservo.attach(10);
  delay(2000);
  myservo.write(20); // Speed Controller Startup PWM
  delay(2000);
  myservo.write(120); // Normal Spin
  myservo.write(120); // Normal Spin

  // Flash LED to indicate that initialization is complete
  digitalWrite(LED_BUILTIN, HIGH);
  delay(500);
  digitalWrite(LED_BUILTIN, LOW);
  delay(200);
  digitalWrite(LED_BUILTIN, HIGH);
  delay(500);
  digitalWrite(LED_BUILTIN, LOW);
}

#ifdef SERIAL_BINARY
void sendSerial(int counts)
{
  Serial.write('N');
  Serial.write('O');
  Serial.write('A');

  Serial.write(0xFF & (counts >> 8));
  Serial.write(0xFF & (counts));

  int rollInt = (ypr[2] * 180/M_PI) * 100;
  Serial.write(0xFF & (rollInt >> 8));
  Serial.write(0xFF & (rollInt));

  int pitchInt = (ypr[1] * 180/M_PI) * 100;
  Serial.write(0xFF & (pitchInt >> 8));
  Serial.write(0xFF & (pitchInt));

  int yawInt = (ypr[0] * 180/M_PI) * 100;
  Serial.write(0xFF & (yawInt >> 8));
  Serial.write(0xFF & (yawInt));

  Serial.write(0x04); // End-of-transmission
}
#endif

#ifdef SERIAL_TEXT
void sendSerial(int counts)
{
  Serial.print("counts: ");
  Serial.print(counts);

  Serial.print(", roll: ");
  Serial.print(ypr[2]);

  Serial.print(", pitch: ");
  Serial.print(ypr[1]);

  Serial.print(", yaw: ");
  Serial.println(ypr[0]);
}
#endif

void loop()
{
  // Take a field mill reading
  int millCounts = getAnalogPeakToPeak();

  // Get orientation from the MPU6050
  mpu.dmpGetCurrentFIFOPacket(mpuFifoBuffer);
  mpu.dmpGetQuaternion(&q, mpuFifoBuffer);
  mpu.dmpGetGravity(&gravity, &q);
  mpu.dmpGetYawPitchRoll(ypr, &q, &gravity);

  // Transmit the measurements through the UART interface to the space charge sensor
  sendSerial(millCounts);
}
