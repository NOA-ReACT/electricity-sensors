/**
 * NOA ReACT - Field Mill
 */

#include <Servo.h>
#include <Wire.h>

/** Servo motor **/
Servo myservo;

/** acceleration vector */
float accX, accY, accZ;
/** Rotation vector */
float gyroX, gyroY, gyroZ;
float accAngleX, accAngleY, gyroAngleX, gyroAngleY, gyroAngleZ;
/** IMU orientation */
float roll, pitch, yaw;
/** Calibration values for IMU */
float accErrorX, accErrorY, gyroErrorX, gyroErrorY, gyroErrorZ;

float elapsedTime, currentTime, previousTime;
int c = 0;

// Counts from ADC/Field Mill
int millCounts = 0;

/**
 * Initializes the MPU6050 gyro through I2C
 */
void setupMPU6050(byte id)
{
  Wire.begin();
  Wire.beginTransmission(id);
  Wire.write(0x6B);
  Wire.write(0x00);
  Wire.endTransmission(true);
}

/**
 * Calculate accelerometer and gyroscope error values.
 * The IMU should be placed on a flat surface before calibration.
 *
 * The error is stored inside the `accError*` and `gyroError*` variables.
 *
 * @param id              The I2C bus ID for MPU6050.
 * @param printToSerial   If true, the error values will be printed to the serial port.
 */
void calibrateMPU6050(byte id, bool printToSerial)
{
  // Read accelerometer values 200 times
  byte c;
  for (c = 0; c < 200; c++)
  {
    Wire.beginTransmission(id);
    Wire.write(0x3B);
    Wire.endTransmission(false);
    Wire.requestFrom(id, 6, true);
    accX = (Wire.read() << 8 | Wire.read()) / 16384.0;
    accY = (Wire.read() << 8 | Wire.read()) / 16384.0;
    accZ = (Wire.read() << 8 | Wire.read()) / 16384.0;
    // Sum all readings
    accErrorX = accErrorX + ((atan((accY) / sqrt(pow((accX), 2) + pow((accZ), 2))) * 180 / PI));
    accErrorY = accErrorY + ((atan(-1 * (accX) / sqrt(pow((accY), 2) + pow((accZ), 2))) * 180 / PI));
  }

  // Divide the sum by 200 to get the error value
  accErrorX = accErrorX / 200;
  accErrorY = accErrorY / 200;

  // Read gyro values 200 times
  for (c = 0; c < 200; c++)
  {
    Wire.beginTransmission(id);
    Wire.write(0x43);
    Wire.endTransmission(false);
    Wire.requestFrom(id, 6, true);
    gyroX = Wire.read() << 8 | Wire.read();
    gyroY = Wire.read() << 8 | Wire.read();
    gyroZ = Wire.read() << 8 | Wire.read();
    // Sum all readings
    gyroErrorX = gyroErrorX + (gyroX / 131.0);
    gyroErrorY = gyroErrorY + (gyroY / 131.0);
    gyroErrorZ = gyroErrorZ + (gyroZ / 131.0);
    c++;
  }
  // Divide the sum by 200 to get the error value
  gyroErrorX = gyroErrorX / 200;
  gyroErrorY = gyroErrorY / 200;
  gyroErrorZ = gyroErrorZ / 200;

  // Print the error values on the Serial Monitor
  if (printToSerial)
  {
    Serial.print("accErrorX: ");
    Serial.println(accErrorX);
    Serial.print("accErrorY: ");
    Serial.println(accErrorY);
    Serial.print("gyroErrorX: ");
    Serial.println(gyroErrorX);
    Serial.print("gyroErrorY: ");
    Serial.println(gyroErrorY);
    Serial.print("gyroErrorZ: ");
    Serial.println(gyroErrorZ);
  }
}

