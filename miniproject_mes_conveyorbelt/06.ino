// === Pins ===
const int IR_LEFT  = A0;   // 입구 쪽 IR (LOW = 감지)
const int IR_RIGHT = A1;   // 출구 쪽 IR (LOW = 감지)

const int MOTOR_PWM  = 10; // 모터 속도(PWM)
const int MOTOR_DIR  = 12; // 모터 방향

// === Params ===
const int RUN_SPEED = 90;     // 벨트 기본 속도(0~255)
const unsigned long SAFETY_MS = 15000UL; // 안전 정지 타임아웃(선택)

// === State ===
enum { IDLE, RUNNING } state = IDLE;
unsigned long startedAt = 0;

// 이전 센서 상태(엣지 감지용)
bool prevLeft = false;
bool prevRight = false;

void startConveyor() {
  digitalWrite(MOTOR_DIR, HIGH);   // 결선에 따라 전/후진 맞게 설정
  analogWrite(MOTOR_PWM, RUN_SPEED);
  startedAt = millis();
  Serial.println("CONVEYOR: START");
}

void stopConveyor() {
  analogWrite(MOTOR_PWM, 0);
  Serial.println("CONVEYOR: STOP");
}

void setup() {
  Serial.begin(9600);

  // 센서 모듈이 오픈컬렉터/단순 포토인터럽터라면 내부 풀업 사용
  // (시중의 IR 모듈 대부분은 보드에 풀업/컴퍼레이터가 있어 INPUT만으로도 동작함)
  pinMode(IR_LEFT,  INPUT);        // 필요 시 INPUT_PULLUP 으로 바꿔도 됨
  pinMode(IR_RIGHT, INPUT);

  pinMode(MOTOR_DIR, OUTPUT);
  stopConveyor();                  // 시작은 정지 상태
  Serial.println("arduino starts");
}

void loop() {
  // 현재 감지값 (LOW-Active)
  bool left  = (digitalRead(IR_LEFT)  == LOW);
  bool right = (digitalRead(IR_RIGHT) == LOW);

  // LOW로 막 바뀐 순간만 true (엣지)
  bool leftEdge  = left  && !prevLeft;
  bool rightEdge = right && !prevRight;

  switch (state) {
    case IDLE:
      if (leftEdge) {              // 왼쪽 센서에 '처음' 들어오면
        startConveyor();
        state = RUNNING;
      }
      break;

    case RUNNING:
      if (rightEdge) {             // 오른쪽 센서에 '처음' 도달하면
        stopConveyor();
        state = IDLE;
      }
      // 안전 타임아웃(옵션): 너무 오래 달리면 정지
      if (SAFETY_MS && (millis() - startedAt > SAFETY_MS)) {
        Serial.println("TIMEOUT -> STOP");
        stopConveyor();
        state = IDLE;
      }
      break;
  }

  // 다음 루프를 위한 이전 값 갱신
  prevLeft  = left;
  prevRight = right;

  delay(5); // 아주 짧게 쉼(노이즈 완화)
}