# Project Z. Zoombie 


  ## 목차 
  [1. 개요](#1-개요)
  
  [2. 개발 인원](#2-개발-인원) 
  
  [3. 기술 스택](#3-기술-스택) 
  
  [4. 개발 일정](#4-개발-일정) 
  
  [5. 흐름도](#5-흐름도) 
  
  [6. 프로젝트 기능](#6-프로젝트-기능) 
  
  [7. 아쉬운 부분](#7-아쉬운-부분)   

  [8. 만족스러운 부분](#8-만족스러운-부분)   

  ## 1. 개요 
  진행 기간 : 2024.07.04 ~ 2024.08.12  (약 1달간 진행)
  
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

  - 전체적인 구현 로직
  - [좀비 Controller Code](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/02.Scripts/04.Enemy/EnemyController.cs)
  - [노말 좀비 Code](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/02.Scripts/04.Enemy/NormalEnemy.cs)

  #### 좀비 종류 
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/노말좀비.png" width="17%" height="50%" /> <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/엘리트근거리.png" width="17%" height="50%" /> <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/엘리트원거리.png" width="17%" height="50%" />  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/보스1.png" width="17%" height="50%" />  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/보스2.png" width="17%" height="50%" />  
      기본좀비    엘리트 근거리 좀비 엘리트 원거리 좀비      보스 좀비1         보스 좀비2 
  - 각 좀비는 기본 좀비를 상속받아 구현하였으며 공격로직 특수 공격 로직은 부모의 메소드를 오버라이딩하여 구현하였습니다.
  - '엘리트 근거리 좀비'는 죽게되면 자폭 데미지와 함깨 4마리의 작은 좀비를 분열시켜 등장하는 능력을 구현하였습니다.
  - '엘리트 원거리 좀비'는 공격이 지형지물에 맞게되면 초당 도트데미지를 주는 장판을 생성하는 능력을 구현하였습니다.

  - 보스 좀비는 공통적으로 여러 패턴을 가지고 있으며 일정 주기에 따라 패턴이 랜덤적으로 등장하도록 구현하였습니다. 
  - '보스 좀비1'은 기본 패턴으로 물기, 마구찍기, 브레스뱉기, 꼬리치기 가 있으며 탐색하는 플레이어가 일정거리 이상 멀어질 경우 특수 패턴으로 굴러서 돌진하는 패턴을 구현하였습니다
  - '보스 좀비2'는 기본 패턴으로 지면 충격파, 칼 휘두르기 패턴이 있으며 탐색하는 플레이어가 일정거리 이상 멀어질 경우 특수 패턴으로 돌진하는 패턴을 구현하였습니다


  #### 좀비 추적
  <p align="left">
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/GIF/추적2.gif" width="30%" height="30%" /> 
  </p>

  - 좀비는 일정 범위안에 들어온 플레이어를 추격하며 내부의 작은 콜라이더 범위에 닿았을 경우에는 공격을 시전합니다.
  - 내부의 들어온 플레이어가 여럿일 경우는 가장 가까운 플레이어를 탐색하여 추적하도록 구현하였습니다
  - 좀비는 한번 추척 시 죽을 때까지 플레이어를 추적하도록 구현하였습니다.
  - 추적하는 길찾기 방식은 NavMesh를 사용하여 플레이어를 최단 거리로 추적할 수 있도록 구현하였습니다. 

  #### 어그로 변경 
  <p align="left">
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/GIF/어그로.gif" width="30%" height="30%" /> 
  </p>

  - 좀비는 원거리에서 공격을 맞았을 시 범위안에 플레이어가 없어도 추적을 할 수 있도록 하였습니다.
  - 또한 이 추적 같은경우 쏜 플레이어가 아닌 가장 가까운 플레이어를 추적하도록 하여 게임의 자연스러움을 구현하였습니다.
  - 일반, 엘리트 좀비와 다르게 보스 좀비는 10초의 간격을 가지고 범위안에 있는 플레이어로 어그로를 자동적으로 전환하도록 구현하였습니다.
  - [보스 1좀비 Code](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/02.Scripts/04.Enemy/BossZombie.cs)

  ### 맵 

  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/맵1.png" width="30%" height="50%" /> 

  - 튜토리얼 형식의 1스테이지 입니다.
  - 기본적인 무기들과 좀비들의 종류를 알려주기 위해 맵을 가장 단순하고 짧게 만들었습니다.

  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/맵2.png" width="30%" height="50%" /> 

  - 2번째 스테이지 입니다.
  - 처음으로 보스가 등장하게 되는 스테이지며 보스를 처치하여 특정 오브젝트를 얻어 NPC에 제출하여 다음으로 넘어가는 길을 여는 방식으로 진행되는 스테이지 입니다. 
  
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/맵3.png" width="30%" height="50%" />  

  - 3번째 스테이지 입니다.
  - 각 맵의 협동 기믹을 수행하여 보스방에 도달하여 보스를 처치하면 되는 맵입니다. 
  - 이 스테이지를 클리어할 시 엔딩씬으로 넘어가게 되며 게임의 전체적인 플레이는 끝이나게 됩니다. 

  ### 맵 전환

  <p align="left">
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/GIF/씬이동.gif" width="50%" height="50%" /> 
  </p>

  - 각 맵에는 다음으로 넘어갈 수 있는 구역이 파란색 반투명 오브젝트로 표시되며 해당 구역에는 살아있는 모든 플레이어가 들어가야 맵이 이동이됩니다.
  - 해당 구역 안에서는 공격을 할 수 없으며 좀비 또한 구역안으로 들어올 수 없습니다. 
  - 어두워졌다 밝아지는 효과는 각 맵 전환시 발생하는 효과입니다.
  - 플레이어가 현재 맵을 이동하고 있는지, 이동이 되었는지를 나타내기 위해서 만든효과입니다.
  - 또한 맵을 이동하게 되면 현재 가지고 있던 아이템들은 DB에 저장이 되게 되며 다음 맵으로 이동할 시 습득한 아이템이 보존되도록 구현하였습니다.
  - [씬 전환 Code](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/02.Scripts/02.Manager/NextSceneManager.cs)

  문제점 해결 
  - 해당 Photon에서는 방장만 방을 이동할 수 있는 권한이 있었습니다.
  - 저희 게임 같은 경우 방장도 죽을 수가 있기때문에 이러한 상황에서는 맵을 이동하지 못하는 상황이 발생하였습니다.
  - 또한 퀘스트 아이템을 먹은 플레이가 죽게되면 해당 임무를 클리어할 수가 없어 맵을 이동하지 못하는 상황이 발생하였습니다.
  - 이를 해결하고자 우선 방장이 죽은 경우 살아있는 모든 플레이어중 한명을 마스터클라이언트로 권한을 넘겨주어 방장이 죽어라도 다른 사람이 방장이 되어 방을 이동할 수 있도록 하였습니다.
  - 두번째로 퀘스트 아이템을 가진 사람이 죽게 될 경우 해당 플레이어는 퀘스트 아이템을 생성하도록 구현하였습니다.
  - 이러한 문제점들을 해결하여 씬 이동에서의 문제점을 해결하였습니다. 

  ### 사운드 

  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/사운드.png" width="20%" height="20%" /> 

  - 우선 BGM 같은 경우 각 스테이지마다 다른소리를 주고자 3개의 BGM을 놓고 순서대로 실행하였습니다.
  - 효과음 같은 경우는 하나의 AudioSource만 사용하기에는 다중적으로 많은 소리들이 발생하여 소리가 끊기는 현상이 발생하였습니다.
  - 이를 해결하기 위해 효과음은 채널링 구현 방식을 사용하였습니다.
  - 다중의 AudioSource를 생성하여 현재 AudioSource가 재생중인지 아닌지 판단하여 다음 AudioSource에서 재생할 지 현재 AudioSource에서 재생할지를 결정하게 구현하였습니다.
  - 이렇게 구현하였을 때 장점으로는 동시에 여러소리가 발생하더라도 사운드가 끊기지 않고 자연스럽게 여러 소리가 들린다는 것이였습니다.
  - [오디오 채널링 Code](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/02.Scripts/02.Manager/AudioManager.cs)


  ### UI/UX
  
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/UI오류.PNG" width="50%" height="50%" /> 

  - 로그인 실패시 사용자가 확인할 수 있도록 가시성을 추가한 UI 입니다.
  - 이외에도 회원가입 실패시, 회원가입 성공시, 로그인 성공시, 방에 입장할시, 방에 들어갈수 없을 시, 강퇴당했을 시 에 모든 UI를 추가하여
  - 사용자가 플레이 하는데 있어 어느 상황에 처해있는지 알 수 있도록 구현하였습니다.
  
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/UI표시.png" width="50%" height="50%" /> 

  - 사용자가 상호작용할 수 있는 모든 오브젝트에는 가운데의 조준점 아래에 초록색깔 글씨로
  - 상요작용할 수 있는 키, 상호작용하는 오브젝트 이름, 할 수있는 행동을 표기하여 사용자의 가시성을 돕도록 구현하였습니다.
  
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/인게임UI.png" width="50%" height="50%" /> 

  - 사용자가 플레이 하면서 보여지는 인게임 UI입니다.
  - 우측 하단의 작은 버튼은 채팅창이며 확태 축소가 가능하도록 구현하였습니다.
  - 우측 사이드의 UI는 현재의 아이템 상황을 보여주는 UI 입니다. 장착을 하지않으면 반투명, 장착을 하게되면 흰색에 아이템 개수가 나오게됩니다. 장착을 하고 장비를 들게되면 초록색으로 현재 무슨 장비를 들고 있는지 표시해주도록 구현하였습니다.
  - 좌측 하단은 자신의 닉네임과 현재 채력 상태를 알 수 있도록 구현하였습니다. 
  - 좌측 상단은 다른 플레이어들의 체력과 위치를 알 수 있는 미니맵을 구현하였습니다. 

  - [인게임 UI Code](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/02.Scripts/02.Manager/UIManager.cs)
  <p align="left">
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/GIF/미니맵.gif" width="50%" height="50%" /> 
  </p>

  - 자신의 위치는 빨간색, 다른 플레이어의 위치는 파란색 화살표로 표시하여 자신이 어디있는지 확인할 수 있도록 구현하였습니다.
  - 현재 자신의 위치로 상태 플레이어의 위치를 계산하여 상대플레이어의 위치를 보여지도록 하였습니다.
  - 카메라를 사용하여 쉽게 구현할 수 있었지만 카메라를 사용하지않고 정적인 이미지에 플레이어의 위치를 표현할 수 있도록 구현하였습니다.
  - [미니맵 Code](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/02.Scripts/03.Player/PlayerMiniMap.cs)

  ### 쉐이더
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/외곽선셰이더.png" width="50%" height="50%" /> 
  
  [외곽선 쉐이더](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/Resources/OutLine.Shader)

  - 획득할 수 있는 아이템인지 알 수 있도록 외곽선 쉐이더를 추가하였습니다.
  - 오브젝트의 현재 크기에서 조금더 큰 크기의 행태를 만들고 거기에 색을 입히는 방식으로 구현하였습니다. 
  
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/반투명쉐이더.png" width="50%" height="50%" /> 
  
  [Hidden 쉐이더](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/Resources/Deep.shader)

  - 특정 임무 아이템의 위치를 보여주기위해 구현한 히든 쉐이더입니다.
  - "Queue" = "Geometry+1" 값을 어느것 보다 1높게 지정하여 어느 오브젝트들 보다 우선순위가 높게 보이도록 구현하였습니다. 

  ### 레벨 디자인

  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/맵스폰1.png" width="30%" height="30%" /> 

  - 각 구역에는 보이지 않는 콜라이더를 생성하고 해당 구역을 플레이어가 통과할 때 몬스터들이 나오도록 하였습니다.
  - 다중의 생성을 막기위해 어느 플레이어가 통과를 하든간에 bool로 확인을 하여 추가적으로 좀비가 생성되지 않게 구현하였습니다. 
  
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/맵스폰2.png" width="20%" height="30%" /><img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/맵스폰3.png" width="20%" height="30%" /><img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/맵스폰4.png" width="20%" height="30%" /> 

  - 각각의 해당 구역은 몬스터 또는 아이템의 종류와 서로 맵핑이되는 확률이 있습니다.
  - 해당 지역에 생성되는 몬스터의 종류과 그에 맞는 각각의 확률을 조정할 수 있습니다.
  - 해당 확률을 조정하여 레벨 디자인을 보다 수월하게 진행하였습니다. 

  ### 최적화 

  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/풀링.png" width="20%" height="20%" /> 

  - 가장 기본적인 최적화방식인 오브젝트 풀링입니다.
  - 기존의 오브젝트 풀링 방식인 [기존 오브젝트 풀링](https://github.com/parkjun-0521/unity_-practice/tree/main/ObjectPooing) index로 찾는 방식으로는 index에 무슨 아이템이 매핑되어있는지 외워야한다는 단점이 있었습니다.
  - 그래서 변경하였습니다. [변경한 오브젝트 풀링](https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Assets/02.Scripts/02.Manager/Pooling.cs) String 즉, 오브젝트 프리펩의 이름으로 찾을 수 있는 방법으로 변경하였습니다.
  - 이렇게 변경함으로써 index를 외우고 있지 않아도 오브젝트의 이름으로 아이템을 생성할 수 있게 되어 코드의 가독성이 향상되었습니다. 

  <p align="left">
  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/GIF/OcclusionCulling.gif" width="50%" height="50%" /> 
  </p>

  - 카메라 최적화 부분인 OcclusionCulling입니다.
  - 카메라가 랜더링 하고있지 않은 부분은 오브젝트를 보이지 않게 하는 방식이며 특정 오브젝트 뒤에 있는 오브젝트들도 보이지 않게 하여 메모리를 관리할 수 있는 카메라 최적화 기법입니다.
  - 실제로 해당 최적화를 진행하지 않았을 때 좀비가 50~100 마리 생성되어있는지역에서 버벅임이 발생하였습니다. 하지만 해당 최적화를 진행하고 난 후에는 이전 보다 상대적으로 버벅임이 적어졌습니다.


  <img src="https://github.com/parkjun-0521/FinalProject_ZombieFPS/blob/master/Image/라이트.png" width="40%" height="40%" /> 

  - 모든 맵의 빛은 LightProbe를 사용하여 구웠습니다.
  - LightProbe를 사용하여 빛을 구움으로써 동적인 오브젝트가 빛을 받는데 있어 좀 더 자연스러워졌습니다.
  - 또한 동적인 오브젝트의 빛에 대한 연산을 줄여 빛을 구현하는데에 필요한 비용을 줄였습니다.
  - 상대적으로 프로젝트 규모가 작아 티가 많이 나지는 않지만 규모가 커지면 꼭 필요할 것이라고 생각하여 최적화를 진행하였습니다. 
    

  ## 7. 아쉬운 부분

  - 국비인것 치고 개발기간이 너무나도 짧았다.오전부터 프로젝트 작업을 한것이 아닌 프로젝트는 오후 2시간씩 밖에 하지 못했고 대부분 남아서나 주말에 완성을 하였다.
  - 8월부터 오전, 오후 프로젝트를 진행하였다.
  - 개인의 능력을 잘 판단하여 역할 분담을 하지 못한 부분이 존재한다.
  - 예를 들면 기본 좀비로직을 능력에 맞는 역할분담을 잘못하여 프로젝트 종료 2주전에 내가 좀비로직으로 처음부터 수정하는 일이 발생하였다.
  - 처음부터 역량을 잘 알고 역할 분담을 했으면 좀 더 좋은 프로젝트가 되었을 것이라 생각한다.

  ## 8. 만족스러운 부분

  - 개발기간이 짧았지만 잘 따라와준 팀원 덕분에 미완성인 작품은 아니였다는 것에 만족한다.
  - 오히려 개발 기간의 일주일 정도 여유가 생겨 버그를 발견하고 수정하는데 힘을 쏟아 눈에 보이는 오류는 거의 다 잡았다.
  - 첫 팀장으로써 프로젝트였지만 중간에 포기하는 팀원없이 팀을 잘 이끌어간것 같아 만족스럽다. 
