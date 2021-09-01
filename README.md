# test-extractor
An application for extracting classes and methods from compiled test assemblies.

When provided an assembly's path as an argument, the application will attempt to extract class and/or method information.

This is useful for running test via a build pipeline where you can divvy up the test execution across multiple worker nodes.

If a class' full name exists in the `exceptionClasses` list, it's methods will be extracted. Otherwise the class will be extracted instead. Sub-classes will also attempted to be discovered and exported as well.

## Example Output

```
TestExtractorTests.classes.txt already exists. Deleting.
Outputting results to TestExtractorTests.classes.txt
    TestExtractor.Tests.ProgramTests+MoreProgramTests

TestExtractorTests.methods.txt already exists. Deleting.
Outputting results to TestExtractorTests.methods.txt
    TestExtractor.Tests.ProgramTests+CheckArgumentsTest
    TestExtractor.Tests.ProgramTests+LoadAssemblyTest
    TestExtractor.Tests.ProgramTests+OutputLoadExceptionsTest
    TestExtractor.Tests.ProgramTests+SearchCustomAttributesTest
```
