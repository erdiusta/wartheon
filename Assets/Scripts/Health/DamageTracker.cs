using System.Collections.Generic;
using UnityEngine;

public class DamageTracker : MonoBehaviour
{
    private class DamageEvent
    {
        public float timeStamp;
        public int damage;

        public DamageEvent(float time, int dmg)
        {
            timeStamp = time;
            damage = dmg;
        }
    }

    List<DamageEvent> damageEvents = new List<DamageEvent>();
    float timeWindow = 4f;

    // Call this whenever damage is dealt
    public void RecordDamage(int damageAmount)
    {
        float now = Time.time;
        damageEvents.Add(new DamageEvent(now, damageAmount));
        CleanUpOldEntries(now);
    }

    // Call this when you want to get damage within the last 4 seconds
    public int GetRecentDamage()
    {
        float now = Time.time;
        CleanUpOldEntries(now);

        int totalDamage = 0;
        foreach (var evt in damageEvents)
        {
            totalDamage += evt.damage;
        }

        return totalDamage;
    }

    private void CleanUpOldEntries(float now)
    {
        damageEvents.RemoveAll(evt => evt.timeStamp > timeWindow);
    }
}
