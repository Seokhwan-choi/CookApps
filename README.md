# CookApps
쿡앱스 클라이언트 프로그래머 채용 과제

* 구현 항목
    - 필수 구현 항목
        - 드랍 로직
        - 매칭 조건
    - 추가 구현 항목
        - 특수 블럭 구현
           - 제공해주신 리소스를 최대한 활용하여 특수 블럭의 기능과 실제 이미지의 차이가 있습니다.
           - UFO 블럭 : 같은 색상의 블럭을 모두 삭제하는 블럭
               - ![Image](https://github.com/user-attachments/assets/fd94302e-0be6-4c37-9ec0-00b6495af6ff)

           - 로켓 블럭 : 한 라인을 모두 삭제하는 블럭
               - ![Image](https://github.com/user-attachments/assets/aa54089b-a677-4b96-86be-ccfd409fd3ae) ![Image](https://github.com/user-attachments/assets/8e34e3e4-e447-49cc-8818-874dbe6466a3) ![Image](https://github.com/user-attachments/assets/711bd9e8-d500-4fbf-a094-422f710e7433) ![Image](https://github.com/user-attachments/assets/dabfac9d-2aa0-4a11-bbf5-7ba4307eebda) ![Image](https://github.com/user-attachments/assets/ca551561-5133-4c8f-807a-3ee73d860e7f) ![Image](https://github.com/user-attachments/assets/e7c074ff-33da-42b7-88ca-1d902c9b0391)
           
           - 거북이 블럭 : 6방향의 라인을 모두 삭제하는 블럭
               - ![Image](https://github.com/user-attachments/assets/db113a2f-9351-491c-9851-c62b881fa6c5) ![Image](https://github.com/user-attachments/assets/c7392ab6-ebc8-47aa-ba79-ec0dfb0bdb43) ![Image](https://github.com/user-attachments/assets/aac1990c-b69d-4862-b3ff-43b4993e22cb) ![Image](https://github.com/user-attachments/assets/a8c41b63-54f5-4afd-8d18-11f620b6b0e7) ![Image](https://github.com/user-attachments/assets/eb0db02d-d492-4099-baf7-9be53271b81f) ![Image](https://github.com/user-attachments/assets/4967dac6-1513-4ae1-8617-a84ca789160b)

           - TNT 블럭 : 일정 범위의 블럭을 모두 삭제하는 블럭
               - ![Image](https://github.com/user-attachments/assets/d2b5e86e-260b-449e-a3d8-93984bf6b11a) ![Image](https://github.com/user-attachments/assets/5f76c040-2213-499b-aab8-121d3844415b) ![Image](https://github.com/user-attachments/assets/7f8e0be1-bb36-4adc-a4b1-1bfda6847388) ![Image](https://github.com/user-attachments/assets/76f20aef-0afa-46d0-a3e5-6a11dac83426) ![Image](https://github.com/user-attachments/assets/6632126b-5bc2-478b-9680-d82418826799) ![Image](https://github.com/user-attachments/assets/ce6f46b4-69b7-4c10-9e7b-85b1c76570b6)

           - 부메랑 블럭 : 랜덤 블럭 1개를 삭제하는 블럭
               - ![Image](https://github.com/user-attachments/assets/4e8181f7-26dd-429b-bff9-bbcad21349e3) ![Image](https://github.com/user-attachments/assets/d2e55910-ef14-4340-aea2-bdb9432dcce2) ![Image](https://github.com/user-attachments/assets/a4a6d64a-5073-4f5c-a858-f8854aa8191f) ![Image](https://github.com/user-attachments/assets/06dc8d73-1d85-4227-af3f-8c7eb6580c9a) ![Image](https://github.com/user-attachments/assets/7447e3a9-27ad-4043-928c-43d2cd312c6b) ![Image](https://github.com/user-attachments/assets/698d73d2-2e17-4a87-a8e4-69715417050e)

        - 장애물 블럭
            - 팽이 : 주변 블럭이 매칭되었을 때 활성화&파괴
               - ![Image](https://github.com/user-attachments/assets/02d6018f-f28b-4d30-bdd2-7be10d425a72) ![Image](https://github.com/user-attachments/assets/0dcdce8a-070b-4c4f-a70b-abb9f0c068d7)

        - 유효한 스왑이 불가능 하다면 블럭 셔플 진행
        - 간단한 UI
            - 남은 팽이 갯수 표기
            - 남은 움직임 횟수 표기
            - 스코어 표기
            - StageClear / GameOver / Shuffle Notify UI 적용
          
