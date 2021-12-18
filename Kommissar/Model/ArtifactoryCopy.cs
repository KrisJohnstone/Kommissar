namespace Kommissar.Model;

public class ArtifactoryCopy
{
    /// <summary>
    /// The Target Repo that we are copying or moving too.
    /// </summary>
    public string targetRepo { get; set; }
    /// <summary>
    /// Docker Repo to promote. Clear as fucking mud but this is actually the container name -_-
    /// </summary>
    public string dockerRepository { get; set; }
    /// <summary>
    /// Current image tag
    /// </summary>
    public string tag { get; set; }
    /// <summary>
    /// The new tag
    /// </summary>
    public string targetTag { get; set; }
    /// <summary>
    /// Whether to move or copy. Set to true for copy
    /// </summary>
    public bool copy { get; set; }
}