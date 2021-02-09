char out = '0';
void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  while(!Serial);
}

void loop() {
  // put your main code here, to run repeatedly:
   Serial.println(out);
   if (out == '0') {
      out = '1';    
   } else {
      out = '0';
   }
}
