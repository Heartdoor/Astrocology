using Godot;
using System;
using System.Security.Claims;
using static Asriela.BasicFunctions;
using static Godot.WebSocketPeer;
public partial class Spawn : Sprite2D
{
    [Export] public bool active=true;
    [Export] PackedScene sceneToSpawn;
    [Export] public Species species;
    [Export] public Godot.Collections.Array<Tag> startingTags = new Godot.Collections.Array<Tag>();
    public static int count=0;
    public Alarms alarm = new Alarms();
    #region START
    void Start()
    {
        Texture = null;
        if (!active) return;
        count++;
        var spawnedEntity=(Entity)Spawn(sceneToSpawn, this);

        foreach (Tag tag in startingTags)
        {
            spawnedEntity.startingTags.Add(tag);

            Log($"{tag}", LogType.game);

        }
        spawnedEntity.idNumber = count;
        spawnedEntity.species = species;
        spawnedEntity.AddSpeciesTag();
        spawnedEntity.PassOnStartingTags(); 
        spawnedEntity.SetupSpecies();
        spawnedEntity.startPosition = GlobalPosition;
        spawnedEntity.ChangeName();


    }


    #endregion
    #region OLD

    public override void _Ready()
    {
   
        Start();
    }



    #endregion
}
