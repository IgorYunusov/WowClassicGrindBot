﻿using Core.Goals;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedLib;
using Game;

namespace Core.GOAP
{
    public sealed class GoapAgent
    {
        private ILogger logger;
        private ConfigurableInput input;
        private StopMoving stopMoving;

        private GoapPlanner planner;
        public IEnumerable<GoapGoal> AvailableGoals { get; set; }
        
        public PlayerReader PlayerReader { get; private set; }
        private ClassConfiguration classConfiguration;

        public GoapGoal? CurrentGoal { get; set; }
        public HashSet<KeyValuePair<GoapKey, object>> WorldState { get; private set; } = new HashSet<KeyValuePair<GoapKey, object>>();
        private IBlacklist blacklist;

        public GoapAgent(ILogger logger, ConfigurableInput input, PlayerReader playerReader, HashSet<GoapGoal> availableGoals, IBlacklist blacklist, ClassConfiguration classConfiguration)
        {
            this.logger = logger;
            this.input = input;

            this.PlayerReader = playerReader;

            this.stopMoving = new StopMoving(input, playerReader);

            this.AvailableGoals = availableGoals.OrderBy(a => a.CostOfPerformingAction);
            this.blacklist = blacklist;

            this.planner = new GoapPlanner(logger);
            this.classConfiguration = classConfiguration;
        }

        public void UpdateWorldState()
        {
            WorldState = GetWorldState(PlayerReader);
        }

        public async Task<GoapGoal?> GetAction()
        {
            if (PlayerReader.HealthPercent > 1 && blacklist.IsTargetBlacklisted())
            {
                logger.LogWarning($"{GetType().Name}: Target is blacklisted - StopAttack & ClearTarget");
                await input.TapStopAttack("");
                await input.TapClearTarget("");
                UpdateWorldState();
            }

            var goal = new HashSet<KeyValuePair<GoapKey, GoapPreCondition>>();

            //Plan
            Queue<GoapGoal> plan = planner.Plan(AvailableGoals, WorldState, goal);
            if (plan != null && plan.Count > 0)
            {
                if (CurrentGoal == plan.Peek() && !CurrentGoal.Repeatable)
                {
                    CurrentGoal = null;
                }
                else
                {
                    CurrentGoal = plan.Peek();
                }
            }
            else
            {
                if (CurrentGoal != null && !CurrentGoal.Repeatable)
                {
                    logger.LogInformation($"Plan= {CurrentGoal.GetType().Name} is not Repeatable!");
                    CurrentGoal = null;

                    await stopMoving.Stop();
                }
            }

            return CurrentGoal;
        }

        private HashSet<KeyValuePair<GoapKey, object>> GetWorldState(PlayerReader playerReader)
        {
            var state = new HashSet<KeyValuePair<GoapKey, object>>
            {
                new KeyValuePair<GoapKey, object>(GoapKey.hastarget,!blacklist.IsTargetBlacklisted() && (!string.IsNullOrEmpty(playerReader.Target)|| playerReader.TargetHealth>0)),
                new KeyValuePair<GoapKey, object>(GoapKey.targetisalive,!string.IsNullOrEmpty(this.PlayerReader.Target) &&  (!playerReader.PlayerBitValues.TargetIsDead || playerReader.TargetHealth>0)),
                new KeyValuePair<GoapKey, object>(GoapKey.incombat, playerReader.PlayerBitValues.PlayerInCombat ),
                new KeyValuePair<GoapKey, object>(GoapKey.withinpullrange, playerReader.WithInPullRange),
                new KeyValuePair<GoapKey, object>(GoapKey.incombatrange, playerReader.WithInCombatRange),
                new KeyValuePair<GoapKey, object>(GoapKey.pulled, false),
                new KeyValuePair<GoapKey, object>(GoapKey.isdead, playerReader.HealthPercent==0),
                new KeyValuePair<GoapKey, object>(GoapKey.isswimming, playerReader.PlayerBitValues.IsSwimming),
                new KeyValuePair<GoapKey, object>(GoapKey.itemsbroken,playerReader.PlayerBitValues.ItemsAreBroken),
                new KeyValuePair<GoapKey, object>(GoapKey.producedcorpse, playerReader.LastCombatKillCount>0),
                new KeyValuePair<GoapKey, object>(GoapKey.consumecorpse, playerReader.ShouldConsumeCorpse),
                new KeyValuePair<GoapKey, object>(GoapKey.shouldloot, playerReader.NeedLoot),
                new KeyValuePair<GoapKey, object>(GoapKey.shouldskin, playerReader.NeedSkin)
        };

            actionState.ToList().ForEach(kv => state.Add(kv));

            return state;
        }

        private Dictionary<GoapKey, object> actionState = new Dictionary<GoapKey, object>();

        public void OnActionEvent(object sender, ActionEventArgs e)
        {
            if (!actionState.ContainsKey(e.Key))
            {
                actionState.Add(e.Key, e.Value);
            }
            else
            {
                actionState[e.Key] = e.Value;
            }
        }
    }
}