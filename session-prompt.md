# 다음 세션 시작 프롬프트

## 현재 상태
- Git 커밋: 최신 - 무기 시스템 연결 및 보상 시스템 구현 완료
- 브랜치: main

## 구현된 기능 (최신)
✅ 무기 시스템 연결 완료
   - GameData의 Claw(201), Lance(202) 무기 연결
   - 무기별 BaseAttack 데미지 적용 (Claw=10, Lance=14)
   - 플레이어 무기 스프라이트 표시 (자식 오브젝트로 생성)
   - 무기별 색상 구분 (Claw=빨강, Lance=파랑)

✅ 보상 및 성장 시스템 구현 완료
   - 전투 승리 시 보상 획득 (골드 + 확률적 무기)
   - RewardResolver를 통한 보상 계산
   - BattleRewardService로 PlayerWalletState/PlayerInventoryState에 적용
   - GUI 기반 보상 팝업 (OnGUI 사용)
   - "계속하기" 버튼 - 다음 층 진행
   - "나가기" 버튼 - Lobby 씬으로 이동
   - FloorProgressionService로 층 진행 관리

## 핵심 파일 위치
- 전투 초기화: `Assets/Scripts/Combat/BattleSceneInitializer.cs`
- 플레이어 컨트롤러: `Assets/Scripts/Combat/PlayerController.cs`
- 보상 서비스: `Assets/Scripts/Meta/Rewards/BattleRewardService.cs`
- 보상 계산: `Assets/Scripts/Meta/Rewards/RewardResolver.cs`
- 층 진행: `Assets/Scripts/Meta/Progression/FloorProgressionService.cs`
- 플레이어 상태: `Assets/Scripts/Meta/State/PlayerInventoryState.cs`
- 플레이어 지갑: `Assets/Scripts/Meta/State/PlayerWalletState.cs`

## 다음 작업 후보
1. **보상 시스템 개선** (추천)
   - 무기 보상 엔트리 추가 (GameData에 Weapon 타입 보상 추가)
   - 보상 팝업 UI 개선 (TextMeshPro 적용, 애니메이션 추가)
   - 획득한 무기 정보 상세 표시 (무기 이름, 스탯)

2. **Growth 시스템 연결**
   - 로비에서 Growth 씬으로 이동
   - EquipmentRerollService로 무기 강화 구현
   - EquipmentComparisonService로 무기 비교
   - 장비 교체/강화 UI 연결

3. **전투 밸런싱**
   - 적 HP 조정 (현재 한방에 죽음)
   - 공격 쿨타임 추가
   - 플레이어 체력/스태미나 시스템

4. **UI 개선**
   - 체력바/벽 HP바 표시
   - 데미지 숫자 팝업
   - 스킬 쿨타임 인디케이터

5. **다음 층 진행**
   - Battle 씬에서 다음 층 파라미터 전달
   - 2층 이상 적 난이도 증가
   - 층별 다른 보상 테이블 적용

## 테스트 방법
1. Unity에서 Bootstrap 씬 실행
2. Title → Lobby → Battle 순으로 진행
3. Space: 공격, G: 가드, Left Shift: 대시
4. 적 처치 후 보상 팝업 확인
5. 계속하기/나가기 버튼 테스트

## 주의사항
- Battle 씬에는 Player 오브젝트가 없음 (런타임에 생성)
- Lobby 씬의 B키로 Battle로 이동 가능
- 무기는 BootstrapInstaller에서 Claw(201) 기본 장착
- 보상은 RewardTable 1001, 1002, 1003 사용
- 현재는 골드 보상만 존재 (무기 보상 엔트리 미구현)

## 알려진 이슈
- 무기 보상은 GameData에 RewardType.Weapon 엔트리 추가 필요
- 보상 팝업은 OnGUI 사용 (향후 TextMeshPro로 개선 권장)
- Lance 무기 테스트하려면 BootstrapInstaller에서 weaponId를 202로 변경
