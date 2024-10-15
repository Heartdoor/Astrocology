using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Asriela.BasicFunctions;
using static System.Net.Mime.MediaTypeNames;
public partial class Entity : CharacterBody2D
{
    
    #region INITIAL VARIABLES

    public int idNumber = 0;
    public string name = "";
    public float delta;
    public string emoji = "";
    public static Node2D globalSelected;
    public static List<Entity> selectedEntities = new List<Entity>();
    public static List<Entity> overEntities = new List<Entity>();
    public bool clickable=false;
    public bool selected = false;
    public Dictionary<Makeup,float> contains = new Dictionary<Makeup, float>();
    public Dictionary<Makeup, float> maxContainment = new Dictionary<Makeup, float>();

    public Sprite2D shadow;
    
    public Area2D InInteractionArea = null;
    public Area2D area;
    public Area2D clickableArea;
    
    public AnimatedSprite2D animator = null;
    public Vector2 startPosition;
    public bool labelsOn = false;
    public float bodyRange = 0;
    public bool alive = true;
    public Main main; 
    public Node2D inAreaOf;
    #endregion

    #region NEEDS
    public Dictionary<Need, NeedDetails> Needs = new Dictionary<Need, NeedDetails>();


    public class NeedDetails
    {
        public float needValue = 0;
        public float needDecayRate = 0.01f;
        public List<Tag> needTags = new List<Tag>();
        public float interactionNeedReduction = 0;
        public Makeup makeupToTransfer;
        public float transferRate = 0;
        public Tag interactionRemoveTag;
        public Tag interactionAddTag;
        public string animation;

        public NeedDetails(float needValue, float needDecayRate, float interactionNeedReduction,Makeup makeupToTransfer, float transferRate, Tag interactionRemoveTag, Tag interactionAddTag, string animation,params Tag[] needTags)
        {
            this.needValue = needValue;
            this.needDecayRate = needDecayRate;
            this.interactionNeedReduction = interactionNeedReduction;
            this.makeupToTransfer = makeupToTransfer;
            this.transferRate = transferRate;
            this.interactionRemoveTag = interactionRemoveTag;
            this.interactionAddTag = interactionAddTag;
            this.animation = animation;
            
            foreach (Tag tag in needTags)
            {
                this.needTags.Add(tag);
            }
        }

    }

    #endregion
    #region COMMAND
    public List<Command> commandsList = new List<Command>();
    public class Command
    {
        public Act action;
        public Node2D target;
        public float transferRate;
        public Makeup makeupToTransfer;
        public Tag changeTag;
        public Vector2 position;

        public Command(Act action, Tag changeTag, Node2D target, Makeup makeupToTransfer, float transferRate, Vector2 position)
        {
            this.action = action;
            this.target = target;
            this.transferRate = transferRate;
            this.makeupToTransfer = makeupToTransfer;
            this.changeTag = changeTag;
            this.position = position;

        }
    }
    #endregion
    #region TAG VARIABLES
    public Label tagsLabel;
    public List<Tag> Tags = new List<Tag>();
    public List<Tag> ConditionTags = new List<Tag>();
    [Export] public Godot.Collections.Array<Tag> startingTags = new Godot.Collections.Array<Tag>();
    #endregion

    #region SPECIES
    [Export] public Species species = Species.none;

    #endregion

    #region MyRegion
    public static readonly float StandardDecayRate = 0.01f;
    #endregion
    #region INTERACTION
    public Entity alreadyTargetedBy = null;
    #endregion

    #region BASIC FUNCTIONS CONNECTIONS
    Alarms alarm = new Alarms();
    #endregion


    #region START
    void Start()
    {

        Log("TESTED", LogType.game);

        animator = GetNode<AnimatedSprite2D>("AnimatedSprite");


        
        //NODES
        area = GetNode<Area2D>("Area2D");
        clickableArea = GetNode<Area2D>("clickableArea");
        tagsLabel = GetNode<Label>("Node2D/TagsLabel");
        shadow= GetNode<Sprite2D>("Shadow");

         

        //SIGNALS
        clickableArea.MouseEntered += MouseEnteredArea;
        clickableArea.MouseExited += MouseExitedArea;
        area.AreaEntered += EnteredInteractionArea;
        area.AreaExited += LeftInteractionArea;
    }

