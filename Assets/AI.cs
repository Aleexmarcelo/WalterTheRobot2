﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Panda;

namespace Panda.Examples.Shooter
{
    public class AI : MonoBehaviour
    {
        public Transform player;
        public Transform bulletSpawn;
        public Slider healthBar;
        public GameObject bulletPrefab;

        NavMeshAgent agent;
        public Vector3 destination; // The movement destination.
        public Vector3 target; // The position to aim to.
        float health = 100.0f;
        float rotSpeed = 5.0f;
        float visibleRange = 80.0f;
        float shotRange = 40.0f;

        Unit enemy;
        Unit self;
        AIVision vision;

        float random_destination_radius = 1.0f;

        Vector3 enemyLastSeenPosition;

        [Task]
        public bool SetTarget_Enemy()
        {
            if (enemy != null)
            {
                self.SetTarget(enemy.transform.position);
                return true;
            }
            return false;
        }

        [Task]
        bool SetTarget_EnemyLastSeenPosition()
        {
            if (enemy != null)
            {
                self.SetTarget(enemyLastSeenPosition);
                return true;
            }
            return false;
        }

        [Task]
        public bool SetTarget_Angle(float angle)
        {
            var p = this.transform.position +  Quaternion.AngleAxis( angle, Vector3.up)*this.transform.forward;
            self.SetTarget(p);
            return true;
        }

        float lastEnemyAcquisitionTime = float.NegativeInfinity;
        [Task]
        public void Acquire_Enemy()
        {
            if (Time.time - lastEnemyAcquisitionTime > 0.5f)
            {
                enemy = null;

                if (enemy == null && self.shotBy != null && self.shotBy.team != self.team && (Time.time - self.lastShotTime) < 1.0f)
                    enemy = self.shotBy;

                if (enemy == null && vision.visibles != null)
                {
                    foreach (var v in vision.visibles)
                    {
                        if (v == null)
                            continue;

                        var shooter = v.GetComponent<Unit>();

                        if (shooter == null)
                        {
                            var bullet = v.GetComponent<Bullet>();
                            shooter = bullet != null && bullet.shooter != null ? bullet.shooter.GetComponent<Unit>() : null;

                            if (shooter != null && self.team == shooter.team)
                                shooter = null;
                        }

                        if (shooter != null && shooter.team != self.team)
                        {
                            enemy = shooter;
                            break;
                        }
                    }
                }
                lastEnemyAcquisitionTime = Time.time;
            }

            ThisTask.Complete(enemy != null);

        }

        [Task]
        bool HasAmmo_Ememy()
        {
            bool has = false;
            if (enemy != null)
                has = enemy.ammo > 0;
            return has;
        }

        [Task]
        bool Clear_Enemy()
        {
            enemy = self.shotBy = null;
            return true;
        }

        float lastSeenTime = float.NegativeInfinity;
        [Task]
        bool IsVisible_Enemy()
        {
            if (enemy != null && enemy.gameObject != null)
            {
                foreach (var v in vision.visibles)
                {
                    if (v == enemy.gameObject)
                    {
                        lastSeenTime = Time.time;
                        enemyLastSeenPosition = enemy.transform.position;
                        break;
                    }
                }
            }

            return (Time.time - lastSeenTime) < 0.5f;
        }

        [Task]
        bool SetDestination_Enemy()
        {
            bool succeeded = false;

            if( enemy != null )
            {
                self.SetDestination(enemy.transform.position);
                succeeded = true;
            }
            return succeeded;
        }

        [Task]
        bool SetDestination_Random(float radius)
        {
            random_destination_radius = radius;
            return SetDestination_Random();
        }

        [Task]
        bool SetDestination_Random()
        {
            var dst = this.transform.position + (Random.insideUnitSphere * random_destination_radius);
            self.SetDestination(dst);
            return true;
        }

        [Task]
        bool HasEnemy()
        {
            return enemy != null;
        }

        [Task]
        bool IsThereLineOfSight_Attacker_Destination()
        {
            bool hasLoS = false;
            var attacker = self.shotBy != null ? self.shotBy : enemy;
            if (attacker != null  )
            {
                var ignoreList = new List<GameObject>() { this.gameObject, attacker.gameObject };
                var src = attacker.transform.position;
                var dst =  self.destination ;
                hasLoS = HasLoS(src, dst, ignoreList);
            }
            return hasLoS;
        }

        [Task]
        bool LastBulletSeenTime_LessThan( float duration )
        {
            float t = Time.time - vision.lastBulletSeenTime;
            if( Task.isInspected )
                ThisTask.debugInfo = string.Format("t={0:0.00}", t);
            return t < duration;
        }

