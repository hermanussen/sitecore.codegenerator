#Sitecore.CodeGenerator

This project allows you to generate interfaces/classes based on your Sitecore template structure. It works with serialized data from Sitecore (incl. TDS/Unicorn) and uses T4 templates for generating the files. The [T4 Toolbox](http://www.olegsych.com/2012/12/t4-toolbox-for-visual-studio-2012/) is used to generate multiple files.

By default, it supports generating interfaces that are decorated with [Glass Mapper](https://github.com/mikeedwards83/Glass.Mapper) attributes. But the Sitecore.CodeGenerator project only provides easy access to the serialized template information and can therefore easily be used to create T4 templates for other mapping/wrapping frameworks.

## How it works
Sitecore.CodeGenerator reads serialized data from the filesystem. The T4 templates do the rest of the work.
- **GlassGenerator.tt** - The central class that actually uses Sitecore.CodeGenerator.DLL to read the serialized data and then creates the individual generated files.
- **GlassMappedClassTemplate.tt** - Individual template for what the generated files should look like. If you need to customize the code that needs to be generated, this is the first place to look.
- **SampleScriptTemplates.tt** - You can make as many copies of this as you like, for different groups/folders of templates. The generated files will show up as children of this item in the Solution Explorer.

## Adding code generation to your project

1. Install the [T4 Toolbox](http://www.olegsych.com/2012/12/t4-toolbox-for-visual-studio-2012/) Visual Studio extension.
2. Compile Sitecore.CodeGenerator or download it [here](https://github.com/hermanussen/sitecore.codegenerator/archive/master.zip). Copy the DLL to a folder that is within reach from your project (no need to add a reference; a dependency within your project isn't needed).
3. Copy the .tt files and include them in your project. The files are GlassGenerator.tt, GlassMappedClassTemplate.tt and SampleScriptTemplates.tt.
4. For GlassGenerator.tt and GlassMappedClassTemplate.tt, empty the project property "Custom Tool". Also, change the file references at the top of these files to correctly reference the Sitecore.Kernel.DLL, Sitecore.CodeGenerator.DLL and Glass.Mapper.Sc.DLL files.
5. In GlassGenerator.tt, change the following code so the path references your serialized data:```Context.Host.ResolvePath(@"..\Data\serialization\")```. The path is relative to the location of the .tt files.
6. In SampleScriptTemplates.tt, change the following code to set the path(s) of the templates that you want to generate code for: ```new [] { "/sitecore/templates/Sitecore Code Generator Sample" }```.
7. Run the code generation either by right-clicking SampleScriptTemplates.tt and choosing "Run Custom Tool", or choosing "Build" > "Transform All T4 Templates".

## Using Unicorn's Rainbow format

Since Unicorn has adopted a new serialization format from version 3, the code generation needs to be configured slightly different to be used with that.

1. Go to GlassGenerator.tt and locate the RunCore() method
2. Locate the following line within that method:
`
var resolver = new TemplatesResolver(
`
3. Change that line to the following:
`
var resolver = new TemplatesResolverRainbow(
`
4. Ensure that the path to the serialized data (as set in step 5 of the previous section) points to a valid Unicorn configuration that contains Sitecore templates in .yml files. Note that this is on the next line of code.

## Further configuration

It can't always be determined exactly from the Sitecore data what the return types of properties need to be. E.g.: A multilist may be used to select a number of items of a certain type. But you need to define that type yourself.

By default, the mapping of field types to C# types is done by GlassGenerator.tt in the ```GetFieldOptions(...)``` function. You can change it there.

But for individual scenario's like the one described earlier (the multilist that maps to items of a certain type), you can use a specific technique in the file SampleScriptTemplates.tt. Simply put, you make exceptions for individual cases, by checking a field ID and then setting the field options. E.g.:
```
// Add custom options, like setting the return type of a field property explicitly

// Dog -> Food
if("{1033D7C1-9C1A-4C65-8316-81B6D5E46EB5}".Equals(fieldId))
{
	fieldOptions.GlassFieldTypeName = "IEnumerable<IFood>";
}
```

## License
Copyright (C) 2015-2016 Robin Hermanussen

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see http://www.gnu.org/licenses/.
