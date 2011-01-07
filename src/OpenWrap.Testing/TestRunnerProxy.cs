using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TestDriven.Framework;

namespace OpenWrap.Testing
{
    public class TestRunnerProxy : MarshalByRefObject
    {
        string _tempDirectory;

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public IEnumerable<KeyValuePair<string, bool?>> RunTests(string runnerAssemblyPath, string runnerTypeName, IEnumerable<string> assembliesToTest)
        {
            return RunTestsCore(runnerAssemblyPath, runnerTypeName, assembliesToTest).ToArray();
        }

        public void SetTempDirectory(string path)
        {
            _tempDirectory = path;
        }

        IEnumerable<KeyValuePair<string, bool?>> RunTestsCore(string runnerAssemblyPath, string runnerTypeName, IEnumerable<string> assembliesToTest)
        {
            var runnerAssembly = Assembly.LoadFrom(runnerAssemblyPath);
            var runnerType = runnerAssembly.GetType(runnerTypeName);

            var runner = Activator.CreateInstance(runnerType) as TestDriven.Framework.ITestRunner;
            if (runner == null)
                yield break;
            var testListener = new TestListener();

            foreach (var asm in assembliesToTest)
            {
                bool success = true;
                try
                {
                    var assemblyToTest = Assembly.LoadFrom(Path.Combine(_tempDirectory, Path.GetFileName(asm)));
                    runner.RunAssembly(testListener, assemblyToTest);
                }
                catch
                {
                    success = false;
                }
                if (!success)
                {
                    yield return new KeyValuePair<string, bool?>("An error occured while executing tests.", false);
                    continue;
                }
                foreach (var result in testListener.Results)
                    yield return new KeyValuePair<string, bool?>(ToMessage(result), ToSuccess(result));
            }
        }

        string ToMessage(TestListenerResult result)
        {
            return result.State != null 
                ? string.Format("\t- {0} ({1})", result.Message, result.State.ToString().ToLower())
                : result.Message;
        }

        bool? ToSuccess(TestListenerResult result)
        {
            if (result.State == TestState.Failed) return false;
            if (result.State == TestState.Ignored) return null;
            return true;
        }

        class TestListener : ITestListener
        {
            public TestListener()
            {
                Results = new List<TestListenerResult>();
            }

            public List<TestListenerResult> Results { get; set; }

            public void TestFinished(TestResult summary)
            {
                Results.Add(new TestListenerResult(summary.Name, summary.State));
            }

            public void TestResultsUrl(string resultsUrl)
            {
            }

            public void WriteLine(string text, Category category)
            {
            }
        }
        class TestListenerResult
        {
            public string Message { get; private set; }
            public TestState? State { get; private set; }

            public TestListenerResult(string message, TestState? state = null)
            {
                Message = message;
                State = state;
            }
        }
    }
}