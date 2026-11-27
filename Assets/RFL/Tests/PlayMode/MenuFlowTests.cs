#nullable enable
using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using RFL.Game.Core;
using RFL.Game.GreyBox;
using RFL.Game.UI;
using RFL.Game.UI.Screens;
using RFL.Sim;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RFL.Tests
{
    /// <summary>
    /// Drives the real menu with virtual devices: controller only (two pads, PvP into the fight scene,
    /// pause, back to the main menu) and keyboard only (settings, language switch, back).
    /// </summary>
    public class MenuFlowTests : InputTestFixture
    {
        string _profilePath = null!;

        [UnitySetUp]
        public IEnumerator UseTemporaryProfile()
        {
            _profilePath = Path.Combine(
                Path.GetTempPath(),
                "rfl-menu-" + Guid.NewGuid().ToString("N") + ".json"
            );
            GameServices.UseProfilePath(_profilePath);
            MatchRequest.ReturnTo = ReturnTarget.Title;
            MatchRequest.LastP1Index = 0; // robot select remembers the last pick; each test starts fresh
            MatchRequest.LastP2Index = 0;
            MatchRequest.Dummy = DummyAction.Stand;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator DeleteTemporaryProfile()
        {
            if (File.Exists(_profilePath))
                File.Delete(_profilePath);
            yield return null;
        }

        static MenuScreen? Top =>
            MenuController.Instance != null ? MenuController.Instance.Top : null;

        /// <summary>Asserts the top screen's type and returns it typed.</summary>
        static T As<T>(MenuScreen? screen)
            where T : MenuScreen
        {
            Assert.IsInstanceOf<T>(screen);
            return (T)screen!;
        }

        static IEnumerator WaitFor(Func<bool> condition, string what, float seconds = 15f)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (!condition())
            {
                if (Time.realtimeSinceStartup > end)
                    Assert.Fail("Timed out waiting for " + what);
                yield return null;
            }
        }

        IEnumerator Tap(ButtonControl button)
        {
            Press(button, queueEventOnly: true);
            yield return null;
            Release(button, queueEventOnly: true);
            yield return null;
        }

        static IEnumerator LoadMenu()
        {
            var previous = MenuController.Instance;
            SceneManager.LoadScene("Menu");
            yield return WaitFor(
                () =>
                    MenuController.Instance != null
                    && MenuController.Instance != previous
                    && MenuController.Instance.Ready,
                "new menu ready"
            );
            yield return null;
        }

        [UnityTest]
        public IEnumerator ControllerOnly_PvpToFightPauseAndBack()
        {
            var pad1 = InputSystem.AddDevice<Gamepad>();
            var pad2 = InputSystem.AddDevice<Gamepad>();
            yield return LoadMenu();
            Assert.IsInstanceOf<TitleScreen>(Top);

            yield return Tap(pad1.buttonSouth);
            Assert.IsInstanceOf<MainMenuScreen>(Top);
            Assert.AreSame(pad1, MatchRequest.P1Device);

            yield return Tap(pad1.buttonSouth); // Versus
            Assert.IsInstanceOf<VersusScreen>(Top);
            yield return Tap(pad1.buttonSouth); // Player vs Player
            var select = As<RobotSelectScreen>(Top);
            Assert.IsNotNull(select);
            Assert.IsFalse(select.P2Joined);

            yield return Tap(pad2.dpad.right); // P2 has not joined: ignored
            yield return Tap(pad1.dpad.right); // P1 → POWER
            yield return Tap(pad1.buttonSouth); // P1 ready
            Assert.IsTrue(select.P1Ready);
            yield return Tap(pad2.buttonSouth); // P2 joins
            Assert.IsTrue(select.P2Joined);
            yield return Tap(pad2.dpad.left); // P2 → SPEED (wraps)
            yield return Tap(pad2.buttonSouth); // P2 ready

            yield return WaitFor(
                () => SceneManager.GetActiveScene().name == "GreyBox",
                "fight scene"
            );
            Assert.AreEqual(MatchMode.VersusPlayer, MatchRequest.Config.Mode);
            Assert.AreEqual(StarterType.Power, MatchRequest.Config.P1);
            Assert.AreEqual(StarterType.Speed, MatchRequest.Config.P2);
            Assert.AreSame(pad2, MatchRequest.P2Device);

            GreyBoxController box = null!; // set by the WaitFor below
            yield return WaitFor(
                () => (box = UnityEngine.Object.FindAnyObjectByType<GreyBoxController>()) != null,
                "grey box"
            );
            yield return null;
            Assert.AreEqual(
                GreyBoxController.Stage.VsIntro,
                box.CurrentStage,
                "the challenger is declared first"
            );
            yield return Tap(pad2.startButton); // skips the VS intro
            Assert.AreEqual(GreyBoxController.Stage.Fight, box.CurrentStage);
            yield return Tap(pad2.startButton);
            Assert.IsTrue(box.Paused);
            for (int i = 0; i < 4; i++)
                yield return Tap(pad1.dpad.down); // resume, frame data, restart, robot select, main menu
            yield return Tap(pad1.buttonSouth);

            yield return WaitFor(() => SceneManager.GetActiveScene().name == "Menu", "menu scene");
            yield return WaitFor(
                () => MenuController.Instance != null && MenuController.Instance.Ready,
                "menu ready"
            );
            Assert.IsInstanceOf<MainMenuScreen>(Top);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator ControllerOnly_PauseToRobotSelectKeepsTheMode()
        {
            var pad = InputSystem.AddDevice<Gamepad>();
            yield return LoadMenu();
            yield return Tap(pad.buttonSouth); // title
            yield return Tap(pad.buttonSouth); // Versus
            yield return Tap(pad.dpad.down);
            yield return Tap(pad.buttonSouth); // Player vs CPU
            Assert.IsInstanceOf<DifficultyScreen>(Top);
            yield return Tap(pad.dpad.down); // Hard
            yield return Tap(pad.buttonSouth);
            yield return Tap(pad.buttonSouth); // P1 ready
            yield return Tap(pad.buttonSouth); // CPU robot ready
            yield return WaitFor(
                () => SceneManager.GetActiveScene().name == "GreyBox",
                "fight scene"
            );
            Assert.AreEqual(CpuLevel.Hard, MatchRequest.Config.Cpu);
            yield return null;
            yield return null;
            yield return Tap(pad.buttonSouth); // skip the VS intro
            yield return Tap(pad.startButton);
            for (int i = 0; i < 3; i++)
                yield return Tap(pad.dpad.down); // resume, frame data, restart, robot select
            yield return Tap(pad.buttonSouth);
            yield return WaitFor(
                () => MenuController.Instance != null && MenuController.Instance.Ready,
                "menu ready"
            );
            var select = As<RobotSelectScreen>(Top);
            Assert.IsNotNull(select);
            Assert.AreEqual(MatchMode.VersusCpu, select.Mode);
            yield return Tap(pad.buttonEast); // back to difficulty
            Assert.IsInstanceOf<DifficultyScreen>(Top);
        }

        [UnityTest]
        public IEnumerator Training_GamepadWalksInAndJabConnects()
        {
            var pad = InputSystem.AddDevice<Gamepad>();
            yield return LoadMenu();
            yield return Tap(pad.buttonSouth); // title
            yield return Tap(pad.dpad.down);
            yield return Tap(pad.buttonSouth); // Training
            Assert.IsInstanceOf<RobotSelectScreen>(Top);
            yield return Tap(pad.buttonSouth); // player robot
            yield return Tap(pad.buttonSouth); // dummy robot
            GreyBoxController box = null!; // set by the WaitFor below
            yield return WaitFor(
                () => (box = UnityEngine.Object.FindAnyObjectByType<GreyBoxController>()) != null,
                "grey box"
            );
            yield return null;
            yield return Tap(pad.buttonSouth); // skip the VS intro
            yield return WaitFor(() => box.State.Phase == MatchPhase.Fight, "ROUND 1 — FIGHT!");

            // Walk in for half a second, then tap X.
            Press(pad.dpad.right, queueEventOnly: true);
            float until = Time.realtimeSinceStartup + 0.5f;
            while (Time.realtimeSinceStartup < until)
                yield return null;
            Release(pad.dpad.right, queueEventOnly: true);
            yield return null;
            Assert.Less(
                box.State.P2.X.ToMilli() - box.State.P1.X.ToMilli(),
                2000,
                "P1 walked forward"
            );

            yield return Tap(pad.buttonWest);
            yield return WaitFor(() => box.TotalHits >= 1, "jab hit", 3f);
            Assert.AreEqual(1, box.LastComboHits);
        }

        IEnumerator WalkInAndJab(Gamepad pad, GreyBoxController box)
        {
            Press(pad.dpad.right, queueEventOnly: true);
            float until = Time.realtimeSinceStartup + 0.6f;
            while (Time.realtimeSinceStartup < until)
                yield return null;
            Release(pad.dpad.right, queueEventOnly: true);
            yield return null;
            yield return Tap(pad.buttonWest);
        }

        [UnityTest]
        public IEnumerator Training_FullMatch_WinnerIsDeclaredAndMenusWork()
        {
            var pad = InputSystem.AddDevice<Gamepad>();
            yield return LoadMenu();
            yield return Tap(pad.buttonSouth); // title
            yield return Tap(pad.dpad.down);
            yield return Tap(pad.buttonSouth); // Training
            yield return Tap(pad.buttonSouth); // P1 STANDARD
            yield return Tap(pad.dpad.right); // challenger POWER
            yield return Tap(pad.buttonSouth);
            GreyBoxController box = null!; // set by the WaitFor below
            yield return WaitFor(
                () => (box = UnityEngine.Object.FindAnyObjectByType<GreyBoxController>()) != null,
                "grey box"
            );
            Assert.AreEqual(StarterType.Standard, MatchRequest.Config.P1);
            Assert.AreEqual(StarterType.Power, MatchRequest.Config.P2);
            yield return null;
            yield return Tap(pad.buttonSouth); // skip the VS intro

            for (int round = 1; round <= 3; round++)
            {
                yield return WaitFor(
                    () => box.State.Phase == MatchPhase.Fight,
                    $"round {round} starts",
                    10f
                );
                Assert.AreEqual(round, box.State.Round);
                box.DevSetHp(2, 5);
                yield return WalkInAndJab(pad, box);
                yield return WaitFor(
                    () => box.State.Phase == MatchPhase.RoundOver,
                    $"round {round} KO",
                    5f
                );
                Assert.AreEqual((byte)1, box.State.RoundWinner);
                if (round < 3)
                {
                    yield return WaitFor(
                        () => box.CurrentStage == GreyBoxController.Stage.CornerBreak,
                        "corner break",
                        8f
                    );
                    yield return Tap(pad.buttonSouth); // repair the most damaged part
                    Assert.AreEqual(GreyBoxController.Stage.Fight, box.CurrentStage);
                }
            }
            yield return WaitFor(
                () => box.CurrentStage == GreyBoxController.Stage.Results,
                "results",
                8f
            );
            Assert.AreEqual((byte)1, box.State.MatchWinner);
            Assert.AreEqual(3, box.State.WinsP1);

            yield return Tap(pad.dpad.down);
            yield return Tap(pad.dpad.down);
            yield return Tap(pad.buttonSouth); // Main menu
            yield return WaitFor(
                () =>
                    SceneManager.GetActiveScene().name == "Menu"
                    && MenuController.Instance != null
                    && MenuController.Instance.Ready,
                "main menu"
            );
            Assert.IsInstanceOf<MainMenuScreen>(Top);
        }

        [UnityTest]
        public IEnumerator KeyboardOnly_SettingsLanguageSwitchAndSave()
        {
            var kb = InputSystem.AddDevice<Keyboard>();
            yield return LoadMenu();
            yield return Tap(kb.enterKey);
            Assert.IsInstanceOf<MainMenuScreen>(Top);
            yield return Tap(kb.downArrowKey);
            yield return Tap(kb.downArrowKey);
            yield return Tap(kb.enterKey);
            var settings = As<SettingsScreen>(Top);
            Assert.IsNotNull(settings);

            string before = Loc.Code;
            yield return Tap(kb.rightArrowKey); // language
            yield return null;
            Assert.AreNotEqual(before, Loc.Code);
            Assert.AreEqual(
                Loc.Code == "id" ? "Pengaturan" : "Settings",
                Loc.Get("ui.settings.title")
            );

            yield return Tap(kb.eKey); // Audio tab
            Assert.AreEqual(RFL.Game.Profile.SettingsTab.Audio, settings.Tab);
            yield return Tap(kb.leftArrowKey); // master 8 → 7
            Assert.AreEqual(7, GameServices.Settings.S.master);

            yield return Tap(kb.escapeKey);
            Assert.IsInstanceOf<MainMenuScreen>(Top);
            Assert.IsTrue(File.Exists(_profilePath), "leaving settings saves the profile");
            var saved = RFL.Game.Profile.ProfileStore.Load(_profilePath);
            Assert.AreEqual(7, saved.settings.master);
            Assert.AreEqual(Loc.Code, saved.settings.language);

            yield return Tap(kb.escapeKey); // quit confirm
            Assert.IsInstanceOf<ConfirmDialog>(Top);
            yield return Tap(kb.enterKey); // default is No
            Assert.IsInstanceOf<MainMenuScreen>(Top);
        }
    }
}
