# BackEndHero
게임을 개발할 때 도움이 될 수 있도록 예제게임을 개발했습니다!

BackEndHero는 [므쮸 컴퍼니](http://mmzzuu.com/)에서 게임을 제작하였고, 뒤끝 SDK를 사용하여 서버기능을 구현하였습니다.

관련 소스코드는 모두 오픈소스로 공개됩니다.

BackEndHero는 Unity 2018.2.20f1과 Backend-3.8.3 .NET 3버전을 기준으로 개발되었습니다.

BackEndHero에 대한 자세한 설명은 아래 링크를 참고해주세요.
https://developer.thebackend.io/outline/guide/sampleGameGuide

## 라이센스
* BackEndHero는 [BSD-2-Clause](https://github.com/thebackend-io/BackEndHero/blob/master/LICENSE) 라이센스를 따릅니다.

## 마켓 링크
<!-- - [Apple App Store](https://apps.apple.com/us/app/backend-hero/id1461432877?l=ko&ls=1) -->
- [Google Play Store](https://play.google.com/store/apps/details?id=io.thebackend.backendhero)


## 포함된 뒤끝베이스 기능
BackEndHero는 아래의 뒤끝베이스 기능들을 포함하고 있습니다.

사용된 뒤끝 기능은 뒤끝의 모든 기능이 아닌, 뒤끝 기능의 일부이며 추후 BackEndHero 예제게임에 추가될 수 있습니다.

뒤끝베이스 관련 기능들은 `Assets/Sources/Scripts/BackEndServerManager.cs` 와 `Assets/Sources/Scripts/BackEndUIManager.cs` 를 주로 참고하시면 됩니다.

### 버전관리
* 게임 버전 관리

### 게임 유저 관리
* 커스텀 회원가입
* GPGS 페더레이션 회원가입
* 뒤끝 토큰을 이용하여 로그인
* 닉네임 설정/변경

### 게임 정보 관리
* 유저 점수를 서버로 업데이트/다운로드
* 구매 내역을 서버로 업데이트/다운로드

### 공지사항
* 공지사항 불러오기

### 랭킹
* 1 ~ 10위까지의 랭킹 및 점수 불러오기
* 유저의 랭킹 및 점수 불러오기

### 푸시설정
* 안드로이드, iOS 푸시설정

### 게임 캐시 관리
* 영수증 검증

### 로그
* 최고점수를 서버에 업데이트 할 때 서버에 로그 기록
* 유저가 광고제거 아이템을 구매할 때 서버에 로그 기록


## 포함된 뒤끝챗 기능
BackEndHero는 아래의 뒤끝챗 기능들이 포함되어 있습니다.

사용된 뒤끝 기능은 뒤끝의 모든 기능이 아닌, 뒤끝 기능의 일부이며 추후 BackEndHero 예제게임에 추가될 수 있습니다.

뒤끝베이스 관련 기능들은 `Assets/Sources/Scripts/BackEndChatManager.cs` 와 `Assets/Sources/Scripts/BackEndUIManager.cs` 를 주로 참고하시면 됩니다.

### 채팅서버에 접속/접속종료
* 채팅서버 상태 확인
* 활성화 되어있는 일반채널 리스트 받아오기
* 일반 채널에 접속
* 일반 채널에서 나가기

### 메시지 송신
* 채팅 메시지 송신
* 특정 유저에게 귓속말 송신
* 특정 유저 차단/차단해제
* 특정 유저 신고

### 채팅 핸들러 설정
* 입장 핸들러 설정
    * 유저의 입장
    * 다른 유저의 입장
* 퇴장 핸들러 설정
    * 유저의 퇴장
    * 다른 유저의 퇴장
* 채팅 핸들러 설정
    * 메시지 수신
* 귓속말 핸들러 설정
    * 귓속말 수신
* 오류 핸들러 설정

### 채팅 추가 기능 설정
* 필터링 유무 설정
* 필터링 메시지 설정
* 도배방지 메시지 설정
* 자동접속 종료 메시지 설정

### 같은 채널에 참여한 유저 정보 관리
* 세션정보
* 닉네임
