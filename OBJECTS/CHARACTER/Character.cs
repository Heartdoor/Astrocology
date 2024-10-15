using Godot;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using static Asriela.BasicFunctions;
public partial class Character : Entity
{
    #region INITIAL VARIABLES
    public CharacterStateMachine myStateMachine;
    #endregion
    #region BASIC FUNCTIONS CONNECTIONS
    Alarms alarm = new Alarms();
    #endregion
    #region MOVEMENT VARIABLES
    public int canReachTarget = 0;
    NavigationRegion2D navRegion;
    public NavigationAgent2D NavAgent = null;
    public Node2D currentTarget;
    public bool completedJourney;
    public bool hasTarget;
    bool reachedPoint = false;
    Vector2 wanderDestination;
    Node2D lastTarget = null;
    Vector2 direction;
    public bool foundRandomPlace = false;
    Vector2 myVelocity;
    public float interactionDistance = 50;
    public bool performedAction = false;
    int count = 0;
    float restAtPoint = 0;
    Vector2 destination;


    public float accel = 1800f;
    public float speed = 50;
    public float friction = 1300f;
    #endregion

    #region GROUP VARIABLES
    public Vector2 groupPoint;
    public float maxDistanceFromGroup = 1000f;
    public float distanceToGroupPoint = 0f;
    public bool movingToGroupPoint = true;
    #endregion

    #region CHARACTER VARS
    public bool isPlayer = true;
    public Sprite2D ringSprite;
    public Sprite2D dangerSprite;
    public Sprite2D guardSprite;
    public Label actLabel;
    public float backpackWeight = 0;
    public float maxCarryWeight = 100;
    public bool isStandingGuard;
    public bool stopStandingGuard= false;
    public Vector2 guardPosition;
    #endregion
    #region TOGGLES
    bool showNeedsLabel = false;
    #endregion

    #region START
    void Start()
    {
        base._Ready();


        ringSprite = GetNode<Sprite2D>( "SelectedRing");
        dangerSprite = GetNode<Sprite2D>("Danger");
        guardSprite = GetNode<Sprite2D>("Guard");
        actLabel = GetNode<Label>("actNode/act");









        //STATEMACHINE
        if(isPlayer == false)
        {
            //NAV
            navRegion = null; //(NavigationRegion2D)GetParentParentNode(this, "MainNavRegion");
            SetupNavAgent(this, ref NavAgent, interactionDistance);
            NavAgent.TargetReached += ReachTarget;
            NavAgent.VelocityComputed += VelocityComputedSignal; 

            myStateMachine  = new CharacterStateMachine();
            myStateMachine.con = this;
            myStateMachine.Start();
      


            //ALARMS
            alarm.Start(TimerType.FindTarget, 1, true, 0.1f);
            alarm.Start(TimerType.initialAction, 0.1f, false, 0.1f);
            alarm.Start(TimerType.stateMachineLoop, 2f, true, 0.1f);
        }else
            animator.Animation = "idle";

        animator.Play();
    }
    #endregion
    #region RUN
    void Run(float delta)
    {
        base.RunEntity();
        alarm.Run();
        actLabel.Text = "";
        if (isPlayer == false)
        {
            //STATEMACHINE
            if (alive)
                myStateMachine.Run();
            //ACTIVE WHEN WE FINNISHED CALCULATING HIGHEST SCORING STATE
            if (myStateMachine.canRunState && alive)
            {

                // Log("RUNNING STATE ", LogType.game);

                myStateMachine.newState.Invoke();
            }

            if (clickable && ButtonPressed("RightClick") && isStandingGuard && Main.main.doubleRight > 0)
            {
                stopStandingGuard = true;
            }

            guardSprite.Visible = isStandingGuard;
            if (isStandingGuard)
                guardSprite.GlobalPosition = guardPosition;
            isStandingGuard = false;
        }
        else
        {
            animator.SpeedScale = 1.7f;
            PlayerMovement(this, animator,delta, speed, accel, friction);
            if( ButtonPressed("up") )
                Log("UP", LogType.player);

        }

        

        ringSprite.Visible = selected;
        dangerSprite.Visible = false;


   


        if (Needs[Need.hunger].needValue > 80 || Needs[Need.thirst].needValue > 80 || Needs[Need.tiredness].needValue > 80)
            dangerSprite.Visible = true;

        if (Needs[Need.hunger].needValue >= 100 || Needs[Need.thirst].needValue >= 100 || Needs[Need.tiredness].needValue >= 100)
        {
            alive = false;
            animator.Animation = "dead";
        }
            

            if (alive) 
            Main.main.SurvivorCount++;
            else

            Log("DEAD", LogType.game);


        //ACTIVE WHEN WE HAVE DONE ALL THE ACTIONS OF STATE AND WANNA CHECK SCORE AGGAIN
        // if (alarm.Ended(TimerType.actionLength))
        // {
        //myStateMachine.FindOurNextState();
        // }


     //   FlipAnimatedSprite(animator, Velocity);

    }
    #endregion
    #region PLAYER MOVEMENT

