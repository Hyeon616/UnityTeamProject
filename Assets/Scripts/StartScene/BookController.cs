using Cinemachine;
using echo17.EndlessBook;
using TMPro;
using UnityEngine;

public class BookController : MonoBehaviour
{

    [SerializeField] private EndlessBook book;

    [SerializeField] private TextMeshProUGUI gameStart;

    [SerializeField] private CinemachineVirtualCamera startView;
    [SerializeField] private CinemachineVirtualCamera bookView;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openBook;
    [SerializeField] private AudioClip pagingBook;

    [SerializeField] private GameObject auth;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        RegisterLoginManager.OnLoginSuccess += HandleLoginSuccess;
    }
    private void OnDisable()
    {
        RegisterLoginManager.OnLoginSuccess -= HandleLoginSuccess;
    }

    private void HandleLoginSuccess()
    {
        // 로그인 성공 시 책을 여는 동작 수행
        if (book.CurrentState == EndlessBook.StateEnum.ClosedFront)
        {
            book.SetState(EndlessBook.StateEnum.OpenMiddle);
            BookSound(openBook);
            startView.Priority = 0;
            bookView.Priority = 1;
        }
    }

    private void OnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Book")) && !RegisterLoginManager.isLogin)
        {
            if (book.CurrentState == EndlessBook.StateEnum.ClosedFront)
            {
                gameStart.gameObject.SetActive(false);
                auth.gameObject.SetActive(true);

            }
        }

        if (!book.IsLastPageGroup && book.CurrentState == EndlessBook.StateEnum.OpenMiddle)
        {
            book.TurnToPage(book.CurrentLeftPageNumber + 2, EndlessBook.PageTurnTimeTypeEnum.TimePerPage, 1f);
            BookSound(pagingBook);
        }

        // 책 마지막페이지 클릭
        if (book.CurrentLeftPageNumber == 7)
        {
            
            // 튜토리얼 Scene으로 이동
            // SceneLoader.Instance.LoadSceneAsync("TutorialScene");
            SceneLoader.Instance.LoadSceneAsync("LobbyScene");
        }

    }

    void BookSound(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.PlayOneShot(audioClip);
    }

}
