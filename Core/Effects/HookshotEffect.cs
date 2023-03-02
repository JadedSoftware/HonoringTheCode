using System.Collections;
using Core.GameManagement.Interfaces;
using UnityEngine;

namespace Core.Effects
{
	public class HookshotEffect : EffectsCommon
	{
		public ParticleSystem highlightedEffect;
		public ParticleSystem activeEffect;
		public IHookshotable hookShotObject;

		public void Init(IHookshotable hookShotTarget)
		{
			hookShotObject = hookShotTarget;
			hookShotTarget.hookshotEffect = this;
			switch (hookShotTarget)
			{
				case NavPoint navPoint:
					SetPosition(navPoint.GetPosition() + (navPoint.hit.normal * .25f));
					break;
				case UnitCommon unitCommon:
					SetPosition(unitCommon.motor.Capsule.ClosestPointOnBounds(CameraController.instance.mainCamera.transform.position));
					break;
			}
		}
		private IEnumerator FaceMainCamera()
		{
			while (activeEffect.isPlaying)
			{
				transform.LookAt(CameraController.instance.mainCamera.transform.position);
				yield return new WaitForSeconds(.5f);
			}
		}

		public void EngageEffect()
		{
			highlightedEffect.Stop();
			activeEffect.Play();
			transform.LookAt(CameraController.instance.mainCamera.transform.position);
			if (hookShotObject is UnitCommon unitCommon)
			{
				SetPosition(unitCommon.motor.Capsule.ClosestPointOnBounds(CameraController.instance.mainCamera.transform.position));
			}
			StartCoroutine(FaceMainCamera());
		}

		public void DisengageEffect()
		{
			highlightedEffect.Stop();
			activeEffect.Stop();
			StopAllCoroutines();
		}

		public void EngageHighlight()
		{
			if(!highlightedEffect.isPlaying)
				highlightedEffect.Play();
			StartCoroutine(EnsureMouseOver());
		}

		private IEnumerator EnsureMouseOver()
		{
			yield return new WaitForSeconds(.5f);
		}

		public void DisengageHighlight()
		{
			highlightedEffect.Stop();
		}
	}
}
