namespace Bergmann.Client.InputHandlers;

/// <summary>
/// A standard interface for handling input. 
/// </summary>
public interface IInputHandler {
    /// <summary>
    /// Handles the input for this. 
    /// </summary>
    /// <param name="updateArgs">The update args for this update. It provides all data necessary 
    /// to perform such an update.</param>
    public void HandleInput(InputUpdateArgs updateArgs);
}