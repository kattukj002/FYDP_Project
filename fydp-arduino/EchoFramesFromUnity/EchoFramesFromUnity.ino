//
const int header_len = 1;
const int msg_len = 6;
byte frameHeader[header_len] = {0xC0};
byte msg[msg_len];
int curr_header_idx = 0;

void setup(){
  Serial.begin(9600);
  
  pinMode(LED_BUILTIN, OUTPUT);
}
void loop(){
  byte readChar = Serial.read();
  if (frameHeader[curr_header_idx] == readChar) {
    curr_header_idx += 1;
  } else {
    curr_header_idx = 0;
    Serial.print("No match, got ");
    Serial.print((int)readChar);
    Serial.print('\n');
  }

  if (curr_header_idx == 2) {
    curr_header_idx = 0;
    Serial.readBytesUntil((char)0xC0, msg, msg_len);

    for(int i = 0; i < msg_len; ++i){
      Serial.print(msg[i]);
      Serial.print(" ");
    }
    Serial.print("\n");
    
    Serial.flush();
  }
}
