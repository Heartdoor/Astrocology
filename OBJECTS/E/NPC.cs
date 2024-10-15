/*using Godot;
using System;
using System.Xml.Linq;
using static Asriela.BasicFunctions;

public partial class NPC : Contestant
{
    #region BASIC VARIABLES
    public NPC()
    {
        SetPhysicsProcess(true);
    }
    public string className = "npc";
    public NavigationAgent2D NavAgent = null;

    bool reachedTarget;
    public bool foundTarget;
    public StateMachine myStateMachine = new StateMachine();
    public bool didCalculation = false;

    [Export] NavigationRegion2D navRegion;
    bool reachedPoint = false;

    Vector2 wanderDestination;
    Node2D lastTarget = null;
    Vector2 direction;
    public bool foundRandomPlace = false;
    Vector2 myVelocity;
    float interactionDistance = 150;
    public bool performedAction = false;
    int count = 0;
    float restAtPoint = 0;
    #endregion

    #region START
    public void Start()
    {

        navRegion = (NavigationRegion2D)GetParentParentNode(this, "MainNavRegion");
        Name = "NPC";
        SetupNavAgent(this, ref NavAgent, interactionDistance);


        myStateMachine.con = this;
        myStateMachine.Start();
        base._Ready(); //this will call Contestant's _Ready and hook up the signals
        alarm.Start(TimerType.FindTarget, 10, true, 0.1f);
        alarm.Start(TimerType.initialAction, 0.1f, false, 0.1f);


        NavAgent.TargetReached += ReachTarget;
        NavAgent.VelocityComputed += VelocityComputedSignal;

        myVelocity = Velocity;
    }
    #endregion
    #region SIGNALS
    public void VelocityComputedSignal(Vector2 safeVelocity)
    {
        Velocity = safeVelocity;
        MoveAndSlide();
        didCalculation = true;
    }
    #endregion
    #region RUN
    public void Run(float delta)
    {


        base.Run();

        myStateMachine.Run();
        if (myStateMachine.canRunState)
            myStateMachine.newState.Invoke();
        if (alarm.Ended(TimerType.initialAction))
            ChangeEmotion(Emo.neutral);
        if (alarm.Ended(TimerType.actionLength))
        {
            myStateMachine.FindOurNextState();
        }


        var destination = wanderDestination;

        if (currentTarget != null)
            destination = currentTarget.GlobalPosition;

        if ((!beingSocialed && socializing == null && restAtPoint <= 0))
            GotoPointAndAvoid(this, NavAgent, speed, friction, accel, delta, destination, reachedTarget);
        restAtPoint = restAtPoint > 0 ? restAtPoint -= 0.01f : restAtPoint = 0;
    }
    #endregion



    public void Set_Condition_Tags(params Tag[] allTheTags)
    {
        ConditionTags.Clear();
        foreach (Tag tag in allTheTags)
        {
            ConditionTags.Add(tag);
        }

    }

    public void Find_Target()
    {

        ChangeApproachDistance(NavAgent, interactionDistance);

        currentTarget = FindNearestWithTags(this, GetTree(), ConditionTags, lastTarget, "Contestant");
        var con = (Contestant)currentTarget;

        con.alreadyTargetedBy = this;
        lastTarget = currentTarget;
        Log($"found target: {currentTarget}", LogType.state);

        reachedTarget = false;
        foundTarget = true;


    }



    public void Find_Random_Place()
    {

       // wanderDestination = ChangePosition(this.Position, (float)RandomRange(1, 10), (float)RandomRange(1, 10));
      
                do
               {
                    wanderDestination = ChangePosition(GlobalPosition, (float)RandomRange(-400, 400), (float)RandomRange(-400, 400));
                }
                while (!IsPointInNavigationRegion2D(wanderDestination,navRegion));

        ChangeApproachDistance(NavAgent, 0f);
        reachedTarget = false;
        foundRandomPlace = true;
    }

    public void ReachTarget()
    {

        if (currentTarget == null)
        {
            if (performedAction == false)
            {
                Log("REACHED", LogType.nearest);


                reachedTarget = true;
            }
            foundRandomPlace = false;
            reachedTarget = true;
        }
        else
        {
            reachedTarget = true;
        
        }

        restAtPoint = (float)RandomRange(1, 6);
        
    }


    public bool FindContestant_Goto_AndInteract(Act interactionType)
    {
        if (socialEnergy >= enoughSocialEnergy)
        {

            Log("SOCIAL", LogType.error);

            if (foundTarget == false)
                Find_Target();

        }
        else
        {
            if(foundRandomPlace==false)
            Find_Random_Place();
        }
        Log("FiNDING CONTESTANT TO INTERACT WITH", LogType.game);



        if (reachedTarget && foundTarget)
        {

            var target = (Contestant)currentTarget;



            reachedTarget = false;

            PerformSocialAction(interactionType, target);

            performedAction = true;
            var con = (Contestant)currentTarget;
            con.alreadyTargetedBy = null;
            currentTarget = null;
            myStateMachine.canRunState = false;
            alarm.Start(TimerType.actionLength, 1f, false, 0f);
            return true;
        }
        else
        {

            return false;
        }
           
    }

    #region OLD
    public override void _PhysicsProcess(double delta)
    {

        Run((float)delta);
    }



    #endregion
}
*/