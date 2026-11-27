#nullable enable
using System.Collections.Generic;
using RFL.Data;
using RFL.Sim;
using UnityEngine;

namespace RFL.Game.GreyBox
{
    /// <summary>
    /// Draws one robot from its <see cref="RobotState"/>. With the robot's art prefab it poses the Humanoid
    /// skeleton procedurally from the move's frame data (guard, punches, uppercut, kicks, crouch, knockdown)
    /// until real animation clips exist; without it, it falls back to the grey-box capsule and a cube arm.
    /// View only: nothing here feeds back into the fight.
    /// </summary>
    public sealed class RobotView
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly Color HitTint = new Color(1.9f, 0.55f, 0.45f);
        static readonly Color BlockTint = new Color(0.55f, 0.8f, 1.9f);
        static readonly Color ActiveTint = new Color(1.6f, 1.45f, 1.1f);
        static readonly Color OverloadTint = new Color(0.55f, 0.85f, 2.2f);

        readonly Transform _root;
        readonly float _height;
        readonly float _radius;
        readonly bool _art;
        readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();
        Color _flashColor;
        float _flashUntil;
        Transform _marker = null!;

        // Capsule fallback
        Renderer _capsule = null!;
        Color _capsuleColor;
        Transform _cubeArm = null!;
        Renderer _cubeArmRenderer = null!;

        // Art
        Renderer[] _pieces = null!;
        Color[] _pieceBase = null!;
        int[] _piecePart = null!; // Part index per piece, -1 if none
        Renderer _armL = null!,
            _armR = null!,
            _legs = null!;
        Transform _hips = null!,
            _lUpArm = null!,
            _lLowArm = null!,
            _lHand = null!,
            _rUpArm = null!,
            _rLowArm = null!,
            _rHand = null!,
            _lUpLeg = null!,
            _lLowLeg = null!,
            _lFoot = null!,
            _rUpLeg = null!,
            _rLowLeg = null!,
            _rFoot = null!;
        readonly List<KeyValuePair<Transform, Quaternion>> _rest =
            new List<KeyValuePair<Transform, Quaternion>>();
        Vector3 _hipsRest;

        public RobotView(Transform root, StarterTypeDef def, FighterDef body, GameObject? artPrefab)
        {
            _root = root;
            _height = def.Class switch
            {
                WeightClass.Light => 2.2f,
                WeightClass.Heavy => 2.8f,
                _ => 2.5f,
            };
            _radius = body.RadiusMm / 1000f;
            var placeholder = root.GetComponentInChildren<Renderer>();
            var sideMaterial = placeholder != null ? placeholder.sharedMaterial : null;
            _art = artPrefab != null && SetupArt(artPrefab);
            if (!_art)
                SetupCapsule(placeholder!); // the grey-box root always carries its capsule renderer
            AddMarker(sideMaterial);
        }

        public static float F(Fix f) => f.Raw / (float)Fix.OneRaw;

        public void Flash(bool blocked)
        {
            _flashColor = blocked ? BlockTint : HitTint;
            _flashUntil = Time.unscaledTime + 0.14f;
        }

