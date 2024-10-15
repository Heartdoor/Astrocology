                                                                                                                                                                                                                                                                                                                                                                                                                    using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Asriela.BasicFunctions;

namespace Asriela
{
    public partial class BasicFunctions : CharacterBody2D
    {
        #region SETUP VARIABLES
        public static RandomNumberGenerator Random = new RandomNumberGenerator();

        public override void _Ready()
        {
            Random.Randomize();
        }
        public static float Range(float low, float high)
        {
            return Random.RandfRange(low, high);
        }
        #endregion
        #region UPDATE VARIABLES

        public override void _PhysicsProcess(double delta)
        {
            globalTimer += (float)delta;

          

        }
        #endregion

        #region TRICKS

        public static bool BinaryBool(int binary)
        {
            return binary == 0 ? false : true;
        }
        #endregion
        #region LOG
        public enum LogType : short
        {
            alarms,
            nearest,
            action,
            state,
            timedFunction,
            game,
            error,
            file,
            ui,
            weird,
            player
        }

        private static Dictionary<LogType, bool> LogTypesOn = new Dictionary<LogType, bool>()
    {
        { LogType.alarms,           BinaryBool(0)},
        { LogType.nearest,          BinaryBool(0)},
        { LogType.action,           BinaryBool(0)},
        { LogType.timedFunction,    BinaryBool(0)},
        { LogType.state,            BinaryBool(0)},
        { LogType.game,             BinaryBool(0)},
        { LogType.file,             BinaryBool(0)},
        { LogType.error,            BinaryBool(0)},
        { LogType.ui,               BinaryBool(0)},
        { LogType.player,               BinaryBool(1)},
        { LogType.weird,            BinaryBool(0)}
    };


        public static void Log(string text, LogType type)
        {
            if (type == LogType.error || type== LogType.ui)
            {
                text = "ERROR! " + text;
                if (LogTypesOn[type] == true)
                    GD.Print(text);
            }

            if (LogTypesOn[type] == true)
                GD.Print(text);
        }

        #endregion

        #region EMOTIONS
        public enum Trait : short
        {
            neutral,
            emo,
            jokester,
            hothead,
            flirt


        }
        public enum Emo : short
        {
            none,
            neutral,
            stoic,
            happy,
            excited,
            jokey,                    
            confident,
            sad,
            depressed,
            angry,
            furious,
            frustrated,
            flirty,
            aroused     
        }

        public enum EmoDir: short
        {
           with,
           against
        }
        public static Emo ConvertStringToEmo(string emoAsString)
        {
            return emoAsString switch
            {
                "sad" => Emo.sad,
                "happy" => Emo.happy,
                "angry" => Emo.angry,
                "flirty" => Emo.flirty,
                "neutral" => Emo.neutral,
                _ => Emo.none
            };
        }

        public static Tag EmotionalStateToTag(Emo EmotionalState)
        {
            return EmotionalState switch
            {
               // Emo.sad => Tag.sad,
               
                _ => Tag.none
            };
        }
        #endregion

        #region NEEDS
        public enum Need : short
        {
            none,
            hunger,
            thirst,
            tiredness,
            aggression,
            overweight
        }
        #endregion

        #region INTERACTIONS
        public enum Act : short
        {
           eat, 
           drink,
           sleep,
           attack,
            unpack,
            collectWater,
            collectFood,
            collectWood,
            standGuard

        }

        #endregion

        #region NEAREST
        public enum Tag
        {
            none,
            destroy,
            full,
            empty,
            red,
            hasWater,
            hasFood,
            water,
            blue,
            human,
            blueHerbivore,
            greenHerbivore,
            purpleTree,
            greenBush,
            buildingTent,
            buildingCapsule,
            waterPackage,
            foodPackage,
            buildingWaterTower,
            hasWood,
            Sandy,
            Fred,
            isBuilding,
            hasBed,
            unguarded
        }

        public enum Species
        {
            none, 
            human,
            water,
            blueHerbivore,
            greenHerbivore,
            buildingTent,
            buildingCapsule,
            waterPackage,
            foodPackage,
            purpleTree,
            greenBush,
            buildingWaterTower

        }

        public enum Makeup
        {
            none,
            fluid,
            redFluid,
            food,
            energy,
            decay,
            wood
        }



