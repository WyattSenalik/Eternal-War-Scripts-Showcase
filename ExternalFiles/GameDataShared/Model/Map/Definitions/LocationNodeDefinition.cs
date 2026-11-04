// Original Authors - Jack Dekko
using EternalWar.Core.Helpers;
using System.Text.Json.Serialization;

namespace EternalWar.Core.Model.Map.Definitions
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "locationNodeType")]
    [JsonDerivedType(typeof(LocationNodeDefinition_Encounter_Normal), "NormalEncounter")]
    [JsonDerivedType(typeof(LocationNodeDefinition_Encounter_Elite), "EliteEncounter")]
    [JsonDerivedType(typeof(LocationNodeDefinition_Encounter_MidBoss), "MidBossEncounter")]
    [JsonDerivedType(typeof(LocationNodeDefinition_Encounter_BigBoss), "BigBossEncounter")]
    [JsonDerivedType(typeof(LocationNodeDefinition_Event), "Event")]
    [JsonDerivedType(typeof(LocationNodeDefinition_Chest), "Chest")]
    [JsonDerivedType(typeof(LocationNodeDefinition_Rest), "Rest")]

    public abstract record LocationNodeDefinition(string Id) : IHaveStringId;

    public abstract record LocationNodeDefinition_Encounter(string Id, ValueArray<string> EncounterIds) : LocationNodeDefinition(Id);

    public sealed record LocationNodeDefinition_Encounter_Normal(string Id, ValueArray<string> EncounterIds) : LocationNodeDefinition_Encounter(Id, EncounterIds);

    public sealed record LocationNodeDefinition_Encounter_Elite(string Id, ValueArray<string> EncounterIds) : LocationNodeDefinition_Encounter(Id, EncounterIds);

    public sealed record LocationNodeDefinition_Encounter_MidBoss(string Id, ValueArray<string> EncounterIds) : LocationNodeDefinition_Encounter(Id, EncounterIds);  
    
    public sealed record LocationNodeDefinition_Encounter_BigBoss(string Id, ValueArray<string> EncounterIds) : LocationNodeDefinition_Encounter(Id, EncounterIds);  

    public sealed record LocationNodeDefinition_Event(string Id, ValueArray<string> EventIds) : LocationNodeDefinition(Id);

    public sealed record LocationNodeDefinition_Chest(string Id, ValueArray<string> ChestIds) : LocationNodeDefinition(Id);

    public sealed record LocationNodeDefinition_Rest(string Id, int HealAmount) : LocationNodeDefinition(Id);
}
