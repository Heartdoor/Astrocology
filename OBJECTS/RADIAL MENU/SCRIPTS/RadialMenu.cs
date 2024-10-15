using Godot;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using static Asriela.BasicFunctions;


public partial class RadialMenu : Node2D
{
    void myMethod()
    {
        myMethod();
    }
    #region INTERACTION OPTION SETUP
    private void GenerateInteractionOptions(List<Entity.Command> interactionTypes)
    {
        foreach (Entity.Command type in interactionTypes)
            interactionOptions.Add(new InteractionOption(type));
    }
    private List<Node2D> armsList = new List<Node2D>();
    private List<InteractionOption> interactionOptions = new List<InteractionOption>();
    private List<Button> buttonList = new List<Button>();
    private Main gameController;
    public class InteractionOption
    {
        public Entity.Command Type { get; set; }
        public string IconPath { get; set; } // Path to the icon resource
        public string Name { get; set; }
       
        // Constructor to initialize the option with a name and icon path
        public InteractionOption(Entity.Command type)
        {
            Type = type;
            IconPath = $"res://OBJECTS/RADIAL MENU/SPRITES/{Type}.png";

            Name = $"{Type.action}"; 


        }
    }

    #endregion

    #region OPEN MENU
    public void OpenMenu(Main main, List<Entity.Command> interactionTypes, Entity target)
    {
        gameController = main;
        GenerateInteractionOptions(interactionTypes);
        var optionsCount = interactionTypes.Count;


        Log("OPENED MENU", LogType.ui);

        for (int i = 0; i < optionsCount; i++)
        {

            Log($"OPTIONS COUNT {optionsCount}", LogType.ui);


            Node2D arm = Add2DNode("RADIAL MENU/arm.tscn",this);
            armsList.Add(arm);
            arm.RotationDegrees = i * (360.0f / (float)optionsCount);
            var optionButton = GetButton(arm, "Node2D/OptionButton");

            optionButton.Text = interactionOptions[i].Name;
            SetIcon(optionButton, GetTexture2D(interactionOptions[i].IconPath));
            buttonList.Add(optionButton);

            var type = interactionOptions[i].Type;

         
            optionButton.Pressed += () => { SelectOption(type, target); };//interactionOptions[i].Type
            
        }
    }


    #endregion

    #region SELECT OPTION
    private void SelectOption(Entity.Command type, Entity target)
    {

       
           gameController.PerformSelectedAction(type, target);

        gameController.CloseMenu(this); 

        

    }
    #endregion


}