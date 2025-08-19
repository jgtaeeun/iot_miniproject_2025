// ================== 핀 설정 ==================
/*** IR 센서 & 모터 ***/
const int IR_LEFT  = A0;   // 입구 IR (LOW = 감지)
const int IR_RIGHT = A1;   // 출구 IR (LOW = 감지)
const int MOTOR_PWM  = 10; // 모터 속도(PWM)
const int MOTOR_DIR  = 12; // 모터 방향

/*** TCS3200 (사용자 배선 반영: S2=8) ***/
#define S0 2
#define S1 3
#define S2 8
#define S3 5
#define OUT_PIN 6

// ================== 파라미터 ==================
const int RUN_SPEED = 90;                 // 0~255
const unsigned long SAFETY_MS = 15000UL;  // 안전 타임아웃(옵션)
const unsigned long WAIT_BEFORE_SENSE = 1000UL; // 도착 후 대기 1s
const int SENSE_COUNT = 3;                // 측정 3회(다수결)

// 컬러 분류 임계치(환경에 맞게 살짝 조정 가능)
const unsigned long DARK_US = 900;        // min 주기가 이 값보다 크면 너무 어두움(Black/미접촉)
const int SAME_PCT = 12;                  // RGB 주기가 서로 12% 이내면 White/Gray

// ================== 상태 ==================
enum State { IDLE, RUNNING, COLOR_WAIT, COLOR_MEASURE } state = IDLE;
unsigned long startedAt = 0;              // 벨트 시작 시각
unsigned long senseStartAt = 0;           // 도착 후 대기 시작 시각

// 에지 검출용
bool prevLeft  = false;
bool prevRight = false;

// 다수결/측정 카운트
int sampleDone = 0;
int votes[6] = {0,0,0,0,0,0}; // 0:Unknown,1:R,2:G,3:B,4:White/Gray,5:Black

// ================== 유틸 ==================
void startConveyor() {
  digitalWrite(MOTOR_DIR, HIGH);     // 결선 방향에 맞게 필요시 LOW로
  analogWrite(MOTOR_PWM, RUN_SPEED);
  startedAt = millis();
  Serial.println("CONVEYOR: START");
}

void stopConveyor() {
  analogWrite(MOTOR_PWM, 0);
  Serial.println("CONVEYOR: STOP");
}

// --- TCS3200: 20% 스케일 ---
void setScale20() { digitalWrite(S0, HIGH); digitalWrite(S1, LOW); }

// 중앙값 취득(노이즈 완화)
unsigned long readColorPeriod(bool s2, bool s3) {
  digitalWrite(S2, s2);
  digitalWrite(S3, s3);
  delay(40); // 필터 안정화

  const int N=7;
  unsigned long v[N];
  for (int i=0;i<N;i++){
    unsigned long p = pulseIn(OUT_PIN, LOW, 30000UL); // μs
    v[i] = p ? p : 30000UL; // 타임아웃이면 어두움으로 간주
  }
  // 선택정렬로 중앙값
  for (int i=0;i<N-1;i++){
    int m=i; for(int j=i+1;j<N;j++) if(v[j]<v[m]) m=j;
    unsigned long t=v[i]; v[i]=v[m]; v[m]=t;
  }
  return v[N/2];
}

// 한 번 측정해서 분류
// return: 0 Unknown, 1 R/Orange, 2 G, 3 B, 4 White/Gray, 5 Black/No obj
int classifyOnce(unsigned long &pR, unsigned long &pG, unsigned long &pB) {
  pR = readColorPeriod(LOW,  LOW);   // Red
  pB = readColorPeriod(LOW,  HIGH);  // Blue
  pG = readColorPeriod(HIGH, HIGH);  // Green

  unsigned long minP = min(pR, min(pG, pB));
  unsigned long maxP = max(pR, max(pG, pB));
  unsigned long spread = maxP - minP;

  bool tooDark = (minP > DARK_US);
  bool nearlySame = (spread < max(15UL, (minP * SAME_PCT) / 100UL));

  if (tooDark) return 5;            // Black/No object
  if (nearlySame) return 4;         // White/Gray
  if (minP == pR) return 1;         // Red/Orange
  if (minP == pG) return 2;         // Green
  return 3;                         // Blue
}

const char* labelName(int code) {
  static const char* L[] = {
    "Unknown","Red/Orange","Green","Blue","White/Gray","Black/No object"
  };
  return L[ (code>=0 && code<=5) ? code : 0 ];
}

// ================== SETUP ==================
void setup() {
  Serial.begin(115200);
  Serial.println("System boot");

  // IR
  pinMode(IR_LEFT,  INPUT);      // 필요시 INPUT_PULLUP
  pinMode(IR_RIGHT, INPUT);

  // 모터
  pinMode(MOTOR_DIR, OUTPUT);
  pinMode(MOTOR_PWM, OUTPUT);
  stopConveyor();

  // TCS3200
  pinMode(S0, OUTPUT); pinMode(S1, OUTPUT);
  pinMode(S2, OUTPUT); pinMode(S3, OUTPUT);
  pinMode(OUT_PIN, INPUT);
  setScale20();                  // 20% 스케일
}

// ================== LOOP ==================
void loop() {
  // IR 현재값(LOW active)
  bool left  = (digitalRead(IR_LEFT)  == LOW);
  bool right = (digitalRead(IR_RIGHT) == LOW);

  // 에지 검출
  bool leftEdge  = left  && !prevLeft;
  bool rightEdge = right && !prevRight;

  switch (state) {
    case IDLE:
      if (leftEdge) {
        startConveyor();
        state = RUNNING;
      }
      break;

    case RUNNING:
      if (rightEdge) {
        // 도착: 컨베이어 정지 후 1초 대기 → 컬러 측정
        stopConveyor();
        senseStartAt = millis();
        sampleDone = 0;
        for (int i=0;i<6;i++) votes[i]=0;
        state = COLOR_WAIT;
        Serial.println("ARRIVED: wait 1s, then color sensing x3");
      }
      if (SAFETY_MS && (millis() - startedAt > SAFETY_MS)) {
        Serial.println("TIMEOUT -> STOP");
        stopConveyor();
        state = IDLE;
      }
      break;

    case COLOR_WAIT:
      if (millis() - senseStartAt >= WAIT_BEFORE_SENSE) {
        state = COLOR_MEASURE;
      }
      break;

    case COLOR_MEASURE: {
      if (sampleDone < SENSE_COUNT) {
        unsigned long pR,pG,pB;
        int code = classifyOnce(pR,pG,pB);
        votes[code]++;

        Serial.print("Sample "); Serial.print(sampleDone+1);
        Serial.print("  Period(us) R:"); Serial.print(pR);
        Serial.print(" G:"); Serial.print(pG);
        Serial.print(" B:"); Serial.print(pB);
        Serial.print("  -> "); Serial.println(labelName(code));

        sampleDone++;
      } else {
        // 다수결
        int bestCode = 0, bestCnt = -1;
        for (int i=0;i<6;i++){
          if (votes[i] > bestCnt) { bestCnt = votes[i]; bestCode = i; }
        }
        Serial.print("FINAL COLOR: ");
        Serial.println(labelName(bestCode));

        // 여기서 분기 동작/서보/릴레이 등 후속 액션 넣기 가능
        // 예) if (bestCode==3) ... 블루 처리

        state = IDLE;  // 다음 물체 대기
        Serial.println("READY (IDLE)");
      }
      break;
    }
  }

  prevLeft  = left;
  prevRight = right;

  delay(5);
}