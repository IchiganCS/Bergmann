using Bergmann.Client.InputHandlers;

namespace Bergmann.Client.Controllers;

public class UpdateArgs : InputUpdateArgs {
    public UpdateArgs(float deltaTime) : base(deltaTime) {
        
    }
}