        public static Godot.Collections.Array<Godot.Node> GetAllObjects(SceneTree tree, string groupName)
        {
            return tree.GetNodesInGroup(groupName);

        }

        public static float DistanceToNearestObject(Node2D subject, SceneTree tree, string groupName)
        {
            var ret = 0f;
            var allObjects = GetAllObjects(tree, groupName);
            List<Node2D> objectsWithMatchingTags = new List<Node2D>();
            float nearestDistance = 99999999999999;
            Node2D nearestObject = null;

            foreach (Node2D obj in allObjects)
            {
                if (subject != obj)
                {
                    float distance = MeasureDistance(subject, obj);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestObject = obj;

                    }
                }
            }
            ret = nearestDistance;
            return ret;
        }
        public static Node2D FindNearest(Node2D subject, SceneTree tree, Node2D mayNotBe, string groupName)
        {
            var allObjects = GetAllObjects(tree, groupName);
            List<Node2D> objectsWithMatchingTags = new List<Node2D>();

            float nearestDistance = 99999999999999;
            Node2D nearestObject = null;

            foreach (Node2D obj in allObjects)
            {
                if (subject != obj && (mayNotBe != obj))
                {
                    float distance = MeasureDistance(subject, obj);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestObject = obj;

                    }
                }
            }

