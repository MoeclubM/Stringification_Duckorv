using UnityEngine;
using System.Collections;

namespace Stringification.Components
{
    public class JumpMechanics
    {
        public float SingleJumpHeight { get; set; } = 3.0f;
        public float DoubleJumpHeight { get; set; } = 2.0f;
        public float Gravity { get; set; } = 40.0f;
        public float MinJumpTime { get; set; } = 0.15f;

        private Coroutine? currentJumpCoroutine;

        public void PerformSingleJump(MonoBehaviour runner, GameObject? player, Rigidbody? rb)
        {
            if (player == null || rb == null) return;
            
            if (currentJumpCoroutine != null) runner.StopCoroutine(currentJumpCoroutine);
            currentJumpCoroutine = runner.StartCoroutine(SimulatePhysicsJump(player, rb, SingleJumpHeight));
            
            Debug.Log("Stringification: Single Jump (Simulated Physics)!");
        }

        public void PerformDoubleJump(MonoBehaviour runner, GameObject? player, Rigidbody? rb)
        {
            if (player == null || rb == null) return;

            if (currentJumpCoroutine != null) runner.StopCoroutine(currentJumpCoroutine);
            currentJumpCoroutine = runner.StartCoroutine(SimulatePhysicsJump(player, rb, DoubleJumpHeight));
            
            Debug.Log("Stringification: Double Jump (Simulated Physics)!");
        }

        public void StopJump(MonoBehaviour runner)
        {
            if (currentJumpCoroutine != null)
            {
                runner.StopCoroutine(currentJumpCoroutine);
                currentJumpCoroutine = null;
            }
        }

        private IEnumerator SimulatePhysicsJump(GameObject player, Rigidbody rb, float height)
        {

            // v = sqrt(2 * g * h)
            float jumpVelocity = Mathf.Sqrt(2 * Gravity * height);
            
            // 跟踪绝对 Y 轴以防止游戏将我们拉下
            float currentY = player.transform.position.y + 0.25f;
            player.transform.position = new Vector3(player.transform.position.x, currentY, player.transform.position.z);

            float timeStep = 0f;
            float minJumpTime = MinJumpTime; // 增加以确保我们不会立即着陆
            
            Debug.Log($"Stringification: Jump Started. Height={height}, Velocity={jumpVelocity}");

            while (true)
            {
                if (player == null) break;

                // 使用 WaitForEndOfFrame 来覆盖 Update/FixedUpdate 中发生的任何 Kinematic/CharacterController 捕捉
                yield return new WaitForEndOfFrame();
                float dt = Time.deltaTime;
                
                jumpVelocity -= Gravity * dt;
                currentY += jumpVelocity * dt;
                
                // 强制绝对 Y 位置
                player.transform.position = new Vector3(player.transform.position.x, currentY, player.transform.position.z);

                // 落地检查
                // 仅在下落且超过最短时间时检查
                if (jumpVelocity < 0 && timeStep > minJumpTime)
                {
                    // 使用 PhysicsUtils 进行鲁棒的地面检测
                    if (Stringification.Utils.PhysicsUtils.IsGrounded(player, 0.6f, true, true))
                    {
                        Debug.Log("Stringification: Jump Landed.");
                        break;
                    }
                }

                timeStep += dt;
                if (timeStep > 2.0f) break;
            }
            
            currentJumpCoroutine = null;
        }
    }
}