        bool SetupArt(GameObject prefab)
        {
            var instance = Object.Instantiate(prefab, _root);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            var anim = instance.GetComponentInChildren<Animator>();
            Transform Bone(HumanBodyBones b)
            {
                var t = anim != null && anim.isHuman ? anim.GetBoneTransform(b) : null;
                // A missing bone stays null here; the bone check below rejects the prefab.
                return t != null ? t : FindDeep(instance.transform, b.ToString())!;
            }
            _hips = Bone(HumanBodyBones.Hips);
            _lUpArm = Bone(HumanBodyBones.LeftUpperArm);
            _lLowArm = Bone(HumanBodyBones.LeftLowerArm);
            _lHand = Bone(HumanBodyBones.LeftHand);
            _rUpArm = Bone(HumanBodyBones.RightUpperArm);
            _rLowArm = Bone(HumanBodyBones.RightLowerArm);
            _rHand = Bone(HumanBodyBones.RightHand);
            _lUpLeg = Bone(HumanBodyBones.LeftUpperLeg);
            _lLowLeg = Bone(HumanBodyBones.LeftLowerLeg);
            _lFoot = Bone(HumanBodyBones.LeftFoot);
            _rUpLeg = Bone(HumanBodyBones.RightUpperLeg);
            _rLowLeg = Bone(HumanBodyBones.RightLowerLeg);
            _rFoot = Bone(HumanBodyBones.RightFoot);
            var bones = new[]
            {
                _hips,
                _lUpArm,
                _lLowArm,
                _lHand,
                _rUpArm,
                _rLowArm,
                _rHand,
                _lUpLeg,
                _lLowLeg,
                _lFoot,
                _rUpLeg,
                _rLowLeg,
                _rFoot,
            };
            foreach (var b in bones)
            {
                if (b == null)
                {
                    Object.Destroy(instance);
                    Debug.LogWarning(
                        $"{prefab.name}: missing Humanoid bones; using the grey-box capsule."
                    );
                    return false;
                }
                _rest.Add(new KeyValuePair<Transform, Quaternion>(b, b.localRotation));
            }
            _hipsRest = _hips.localPosition;
            if (anim != null)
                anim.enabled = false; // no clips yet: the pose is set here every frame

            foreach (Transform child in _root)
            {
                if (child != instance.transform)
                    child.gameObject.SetActive(false); // hide the capsule placeholder
            }
            _root.localScale = Vector3.one;
            _pieces = instance.GetComponentsInChildren<Renderer>();
            _pieceBase = new Color[_pieces.Length];
            _piecePart = new int[_pieces.Length];
            for (int i = 0; i < _pieces.Length; i++)
            {
                var mat = _pieces[i].sharedMaterial;
                _pieceBase[i] =
                    mat != null && mat.HasProperty(BaseColorId)
                        ? mat.GetColor(BaseColorId)
                        : Color.white;
                string n = _pieces[i].name;
                _piecePart[i] =
                    n.EndsWith("_Head") ? (int)Part.Head
                    : n.EndsWith("_Torso") ? (int)Part.Torso
                    : n.EndsWith("_ArmL") ? (int)Part.ArmL
                    : n.EndsWith("_ArmR") ? (int)Part.ArmR
                    : n.EndsWith("_Legs") ? (int)Part.Legs
                    : -1;
                if (n.EndsWith("_ArmL"))
                    _armL = _pieces[i];
                else if (n.EndsWith("_ArmR"))
                    _armR = _pieces[i];
                else if (n.EndsWith("_Legs"))
                    _legs = _pieces[i];
            }
            return true;
        }

        static Transform? FindDeep(Transform t, string name)
        {
            if (t.name == name)
                return t;
            foreach (Transform c in t)
            {
                var f = FindDeep(c, name);
                if (f != null)
                    return f;
            }
            return null;
        }

        void SetupCapsule(Renderer placeholder)
        {
            _capsule = placeholder;
            _capsuleColor = _capsule.sharedMaterial.GetColor(BaseColorId);
            _root.localScale = new Vector3(_radius * 2f, _height / 2f, _radius * 2f);
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = _root.name + " arm";
            Object.Destroy(go.GetComponent<Collider>());
            _cubeArm = go.transform;
            _cubeArmRenderer = go.GetComponent<Renderer>();
            _cubeArmRenderer.sharedMaterial = _capsule.sharedMaterial;
        }

        /// <summary>A flat disc in the player's colour under the robot, so identical robots stay readable.</summary>
        void AddMarker(Material? sideMaterial)
        {
            if (sideMaterial == null)
                return;
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = _root.name + " marker";
            Object.Destroy(disc.GetComponent<Collider>());
            disc.GetComponent<Renderer>().sharedMaterial = sideMaterial;
            disc.transform.localScale = new Vector3(_radius * 2.3f, 0.004f, _radius * 2.3f);
            _marker = disc.transform;
        }

