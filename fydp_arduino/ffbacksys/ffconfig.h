#ifndef FFCONFIG_H_
#define FFCONFIG_H_

#define FF_DAISY_MODE

#define FF_BAUD 115200

#define NDRIVERS 3

#define NDSPINS NDRIVERS

#ifndef FF_DAISY_MODE
    #define NDSPINS 1
#endif

#define CS0 52
#define CS1 10
#define CS2 4

#define RESET0_PIN 13
#define RESET1_PIN 12
#define RESET2_PIN 11

#define DSPIN_SHOULDER0 0
#define DSPIN_SHOULDER1 1
#define DSPIN_ELBOW     2

#define POT0_PIN 0
#define POT1_PIN 1
#define POT2_PIN 2

#define ENC0_CK_PIN 32
#define ENC0_DT_PIN 30
#define ENC1_CK_PIN 28
#define ENC1_DT_PIN 26
#define ENC2_CK_PIN 24
#define ENC2_DT_PIN 22

#define FF_I_FACTOR  32
#define FF_I_LIMIT 2.8f

#define FF_MOTOR_SPEED 550
#define FF_MOTOR_SHOULDER_SPEED 400
#define FF_MOTOR_SHOULDER_REEL_SPEED 500



#endif