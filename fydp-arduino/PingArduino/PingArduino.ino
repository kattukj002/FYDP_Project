int ledState = 0;

void setup(){
  Serial.begin(9600);
  pinMode(LED_BUILTIN, OUTPUT);
}
void loop(){
  ledState = recvSerial();
  delay(10);
  if (ledState == 1){
    digitalWrite(LED_BUILTIN, HIGH);
  } else {
    digitalWrite(LED_BUILTIN, LOW);
  }
}

int recvSerial() {
  if(Serial.available()){
    int serialData = Serial.read();
    switch (serialData) {
      case '1':
        return 1;
        break;
      default:
        return 0;     
     }
  }
  return 0;
}