    #endregion
    #region AI MOVEMENT

    bool Consume(Act act, Need need, Node2D target)
    {
        var targetEntity = (Entity)target;
        var done = false;
        if (target != null && distanceToGroupPoint <= maxDistanceFromGroup && CheckIfObjectMatchesAllTags(target, Needs[need].needTags))
        {

                var makeupToTransfer = Needs[need].makeupToTransfer;
                var maxTransferReached = false;
                if (containmentChangeAtMaxOrMin(targetEntity.contains[makeupToTransfer], targetEntity.maxContainment[makeupToTransfer], Needs[need].transferRate))
                    maxTransferReached = true;

                if ((maxTransferReached || (Needs[need].needValue <= 0.1)))
                {
                    if (maxTransferReached)
                    {
                        targetEntity.ChangeTag(Needs[need].interactionRemoveTag, Needs[need].interactionAddTag);

                    }

                    Log($"{name} COMPLETED JOURNEY of moving to {targetEntity.name}, ENDING STATE", LogType.game);
                    done = true;
                }
                else
                {
                Needs[need].needValue = Needs[need].needValue - Needs[need].interactionNeedReduction;
                    targetEntity.contains[makeupToTransfer] += Needs[need].transferRate;
                    animator.Animation = Needs[need].animation;

                }

        }
        else
        {
            done = true;
            Log($"{name}  is checking conditions target is {targetEntity.name} | distance is {distanceToGroupPoint}<={maxDistanceFromGroup} | matched tags {CheckIfObjectMatchesAllTags(target, ConditionTags)}", LogType.action);

            Log($"{name}  CANT GO TO TARGET, ENDING STATE", LogType.game);

        }
        return done;
    }
    bool Transfer(Act act, Need need, Node2D target)
    {
        Log("TRANSFER", LogType.game);
        var targetEntity = (Entity)target;
        var done = false;
        var command = commandsList[commandsList.Count - 1];
        var makeupToTransfer = command.makeupToTransfer;
        var maxTransferReached = false;
        float transferRate;

        if (act == Act.unpack)
            transferRate = 0.1f;
        else
            transferRate = command.transferRate;

        if (containmentChangeAtMaxOrMin(targetEntity.contains[makeupToTransfer], targetEntity.maxContainment[makeupToTransfer], transferRate))
            maxTransferReached = true;
 
            if ((maxTransferReached || (act != Act.unpack && backpackWeight >= maxCarryWeight) || (act == Act.unpack && backpackWeight <= 0)))
           {
            if (act == Act.unpack)
                targetEntity.ChangeTag(Tag.none, command.changeTag);
            if (maxTransferReached)
            {

                if (act != Act.unpack)
                    targetEntity.ChangeTag(command.changeTag, Tag.none );

            }

            Log($"{name} COMPLETED JOURNEY of moving to {targetEntity.name}, ENDING STATE", LogType.game);
            done = true;
        }
        else
        {
            if (act == Act.unpack)
                backpackWeight += -transferRate;
            else
                backpackWeight -= transferRate;

            targetEntity.contains[makeupToTransfer] += transferRate;
            animator.Animation = "consume";

        }
            
        return done;
    }