        public void Draw(RobotState r, Simulation sim, int side, bool hitstop)
        {
            if (_marker != null)
                _marker.position = new Vector3(F(r.X), 0.006f, 0f);
            if (_art)
                DrawArt(r, sim, sim.Fighter(side), hitstop);
            else
                DrawCapsule(r, sim, hitstop);
        }

        /// <summary>Lean in degrees (positive leans back) and a wobble, from the robot's mode.</summary>
        static float Lean(in RobotState r)
        {
            switch (r.Mode)
            {
                case RobotMode.Knockdown:
                case RobotMode.Ko:
                    return 82f;
                case RobotMode.WakeUp:
                    return 82f * Mathf.Clamp01(r.Stun / (float)Simulation.WakeUpFrames);
                case RobotMode.Launched:
                    return 35f
                        + 30f * Mathf.Clamp01(r.ModeFrame / (float)Mathf.Max(1, r.ModeLength));
                case RobotMode.Hitstun:
                    return r.SlideFrames > 0 ? 20f : 9f;
                case RobotMode.Blockstun:
                    return 4f;
                case RobotMode.Groggy:
                    return 6f + Mathf.Sin(Time.unscaledTime * 7f) * 8f;
                case RobotMode.Overload:
                    return 3f + Mathf.Sin(Time.unscaledTime * 40f) * 1.5f;
                case RobotMode.Dash:
                    return -10f;
                case RobotMode.Backdash:
                    return 8f;
                default:
                    return 0f;
            }
        }

        static float Extension(MoveDef m, int frame, out bool active)
        {
            active = false;
            if (m.InStartup(frame))
                return 0.15f * frame / Mathf.Max(1, m.Startup);
            if (m.IsActive(frame))
            {
                active = true;
                return 1f;
            }
            return Mathf.Clamp01(
                1f - (frame - m.Startup - m.Active) / (float)Mathf.Max(1, m.Recovery)
            );
        }

        static bool IsLeftArm(MoveId id) => id == MoveId.Jab || id == MoveId.Uppercut;

        static bool IsRightArm(MoveId id) => id == MoveId.Cross || id == MoveId.Straight;

        static bool IsKick(MoveId id) => id == MoveId.LowKick || id == MoveId.BodyKick;

