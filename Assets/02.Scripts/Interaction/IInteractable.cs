
using Fusion;

namespace BumblingKitchen.Interaction
{
	public interface IInteractable
	{
		NetworkId NetworkId { get; }
		InteractionType Type { get; }
		bool TryInteraction(Interactor interactor, IInteractable interactable);
	}
}
