using Fusion;

namespace BumblingKitchen.Interaction
{
	public class TestInteraction : NetworkBehaviour, IInteractable
	{
		public InteractionType Type => InteractionType.None;

		public bool CanInteraction(InteractionType type)
		{
			throw new System.NotImplementedException();
		}

		public bool CanInteraction(Interactor interactor)
		{
			throw new System.NotImplementedException();
		}

		public void Interaction(Interactor interactor, IInteractable interactionObject)
		{
			throw new System.NotImplementedException();
		}


	}
}
