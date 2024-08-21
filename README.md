# Project Z. Zoombie 


  ## 목차 
  [1. 개요](#1-개요)
  
  [2. 개발 인원](#2-개발-인원) 
  
  [3. 기술 스택](#3-기술-스택) 
  
  [4. 개발 일정](#4-개발-일정) 
  
  [5. 흐름도](#5-흐름도) 
  
  [6. 프로젝트 핵심 구현](#6-프로젝트-핵심-구현) 
  
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

  ## 6. 프로젝트 핵심 구현 

  ### 네트워크 
  - Photon Pun2 를 사용하여 네트워크를 구현하였습니다.
  - 서버 접속과 로비접속, 방 입장, 방 생성 부분을 구현하였으며 이전 프로젝트와 다르게 네트워크 외의 기능은 독립적으로 구현하였습니다.
  - 기본적인 네크워크 구성 외에도 각 오브젝트의 RPC 작업을 해주어 동기화를 구현하였습니다.
  - [네트워크 Code](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/02.Scripts/01.Network/NetworkManager.cs)

  ### 로그인
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/로그인.png" width="50%" height="50%" />
  
  ### 플레이어

  ### 인벤토리

  ### 좀비 

  ### 맵 

  ### 사운드 

  ### UI/UX

  ### 최적화 

  ## 7. 아쉬운 부분
