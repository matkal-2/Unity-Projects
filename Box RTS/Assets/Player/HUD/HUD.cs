using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BOXRTS;

public class HUD : MonoBehaviour {
    private Player player;
    public GUISkin resourceSkin, orderSkin, infoSkin, selectBoxSkin;

    // GUI skin constants
    private const int INFO_BAR_HEIGHT = 64, INFO_BAR_WIDTH = 450, ORDER_BAR_HEIGHT = 150, ORDER_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40, SELECTION_NAME_HEIGHT = 25;
    private const int ICON_WIDTH = 32, ICON_HEIGHT = 32, TEXT_WIDTH = 128, TEXT_HEIGHT = 32;
    private const int BUILD_IMAGE_WIDTH = 64, BUILD_IMAGE_HEIGHT = 64, BUTTON_SPACING = 7, SCROLL_BAR_WIDTH = 6, BUILD_IMAGE_PADDING = 2;
    

    // Resources to be displayed and their Textures
    public Texture2D[] resources;
    private Dictionary<ResourceType, Texture2D> resourceImages;
    private Dictionary<ResourceType, int> resourceValues, resourceLimits;

    // Cursor Textures, Skin and variables
    public Texture2D activeCursor;
    public Texture2D selectCursor, leftCursor, rightCursor, upCursor, downCursor;
    public Texture2D[] moveCursors, attackCursors, harvestCursors;
    public Texture2D rallyPointCursor;
    public GUISkin mouseCursorSkin;
    private CursorState activeCursorState;
    private int currentFrame = 0;
    private CursorState previousCursorState;


    // Unit creatin variables and texture
    private WorldObject lastSelection;
    private float sliderValue;
    private int buildAreaHeight = 0;
    public Texture2D buildFrame, buildMask;

    // Buttons Hover/click texture
    public Texture2D buttonHover, buttonClick;

    // Rally Point And other small button
    public Texture2D smallButtonHover, smallButtonClick;

    


	// Use this for initialization
	void Start () {
        player = transform.root.GetComponent<Player>();
        ResourceManager.StoreSelectBoxItems(selectBoxSkin);
        SetCursorState(CursorState.Select);
        resourceValues = new Dictionary<ResourceType, int>();
        resourceLimits = new Dictionary<ResourceType, int>();
        resourceImages = new Dictionary<ResourceType, Texture2D>();

        // Unit creatin variable
        buildAreaHeight = ORDER_BAR_HEIGHT - BUILD_IMAGE_HEIGHT/4;

        for (int i = 0; i < resources.Length; i++)
        {
            switch (resources[i].name)
            {
                case "Money":
                    resourceImages.Add(ResourceType.Money, resources[i]);
                    resourceValues.Add(ResourceType.Money, 0);
                    resourceLimits.Add(ResourceType.Money, 0);
                    break;
                case "Power":
                    resourceImages.Add(ResourceType.Power, resources[i]);
                    resourceValues.Add(ResourceType.Power, 0);
                    resourceLimits.Add(ResourceType.Power, 0);
                    break;
                default: break;
            }
        }
	}
	
	// Update is called once per frame
	void OnGUI () {
        if (player && player.human)
        {
            GUI.depth = ResourceManager.GUIBarDepth;
            DrawOrdersBar();
            DrawResourceBar();
            DrawInfoBar();
            DrawQueBar();
            DrawMouseCursor();
            
        }
	}

    //  -------     Drwaing bars        -------------
    private void DrawQueBar() 
    {
        
        if (player.SelectedObject)
        {
            if (player.SelectedObject.IsOwnedBy(player))
            {
                // draw build queue
                Building selectedBuilding = lastSelection.GetComponent<Building>();
                if (selectedBuilding)
                {
                    GUI.skin = null;
                    GUI.BeginGroup(new Rect(Screen.width / 2 - INFO_BAR_WIDTH / 2, Screen.height - 2 * INFO_BAR_HEIGHT, INFO_BAR_WIDTH, INFO_BAR_HEIGHT));
                    GUI.Box(new Rect(0, 0, INFO_BAR_WIDTH, INFO_BAR_HEIGHT), "");
                    DrawBuildQueue(selectedBuilding.getBuildQueueValues(), selectedBuilding.getBuildPercentage());
                    GUI.EndGroup();
                }
            }
        }
        
        
    }