    #endregion
    #region RUN
    public void RunEntity()
    {
        ManageNeeds();
        UpdateLabel();
        UpdateVisuals();
    }
    #endregion

    #region TAG MANAGEMENT
    public void PassOnStartingTags()
    {
        foreach (Tag tag in startingTags)
        {
            Tags.Add(tag);
        }
    }

    public void UpdateLabel()
    {
        tagsLabel.Text = "";
        if (!labelsOn) return;
        //ADD EMOTIONAL TAGS
        // currentEmotionalTag = UpdateATagGrouping(EmotionalStateToTag(MyEmotionalState), currentEmotionalTag, this);

        //ADD RELATIONSHIP TAGS
        var text = "";
        //ADD MOOD TAGS
        if (!(this is WorldObject w)) { 


        foreach (Tag t in Tags)
        {
            text += $"{t} ";
        }
        var needs = Needs.Keys.ToArray();
        //
        foreach (var need in needs)
        {
            text += $"\n{need} : {(float)Math.Round(Needs[need].needValue, 2)}";
        }

        

        if(this is Creature creature) {

            

          
                //Log("😀MADE IT", LogType.game);
                var en = (Entity)creature.currentTarget;
                if (en != null) 
                text += $"\ntarget is: {en.name} moving to group: {creature.movingToGroupPoint}  ";
            
        }

        if (this is Character character)
        {
            foreach(Command command in commandsList){
                text += $"\ncommand: {command.action}   ";
            }

            text += $"\n backpack: {character.backpackWeight}";


        }


        var keys = contains.Keys.ToArray();
        foreach (var key in keys)
        {
            text += $"\n contains: {key}  {contains[key]}   ";
        }
        }
        else
        {
            if (HasTag(Tags, Tag.unguarded))
                text += $"{inAreaOf}✅";
            else
                text += $"{inAreaOf}❌";
        }


           // text +=$"CLICKABLE {clickable}";
        tagsLabel.Text = text;

    }
    #endregion
    #region MANAGE NEEDS
    void ManageNeeds()
    {
        var needs = Needs.Keys.ToArray();
        //
        foreach (var need in needs)
        {
            Needs[need].needValue += Needs[need].needDecayRate * delta;
        }

    }