            Log($"   {nearestObject.Name} was the closest  ", LogType.nearest);
            return nearestObject;
        }


        public static Node2D FindNearestWithTags(Node2D subject, SceneTree tree, List<Tag> objectTypeTags, Node2D mayNotBe, string groupName, Vector2 pointToMeasureFrom, float maxDistance, ref float theDistanceToTarget)
        {
            Log($"Starting search for nearest with tags", LogType.nearest);
            var allObjects = GetAllObjects(tree, groupName);
            List<Node2D> objectsWithMatchingTags = new List<Node2D>();
            Log($"found {allObjects.Count} objects from group {groupName}", LogType.nearest);
            Log($"amount of objects to ch {allObjects.Count}...", LogType.nearest);




            string targName;

            for (int i = 0; i < allObjects.Count; i++) {
                //Log($"Checking tags of ...", LogType.nearest);
      
                var obj = (Node2D)allObjects[i];
                targName = ((Entity)obj).name;

                if (subject != obj && ( mayNotBe!=obj) )
                {//&& con.alreadyTargetedBy==null

                    

                    if (CheckIfObjectMatchesAllTags(obj, objectTypeTags))
                    {
                        Log($"🏆{targName} matched all tags", LogType.nearest);
                        objectsWithMatchingTags.Add(obj);
                    }
                    else
                    {
                        Log($"{targName} didnt match all the tags", LogType.nearest);
                  
                    }
                        


                }
            }
            if(objectsWithMatchingTags.Count==0)
                Log($"  found no matches", LogType.nearest);
            float nearestDistance = 99999999999999;
            Node2D nearestObject = null;
            if (objectsWithMatchingTags.Count > 0)
            {
                foreach (Node2D obj in objectsWithMatchingTags)
                {

                    float distance = MeasureDistance(subject, obj);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestObject = obj;

                    }
                }
            }
            else
                nearestObject = null;
            Log($" was the closest  ", LogType.nearest);
            return nearestObject;
        }
        public static float MeasureDistance(Node2D subjectNode, Node2D objNode)
        {
            if (subjectNode != null && objNode != null)
                if (subjectNode is CharacterBody2D subjectNode2D)
                    if (objNode is CharacterBody2D objNode2D)
                    {
                        Vector2 subjectPosition = subjectNode2D.GlobalPosition;
                        Vector2 objPosition = objNode2D.GlobalPosition;

                        // Calculate the distance between positions
                        float distance = subjectPosition.DistanceTo(objPosition);

                        return distance;
                    }

            // Return a default value or handle the case where nodes are not valid
            return 0f;
        }

        public static bool HasTag(List<Tag> Tags, Tag tag )
        {
            if (Tags.Contains(tag))
            {
                return true;
            }
            else
                return false;
        }   
        public static bool CheckIfObjectMatchesAllTags(Node obj, List<Tag> objectTypeTags)
        {
            var allMatch = true;

            foreach (Tag tag in objectTypeTags)
            {
                if (obj is Entity entity)
                {

                    //Log($"❌contains? {tag} has {entity.Tags[0]} {entity.Tags[1]} ", LogType.game);

                    if (entity.Tags.Contains(tag)==false) // Check if the object has the specified tag
                    {
                        allMatch = false;
                        break; // Exit the loop if any tag doesn't match
                    }
                }
            }

            return allMatch;
        }

        public static Tag UpdateATagGrouping(Tag newTag, Tag lastTag, Entity entity)
        {
            if (newTag != Tag.none)
            {
                if (lastTag != Tag.none)
                    entity.Tags.Remove(lastTag);
                entity.Tags.Add(newTag);
            }
            
            return newTag;
        }


        #endregion

        #region TIMED_FUNCTION
        private static Dictionary<string, Alarm> timedFunction = new Dictionary<string, Alarm>();


        public static void TimedFunction(Action function, float initialTime, float repeatingTime)
        {
            var stringKey = function.GetHashCode().ToString();

            if (!timedFunction.ContainsKey(stringKey))
                timedFunction.Add(stringKey, new Alarm(repeatingTime, false, true, globalTimer + initialTime));


            var alarm = timedFunction[stringKey];

            if (globalTimer > alarm.EndTime)
            {
                function.Invoke();
                Log($"{stringKey} has performed its function", LogType.timedFunction);
                alarm.EndTime = globalTimer + repeatingTime;
            }

        }
        #endregion

        #region ALARM
        public enum TimerType
        {
            initialAction,
            physicsStep,
            performance,
            stateMachine,
            spawnInterval,
            spawnInterval2,
            actionLength,
            FindTarget,
            life,
            moveOn,
            stateMachineLoop
        }

        public static float globalTimer = 0;
        class Alarm
        {
            public float TimeLength { get; set; }
            public bool Triggered { get; set; }
            public bool Loops { get; set; }
            public float EndTime { get; set; }
            public int Count { get; set; }

            public int Ended { get; set; }


            public Alarm(float TimeLength, bool Triggered, bool Loops, float EndTime)
            {
                this.TimeLength = TimeLength;
                this.Triggered = Triggered;
                this.Loops = Loops;
                this.EndTime = EndTime;
                Ended = 0;
                Count = 0;
            }
        }

        public class Alarms
        {


            Dictionary<TimerType, Alarm> alarmsList = new Dictionary<TimerType, Alarm>();

            public bool initialized;
            public bool hasBeenSetup { get; set; } = false;


            public void Start(TimerType timerType, float timeLength, bool loop, float firstLength)
            {
                Log("Created new Alarm: " + timerType, LogType.alarms);
                if (alarmsList.ContainsKey(timerType))
                {
                    alarmsList[timerType].Triggered = false;
                    alarmsList[timerType].TimeLength = timeLength;
                    if (loop) { 

                        alarmsList[timerType].EndTime = 1;
                    }
                    else
                        alarmsList[timerType].EndTime = globalTimer + timeLength;
                    alarmsList[timerType].Loops = loop;
                }
                else
                {
                    var endTime = 0f;
                    var triggered = false;
                    if (loop)
                    {
                        endTime = globalTimer + 2;
                        triggered = true;
                    }
                    else
                    {
                        endTime = globalTimer + timeLength;
                    }
                        

                    alarmsList.Add(timerType, new Alarm(timeLength, triggered, loop, endTime));
                }
                    
            }
            public void Run()
            {
                var keys = alarmsList.Keys.ToArray();
                //
                foreach (var key in keys)
                {
                    TimerType tempType = key;

          
             




                    
              
                    if (alarmsList[key].Triggered)
                    {




                        if (alarmsList[key].Loops)
                        {
                            Log($"LOOPING ALARM {tempType} FINNISHED [{alarmsList[key].Count}] => next alarm at {alarmsList[key].EndTime}", LogType.alarms);
                            alarmsList[key].EndTime = globalTimer + alarmsList[key].TimeLength;
                            alarmsList[key].Count++;
                            Log($"LOOPING ALARM {tempType} end time is: {alarmsList[key].EndTime}", LogType.alarms);


                        }
                        else
                        {
                            Log("NON LOOPING ALARM FINNISHED " + alarmsList[key].Count, LogType.alarms);
                            alarmsList[key].EndTime = -1;

                        }
                            
                    }

                    alarmsList[key].Triggered = (globalTimer > alarmsList[key].EndTime) && !(alarmsList[key].EndTime==-1);

                }
            }

            public bool Ended(TimerType timerType)
            {
                //if (alarmsList[timerType].Triggered)
               
                if (alarmsList.ContainsKey(timerType) )
                {
                    var ret = false;
                    

                    ret= alarmsList[timerType].Triggered;
                    if (ret)Log($"ALARM {timerType}, ENDED CHECK IS TRUE", LogType.alarms);
                    
                    return ret;

                }
   
                else
                    return false;
            }
        }
        #endregion

        #region BUTTON


        public static bool ButtonPressed(string actionName)
        {
            return Input.IsActionJustPressed(actionName);
        }

        #endregion

        #region PLAYER
        public static void PlayerMovementRidgid(CharacterBody2D player, float speed)
        {
            Vector2 move_input = Input.GetVector("left", "right", "up", "down");
            player.Velocity = move_input * (1 * speed);

            player.MoveAndSlide();
        }
        public static void PlayerMovement(CharacterBody2D subject, AnimatedSprite2D animator, float delta, float maxSpeed, float accel, float friction)
        {
            Vector2 input = Input.GetVector("left", "right", "up", "down");


            if (input == Vector2.Zero)
            {
                if (subject.Velocity.Length() > (friction * delta))
                    subject.Velocity -= subject.Velocity.Normalized() * (friction * delta);
                else
                    subject.Velocity = Vector2.Zero;
            }
            else
            {
                subject.Velocity += (input * accel * delta);
                subject.Velocity = subject.Velocity.LimitLength(maxSpeed);

            }


            if (input.X != 0)
            {
                // If moving right, set scale.x to positive value
                animator.FlipH = true;
                if (animator.Animation != "walk")
                {
                    animator.Play();
                    animator.Animation = "walk";
                }
            }
            if (input.X < 0)
            {
                // If moving left, set scale.x to negative value
                animator.FlipH = false;

                    
            }

            if (input.X != 0 || input.Y != 0)
            {
                if (animator.Animation != "walk")
                {
                    animator.Play();
                    animator.Animation = "walk";
                }
            }
            else


                animator.Animation = "idle";
            subject.MoveAndSlide();
        }
        #endregion

        #region MOVEMENT
        public static void GotoTargetSmoothly(CharacterBody2D subject, NavigationAgent2D nav, float maxSpeed, float friction, float accel, float delta, Node2D target, bool completedJourney)
        {
            var canMove = false;
            if (target != null )
            {
                nav.TargetPosition = target.GlobalPosition;
                if (nav.IsTargetReachable() && !nav.IsTargetReached() && completedJourney == false)
                {
                    var nextLocation = nav.GetNextPathPosition();
                    var direction = subject.GlobalPosition.DirectionTo(nextLocation);
                    subject.Velocity += (direction * accel * delta);
                    subject.Velocity = subject.Velocity.LimitLength(maxSpeed);
                    // subject.Velocity = direction * maxSpeed;
                    subject.MoveAndSlide();
                    canMove = true;
                }


            }

            if (!canMove)
            {
                if (subject.Velocity.Length() > (friction * delta))
                    subject.Velocity -= subject.Velocity.Normalized() * (friction * delta);
                else
                    subject.Velocity = Vector2.Zero;
            }

            // Log($"POS {nav.TargetPosition} THE TARGET {target} THE SUBJECT {subject} THE NAV {nav}", LogType.nearest);

        }

        public static void GotoPointSmoothly(CharacterBody2D subject, NavigationAgent2D nav, float maxSpeed, float friction, float accel, float delta, Vector2 destination, bool completedJourney)
        {
            var canMove = false;
          
                nav.TargetPosition = destination;
                if (nav.IsTargetReachable() && !nav.IsTargetReached() && completedJourney == false)
                {
                    var nextLocation = nav.GetNextPathPosition();
                    var direction = subject.GlobalPosition.DirectionTo(nextLocation);
                    subject.Velocity += (direction * accel * delta);
                    subject.Velocity = subject.Velocity.LimitLength(maxSpeed);
                    // subject.Velocity = direction * maxSpeed;
                    subject.MoveAndSlide();
                    canMove = true;
                }



            if (!canMove)
            {
                if (subject.Velocity.Length() > (friction * delta))
                    subject.Velocity -= subject.Velocity.Normalized() * (friction * delta);
                else
                    subject.Velocity = Vector2.Zero;
            }

            // Log($"POS {nav.TargetPosition} THE TARGET {target} THE SUBJECT {subject} THE NAV {nav}", LogType.nearest);

        }

        public static void GotoPointSmoothlyAndAvoid(CharacterBody2D subject, NavigationAgent2D nav, float maxSpeed, float friction, float accel, float delta, Vector2 destination, bool completedJourney, Vector2 MyVelocity)
        {
            var canMove = false;

            nav.TargetPosition = destination;
            if (nav.IsTargetReachable() && !nav.IsTargetReached() && completedJourney == false)
            {
                var nextLocation = nav.GetNextPathPosition();
                var direction = subject.GlobalPosition.DirectionTo(nextLocation);

                MyVelocity += (direction * accel * delta);
                MyVelocity = MyVelocity.LimitLength(maxSpeed);
                 subject.Velocity = MyVelocity;
                subject.MoveAndSlide();
                canMove = true;

                //nav.SetVelocityForced(subject.Velocity);
            }



            if (!canMove)
            {
                if (MyVelocity.Length() > (friction * delta))
                    MyVelocity -= MyVelocity.Normalized() * (friction * delta);
                else
                    MyVelocity = Vector2.Zero;
            }

            // Log($"POS {nav.TargetPosition} THE TARGET {target} THE SUBJECT {subject} THE NAV {nav}", LogType.nearest);

        }

        public static void GotoPointAndAvoid(CharacterBody2D subject, NavigationAgent2D nav, float maxSpeed, float friction, float accel, float delta, Vector2 destination, bool completedJourney)
        {
            /*
                public override void _Ready()
                {
                NavAgent.VelocityComputed += VelocityComputedSignal;
                }

                public void VelocityComputedSignal(Vector2 safeVelocity)
                {
                    Velocity = safeVelocity;
                    MoveAndSlide();
                }
            */

            nav.TargetPosition = destination;

                var nextLocation = subject.ToLocal(nav.GetNextPathPosition()).Normalized();

                var intendedVelocity = nextLocation * maxSpeed;
                subject.Velocity = nextLocation * maxSpeed;

            
                subject.MoveAndSlide();


                nav.SetVelocityForced(intendedVelocity);




        }

        public static void SetupNavAgent(Node2D subject, ref NavigationAgent2D NavAgent , float interactionDistance)
        {
            NavAgent = subject.GetNode<NavigationAgent2D>("NavigationAgent2D");
            NavAgent.TargetDesiredDistance = interactionDistance;
            //NavAgent.PathDesiredDistance = 10f;
        }


        public static void ChangeApproachDistance( NavigationAgent2D NavAgent, float interactionDistance)
        {
            NavAgent.TargetDesiredDistance = interactionDistance;
        }

        public static void GotoPoint(CharacterBody2D subject, NavigationAgent2D nav, float maxSpeed, float accel, float delta, Vector2 position)
        {
            nav.TargetPosition = position;
            if (nav.IsTargetReachable() && !nav.IsTargetReached())
            {
                var nextLocation = nav.GetNextPathPosition();
                var direction = subject.GlobalPosition.DirectionTo(nextLocation);

                subject.Velocity = direction * maxSpeed;
                subject.MoveAndSlide();
            }

        }
        public static void GotoTarget(CharacterBody2D subject, NavigationAgent2D nav, float maxSpeed, float accel, float delta, Node2D target)
        {
            nav.TargetPosition = target.GlobalPosition;
            if (nav.IsTargetReachable() && !nav.IsTargetReached())
            {
                var nextLocation = nav.GetNextPathPosition();
                var direction = subject.GlobalPosition.DirectionTo(nextLocation);

                 subject.Velocity = direction * maxSpeed;
                subject.MoveAndSlide();
            }


            Log($"POS {nav.TargetPosition} THE TARGET {target} THE SUBJECT {subject} THE NAV {nav}", LogType.nearest);

            

        }

        #endregion

        #region RANDOM


        public static double RandomRange(double first, double second)
        {
            return GD.RandRange(first, second);
        }


        public static T Choose<T>(params T[] args)
        {
            var result=(int)RandomRange(0, args.Length);

            return args[result];
        }
        #endregion

        #region CREATE
        public static Node2D Add2DNode(string path, Node subject )
        {
            PackedScene armScene = (PackedScene)GD.Load($"res://OBJECTS/{path}");
            Node2D arm = (Node2D)armScene.Instantiate();

            subject.AddChild(arm);


            return arm;

        }
        public static PackedScene GetObject(string path)
        {
            return (PackedScene)ResourceLoader.Load("res://OBJECTS/" + path);
        }
        public static Node SpawnRandom(PackedScene objectToSpawn, Vector2 position, float innerRange, float outerRange, Node nodeToSpawnOn)
        {
            float angle = Range(0, Mathf.Tau);
            var randomDistance = Range(innerRange, outerRange);
            position += new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * randomDistance;
            return Spawn(objectToSpawn,  nodeToSpawnOn);
        }
        public static Node2D Spawn(PackedScene objectToSpawn, Node nodeToSpawnOn)
        {
            Node2D obj = (Node2D)objectToSpawn.Instantiate();
            if (obj is Node2D node2d)
            {
   
                nodeToSpawnOn.AddChild(obj);
            }
            else
                Log("CANNOT SPAWN NON 2D NODE", LogType.error);
            return obj;
        }




        public static PackedScene GetScene(string path)
        {
            return (PackedScene)ResourceLoader.Load(path);
        }
        #endregion

        #region DESTROY


        public static void Destroy(Node node)
        {
            node.QueueFree();
        }
        #endregion

        #region END GAME

        public static void EndGame(Node subject)
        {
            subject.GetTree().Quit();
        }
        #endregion

        #region SPRITES
        public static void FlipAnimatedSprite(AnimatedSprite2D animator, Vector2 velocity)
        {

            if (velocity.X != 0)
            {
                // If moving right, set scale.x to positive value
                animator.FlipH = true;
            }
            if (velocity.X < 0)
            {
                // If moving left, set scale.x to negative value
                animator.FlipH = false;
            }

        }
        #endregion

        #region ANIMATION

        public static void PlayAnimation(AnimatedSprite2D animator, string animation)
        {
            if (animator.Animation != animation)
                animator.Play(animation);
        }

        public static void PlayAnimationUntilLoopsComplete(AnimatedSprite2D animator, string animation, float loops, ref string animationActionCompleted)
        {
            if (loops > 0)
            {
                PlayAnimation(animator, animation);
            }
            else
                animationActionCompleted = animation;

        }




        public static bool WalkAnimation(Vector2 currentSpeed, AnimatedSprite2D animator)
        {
            if (currentSpeed.X != 0 || currentSpeed.Y != 0)
            {

                PlayAnimation(animator, "walk");
                return true;
            }
            else
            {
                return false;
            }



        }
        #endregion
        #region XY

        public static float DistanceBetweenObjects(Node2D subject, Node2D target)
        {
            return subject.Position.DistanceTo(target.Position);
        }

        public static Vector2 MousePosition(Node subject)
        {
            return subject.GetViewport().GetMousePosition();
        }
        public static bool IsPointInNavigationRegion2D(Vector2 position , NavigationRegion2D navRegion)
        {
            var outlines = navRegion.NavigationPolygon.GetOutlineCount();

            Log($"{navRegion} {outlines}", LogType.ui);

            var isInPolygon = false;
            for (var i = 0; i < outlines; i++)
            {
                if (Geometry2D.IsPointInPolygon(position, navRegion.NavigationPolygon.GetOutline(i)))
                {
                    isInPolygon = true;
                //break;

               // Log("WAS IN ", LogType.ui);

            }

            }
            //  isInPolygon = true;
            return isInPolygon;
        }

        public static Vector2 ChangePosition(Vector2 position, float xAdd, float yAdd)
        {
            position.X += xAdd;
            position.Y += yAdd;
            return position;
        }


        public static float PointDirection(CharacterBody2D subject, CharacterBody2D target)
        {
            Vector2 direction = (target.GlobalPosition - subject.GlobalPosition).Normalized();
            float angle = Mathf.Atan2(direction.Y, direction.X); // Calculate angle in radians

            return angle;
        }

        #endregion

        #region COLOR
        public static readonly Color ContestantBlue = new Color(0x0094FFff);
        public static readonly Color ContestantRed = new Color(0xFF5B4Cff);
        public static readonly Color ContestantGreen = new Color(0xAEFF4Cff);
        public static readonly Color ContestantYellow = new Color(0xFFCC4Cff);
        public static readonly Color ContestantPurple = new Color(0xCC4CFFff);
        public static void ChangeColor(Sprite2D sprite, Color newColor)
        {
            if (sprite != null)
            {
                sprite.Modulate = newColor; // Change the Modulate property to set the color
            }
        }
        #endregion

        #region GET PARENT NODE
        
        // public static Node GetTreeNode(Node subject, string nodeName)
       // {
           // return subject.GetTree().GetFirstNodeInGroup().GetNode<Node>(nodeName);
        //}
        //USE this keyword in subject
        public static Node GetParentNode(Node subject, string nodeName)
        {
            return subject.GetParent().GetNode<Node>(nodeName);
        }
        public static Node GetParentParentNode(Node subject, string nodeName)
        {
            return subject.GetParent().GetParent().GetNode<Node>(nodeName);
        }
        #endregion
        #region GET NODES
        public static Sprite2D GetSprite(Node node, string name)
        {
            return node.GetNode<Sprite2D>(name);

        }

        
        public static Button GetButton(Node node, string name)
        {
            return node.GetNode<Button>(name);

        }
        public static Label GetLabel(Node node, string name)
        {
            return node.GetNode<Label>(name);

        }
        public static NavigationRegion2D GetNavRegion2D(Node node, string name)
        {
            return node.GetNode<NavigationRegion2D>(name);

        }





        // Set the icon size (adjust as needed)
        #endregion

        #region GET RESOURCES
        public static Texture2D GetTexture2D(string path)
        {
            return (Texture2D)ResourceLoader.Load(path);

        }


        public static Resource GetResource(string path)
        {
            return ResourceLoader.Load(path);
        }

        public static SpriteFrames GetFrames( string path)
        {
            return (SpriteFrames)ResourceLoader.Load(path);

        }
        #endregion

        #region MODIFY NODES

        public static void SetIcon(Button button, Texture2D iconTexture)
        {
            if (iconTexture != null)
            {
                button.Icon = iconTexture;

            }
        }

        public static void SetScale(Control control, float xScale, float yScale)
        {
            control.Scale = new Vector2(xScale, yScale);

        }


        public static void ChangeSprite(Node parentNode, string nodeName, string newTexturePath)
        {
            var sprite = GetSprite(parentNode, nodeName);
            sprite.Texture = GetTexture2D(newTexturePath);
        }
        public static void ChangeAnimationFrames(AnimatedSprite2D animator,string newFramesPath, string animation)
        {

            animator.SpriteFrames = GetFrames(newFramesPath);
            animator.Animation = animation;
        }
        
        #endregion

        #region LOAD AND SAVE

        public static void SaveResource(string path, string fileName, Resource resource)
        {
            ResourceSaver.Save(resource, path + fileName);

        }


        public static Resource LoadResource(string path, string fileName)
        {
            Resource ret = null;
            if (DirAccess.DirExistsAbsolute(path))
            {
                Log($"Loading path: {path}", LogType.file);
                if (Godot.FileAccess.FileExists(path+ fileName))
                {
                    Log($"Loading file: {fileName}", LogType.file);
                    Log($"{GD.Load<Resource>(path + fileName)}", LogType.file);
                    ret = ResourceLoader.Load(path + fileName).Duplicate(true);
                    Log($"Success on resource load of: {ret}", LogType.file);
                }
                
                else
                    Log($"Loading file: {fileName} doesnt exist", LogType.error);
            }
            else
                Log($"Loading path: {path} doesnt exist", LogType.error);

            return ret;

        }
        #endregion
    }

}

