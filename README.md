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

  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/게임시작.png" width="70%" height="50%" />

  - 게임을 시작하게되면 서로 누구인지 구별되기 위해 캐릭터 상단에 닉네임이 표시되도록 하였습니다.
  - 또한 UI/UX에도 체력이 보이도록 구현하였습니다. 자세한것은 UI/UX 부분에서 설명하겠습니다. 
  
  ### 플레이어
  - 전반적인 플레이어 구현 Code
  - [Player Controller Code](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/02.Scripts/03.Player/PlayerController.cs)
  - [플레이어 구현 Code](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/02.Scripts/03.Player/Player.cs) 
  #### 이동 및 회전 
  - 가장 기본적인 동작으로 wasd 를 사용하여 캐릭터가 이동할 수 있습니다.
  - 회전 같은 경우는 현재의 카메라 시점으로 기준으로 하여 마우스를 회전시켜 카메라가 보는 방향으로 앞으로 할 수 있도록 구현하였습니다.
  - 또한 카메라의 회전을 부드럽게 하기위해 Lerp() 메소드를 사용하여 회전 보간을 잡아주었습니다.
  - [회전 Code](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/02.Scripts/03.Player/RotateToMouse.cs)

  #### 1인칭 시점 카메라
  - 플레이어의 카메라의 같은 경우 손 은 보여주어야 하며 내 플레이어는 보여주지 않도록 하여야 하며
  - 상대플레이어는 보여주며 상대플레이어의 손은 안보여줘야하는 모순적인 상황이 발생하게 되었다.
  - 이를 해결하기 위해 LayerMask를 사용하여 생성되는 플레이어와 로컬플레이어의 Layer를 바꿔주게 하여 카메라에서 랜더링 되지 않게 구현하였습니다.
  - [카메라 렌더링 Code](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/02.Scripts/03.Player/PlayerSetup.cs)
  
  #### 공격 및 무기 스왑 
  <p align="left">
    <h5>총</h5>
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/GIF/총.gif" width="50%" height="50%" /> 
    <h5>샷건</h5>
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/GIF/샷건.gif" width="50%" height="50%" /> 
    <h5>근접 및 수류탄</h5>
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/GIF/칼수류탄.gif" width="50%" height="50%" /> 
  </p>

  - 플레이어가 할 수 있는 공격구현 부분입니다.
  - 플레이어는 1,2,3,4번 키를 눌러 무기를 변경할 수 있도록 구현하였습니다.
  - 해당 무기는 장착한 아이템에 맞게 변경되어 발사될 수 있도록 구현하였습니다.

  #### 인벤토리 
  - 인벤토리 핵심 구현 Code
  - [슬롯 구현 Code](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/02.Scripts/06.Inventory/Slot.cs)
  - [인벤토리 구현 Code](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/02.Scripts/06.Inventory/Inventory.cs) 
  <p align="left">
    <h5>아이템 줍기</h5>
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/GIF/아이템줍기.gif" width="50%" height="50%" /> 
    <h5>아이템 버리기</h5>
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/GIF/아이템버리기.gif" width="50%" height="50%" /> 
    <h5>아이템 교환 및 장착</h5>
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/GIF/아이템교환장착.gif" width="50%" height="50%" /> 
  </p>

  - 플레이어가 아이템을 저장할 수 있는 인벤토리 입니다.
  - 각 아이템은 합쳐지는 아이템과 합쳐지지 않는 아이템으로 구분하여 인벤토리에 저장될 수 있도록 하였습니다.
  - 아이템 버리기 같은 경우 shift를 누른 상태에서 버리게 되면 해당 아이템의 절반만 버려지도록 구현하였습니다.
  - 아이템 장착 같은 경우 드래그그랍으로도 장착이 되지만 우클릭을 이용하여 퀵장착이 되도록 구현하였습니다.

  문제점 해결 
  - 초기 아이템 구현을 스크립터블 오브젝트로 하였습니다.
  - 여기서의 문제점은 아이템 버리기를 구현하기 위해서 해당 아이템의 Total Count를 사용해야한다는 점이였습니다.
  - 스크립터블 오브젝트는 변수의 값이 모두 공유되기 때문에 아이템을 두번 버리게 되면 처음 버린 아이템의 개수가 최종적으로 버린 아이템의 개수와 같아져 최종적인 아이템의 개수가 감소한다는 문제점이 있었습니다.
  - 이를 해결하기 위해 아이템 Class로 바꾸어 다시 구현하였으며 이로 인해 변수를 독립적으로 사용하게 되어 아이템 버리기 기능을 구현할 수 있었습니다.
  - [아이템 Code](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/02.Scripts/05.Item/ItemController.cs)

  #### 기절 및 부활 
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/살리기.png" width="50%" height="50%" /> 

  - 플레이어가 기절되어 있는 상태일 경우 'E' 키를 눌러 플레이어를 살릴 수 있습니다.
  - 살아나게된 플레이어는 체력이 절반 회복됩니다. 
  
  #### 사망 
   <p align="left">
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/GIF/사망.gif" width="50%" height="50%" /> 
  </p>

  - 기절하였을 때 아래에 보이게 되는 게이지가 다 소멸하게 되면 사망 상태로 변경되게 됩니다.
  - 사망 상태일 경우 플레이어는 살아있는 플레이어를 기준으로 1인칭 관전 모드로 전환 되게 되며 마우스 클릭을 사용하여 다른 플레이어의 시점도 관찰할 수 있도록 구현하였습니다. 
  - 게임의 난이도를 높게 설정하여 다음 스테이지로 넘어갔을 경우는 죽은 플레이어를 부활시켜 게임을 다시 진행할 수 있도록 구현하였습니다. 
  
  ### 좀비 

  ### 맵 

  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/맵1.png" width="30%" height="50%" /> 
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/맵2.png" width="30%" height="50%" /> 
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/맵3.png" width="30%" height="50%" /> 
  

  ### 사운드 

  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/사운드.png" width="20%" height="20%" /> 

  ### UI/UX

  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/UI오류.PNG" width="50%" height="50%" /> 
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/UI표시.png" width="50%" height="50%" /> 
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/인게임UI.png" width="50%" height="50%" /> 

  ### 쉐이더
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/외곽선셰이더.png" width="50%" height="50%" /> 
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/반투명쉐이더.png" width="50%" height="50%" /> 

  ### 레벨 디자인

  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/맵스폰1.png" width="30%" height="30%" /> 
  
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/맵스폰2.png" width="20%" height="30%" /><img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/맵스폰3.png" width="20%" height="30%" /><img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/맵스폰4.png" width="20%" height="30%" /> 

  ### 최적화 

  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/풀링.png" width="20%" height="20%" /> 

  ## 7. 아쉬운 부분
