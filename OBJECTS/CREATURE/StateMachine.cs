using Godot;
using System;
using System.Collections.Generic;
using static Asriela.BasicFunctions;
using static Godot.WebSocketPeer;
using static System.Collections.Specialized.BitVector32;

public partial class StateMachine : Node2D
{
    #region INITIAL VARIABLES

    public Creature con;
    public Node2D conNode;
    bool initialRunStarted = false;
    public string Section { get; set; }
    public bool HasBegun { get; set; }
    public Action LastState;
    public delegate void FindNextState();
    public FindNextState scoreChecksDelegate;
    public float highestScore = -1;
    public Action newState;
    private int stateChangeInterval = 5;
    public bool canRunState = false;
    public Alarms alarm = new Alarms();
    private string highestStateName;
    private string statesInQue = "";
    private Node2D currentTarget = null;
    private Node2D target = null;
    float distanceToTarget = 0;
    string targetsGrouping;
    string currentTargetsGrouping;
    float score = 0;
    bool failed = false;
    bool thereIsAWinningState = false;
    
    Need need;
    #endregion
    #region RUN AND START
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public void Run()
    {

     

        //TimedFunction(FindOurNextState, 1f, 10f);
        CheckForStateSwitch();
        alarm.Run();
        if (alarm.Ended(TimerType.initialAction) )
        {
            Log("INITIAL ACTION FOR STATE MACHINE", LogType.game);
            initialRunStarted =true;
            FindOurNextState();

            

        }
        if (alarm.Ended(TimerType.actionLength))
            FindOurNextState();


    }

    public void Start()
    {
        Log("STATE MACHINE STARTS", LogType.game);
        SetupDesires();
        alarm.Start(TimerType.initialAction, Range(0.1f,1f), false,0f) ;
        //scoreChecksDelegate += DrinkScore;
        // += "drink-";
    }



    #endregion

//===============
 /// BRAIN 
    /// 
//==============

    public Dictionary<Emo, int> DesireFor = new Dictionary<Emo, int>();
    void SetupDesires()
    {
        DesireFor.Add(Emo.excited, 10);
    }

//===============
 /// STATES 
    /// 
//==============




    void LocalScoreStart(string grouping)
    {
        score = 0;
        target = null;
        targetsGrouping = grouping;
        failed = false;

    }
    bool LocalNeedScoreCalculation(Need theNeed, List<Tag> tags )
    {
        con.Set_Condition_Tags(tags);
        target = FindNearestWithTags(conNode, con.GetTree(), con.ConditionTags, null, targetsGrouping, con.groupPoint, con.maxDistanceFromGroup, ref distanceToTarget);
        Log($"📃{con.name} distance {distanceToTarget}", LogType.state);

        var foundSomeone = false;
        if (target != null)
        {
            foundSomeone = true;
            score = con.Needs[theNeed].needValue * (1 - distanceToTarget / con.maxDistanceFromGroup);
            Log($"📃{con.name} CALCULATING SCORE based of need:{con.Needs[theNeed].needValue} and distance: {(1 - distanceToTarget / con.maxDistanceFromGroup)}", LogType.state);
        }
        else
        {

            Log($"📃{con.name} NO TARGET FOUND", LogType.state);

        }
        return foundSomeone;

    }

    #region DRINK
    void DrinkScore()
    {

        Log($"📃{con.name} DRINK score being calculated", LogType.state);

        LocalScoreStart("WorldObject" );

        if(!LocalNeedScoreCalculation(Need.thirst, con.Needs[Need.thirst].needTags))
        {
            Log("WE FAILED", LogType.state);
            failed = true;
        }
        var entity = (Entity)target;



        Return(score, Drink, "Drink", target, targetsGrouping);
    }

    void Drink()
    {

        if (Begin("begin Drink"))
        {
            Log($"📃{con.name} DOING DRINK STATE", LogType.state);            
            {
                if (con.Goto_AndInteract_WithTarget(Act.drink, Need.thirst, currentTarget, currentTargetsGrouping))
                    Section = "updateScores";
            }
         
            
            
        }

        if (Section == "updateScores")
        {

            Log($"📃{con.name}  EXIT STATE", LogType.state);

            Exit();
        }
    }
    #endregion
    #region EAT
    void EatScore()
    {

        Log($"📃{con.name} HUNGER score being calculated", LogType.state);

        LocalScoreStart("WorldObject");

        if (!LocalNeedScoreCalculation(Need.hunger, con.Needs[Need.hunger].needTags))
        {
            Log("WE FAILED", LogType.state);
            failed = true;
        }
        var entity = (Entity)target;



        Return(score, Eat, "Eat", target, targetsGrouping);
    }