        void DrawArt(RobotState r, Simulation sim, FighterDef body, bool hitstop)
        {
            bool down =
                r.Mode == RobotMode.Knockdown
                || r.Mode == RobotMode.Ko
                || r.Mode == RobotMode.WakeUp;
            float tilt = Lean(r);
            float shake =
                hitstop
                && (
                    r.Mode == RobotMode.Hitstun
                    || r.Mode == RobotMode.Blockstun
                    || r.Mode == RobotMode.Groggy
                    || r.Mode == RobotMode.Overload
                )
                    ? Mathf.Sin(Time.unscaledTime * 90f) * 0.03f
                    : 0f;
            _root.position = new Vector3(F(r.X) + shake, F(r.Y), 0f);
            _root.rotation =
                Quaternion.Euler(0f, 0f, tilt * r.Facing)
                * Quaternion.Euler(0f, 90f * r.Facing, 0f);

            foreach (var kv in _rest)
                kv.Key.localRotation = kv.Value;
            _hips.localPosition = _hipsRest;

            Vector3 fwd = _root.forward;
            Vector3 up = _root.up;
            Vector3 left = (_lUpArm.position - _rUpArm.position).normalized;

            bool active = false;
            Renderer? activePiece = null;
            if (!down)
            {
                bool airborne =
                    r.Mode == RobotMode.Jump && r.Phase == 1 || r.Mode == RobotMode.Launched;
                bool squat = r.Mode == RobotMode.Jump && r.Phase != 1;
                if (airborne)
                {
                    // Knees tucked in the air.
                    Aim(_lUpLeg, _lLowLeg, (fwd * 0.8f - up * 0.35f + left * 0.1f).normalized);
                    Aim(_lLowLeg, _lFoot, (-fwd * 0.2f - up * 0.98f).normalized);
                    Aim(_rUpLeg, _rLowLeg, (fwd * 0.65f - up * 0.6f - left * 0.1f).normalized);
                    Aim(_rLowLeg, _rFoot, (-fwd * 0.3f - up * 0.95f).normalized);
                }
                else if ((r.Crouching || squat) && r.Mode != RobotMode.Attack)
                {
                    _hips.position -= up * _height * 0.13f;
                    Aim(_lUpLeg, _lLowLeg, (fwd * 0.75f - up * 0.65f + left * 0.12f).normalized);
                    Aim(_lLowLeg, _lFoot, (-fwd * 0.45f - up * 0.9f).normalized);
                    Aim(_rUpLeg, _rLowLeg, (fwd * 0.55f - up * 0.8f - left * 0.12f).normalized);
                    Aim(_rLowLeg, _rFoot, (-fwd * 0.5f - up * 0.86f).normalized);
                }

                var guardUpperL = (fwd * 0.3f - up * 0.9f + left * 0.25f).normalized;
                var guardLowerL = (fwd * 0.55f + up * 0.78f - left * 0.3f).normalized;
                var guardUpperR = (fwd * 0.3f - up * 0.9f - left * 0.25f).normalized;
                var guardLowerR = (fwd * 0.55f + up * 0.78f + left * 0.3f).normalized;
                float eL = 0f,
                    eR = 0f;
                Vector3 punchL = fwd,
                    punchR = fwd;
                if (r.Mode == RobotMode.Attack)
                {
                    var m = sim.Move(r.Move);
                    float e = Extension(m, r.MoveFrame, out active);
                    if (IsLeftArm(r.Move))
                    {
                        eL = e;
                        punchL =
                            r.Move == MoveId.Uppercut
                                ? (fwd * 0.45f + up * 0.9f - left * 0.1f).normalized
                                : (fwd - left * 0.12f + up * 0.04f).normalized;
                        activePiece = _armL;
                    }
                    else if (IsRightArm(r.Move))
                    {
                        eR = e;
                        punchR = (fwd + left * 0.12f + up * 0.04f).normalized;
                        activePiece = _armR;
                    }
                    else if (IsKick(r.Move))
                    {
                        bool low = r.Move == MoveId.LowKick;
                        var thigh = low ? _lUpLeg : _rUpLeg;
                        var shin = low ? _lLowLeg : _rLowLeg;
                        var foot = low ? _lFoot : _rFoot;
                        var kick = low
                            ? (fwd * 0.85f - up * 0.5f).normalized
                            : (fwd * 0.95f + up * 0.3f).normalized;
                        var thighRest = (shin.position - thigh.position).normalized;
                        var shinRest = (foot.position - shin.position).normalized;
                        Aim(thigh, shin, Vector3.Slerp(thighRest, kick, e));
                        Aim(shin, foot, Vector3.Slerp(shinRest, kick, e));
                        activePiece = _legs;
                    }
                }
                Aim(_lUpArm, _lLowArm, Vector3.Slerp(guardUpperL, punchL, eL));
                Aim(_lLowArm, _lHand, Vector3.Slerp(guardLowerL, punchL, eL));
                Aim(_rUpArm, _rLowArm, Vector3.Slerp(guardUpperR, punchR, eR));
                Aim(_rLowArm, _rHand, Vector3.Slerp(guardLowerR, punchR, eR));
            }

            float t = Mathf.Clamp01((_flashUntil - Time.unscaledTime) / 0.14f);
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 18f);
            bool flicker = Mathf.Repeat(Time.unscaledTime * 6f, 1f) < 0.5f;
            for (int i = 0; i < _pieces.Length; i++)
            {
                var c = _pieceBase[i];
                if (_piecePart[i] >= 0)
                {
                    switch (StatusRules.StateOf(r.GetPart((Part)_piecePart[i]), body.PartMax))
                    {
                        case PartState.Damaged:
                            c *= 0.62f;
                            break;
                        case PartState.Broken:
                            c *= flicker ? 0.3f : 0.7f;
                            break;
                    }
                    c.a = 1f;
                }
                if (r.Mode == RobotMode.Overload)
                    c = Color.Lerp(c, c * OverloadTint, pulse);
                if (active && _pieces[i] == activePiece)
                    c *= ActiveTint;
                c = Color.Lerp(c, _pieceBase[i] * _flashColor, t);
                _block.Clear();
                _block.SetColor(BaseColorId, c);
                _pieces[i].SetPropertyBlock(_block);
            }
        }

        /// <summary>Rotates <paramref name="bone"/> so the direction to <paramref name="child"/> becomes <paramref name="dir"/>.</summary>
        static void Aim(Transform bone, Transform child, Vector3 dir)
        {
            var cur = child.position - bone.position;
            if (cur.sqrMagnitude < 1e-8f || dir.sqrMagnitude < 1e-8f)
                return;
            bone.rotation = Quaternion.FromToRotation(cur.normalized, dir) * bone.rotation;
        }

        void DrawCapsule(RobotState r, Simulation sim, bool hitstop)
        {
            float x = F(r.X);
            bool down = r.Mode == RobotMode.Knockdown || r.Mode == RobotMode.Ko;
            float tilt =
                down ? 75f
                : r.Mode == RobotMode.Hitstun ? 12f
                : 0f;
            float drop =
                down ? _height * 0.32f
                : r.Crouching ? _height * 0.12f
                : 0f;
            float shake =
                hitstop && r.Mode == RobotMode.Hitstun
                    ? Mathf.Sin(Time.unscaledTime * 90f) * 0.03f
                    : 0f;
            _root.position = new Vector3(x + shake, _height / 2f - drop + F(r.Y), 0f);
            _root.rotation = Quaternion.Euler(0f, 0f, (down ? tilt : Lean(r)) * r.Facing);
            float t = Mathf.Clamp01((_flashUntil - Time.unscaledTime) / 0.14f);
            _block.Clear();
            _block.SetColor(BaseColorId, Color.Lerp(_capsuleColor, Color.white, t));
            _capsule.SetPropertyBlock(_block);

            float shoulder = _height * 0.72f - drop;
            float length = 0.35f,
                angle = 0f,
                side = -0.22f;
            bool active = false;
            _cubeArm.gameObject.SetActive(!down);
            if (down)
                return;
            if (r.Mode == RobotMode.Attack)
            {
                var m = sim.Move(r.Move);
                float e = Extension(m, r.MoveFrame, out active);
                length = 0.35f + e * Mathf.Max(0f, m.ReachMm / 1000f - _radius - 0.35f);
                if (r.Move == MoveId.Uppercut)
                {
                    angle = 55f * e;
                    shoulder = _height * 0.55f;
                }
                if (r.Move == MoveId.LowKick)
                {
                    angle = -35f;
                    shoulder = _height * 0.3f;
                }
                if (r.Move == MoveId.BodyKick)
                {
                    angle = 10f;
                    shoulder = _height * 0.45f;
                }
                side = IsRightArm(r.Move) || r.Move == MoveId.BodyKick ? 0.22f : -0.22f;
            }
            float rad = angle * Mathf.Deg2Rad;
            var dir = new Vector3(Mathf.Cos(rad) * r.Facing, Mathf.Sin(rad), 0f);
            var start = new Vector3(x + r.Facing * _radius * 0.8f, shoulder, side);
            _cubeArm.position = start + dir * (length * 0.5f);
            _cubeArm.rotation = Quaternion.FromToRotation(Vector3.right, dir);
            _cubeArm.localScale = new Vector3(length, 0.17f, 0.17f);
            _block.Clear();
            _block.SetColor(BaseColorId, active ? Color.white : _capsuleColor * 0.8f);
            _cubeArmRenderer.SetPropertyBlock(_block);
        }
    }
}
