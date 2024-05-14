namespace Zetworking;

/// <summary>
/// Specifies the state of a <see cref="ZetNode"/>.
/// </summary>
public enum ZetNodeState
{
    /// <summary>
    /// The <see cref="ZetNode"/> is stopped.
    /// </summary>
    Stopped,

    /// <summary>
    /// The <see cref="ZetNode"/> is starting up.
    /// </summary>
    Starting,

    /// <summary>
    /// The <see cref="ZetNode"/> is running.
    /// </summary>
    Running,

    /// <summary>
    /// The <see cref="ZetNode"/> is stopping.
    /// </summary>
    Stopping,
}
