const int header_len = 2;
const int msg_len = 5;

typedef struct out_frame {
  byte header[header_len] = {0xC0, 0xC0};
  int16_t msg[msg_len];
} out_frame_t;

out_frame_t out_frame;

void setup(){
  Serial.begin(115200);
  
  for (int i = 0; i < msg_len; i++) {
    out_frame.msg[i] = i + 1;
  }
}
void loop(){
  if (Serial.availableForWrite() >= sizeof(out_frame_t)) {
    Serial.write((uint8_t *)&out_frame, sizeof(out_frame_t));     
  }
  delay(100);
}
