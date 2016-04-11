using BOXRTS;
using UnityEngine;

public class SelectPlayerMenu : MonoBehaviour
{
    private string playerName = "NewPlayer";
    public GUISkin selectionSkin;
    public GUISkin mySkin;

    void Start() {
        PlayerManager.Load();
        Cursor.visible = true;
        SelectionList.LoadEntries(PlayerManager.GetPlayerNames());

    }

    void OnGUI()
    {

        GUI.skin = mySkin;

        float menuHeight = GetMenuHeight();
        float groupLeft = Screen.width / 2 - ResourceManager.MenuWidth / 2;
        float groupTop = Screen.height / 2 - menuHeight / 2;
        Rect groupRect = new Rect(groupLeft, groupTop, ResourceManager.MenuWidth, menuHeight);

        GUI.BeginGroup(groupRect);
        //background box
        GUI.Box(new Rect(0, 0, ResourceManager.MenuWidth, menuHeight), "");
        //menu buttons
        float leftPos = ResourceManager.MenuWidth / 2 - ResourceManager.ButtonWidth / 2;
        float topPos = menuHeight - ResourceManager.Padding - ResourceManager.ButtonHeight;
        if (GUI.Button(new Rect(leftPos, topPos, ResourceManager.ButtonWidth, ResourceManager.ButtonHeight), "Select"))
        {
            SelectPlayer();
        }
        //text area for player to type new name
        float textTop = menuHeight - 2 * ResourceManager.Padding - ResourceManager.ButtonHeight - ResourceManager.TextHeight;
        float textWidth = ResourceManager.MenuWidth - 2 * ResourceManager.Padding;
        playerName = GUI.TextField(new Rect(ResourceManager.Padding, textTop, textWidth, ResourceManager.TextHeight), playerName, 14);
        SelectionList.SetCurrentEntry(playerName);
        GUI.EndGroup();

        string prevSelection = SelectionList.GetCurrentEntry();

        //selection list, needs to be called outside of the group for the menu
        float selectionLeft = groupRect.x + ResourceManager.Padding;
        float selectionTop = groupRect.y + ResourceManager.Padding;
        float selectionWidth = groupRect.width - 2 * ResourceManager.Padding;
        float selectionHeight = groupRect.height - GetMenuItemsHeight() - ResourceManager.Padding;
        SelectionList.Draw(selectionLeft, selectionTop, selectionWidth, selectionHeight, selectionSkin);

        string newSelection = SelectionList.GetCurrentEntry();
        //set saveName to be name selected in list if selection has changed
        if (prevSelection != newSelection)
        {
            playerName = newSelection;
        }
    }

    private void SelectPlayer()
    {
        PlayerManager.SelectPlayer(playerName);
        GetComponent<SelectPlayerMenu>().enabled = false;
        MainMenu main = GetComponent<MainMenu>();
        if (main) main.enabled = true;
    }

    private float GetMenuHeight()
    {
        return 250 + GetMenuItemsHeight();
    }

    private float GetMenuItemsHeight()
    {
        float avatarHeight = 0;
        return avatarHeight + ResourceManager.ButtonHeight + ResourceManager.TextHeight + 3 * ResourceManager.Padding;
    }
}