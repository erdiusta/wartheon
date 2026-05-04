using UnityEngine;
using Random = UnityEngine.Random;
using System;

namespace Pathfinding {
	/// <summary>
	/// Simple patrol behavior.
	/// This will set the destination on the agent so that it moves through the sequence of objects in the <see cref="targets"/> array.
	/// Upon reaching a target it will wait for <see cref="delay"/> seconds.
	/// </summary>
	[UniqueComponent(tag = "ai.destination")]
	public class PatrolRigidbody2D : VersionedMonoBehaviour {
		/// <summary>Target points to move to in order</summary>
		public Transform[] targets;

		/// <summary>Time in seconds to wait at each target</summary>
		public float delay = 0;

        /// For indexing spawn positions
        public static event Func<Vector2Int[]> OnRequestSpawnPositions;

		/// <summary>Current target index</summary>
		int index;

		IAstarAI agent;
		float switchTime = float.PositiveInfinity;

        // Change spawn checkpoint in case of collision with enemy
        float collisionCooldown = 1f;
        float cooldownTimer = 1;
        bool isCollided = false;

		protected override void Awake () 
        {
			base.Awake();
			agent = GetComponent<IAstarAI>();
		}

        private void FixedUpdate()
        {
            if (agent == null) return;

            cooldownTimer += Time.fixedDeltaTime;

            if (targets.Length == 0) return;

            bool search = false;

            // Note: using reachedEndOfPath and pathPending instead of reachedDestination here because
            // if the destination cannot be reached by the agent, we don't want it to get stuck, we just want it to get as close as possible and then move on.
            if (agent.reachedEndOfPath && !agent.pathPending && float.IsPositiveInfinity(switchTime))
            {
                switchTime = Time.time + delay;
            }

            if (Time.time >= switchTime)
            {
				index++;

				if (OnRequestSpawnPositions != null)
				{
					Vector2Int[] spawnPositions;

                    try
                    {
                        spawnPositions = OnRequestSpawnPositions.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Spawn position provider crashed: {e}");
                        spawnPositions = null;
                    }

                    if (spawnPositions != null && spawnPositions.Length > 0)
                    {
                        index = Random.Range(0, spawnPositions.Length);
                    }
                }

                search = true;
                switchTime = float.PositiveInfinity;
            }

            index = index % targets.Length;
            agent.destination = targets[index].position;

            if (search || isCollided) agent.SearchPath();
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            Enemy enemy = GetComponent<Enemy>();

            if (enemy == null || enemy.enemyAI == null || !enemy.enemyAI.enabled) return;

            if (enemy != null && enemy.enemyAI.enemyPhase != EnemyPhase.Patrol) return;

            if ((collision.collider.CompareTag(Settings.enemyTag) || collision.collider.CompareTag(Settings.collisionTilemap)) && cooldownTimer >= collisionCooldown)
            {
                int originalIndex = index;

                while (index == originalIndex)
                {
                    index = Random.Range(0, targets.Length);
                }

                isCollided = true;
                cooldownTimer = 0;
            }
            else
            {
                isCollided = false;
            }
        }
    }
}
