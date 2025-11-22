using UnityEngine;
using Duckov.Modding;
using System.Linq;

namespace Stringification.Components
{
    public class PlayerManager
    {
        public GameObject? PlayerObject { get; private set; }
        public CharacterMainControl? PlayerControl { get; private set; }
        public Transform? TargetModel { get; private set; }
        public Transform? DamageReceiver { get; private set; }
        public Rigidbody? PlayerRigidbody { get; private set; }

        private float nextPlayerSearchTime = 0f;

        public void Reset()
        {
            PlayerObject = null;
            PlayerControl = null;
            TargetModel = null;
            DamageReceiver = null;
            PlayerRigidbody = null;
        }

        public bool UpdatePlayerReference()
        {
            if (PlayerObject != null && TargetModel != null && DamageReceiver != null) return true;
            if (Time.time < nextPlayerSearchTime) return false;

            // Find Player
            PlayerObject = null;
            PlayerControl = null;
            var characterControls = Object.FindObjectsOfType<CharacterMainControl>();
            foreach (var cc in characterControls)
            {
                if (IsValidPlayer(cc))
                {
                    PlayerObject = cc.gameObject;
                    PlayerControl = cc;
                    break;
                }
            }

            // Find Model
            if (PlayerObject != null)
            {
                PlayerRigidbody = PlayerObject.GetComponent<Rigidbody>();
                
                Transform modelRoot = PlayerObject.transform.Find("ModelRoot");
                if (modelRoot != null)
                {
                    // 查找 ModelRoot 下的实际网格容器（跳过 HiderPoints）
                    foreach (Transform child in modelRoot)
                    {
                        if (!child.name.Contains("HiderPoints"))
                        {
                            TargetModel = child;
                            break;
                        }
                    }
                }

                // Find DamageReceiver
                DamageReceiver = PlayerObject.transform.Find("DamageReceiver");

                if (TargetModel != null)
                {
                    Debug.Log($"Stringification: Found Target '{TargetModel.name}' on Player '{PlayerObject.name}'");
                    if (DamageReceiver != null) Debug.Log($"Stringification: Found DamageReceiver on Player '{PlayerObject.name}'");
                    return true;
                }
            }
            
            nextPlayerSearchTime = Time.time + 1.0f;
            return false;
        }

        private bool IsValidPlayer(CharacterMainControl cc)
        {
            // 通过检查 AI 相关的子对象来识别 NPC
            var aiChild = cc.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.gameObject.name.Contains("AIController"));
            if (aiChild != null)
            {
                Debug.Log($"Stringification: Excluded '{cc.name}' due to AI child: {aiChild.gameObject.name}");
                return false;
            }
            return true;
        }
    }
}
