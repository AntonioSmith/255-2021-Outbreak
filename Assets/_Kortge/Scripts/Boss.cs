using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kortge
{
    /// <summary>
    /// Controls the behavior of the boss.
    /// </summary>
    public class Boss : MonoBehaviour
    {
        /// <summary>
        /// Lists all of the states the boss can switch between.
        /// </summary>
        public enum BossState
        {
            Introduction,
            Teleport,
            WindUp,
            AstralProjections,
            Charge,
            Stun,
            PostHitInvulnerability,
            Death
        }

        /// <summary>
        /// The different behaviors the boss has.
        /// </summary>
        public static class States
        {
            /// <summary>
            /// Sets the template for how switching between states is handled.
            /// </summary>
            public class State
            {
                /// <summary>
                /// Used to reference anything inside of that boss class that a state can update.
                /// </summary>
                protected Boss boss;
                /// <summary>
                /// Repeats each frame.
                /// </summary>
                /// <returns></returns>
                virtual public State Update()
                {
                    return null;
                }
                /// <summary>
                /// Activates as soon as a state is started, while also referencing the boss class.
                /// </summary>
                /// <param name="boss"></param>
                virtual public void OnStart(Boss boss)
                {
                    this.boss = boss;
                }
                /// <summary>
                /// Activates once the state is switched to something else.
                /// </summary>
                virtual public void OnEnd()
                {

                }
            }
            /// <summary>
            /// The boss remains idle until approached to give the player time to get used to the controls.
            /// </summary>
            public class Introduction : State
            {
                /// <summary>
                /// The boss checks if the player is near enough for it to teleport.
                /// </summary>
                /// <returns></returns>
                public override State Update()
                {
                    float distance = Vector3.Distance(boss.transform.position, boss.player.position);
                    if (distance <= 4) return new States.Teleport();
                    return null;
                }
                /// <summary>
                /// A reference to the boss is set.
                /// </summary>
                /// <param name="boss"></param>
                public override void OnStart(Boss boss)
                {
                    base.OnStart(boss);
                }
            }
            /// <summary>
            /// The boss teleports from one area to another through a smoke effect.
            /// </summary>
            public class Teleport : State
            {
                /// <summary>
                /// How much more time this state will remain active for.
                /// </summary>
                float stateTime;
                /// <summary>
                /// Winds the time down until there is none left and the state is switched.
                /// </summary>
                /// <returns></returns>
                public override State Update()
                {
                    stateTime -= Time.deltaTime;
                    if (stateTime <= 0) return new States.WindUp();
                    return null;
                }
                /// <summary>
                /// Activates the "smoke out" method, sets the state time, and gets the boss reference.
                /// </summary>
                /// <param name="boss"></param>
                public override void OnStart(Boss boss)
                {
                    boss.SmokeOut();
                    stateTime = boss.stateTime;
                    base.OnStart(boss);
                }
                /// <summary>
                /// Activates the "smoke in" method and makes the boss look at the player.
                /// </summary>
                public override void OnEnd()
                {
                    boss.SmokeIn();
                    boss.focused = true;
                    boss.colliding = false;
                }
            }
            /// <summary>
            /// A brief moment of time that occurs before the boss attacks the player.
            /// </summary>
            public class WindUp : State
            {
                /// <summary>
                /// How much more time this state will remain active for.
                /// </summary>
                float stateTime;
                /// <summary>
                /// Winds the time down until there is none left and the state is switched. An wind-up animation is played shortly before the state ends.
                /// </summary>
                /// <returns></returns>
                public override State Update()
                {
                    stateTime -= Time.deltaTime;
                    if(stateTime <= 0.25f ) boss.animator.SetBool("BeamReady", true);
                    if (stateTime <= 0) return new States.AstralProjections();
                    return null;
                }
                /// <summary>
                /// Sets the state time and gets the boss reference.
                /// </summary>
                /// <param name="boss"></param>
                public override void OnStart(Boss boss)
                {
                    stateTime = boss.stateTime;
                    base.OnStart(boss);
                }
                /// <summary>
                /// Disable the animator bool to prepare it for the next animation.
                /// </summary>
                public override void OnEnd()
                {
                    boss.animator.SetBool("BeamReady", false);
                }
            }
            /// <summary>
            /// Fires a flurry of projectiles at the player.
            /// </summary>
            public class AstralProjections : State
            {
                /// <summary>
                /// The amount of time in between astral projections.
                /// </summary>
                float shotDelay;
                /// <summary>
                /// How much time is left until a new a astral projections.
                /// </summary>
                float shotTime;
                /// <summary>
                /// How much more time this state will remain active for.
                /// </summary>
                float stateTime = 1;
                /// <summary>
                /// Determines when it is time to fire a new astral projection or switch to a new state.
                /// </summary>
                /// <returns></returns>
                public override State Update()
                {
                    stateTime -= Time.deltaTime;
                    shotTime -= Time.deltaTime;
                    if (stateTime <= 0) return new States.Charge();
                    if (shotTime <= 0)
                    {
                        boss.AstralProjection(5 + (6-boss.health.health));
                        shotTime = shotDelay;
                    }
                    else shotTime -= Time.deltaTime;
                    return null;
                }
                /// <summary>
                /// Sets up the shot delay based on the boss's current health while getting a reference to the boss.
                /// </summary>
                /// <param name="boss"></param>
                public override void OnStart(Boss boss)
                {
                    shotDelay = 1f/(12f*(6-boss.health.health));
                    shotTime = shotDelay;
                    base.OnStart(boss);
                }
            }
            /// <summary>
            /// The boss rushes towards the player while leaving damaging afterImages behinds.
            /// </summary>
            public class Charge : State
            {
                /// <summary>
                /// Moves the boss and leaves behind afterimages until the boss runs into a wall.
                /// </summary>
                /// <returns></returns>
                public override State Update()
                {
                    boss.controller.SimpleMove(boss.transform.forward * ((6-boss.health.health) * 4));
                    AfterImage();
                    if (boss.colliding) return new States.Stun();
                    return null;
                }

                /// <summary>
                /// Spawns an afterimage and sets the amount of time it is active depending on how much health the boss has.
                /// </summary>
                private void AfterImage()
                {
                    AfterImage newImage = Instantiate(boss.afterImage, boss.transform.position, boss.transform.rotation);
                    newImage.destroyTime = 1f / boss.health.health;
                }
                /// <summary>
                /// A rose is thrown for the player to retrieve and the boss is no longer looking at player automatically. The boss reference is also set.
                /// </summary>
                /// <param name="boss"></param>
                public override void OnStart(Boss boss)
                {
                    boss.maidens.ThrowRose();
                    boss.focused = false;
                    base.OnStart(boss);
                }
            }

            /// <summary>
            /// The boss is slammed into a wall, leaving it vulnerable to attack.
            /// </summary>
            public class Stun : State
            {
                /// <summary>
                /// How much more time this state will remain active for.
                /// </summary>
                float stateTime;
                /// <summary>
                /// Checks if the boss is hit and needs to switch to another state.
                /// </summary>
                /// <returns></returns>
                public override State Update()
                {
                    stateTime -= Time.deltaTime;
                    if (boss.hit)
                    {
                        return Hit();
                    }
                    if (stateTime <= 0) return new States.Teleport();
                    return null;
                }

                private State Hit()
                {
                    if (boss.health.health <= 0) return new States.Death();
                    boss.hit = false;
                    return new States.PostHitInvulnerability();
                }
                /// <summary>
                /// Makes the boss vulnerable to attack for a length of time depending on how much health is left while setting the boss reference.
                /// </summary>
                /// <param name="boss"></param>
                public override void OnStart(Boss boss)
                {
                    boss.sweat.Play();
                    boss.health.vulnerable = true;
                    stateTime = boss.stateTime;
                    base.OnStart(boss);
                }
                /// <summary>
                /// Reverts the animation, particle system, and vulnerability status back to normal.
                /// </summary>
                public override void OnEnd()
                {
                    boss.sweat.Stop();
                    boss.health.vulnerable = false;
                    boss.animator.SetTrigger("Beam Finish");
                }
            }
            /// <summary>
            /// A state played to inform the player that the boss has been hit.
            /// </summary>
            public class PostHitInvulnerability : State
            {
                /// <summary>
                /// How much more time this state will remain active for.
                /// </summary>
                float stateTime = 0.2f;
                /// <summary>
                /// Winds the time down until there is none left and the state is switched.
                /// </summary>
                /// <returns></returns>
                public override State Update()
                {
                    stateTime -= Time.deltaTime;
                    if (stateTime <= 0) return new States.Teleport();
                    return null;
                }
                /// <summary>
                /// Plays the next animation and sets the reaction time based on the boss's health.
                /// </summary>
                /// <param name="boss"></param>
                public override void OnStart(Boss boss)
                {
                    boss.animator.SetTrigger("Beam Finish");
                    stateTime = boss.stateTime;
                }
            }
            /// <summary>
            /// Play's the boss's death particle effect for some time before transfering the player to the next zone.
            /// </summary>
            public class Death : State
            {
                /// <summary>
                /// Set's the boss's status to "dead."
                /// </summary>
                /// <param name="boss"></param>
                public override void OnStart(Boss boss)
                {
                    boss.dead = true;
                }
            }
        }

        /// <summary>
        /// Checks if the boss is colliding with an object.
        /// </summary>
        private bool colliding;
        /// <summary>
        /// Determines if the boss is making an active effort to look at the player.
        /// </summary>
        private bool focused = true;
        /// <summary>
        /// Controls the sprite of the boss.
        /// </summary>
        private Animator animator;
        /// <summary>
        /// Calculates movement for the boss.
        /// </summary>
        private CharacterController controller;
        /// <summary>
        /// Keeps track of how much health the boss has.
        /// </summary>
        private Health health;
        /// <summary>
        /// The current behavior of the boss.
        /// </summary>
        private States.State state;
        /// <summary>
        /// Signifies if the boss is dead or not so that the maidens can throw roses endlessly.
        /// </summary>
        public bool dead;
        /// <summary>
        /// Determines if the boss has been stuck by the player.
        /// </summary>
        public bool hit = false;
        /// <summary>
        /// Determines how long many of the boss's states will last.
        /// </summary>
        public float stateTime;
        /// <summary>
        /// A damaging clone of the boss left behind while dashing.
        /// </summary>
        public AfterImage afterImage;
        /// <summary>
        /// The object the boss singals to throw roses while charging.
        /// </summary>
        public Maidens maidens;
        /// <summary>
        /// Particle system that is instantiated when it has finished teleporting.
        /// </summary>
        public ParticleSystem smokeIn;
        /// <summary>
        /// Particle system that is instantiated when it has started teleporting.
        /// </summary>
        public ParticleSystem smokeOut;
        /// <summary>
        /// Emits when the boss is stunned to signify that it is vulnerable.
        /// </summary>
        public ParticleSystem sweat;
        /// <summary>
        /// The prefab used to represent projectiles shot out.
        /// </summary>
        public Projectile astralProjectionPrefab;
        /// <summary>
        /// The character the boss focuses on.
        /// </summary>
        public Transform player;
        /// <summary>
        /// Gathers the animator, controller, and health scripts and sets the state time to one second by default.
        /// </summary>
        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponentInChildren<Animator>();
            controller = GetComponent<CharacterController>();
            health = GetComponent<Health>();
            stateTime = 1f;
        }

        /// <summary>
        /// Manages the current state, makes the boss look at the player, and update the state time based on the current amount of health left.
        /// </summary>
        // Update is called once per frame
        void Update()
        {
            StateManagement();

            if (focused) LookAtPlayer();

            stateTime = health.health * 0.2f;
        }
        /// <summary>
        /// Determines when to switch the scene or just update it.
        /// </summary>
        private void StateManagement()
        {
            if (state == null) SwitchState(new States.Introduction());

            if (state != null)
            {
                if (state != null) SwitchState(state.Update());
            };
        }
        /// <summary>
        /// Changes the state to something different.
        /// </summary>
        /// <param name="newState"></param>
        void SwitchState(States.State newState)
        {
            if (newState == null) return;

            if (state != null) state.OnEnd();

            state = newState;

            state.OnStart(this);
        }
        /// <summary>
        /// Teleports the boss above the camera with a smoke effect.
        /// </summary>
        void SmokeOut()
        {
            Instantiate(smokeOut, transform.position, new Quaternion(0,0,0,0));
            transform.position = Vector3.up * (20);
        }
        /// <summary>
        /// Teleports the boss to a random location within the arena with a smoke effet.
        /// </summary>
        void SmokeIn()
        {
            transform.position = (Vector3.right * Random.Range(-9f, 9f)) + (Vector3.forward * Random.Range(-4f, 4f)) + (Vector3.up/2);
            Instantiate(smokeIn, transform.position, new Quaternion(0, 0, 0, 0));
        }
        /// <summary>
        /// Keeps the boss facing the direction the player is in.
        /// </summary>
        void LookAtPlayer()
        {
            if (player != null)
            {
                Vector3 vectorToHitPos = player.position - transform.position;

                float angle = Mathf.Atan2(vectorToHitPos.x, vectorToHitPos.z);

                angle /= Mathf.PI;
                angle *= 180; // Converts from radians to half-circles to degrees.

                transform.eulerAngles = new Vector3(0, angle, 0);
            }
            else return;
        }
        /// <summary>
        /// Fires an astral projection in the direction of the player.
        /// </summary>
        /// <param name="speed"></param>
        void AstralProjection(float speed)
        {
            Projectile projection = Instantiate(astralProjectionPrefab, transform.position + transform.forward, transform.rotation);
            projection.InitBullet(transform.forward * speed);
        }
        /// <summary>
        /// Detects collision to stun the boss while charging.
        /// </summary>
        /// <param name="hit"></param>
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            GameObject wall = hit.gameObject;
            if(wall.CompareTag("Wall")) colliding = true;
        }
    }
}