    private void DrawInfoBar()
    {
        GUI.skin = orderSkin;
        GUI.BeginGroup(new Rect(Screen.width / 2 - INFO_BAR_WIDTH / 2, Screen.height - INFO_BAR_HEIGHT, INFO_BAR_WIDTH, INFO_BAR_HEIGHT));
        GUI.Box(new Rect(0, 0, INFO_BAR_WIDTH, INFO_BAR_HEIGHT), "");
        string selectionName = "";
        if (player.SelectedObject)
        {
            selectionName = player.SelectedObject.objectName;            
        }
        if (!selectionName.Equals(""))
        {
            GUI.Label(new Rect(INFO_BAR_HEIGHT/2, INFO_BAR_HEIGHT/2-SELECTION_NAME_HEIGHT/2, INFO_BAR_WIDTH , SELECTION_NAME_HEIGHT), selectionName);
        }
        GUI.EndGroup();
    }

    private void DrawOrdersBar()
    {
        GUI.skin = orderSkin;
        GUI.BeginGroup(new Rect(Screen.width - ORDER_BAR_WIDTH, Screen.height-ORDER_BAR_HEIGHT, ORDER_BAR_WIDTH, ORDER_BAR_HEIGHT));
        GUI.Box(new Rect(0, 0, ORDER_BAR_WIDTH, ORDER_BAR_HEIGHT), "");
        if (player.SelectedObject)
        {
            if (player.SelectedObject.IsOwnedBy(player))
            {
                //reset slider value if the selected object has changed
                if (lastSelection && lastSelection != player.SelectedObject) sliderValue = 0.0f;
                DrawActions(player.SelectedObject.GetActions());
                //store the current selection
                lastSelection = player.SelectedObject;
                Building selectedBuilding = lastSelection.GetComponent<Building>();
                if (selectedBuilding)
                {
                    DrawStandardBuildingOptions(selectedBuilding);
                }
                
            }
        }
        GUI.EndGroup();
    }

    private void DrawResourceBar()
    {
        GUI.skin = resourceSkin;
        GUI.BeginGroup(new Rect(0, 0, Screen.width, RESOURCE_BAR_HEIGHT));
        int topPos = 1, iconLeft = 4, textLeft = 20;
        GUI.Box(new Rect(0, 0, Screen.width, RESOURCE_BAR_HEIGHT), "");
        DrawResourceIcon(ResourceType.Money, iconLeft, textLeft, topPos);
        iconLeft += TEXT_WIDTH;
        textLeft += TEXT_WIDTH;
        DrawResourceIcon(ResourceType.Power, iconLeft, textLeft, topPos);
        GUI.EndGroup();
    }

