﻿using RogueSurvivor.Data;
using System;

namespace RogueSurvivor.Engine.Actions
{
    class ActionEatCorpse : ActorAction
    {
        readonly Corpse m_Target;

        public ActionEatCorpse(Actor actor, RogueGame game, Corpse target)
            : base(actor, game)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            m_Target = target;
        }

        public override bool IsLegal()
        {
            return m_Game.Rules.CanActorEatCorpse(m_Actor, m_Target, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoEatCorpse(m_Actor, m_Target);
        }
    }
}
