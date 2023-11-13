using System;

namespace RogueSurvivor.Data
{
    [Serializable]
    class ActorSheet
    {
        SkillTable m_SkillTable = new SkillTable(null);

        [NonSerialized]
        public static readonly ActorSheet BLANK = new ActorSheet(null, 0, 0, 0, 0, 0, Attack.BLANK, Defence.BLANK, 0, 0, 0, 0);

        public Actor Actor { get; private set; }
        public int BaseHitPoints { get; private set; }
        public int BaseStaminaPoints { get; private set; }
        public int BaseFoodPoints { get; private set; }
        public int BaseSleepPoints { get; private set; }
        public int BaseSanity { get; private set; }
        public Attack UnarmedAttack { get; private set; }
        public Defence BaseDefence { get; private set; }
        public int BaseViewRange { get; private set; }
        public int BaseAudioRange { get; private set; }
        public float BaseSmellRating { get; private set; }
        public int BaseInventoryCapacity { get; private set; }

        public SkillTable SkillTable
        {
            get { return m_SkillTable; }
            set { m_SkillTable = value; }
        }

        public ActorSheet(Actor actor, int baseHitPoints, int baseStaminaPoints,
            int baseFoodPoints, int baseSleepPoints, int baseSanity,
            Attack unarmedAttack, Defence baseDefence,
            int baseViewRange, int baseAudioRange, int smellRating,
            int inventoryCapacity)
        {
            this.Actor = actor;
            this.BaseHitPoints = baseHitPoints;
            this.BaseStaminaPoints = baseStaminaPoints;
            this.BaseFoodPoints = baseFoodPoints;
            this.BaseSleepPoints = baseSleepPoints;
            this.BaseSanity = baseSanity;
            this.UnarmedAttack = unarmedAttack;
            this.BaseDefence = baseDefence;
            this.BaseViewRange = baseViewRange;
            this.BaseAudioRange = baseAudioRange;
            this.BaseSmellRating = smellRating / 100.0f;
            this.BaseInventoryCapacity = inventoryCapacity;
        }

        public ActorSheet(Actor actor, ActorSheet copyFrom)
        {
            if (copyFrom == null)
                throw new ArgumentNullException("copyFrom");

            this.Actor = actor;
            this.BaseHitPoints = copyFrom.BaseHitPoints;
            this.BaseStaminaPoints = copyFrom.BaseStaminaPoints;
            this.BaseFoodPoints = copyFrom.BaseFoodPoints;
            this.BaseSleepPoints = copyFrom.BaseSleepPoints;
            this.BaseSanity = copyFrom.BaseSanity;
            this.UnarmedAttack = copyFrom.UnarmedAttack;
            this.BaseDefence = copyFrom.BaseDefence;
            this.BaseViewRange = copyFrom.BaseViewRange;
            this.BaseAudioRange = copyFrom.BaseAudioRange;
            this.BaseSmellRating = copyFrom.BaseSmellRating;
            this.BaseInventoryCapacity = copyFrom.BaseInventoryCapacity;

            if (copyFrom.SkillTable.Skills != null)
                m_SkillTable = new SkillTable(this.Actor, copyFrom.SkillTable.Skills);
        }
    }
}
