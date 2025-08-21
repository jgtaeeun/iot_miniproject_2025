# 이미지 탐지 웹서비스
from fastapi import FastAPI, HTTPException, UploadFile, File, Form
from pydantic import BaseModel
import io
import base64
from PIL import Image, ImageDraw, ImageFont
import numpy as np
import uvicorn
import os


from ultralytics import YOLO
# import cv2   # OpenCV


# 현재 작업 디렉토리 확인
current_directory = os.getcwd()
print(current_directory)

app = FastAPI()

model = YOLO('yolov8n.pt')  # YOLOv8 pretrained model(웹상에 존재, 최초에 다운로드)

# 웹상으로 전달할 BaseModel기반 클래스 생성
class DetectionResult(BaseModel):
    message: str        # 객체인식 결과 메시지
    image: str          # 인식결과 이미지

# 이미지 객체탐지 함수
def detectObjects(image: Image.Image):
    img = np.array(image)       # Pillow이미지 numpy배열로 변환
    results = model(img)        # 객체탐지, 물체 여러개
    class_name = model.names        # person, clock, car...    

    # 그리기 준비
    annotated = image.convert('RGB').copy()  # 원본을 복사
    draw = ImageDraw.Draw(annotated)  # 복사본 이미지
    font = ImageFont.load_default()

    for result in results:       # 여러개 물체들을 반복
        boxes = result.boxes.xyxy       
        confiences = result.boxes.conf  # 신뢰도 98.0%
        class_ids = result.boxes.cls    # 클래스 명

        for box, confience, class_id in zip(boxes, confiences, class_ids):
            x1, y1, x2, y2 = map(int, box)  # x1,y1(사각형 왼쪽 상단), x2,y2(사각형 오른쪽 하단)
            label = class_name[int(class_id)]    # 단일된 클래스 명

            # 각 인식된 객체에 사각형            
            draw.rectangle([x1, y1, x2, y2], outline=(255,0,0), width=3)
            
            # TODO : 종류별로 색상 다르게
            # TODO : 라벨(클래스명)

    
    return annotated

@app.get('/')
async def index():
    image = Image.open('./miniproject_aspnet/step2/test.jpg')  # 이미지 로드
    result = detectObjects(image)
    result.save('result1.jpg')
     
    return { 'message': 'Hello FastAPI', 'result': 'Image saved!' }

@app.post('/detect')
async def detectService():
    pass


if __name__ == '__main__':
    uvicorn.run(app, host='127.0.0.1', port=8000)