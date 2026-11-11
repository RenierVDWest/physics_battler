using PhysicsBattler.Combat;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Audio
{
    /// <summary>
    /// Where the game's sounds come from: everywhere two things meet. Listens to what the fight and the lab already
    /// tell (a blow landing on a body, a shield or a blade; a blade glancing off; a swing about to land; a limb taken
    /// off; a foot coming down or a leap landing; a body part or a dropped weapon striking the ground; two shields
    /// meeting) and plays the sound for each, louder for a harder meeting. Hooked up by itself when play starts, in
    /// every scene; with no sounds made yet it stays quiet.
    /// </summary>
    public static class SoundHooks
    {
        /// <summary>Below this speed (m/s) a meeting is too soft to hear.</summary>
        public const float LeastHeard = 0.6f;

        /// <summary>The speed (m/s) a blow is heard at full volume, and a body or a dropped thing on the ground.</summary>
        public const float FullBlow = 12f, FullFall = 4f;

        private static float lastFall, lastThud, lastDrop, lastShields;
        private static readonly System.Collections.Generic.Dictionary<BodyPart, float> lastStep = new System.Collections.Generic.Dictionary<BodyPart, float>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook() => Install();

        /// <summary>Listens to everything that makes a sound (done by itself when play starts; the headless runners call it).</summary>
        public static void Install()
        {
            Unhook();
            StrikeHits.Sounded += OnLabMet;
            StrikeHits.Swung += OnLabSwung;
            LocoBody.FootPlanted += OnFoot;
            UnitController.Hit += OnHit;
            UnitController.Severed += OnSevered;
            UnitController.MoveStarted += OnMoveStarted;
            BodyPart.Collided += OnPartCollided;
            Weapon.Collided += OnWeaponCollided;
            Sfx.SetAmbience(0f);
        }

        private static void Unhook()
        {
            StrikeHits.Sounded -= OnLabMet;
            StrikeHits.Swung -= OnLabSwung;
            LocoBody.FootPlanted -= OnFoot;
            UnitController.Hit -= OnHit;
            UnitController.Severed -= OnSevered;
            UnitController.MoveStarted -= OnMoveStarted;
            BodyPart.Collided -= OnPartCollided;
            Weapon.Collided -= OnWeaponCollided;
        }

        private static float Loudness(float speed, float full) => Mathf.Clamp(0.45f + 0.55f * speed / full, 0.45f, 1f);

        // ---- The lab ------------------------------------------------------------------------------------------

        private static void OnLabMet(StrikeHits.Met met, Vector3 at, float speed, bool club, bool showstopper)
        {
            float loud = Loudness(speed, FullBlow);
            switch (met)
            {
                case StrikeHits.Met.Flesh:
                    Sfx.Play(club ? SfxIds.HitClubFlesh : SfxIds.HitBladeFlesh, at, loud);
                    break;
                case StrikeHits.Met.Shield:
                    Sfx.Play(club ? SfxIds.ClubShield : SfxIds.BladeShield, at, loud);
                    break;
                case StrikeHits.Met.Parry:
                    Sfx.Play(club ? SfxIds.ClubParry : SfxIds.ParryClang, at, loud);
                    break;
                case StrikeHits.Met.Glance:
                    Sfx.Play(SfxIds.BladeGlance, at, loud);
                    break;
                case StrikeHits.Met.TouchShield:
                case StrikeHits.Met.TouchBlade:
                    if (speed >= LeastHeard)
                    {
                        Sfx.Play(SfxIds.BladeTap, at, Loudness(speed, 4f));
                    }

                    break;
            }

            // (The combo's last blow lands with a boom of its own: Bam! Bam! BAM!)
            if (showstopper && (met == StrikeHits.Met.Flesh || met == StrikeHits.Met.Parry || met == StrikeHits.Met.Shield))
            {
                Sfx.Play(SfxIds.HitShowstopper, at, met == StrikeHits.Met.Flesh ? 1f : 0.6f);
            }
        }

        private static void OnLabSwung(Vector3 at, bool club, bool heavy)
        {
            Sfx.Play(heavy ? SfxIds.SwingHeavy : club ? SfxIds.SwingClub : SfxIds.SwingBlade, at);
        }

        // ---- Feet (the lab's and the fight's) --------------------------------------------------------------------

        private static void OnFoot(LocoBody body, int foot, Vector3 at, float speed, float aloft)
        {
            if (aloft > 0.3f)
            {
                Sfx.Play(SfxIds.Land, at, Mathf.Clamp(0.5f + aloft, 0.5f, 1f));
            }
            else if (speed > 2.6f)
            {
                Sfx.Play(SfxIds.StepRun, at, Loudness(speed, 5f));
            }
            else if (speed > 0.15f || aloft > 0.12f)
            {
                Sfx.Play(SfxIds.StepWalk, at, Mathf.Clamp(0.5f + speed * 0.3f, 0.5f, 1f));
            }
        }

        // ---- The fight ----------------------------------------------------------------------------------------

        private static bool IsClub(MoveState move) => move != null && move.Weapon != null && move.Weapon.kind == WeaponKind.Club;

        private static void OnHit(HitReport r)
        {
            if (r == null)
            {
                return;
            }

            float loud = Loudness(r.Speed, FullBlow);
            bool club = IsClub(r.Move);
            if (r.Impact)
            {
                Sfx.Play(SfxIds.BodyImpact, r.Point, loud);
                return;
            }

            if (r.Parried)
            {
                Sfx.Play(club ? SfxIds.ClubParry : SfxIds.ParryClang, r.Point, loud);
            }
            else if (r.Shield)
            {
                Sfx.Play(club ? SfxIds.ClubShield : SfxIds.BladeShield, r.Point, loud);
            }
            else
            {
                Sfx.Play(club ? SfxIds.HitClubFlesh : SfxIds.HitBladeFlesh, r.Point, loud);
            }

            if (r.GuardBreak)
            {
                Sfx.Play(SfxIds.GuardBreak, r.Point);
            }
        }

        private static void OnSevered(UnitController unit, PartId part, HitReport r)
        {
            Sfx.Play(SfxIds.Sever, r != null ? r.Point : unit != null ? unit.transform.position : (Vector3?)null);
        }

        private static void OnMoveStarted(UnitController unit, MoveState move)
        {
            if (unit == null || move == null || move.Move == null)
            {
                return;
            }

            // (Heard as the blow comes, just before its contact.)
            float delay = Mathf.Max(0f, move.ContactTime * move.Duration - StrikeHits.SwingHeardBefore);
            Sfx.PlayLater(IsClub(move) ? SfxIds.SwingClub : SfxIds.SwingBlade, delay, unit.transform.position);
        }

        // ---- Bodies and things on the ground; shields meeting --------------------------------------------------

        private static bool IsGround(Collision c) => c != null && c.collider != null && c.collider.gameObject.layer == BodyPart.GroundLayer;

        private static void OnPartCollided(BodyPart part, Collision c)
        {
            if (part == null || !IsGround(c))
            {
                return;
            }

            float speed = c.relativeVelocity.magnitude;
            float now = Time.unscaledTime;
            // (Feet are heard as steps: the new walk's when it plants them (OnFoot); the fight's own gait's as they come down.)
            if (part.id == PartId.FootL || part.id == PartId.FootR)
            {
                if (!UnitController.NewWalk && part.Owner == null && speed > 0.25f && (!lastStep.TryGetValue(part, out float was) || now - was > 0.2f))
                {
                    lastStep[part] = now;
                    Sfx.Play(speed > 2.2f ? SfxIds.StepRun : SfxIds.StepWalk, part.transform.position, Mathf.Clamp(0.5f + speed * 0.25f, 0.5f, 1f));
                }

                return;
            }

            bool trunk = part.id == PartId.Pelvis || part.id == PartId.Spine || part.id == PartId.Chest;
            if (trunk && speed > 1.8f && now - lastFall > 0.4f)
            {
                lastFall = now;
                Sfx.Play(SfxIds.BodyFall, part.transform.position, Loudness(speed, FullFall));
            }
            else if (speed > 1.2f && now - lastThud > 0.08f)
            {
                lastThud = now;
                Sfx.Play(SfxIds.BodyThud, part.transform.position, Loudness(speed, FullFall));
            }
        }

        private static void OnWeaponCollided(Weapon weapon, Collision c)
        {
            if (weapon == null || weapon.definition == null || c == null || c.collider == null)
            {
                return;
            }

            float speed = c.relativeVelocity.magnitude;
            float now = Time.unscaledTime;
            if (weapon.Owner == null)
            {
                // (Dropped: a weapon or a shield striking the ground.)
                if (IsGround(c) && speed > 1f && now - lastDrop > 0.15f)
                {
                    lastDrop = now;
                    Sfx.Play(weapon.definition.IsShield ? SfxIds.ShieldDrop : SfxIds.WeaponDrop, weapon.transform.position, Loudness(speed, FullFall));
                }

                return;
            }

            // (Two held shields meeting: a bash, or two guards pressed together.)
            var other = c.collider.GetComponentInParent<Weapon>();
            if (weapon.definition.IsShield && other != null && other != weapon && other.definition != null && other.definition.IsShield
                && speed > LeastHeard && now - lastShields > 0.12f)
            {
                lastShields = now;
                Sfx.Play(SfxIds.ShieldShield, weapon.transform.position, Loudness(speed, FullFall));
            }
        }
    }
}
