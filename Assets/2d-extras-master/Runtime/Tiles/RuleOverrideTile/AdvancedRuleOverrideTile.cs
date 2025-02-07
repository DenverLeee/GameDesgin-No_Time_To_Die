﻿using System;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Tilemaps
{
    /// <summary>
    ///     Rule Override Tiles are Tiles which can override a subset of Rules for a given Rule Tile to provide specialised
    ///     behaviour while keeping most of the Rules originally set in the Rule Tile.
    /// </summary>
    [MovedFrom(true, "UnityEngine")]
    [Serializable]
    [CreateAssetMenu(fileName = "New Advanced Rule Override Tile", menuName = "Tiles/Advanced Rule Override Tile")]
    public class AdvancedRuleOverrideTile : RuleOverrideTile
    {
        public Sprite m_DefaultSprite;
        public GameObject m_DefaultGameObject;
        public Tile.ColliderType m_DefaultColliderType = Tile.ColliderType.Sprite;

        /// <summary>
        ///     A list of TilingRule Overrides
        /// </summary>
        public List<RuleTile.TilingRuleOutput> m_OverrideTilingRules = new List<RuleTile.TilingRuleOutput>();

        [NonSerialized] public int m_MissingTilingRuleIndex = -1;

        /// <summary>
        ///     Gets the overriding TilingRuleOutput of a given TilingRule.
        /// </summary>
        /// <param name="original">The original TilingRule that is overridden</param>
        public RuleTile.TilingRuleOutput this[RuleTile.TilingRule originalRule]
        {
            get
            {
                foreach (var overrideRule in m_OverrideTilingRules)
                    if (overrideRule.m_Id == originalRule.m_Id)
                        return overrideRule;

                return null;
            }
            set
            {
                for (var i = m_OverrideTilingRules.Count - 1; i >= 0; i--)
                    if (m_OverrideTilingRules[i].m_Id == originalRule.m_Id)
                    {
                        m_OverrideTilingRules.RemoveAt(i);
                        break;
                    }

                if (value != null)
                {
                    var overrideRule = new RuleTile.TilingRuleOutput();
                    CopyTilingRule(value, overrideRule);
                    m_OverrideTilingRules.Add(overrideRule);
                }
            }
        }

        /// <summary>
        ///     Applies overrides to this
        /// </summary>
        /// <param name="overrides">A list of overrides to apply</param>
        /// <exception cref="ArgumentNullException">The input overrides list is not valid</exception>
        public void ApplyOverrides(IList<KeyValuePair<RuleTile.TilingRule, RuleTile.TilingRuleOutput>> overrides)
        {
            if (overrides == null)
                throw new ArgumentNullException("overrides");

            for (var i = 0; i < overrides.Count; i++)
                this[overrides[i].Key] = overrides[i].Value;
        }

        /// <summary>
        ///     Gets overrides for this
        /// </summary>
        /// <param name="overrides">A list of overrides to fill</param>
        /// <exception cref="ArgumentNullException">The input overrides list is not valid</exception>
        public void GetOverrides(List<KeyValuePair<RuleTile.TilingRule, RuleTile.TilingRuleOutput>> overrides)
        {
            if (overrides == null)
                throw new ArgumentNullException("overrides");

            overrides.Clear();

            if (m_Tile)
                foreach (var originalRule in m_Tile.m_TilingRules)
                {
                    var overrideRule = this[originalRule];
                    overrides.Add(
                        new KeyValuePair<RuleTile.TilingRule, RuleTile.TilingRuleOutput>(originalRule, overrideRule));
                }

            m_MissingTilingRuleIndex = overrides.Count;

            foreach (var overrideRule in m_OverrideTilingRules)
                if (!overrides.Exists(o => o.Key.m_Id == overrideRule.m_Id))
                {
                    var originalRule = new RuleTile.TilingRule {m_Id = overrideRule.m_Id};
                    overrides.Add(
                        new KeyValuePair<RuleTile.TilingRule, RuleTile.TilingRuleOutput>(originalRule, overrideRule));
                }
        }

        public override void Override()
        {
            if (!m_Tile || !m_InstanceTile)
                return;

            var tile = m_InstanceTile;

            tile.m_DefaultSprite = m_DefaultSprite;
            tile.m_DefaultGameObject = m_DefaultGameObject;
            tile.m_DefaultColliderType = m_DefaultColliderType;
            tile.m_TilingRules.Clear();

            foreach (var originalRule in m_Tile.m_TilingRules)
            {
                var overrideRule = this[originalRule];
                var instanceRule = new RuleTile.TilingRule();
                CopyTilingRule(originalRule, instanceRule);
                if (overrideRule != null)
                    CopyTilingRule(overrideRule, instanceRule);
                tile.m_TilingRules.Add(instanceRule);
            }
        }
    }
}