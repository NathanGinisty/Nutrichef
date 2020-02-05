using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractive
{
    void Interact(PlayerController pController);
    void StopInteraction();

    void Begin();
    void End();

	bool CanInteract(PlayerController pController);

    void CancelInteraction();
}
