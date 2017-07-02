// /*
// Copyright (C) 2016 Robin Hermanussen
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program. If not, see http://www.gnu.org/licenses/.
// */
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Sitecore.CodeGenerator.Domain;

namespace Sitecore.CodeGenerator.Tests
{
    using System.IO;

    public class TemplatesResolverRainbowTest
    {
        [Test]
        public void ShouldResolveTemplates()
        {
            var resolver = new TemplatesResolverRainbow(
                @"..\..\..\Sitecore.CodeGenerator.Sample.Glass\Data\Unicorn", new[] { "/sitecore/templates" });
            AssertTemplates(resolver);
        }

        [Test]
        public void ShouldResolveTemplatesFromProject()
        {
            DirectoryInfo serializationFolder = new DirectoryInfo(@"..\..\..\Sitecore.CodeGenerator.Scripty.Sample.Glass\Data\Unicorn");
            serializationFolder.Exists.Should().BeTrue();
            var files = serializationFolder.GetFiles("*.yml", SearchOption.AllDirectories);
            files.Length.ShouldBeEquivalentTo(14);
            var resolver = new TemplatesResolverRainbow(files.Select(f => f.FullName).ToList());
            AssertTemplates(resolver);
        }
        
        private static void AssertTemplates(TemplatesResolverRainbow resolver)
        {
            resolver.Templates.Select(t => t.SyncItem.Name).ShouldAllBeEquivalentTo(new[]
            {
                "Animal",
                "Dog",
                "Food",
                "Nameable"
            });

            TemplateItem dogTemplate = resolver.Templates.FirstOrDefault(t => t.SyncItem.Name == "Dog");
            dogTemplate.Should().NotBeNull();
            dogTemplate.BaseTemplates.Select(b => b.SyncItem.Name).ShouldAllBeEquivalentTo(new[]
            {
                "Animal",
                "Nameable"
            });

            TemplateSection dogSection = dogTemplate.Sections.FirstOrDefault(s => s.SyncItem.Name == "Dog");
            dogSection.Should().NotBeNull();

            dogSection.Fields.Select(f => f.SyncItem.Name).ShouldAllBeEquivalentTo(new[]
            {
                "Eats",
                "Friends"
            });
            TemplateField eatsField = dogSection.Fields.FirstOrDefault(f => f.SyncItem.Name == "Eats");
            eatsField.Should().NotBeNull();
            eatsField.FieldTitle.Should().BeEquivalentTo("What food does the dog eat?");
            eatsField.FieldTypeName.Should().BeEquivalentTo("Multilist with Search");
        }
    }
}
