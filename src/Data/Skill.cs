using System;
using System.Collections.Generic;

namespace RogueSurvivor.Data
{
    [Serializable]
    class Skill
    {
        int m_ID;
        int m_Level;

        public int ID
        {
            get { return m_ID; }
        }

        public int Level
        {
            get { return m_Level; }
            set { m_Level = value; }
        }

        public Skill(int id)
        {
            m_ID = id;
        }
    }

    [Serializable]
    class SkillTable
    {
        Actor m_Actor;
        public Actor Actor {
            get { return m_Actor; }
            set { m_Actor = value; }
        }

        Dictionary<int, Skill> m_Table;   // allocated only if needed (some actors have 0 skills)

        /// <summary>
        /// Get all skills null if no skills.
        /// </summary>
        public IEnumerable<Skill> Skills
        {
            get
            {
                if (m_Table == null)
                    return null;

                return m_Table.Values;
            }
        }

        /// <summary>
        /// List all non-zero skills ids as an array; null if no skills.
        /// </summary>
        public int[] SkillsList
        {
            get
            {
                if (m_Table == null)
                    return null;

                int[] array = new int[CountSkills];
                int i = 0;
                foreach (Skill s in m_Table.Values)
                {
                    array[i++] = s.ID;
                }

                return array;
            }
        }

        /// <summary>
        /// Count non-zero skills.
        /// </summary>
        public int CountSkills
        {
            get
            {
                if (m_Table == null)
                    return 0;

                return m_Table.Values.Count;
            }
        }

        public int CountTotalSkillLevels
        {
            get
            {
                int sum = 0;
                foreach (Skill s in m_Table.Values)
                    sum += s.Level;
                return sum;
            }
        }

        public SkillTable(Actor actor)
        {
            this.Actor = actor;
        }

        public SkillTable(Actor actor, IEnumerable<Skill> startingSkills)
        {
            this.Actor = actor;
            if (startingSkills == null)
                throw new ArgumentNullException("startingSkills");

            foreach (Skill sk in startingSkills)
                AddSkill(sk);
        }

        public Skill GetSkill(int id)
        {
            if (m_Table == null)
                return null;

            Skill sk;
            if (m_Table.TryGetValue(id, out sk))
                return sk;
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>0 for a missing skill</returns>
        public int GetSkillLevel(int id)
        {
            Skill sk = GetSkill(id);
            if (sk == null)
                return 0;

            // get a bonus added to your necrology/carpentry if your leader has the skill
            int bonus = 0;
            if (Actor != null && Actor.HasLeader)
            {
                if (id == (int) Gameplay.Skills.IDs.NECROLOGY)
                    bonus = (int) Math.Ceiling(Actor.Leader.Sheet.SkillTable.GetSkillLevel(id) / 3f);
                if (id == (int) Gameplay.Skills.IDs.MEDIC)
                    bonus = (int) Math.Ceiling(Actor.Leader.Sheet.SkillTable.GetSkillLevel(id) / 3f);
                if (id == (int) Gameplay.Skills.IDs.CARPENTRY)
                    bonus = (int) Math.Ceiling(Actor.Leader.Sheet.SkillTable.GetSkillLevel(id)/2f);
            }
            return (int) Math.Min(sk.Level + bonus, Gameplay.Skills.MaxSkillLevel(id));
        }

        public void AddSkill(Skill sk)
        {
            if (m_Table == null)
                m_Table = new Dictionary<int, Skill>(3);

            if (m_Table.ContainsKey(sk.ID))
                throw new ArgumentException("skill of same ID already in table");
            if (m_Table.ContainsValue(sk))
                throw new ArgumentException("skill already in table");

            m_Table.Add(sk.ID, sk);
        }

        public void AddOrIncreaseSkill(int id)
        {
            if (m_Table == null)
                m_Table = new Dictionary<int, Skill>(3);

            Skill sk = GetSkill(id);
            if (sk == null)
            {
                sk = new Skill(id);
                m_Table.Add(id, sk);
            }

            ++sk.Level;
        }

        public void DecOrRemoveSkill(int id)
        {
            if (m_Table == null) return;

            Skill sk = GetSkill(id);
            if (sk == null) return;
            if (--sk.Level <= 0)
            {
                m_Table.Remove(id);
                if (m_Table.Count == 0)
                    m_Table = null;
            }
        }
    }
}
