using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using TMPro;

public class VictoryMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject positionCell;
    [SerializeField] private GameObject cellParent;
    
    [Space]
    [SerializeField] private TMP_Text victoryDefeatText;
    [SerializeField] private Color victoryColor;
    [SerializeField] private Color defeatColor;

    void Start()
    {
        // Sort teams by score
        List<int> teams = new List<int>(ScoreManager.Instance.TeamIdToPlayerIds.Keys);
        teams.Sort((a, b) => {
            int xScore = ScoreManager.Instance.GetPoints(a);
            int yScore = ScoreManager.Instance.GetPoints(b);
            return yScore.CompareTo(xScore); // Sort descending
        });

        int spawnedCells = 0;
        for (int index = 0; index < teams.Count; index++) {
            int team = teams[index];
            
            int score = ScoreManager.Instance.GetPoints(team);
            List<int> playerIds = ScoreManager.Instance.GetPlayerIdsForTeam(team);

            foreach (int playerId in playerIds) {
                if (!ClientInstance.playerInstances.TryGetValue(playerId, out ClientInstance playerInstance)) { continue; }

                GameObject newCell = Instantiate(positionCell, cellParent.transform);
                newCell.SetActive(true);
                newCell.transform.localPosition = new Vector3(0, spawnedCells * -3, 0);
                spawnedCells++;

                bool won = index == 0;

                // Ik this kinda jank but I didn't want to add another prefab or monobehaviour
                newCell.transform.GetChild(0).gameObject.SetActive(won);
                newCell.transform.GetChild(1).GetComponent<TMP_Text>().text = ToOrdinal(spawnedCells);
                newCell.transform.GetChild(2).GetComponent<TMP_Text>().text = playerInstance.PlayerName;
                newCell.transform.GetChild(3).GetComponent<TMP_Text>().text = "Won "+ score + " rounds!";

                int imageID = SteamFriends.GetLargeFriendAvatar((CSteamID)playerInstance.PlayerSteamID);
                newCell.transform.GetChild(4).GetChild(0).GetComponent<RawImage>().texture = GetSteamImageAsTexture(imageID);
            }
        }

        Settings.Instance.IncreaseGamesPlayed();
        
        int winningTeamId = teams[0];
        if (ScoreManager.Instance.GetTeamId(LobbyController.Instance.LocalPlayerController.PlayerId) == winningTeamId) {
            Settings.Instance.IncreaseGamesWon();
            victoryDefeatText.color = victoryColor;
            victoryDefeatText.text = "Victory";
        }
        else {
            Settings.Instance.IncreaseGamesLost();
            victoryDefeatText.color = defeatColor;
            victoryDefeatText.text = "Defeat";
        }
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        return texture;
    }

    public static string ToOrdinal(int n) {
        return n + (n % 100 >= 11 && n % 100 <= 13 ? "th" :
            n % 10 == 1 ? "st" :
            n % 10 == 2 ? "nd" :
            n % 10 == 3 ? "rd" : "th");
    }
}
