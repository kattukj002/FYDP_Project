#ifndef _FORCEFEEDBACK_H
#define _FORCEFEEDBACK_H
#include <stdlib.h>
#include "ffconfig.h"
#include "dSPIN.h"

#define FF_I2B(_I) ((uint8_t)(_I*FF_I_FACTOR))
#define FF_LOAD_MAX FF_I2B(FF_I_LIMIT)

typedef struct ff_com_motor{
    uint8_t cmd;
    uint8_t val;
} ff_com_motor_t;

typedef struct ff_com_rx_payload{
    uint16_t header;
    ff_com_motor_t motor[3];
} ff_com_rx_payload_t;

typedef struct ff_com_tx_payload{
    uint16_t header;
    uint16_t adc_data[3];
    uint8_t encoder_data[3];
    uint8_t other_dat;
    int16_t accel_X;
    int16_t accel_Y;
    int16_t accel_Z;
    int16_t gyro_X;
    int16_t gyro_Y;
    int16_t gyro_Z;
} ff_com_tx_payload_t;

typedef enum{
    FF_MOTOR_NOP = 0,
    FF_MOTOR_HIZ,
    FF_MOTOR_RUN_FWD,
    FF_MOTOR_RUN_REV,
    FF_MOTOR_HOLD,
    FF_MOTOR_MOV_FWD,
    FF_MOTOR_MOV_REV,   
    FF_MOTOR_HOLD_LOAD_SET,
    FF_MOTOR_RUN_LOAD_SET,
    FF_MOTOR_ACC_LOAD_SET,
    FF_MOTOR_DEC_LOAD_SET,
    FF_MOTOR_GOTO,
    FF_MOTOR_TRQ_FWD = FF_MOTOR_RUN_FWD,
    FF_MOTOR_TRQ_REV = FF_MOTOR_RUN_REV,
    FF_MOTOR_TRQ_HOLD = FF_MOTOR_HOLD,
    FF_MOTOR_SHOULD_REEL = 22,
} ff_motor_cmd_t;

void ff_proc_cmd(ff_com_rx_payload_t data, dSPIN dspin);
void ff_proc_cmd(ff_com_rx_payload_t data, dSPIN * dspin);
void ff_proc_motor_cmd(ff_com_motor_t cmd, dSPIN dspin, uint8_t index);



#define FF_COM_HEADER 0xC0C0


#endif