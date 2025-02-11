using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Szczesniak {
    public class BossAttackState : MonoBehaviour {

        static class States {
            public class State {

                protected BossAttackState bossAttack;

                virtual public State Update() {

                    return null;
                }

                virtual public void OnStart(BossAttackState bossAttack) {
                    this.bossAttack = bossAttack;
                }

                virtual public void OnEnd() {

                }
            }

            //////////////////////////// Child Classes: 
            ///

            public class Idle : State {
                public override State Update() {
                    if (bossAttack.CanSeeThing(bossAttack.player, bossAttack.viewingDistance)) return new States.MiniGunAttack();
                    if (bossAttack.misslesSpawned && bossAttack.CanSeeThing(bossAttack.player, bossAttack.missileDistance) && !bossAttack.CanSeeThing(bossAttack.player, bossAttack.missileDistance - 5)) {
                        return new States.HomingMissleAttack();
                    }
                    return null;
                }

            }

            public class SpawnMinions : State {

            }

            public class MiniGunAttack : State {
                public override State Update() {
                    // behavior
                    bossAttack.MachineGun();
                    bossAttack.TurnTowardsTarget();

                    // transition
                    if (!bossAttack.CanSeeThing(bossAttack.player, bossAttack.viewingDistance)) return new States.Idle();

                    return null;
                }
            }

            public class HomingMissleAttack : State {

                public override State Update() {
                    // behavior:
                    bossAttack.Missles();

                    return new States.Idle();
                }
            }
        }

        private States.State state;

        public Projectile prefabMachineGunBullets;

        public Transform leftMuzzle;
        public Transform rightMuzzle;

        public Transform missile1;
        public Transform missile2;
        public float missileDistance = 25;
        public Transform missilePos1;
        public Transform missilePos2;


        public float roundPerSec = 20;
        private float bulletAmountTime = 0;

        public float missleRespawnTime = 5;
        bool misslesSpawned = true;

        public float viewingAngle = 90;
        public float viewingDistance = 20;

        public Transform player;

        /// <summary>
        /// The start rotation
        /// </summary>
        private Quaternion startingRotation;

        [Header("Rotation Lock")]

        /// <summary>
        /// Lock X rotation
        /// </summary>
        public bool lockRotationX;

        /// <summary>
        /// Lock Y rotation
        /// </summary>
        public bool lockRotationY;

        /// <summary>
        /// Lock Z rotation
        /// </summary>
        public bool lockRotationZ;

        
        void Start() {
            // Getting components
            startingRotation = transform.localRotation;
        }

        private void Update() {

            if (state == null) SwitchState(new States.Idle());

            if (state != null) SwitchState(state.Update());

            if (bulletAmountTime > 0) bulletAmountTime -= Time.deltaTime;

            if (missleRespawnTime > 0) {
                missleRespawnTime -= Time.deltaTime;
            }

            if (missleRespawnTime <= 0) {
                Transform missle1Pos = Instantiate(missile1, missilePos1.position, missilePos1.rotation);
                missle1Pos.parent = missilePos1;
                Transform missle2Pos = Instantiate(missile2, missilePos2.position, missilePos2.rotation);
                missle2Pos.parent = missilePos2;
                missleRespawnTime = 5;
            }
        }

        void SwitchState(States.State newState) {
            if (newState == null) return; // don't switch to nothing...

            if (state != null) state.OnEnd(); // tell previous state it is done
            state = newState; // swap states
            state.OnStart(this);
        }

        private void TurnTowardsTarget() {

            if (player) {
                Vector3 disToTarget = player.position - transform.position; // Gets distance

                Quaternion targetRotation = Quaternion.LookRotation(disToTarget, Vector3.up); // Gets target rotation

                Vector3 euler1 = transform.localEulerAngles; // get local angles BEFORE rotation
                Quaternion prevRot = transform.rotation; // 
                transform.rotation = targetRotation; // Set Rotation
                Vector3 euler2 = transform.localEulerAngles; // get local angles AFTER rotation

                if (lockRotationX) euler2.x = euler1.x; //revert x to previous value;
                if (lockRotationY) euler2.y = euler1.y; //revert y to previous value;
                if (lockRotationZ) euler2.z = euler1.z; //revert z to previous value;

                transform.rotation = prevRot; // This objects rotation turns into the prevRot

                transform.localRotation = AnimMath.Slide(transform.localRotation, Quaternion.Euler(euler2), .5f); // slides to rotation
            } else {
                // figure out bone rotation, no target:

                transform.localRotation = AnimMath.Slide(transform.localRotation, startingRotation, .05f);
            }
        }

        void MachineGun() {
            if (bulletAmountTime > 0) return;

            Projectile leftBullets = Instantiate(prefabMachineGunBullets, leftMuzzle.position, Quaternion.identity);
            leftBullets.InitBullet(transform.forward * 30);

            Projectile RightBullets = Instantiate(prefabMachineGunBullets, rightMuzzle.position, Quaternion.identity);
            RightBullets.InitBullet(transform.forward * 30);

            bulletAmountTime = 1 / roundPerSec;
        }

        void Missles() {
            MissleScript startMissle = GetComponentInChildren<MissleScript>();
            if (startMissle)
                startMissle.timeToLaunch = true;
        }

        private bool CanSeeThing(Transform thing, float viewingDis) {

            if (!thing) return false; // uh... error

            Vector3 vToThing = thing.position - transform.position;

            // check distance
            if (vToThing.sqrMagnitude > viewingDis * viewingDis) {
                return false; // Too far away to see...
            }

            // check direction
            if (Vector3.Angle(transform.forward, vToThing) > viewingAngle) return false; // out of vision "cone"

            // TODO: Check occulusion

            return true;
        }
    }
}