using Godot;
using System;
using System.Collections.Generic;
using static Asriela.BasicFunctions;
public partial class Creature : Entity
{
    #region INITIAL VARIABLES
    public StateMachine myStateMachine = new StateMachine();
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
    public float speed = 40;
    public float friction = 1300f;
    #endregion

    #region GROUP VARIABLES
    public Vector2 groupPoint;
    public float maxDistanceFromGroup= 1000f;
    public float distanceToGroupPoint=0f;
    public bool movingToGroupPoint=true;
    #endregion


    #region TOGGLES
    bool showNeedsLabel = false;
    #endregion

    #region START
    void Start()
    {
        base._Ready();


        //NAV

        navRegion = null;// GetParent().GetParent().GetParent().GetParent().GetNode<NavigationRegion2D>("MainNavRegion"); 
        SetupNavAgent(this, ref NavAgent, interactionDistance);
        NavAgent.TargetReached += ReachTarget;
        NavAgent.VelocityComputed += VelocityComputedSignal;








        //STATEMACHINE
        myStateMachine.con = this;
        myStateMachine.Start();

        //ALARMS
        alarm.Start(TimerType.FindTarget, 1, true, 0.1f);
        alarm.Start(TimerType.initialAction, 0.1f, false, 0.1f);

    }
    #endregion
    #region RUN
    public void Run()
    {
        base.RunEntity();
        alarm.Run();

        //STATEMACHINE
        myStateMachine.Run();
            //ACTIVE WHEN WE FINNISHED CALCULATING HIGHEST SCORING STATE
            if (myStateMachine.canRunState)
            {

           // Log("RUNNING STATE ", LogType.game);

            myStateMachine.newState.Invoke();
            }
                

            //ACTIVE WHEN WE HAVE DONE ALL THE ACTIONS OF STATE AND WANNA CHECK SCORE AGGAIN
           // if (alarm.Ended(TimerType.actionLength))
           // {
//myStateMachine.FindOurNextState();
           // }


        FlipAnimatedSprite(animator, Velocity);
        MoveToGroupPoint();
        CalculateDistanceToGroupPoint();
    }
    #endregion
    #region MOVEMENT
    public bool Goto_AndInteract_WithTarget(Act act, Need need, Node2D target, string targetsGroup)
    {
        currentTarget = target;
        canReachTarget++;



        var targetEntity = (Entity)target;
        interactionDistance = targetEntity.bodyRange;
        var done = false;
        //IF conditions for chasing target is met goto target and interact
     
        if (target != null && distanceToGroupPoint<= maxDistanceFromGroup && CheckIfObjectMatchesAllTags(target, Needs[need].needTags))
        {
            //Log("we can still head to target", LogType.game);
            movingToGroupPoint = false;
            if (completedJourney)
            {
                var makeupToTransfer = Needs[need].makeupToTransfer;
                var maxTransferReached = false;
                if (containmentChangeAtMaxOrMin(targetEntity.contains[makeupToTransfer], targetEntity.maxContainment[makeupToTransfer], Needs[need].transferRate))
                    maxTransferReached = true;
                //INTERACT WITH TARGET
                if ((maxTransferReached || Needs[need].needValue<=0.1) )
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
                    Needs[need].needValue = Mathf.Clamp(Needs[need].needValue - Needs[need].interactionNeedReduction, 0, 100);
                    targetEntity.contains[makeupToTransfer] += Needs[need].transferRate;
                    animator.Animation = Needs[need].animation;
       
                }

                

                
                
            }
            else
                MoveToTarget(target, targetsGroup);


        }
        else
        {
            done = true;
            Log($"{name}  is checking conditions target is {targetEntity.name} | distance is {distanceToGroupPoint}<={maxDistanceFromGroup} | matched tags {CheckIfObjectMatchesAllTags(target, ConditionTags)}", LogType.action);

            Log($"{name}  CANT GO TO TARGET, ENDING STATE", LogType.game);

        }
            
        //ELSE
        //true
        return done;
    }
    void MoveToTarget(Node2D target, string targetsGroup) 
    {
        ChangeApproachDistance(NavAgent, interactionDistance);
       // Log($"{name}  GOTO TARGET", LogType.game);
        var destination = target.GlobalPosition;
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

        Run();
        this.delta = (float)delta;
    }
#endregion
}
