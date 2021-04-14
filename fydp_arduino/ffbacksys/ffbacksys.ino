
#include "ffconfig.h"
#include "forcefeedback.h"
#include <SPI.h>
#include "dSPIN.h"
#include "GY521.h"

#define FF_USE_IMU

#ifdef FF_USE_IMU
GY521 imu(0x68);
static int16_t acceldata[3] = {0,0,0};
static int16_t gyrodata[3] = {0,0,0};
#endif

#ifdef FF_DEBUG
  #define FF_ONDEBUG(x) {x}
#else
  #define FF_ONDEBUG(x) while(false){}
#endif

//choose which port is being used
#ifdef FF_USE_USB_NATIVE
  #define FFSERIAL SerialUSB
#else
  #define FFSERIAL Serial
#endif

#define ANALOG_SMOOTH_RATE_BITS 3
#define ANALOG_NUM_SAMPS  (1<<(ANALOG_SMOOTH_RATE_BITS+1))

const int pot_pins[] = {POT0_PIN, POT1_PIN, POT2_PIN};
static uint16_t analogVals[3];
static uint16_t analog_samps[3][ANALOG_NUM_SAMPS];
static uint8_t analogIndex = 0;


static ff_com_rx_payload_t ff_rx;
static bool ff_rx_flag = false;



#ifdef FF_DAISY_MODE
  dSPIN dspin(CS0, RESET0_PIN);
#else
  dSPIN dspin[3] = {{CS0, RESET0_PIN},{CS1, RESET1_PIN},{CS1, RESET2_PIN}};
#endif


static uint8_t enc_val[3] = {0,0,0};
static volatile int enc_prev[3] = {0,0,0};
const int enc_ck_pins[] = {ENC0_CK_PIN, ENC1_CK_PIN, ENC2_CK_PIN};
const int enc_dt_pins[] = {ENC0_DT_PIN, ENC1_DT_PIN, ENC2_DT_PIN};

static unsigned long last_send = 0;
const int flag_pin = 5;

void initEncoders();
void initADC();
void initDrivers();
void initIMU();

void setup() {
  // put your setup code here, to run once:
  I2C_ClearBus();
  Serial.begin(FF_BAUD);
  //SerialUSB.begin(FF_BAUD);
  pinMode(flag_pin, INPUT);
  //Serial.println("Starting");
  initEncoders();
  initADC();
  #ifdef FF_USE_IMU
  initIMU();
  #endif
  initDrivers();
  Serial.flush();
  last_send = millis();
}



void loop() {
  unsigned long time = millis();
  if (!ff_rx_flag && time-last_send > 3000){
   for(uint8_t i = 0; i < NDRIVERS; i ++){
    Serial.println(dspin.getStatus(i),HEX);
    dspin.softHiZ(i);
  }   
  } 

  if(ff_rx_flag){
    ff_proc_cmd(ff_rx, dspin);
    ff_rx_flag = false;
  }

  ADC_update();
  //encoder_check();
  if(time-last_send > 5){
    readIMU();
    ff_send(analogVals, enc_val);
    last_send = millis();
  } 

}

void initDrivers(){
 #ifdef FF_DAISY_MODE
   dspin.setup();
   delay(10);
  for(uint8_t i = 0; i < NDRIVERS; i ++){
    Serial.println(dspin.getStatus(i),HEX);
    dspin.configStepMode(STEP_SEL_1_2,i);
    dspin.setHoldTVAL(10,i);
    dspin.setRunTVAL(20,i);
    dspin.setAccTVAL(35,i);
    dspin.setDecTVAL(10,i);
    dspin.setMaxSpeed(FF_MOTOR_SPEED*4,i);
    dspin.setAcc(300,i);
    dspin.softHiZ(i);
  }
 #else
  for(uint8_t i = 0; i < NDRIVERS; i ++){
    dspin[i].setup();
    dspin[i].getStatus(0);
    dspin[i].configStepMode(STEP_SEL_1_8);
    dspin[i].setHoldTVAL(FF_LOAD_MAX/2);
    dspin[i].setRunTVAL(FF_LOAD_MAX/2);
    dspin[i].setAccTVAL(FF_LOAD_MAX/2);
    dspin[i].setDecTVAL(FF_LOAD_MAX/2);
    dspin[i].setMaxSpeed(FF_MOTOR_SPEED);
    dspin[i].softHiZ();
  }
 #endif
}

