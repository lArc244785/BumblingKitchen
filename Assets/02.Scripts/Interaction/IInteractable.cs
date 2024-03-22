
namespace BumblingKitchen.Interaction
{
	public interface IInteractable
	{
		InteractionType Type { get; }
		bool CanInteraction(InteractionType type);
		bool CanInteraction(Interactor interactor);
		void Interaction(Interactor interactor, IInteractable interactionObject);
	}
}