    #endregion
    #region CONTAINING
    public bool containmentChangeAtMaxOrMin(float currentAmount, float max, float transfer ) 
    {
        bool ret=false;
        //pos
        if (transfer > 0)
        {
            if (currentAmount>= max)
            {
                ret= true;
            }
        }
        else
        {
            if (currentAmount <= 0)
            {
                ret = true;
            }
        }

           
        return ret;
    }
    #endregion
    #region INITIALIZE SPECIES
    public void ChangeName()
    {
        name += $"{emoji}{species} {idNumber}";
    }
    public void SetupSpecies()
    {
        
        Needs.Clear();
        Creature creature;
        Character character;
        switch (species)
        {
            case Species.water:
                animator.ZIndex = -100;
                animator.ShowBehindParent = true;
                bodyRange = 10;
                labelsOn = BinaryBool(0); 
                emoji = "💧";
                 
                
                ZIndex = -2;
                if (HasTag(Tags, Tag.hasWater))
                {
                    AddMakeup(Makeup.fluid, 100, 100);
                    if (HasTag(Tags, Tag.red))
                    {

                        AddMakeup(Makeup.redFluid, 40, 40);

                        ChangeAnimationFrames(animator, "res://OBJECTS/WORLDOBJECT/FRAMES/waterFrames.tres", "red");
                    }
                    else
                    {
                        AddMakeup(Makeup.redFluid, 40, 0);
                        ChangeAnimationFrames(animator, "res://OBJECTS/WORLDOBJECT/FRAMES/waterFrames.tres", "blue");
                    }
                }
                else
                {
                    AddMakeup(Makeup.fluid, 100, 0);
                    ChangeAnimationFrames(animator, "res://OBJECTS/WORLDOBJECT/FRAMES/waterFrames.tres", "empty");
                }
                break;
            case Species.greenBush:
                bodyRange = 2;
                labelsOn = BinaryBool(0); 
                emoji = "🥦";

                animator.Offset = new Vector2(0, -10);
                AddMakeup(Makeup.food, 50, 50);
                if (HasTag(Tags, Tag.empty))
                {
                    AddMakeup(Makeup.food, 50, 0);
                    ChangeAnimationFrames(animator, "res://OBJECTS/WORLDOBJECT/FRAMES/greenBushFrames.tres", "empty");
                }
                else
                    ChangeAnimationFrames(animator, "res://OBJECTS/WORLDOBJECT/FRAMES/greenBushFrames.tres", "idle");
                break;
            case Species.purpleTree:
                bodyRange = 40;
                labelsOn = BinaryBool(0);
                emoji = "🍄";
                AddMakeup(Makeup.wood, 200, 0);
                AddMakeup(Makeup.food, 200,0);
                shadow.Texture = GetTexture2D("res://OBJECTS/WORLDOBJECT/SPRITES/treePurpleShadow.png");
                animator.Offset = new Vector2(15,-115);
                if (HasTag(Tags, Tag.empty))
                    Tags.Remove(Tag.empty);
                if (HasTag(Tags, Tag.hasFood))
                {
                    if (!HasTag(Tags, Tag.hasWood))
                        Tags.Add(Tag.hasWood);
                    AddMakeup(Makeup.food, 300, 300);
                    ChangeAnimationFrames(animator, "res://OBJECTS/WORLDOBJECT/FRAMES/purpleTreeFrames.tres", "idle");
                }
                else
                    if (HasTag(Tags, Tag.hasWood))
                {
                    if (!HasTag(Tags, Tag.empty))
                        Tags.Add( Tag.empty);
                    AddMakeup(Makeup.wood, 200, 200);
                    ChangeAnimationFrames(animator, "res://OBJECTS/WORLDOBJECT/FRAMES/purpleTreeFrames.tres", "empty");
                }
                    else
                    ChangeAnimationFrames(animator, "res://OBJECTS/WORLDOBJECT/FRAMES/purpleTreeFrames.tres", "stump");
                break;
            case Species.blueHerbivore:
                bodyRange = 40;
                creature = (Creature)this;
                
                labelsOn = BinaryBool(0);
                emoji = "🐃";
                AddMakeup(Makeup.food, 80, 80);
                ChangeAnimationFrames(animator, "res://OBJECTS/CREATURE/FRAMES/blueHerbivoreFrames.tres", "idle");
                creature.myStateMachine.AddActionToDelegate(Act.drink);
                AddNeed(Need.thirst, 0.4f, 0.1f, Makeup.redFluid, -0.1f, Tag.red, Tag.blue, "consume", Tag.water, Tag.red, Tag.hasWater);
                creature.myStateMachine.AddActionToDelegate(Act.eat);
                AddNeed(Need.hunger, 0.6f, 0.1f, Makeup.food, -0.1f, Tag.hasFood, Tag.empty, "consume", Tag.greenBush, Tag.hasFood);
                creature.myStateMachine.AddActionToDelegate(Act.sleep);
                AddNeed(Need.tiredness, 0.3f, 0.1f, Makeup.food, 0.4f, Tag.none, Tag.hasFood, "sleep", Tag.purpleTree,Tag.empty);
                break;

            case Species.greenHerbivore:
                bodyRange = 40;
                creature = (Creature)this;

                labelsOn = BinaryBool(0); 
                emoji = "🐢";
                AddMakeup(Makeup.food, 40, 40);
                creature.myStateMachine.AddActionToDelegate(Act.drink);
                ChangeAnimationFrames(animator, "res://OBJECTS/CREATURE/FRAMES/greenHerbivoreFrames.tres", "idle");
                AddNeed(Need.thirst, 1.2f, 0.1f, Makeup.redFluid,  0.1f, Tag.blue, Tag.red, "consume", Tag.water, Tag.blue, Tag.unguarded);
                
                creature.myStateMachine.AddActionToDelegate(Act.sleep);
                AddNeed(Need.tiredness, 0.3f, 0.1f, Makeup.food, 0.1f, Tag.empty, Tag.hasFood, "sleep", Tag.greenBush, Tag.empty);
                // SetNeedDecay(Need.thirst, 0.01f);
                break;

            case Species.human:
                bodyRange = 40;
                character = (Character)this;

                labelsOn = BinaryBool(0);
                emoji = "👩‍🚀";
                AddMakeup(Makeup.food, 40, 40);
                if(HasTag(Tags, Tag.Sandy))
                ChangeAnimationFrames(animator, "res://OBJECTS/CHARACTER/FRAMES/humanFrames.tres", "idle");
                if (HasTag(Tags, Tag.Fred))
                    ChangeAnimationFrames(animator, "res://OBJECTS/CHARACTER/FRAMES/humanFrames2.tres", "idle");
                character.myStateMachine.AddActionToDelegate(Act.drink);
                AddNeed(Need.thirst, 0.9f, 0.17f, Makeup.fluid, -0.1f, Tag.hasWater, Tag.none, "consume",  Tag.hasWater, Tag.isBuilding);
                character.myStateMachine.AddActionToDelegate(Act.eat);
                AddNeed(Need.hunger, 0.6f, 0.13f, Makeup.food, -0.1f, Tag.hasFood, Tag.none, "consume",  Tag.hasFood, Tag.isBuilding);
                character.myStateMachine.AddActionToDelegate(Act.sleep);
                AddNeed(Need.tiredness, 0.3f, 0.07f, Makeup.energy, 0, Tag.none, Tag.none, "sleep", Tag.hasBed, Tag.isBuilding);

                character.myStateMachine.AddActionToDelegate(Act.collectWater);
                character.myStateMachine.AddActionToDelegate(Act.collectFood);
                character.myStateMachine.AddActionToDelegate(Act.collectWood);
                character.myStateMachine.AddActionToDelegate(Act.standGuard);
                // character.myStateMachine.AddActionToDelegate(Act.unpack);
                // AddNeed(Need.overweight, 0.00f, 0.1f, Makeup.energy, 0, Tag.none, Tag.none, "sleep", Tag.buildingTent);
                break;

            case Species.buildingWaterTower:
                bodyRange = 40;
                labelsOn = BinaryBool(0);
               
                shadow.Offset = new Vector2(-5, 48);
                shadow.Texture = GetTexture2D("res://OBJECTS/WORLDOBJECT/SPRITES/towerShadow.png");
                emoji = "";
               
                if (HasTag(Tags, Tag.hasWater))
                {
                    AddMakeup(Makeup.fluid, 1000, 1);
                    ChangeAnimationFrames(animator, "res://OBJECTS/WORLDOBJECT/FRAMES/buildingWaterTowerFrames.tres", "full");
                }
                else
                {
                    AddMakeup(Makeup.fluid, 1000, 0);
                    ChangeAnimationFrames(animator, "res://OBJECTS/WORLDOBJECT/FRAMES/buildingWaterTowerFrames.tres", "empty");
                }
                    
                break;
            case Species.buildingCapsule:
                bodyRange = 120;
                labelsOn = BinaryBool(0);
                emoji = "🚀";
                AddMakeup(Makeup.fluid, 10000, 1000);
                AddMakeup(Makeup.food, 10000, 1000);
                AddMakeup(Makeup.wood, 10000, 0);
                shadow.Texture = GetTexture2D("res://OBJECTS/WORLDOBJECT/SPRITES/capsuleShadow.png");
                shadow.Offset = new Vector2(0, 56);
                ChangeAnimationFrames(animator, "res://OBJECTS/WORLDOBJECT/FRAMES/buildingCapsuleFrames.tres", "idle");
            
                break;
            case Species.buildingTent:
                bodyRange = 80;
                labelsOn = BinaryBool(0);
                emoji = "⛺";
                
                AddMakeup(Makeup.energy, 10000, 1000);
                ChangeAnimationFrames(animator, "res://OBJECTS/WORLDOBJECT/FRAMES/buildingTentFrames.tres", "idle");

                break;
            case Species.foodPackage:
                bodyRange = 0;
                labelsOn = BinaryBool(0);
                emoji = "⛺";

                ChangeAnimationFrames(animator, "res://OBJECTS/WORLDOBJECT/FRAMES/foodPackageFrames.tres", "idle");

                break;
            case Species.waterPackage:
                bodyRange = 0;
                labelsOn = BinaryBool(0);
                emoji = "⛺";

                ChangeAnimationFrames(animator, "res://OBJECTS/WORLDOBJECT/FRAMES/waterPackageFrames.tres", "idle");

                break;
        }
     
    }
    public void AddMakeup(Makeup makeup, float max, float amount)
    {
        contains[makeup] = amount;
        maxContainment[makeup] = max;
    }
    public void AddSpeciesTag()
    {

        Tag tag = species switch
        {
            Species.water => Tag.water,
            Species.blueHerbivore => Tag.blueHerbivore,
            Species.greenHerbivore => Tag.greenHerbivore,
            Species.greenBush => Tag.greenBush,
            Species.buildingWaterTower => Tag.buildingWaterTower,
            Species.purpleTree => Tag.purpleTree,
            Species.human => Tag.human,
            Species.buildingTent => Tag.buildingTent,
            Species.buildingCapsule => Tag.buildingCapsule,
            Species.waterPackage => Tag.waterPackage,
            Species.foodPackage => Tag.foodPackage,

            _ => Tag.none
        };

        Tags.Add(tag);
    }
    void AddNeed(Need needToAdd, float decayRate, float needReduction, Makeup makeupToTransfer,float transferRate, Tag removeTag, Tag addTag, string animation, params Tag[] tags)
    {
        var obj = new NeedDetails((float)RandomRange(0,25), decayRate, needReduction, makeupToTransfer, transferRate, removeTag, addTag, animation, tags);
        Needs.Add(needToAdd, obj);


        //Log($"{NeedTags[needToAdd][0]}  {NeedTags[needToAdd][1]}", LogType.game);


    }


