﻿using UnityEngine;
using BOXRTS;

public class PauseMenu : Menu
{

    private Player player;

    protected override void Start()
    {
        base.Start();
        player = transform.root.GetComponent<Player>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Resume();
    }

    protected override void SetButtons()
    {
        buttons = new string[] { "Resume", "Exit Game" };
    }

    protected override void HandleButton(string text)
    {
        switch (text)
        {
            case "Resume": Resume(); break;
            case "Exit Game": ReturnToMainMenu(); break;
            default: break;
        }
    }

    private void ReturnToMainMenu()
    {
        Cursor.visible = true;
        Application.LoadLevel("MainMenu");        
    }

    private void Resume()
    {
        Time.timeScale = 1.0f;
        GetComponent<PauseMenu>().enabled = false;
        if (player) player.GetComponent<UserInput>().enabled = true;
        Cursor.visible = false;
        ResourceManager.MenuOpen = false;
    }

}