        bool HasLoS( Vector3 source, Vector3 destination, List<GameObject>  ignoreList )
        {
            bool hasLos = true;
            var delta = (destination - source);
            var ray = new Ray(source, delta.normalized);
            var hits = Physics.RaycastAll(ray, delta.magnitude );
            foreach( var hit in hits)
            {
                var type = hit.collider.GetComponent<TriggerType>();
                if (type == null || !type.collidesWithBullet)
                    continue;
                    
                var go = hit.collider.attachedRigidbody != null ? hit.collider.attachedRigidbody.gameObject: hit.collider.gameObject;
                if(! ignoreList.Contains( go ) && Vector3.Distance( hit.point, destination ) > 2.0f)
                {
                    hasLos = false;
                    break;
                }
            }
            return hasLos;
        }

        [Task]
        bool SetDestination_Cover()
        {
            // Search for a cover where the enemy has no line of sight.

            var possibleCovers = new List<Vector3>();
            float searchRadius = 3.0f;

            int n = 20; // Maxiumum number of candidate cover points.
            int s = 10; // Number of sample per circle

            bool isSet = false;

            var attacker = self.shotBy != null ? self.shotBy : enemy;

            if (attacker != null)
            {

                // Sample random cover points on an increasing circle.
                var src = attacker.transform.position;
                var pos = this.transform.position;
                var ignoreList = new List<GameObject>() { this.gameObject, attacker.gameObject };
                while (possibleCovers.Count < n)
                {
                    for (int i = 0; i < s; i++)
                    {
                        float a = Random.value * Mathf.PI * 2.0f;
                        var dst = pos + new Vector3(Mathf.Cos(a), 0.0f ,Mathf.Sin(a)) * searchRadius;

                        if ( ! HasLoS(src, dst, ignoreList) )
                            possibleCovers.Add(dst);

                    }
                    searchRadius += 2.0f;
                }

                // Search the closest cover point
                UnityEngine.AI.NavMeshPath selfPath = new UnityEngine.AI.NavMeshPath();
                UnityEngine.AI.NavMeshPath attackerPath = new UnityEngine.AI.NavMeshPath();
                Vector3 closest = pos;
                float minD = float.PositiveInfinity;
                foreach ( var p in possibleCovers)
                {
                    if( self.navMeshAgent.CalculatePath(p, selfPath) && selfPath.status == UnityEngine.AI.NavMeshPathStatus.PathComplete )
                    {
                        float attackerDistance = 0.0f;
                        if (attacker != null && attacker.navMeshAgent != null && attacker.navMeshAgent.CalculatePath(p, attackerPath))
                            attackerDistance = PathLength(attackerPath);

                        float d = PathLength(selfPath) - attackerDistance*0.1f;
                        if( d < minD)
                        {
                            minD = d;
                            closest = p;
                        }
                    }
                }

                self.SetDestination(closest);
                isSet = true;
            }

            return isSet;

        }

        static float PathLength( UnityEngine.AI.NavMeshPath path )
        {
            float d = float.PositiveInfinity;

            if (path != null && path.corners.Length > 1)
            {
                d = 0.0f;
                for (int i = 0; i < path.corners.Length-1; i++)
                {
                    var p0 = path.corners[i + 0];
                    var p1 = path.corners[i + 1];
                    d += Vector3.Distance(p1, p0);
                }
            }
            return d;
        }

        [Task]
        public void PickRandomDestination()
        {
            Vector3 dest = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
            agent.SetDestination(dest);
            Task.current.Succeed();
        }
        [Task]
        public void MoveToDestination()
        {
            if (Task.isInspected)
            Task.current.debugInfo = string.Format("t={0:0.00}", Time.time);
            if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            Task.current.Succeed();
        }
        }

        [Task]
        public bool IsHealthLessThan(float health)
        {
            return this.health < health;
        }
        [Task]
        public bool Explode()
        {
            Destroy(healthBar.gameObject);
            Destroy(this.gameObject);
            return true;
        }

        [Task]
        public void Fire(){
            GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
            bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward*2000);
        }

        [Task]
         bool SeePlayer(){
            if (player != null && player.gameObject != null)
            {
                foreach (var v in vision.visibles)
                {
                    if (v == player.gameObject)
                    {
                        lastSeenTime = Time.time;
                        enemyLastSeenPosition = player.transform.position;
                        break;
                    }
                }
            }

            return (Time.time - lastSeenTime) < 0.5f;
        }

        // Use this for initialization
        void Start()
        {
            self = this.GetComponent<Unit>();
            vision = this.GetComponentInChildren<AIVision>();
            agent = this.GetComponent<NavMeshAgent>();
            agent.stoppingDistance = shotRange - 5; //for a little buffer
            InvokeRepeating("UpdateHealth",5,0.5f);
        }

        void Update() {
            Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);
            healthBar.value = (int)health;
            healthBar.transform.position = healthBarPos + new Vector3(0,60,0);
        }

        void UpdateHealth()
        {
            if(health < 100)
            health ++;
        }

        void OnCollisionEnter(Collision col)
        {
        if(col.gameObject.tag == "bullet")
        {
            health -= 10;
        }
        }

    }
}