void initADC(){
  analogReadResolution(12);
}

void ADC_update(){
  for(int i = 0; i < 3; i ++){
    analog_samps[i][analogIndex] = analogRead(pot_pins[i]);
  }
  analogIndex ++;
  if (analogIndex >= ANALOG_NUM_SAMPS) analogIndex = 0;
  for(int i = 0; i < 3; i ++){
    uint16_t temp = 0;
    for(int j = 0; j < ANALOG_NUM_SAMPS; j++){  
      temp += analog_samps[i][j];
    }
    analogVals[i] = analogVals[i] + ((analogRead(pot_pins[i]) - analogVals[i]) >> 2); //temp >> ANALOG_SMOOTH_RATE_BITS;
  }
}

#ifdef FF_USE_IMU
void initIMU(){
  delay(500);
  imu.begin();
  delay(50);
  imu.wakeup();
  imu.gxe = 0;
  imu.gye = 0;
  imu.gze = 0;
  Serial.println("IMU Connected!");
  imu.setAccelSensitivity(GY521_ACCEL_SENSE_4g);
  imu.setGyroSensitivity(1);
}

void readIMU(){
  imu.read();
  acceldata[0] = imu.getRawAccelX();
  acceldata[1] = imu.getRawAccelY();
  acceldata[2] = imu.getRawAccelZ();
  gyrodata[0] = imu.getRawGyroX();
  gyrodata[1] = imu.getRawGyroX();
  gyrodata[2] = imu.getRawGyroX();
}
#endif

void initEncoders(){
  pinMode(ENC0_CK_PIN, INPUT);
  pinMode(ENC0_DT_PIN, INPUT);
  pinMode(ENC1_CK_PIN, INPUT);
  pinMode(ENC1_DT_PIN, INPUT);
  pinMode(ENC2_CK_PIN, INPUT);
  pinMode(ENC2_DT_PIN, INPUT);
  delay(10);
  enc_prev[0] = digitalRead(ENC0_CK_PIN);
  enc_prev[1] = digitalRead(ENC1_CK_PIN);
  enc_prev[2] = digitalRead(ENC2_CK_PIN);
}

void encoder_check(){
  for(uint8_t i = 0; i < 3; i ++){
    if(enc_prev[i] == LOW && digitalRead(enc_ck_pins[i]) == HIGH){
          if(digitalRead(enc_dt_pins[i])){
            enc_val[i]++;
          }
          else{
            enc_val[i]--;
          }
          Serial.println(enc_val[i]);
    }
    enc_prev[i] = digitalRead(enc_ck_pins[i]);
  }
}
const uint8_t FUBAR[] = {0xc0,0xc0,0xFF,0xff,0xFF,0xff,0xFF,0xff,0xFF,0xff,0xFF,0xff,0xFF,0xff,0xFF,0xff,0xFF,0xff,0xFF,0xff,0xFF,0xff,0xFF,0xff};
void serialEvent(){
  if(Serial.available() >= sizeof(ff_com_rx_payload_t)){
    int temp = Serial.available();
    for(int i = 0; i < temp; i += 2){
      uint16_t value;
      Serial.readBytes((uint8_t*) &value, 2);
      if(value == FF_COM_HEADER) {ff_rx.header = FF_COM_HEADER; break;}
    }
    Serial.readBytes((uint8_t*) &ff_rx.motor[0], sizeof(ff_com_rx_payload_t)-2);
    FF_ONDEBUG({
      Serial.write((uint8_t*) &ff_rx, sizeof(ff_com_rx_payload_t));
    })
    if (ff_rx.header != FF_COM_HEADER){
        Serial.write(FUBAR,24);
    }
    //for (int i = 0; i < sizeof(sizeof(ff_com_rx_payload_t)); i ++){
    //SerialUSB.print(((uint8_t*) &ff_rx)[i],HEX);
    //}
    //SerialUSB.println("");
    ff_rx_flag = true;
  }
}


