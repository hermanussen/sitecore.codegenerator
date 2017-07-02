namespace Sitecore.Codegenerator.Scripty
{
    using System.Collections.Generic;
    using System.Linq;
    using CodeGenerator;
    using CodeGenerator.Domain;
    using Microsoft.CodeAnalysis;
    using global::Scripty.Core.ProjectTree;

    public static class SerializedDataUtil
    {
        public static List<TemplateItem> GetTemplates(ProjectRoot project)
        {
            var resolver = new TemplatesResolverRainbow(project.Analysis.AdditionalDocuments.Select(d => d.FilePath).ToList());
            return resolver.Templates;
        }
    }
}