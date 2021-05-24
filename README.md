# Wordgame Selenium Tests

## Installing Chromedriver

These selenium tests use the chromedriver.  This needs to be installed on the machine running the tests.  It can be downloaded [here](https://chromedriver.chromium.org/downloads).
The tests are hardcoded to look for the chromedriver in the root of the C drive, at `C:\`  The chromedriver either needs to be placed there, or the test setup updated to point to the folder holding chromedriver.

## Running Tests

Tests can be run from Visual Studios Test Runner, or by using TestCentric GUI Test Runner.

### Running in Visual Studios
Open solution in Visual Studios and build the project.
Open Test Explorer and run all tests.
For more information see [Microsoft Docs](https://docs.microsoft.com/en-us/visualstudio/test/run-unit-tests-with-test-explore)

### Running with TestCentric test runner
1. Install TestCentric, instructions on installing can be found [here](https://github.com/TestCentric/testcentric-gui/blob/main/INSTALL.md)
2. Build the solution.  From the Wordgame.NUnit directory, in powershell run `dotnet build --configuration Release`
This should create the Wordgame DLL in `Wordgame.NUnit\bin\Release\net5.0\Wordgame.NUnit.dll`
3. Open the DLL in TestCentric and run all tests.
