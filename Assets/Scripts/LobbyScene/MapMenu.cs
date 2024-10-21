using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapMenu : MonoBehaviour
{

    [Header("Map")]
    [SerializeField] private Image mapImage;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private TextMeshProUGUI mapName;
    [SerializeField] private MapSelectionData mapSelectionData;

    private int currentMapIndex = 0;

    public static MapInfo SelectedMap { get; private set; }

    private void OnEnable()
    {
        leftButton.onClick.AddListener(OnLeftButtonClicked);
        rightButton.onClick.AddListener(OnRightButtonClicked);
        UpdateMap();
    }

    private void OnDisable()
    {
        leftButton.onClick.RemoveListener(OnLeftButtonClicked);
        rightButton.onClick.RemoveListener(OnRightButtonClicked);
    }


    private void OnLeftButtonClicked()
    {
        if (currentMapIndex - 1 > 0)
        {
            currentMapIndex--;
        }
        else
        {
            currentMapIndex = mapSelectionData.Maps.Count - 1;
        }

        UpdateMap();

    }

    private void OnRightButtonClicked()
    {
        if (currentMapIndex + 1 < mapSelectionData.Maps.Count)
        {
            currentMapIndex++;
        }
        else
        {
            currentMapIndex = 0;
        }

        UpdateMap();

    }

    private void UpdateMap()
    {
        SelectedMap = MapSelectionData.Instance.Maps[currentMapIndex];
        mapImage.sprite = SelectedMap.MapImage;
        mapName.text = SelectedMap.MapName;
    }

}
