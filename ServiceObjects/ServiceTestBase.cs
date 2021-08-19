using System.Configuration;
using NUnit.Framework;
using System;
using System.IO;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports;
using NUnit.Framework.Interfaces;

namespace ServiceObjects
{

    /// <summary>
    /// Base test class inherited by all test classes.
    /// rovides logging, and automatic browser launching/closing
    /// </summary>
    public abstract class ServiceTestBase
    {
        #region Test attributes

        private ExtentTest test => ExtentManager.GetTest();


        [OneTimeTearDown]
        public void TeardownAll()
        {
            //calling flush generates the report 
            ExtentManager.Flush();
           // DriverManager.CloseAllDrivers();
        }

      
        //Use TestInitialize to run code before running each test 
        [SetUp]
        public void Setup()
        {
            // creates a test 
            ExtentManager.CreateTest(this.GetType().Name + "." + NUnit.Framework.TestContext.CurrentContext.Test.Name, NUnit.Framework.TestContext.CurrentContext.Test.FullName);
        }

        //Use TestCleanup to run code after each test has run
        [TearDown]
        public void Teardown()
        {
            try
            {
                if (NUnit.Framework.TestContext.CurrentContext.Result.Outcome != ResultState.Success)
                {
                    test.Fail(NUnit.Framework.TestContext.CurrentContext.Result.StackTrace);
                }

            }
            catch (Exception e)
            {
                test.Fatal(e);
            }          
        }

        #endregion

    }
}
