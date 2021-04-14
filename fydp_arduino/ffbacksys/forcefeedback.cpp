#include "ffconfig.h"
#include "forcefeedback.h"

void ff_proc_motor_cmd(ff_com_motor_t cmd, dSPIN dspin, uint8_t index){
    uint8_t load = 0;
    uint16_t motor_speed = (index == 1) ? FF_MOTOR_SHOULDER_SPEED:FF_MOTOR_SPEED;
    switch((ff_motor_cmd_t) cmd.cmd){
        case FF_MOTOR_NOP:
        break;
        case FF_MOTOR_RUN_FWD:
            load = min(cmd.val, FF_LOAD_MAX);
            if (index == 1){dspin.setRunTVAL(load+10, index);
            }
            else{
                dspin.setRunTVAL(load, index);
            }
            
            dspin.run(FWD, motor_speed, index);
        break;
        case FF_MOTOR_RUN_REV:
            load = min(cmd.val, FF_LOAD_MAX);
            dspin.setRunTVAL(load, index);
            dspin.run(REV, motor_speed, index);
        break;
        case FF_MOTOR_MOV_FWD:
            load = min(cmd.val, FF_LOAD_MAX);
            dspin.setRunTVAL(load, index);
            dspin.move(FWD, cmd.val, index);
        break;
        case FF_MOTOR_MOV_REV:
            load = min(cmd.val, FF_LOAD_MAX);
            dspin.setRunTVAL(load, index);
            dspin.move(REV, cmd.val, index);
        break;
        case FF_MOTOR_HOLD:
            load = min(cmd.val, FF_LOAD_MAX);
            dspin.setHoldTVAL(load, index);
            dspin.hardStop(index);
        break;
        case FF_MOTOR_HIZ:
            dspin.hardHiZ(index);
        break;
        case FF_MOTOR_HOLD_LOAD_SET:
            load = min(cmd.val, FF_LOAD_MAX);
            dspin.setHoldTVAL(load, index);
        break;
        case FF_MOTOR_RUN_LOAD_SET:
            load = min(cmd.val, FF_LOAD_MAX);
            dspin.setRunTVAL(load, index);
        break;
        case FF_MOTOR_ACC_LOAD_SET:
            load = min(cmd.val, FF_LOAD_MAX);
            dspin.setAccTVAL(load, index);           
        break;
        case FF_MOTOR_DEC_LOAD_SET:
            load = min(cmd.val, FF_LOAD_MAX);
            dspin.setDecTVAL(load, index); 
        break;
        case FF_MOTOR_GOTO:
        break;
        case FF_MOTOR_SHOULD_REEL:
                dspin.setRunTVAL(20, index);
                dspin.run(FWD, FF_MOTOR_SHOULDER_REEL_SPEED, index);
                break;
    }
}

// for non daisy chain
void ff_proc_cmd(ff_com_rx_payload_t data, dSPIN * dspin){
     if(data.header == FF_COM_HEADER){
        for (uint8_t i = 0; i < NDRIVERS; i ++){
            ff_proc_motor_cmd(data.motor[i], dspin[i], i);
        }
    }

}


// for daisy chain
void ff_proc_cmd(ff_com_rx_payload_t data, dSPIN dspin){
    if(data.header == FF_COM_HEADER){
        for (uint8_t i = 0; i < NDRIVERS; i ++){
            ff_proc_motor_cmd(data.motor[i], dspin, i);
        }
    }

}