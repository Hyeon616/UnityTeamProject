using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PlayInfo : MonoBehaviour
{
    
    public static int coins;
    public static int jewels;
    public TextMeshProUGUI Cointext;
    public TextMeshProUGUI Jeweltext;
    private void Update()
    {
        Cointext.text = $"���� :{coins}";
        Jeweltext.text = $"���� :{jewels}";
    }
    public static void JewelPlus()
    {
        jewels++;
    }
    public static void CoinPlus()
    {
        
        coins++;
    }
}
