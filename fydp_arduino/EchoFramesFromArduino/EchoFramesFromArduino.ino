const int header_len = 1;
const int msg_len = 3;
byte frameHeader[header_len] = {0xFF};
byte frame[header_len + msg_len];
int curr_header_idx = 0;

void setup(){
  Serial.begin(9600);
  
  for (int i = 0; i < header_len; i++) {
    frame[i] = frameHeader[i];
  }
  for (int i = header_len; i < header_len + msg_len; i++) {
    frame[i] = i;
  }
}
void loop(){
  if (Serial.availableForWrite() >= header_len + msg_len) {
    Serial.write(frame, header_len + msg_len);     
    Serial.write('\n');
  }
  delay(100);
}
