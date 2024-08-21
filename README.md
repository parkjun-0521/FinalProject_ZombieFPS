# Project Z. Zoombie 


  ## 목차 
  [1. 개요](#1-개요)
  
  [2. 개발 인원](#2-개발-인원) 
  
  [3. 기술 스택](#3-기술-스택) 
  
  [4. 개발 일정](#4-개발-일정) 
  
  [5. 흐름도](#5-흐름도) 
  
  [6. 프로젝트 기능](#6-프로젝트-기능) 
  
  [7. 아쉬운 부분](#7-아쉬운-부분)   

  ## 1. 개요 
  프로젝트 명 : Project Z. Zoombie
  
  장르 : 1인칭 FPS 게임 
  
  배경 : 좀비, 아포칼립스
  
  플랫폼 : PC, VR

  소개 : 플레이어는 다양한 아이템을 습득하여 수 많은 좀비에게서 생존해야 하며, 특정 임무를 수행하여 최종 스테이지까지 이동하여 탈출하는 게임입니다. 
  
  ## 2. 개발 인원 

  ### 박준 ( PM ) 
  - 네트워크 ( Photon )
  - 플레이어 ( 메인 ) 
  - 좀비 ( 엘리트 근거리, 보스1 )
  - 아이템
  - 인벤토리
  - UX / UI ( 메인 )
  - 사운드 ( 메인 )
  - 최적화 ( 오브젝트 풀링, InputManager ) 

  ### 오상준 ( PA ) 
  - 애니메이션 ( 메인 )
  - 플레이어 ( 서브 )
  - 좀비 ( 엘리트 원거리, 보스2 )
  - 미니맵
  - 최적화 ( Occlusion Culling )

  ### 신희빈 ( PA ) 
  - 좀비 ( 기본 좀비 ) 
  - 애니메이션 ( 서브 )
  - 사운드 ( 서브 )
  - UX / UI ( 서브 ) 
  - 맵
  - VR ( 총쏘고 적 잡는 기능까지만 구현 ) 

  ## 3. 기술 스택  

  ### 개발 툴 
  - Unity 2021.3.8f1
  - Visual Studio 2022

  ### 개발 서버 및 DB 
  - Apache 2.4
  - MySQL

  ### 버전 관리 
  - Source Tree
  - GitHub

  ### 개발 일정 관리 
  - Notion


  ## 4. 개발 일정 
  - 개발 일정 관리의 일부 
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/개발일정.PNG" width="100%" height="50%" />
  
  - [전체 개발 일정관리 노션 링크](https://dark-background-538.notion.site/d6830528245e483098e0bcc56151fcf7?v=963fb2bc1f604f26bf04f5a224c15e14&pvs=4)


  ## 5. 흐름도 
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/흐름도.png" width="60%" height="30%" />

  ## 6. 프로젝트 기능

  ### 네트워크 
  - Photon Pun2 를 사용하여 네트워크를 구현하였습니다.
  - 서버 접속과 로비접속, 방 입장, 방 생성 부분을 구현하였으며 이전 프로젝트와 다르게 네트워크 외의 기능은 독립적으로 구현하였습니다.
  - 기본적인 네크워크 구성 외에도 각 오브젝트의 RPC 작업을 해주어 동기화를 구현하였습니다.
  - [네트워크 Code](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/02.Scripts/01.Network/NetworkManager.cs)

  ### 로그인
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/로그인.png" width="50%" height="50%" /> <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/로그인DB.png" width="30%" height="30%" />

  - 게임을 시작하면 가장 먼저 등장하게되는 화면이며 로그인을 하게 되면 서버접속과 동시에 로비로 들어가지게 됩니다.
  - 각 아이디 정보는 DB에 들어가 있으며 해당 ID에 맞는 비밀번호를 입력해야 들어갈 수 있도록 구현하였습니다.

  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/회원가입.PNG" width="50%" height="50%" />

  - 아이디가 없을 경우는 회원가입을 통해 계정을 생성할 수 있습니다.
  - 계정을 생성하게 되면 해당 계정 정보는 DB에 저장되어 로그인 정보를 유지할 수 있도록 구현하였습니다.

  - [로그인 Code](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/02.Scripts/02.Manager/LoginManager.cs)

  ### 로비 

  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/방입장.png" width="50%" height="50%" />

  - 로그인을 하게되면 등장하게 되는 로비 화면입니다.
  - 로비화면 같은 경우 사용자의 시각적인 재미를 주기 위해 배경화면의 이미지를 MP4로 하여 움직이는 배경화면을 구현하였습니다.
  - 오른쪽의 RoomList는 현재 만들어져 있는 방들의 이름을 나타내며 해당 버튼을 눌러 방에 들어갈 수 있습니다.
  - < > 화살표 버튼을 통해 다음 페이지로 넘겨가며 방을 찾을 수 있습니다. 또한 방이 많아지는 것을 대비하여 Input Field 만들어 방의 이름으로도 들어갈 수 있도록 구현하였습니다.

  ### 방 입장 ( 방장, 플레이어 ) 

  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/방에입장시.png" width="70%" height="50%" />

  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/모든플레이어준비.png" width="70%" height="50%" />

  - 왼쪽은 방장의 화면이며 오른쪽은 로컬플레이어의 화면입니다.
  - 방장은 항상 빨간 테두리로 상단에 표시되도록 구현하였으며 로컬플레이어는 자신이 누구인지 알 수 있도록 초록색 외곽선 테두리로 표시하였습니다.
  - 방장의 경우 로컬플레이어 이름 옆에 X 표시가 보이게 되는데 해당 기능은 강퇴기능으로 자신이 모르는 플레이어가 들어왔을 때는 강퇴를 할 수 있도록 구현하였습니다.
  - 로컬플레이어 같은 경우 준비 버튼을 누르게 되면 해당 이름 Text가 초록색으로 변하게 되며 준비가 된 상태라는 것을 모두에게 알려주도록 RPC를 보내 동기화를 하였습니다.
  - 모든 플레이어가 준비를 할 경우 방장의 게임시작 버튼이 초록색으로 변하게 되며 게임시작을 할 수 있는 상태가 됩니다. 
  
  ### 플레이어

  ### 인벤토리

  ### 좀비 

  ### 맵 

  ### 사운드 

  ### UI/UX

  ### 최적화 

  ## 7. 아쉬운 부분