    public void ChangeTag(Tag tagToChange, Tag tagToBecome)
    {

        Log($"REMOVE TAG: {tagToChange}", LogType.game);
        Log($"ADD TAG: {tagToBecome}", LogType.game);
        if(tagToChange!=Tag.none)
        Tags.Remove(tagToChange);
        if (tagToBecome != Tag.none)
            if (!Tags.Contains(tagToBecome))
            Tags.Add(tagToBecome);
        SetupSpecies();
    }

   // void SetNeedDecay(Need need, float rate)
   // {
      //  NeedDecayRate.Add(need, rate);
    //}
    #endregion


    #region SIGNALS
    public void MouseEnteredArea()
    {
        Entity.globalSelected = this;
        clickable = true;
        overEntities.Add(this);
    }
    public void MouseExitedArea()
    {
        if (Entity.globalSelected==this)
            Entity.globalSelected = null;
        clickable = false;
        overEntities.Remove(this);
    }
    public void EnteredInteractionArea(Area2D area)
    {
  
        InInteractionArea = area;
        inAreaOf = (Node2D )area.GetParent();

        Log($"IN AREA : {InInteractionArea}", LogType.game);
    }
    public void LeftInteractionArea(Area2D area)
    {

        if (InInteractionArea == area)
            InInteractionArea = null;
        inAreaOf = null;
        Log($"LEFT AREA : {InInteractionArea}", LogType.game);


    }
    #endregion




    #region CHANGESPRITE
    public void UpdateVisuals()
    {

         if (clickable)
        {
            if (ButtonPressed("LeftClick"))
            {
                selected = true;
                selectedEntities.Add(this);

            }

            if (ButtonPressed("RightClick"))
            {

            }

            animator.Modulate = new Color(Colors.YellowGreen);
        }

        else
          animator.Modulate = new Color(Colors.White);


    }
    
    #endregion
    #region OLD
    public override void _Ready()
    {
        Start();
    }
    #endregion
}
