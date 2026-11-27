#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RFL.Game.GreyBox;
using RFL.Game.Input;
using RFL.Game.UI;
using RFL.Sim;
using UnityEngine;

namespace RFL.Game.DevTools
{
    /// <summary>
    /// Development aid: with "-rfl-shots &lt;dir&gt;" on the command line, plays the week's 1v1 Training flow with
    /// synthetic input, saves a PNG of each step and quits. Optional "-rfl-lang id|en". Never active otherwise.
    /// Uses its own profile file, so the player's settings are never touched.
    /// </summary>
    public sealed class ShotRunner : MonoBehaviour
    {
        string _dir = null!;
        string? _lang;

        public static void StartIfRequested(GameObject host)
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "-rfl-shots");
            if (i < 0 || i + 1 >= args.Length)
                return;
            Application.runInBackground = true; // the window launched from a terminal may not have focus
            var runner = host.AddComponent<ShotRunner>();
            runner._dir = args[i + 1];
            Core.GameServices.UseProfilePath(Path.Combine(runner._dir, "profile.json")); // never touch the player's profile
            int l = Array.IndexOf(args, "-rfl-lang");
            runner._lang = l >= 0 && l + 1 < args.Length ? args[l + 1] : null;
        }

        static readonly InputFrame Walk = new InputFrame(0, InputFrame.Right);
        static readonly InputFrame X = new InputFrame(InputFrame.LeftPunch, 0);
        static readonly InputFrame Y = new InputFrame(InputFrame.RightPunch, 0);

        static IEnumerable<InputFrame> Frames(int walk, params (int wait, InputFrame f)[] steps)
        {
            for (int i = 0; i < walk; i++)
                yield return Walk;
            foreach (var (wait, f) in steps)
            {
                for (int i = 0; i < wait; i++)
                    yield return default;
                yield return f;
            }
            for (int i = 0; i < 30; i++)
                yield return default;
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(_dir);
            yield return Until(() =>
                MenuController.Instance != null && MenuController.Instance.Ready
            );
            if (!string.IsNullOrEmpty(_lang))
            {
                Core.GameServices.Settings.S.language = _lang; // the runner's own profile, see StartIfRequested
                Core.GameServices.Settings.ApplyLanguage();
            }
            yield return Wait(0.5f);
            yield return Shot("01-title");
            yield return Press(new NavFrame { Submit = true });
            yield return Press(new NavFrame { Y = -1 });
            yield return Shot("02-main");
            yield return Press(new NavFrame { Submit = true }); // Training
            yield return Shot("03-select-standard");
            yield return Press(new NavFrame { Submit = true }); // P1: STANDARD
            yield return Press(new NavFrame { X = 1 }); // challenger: POWER
            yield return Shot("04-select-challenger");
            yield return Press(new NavFrame { Submit = true });

            GreyBoxController box = null!; // set by the wait below
            yield return Until(() => (box = FindAnyObjectByType<GreyBoxController>()) != null);
            yield return Wait(1.0f);
            yield return Shot("05-vs-p1");
            yield return Wait(1.3f);
            yield return Shot("06-vs-challenger");
            yield return Until(() => box.CurrentStage == GreyBoxController.Stage.Fight);
            yield return Wait(0.3f);
            yield return Shot("07-round-1");
            yield return Until(() =>
                box.State.Phase == MatchPhase.Intro && box.State.PhaseFrames < 25
            );
            yield return Shot("08-fight");
            yield return Until(() => box.State.Phase == MatchPhase.Fight);

            // Combo: X, Y, X launches; a jab juggles.
            box.QueueP1(Frames(30, (4, X), (5, Y), (7, X), (40, X)));
            yield return Until(() => box.LastComboHits >= 3);
            yield return Shot("09-launch");
            yield return Until(() => box.LastComboHits >= 4 || !box.ScriptRunning);
            yield return Shot("10-juggle");
            yield return Until(() => !box.ScriptRunning && box.State.P2.Mode == RobotMode.Idle);

            // OVERLOAD, then a broken head.
            box.DevSetOverload(2, 960);
            box.QueueP1(Frames(25, (2, X)));
            yield return Until(() => box.State.P2.Mode == RobotMode.Overload);
            yield return Wait(0.4f);
            yield return Shot("11-overload");
            yield return Until(() => !box.ScriptRunning && box.State.P2.Mode == RobotMode.Idle);
            box.DevSetPart(2, Part.Head, 1);
            box.DevSetPart(2, Part.Legs, 60);
            box.QueueP1(Frames(25, (2, X)));
            yield return Until(() => box.State.P2.PartHead == 0);
            yield return Wait(0.15f);
            yield return Shot("12-part-broken");
            yield return Until(() => !box.ScriptRunning);

            yield return Press(new NavFrame { Start = true });
            yield return Shot("13-pause-options");
            yield return Press(new NavFrame { Start = true });

            // Three rounds: KO, round winner, corner break.
            for (int round = 1; round <= 3; round++)
            {
                yield return Until(() => box.State.Phase == MatchPhase.Fight);
                box.DevSetHp(2, 5);
                box.QueueP1(Frames(30, (2, X)));
                yield return Until(() => box.State.Phase == MatchPhase.RoundOver);
                if (round == 1)
                {
                    yield return Wait(0.3f);
                    yield return Shot("14-ko");
                    yield return Until(() => box.State.PhaseFrames < 60);
                    yield return Shot("15-round-winner");
                }
                if (round < 3)
                {
                    yield return Until(() =>
                        box.CurrentStage == GreyBoxController.Stage.CornerBreak
                    );
                    yield return Wait(0.3f);
                    if (round == 1)
                        yield return Shot("16-corner-break");
                    yield return Press(new NavFrame { Submit = true });
                }
            }
            yield return Until(() => box.CurrentStage == GreyBoxController.Stage.Results);
            yield return Wait(0.5f);
            yield return Shot("17-winner");
            yield return Press(new NavFrame { Y = -1 });
            yield return Press(new NavFrame { Y = -1 });
            yield return Press(new NavFrame { Submit = true }); // Main menu
            yield return Until(() =>
                MenuController.Instance != null && MenuController.Instance.Ready
            );
            yield return Wait(0.4f);
            yield return Shot("18-back-to-main");
            Application.Quit();
        }

        static IEnumerator Until(Func<bool> condition)
        {
            float end = Time.realtimeSinceStartup + 30f;
            while (!condition() && Time.realtimeSinceStartup < end)
                yield return null;
        }

        static IEnumerator Wait(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
                yield return null;
        }

        static IEnumerator Press(NavFrame f)
        {
            MenuInput.Instance.Inject(f);
            yield return Wait(0.25f);
        }

        IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(_dir, name + ".png"));
            yield return Wait(0.3f);
        }
    }
}