    private void DrawResourceIcon(ResourceType type, int iconLeft, int textLeft, int topPos)
    {
        Texture2D icon = resourceImages[type];
        string text = resourceValues[type].ToString() + "/" + resourceLimits[type].ToString();
        GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), icon);
        GUI.Label(new Rect(textLeft, topPos, TEXT_WIDTH, TEXT_HEIGHT), text);
    }

    public void SetResourceValues(Dictionary<ResourceType, int> resourceValues, Dictionary<ResourceType, int> resourceLimits)
    {
        this.resourceValues = resourceValues;
        this.resourceLimits = resourceLimits;
    }

        //  -----------  unit creating -----------
    private void DrawActions(string[] actions)
    {
        GUIStyle buttons = new GUIStyle();
        buttons.hover.background = buttonHover;
        buttons.active.background = buttonClick;
        GUI.skin.button = buttons;
        int numActions = actions.Length;
        //define the area to draw the actions inside
        GUI.BeginGroup(new Rect(0, 0, ORDER_BAR_WIDTH, buildAreaHeight));
        //draw scroll bar for the list of actions if need be
        if (numActions >= MaxNumRows(buildAreaHeight)) DrawSlider(buildAreaHeight, numActions / 2.0f);
        //display possible actions as buttons and handle the button click for each
        for (int i = 0; i < numActions; i++)
        {
            int column = i % 2;
            int row = i / 2;
            Rect pos = GetButtonPos(row, column);
            Texture2D action = ResourceManager.GetBuildImage(actions[i]);
            if (action)
            {
                //create the button and handle the click of that button
                if (GUI.Button(pos, action))
                {
                    if (player.SelectedObject) player.SelectedObject.PerformAction(actions[i]);
                }
            }
        }
        GUI.EndGroup();
    }

    private int MaxNumRows(int areaHeight)
    {
        return areaHeight / BUILD_IMAGE_HEIGHT;
    }

    private Rect GetButtonPos(int row, int column)
    {
        int left = SCROLL_BAR_WIDTH + column * BUILD_IMAGE_WIDTH;
        float top = row * BUILD_IMAGE_HEIGHT - sliderValue * BUILD_IMAGE_HEIGHT;
        return new Rect(left, top, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
    }

    private void DrawSlider(int groupHeight, float numRows)
    {
        //slider goes from 0 to the number of rows that do not fit on screen
        sliderValue = GUI.VerticalSlider(GetScrollPos(groupHeight), sliderValue, 0.0f, numRows - MaxNumRows(groupHeight));
    }

    private Rect GetScrollPos(int groupHeight)
    {
        return new Rect(BUTTON_SPACING, BUTTON_SPACING, SCROLL_BAR_WIDTH, groupHeight - 2 * BUTTON_SPACING);
    }

    private void DrawBuildQueue(string[] buildQueue, float buildPercentage)
    {
        for (int i = 0; i < buildQueue.Length; i++)
        {
            Rect buildPos = new Rect(BUILD_IMAGE_PADDING+i*BUILD_IMAGE_WIDTH, -BUILD_IMAGE_PADDING, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
            GUI.DrawTexture(buildPos, ResourceManager.GetBuildImage(buildQueue[i]));
            GUI.DrawTexture(buildPos, buildFrame);
            float width = BUILD_IMAGE_WIDTH - 2 * BUILD_IMAGE_PADDING;
            float height = BUILD_IMAGE_HEIGHT - 2 * BUILD_IMAGE_PADDING;
            float topPos = 0;
            if (i == 0)
            {
                Debug.Log("Build progress: "+buildPercentage.ToString(), gameObject);
                //shrink the build mask on the item currently being built to give an idea of progress
                topPos += height * buildPercentage;
                height *= (1 - buildPercentage);
            }
            GUI.DrawTexture(new Rect(2 * BUILD_IMAGE_PADDING+i * BUILD_IMAGE_WIDTH, topPos, width, height), buildMask);
        }
    }
        //  -----------  End of unit creating -----------
        // --------  Rally Point    ------------
    private void DrawStandardBuildingOptions(Building building)
    {
        

        GUIStyle buttons = new GUIStyle();
        buttons.hover.background = smallButtonHover;
        buttons.active.background = smallButtonClick;
        GUI.skin.button = buttons;
        int leftPos =  BUTTON_SPACING;
        int topPos = buildAreaHeight - BUILD_IMAGE_HEIGHT / 2;
        int width = BUILD_IMAGE_WIDTH / 2;
        int height = BUILD_IMAGE_HEIGHT / 2;
        if (building.hasSpawnPoint())
        {
            if (GUI.Button(new Rect(leftPos, topPos, width, height), building.rallyPointImage))
            {
                if (activeCursorState != CursorState.RallyPoint && previousCursorState != CursorState.RallyPoint) SetCursorState(CursorState.RallyPoint);
                else
                {
                    //dirty hack to ensure toggle between RallyPoint and not works ...
                    SetCursorState(CursorState.PanRight);
                    SetCursorState(CursorState.Select);
                }
            }
        }
    }

    public void PlacedRallyPoint()
    {
        SetCursorState(CursorState.Select);
        previousCursorState = activeCursorState;
    }
        // -------  End of Rally Point  ----------
        //  -------    End Drwaing bars        -------------




    // When Mouse is not on GUI
    public bool MouseInBounds()
    {
        //Screen coordinates start in the lower-left corner of the screen
        //not the top-left of the screen like the drawing coordinates do
        Vector3 mousePos = Input.mousePosition;
        bool insideOrderWidth = mousePos.x >= 0 && mousePos.x <= Screen.width - ORDER_BAR_WIDTH;
        bool insideOrderHeight = mousePos.y >= 0 && mousePos.y >= ORDER_BAR_HEIGHT;

        bool insideInfoWidth = !( mousePos.x >= Screen.width / 2 - INFO_BAR_WIDTH / 2 && mousePos.x <= Screen.width / 2 + INFO_BAR_WIDTH / 2 );
        bool insideInfoHeight = mousePos.y >= 0 && mousePos.y >= INFO_BAR_HEIGHT;

        bool insideResHeight = mousePos.y >= 0 && mousePos.y <= Screen.height - RESOURCE_BAR_HEIGHT;


        return (insideOrderWidth || insideOrderHeight) && insideResHeight && (insideInfoHeight || insideInfoWidth);
    }

    // Return Screen area
    public Rect GetPlayingArea()
    {
        return new Rect(0, 0, Screen.width , Screen.height);
    }

    //  -------     Mouse Cursor stuff  ----------
    private void DrawMouseCursor()
    {

        bool panSide = activeCursorState != CursorState.PanRight && activeCursorState != CursorState.PanUp && activeCursorState != CursorState.PanDown && activeCursorState != CursorState.PanLeft;
        if (!MouseInBounds() && panSide)
        {
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
            GUI.skin = mouseCursorSkin;
            GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
            UpdateCursorAnimation();
            Rect cursorPosition = GetCursorDrawPosition();
            GUI.Label(cursorPosition, activeCursor);
            GUI.EndGroup();
        }
    }

    private void UpdateCursorAnimation()
    {
        //sequence animation for cursor (based on more than one image for the cursor)
        //change once per second, loops through array of images
        if (activeCursorState == CursorState.Move)
        {
            currentFrame = (int)Time.time % moveCursors.Length;
            activeCursor = moveCursors[currentFrame];
        }
        else if (activeCursorState == CursorState.Attack)
        {
            currentFrame = (int)Time.time % attackCursors.Length;
            activeCursor = attackCursors[currentFrame];
        }
        else if (activeCursorState == CursorState.Harvest)
        {
            currentFrame = (int)Time.time % harvestCursors.Length;
            activeCursor = harvestCursors[currentFrame];
        }
    }

    private Rect GetCursorDrawPosition()
    {
        //set base position for custom cursor image
        float leftPos = Input.mousePosition.x;
        float topPos = Screen.height - Input.mousePosition.y; //screen draw coordinates are inverted
        //adjust position base on the type of cursor being shown
        if (activeCursorState == CursorState.PanRight) leftPos = Screen.width - activeCursor.width;
        else if (activeCursorState == CursorState.PanDown) topPos = Screen.height - activeCursor.height;
        else if (activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Harvest)
        {
            topPos -= activeCursor.height / 2;
            leftPos -= activeCursor.width / 2;
        }
        else if (activeCursorState == CursorState.RallyPoint) topPos -= activeCursor.height;

        return new Rect(leftPos, topPos, activeCursor.width, activeCursor.height);
    }

    public void SetCursorState(CursorState newState)
    {
        if (activeCursorState != newState) previousCursorState = activeCursorState;

        activeCursorState = newState;
        switch (newState)
        {
            case CursorState.Select:
                activeCursor = selectCursor;
                break;
            case CursorState.Attack:
                currentFrame = (int)Time.time % attackCursors.Length;
                activeCursor = attackCursors[currentFrame];
                break;
            case CursorState.Harvest:
                currentFrame = (int)Time.time % harvestCursors.Length;
                activeCursor = harvestCursors[currentFrame];
                break;
            case CursorState.Move:
                currentFrame = (int)Time.time % moveCursors.Length;
                activeCursor = moveCursors[currentFrame];
                break;
            case CursorState.PanLeft:
                activeCursor = leftCursor;
                break;
            case CursorState.PanRight:
                activeCursor = rightCursor;
                break;
            case CursorState.PanUp:
                activeCursor = upCursor;
                break;
            case CursorState.PanDown:
                activeCursor = downCursor;
                break;
            case CursorState.RallyPoint:
                activeCursor = rallyPointCursor;
                break;
            default: break;
        }
    }

    public CursorState GetPreviousCursorState()
    {
        return previousCursorState;
    }

    public CursorState GetCursorState()
    {
        return activeCursorState;
    }




}
