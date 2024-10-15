using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Asriela.BasicFunctions;
public partial class Main : Node2D
{
    public Node2D radialMenu = null;
    public Label statsLabel = null;
    public List<Entity.Command> interactionOptions = new List<Entity.Command>();
    public Dictionary<Makeup, float> Resources = new Dictionary<Makeup,float>();
    public static Main main;
    public int SurvivorCount;
    public float doubleRight =0;
    void Run()
    {
        if (ButtonPressed("LeftClick") && radialMenu==null)
        {
            if (Entity.overEntities.Count == 0)
            {
                foreach (Entity entity in Entity.selectedEntities)
                {
                    entity.selected = false;

                }

                Entity.selectedEntities.Clear();
                Entity.globalSelected = null;
            }
         
        }




        if ((doubleRight>0 && ButtonPressed("RightClick")) && Entity.overEntities.Count == 0)
        {
            foreach (Entity entity in Entity.selectedEntities)
            {
                var command = new Entity.Command(Act.standGuard, Tag.none, null, Makeup.none, -0.1f,GetGlobalMousePosition() );
                entity.commandsList.Add(command);
            }
        }

        if (ButtonPressed("RightClick"))
        {
            doubleRight = 2;
        }
        if(doubleRight>0)
        doubleRight-=0.1f;

        if (SurvivorCount == 0)
            statsLabel.Text = "GAME OVER";
        else 
        statsLabel.Text = $"FOOD {(int)Resources[Makeup.food]} WATER {(int)Resources[Makeup.fluid]} WOOD {(int)Resources[Makeup.wood]} SURVIVORS {SurvivorCount}";
        Resources[Makeup.food] = 0;
        Resources[Makeup.fluid] = 0;
        Resources[Makeup.wood] = 0;
        SurvivorCount = 0;
        Interact_With_Menu();



    }


    void Start()
    {
        main = this;
        Resources.Add(Makeup.food, 0);
        Resources.Add(Makeup.fluid, 0);
        Resources.Add(Makeup.wood, 0);
        Resources.Add(Makeup.energy, 0);
        statsLabel = (Label)GetParentNode(this,"Camera2D/Label");
        statsLabel.Text = "hi";
    }


    public void PerformSelectedAction(Entity.Command act, Node2D target )
    {
        foreach (Entity entity in Entity.selectedEntities)
        {

   
            entity.commandsList.Add(act);

        }
        ///add score for certain delegates in commandsList holds action and target
        ///inside existsing run delegates in score check access command list to see if command exist
    }
    void Interact_With_Menu()
    {

        if (Entity.globalSelected != null  && ButtonPressed("RightClick"))
        {
            if (radialMenu == null )
            {


                OpenRadialMenu(Entity.globalSelected);
            }
            else
            {
                CloseMenu(radialMenu);

            }
        }
        if (radialMenu != null)
            radialMenu.GlobalPosition = GlobalPosition;

    }

    #region RADIAL MENU 

    public  void CloseMenu(Node2D menu)
    {

        Destroy(menu);
        radialMenu = null;
    }

    public  void OpenRadialMenu(Node2D target)
    {
        radialMenu = Add2DNode("RADIAL MENU/radial_menu.tscn", this);
       
        var entity = (Entity)target;
            var containsKeys = (entity.contains).Keys.ToArray();
        interactionOptions.Clear(); 
        foreach (Makeup key in containsKeys )
        {
            switch (key)
            { 
                case Makeup.fluid: if(!entity.Tags.Contains(Tag.red)) interactionOptions.Add(new Entity.Command(Act.collectWater, Tag.hasWater,  target, key, -0.1f,new Vector2(0,0))); break;
                case Makeup.food: if (entity.Tags.Contains(Tag.purpleTree) && entity.Tags.Contains(Tag.hasFood)) interactionOptions.Add(new Entity.Command(Act.collectFood, Tag.hasFood, target, key, -0.1f, new Vector2(0,0))); break;
                case Makeup.wood: if (entity.Tags.Contains(Tag.purpleTree) && entity.Tags.Contains(Tag.hasWood)) interactionOptions.Add(new Entity.Command(Act.collectWood, Tag.hasWood, target, key, -0.1f, new Vector2(0, 0))); break;
            }
           
        }
        





        if (radialMenu is RadialMenu menu)
        {
            GlobalPosition = target.GlobalPosition;
            menu.OpenMenu(this, interactionOptions, entity);
        }
    }


    #endregion



    #region OLD
    public override void _Ready()
    {
        Start();
    }

    public override void _PhysicsProcess(double delta)
    {

        Run();
        //this.delta = (float)delta;
    }
    #endregion
}
