using Cinemachine;
using echo17.EndlessBook;
using System.Collections;
using UnityEngine;

public class BookController : MonoBehaviour
{

    public EndlessBook book;

    [SerializeField] CinemachineVirtualCamera startView;
    [SerializeField] CinemachineVirtualCamera bookView;
    

    void Update()
    {
        
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (book.CurrentState == EndlessBook.StateEnum.ClosedFront)
            {
                book.SetState(EndlessBook.StateEnum.OpenMiddle);
                startView.Priority = 0;
                bookView.Priority = 1;
            }
            // book.CurrentLeftPageNumber �� ���� ������ �ѱ�鼭 UI��������
            Debug.Log(book.CurrentLeftPageNumber);
            if (!book.IsLastPageGroup)
            {
                book.TurnToPage(book.CurrentLeftPageNumber + 2, EndlessBook.PageTurnTimeTypeEnum.TimePerPage, 1f);
            }
        }
    }


}