void readMPU6050(byte id, float *roll, float *pitch, float *yaw)
{
  Wire.beginTransmission(id);
  Wire.write(0x3B); // Start with register 0x3B (ACCEL_XOUT_H)
  Wire.endTransmission(false);

  // Accelerometer data
  Wire.requestFrom(id, 6, true); // Read 6 registers total, each axis value is stored in 2 registers
  // For a range of +-2g, we need to divide the raw values by 16384, according to the datasheet
  accX = (Wire.read() << 8 | Wire.read()) / 16384.0; // X-axis value
  accY = (Wire.read() << 8 | Wire.read()) / 16384.0; // Y-axis value
  accZ = (Wire.read() << 8 | Wire.read()) / 16384.0; // Z-axis value

  // Calculating Roll and Pitch from the accelerometer data
  accAngleX = (atan(accY / sqrt(pow(accX, 2) + pow(accZ, 2))) * 180 / PI) - 0.58;      // accErrorX ~(0.58) See the calculate_IMU_error()custom function for more details
  accAngleY = (atan(-1 * accX / sqrt(pow(accY, 2) + pow(accZ, 2))) * 180 / PI) + 1.58; // accErrorY ~(-1.58)

  // Gyroscope data
  previousTime = currentTime;
  currentTime = millis();
  elapsedTime = (currentTime - previousTime) / 1000; // Divide by 1000 to get seconds
  Wire.beginTransmission(id);
  Wire.write(0x43); // Gyro data first register address 0x43
  Wire.endTransmission(false);
  Wire.requestFrom(id, 6, true);                    // Read 4 registers total, each axis value is stored in 2 registers
  gyroX = (Wire.read() << 8 | Wire.read()) / 131.0; // For a 250deg/s range we have to divide first the raw value by 131.0, according to the datasheet
  gyroY = (Wire.read() << 8 | Wire.read()) / 131.0;
  gyroZ = (Wire.read() << 8 | Wire.read()) / 131.0;
  // Correct the outputs with the calculated error values
  gyroX = gyroX + 0.56; // gyroErrorX ~(-0.56)
  gyroY = gyroY - 2;    // gyroErrorY ~(2)
  gyroZ = gyroZ + 0.79; // gyroErrorZ ~ (-0.8)
  // Currently the raw values are in degrees per seconds, deg/s, so we need to multiply by sendonds (s) to get the angle in degrees
  gyroAngleX = gyroAngleX + gyroX * elapsedTime; // deg/s * s = deg
  gyroAngleY = gyroAngleY + gyroY * elapsedTime;

  // Original code: &yaw = &yaw + gyroZ * elapsedTime;
  // Undefined original &yaw?
  *yaw = gyroZ * elapsedTime;

  // Complementary filter - combine acceleromter and gyro angle values
  *roll = 0.96 * gyroAngleX + 0.04 * accAngleX;
  *pitch = 0.96 * gyroAngleY + 0.04 * accAngleY;
}

/**
 * Get an average counts reading from the ADC
 *
 * @param port     Which ADC to read from
 * @param samples  How many samples to take
 * @param counts   Where to store the average counts
 */
void getAveragedCounts(int port, int samples, int *counts)
{
  long sum = 0;
  for (int i = 0; i < samples; i++)
  {
    sum += analogRead(port);
  }
  *counts = sum / samples;
}

void setup()
{
  while (!Serial)
    ;

  // XData UART interface: 9600baud rate, 8N1
  Serial.begin(9600);

  // Gyro initialization
  setupMPU6050(0x68);
  calibrateMPU6050(0x68, false);
  delay(20); // TODO Why?

  // Servo initialization
  // TODO What are we doing here
  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, HIGH);
  delay(2000);
  digitalWrite(LED_BUILTIN, LOW);

  myservo.attach(10);
  delay(2000);
  myservo.write(20); // Speed Controller Startup PWM
  delay(2000);
  myservo.write(120); // Normal Spin
  myservo.write(120); // Normal Spin

  // TODO What are we doing here
  digitalWrite(LED_BUILTIN, HIGH);
  delay(500);
  digitalWrite(LED_BUILTIN, LOW);
  delay(200);
  digitalWrite(LED_BUILTIN, HIGH);
  delay(500);
  digitalWrite(LED_BUILTIN, LOW);
}

void sendSerial(int counts, float roll, float pitch, float yaw)
{
  Serial.write('N');
  Serial.write('O');
  Serial.write('A');

  Serial.write(0xFF & (counts >> 8));
  Serial.write(0xFF & (counts));

  int rollInt = roll * 100;
  Serial.write(0xFF & (rollInt >> 8));
  Serial.write(0xFF & (rollInt));

  int pitchInt = pitch * 100;
  Serial.write(0xFF & (pitchInt >> 8));
  Serial.write(0xFF & (pitchInt));

  int yawInt = yaw * 100;
  Serial.write(0xFF & (yawInt >> 8));
  Serial.write(0xFF & (yawInt));

  Serial.write(0x04); // End-of-transmission
}

void loop()
{
  // Take a field mill reading
  getAveragedCounts(A0, 10, &millCounts);

  // Get orientation from the MPU6050
  readMPU6050(0x68, &roll, &pitch, &yaw);

  // Transmit the measurements through the UART interface to the space charge sensor
  sendSerial(millCounts, roll, pitch, yaw);

  delay(1000);
}
