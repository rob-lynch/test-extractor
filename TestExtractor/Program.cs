using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestExtractor
{
    public class Program
    {
        static void Main(string[] args)
        {
            CheckArguments(args);

            Assembly assembly = LoadAssembly(args[0]);
            string assemblyName = assembly.ManifestModule.Name.ToString().Replace(".dll","");

            List<string> exceptionClasses = new List<string>();
            List<string> classList = new List<string>();
            List<string> methodList = new List<string>();

            //Classes where we want to extract the methods
            exceptionClasses.Add("TestExtractor.Tests.ProgramTests");

            try
            {
                foreach (Type t in assembly.GetTypes())
                {
                    if(!t.IsPublic)
                    {
                        continue;
                    }

                    MemberInfo[] members = t.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod);

                    foreach (MemberInfo m in members)
                    {
                        IEnumerable<CustomAttributeData> customAttributes = m.CustomAttributes;
                        MemberInfo[] subMembers = m.DeclaringType.GetMembers();

                        if (customAttributes.Count() > 0)
                        {
                            CustomAttributeData hasTestMethod = SearchCustomAttributes(customAttributes, "TestMethodAttribute");

                            if (hasTestMethod == null)
                            {
                                foreach (MemberInfo s in subMembers)
                                {
                                    CustomAttributeData hasSubTestClass = SearchCustomAttributes(customAttributes, "TestClassAttribute");
                                    if (hasSubTestClass != null & s.GetType().ToString() == "System.RuntimeType")
                                    {
                                        AddClassToList($"{t.FullName}+{s.Name} ");
                                    }
                                }

                                continue;
                            }
                            else
                            {
                                if(exceptionClasses.Contains(t.FullName))
                                {
                                    methodList.Add($"{t.FullName}+{m.Name}");
                                }
                                else
                                {
                                    AddClassToList(t.FullName);
                                }
                            }
                        }
                    }
                }
            } 
            catch (ReflectionTypeLoadException e)
            {
                OutputLoadExceptions(e);
            }

            OutputResults("classes", classList);
            OutputResults("methods", methodList);

            void AddClassToList(string className)
            {
                if (!classList.Contains(className))
                {
                    classList.Add(className);
                }
            }

            void OutputResults(string listType, List<string> itemList)
            {
                string fileName = $"{assemblyName}.{listType}.txt";
                
                if (File.Exists(fileName))
                {
                    Console.WriteLine($"{fileName} already exists. Deleting.");
                    File.Delete(fileName);
                }

                Console.WriteLine($"Outputting results to {fileName}");
                using (StreamWriter file = new StreamWriter(fileName, true))
                {
                    foreach (string i in itemList)
                    {
                        Console.WriteLine($"    {i}");
                        file.WriteLine(i);
                    }
                }

                Console.WriteLine();
        }
        }

        public static void CheckArguments(string[] arguments)
        {
            if (arguments.Length == 1)
            {
                return;
            }

            ArgumentException argEx = new ArgumentException("Please provide a valid assembly path.");
            throw argEx;
        }

        public static Assembly LoadAssembly(string assemblyPath)
        {
            Assembly loadAssembly = null;
            try
            {
                loadAssembly = Assembly.LoadFrom(assemblyPath);
            }
            catch (ReflectionTypeLoadException e)
            {
                OutputLoadExceptions(e);
            }

            return loadAssembly;
        }

        public static void OutputLoadExceptions(ReflectionTypeLoadException e)
        {
            foreach (Exception exSub in e.LoaderExceptions)
            {
                FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                if (exFileNotFound != null)
                {
                    Console.WriteLine(exFileNotFound);
                }
                Console.WriteLine("Error loading assembly");
                Console.WriteLine(value: e.Message);
            }
        }

        public static CustomAttributeData SearchCustomAttributes(IEnumerable<CustomAttributeData> cAttrs, string searchString)
        {
            return cAttrs.FirstOrDefault(ca => ca.AttributeType.Name == searchString);
        }
    }
}
