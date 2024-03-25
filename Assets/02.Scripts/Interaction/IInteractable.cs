
namespace BumblingKitchen.Interaction
{
	public interface IInteractable
	{
		InteractionType Type { get; }
		bool TryInteraction(Interactor interactor, IInteractable interactable);
	}
}
