using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleBuffs {

    public interface IBuffExecutionEngine {
        public IEnumerator CreateSpellCastRoutine(List<CastTask> tasks);
    }
    public class BubbleBuffGlobalController : MonoBehaviour {
        public static void Init() {
            Instance = new();
        }
        public static BubbleBuffGlobalController Instance { get; private set; }

        public const int BATCH_SIZE = 8;
        public const float DELAY = 0.05f;

        private void Awake() {
            Instance = this;
        }

        public void Destroy() {
        }

        public void CastSpells(List<CastTask> tasks) {
            var castingCoroutine = Engine.CreateSpellCastRoutine(tasks);
            StartCoroutine(castingCoroutine);
        }

        public static IBuffExecutionEngine Engine =>
            GlobalBubbleBuffer.Instance.SpellbookController.state.VerboseCasting
                ? new AnimatedExecutionEngine()
                : new InstantExecutionEngine();
    }
}