    void Eat()
    {

        if (Begin("begin Hunger"))
        {
              Log($"📃{con.name} DOING EAT STATE", LogType.state);            
            {
                if (con.Goto_AndInteract_WithTarget(Act.eat, Need.hunger, currentTarget, currentTargetsGrouping))
                    Section = "updateScores";
            }



        }

        if (Section == "updateScores")
        {

            Log($"📃{con.name}  EXIT STATE", LogType.state);

            Exit();
        }
    }
    #endregion
    #region SLEEP
    void SleepScore()
    {

        Log($"📃{con.name} Sleep score being calculated", LogType.state);

        LocalScoreStart("WorldObject");

        if (!LocalNeedScoreCalculation(Need.tiredness, con.Needs[Need.tiredness].needTags))
        {
            Log("WE FAILED", LogType.state);
            failed = true;
        }
        var entity = (Entity)target;



        Return(score, Sleep, "sleep", target, targetsGrouping);
    }

    void Sleep()
    {

        if (Begin("begin Sleep"))
        {
            //  Log($"📃{con.name} DOING DRINK STATE", LogType.state);            
            {
                if (con.Goto_AndInteract_WithTarget(Act.sleep, Need.tiredness, currentTarget, currentTargetsGrouping))
                    Section = "updateScores";
            }



        }

        if (Section == "updateScores")
        {

            Log($"📃{con.name}  EXIT STATE", LogType.state);

            Exit();
        }
    }
    #endregion
      #region OPERATION FUNCTIONS
    public void ClearStatesDelegate()
    {
        scoreChecksDelegate = null;
    }
    public void AddActionToDelegate(Act act) 
    {
        var state = "";
        switch(act)
        {
            case Act.drink:
                state = "drink-"; scoreChecksDelegate += DrinkScore;
                
                break;
            case Act.eat:
                state = "eat-"; scoreChecksDelegate += EatScore;
                break;
            case Act.sleep:
                state = "sleep-"; scoreChecksDelegate += SleepScore;
                break;
        };

        statesInQue += state;
        Log($"📃{con.name}  😀{state} was added, count is {scoreChecksDelegate.GetInvocationList().Length}", LogType.game);
    }
    public void FindOurNextState()
    {

        Log($"📃{con.name}  FINDING NEXT STATE", LogType.game);



        Log($"📃{con.name}  {scoreChecksDelegate.GetInvocationList().Length} STATES , they are: {statesInQue} ", LogType.game);

        scoreChecksDelegate.Invoke();

        if (thereIsAWinningState) { 
        var entity = (Entity)currentTarget;
       // Log($"📃{con.name}  TARGET WITH MOST SCORE {entity.name}", LogType.state);
      //  Log($"📃{con.name}  FINNISHED CHECKING SCORES, HSCORE STATE: {highestStateName}", LogType.state);

        HasBegun = true;
        canRunState = true;
        }
        else
        {
            Log($"📃NO WINNING STATE", LogType.state);
            alarm.Start(TimerType.actionLength,2,false,2);
            //
        }

        highestScore = -9999999999999999;

    }
    void RunCurrentState()
    {

        Log("attempting to run state", LogType.state);

        
    }

    void CheckForStateSwitch()
    {
        if (LastState != newState)
        {
            ResetForNewState();
        }
        LastState = newState;

    }

    bool Begin(string sectionName)
    {
        if (HasBegun)
        {
            con.completedJourney = false;
            con.canReachTarget = 0;
            con.hasTarget = false;
            con.foundRandomPlace = false;
            con.animator.Animation = "idle";
        }
 
        con.performedAction = false;
        var ret = HasBegun || Section == sectionName;
        if (ret)
            Section = sectionName;
        HasBegun = false;
        return ret;
    }

    void Exit()
    {
        ResetForNewState();
    }

    void ResetForNewState()
    {

        canRunState = false;
        Section = "";
        HasBegun = true;
        FindOurNextState();
        thereIsAWinningState = false;
        con.animator.Animation = "idle";
    }

    void Return(float score, Action possibleWinnerState, string stateName, Node2D target, string targetsGrouping )
    {
      //  Log($"📃{con.name} 🤍  score of {stateName} is {score} , highest is {highestScore}", LogType.state);

        if (failed==false && score>20 && score > highestScore)
        {
            thereIsAWinningState = true;
            highestScore = score;
            newState = possibleWinnerState;
            highestStateName = stateName;
            currentTarget = target;
           var entity = (Entity)target;
            currentTargetsGrouping =targetsGrouping;
            Log($"📃STATE WON", LogType.state);
            //   Log($"📃{con.name} 🤍  : {stateName} won, target is {entity.name}", LogType.state);
        }
   
            
    }
    #endregion


}