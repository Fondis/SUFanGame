﻿using UnityEngine;
using System.Collections.Generic;
using StevenUniverse.FanGame.Data;
using StevenUniverse.FanGame.Characters.Customization;
using StevenUniverse.FanGame.Util;
using StevenUniverse.FanGame.Factions;
using StevenUniverse.FanGame.Items;

namespace StevenUniverse.FanGame.Characters
{
    /// <summary>
    /// Base character data, intended to entirely encapsulate all information needed to load characters in different contexts.
    /// </summary>
    [System.Serializable]
    public class CharacterData : JsonBase<CharacterData>
    {
        
        [SerializeField]
        private string characterName; //Name
        [SerializeField]
        private Faction faction; //What team?
        [SerializeField]
        private List<Item> heldItems; //WILDCATS
        [SerializeField]
        private Skill[] skills; //All available skills
        [SerializeField]
        private UnitStats stats; //All the unit battle modifiers
        [SerializeField]
        private SupportInfo[] supportInfos;
        
        [SerializeField]
        private SaveData savedData;

        public CharacterData(  
            string characterName,
            string affiliation,
            SaveData saveData
            )
        {
            this.characterName = characterName;
            this.faction = (Faction)System.Enum.Parse(typeof(Faction), affiliation, true );
            this.savedData = saveData;

            // All other data parameters may want to be loaded from SaveData at instantiation.
        }


        //Name
        public string EntityName
        {
            get { return characterName; }
            set { characterName = value; }
        }

        //Team name
        public Faction Faction_
        {
            get { return faction; }
            set { faction = value; }
        }

        //Held items
        public List<Item> HeldItems
        {
            get { return heldItems; }
            set { heldItems = value; }
        }
        
        //Skills available
        public Skill[] Skills
        {
            get { return skills; }
            set { skills = value; }
        }
        
        //Unit stats
        public UnitStats Stats
        {
            get { return stats; }
            set { stats = value; }
        }
    
        public SaveData SavedData
        {
            get { return savedData; }
            set { savedData = value; }
        }
        
        public SupportInfo[] SupportInfos
        {
            get { return supportInfos; }
            set { supportInfos = value; }
        }
        
    } 
}