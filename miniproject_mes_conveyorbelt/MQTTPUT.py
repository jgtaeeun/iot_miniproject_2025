## 파이썬 Mqtt Publish
# paho-mqtt 라이브러리설치
# pip install paho-mqtt

import paho.mqtt.client as mqtt
import json
import datetime as dt
import uuid
from collections import OrderedDict
import random
import time   #스레드 위한 timer


PUB_ID = 'IOT01'                  
BROKER = '192.168.0.2'            # 브로커 IP (자기 컴퓨터)
PORT = 1883                          # MQTT 기본 포트
TOPIC = 'pknu/sf52/data'        # MQTT 토픽
COUNT = 0                            # 메시지 카운터
STATE = ['OK', 'FAIL']
ISON = False

#브로커 연결 성공 시 실행됨
def on_connect(client, userdata, flags, reason_code, properties = None):
    print(f'Connectedc with reason code : {reason_code}')

#publish완료후 발생 콜백 /메시지 발행 완료 시 실행됨
def on_publish(client, userdata,mid) :
    print(f'Message published mid : {mid}')


try :
    
    #MQTT 클라이언트 생성 후 콜백 등록
    client = mqtt.Client(client_id=PUB_ID , protocol=mqtt.MQTTv5 )
    client.on_connect = on_connect
    client.on_publish = on_publish

    client.connect(BROKER ,PORT)
    client.loop_start()

    while True :
        #publish실행
        #매초마다 랜덤 색상을 선택하고, 타임스탬프와 함께 메시지 발행
        #qos=1 : 적어도 한 번은 전송됨 (보장성 있는 전달)

        currTime = dt.datetime.now()
        if ISON == False :
              selected = 'START'
              ISON = True
        else :
            selected = random.choice(STATE)
            ISON = False

        COUNT += 1

        message = {
            "LineNumber": COUNT,
            "ClientID": PUB_ID,
            "Timestamp": currTime.strftime('%Y-%m-%d %H:%M:%S.%f'),
            "Result": selected
        }

        # JSON 문자열로 변환
        payload = json.dumps(message)


        client.publish(TOPIC, payload =  payload , qos = 1)
        time.sleep(1)

except Exception as ex:
    print(f"Error raised: {ex}")

#Ctrl + C 누르면 안전하게 전송을 중단하고 연결 종료
except KeyboardInterrupt :
    print('MQTT 전송중단')
    client.loop_stop()
    client.disconnect()