// CodeDOM parts from: https://msdn.microsoft.com/en-us/library/ms404245(v=vs.110).aspx

using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Framework
{
    public class DOMGenerator
    {
        public static Dictionary<Type, Type> WrapperTypes = new Dictionary<Type, Type>();
        public static Dictionary<Func<bool>, string> CanExecutedProperties = new Dictionary<Func<bool>, string>();

        private CodeNamespace targetNamespace;
        private CodeTypeDeclaration targetClass;
        private Type sourceType;
        private List<string> neededAssemblies;
        private List<Type> processedTypes;


        public static Type GetWrapperType(Type dataType, CodeNamespace targetNamespace = null)
        {
            DOMGenerator generator = new DOMGenerator();
            generator.GeneratedTypeAndChildren(dataType);
            return WrapperTypes[dataType];
        }

        public static object ConvertToWrapperType(object source)
        {
            Type sourceType = source.GetType();

            if (sourceType.IsGenericType)
            {
                var typeName = sourceType.FullName.Split('[')[0];
                Type genericBaseType = Type.GetType(typeName);
                Type[] genericTypes = new Type[sourceType.GenericTypeArguments.Length];

                int i = 0;

                foreach (var item in sourceType.GenericTypeArguments)
                {
                    if (Generator.IsSimple(item))
                    {
                        genericTypes[i] = item;
                    }
                    else
                    {
                        var wrapperType = GetWrapperType(item);
                        genericTypes[i] = wrapperType;
                    }

                    i++;
                }

                var newGenericType = genericBaseType.MakeGenericType(genericTypes);

                if (source is IEnumerable)
                {
                    sourceType = typeof(IEnumerable);
                }

                return Generator.GeneratorConfiguration.WrapperConverter[sourceType](source, newGenericType);
            }
            else
            {
                var wrapperType = Generator.GetWrapper(source);
                return Generator.GetWrapper(source);
            }
        }

        private static void GenerateCSharpCode(CodeCompileUnit generateFrom, string outputFileName)
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            using (StreamWriter sourceWriter = new StreamWriter(outputFileName + ".cs"))
            {
                provider.GenerateCodeFromCompileUnit(
                    generateFrom, sourceWriter, options);
            }
        }

        public static bool CompileCSharpCode(string fileName, List<string> neededAssemblies)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();

            // Build the parameters for source compilation.
            CompilerParameters cp = new CompilerParameters();

            if (neededAssemblies != null)
            {
                foreach (var item in neededAssemblies)
                {
                    cp.ReferencedAssemblies.Add(item);
                }
            }

            // Set the assembly file name to generate.
            cp.OutputAssembly = fileName + ".dll";

            // Save the assembly as a physical file.
            cp.GenerateInMemory = false;

            cp.GenerateExecutable = false;

            // Invoke compilation.
            CompilerResults cr = provider.CompileAssemblyFromFile(cp, fileName + ".cs");

            if (cr.Errors.Count > 0)
            {
                // Display compilation errors.
                foreach (CompilerError ce in cr.Errors)
                {
                    MessageBox.Show(ce.ToString());
                }
            }

            // Return the results of compilation.
            if (cr.Errors.Count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void GeneratedTypeAndChildren(Type source)
        {
            if (!WrapperTypes.ContainsKey(source))
            {
                processedTypes = new List<Type>();
                string fileName = source.FullName + "Wrapper";
                var targetUnit = new CodeCompileUnit();
                targetNamespace = new CodeNamespace("GeneratedArtefacts");
                targetUnit.Namespaces.Add(targetNamespace);

                neededAssemblies = new List<string>();
                neededAssemblies.Add("System.dll");
                neededAssemblies.Add("System.ComponentModel.dll");
                neededAssemblies.Add("Framework.dll");

                targetNamespace.Imports.Add(new CodeNamespaceImport("System"));
                targetNamespace.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));

                GenerateType(source);

                DOMGenerator.GenerateCSharpCode(targetUnit, fileName);
                DOMGenerator.CompileCSharpCode(fileName, neededAssemblies);

                Assembly typeAssembly = Assembly.LoadFrom(fileName + ".dll");
                var wrapperTypes = typeAssembly.GetTypes();

                for (int i = 0; i < wrapperTypes.Count(); i++)
                {
                    WrapperTypes.Add(processedTypes[i], wrapperTypes[i]);
                    Generator.CreateDataTemplateEntries(processedTypes[i], wrapperTypes[i]);
                }
            }
        }

        private CodeTypeReference GetCodeTypeReference(Type childrenType)
        {
            if (WrapperTypes.ContainsKey(childrenType))
            {
                var wrapperType = WrapperTypes[childrenType];
                var assemblyName = wrapperType.Assembly.ManifestModule.Name;

                if (!neededAssemblies.Contains(assemblyName))
                {
                    neededAssemblies.Add(assemblyName);
                }

                return new CodeTypeReference(WrapperTypes[childrenType]);
            }
            else
            {
                var safePreviousTargetClass = targetClass;
                var safeSourceType = sourceType;
                GenerateType(childrenType);
                targetClass = safePreviousTargetClass;
                sourceType = safeSourceType;
                return new CodeTypeReference(childrenType.Name + "Wrapper");
            }
        }

        private void GenerateType(Type source)
        {
            if (processedTypes.Contains(source))
            {
                return;
            }

            processedTypes.Add(source);

            sourceType = source;
            var typeName = sourceType.Name;

            var assemblyName = sourceType.Assembly.ManifestModule.Name;

            if (!neededAssemblies.Contains(assemblyName))
            {
                neededAssemblies.Add(assemblyName);
            }

            targetClass = new CodeTypeDeclaration(typeName + "Wrapper");
            targetClass.IsClass = true;
            targetClass.TypeAttributes =
                TypeAttributes.Public | TypeAttributes.Sealed;
            targetNamespace.Types.Add(targetClass);

            var notifyInterface = new CodeTypeReference("INotifyPropertyChanged");
            targetClass.BaseTypes.Add(notifyInterface);

            AddFields();
            AddProperties();
            AddConstructors();
            AddMethods();
        }

        public void AddFields()
        {
            CodeMemberEvent notifyEvent = new CodeMemberEvent();
            notifyEvent.Attributes = MemberAttributes.Public;
            notifyEvent.Name = "PropertyChanged";
            notifyEvent.Type = new CodeTypeReference(typeof(PropertyChangedEventHandler));
            targetClass.Members.Add(notifyEvent);

            CodeMemberField objectReference = new CodeMemberField();
            objectReference.Attributes = MemberAttributes.Private;
            objectReference.Name = "objectInstance";
            objectReference.Type = new CodeTypeReference(sourceType);
            targetClass.Members.Add(objectReference);
        }

        private void AddProperties()
        {
            foreach (var item in sourceType.GetProperties())
            {
                AddProperty(item.Name, item.PropertyType, item.CanWrite, item.CanRead);
            }

            foreach (var item in sourceType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                AddProperty(item.Name, item.FieldType, true, true);
            }
        }

        private void AddMethods()
        {
            foreach (var method in sourceType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (method.IsPublic
                    // Make sure it's not a set get (Property), add and remove (Events) Method
                    && (!method.IsSpecialName
                    && (!method.Name.StartsWith("set_") && !method.Name.StartsWith("get_") && !method.Name.StartsWith("add_") && !method.Name.StartsWith("remove")))
                    && method.DeclaringType.FullName != "System.Object")
                {
                    CodeMemberMethod generatedMethod = new CodeMemberMethod();
                    generatedMethod.Name = method.Name;
                    generatedMethod.ReturnType = new CodeTypeReference(method.ReturnType);
                    generatedMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;

                    var parameters = method.GetParameters();
                    CodeExpression[] parameterReferences = new CodeExpression[parameters.Count()];
                    int i = 0;

                    foreach (var parameter in parameters)
                    {
                        var generatedParameter = new CodeParameterDeclarationExpression(new CodeTypeReference(parameter.ParameterType), parameter.Name);
                        generatedMethod.Parameters.Add(generatedParameter);
                        parameterReferences[i] = new CodeVariableReferenceExpression(parameter.Name);
                        i++;
                    }

                    CodeMethodInvokeExpression methodCall = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "objectInstance"), method.Name), parameterReferences);
                    generatedMethod.Statements.Add(methodCall);
                    targetClass.Members.Add(generatedMethod);

                    var canExecuteKey = method.DeclaringType.FullName + "." + method.Name;

                    if (Generator.GeneratorConfiguration.CanExecuteCollection.ContainsKey(canExecuteKey))
                    {
                        var function = Generator.GeneratorConfiguration.CanExecuteCollection[canExecuteKey];

                        if (!CanExecutedProperties.Any(p => p.Key == function))
                        {
                            CodeMemberProperty property = new CodeMemberProperty();
                            property.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                            property.Name = "CanExecute" + method.Name;
                            property.Type = new CodeTypeReference(typeof(bool));
                            property.HasGet = true;

                            var returnExpression = new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(Generator)), "GeneratorConfiguration.CanExecuteCollection[\"" + canExecuteKey + "\"]")));
                            property.GetStatements.Add(returnExpression);

                            targetClass.Members.Add(property);
                            CanExecutedProperties[function] = property.Name;
                            Generator.GeneratorConfiguration.HideMember(method.DeclaringType.FullName + "." + property.Name);
                        }
                    }
                }
            }
        }

        private void AddProperty(string propertyName, Type propertyType, bool canWrite, bool canRead)
        {
            CodeMemberProperty property = new CodeMemberProperty();
            property.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            property.Name = propertyName;

            if (Generator.IsSimple(propertyType))
            {
                property.Type = new CodeTypeReference(propertyType);

                if (canRead)
                {
                    property.HasGet = true;
                    property.GetStatements.Add(new CodeMethodReturnStatement(
                        new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(), "objectInstance." + propertyName)));
                }

                if (canWrite)
                {
                    property.HasSet = true;

                    var setStatement = new CodeAssignStatement(
                    new CodeFieldReferenceExpression(
                    new CodeThisReferenceExpression(), "objectInstance." + propertyName), new CodePropertySetValueReferenceExpression());

                    CodeExpression[] eventParams = new CodeExpression[2];
                    eventParams[0] = new CodeThisReferenceExpression();
                    eventParams[1] = new CodeObjectCreateExpression(new CodeTypeReference(typeof(PropertyChangedEventArgs)), new CodeExpression[] { new CodePrimitiveExpression(propertyName) });
                    var callEvent = new CodeDelegateInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "PropertyChanged"), eventParams);

                    var changeCondition = new CodeConditionStatement(new CodeSnippetExpression("this.objectInstance." + propertyName + " != value"));
                    changeCondition.TrueStatements.Add(setStatement);
                    changeCondition.TrueStatements.Add(callEvent);

                    property.SetStatements.Add(changeCondition);
                }

            }
            else
            {
                CodeTypeReference propertyWrapperType = null;

                if (propertyType.IsGenericType)
                {
                    var typeName = propertyType.FullName.Split('[')[0];
                    var fullTypeName = propertyType.FullName;
                    Type genericBaseType = Type.GetType(typeName);
                    CodeTypeReference[] genericTypes = new CodeTypeReference[propertyType.GenericTypeArguments.Length];

                    int i = 0;
                    var genericType = new CodeTypeReference(genericBaseType);

                    foreach (var item in propertyType.GenericTypeArguments)
                    {
                        if (Generator.IsSimple(item))
                        {
                            genericTypes[i] = new CodeTypeReference(item);
                        }
                        else
                        {
                            genericType.TypeArguments.Add(GetCodeTypeReference(item));

                            var wrapperType = GetCodeTypeReference(item);
                            genericTypes[i] = wrapperType;

                            //if (!neededAssemblies.Contains(wrapperType.Name))
                            //{
                            //    neededAssemblies.Add(wrapperType.Name + ".dll");
                            //}
                        }

                        i++;
                    }

                    propertyWrapperType = genericType;
                    
                }
                else
                {
                    propertyWrapperType = GetCodeTypeReference(propertyType);

                    //if (!neededAssemblies.Contains(propertyWrapperType.Name))
                    //{
                    //    neededAssemblies.Add(propertyWrapperType.Name + ".dll");
                    //}
                }

                //var declaration = new CodeTypeDeclaration(propertyWrapperType.ToString());
                property.Type = propertyWrapperType;

                if (canRead)
                {
                    property.HasGet = true;

                    var invocation = new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression(typeof(DOMGenerator)),
                    "ConvertToWrapperType", new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(), "objectInstance." + propertyName));

                    var castedStatement = new CodeCastExpression(propertyWrapperType, invocation);

                    var returnStatement = new CodeMethodReturnStatement(castedStatement);
                    property.GetStatements.Add(returnStatement);
                }
            }

            targetClass.Members.Add(property);
        }

        private void AddConstructors()
        {
            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes =
                MemberAttributes.Public | MemberAttributes.Final;

            constructor.Parameters.Add(new CodeParameterDeclarationExpression(sourceType, "instance"));

            CodeFieldReferenceExpression settingObjectInstance =
                new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(), "objectInstance");
            constructor.Statements.Add(new CodeAssignStatement(settingObjectInstance,
                new CodeArgumentReferenceExpression("instance")));

            targetClass.Members.Add(constructor);
        }
    }
}
