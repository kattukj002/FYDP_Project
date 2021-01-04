void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  while(!Serial);
}

void loop() {
  // put your main code here, to run repeatedly:
  if (analogRead(A0) > 0){
    Serial.println('0');    
  } else {
    Serial.println('1');
  }
}
