using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectSystem : MonoBehaviour
{
    // Sounds
    private Dictionary<StatusEffectType, string> sfxMap = new Dictionary<StatusEffectType, string>()
    {
        { StatusEffectType.BLOCK,      "metal" },
        { StatusEffectType.HEAL,       "energy" },
        { StatusEffectType.STRENGTH,   "metal" },
        { StatusEffectType.DEFENCE,    "metal" },
        { StatusEffectType.WEAKEN,     "metal" },
        { StatusEffectType.FRAIL,      "metal" },
        { StatusEffectType.BARRIER,    "metal" },
        { StatusEffectType.THORNS,     "metal" },
        { StatusEffectType.BURN,       "burn" },
        { StatusEffectType.POISON,     "energy" },
        { StatusEffectType.CONFUSE,    "energy" },
        { StatusEffectType.EXHAUST,    "energy" },
        { StatusEffectType.DISCARD,    "energy" },
        { StatusEffectType.ENERGYGAIN, "energy" },
        { StatusEffectType.ENERGYLOSS, "energy" }
    };

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AddStatusEffectGA>(AddStatusEffectPerformer);
    }

    private void OnDisable() {
        ActionSystem.DetachPerformer<AddStatusEffectGA>();
    }
    private IEnumerator AddStatusEffectPerformer(AddStatusEffectGA addStatusEffectGA)
    {
        foreach (var target in addStatusEffectGA.Targets)
        {

            if (sfxMap.TryGetValue(addStatusEffectGA.StatusEffectType, out string sfxName))
            {
                AudioManager.Instance.PlayRandomByPrefix(sfxName);
            }

            target.AddStatusEffect(addStatusEffectGA.StatusEffectType, addStatusEffectGA.StackCount);
            yield return null; //add status effect animation if want
        }
    }
}
