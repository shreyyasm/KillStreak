using UnityEngine;

namespace StarterAssets
{
    public class UICanvasControllerInput : MonoBehaviour
    {

        [Header("Output")]
        public StarterAssetsInputs starterAssetsInputs;

        public ThirdPersonController thirdPersonController;
        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            starterAssetsInputs.MoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            starterAssetsInputs.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            starterAssetsInputs.JumpInput(virtualJumpState);
            //if(thirdPersonController.Grounded)
            //    thirdPersonController.isPressedJump = true;
        }
        public void VirtualSlideInput(bool virtualJumpState)
        {
            starterAssetsInputs.SlideInput(virtualJumpState);
            //if(thirdPersonController.Grounded)
            //    thirdPersonController.isPressedJump = true;
        }
        public void VirtualSprintInput(bool virtualSprintState)
        {
            starterAssetsInputs.SprintInput(virtualSprintState);
        }
        public void VirtualFireInput(bool virtualFireState)
        {
            starterAssetsInputs.ShootInput(virtualFireState);
        }

        public void VirtualAimInput(bool virtualAimState)
        {
            starterAssetsInputs.AimInput(virtualAimState);
        }
        public void VirtualWeaponChangeInput(bool virtualWeaponChangeState)
        {
            starterAssetsInputs.GunChangeInput(virtualWeaponChangeState);
        }
        public void VirtualReloadInput(bool virtualReloadState)
        {
            starterAssetsInputs.ReloadInput(virtualReloadState);
        }

    }

}
