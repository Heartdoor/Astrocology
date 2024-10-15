using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Asriela.BasicFunctions;
using static System.Net.Mime.MediaTypeNames;
public partial class WorldObject : Entity
{
    #region PROPERTIES


    #endregion
    #region BASIC FUNCTIONS CONNECTIONS
    Alarms alarm = new Alarms();
    #endregion
    #region START
    void Start()
    {
     
        base._Ready();

    }
    #endregion
    #region RUN
    public void Run()
    {

        base.RunEntity();

        GlobalPosition = startPosition;

        alarm.Run();


        if (HasTag(Tags, Tag.isBuilding))
        {
            var keys = contains.Keys.ToArray();
            foreach (var key in keys)
            {
                Main.main.Resources[key] += contains[key];
            }
        }
        if (DistanceToNearestObject(this,GetTree(), "Character") < 250)
        {
            if (HasTag(Tags, Tag.unguarded))
                Tags.Remove(Tag.unguarded);
        }
        else
               if (!HasTag(Tags, Tag.unguarded))
            Tags.Add(Tag.unguarded);

        
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
