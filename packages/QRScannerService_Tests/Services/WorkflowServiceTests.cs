using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QRScannerService_Core.Models;
using QRScannerService_Core.Services;


namespace QRScannerService_Tests.Services
{
    [TestClass]
    public class WorkflowServiceTests
    {
        [TestMethod]
        public void TestAddAndRetrieveWorkflow()
        {
            var workflowService = new WorkflowService();
            var workflow = new WorkflowConfig
            {
                Prefix = "TST",
                ExcelFile = "test.xlsx"
            };

            workflowService.AddWorkflow(workflow);

            var retrievedWorkflow = workflowService.GetWorkflowForPrefix("TST");

            Assert.IsNotNull(retrievedWorkflow);
            Assert.AreEqual("TST", retrievedWorkflow.Prefix);
            Assert.AreEqual("test.xlsx", retrievedWorkflow.ExcelFile);
        }
    }
}