    bool Guard(Act act, Need need, Node2D target)
    {
        var done = false;

        if (alarm.Ended(TimerType.stateMachineLoop))  
        {

            Log("TEST", LogType.weird);

            myStateMachine.FindOurNextState();
        }

        if (stopStandingGuard)
        {
            stopStandingGuard = false;
            done = true;
        }
        return done;
    }
        public bool Goto_AndInteract_WithTarget(Act act, Need need, Node2D target, string targetsGroup)
    {
        
        currentTarget = target;
        canReachTarget++;

        var done = false;
        //IF conditions for chasing target is met goto target and interact



        movingToGroupPoint = false;
        Vector2 destination= new Vector2(0,0);
        
        Log($"ENTERED MOVEMENT {act}", LogType.game);
        if (act == Act.standGuard)
        {
            Log($"{name} setting up guard", LogType.game);
            var command = commandsList[commandsList.Count - 1];
           destination = command.position;
            guardPosition = destination;
            isStandingGuard = true;
            interactionDistance = 5;
        }
        else
        {
            destination = target.GlobalPosition;
            var targetEntity = (Entity)target;
            interactionDistance = targetEntity.bodyRange;
        }


        //INTERACT WITH TARGET
        if (completedJourney)
        {

            if (act== Act.standGuard)
            {
                Log($"{name} doing  guard", LogType.game);
                done = Guard(act, need, target);
            }
            else
            {


                if (need == Need.none)
                {
                    done=Transfer(act, need, target);
                }
                else
                {
                    done=Consume(act, need, target);
                }
            }

        }
         else
            MoveToTarget(destination);




    



        


        //ELSE
        //true
        return done;
    }
    void MoveToTarget(Vector2 destination)
    {
        ChangeApproachDistance(NavAgent, interactionDistance);
        Log($"{name}  GOTO TARGET {destination}", LogType.game);
        //var destination = 
        GotoPointAndAvoid(this, NavAgent, speed, friction, accel, delta, destination, completedJourney);
    }
    void MoveToGroupPoint()
    {

        if (movingToGroupPoint)
        {
            //  Log($"{name}  GOTO group", LogType.game);
            // GotoPointAndAvoid(this, NavAgent, speed, friction, accel, delta, destination, completedJourney);
        }
        movingToGroupPoint = true;
    }

    void CalculateDistanceToGroupPoint()
    {
        distanceToGroupPoint = 0;
    }
    #endregion
    #region FIND THINGS

    public void Find_Random_Place()
    {

        // wanderDestination = ChangePosition(this.Position, (float)RandomRange(1, 10), (float)RandomRange(1, 10));

        do
        {
            wanderDestination = ChangePosition(GlobalPosition, (float)RandomRange(-400, 400), (float)RandomRange(-400, 400));
        }
        while (!IsPointInNavigationRegion2D(wanderDestination, navRegion));

        ChangeApproachDistance(NavAgent, 0f);
        completedJourney = false;
        foundRandomPlace = true;
    }



    #endregion

    #region UPDATE TAGS
    public void Set_Condition_Tags(List<Tag> allTheTags)
    {
        ConditionTags.Clear();
        foreach (Tag tag in allTheTags)
        {
            ConditionTags.Add(tag);

            Log($"adding condition {tag}", LogType.state);

        }

    }



    #endregion
    //============================================
    #region SIGNALS
    public void ReachTarget()
    {
        var en = (Entity)currentTarget;


        if (canReachTarget < 3) return;
        if (currentTarget == null)
        {
            if (performedAction == false)
            {
                Log("REACHED", LogType.nearest);


                completedJourney = true;
            }
            foundRandomPlace = false;
            completedJourney = true;
        }
        else
        {
            //Log($"{name}  REACHED {en.name}", LogType.game);
            // Log("REACHED TARGET", LogType.nearest);
            completedJourney = true;

        }

        restAtPoint = (float)RandomRange(1, 6);

    }
    public void VelocityComputedSignal(Vector2 safeVelocity)
    {
        Velocity = safeVelocity;
        MoveAndSlide();

    }

    #endregion
    #region OLD
    public override void _Ready()
    {
        Start();
    }

    public override void _PhysicsProcess(double delta)
    {
        Run((float)delta);
    }
    #endregion
}