void ff_send(uint16_t * adcvals, uint8_t * encvals){
  ff_com_tx_payload_t send_dat;
  send_dat.header = FF_COM_HEADER;
  for(int i = 0; i < NDRIVERS; i ++){
    send_dat.adc_data[i] = adcvals[i];
    send_dat.encoder_data[i] = encvals[i];
  }
  send_dat.accel_X = acceldata[0];
  send_dat.accel_Y = acceldata[1];
  send_dat.accel_Z = acceldata[2];
  send_dat.gyro_X = gyrodata[0];
  send_dat.gyro_Y = gyrodata[1];
  send_dat.gyro_Z = gyrodata[2];
  Serial.write((uint8_t*) &send_dat, sizeof(ff_com_tx_payload_t));
}

#define SDA 20
#define SCL 21

int I2C_ClearBus() {
#if defined(TWCR) && defined(TWEN)
  TWCR &= ~(_BV(TWEN)); //Disable the Atmel 2-Wire interface so we can control the SDA and SCL pins directly
#endif
  pinMode(SDA, INPUT_PULLUP); // Make SDA (data) and SCL (clock) pins Inputs with pullup.
  pinMode(SCL, INPUT_PULLUP);
 
  delay(200);  
 
  boolean SCL_LOW = (digitalRead(SCL) == LOW); // Check is SCL is Low.
  if (SCL_LOW) { //If it is held low Arduno cannot become the I2C master. 
    return 1; //I2C bus error. Could not clear SCL clock line held low
  }
 
  boolean SDA_LOW = (digitalRead(SDA) == LOW);  // vi. Check SDA input.
  int clockCount = 20; // > 2x9 clock
 
  while (SDA_LOW && (clockCount > 0)) { //  vii. If SDA is Low,
    clockCount--;
  // Note: I2C bus is open collector so do NOT drive SCL or SDA high.
    pinMode(SCL, INPUT); // release SCL pullup so that when made output it will be LOW
    pinMode(SCL, OUTPUT); // then clock SCL Low
    delayMicroseconds(10); //  for >5uS
    pinMode(SCL, INPUT); // release SCL LOW
    pinMode(SCL, INPUT_PULLUP); // turn on pullup resistors again
    // do not force high as slave may be holding it low for clock stretching.
    delayMicroseconds(10); //  for >5uS
    // The >5uS is so that even the slowest I2C devices are handled.
    SCL_LOW = (digitalRead(SCL) == LOW); // Check if SCL is Low.
    int counter = 20;
    while (SCL_LOW && (counter > 0)) {  //  loop waiting for SCL to become High only wait 2sec.
      counter--;
      delay(100);
      SCL_LOW = (digitalRead(SCL) == LOW);
    }
    if (SCL_LOW) { // still low after 2 sec error
      return 2; // I2C bus error. Could not clear. SCL clock line held low by slave clock stretch for >2sec
    }
    SDA_LOW = (digitalRead(SDA) == LOW); //   and check SDA input again and loop
  }
  if (SDA_LOW) { // still low
    return 3; // I2C bus error. Could not clear. SDA data line held low
  }
 
  // else pull SDA line low for Start or Repeated Start
  pinMode(SDA, INPUT); // remove pullup.
  pinMode(SDA, OUTPUT);  // and then make it LOW i.e. send an I2C Start or Repeated start control.
  // When there is only one I2C master a Start or Repeat Start has the same function as a Stop and clears the bus.
  /// A Repeat Start is a Start occurring after a Start with no intervening Stop.
  delayMicroseconds(10); // wait >5uS
  pinMode(SDA, INPUT); // remove output low
  pinMode(SDA, INPUT_PULLUP); // and make SDA high i.e. send I2C STOP control.
  delayMicroseconds(10); // x. wait >5uS
  pinMode(SDA, INPUT); // and reset pins as tri-state inputs which is the default state on reset
  pinMode(SCL, INPUT);
  return 0; // all ok
}