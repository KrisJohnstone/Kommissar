namespace Kommissar.Model
{
    public class AppSettings
    {
        public string[] NamespacesPrefix { get; set; }
        public string ArtifactoryCredential { get; set; }
        public string DatabaseConnection { get; set; }
        public string ArtifactoryBaseUri { get; set; }
    }
}