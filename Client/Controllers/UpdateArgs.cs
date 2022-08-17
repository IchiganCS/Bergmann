using Bergmann.Client.InputHandlers;

namespace Bergmann.Client.Controllers;

/// <summary>
/// Used for updating any controller. Since a controller usually needs to handle user input, it is a subclass of
/// <see cref="InputUpdateArgs"/>.
/// </summary>
public class UpdateArgs : InputUpdateArgs {
    public UpdateArgs(float deltaTime) : base(deltaTime) {
        
    }
}