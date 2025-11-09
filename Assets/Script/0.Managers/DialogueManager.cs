using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    private int curChapter; // GameManager에서 갱신, 변경시 GameManager에 반영
    private int curIndex; // 초기에는 GameManager에서 로드, 이후에는 대사 진행에 따라 변동

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    /*
     필요 로직
     - 챕터별 대사파일 불러오기
        - 게임 시작시 및 챕터 전환시
        - 현재 챕터의 대사 파일을 불러오고 임시로 필드에 지닙니다.
     - CSV 파싱
        - 파싱을 통해 인덱스 
        - charCount가 1일때 char2 관련 변수 건너뛰기
        - nextScene이 null이 아닐 때 다음 씬의 대사 로드하기 (+1000000)
            - 씬 번호는 csv상에 항목을 따로 두지는 말고, 1000100 이런식으로 표기하고자 함 
                - SSIIIAAA SS : 두자리 씬번호, III 세자리 인덱스, AAA 세자리 인덱스 예비번호
                - 기획자는 그대로 씬, 인덱스 번호 따로 csv 파일상에 표기하여 관리하고, 해당 번호 7자리수로 여기서 파싱   
                - Additional 항목이 null이 아닐 때 (그러니까 기획자가 대사를 중간에 추가했을 때) 추가된 대사 로드하기(+1)
                - Additional이 null일 때
                    - AAA가 깨끗하다면 1000을 더한다
                    - AAA가 깨끗하지 않다면 치우고 1000을 더한다
                    => 즉, 백의자리를 버림하고 1000을 더한다는 소리\
                    - 그런데 이방식 필요 없겠다. bool이면 바로 버림하면 되는데 그냥 null로 판단할거면
                    - 1 더했을때 해당 인덱스에 대사가 없다?
                    - 그때 버림을 해서 다음 인덱스 로드하면 된다.
                    
     - 대사 로드
        - 씬과 인덱스에 따른 대사 로드     
     - 씬 전환(실제 씬 x) 및 그에 따른 처리
        - 배경 변경 및 효과음 재생
     */
}