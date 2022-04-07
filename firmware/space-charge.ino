// Input pin
#define cellPin A0

// Calibration outputs
#define CALIBRATION_PIN_1 11
#define CALIBRATION_PIN_2 12

// ADC counts
int counts;

// millis() at the last calibration, used to switch states.
unsigned long lastCalibrationTime = 0;

// State machine for loop()
// 0 = measuring normally
// 1 = Calibration stage 1
// 2 = Calibration stage 2
byte state = 0;

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

/**
 * Sets the state (LOW, HIGH) of the two calibration pins
 * The pins are defined through the definitions CALIBRATION_PIN_1 and CALIBRATION_PIN_2
 *
 * @param pin1 State of pin1
 * @param pin2 State of pin2
 */
void setCalibration(byte pin1, byte pin2)
{
  digitalWrite(CALIBRATION_PIN_1, pin1);
  digitalWrite(CALIBRATION_PIN_2, pin2);
}

void uartInt(int v)
{
  char buffer[4];
  byte *p = reinterpret_cast<byte *>(&v);
  for (int i = 0; i < sizeof(v); i++)
  {
    sprintf(buffer, "%02X", (byte)p[i]);
    Serial.write(buffer[0]);
    Serial.write(buffer[1]);
  }
}

void uartByte(byte v)
{
  char buffer[4];
  byte *p = reinterpret_cast<byte *>(&v);
  for (int i = 0; i < sizeof(v); i++)
  {
    sprintf(buffer, "%02X", (byte)p[i]);
    Serial.write(buffer[0]);
    Serial.write(buffer[1]);
  }
}

void sendXDataSpaceCharge(int counts, byte state)
{
  Serial.print("xdata=01AB");
  uartInt(counts);
  uartByte(state);
  Serial.println();
  Serial.setTimeout(300);
}

void setup()
{
  Serial.begin(9600);
  while (!Serial)
    ;

  // Calibration output pins
  pinMode(12, OUTPUT);
  pinMode(13, OUTPUT);

  // Blinky sequence
  pinMode(13, OUTPUT);
  digitalWrite(13, HIGH);
  delay(1000);
  digitalWrite(13, LOW);
  delay(1000);
  digitalWrite(13, HIGH);
  delay(1000);
  digitalWrite(13, LOW);
  delay(500);
  digitalWrite(13, HIGH);
  delay(200);
  digitalWrite(13, LOW);
  delay(200);
  digitalWrite(13, HIGH);
  delay(200);
  digitalWrite(13, LOW);
  delay(200);
}

void loop()
{
  // Take a field mill measurement
  getAveragedCounts(cellPin, 1000, &counts);

  // Handle calibration
  switch (state)
  {
  case 0: // Wait for calibration
    // Calibration should be performed every 5 minutes
    if (millis() - lastCalibrationTime > (5 * 60 * 1000L))
    {
      state = 1;
      lastCalibrationTime = millis();
      setCalibration(HIGH, LOW);
    }
    break;
  case 1: // Calibration stage 1
    // If 10 seconds have passed, move to stage 2
    if (millis() - lastCalibrationTime > (10 * 1000))
    {
      state = 2;
      lastCalibrationTime = millis();
      setCalibration(LOW, HIGH);
    }
  case 2: // Calibration stage 2
    // If 10 seconds have passed, end calibration
    if (millis() - lastCalibrationTime > (10 * 1000))
    {
      state = 0;
      lastCalibrationTime = millis();
      setCalibration(LOW, LOW);
    }
  }

  // Send data through UART
  sendXDataSpaceCharge(counts, state);

  // Forward packet if available
  if (Serial.available() > 10)
  {
    String incoming = Serial.readStringUntil("\n");
    Serial.print(incoming);
  }

  delay(500);
}
